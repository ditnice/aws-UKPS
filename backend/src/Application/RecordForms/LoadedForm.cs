using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Application.RecordForms;

/// <summary>A record's latest revision and its form, found and authorised for reading.</summary>
internal sealed record LoadedForm(RecordRevision Revision, Record Record, FormModel Model);
