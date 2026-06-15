using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "ASP.NET controllers must be public for endpoint discovery.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Controllers")]

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "DTOs are intentionally public API surface.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.DTOs")]

[assembly: SuppressMessage(
    "Maintainability",
    "CA1812:Internal class that is never instantiated",
    Justification = "EF Core instantiates these classes via reflection; they are never explicitly instantiated in code.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Configurations")]

[assembly: SuppressMessage(
    "Maintainability",
    "CA1812:Internal class that is never instantiated",
    Justification = "EF Core instantiates these classes via reflection; they are never explicitly instantiated in code.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Entities")]

[assembly: SuppressMessage(
    "Maintainability",
    "CA1812:Internal class that is never instantiated",
    Justification = "EF Core instantiates these classes via reflection; they are never explicitly instantiated in code.",
    Scope = "namespaceanddescendants",
    Target = "~N:UKPS.Api.Data")]
