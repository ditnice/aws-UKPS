using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "ASP.NET Core controller discovery requires public controller types.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Controllers"
)]

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "DTOs are public HTTP API contracts exposed by controller actions.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.DTOs"
)]

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "Enums are public HTTP API contract values used by DTOs and JSON serialization.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Enums"
)]

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "Interfaces injected into public controllers must be public constructor parameter types.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Services.Interfaces"
)]
