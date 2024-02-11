using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace PortAbuse2.Common;

internal static class Admin
{
    internal static void CheckAdmin()
    {
        if (!Core.Common.Admin.IsAdministrator())
        {
            RestartAsAdmin();
        }
    }

    internal static void RestartAsAdmin()
    {
        // It is not possible to launch a ClickOnce app as administrator directly,
        // so instead we launch the app as administrator in a new process.
        var processInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule!.FileName!)
            { UseShellExecute = true };

        // The following properties run the new process as administrator
        var args = string.Empty;
        var currentArgs = Environment.GetCommandLineArgs();
        args = currentArgs.Where((t, i) => i > 0).Aggregate(args, (current, t) => current + t + " ");
        processInfo.Arguments = args;
        processInfo.Verb = "runas";

        // Start the new process
        try
        {
            Process.Start(processInfo);
        }
        catch (Exception)
        {
            // The user did not allow the application to run as administrator
            MessageBox.Show("Restart as admin!");
        }

        // Shut down the current process
        Environment.Exit(0);
    }
}