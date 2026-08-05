using Espada.Daemon.Runtime;

namespace Espada.Tests.Daemon.Runtime
{
    public sealed class LocalRuntimeLockTests
    {
        [Fact]
        public void Acquire_WhenLockIsHeld_ShouldProtectSingleInstance()
        {
            string root = CreateTemporaryRoot();
            try
            {
                LocalRuntimePaths paths = new(root);
                paths.EnsureCreated();
                using LocalRuntimeLock runtimeLock = LocalRuntimeLock.Acquire(paths);

                Assert.Throws<LocalRuntimeAlreadyRunningException>(() => LocalRuntimeLock.Acquire(paths));
                Assert.Equal(Environment.ProcessId.ToString(), File.ReadAllText(paths.PidFile));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [Fact]
        public void Acquire_WithStalePidFile_ShouldReplaceAdvisoryPid()
        {
            string root = CreateTemporaryRoot();
            try
            {
                LocalRuntimePaths paths = new(root);
                paths.EnsureCreated();
                File.WriteAllText(paths.PidFile, "999999");

                using LocalRuntimeLock runtimeLock = LocalRuntimeLock.Acquire(paths);

                Assert.Equal(Environment.ProcessId.ToString(), File.ReadAllText(paths.PidFile));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static string CreateTemporaryRoot()
        {
            return Directory.CreateDirectory(Path.Join(Path.GetTempPath(), $"espada-daemon-{Guid.NewGuid():N}"))
                .FullName;
        }
    }
}
