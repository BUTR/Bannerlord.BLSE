namespace Bannerlord.BLSE;

public static class Program
{
    public static void Main(string[] args)
    {
#if STANDALONE
        args = new[] { "standalone" }.Concat(args).ToArray();
#elif LAUNCHER
        args = new[] { "launcher" }.Concat(args).ToArray();
#elif LAUNCHEREX
        args = new[] { "launcherex" }.Concat(args).ToArray();
#endif
        switch (new DirectoryInfo(Directory.GetCurrentDirectory()).Name)
        {
            case "Win64_Shipping_Client":
                NETFrameworkLoader.Launch(args);
                break;
            case "Gaming.Desktop.x64_Shipping_Client":
                NETCoreLoader.Launch(args);
                break;
        }
        
        NETFrameworkLoader.Launch(args);
    }
}