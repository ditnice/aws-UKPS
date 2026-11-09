using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.RecordForms.Bindings;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.Fixtures;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Application.RecordForms.Bindings;

/// <summary>
/// The regulatory date binding against a real database, on <see cref="RecordMhraDate"/>'s UK
/// licence date.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class RegulatoryDateBindingTests : DatabaseTestBase
{
    private readonly RegulatoryDateBinding _binding = new(
        typeof(RecordMhraDate).GetProperty(nameof(RecordMhraDate.UkLicenceDateId))!,
        typeof(RecordMhraDate).GetProperty(nameof(RecordMhraDate.UkLicenceDate))!,
        typeof(RecordMhraDate).GetProperty(nameof(RecordMhraDate.RevisionId))!,
        DateEventType.UkLicence
    );

    private RecordRevision _revision = null!;

    public RegulatoryDateBindingTests(PostgresFixture fixture)
        : base(fixture) { }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        Organisation organisation = await AddEntity(new OrganisationFaker().Generate(), Ct);
        User user = await AddEntity(new UserFaker().Generate(), Ct);
        Record record = await AddEntity(
            new RecordFaker()
                .RuleFor(x => x.OrganisationId, organisation.Id)
                .RuleFor(x => x.RecordType, RecordType.Medicine)
                .RuleFor(x => x.RecordStatus, RecordStatus.Unpublished)
                .Generate(),
            Ct
        );
        _revision = await AddEntity(
            new RecordRevisionFaker()
                .RuleFor(x => x.RecordId, record.Id)
                .RuleFor(x => x.CreatedBy, user.Id)
                .RuleFor(x => x.WorkflowStatus, WorkflowStatus.Draft)
                .Generate(),
            Ct
        );
        await AddEntity(new RecordMhraDate { RevisionId = _revision.Id }, Ct);
    }

    [Fact]
    public async Task Write_FirstAnswer_CreatesTheRegulatoryDate()
    {
        await Write("2027-Q2");

        RegulatoryDate date = await Context.RegulatoryDates.AsNoTracking().SingleAsync(Ct);
        (
            date.RevisionId,
            date.DateEvent,
            date.DatePrecision,
            date.DateValue,
            date.IsConfidential
        ).ShouldBe(
            (
                _revision.Id,
                DateEventType.UkLicence,
                DatePrecision.EstimatedQuarter,
                new DateOnly(2027, 4, 1),
                true
            )
        );
        (await Row(track: false)).UkLicenceDateId.ShouldBe(date.Id);
        _binding.Read(await Row(track: false)).Value.ShouldBe("2027-Q2");
    }

    [Fact]
    public async Task Write_ChangedAnswer_UpdatesTheSameRow()
    {
        await Write("2027-Q2");
        int id = (await Context.RegulatoryDates.AsNoTracking().SingleAsync(Ct)).Id;

        await Write("2027-05-14");

        RegulatoryDate date = await Context.RegulatoryDates.AsNoTracking().SingleAsync(Ct);
        (date.Id, date.DatePrecision, date.DateValue, date.IsConfidential).ShouldBe(
            (id, DatePrecision.ActualDate, new DateOnly(2027, 5, 14), false)
        );
    }

    [Fact]
    public async Task Write_ClearedAnswer_DeletesTheRegulatoryDate()
    {
        await Write("2027-05");

        await Write(null);

        (await Context.RegulatoryDates.AnyAsync(Ct)).ShouldBeFalse();
        RecordMhraDate row = await Row(track: false);
        row.UkLicenceDateId.ShouldBeNull();
        _binding.Read(row).IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public async Task HasAnswerExpression_FollowsTheForeignKey()
    {
        ParameterExpression parameter = Expression.Parameter(typeof(RecordMhraDate), "row");
        Expression<Func<RecordMhraDate, bool>> hasAnswer = Expression.Lambda<
            Func<RecordMhraDate, bool>
        >(_binding.HasAnswerExpression(parameter), parameter);

        (await Context.RecordMhraDates.AnyAsync(hasAnswer, Ct)).ShouldBeFalse();
        await Write("2027-05");
        (await Context.RecordMhraDates.AnyAsync(hasAnswer, Ct)).ShouldBeTrue();
    }

    [Fact]
    public void AuditText_IsTheRegulatoryDateAsJson()
    {
        _binding
            .AuditText(FormAnswer.Single("2027-Q2"))
            .ShouldBe(
                """{"date_precision":"EstimatedQuarter","date_value":"2027-04-01","is_confidential":true}"""
            );
        _binding
            .AuditText(FormAnswer.Single("2027-05-14"))
            .ShouldBe(
                """{"date_precision":"ActualDate","date_value":"2027-05-14","is_confidential":false}"""
            );
        _binding.AuditText(FormAnswer.Single(null)).ShouldBeNull();
    }

    private async Task Write(string? answer)
    {
        Context.ChangeTracker.Clear();
        _binding.Write(Context, await Row(track: true), FormAnswer.Single(answer));
        await Context.SaveChangesAsync(Ct);
        Context.ChangeTracker.Clear();
    }

    private async Task<RecordMhraDate> Row(bool track)
    {
        IQueryable<RecordMhraDate> rows = track
            ? Context.RecordMhraDates
            : Context.RecordMhraDates.AsNoTracking();
        return await rows.Include(static x => x.UkLicenceDate)
            .SingleAsync(x => x.RevisionId == _revision.Id, Ct);
    }
}
