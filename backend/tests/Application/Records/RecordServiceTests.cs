using Bogus;
using Shouldly;
using UKPS.Api.Application.Records;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.VaccinesRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Application.Common;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using GetRecordsResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Common.PaginatedResponseDto<UKPS.Api.Application.Records.Dtos.RecordListItemDto>,
    UKPS.Api.Application.Records.Errors.GetRecordsError
>;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Application.Records;

[Collection(DatabaseCollection.Name)]
public class RecordServiceTests : DatabaseTestBase
{
    private readonly RecordFaker _recordFaker = new();
    private IServiceTestHarness<IRecordService> _harness = null!;
    private IRecordService Service => _harness.Service;
    private readonly DateTime _currentDateTime = new(2003, 4, 12, 12, 12, 44, DateTimeKind.Utc);
    private IReadOnlyCollection<Record> _seededMedicineRecords = null!;
    private Record[] _seededVaccineRecords = null!;
    private IReadOnlyCollection<MedicinesProductDetail> _medicineProductDetailsData = null!;
    private IReadOnlyCollection<VaccinesProductDetail> _vaccineProductDetailsData = null!;
    private int _organisationId;
    private List<Record> OrganisationRecords =>
        _seededMedicineRecords
            .Concat(_seededVaccineRecords)
            .Where(x => x.OrganisationId == _organisationId)
            .ToList();

    public RecordServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        Randomizer.Seed = new Random(342);
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        var organisations = await AddEntities(
            new OrganisationFaker().Generate(5),
            TestContext.Current.CancellationToken
        );

        var userFaker = new UserFaker();
        var records = _recordFaker
            .RuleFor(x => x.OrganisationId, f => f.PickRandom(organisations).Id)
            .RuleFor(x => x.ReviewedAt, f => f.Date.Past(2, _currentDateTime))
            .RuleFor(x => x.CreatedByUser, _ => userFaker.Generate())
            .Generate(30);

        var faker = new Faker();
        var revisionFaker = new RecordRevisionFaker().RuleFor(
            y => y.CreatedByUser,
            _ => userFaker.Generate()
        );

        var medicalRecordRevisionFaker = revisionFaker.RuleFor(
            x => x.Record,
            _ => faker.PickRandom(records.Where(x => x.RecordType == RecordType.Medicine))
        );
        var medicineProductDetailsFaker = new MedicinesProductDetailFaker().RuleFor(
            x => x.Revision,
            _ => medicalRecordRevisionFaker.Generate()

        );
        _medicineProductDetailsData = medicineProductDetailsFaker.Generate(50);

        var vaccineRecordRevisionFaker = revisionFaker.RuleFor(
            x => x.Record,
            _ => faker.PickRandom(records.Where(x => x.RecordType == RecordType.Vaccine))
        );
        var vaccineProductDetailsFaker = new VaccinesProductDetailFaker().RuleFor(
            x => x.Revision,
            _ => vaccineRecordRevisionFaker.Generate()
        );
        _vaccineProductDetailsData = vaccineProductDetailsFaker.Generate(50);

        _organisationId = records
            .GroupBy(x => x.OrganisationId)
            .OrderByDescending(x => x.Count())
            .First()
            .Key;

        await AddEntities(_vaccineProductDetailsData, TestContext.Current.CancellationToken);
        await AddEntities(_medicineProductDetailsData, TestContext.Current.CancellationToken);

        _seededMedicineRecords = _medicineProductDetailsData
            .Select(x => x.Revision!.Record!)
            .DistinctBy(x => x.Id)
            .ToArray();
        _seededVaccineRecords = _vaccineProductDetailsData
            .Select(x => x.Revision!.Record!)
            .DistinctBy(x => x.Id)
            .ToArray();


        foreach(var record in _seededVaccineRecords.Concat(_seededMedicineRecords))
        {
            record.CurrentDraftRevisionId = record.Revisions.OrderBy(x => x.RevisionNo).Last().Id;
        }

        await Context.SaveChangesAsync();

        _harness = new ServiceTestHarness<IRecordService>(Context)
            .UpdateCurrentTime(_currentDateTime)
            .UpdateCurrentUser(x =>
                x with
                {
                    UserRole = UserRole.Champion,
                    OrganisationId = _organisationId,
                }
            );
    }

    [Fact]
    public async Task GetRecords_ReturnsSuccess_WhenRecordsExist()
    {
        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto { PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.ShouldNotBeEmpty();
        dto.TotalCount.ShouldBe(OrganisationRecords.Count);
    }

    [Fact]
    public async Task GetRecords_DevelopmentNameShouldBeSet()
    {
        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto(),
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.Select(x => x.DevelopmentName).ShouldAllBe(x => !string.IsNullOrEmpty(x));
    }

    [Fact]
    public async Task GetRecords_ReturnsEmptyPage_WhenNoRecordsMatch()
    {
        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto { Search = "no-match" },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetRecords_Paginates_WhenRecordsExist()
    {
        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto { Page = 2, PageSize = 1 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.TotalCount.ShouldBe(OrganisationRecords.Count);
        dto.Page.ShouldBe(2);
        dto.PageSize.ShouldBe(1);
        dto.Items.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetRecords_PageBeyondLastPage_ReturnsEmptyItemsWithCorrectTotalCount()
    {
        const int pageSize = 5;
        int pageBeyondLast = (int)Math.Ceiling((double)OrganisationRecords.Count / pageSize) + 1;

        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto { Page = pageBeyondLast, PageSize = pageSize },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(OrganisationRecords.Count);
        dto.Page.ShouldBe(pageBeyondLast);
    }

    [Fact]
    public async Task GetRecords_FiltersByRecordType_WhenTypesProvided()
    {
        RecordType[] types = [OrganisationRecords[0].RecordType];

        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto { RecordType = types, PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.ShouldNotBeEmpty();
        dto.Items.ShouldAllBe(x => types.Contains(x.RecordType));
    }

    [Fact]
    public async Task GetRecords_FiltersByRecordStatus_WhenStatusesProvided()
    {
        RecordStatus[] statuses = [OrganisationRecords[0].RecordStatus];

        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto { RecordStatus = statuses, PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.ShouldNotBeEmpty();
        dto.Items.ShouldAllBe(x => statuses.Contains(x.RecordStatus));
    }

    [Fact]
    public async Task GetRecords_FiltersByMultipleRecordTypes_WhenTypesProvided()
    {
        RecordType[] types = Enum.GetValues<RecordType>().ToArray();

        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto { RecordType = types, PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.ShouldAllBe(x => types.Contains(x.RecordType));
    }

    [Fact]
    public async Task GetRecords_FiltersByMultipleRecordStatuses_WhenStatusesProvided()
    {
        RecordStatus[] statuses = Enum.GetValues<RecordStatus>().ToArray();

        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto { RecordStatus = statuses, PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.ShouldNotBeEmpty();
        dto.Items.ShouldAllBe(x => statuses.Contains(x.RecordStatus));
    }

    [Fact]
    public async Task GetRecords_SearchDoesNotMatchUnrelatedRecords()
    {
        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto { Search = "fake-record", PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetRecords_WhenSortParametersNotSet_ShouldSortByNextUpdateDueAscending()
    {
        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto { PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.ShouldNotBeEmpty();
        dto.Items.Select(x =>
                x.ReviewedAt.HasValue ? x.ReviewedAt.Value.AddMonths(3) : (DateTime?)null
            )
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ShouldBeInOrder(SortDirection.Ascending);
    }

    [Fact]
    public async Task GetRecords_MapsRecordFields_WhenRecordsExist()
    {
        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto { PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        foreach (Record record in OrganisationRecords)
        {
            RecordListItemDto item = dto.Items.Single(x => x.Id == record.Id);
            string developmentName = GetExpectedDevelopmentName(record);

            item.Id.ShouldBe(record.Id);
            item.RecordType.ShouldBe(record.RecordType);
            item.RecordStatus.ShouldBe(record.RecordStatus);
            item.DevelopmentName.ShouldBe(developmentName);

            if (record.ReviewedAt.HasValue)
            {
                item.ReviewedAt.ShouldNotBeNull();
                item.ReviewedAt.Value.ShouldBe(
                    record.ReviewedAt.Value,
                    TimeSpan.FromMicroseconds(1)
                );
            }
            else
            {
                item.ReviewedAt.ShouldBeNull();
            }
        }
    }

    private string GetExpectedDevelopmentName(Record record)
    {
        var relevantRevision = record.Revisions.OrderBy(x => x.RevisionNo).Last();
        if (record.RecordType == RecordType.Medicine)
        {
            var medicalData = _medicineProductDetailsData.First(x =>
                x.RevisionId == relevantRevision.Id
            );
            return medicalData
                .ActiveSubstances.Where(x => x.NameType == SubstanceNameType.DevelopmentName)
                .OrderBy(x => x.DisplayOrder)
                .First()
                .Name;
        }

        var vaccinesProductDetail = _vaccineProductDetailsData.First(x =>
            x.RevisionId == relevantRevision.Id
        );
        return vaccinesProductDetail.CompanyCode;
    }

    [Fact]
    public async Task GetRecords_ReturnsAllRecords_WhenFiltersAreNotProvided()
    {
        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto { PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.TotalCount.ShouldBe(OrganisationRecords.Count);
        dto.Items.Select(x => x.Id).ShouldContainSet(OrganisationRecords.Select(x => x.Id));
    }

    [Fact]
    public async Task GetRecords_ReturnsOnlyRecordsForRequestedOrganisation()
    {
        GetRecordsResult result = await Service.GetOrganisationRecords(
            _organisationId,
            new GetRecordsQueryDto { PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();
        dto.Items.Select(x => x.Id).ShouldContainSet(OrganisationRecords.Select(x => x.Id));
        dto.Items.Count.ShouldBe(OrganisationRecords.Count);
    }
}
