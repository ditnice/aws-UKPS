namespace UKPS.Api.Enums;

internal enum IamEventType
{
    FieldUpdated,
    StatusChanged,
    RoleChanged,
    Disabled,
    Enabled,
    Created,
    InformationRequested,
    TermsRequested,
    TermsSigned,
    TermsExpired
}
