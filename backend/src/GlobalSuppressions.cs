using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "ASP.NET Core controller discovery requires public controller types.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.WebApi.Controllers"
)]

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "DTOs are public HTTP API contracts exposed by controller actions.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Application"
)]

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "Enums are public HTTP API contract values used by DTOs and JSON serialization.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Persistence.Enums"
)]

[assembly: SuppressMessage(
    "Design",
    "CA1034:Nested types should not be visible",
    Justification = "Nested variant records model closed error unions narrowed via pattern matching.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Application"
)]
[assembly: SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "<Pending>",
    Scope = "member",
    Target = "~P:UKPS.Api.Application.Common.IResult`1.Error"
)]
