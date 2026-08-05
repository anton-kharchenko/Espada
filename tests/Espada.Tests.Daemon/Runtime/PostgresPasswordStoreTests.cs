using Espada.Daemon.Runtime;

namespace Espada.Tests.Daemon.Runtime
{
    public sealed class PostgresPasswordStoreTests
    {
        [Fact]
        public void GetOrCreate_ShouldProtectAndReusePasswordFile()
        {
            string root = Directory.CreateDirectory(
                Path.Join(Path.GetTempPath(), $"espada-password-{Guid.NewGuid():N}")).FullName;
            try
            {
                LocalRuntimePaths paths = new(root);
                paths.EnsureCreated();
                PostgresPasswordStore store = new(paths);

                string created = store.GetOrCreate();
                string reused = store.GetOrCreate();

                Assert.Equal(created, reused);
                Assert.Equal(created, File.ReadAllText(paths.PasswordFile));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }
    }
}