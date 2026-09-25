global using CreateRecordResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Records.Dtos.CreateRecordDto,
    UKPS.Api.Application.Records.Errors.CreateRecordError
>;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Records;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;

namespace UKPS.Api.Tests.Application.Records;

[Collection(DatabaseCollection.Name)]
public class RecordCreationServiceTests : DatabaseTestBase
{
    private readonly ServiceTestHarness<IRecordCreationService> _harness;
    private readonly User _currentUser;
    private readonly DateTime _currentDateTime;
    private readonly Organisation _organisation;
    private CreateRecordCommand _validCommand = null!;
    private CreateRecordDto _validResponse = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public RecordCreationServiceTests(PostgresFixture postgresFixture)
        : base(postgresFixture)
    {
        Randomizer.Seed = new Random(0);

        _harness = new ServiceTestHarness<IRecordCreationService>(Context);
        _organisation = new OrganisationFaker()
            .RuleFor(x => x.Status, UserOrgStatus.Active)
            .Generate();
        _currentUser = new UserFaker().Generate();
        _currentDateTime = new DateTime(2022, 11, 01, 12, 56, 03, DateTimeKind.Utc);
        _harness
            .UpdateCurrentUser(x => x with { CognitoUsername = _currentUser.CognitoUsername })
            .UpdateCurrentTime(_currentDateTime);
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        var context = _harness.GetClearedContext();
        context.AddRange(_currentUser, _organisation);
        await context.SaveChangesAsync(Ct);

        _validCommand = new CreateRecordCommandFaker().Generate() with
        {
            OrganisationId = _organisation.Id,
        };

        CreateRecordResult result = await _harness.Service.CreateRecord(_validCommand, Ct);
        _validResponse = result.ShouldBeSuccess();
    }

    [Fact]
    public async Task CreateRecord_OnValidCommand_ShouldCreateANewRecord()
    {
        var context = _harness.GetClearedContext();
        var record = await context.Records.FirstOrDefaultAsync(
            x => x.Id == _validResponse.RecordId,
            Ct
        );

        record.ShouldNotBeNull();
        record.Id.ShouldBe(_validResponse.RecordId);
        record.CreatedBy.ShouldBe(_currentUser.Id);
        record.CreatedAt.ShouldBe(_currentDateTime);
        record.OrganisationId.ShouldBe(_organisation.Id);
        record.RecordStatus.ShouldBe(RecordStatus.Unpublished);
    }

    [Fact]
    public async Task CreateRecord_OnValidCommand_ShouldCreateRecordHistoryEntry()
    {
        var context = _harness.GetClearedContext();
        var historyRecords = await context
            .RecordStatusHistories.Where(x => x.RecordId == _validResponse.RecordId)
            .ToArrayAsync(Ct);
        var historyRecord = historyRecords.ShouldHaveSingleItem();

        historyRecord.Id.ShouldBe(_validResponse.RecordId);
        historyRecord.UpdatedBy.ShouldBe(_currentUser.Id);
        historyRecord.UpdatedAt.ShouldBe(_currentDateTime);
        historyRecord.FromStatus.ShouldBeNull();
        historyRecord.ToStatus.ShouldBe(RecordStatus.Unpublished);
    }

    [Fact]
    public async Task CreateRecord_OnValidCommand_ShouldCreateANewRecordRevision()
    {
        var context = _harness.GetClearedContext();
        var revision = await context.RecordRevisions.FirstOrDefaultAsync(
            x => x.Id == _validResponse.RevisionId,
            Ct
        );

        revision.ShouldNotBeNull();
        revision.Id.ShouldBe(_validResponse.RecordId);
        revision.CreatedBy.ShouldBe(_currentUser.Id);
        revision.CreatedAt.ShouldBe(_currentDateTime);
        revision.RecordId.ShouldBe(_validResponse.RecordId);
        revision.WorkflowStatus.ShouldBe(WorkflowStatus.Draft);
    }

    [Fact]
    public async Task CreateRecord_OnValidCommand_ShouldCreateANewRecordEvent()
    {
        var context = _harness.GetClearedContext();
        var recordEvent = await context.RecordEvents.FirstOrDefaultAsync(
            x => x.RevisionId == _validResponse.RevisionId,
            Ct
        );

        recordEvent.ShouldNotBeNull();
        recordEvent.Id.ShouldBe(_validResponse.RecordId);
        recordEvent.PerformedBy.ShouldBe(_currentUser.Id);
        recordEvent.PerformedAt.ShouldBe(_currentDateTime);
        recordEvent.RecordId.ShouldBe(_validResponse.RecordId);
        recordEvent.EventType.ShouldBe(RecordEventType.RecordCreated);
    }

    [Fact]
    public async Task CreateRecord_OnValidCommand_ShouldCreateMedicineProductDetails()
    {
        var context = _harness.GetClearedContext();
        var medicinesProductDetail = await context
            .MedicinesProductDetails.Include(x => x.ActiveSubstances)
            .FirstOrDefaultAsync(x => x.RevisionId == _validResponse.RevisionId, Ct);

        medicinesProductDetail.ShouldNotBeNull();
        medicinesProductDetail.RecordTitle.ShouldBe(_validCommand.RecordTitle);
        medicinesProductDetail.BrandedName.ShouldBe(_validCommand.BrandedName);

        foreach (var genericName in _validCommand.GenericNames.Enumerate())
        {
            medicinesProductDetail.ActiveSubstances.ShouldContain(x =>
                x.Name == genericName.Value
                && x.DisplayOrder == genericName.Index
                && x.NameType == SubstanceNameType.GenericName
            );
        }

        foreach (var developmentName in _validCommand.DevelopmentNames.Enumerate())
        {
            medicinesProductDetail.ActiveSubstances.ShouldContain(x =>
                x.Name == developmentName.Value
                && x.DisplayOrder == developmentName.Index
                && x.NameType == SubstanceNameType.DevelopmentName
            );
        }
    }

    [Fact]
    public async Task CreateRecord_WhenSpecifiedOrganisationDoesNotExist_ShouldReturnInvalidOrganisationError()
    {
        var nonExistentOrganisationCommand = _validCommand with { OrganisationId = 999 };
        CreateRecordResult result = await _harness.Service.CreateRecord(
            nonExistentOrganisationCommand,
            Ct
        );
        result.ShouldBeError().ShouldBeOfType<CreateRecordError.OrganisationDoesNotExist>();
    }

    [Theory]
    [InlineData(UserRole.Champion)]
    [InlineData(UserRole.Standard)]
    public async Task CreateRecord_WhenTheUserIsWithinTheOrganisation_ShouldCreateNewRecord(
        UserRole userRole
    )
    {
        _harness.UpdateCurrentUser(x => x with { UserRole = userRole });
        CreateRecordResult result = await _harness.Service.CreateRecord(_validCommand, Ct);
        result.ShouldBeSuccess();
    }

    [Theory]
    [InlineData(UserRole.Champion)]
    [InlineData(UserRole.Standard)]
    public async Task CreateRecord_WhenTheUserIsOutsideTheOrganisation_ShouldReturnNotAuthorisedError(
        UserRole userRole
    )
    {
        _harness.UpdateCurrentUser(x => x with { UserRole = userRole, OrganisationId = 999 });
        CreateRecordResult result = await _harness.Service.CreateRecord(_validCommand, Ct);
        result.ShouldBeError().ShouldBeOfType<CreateRecordError.NotAuthorised>();
    }
}
