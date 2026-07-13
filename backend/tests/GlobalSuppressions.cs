using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "xUnit test classes are conventionally public and derive from / are constructed with these fixture types, so the fixtures must be public too even though the test assembly is never referenced externally.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Tests.Fixtures"
)]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "This prevents test classes with no members meaning we can't have test classes that inherit test suites from parent classes.",
    Scope = "type",
    Target = "~T:UKPS.Api.Tests.Services.Organisations.OrganisationServiceWebApiTests"
)]
