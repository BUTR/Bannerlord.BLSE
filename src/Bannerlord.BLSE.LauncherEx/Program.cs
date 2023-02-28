using System;
using System.IO;
using System.Linq;

namespace Bannerlord.BLSE.LauncherEx;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        args = new[] { "launcherex" }.Concat(args).ToArray();
        switch (new DirectoryInfo(Directory.GetCurrentDirectory()).Name)
        {
            case "Win64_Shipping_Client":
                NETFrameworkLoader.Launch(args);
                break;
            case "Gaming.Desktop.x64_Shipping_Client":
                NETCoreLoader.Launch(args);
                break;
        }
    }
}