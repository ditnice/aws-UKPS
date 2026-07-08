using System.Diagnostics.CodeAnalysis;

namespace UKPS.Api.Tests.Fixtures;

[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "Name mirrors the xUnit [CollectionDefinition]/ICollectionFixture convention, not System.Collections."
)]
[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Database";
}
