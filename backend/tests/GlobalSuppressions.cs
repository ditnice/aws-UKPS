using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "xUnit test classes are conventionally public and derive from / are constructed with these fixture types, so the fixtures must be public too even though the test assembly is never referenced externally.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Tests.Fixtures"
)]
[assembly: SuppressMessage(
    "Design",
    "MA0051:Method is too long",
    Justification = "Long method names for tests are completely fine.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Tests"
)]
