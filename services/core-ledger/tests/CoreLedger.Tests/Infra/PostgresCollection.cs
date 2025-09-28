namespace CoreLedger.Tests.Infra;

[CollectionDefinition("pg")]
public sealed class PostgresCollection : ICollectionFixture<TestPostgresFixture>;