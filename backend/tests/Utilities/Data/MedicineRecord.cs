using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Utilities.Data;

internal sealed record MedicineRecord(
    Record Record,
    RecordRevision Revision,
    MedicinesProductDetail ProductDetail,
    Organisation Organisation,
    User User
);
