using System.Globalization;
using Bogus;
using Shouldly;
using UKPS.Api.Application.Records;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Persistence.Data.Fakers;
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
    private readonly Faker _faker = new();
    private readonly IServiceTestHarness<IRecordService> _harness;
    private IRecordService Service => _harness.Service;
    private readonly DateTime _currentDateTime = new(2003, 4, 12, 12, 12, 44, DateTimeKind.Utc);
    private List<Record> _seededRecords = [];

    public RecordServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        Randomizer.Seed = new Random(342);

        _harness = new ServiceTestHarness<IRecordService>(Context).UpdateCurrentTime(
            _currentDateTime
        );
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        var organisations = await AddEntities(
            new OrganisationFaker().Generate(5),
            TestContext.Current.CancellationToken
        );

        _seededRecords = _recordFaker
            .RuleFor(x => x.OrganisationId, f => f.PickRandom(organisations).Id)
            .RuleFor(x => x.ReviewedAt, f => f.Date.Past(2, _currentDateTime))
            .Generate(30);

        await AddEntities(_seededRecords, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetRecords_ReturnsSuccess_WhenRecordsExist()
    {
        GetRecordsResult result = await Service.GetRecords(
            new GetRecordsQueryDto { PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.ShouldNotBeEmpty();
        dto.TotalCount.ShouldBe(_seededRecords.Count);
    }

    [Fact]
    public async Task GetRecords_ReturnsEmptyPage_WhenNoRecordsMatch()
    {
        GetRecordsResult result = await Service.GetRecords(
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
        GetRecordsResult result = await Service.GetRecords(
            new GetRecordsQueryDto { Page = 2, PageSize = 5 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.TotalCount.ShouldBe(_seededRecords.Count);
        dto.Page.ShouldBe(2);
        dto.PageSize.ShouldBe(5);
        dto.Items.Count.ShouldBe(5);
    }

    [Fact]
    public async Task GetRecords_PageBeyondLastPage_ReturnsEmptyItemsWithCorrectTotalCount()
    {
        const int pageSize = 5;
        int pageBeyondLast = (int)Math.Ceiling((double)_seededRecords.Count / pageSize) + 1;

        GetRecordsResult result = await Service.GetRecords(
            new GetRecordsQueryDto { Page = pageBeyondLast, PageSize = pageSize },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(_seededRecords.Count);
        dto.Page.ShouldBe(pageBeyondLast);
    }

    [Fact]
    public async Task GetRecords_FiltersByRecordType_WhenTypesProvided()
    {
        RecordType[] types = [_seededRecords[0].RecordType];

        GetRecordsResult result = await Service.GetRecords(
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
        RecordStatus[] statuses = [_seededRecords[0].RecordStatus];

        GetRecordsResult result = await Service.GetRecords(
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

        GetRecordsResult result = await Service.GetRecords(
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

        GetRecordsResult result = await Service.GetRecords(
            new GetRecordsQueryDto { RecordStatus = statuses, PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.ShouldAllBe(x => statuses.Contains(x.RecordStatus));
    }

    [Fact]
    public async Task GetRecords_SearchesByRecordId_WhenSearchIsNumeric()
    {
        Record record = _faker.PickRandom(_seededRecords);

        GetRecordsResult result = await Service.GetRecords(
            new GetRecordsQueryDto
            {
                Search = record.Id.ToString(CultureInfo.InvariantCulture),
                PageSize = 1000,
            },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.ShouldContain(x => x.Id == record.Id);
    }

    [Fact]
    public async Task GetRecords_SearchDoesNotMatchUnrelatedRecords()
    {
        GetRecordsResult result = await Service.GetRecords(
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
        GetRecordsResult result = await Service.GetRecords(
            new GetRecordsQueryDto { PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.Items.Select(x =>
                x.ReviewedAt.HasValue ? x.ReviewedAt.Value.AddMonths(3) : (DateTime?)null
            )
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ShouldBeInOrder(Shouldly.SortDirection.Ascending);
    }

    [Fact]
    public async Task GetRecords_MapsRecordFields_WhenRecordsExist()
    {
        GetRecordsResult result = await Service.GetRecords(
            new GetRecordsQueryDto { PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        foreach (Record record in _seededRecords)
        {
            RecordListItemDto item = dto.Items.Single(x => x.Id == record.Id);

            item.Id.ShouldBe(record.Id);
            item.RecordType.ShouldBe(record.RecordType);
            item.RecordStatus.ShouldBe(record.RecordStatus);
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

    [Fact]
    public async Task GetRecords_ReturnsAllRecords_WhenFiltersAreNotProvided()
    {
        GetRecordsResult result = await Service.GetRecords(
            new GetRecordsQueryDto { PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();

        dto.TotalCount.ShouldBe(_seededRecords.Count);
        dto.Items.Select(x => x.Id).ShouldContainSet(_seededRecords.Select(x => x.Id));
    }
}
