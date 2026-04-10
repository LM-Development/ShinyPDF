using System;
using System.Diagnostics;

namespace ShinyPDF.Examples.Helpers
{
    internal static class FileOpeningHelper
    {
        internal static bool TryOpen(string path)
        {
            try
            {
                if (TryOpenInVsCode(path))
                    return true;

                // Use the platform shell when available (Windows/macOS and some Linux setups).
                var shellOpenSucceeded = TryStartShell(path);
                if (shellOpenSucceeded)
                    return true;

                if (OperatingSystem.IsLinux())
                    return TryStartProcess("xdg-open", path);

                return TryStartProcess("explorer", path);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryOpenInVsCode(string path)
        {
            if (!IsRunningInVsCode())
                return false;

            return TryStartProcess("code", QuoteArgument(path), "--reuse-window")
                || TryStartProcess("code-insiders", QuoteArgument(path), "--reuse-window");
        }

        private static bool IsRunningInVsCode()
        {
            var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
            if (string.Equals(termProgram, "vscode", StringComparison.OrdinalIgnoreCase))
                return true;

            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VSCODE_IPC_HOOK_CLI"));
        }

        private static string QuoteArgument(string argument)
        {
            if (argument.Contains(' '))
                return $"\"{argument}\"";

            return argument;
        }

        private static bool TryStartShell(string path)
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });

                return process is not null;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryStartProcess(string command, string argument)
        {
            return TryStartProcess(command, argument, null);
        }

        private static bool TryStartProcess(string command, string argument, string additionalArguments)
        {
            try
            {
                var arguments = string.IsNullOrWhiteSpace(additionalArguments)
                    ? argument
                    : $"{additionalArguments} {argument}";

                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false
                });

                return process is not null;
            }
            catch
            {
                return false;
            }
        }
    }
}
