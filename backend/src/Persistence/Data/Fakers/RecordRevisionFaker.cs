using Bogus;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Data.Fakers;

internal sealed class RecordRevisionFaker : Faker<RecordRevision>
{
    public RecordRevisionFaker()
    {
        RuleFor(x => x.WorkflowStatus, f => f.PickRandom<WorkflowStatus>());
        RuleFor(x => x.CreatedAt, f => DateTime.SpecifyKind(f.Date.Past(1), DateTimeKind.Utc));
    }
}
