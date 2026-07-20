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
    Target = "~N:UKPS.Api.Enums"
)]

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "Interfaces injected into public controllers must be public constructor parameter types.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Services.Interfaces"
)]

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "Result is part of public service interface signatures.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Common"
)]

[assembly: SuppressMessage(
    "Design",
    "CA1034:Nested types should not be visible",
    Justification = "Nested variant records model closed error unions narrowed via pattern matching.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Application"
)]

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "Error unions are part of public service interface signatures.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Services.Errors"
)]
