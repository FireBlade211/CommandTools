using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CommandToolsUtilityLibrary
{
    public static partial class ConsoleTools
    {
        [LibraryImport("kernel32.dll")]
        private static partial nint GetConsoleWindow();

        [LibraryImport("user32.dll")]
        private static partial nint GetForegroundWindow();

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static nint GetCurrentConsoleHandle()
        {
            var fgHWnd = GetForegroundWindow();
            GetWindowThreadProcessId(fgHWnd, out uint pid);

            var fgProc = Process.GetProcessById((int)pid);

            if (fgProc.ProcessName.Equals("WindowsTerminal", StringComparison.OrdinalIgnoreCase))
            {
                return fgHWnd;
            }
            else
            {
                return GetConsoleWindow();
            }
        }
    }
}
