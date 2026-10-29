using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Application.RecordForms;

/// <summary>A page of a revision's form, found and authorised for reading.</summary>
internal sealed record LoadedPage(
    RecordRevision Revision,
    Record Record,
    FormModel Model,
    FormPage Page,
    bool CanEdit
);
