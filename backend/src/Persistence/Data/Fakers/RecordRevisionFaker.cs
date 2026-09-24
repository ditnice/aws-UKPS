using Bogus;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Data.Fakers;

internal sealed class RecordRevisionFaker : Faker<RecordRevision>
{
    private const int NumberOfIncrementsPerMajor = 5;

    public RecordRevisionFaker()
    {
        RuleFor(x => x.RevisionNo, f => f.IndexFaker + 1);
        RuleFor(x => x.MajorVersion, (_, o) => o.RevisionNo / NumberOfIncrementsPerMajor);
        RuleFor(x => x.MinorVersion, (_, o) => o.RevisionNo % NumberOfIncrementsPerMajor);
        RuleFor(x => x.WorkflowStatus, f => f.PickRandom<WorkflowStatus>());
        RuleFor(x => x.CreatedAt, f => DateTime.SpecifyKind(f.Date.Past(1), DateTimeKind.Utc));
    }
}
