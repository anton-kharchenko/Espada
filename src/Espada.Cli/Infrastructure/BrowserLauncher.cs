using System.Diagnostics;

namespace Espada.Cli.Infrastructure
{
    internal static class BrowserLauncher
    {
        public static bool TryOpen(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                return true;
            }
            catch (Exception exception) when (exception is InvalidOperationException
                                              or System.ComponentModel.Win32Exception)
            {
                return false;
            }
        }
    }
}
