namespace UKPS.Api.Enums;

[Flags]
public enum ReferenceDataType
{
    MedicinesOnly = 1,
    VaccinesOnly = 2,
    Shared = MedicinesOnly | VaccinesOnly,
}
