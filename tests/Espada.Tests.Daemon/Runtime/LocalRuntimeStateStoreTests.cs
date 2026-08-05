using Espada.Daemon.Runtime;

namespace Espada.Tests.Daemon.Runtime
{
    public sealed class LocalRuntimeStateStoreTests
    {
        [Fact]
        public void Write_ShouldPersistOnlyOperationalState()
        {
            string root = Directory.CreateDirectory(
                Path.Join(Path.GetTempPath(), $"espada-state-{Guid.NewGuid():N}")).FullName;
            try
            {
                LocalRuntimePaths paths = new(root);
                paths.EnsureCreated();
                LocalRuntimeStateStore store = new(paths);
                LocalRuntimeState expected = new(Environment.ProcessId, "healthy", DateTimeOffset.UtcNow,
                    7432, 7433, 5433, "espada-postgres", new Dictionary<string, int> { ["api"] = 123 });

                store.Write(expected);

                LocalRuntimeState actual = Assert.IsType<LocalRuntimeState>(store.Read());
                Assert.Equal(expected.ProcessId, actual.ProcessId);
                Assert.Equal(expected.Status, actual.Status);
                Assert.Equal(expected.ApiPort, actual.ApiPort);
                Assert.Equal(123, actual.ChildProcessIds["api"]);
                string json = File.ReadAllText(paths.StateFile);
                Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("connectionString", json, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }
    }
}