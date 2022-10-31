using Autofac;
using CommandLine;
using NAPS2.Automation;
using NAPS2.Modules;
using NAPS2.Remoting.Worker;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point for NAPS2.Console.exe, the NAPS2 CLI.
/// </summary>
public static class WindowsConsoleEntryPoint
{
    public static int Run(string[] args)
    {
        return ConsoleEntryPoint.Run(args, new GdiModule(), true);
    }
}