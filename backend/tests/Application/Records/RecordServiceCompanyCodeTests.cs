using Shouldly;
using UKPS.Api.Application.Records;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using CompanyCodesResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Records.Dtos.CompanyCodeSuggestionsDto,
    UKPS.Api.Application.Records.Errors.GetRecordsError
>;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Application.Records;

[Collection(DatabaseCollection.Name)]
public class RecordServiceCompanyCodeTests(PostgresFixture fixture) : DatabaseTestBase(fixture)
{
    private IServiceTestHarness<IRecordService> _harness = null!;
    private IRecordService Service => _harness.Service;
    private int _organisationId;
    private int _otherOrganisationId;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        var organisations = (
            await AddEntities(
                new OrganisationFaker().Generate(2),
                TestContext.Current.CancellationToken
            )
        ).ToArray();
        _organisationId = organisations[0].Id;
        _otherOrganisationId = organisations[1].Id;

        _harness = new ServiceTestHarness<IRecordService>(Context).UpdateCurrentUser(x =>
            x with
            {
                UserRole = UserRole.Standard,
                OrganisationId = _organisationId,
            }
        );
    }

    [Fact]
    public async Task GetOrganisationCompanyCodes_ReturnsCodesContainingSearch_IgnoringCase()
    {
        await AddRecords(_organisationId, "ABC-123", "xabc-9", "DEF-456");

        CompanyCodeSuggestionsDto dto = (await Search("abc")).ShouldBeSuccess();

        dto.Items.ShouldBe(["ABC-123", "xabc-9"]);
    }

    [Fact]
    public async Task GetOrganisationCompanyCodes_ListsCodesStartingWithSearchFirst_ThenAlphabetically()
    {
        await AddRecords(_organisationId, "ZZ-AB", "AB-2", "c-ab", "ab-1");

        CompanyCodeSuggestionsDto dto = (await Search("ab")).ShouldBeSuccess();

        dto.Items.ShouldBe(["ab-1", "AB-2", "c-ab", "ZZ-AB"]);
    }

    [Fact]
    public async Task GetOrganisationCompanyCodes_RemovesDuplicates_IgnoringCaseAndSpaces()
    {
        await AddRecords(_organisationId, "ABC-123", "abc-123", " ABC-123 ");

        CompanyCodeSuggestionsDto dto = (await Search("abc")).ShouldBeSuccess();

        dto.Items.ShouldHaveSingleItem().ToUpperInvariant().ShouldBe("ABC-123");
    }

    [Fact]
    public async Task GetOrganisationCompanyCodes_ExcludesOtherOrganisationsAndDiscardedRecords()
    {
        await AddRecords(_organisationId, "ABC-1");
        await AddRecords(_otherOrganisationId, "ABC-2");
        await AddRecords(_organisationId, RecordStatus.Discarded, "ABC-3");

        CompanyCodeSuggestionsDto dto = (await Search("abc")).ShouldBeSuccess();

        dto.Items.ShouldBe(["ABC-1"]);
    }

    [Fact]
    public async Task GetOrganisationCompanyCodes_UsesLatestRevisionCode()
    {
        Record record = await AddRecord(_organisationId, RecordStatus.Unpublished, "ABC-OLD");
        await AddRevision(record, "ABC-NEW", new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        CompanyCodeSuggestionsDto dto = (await Search("abc")).ShouldBeSuccess();

        dto.Items.ShouldBe(["ABC-NEW"]);
    }

    [Theory]
    [InlineData("%")]
    [InlineData("_")]
    [InlineData("\\")]
    public async Task GetOrganisationCompanyCodes_TreatsLikeWildcardsAsText(string search)
    {
        await AddRecords(_organisationId, "ABC-123", $"AB{search}C");

        CompanyCodeSuggestionsDto dto = (await Search(search)).ShouldBeSuccess();

        dto.Items.ShouldBe([$"AB{search}C"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetOrganisationCompanyCodes_ReturnsNothing_WhenSearchIsBlank(string? search)
    {
        await AddRecords(_organisationId, "ABC-123");

        CompanyCodeSuggestionsDto dto = (await Search(search)).ShouldBeSuccess();

        dto.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetOrganisationCompanyCodes_ReturnsAtMostLimitCodes()
    {
        await AddRecords(_organisationId, "A-1", "A-2", "A-3", "A-4");

        CompanyCodeSuggestionsDto dto = (await Search("a-", limit: 2)).ShouldBeSuccess();

        dto.Items.ShouldBe(["A-1", "A-2"]);
    }

    [Fact]
    public async Task GetOrganisationCompanyCodes_ReturnsNotAllowed_ForAnotherOrganisation()
    {
        await AddRecords(_otherOrganisationId, "ABC-1");

        CompanyCodesResult result = await Service.GetOrganisationCompanyCodes(
            _otherOrganisationId,
            new GetCompanyCodeSuggestionsQueryDto { Search = "abc" },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError();
        result.Error.ShouldBeOfType<GetRecordsError.NotAllowed>();
    }

    [Fact]
    public async Task GetOrganisationCompanyCodes_ReturnsOrganisationNotFound_WhenOrganisationDoesNotExist()
    {
        const int missingOrganisationId = int.MaxValue;
        _harness.UpdateCurrentUser(x => x with { OrganisationId = missingOrganisationId });

        CompanyCodesResult result = await Service.GetOrganisationCompanyCodes(
            missingOrganisationId,
            new GetCompanyCodeSuggestionsQueryDto { Search = "abc" },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError();
        result.Error.ShouldBeOfType<GetRecordsError.OrganisationNotFound>();
    }

    private Task<CompanyCodesResult> Search(string? search, int limit = 10) =>
        Service.GetOrganisationCompanyCodes(
            _organisationId,
            new GetCompanyCodeSuggestionsQueryDto { Search = search, Limit = limit },
            TestContext.Current.CancellationToken
        );

    private Task AddRecords(int organisationId, params string[] companyCodes) =>
        AddRecords(organisationId, RecordStatus.Unpublished, companyCodes);

    private async Task AddRecords(
        int organisationId,
        RecordStatus status,
        params string[] companyCodes
    )
    {
        foreach (string companyCode in companyCodes)
        {
            await AddRecord(organisationId, status, companyCode);
        }
    }

    private async Task<Record> AddRecord(
        int organisationId,
        RecordStatus status,
        string companyCode
    )
    {
        Record record = new RecordFaker()
            .RuleFor(x => x.OrganisationId, organisationId)
            .RuleFor(x => x.RecordType, RecordType.Medicine)
            .RuleFor(x => x.RecordStatus, status)
            .RuleFor(x => x.CreatedByUser, _ => new UserFaker().Generate())
            .Generate();

        await AddRevision(record, companyCode, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        return record;
    }

    private async Task AddRevision(Record record, string companyCode, DateTime createdAt)
    {
        // A saved record is linked by ID, so it is not inserted again.
        RecordRevision revision = new RecordRevisionFaker()
            .RuleFor(x => x.Record, _ => record.Id == 0 ? record : null)
            .RuleFor(x => x.RecordId, _ => record.Id)
            .RuleFor(x => x.WorkflowStatus, WorkflowStatus.Draft)
            .RuleFor(x => x.CreatedAt, createdAt)
            .RuleFor(x => x.CreatedByUser, _ => new UserFaker().Generate())
            .Generate();

        RecordProductDetail detail = new RecordProductDetailFaker(RecordType.Medicine)
            .RuleFor(x => x.Revision, revision)
            .RuleFor(x => x.CompanyCode, companyCode)
            .Generate();

        await AddEntities([detail], TestContext.Current.CancellationToken);
        Context.ChangeTracker.Clear();
    }
}
