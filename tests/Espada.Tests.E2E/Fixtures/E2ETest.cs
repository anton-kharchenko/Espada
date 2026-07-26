namespace Espada.Tests.E2E.Fixtures;

public abstract class E2ETest(EspadaE2EFactory factory) : IAsyncLifetime
{
    protected EspadaE2EFactory Factory { get; } = factory;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await Factory.ResetDatabaseAsync();
}