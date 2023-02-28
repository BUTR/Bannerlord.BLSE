using System.IO;
using System.Linq;

namespace Bannerlord.BLSE.Launcher;

public static class Program
{
    public static void Main(string[] args)
    {
        args = new[] { "launcher" }.Concat(args).ToArray();
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