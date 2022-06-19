using NAPS2.Scan;
using Ninject;
using Ninject.Modules;

namespace NAPS2.Modules;

public class PathsModule : NinjectModule
{
    public override void Load()
    {
        Kernel.Get<ScanningContext>().TempFolderPath = Paths.Temp;
    }
}