namespace NAPS2.Tools.Project;

public static class PackageCommand
{
    public static int Run(PackageOptions opts)
    {
        if (opts.What == "exe" || opts.What == "all")
        {
            PackageExe();
        }
        if (opts.What == "msi" || opts.What == "all")
        {
            // PackageMsi();
        }
        if (opts.What == "zip" || opts.What == "zip")
        {
            // PackageZip();
        }
        if (opts.What == "7z" || opts.What == "7z")
        {
            // Package7z();
        }
        return 0;
    }

    private static void PackageExe()
    {
        //var iscc = Environment.ExpandEnvironmentVariables("%PROGRAMFILES(X86)%/Inno Setup 6/iscc.exe");
        //Cli.Run(iscc, "");
    }
}