namespace Bannerlord.BLSE;

public static class NETFrameworkLoader
{
#if LAUNCHEREX
    // We need to have System.Windows.Forms loaded for LauncherEx, since we use reflection to access it    
    private static System.Type _ = typeof(System.Windows.Forms.MessageBox);
#endif

    public static void Launch(string[] args)
    {
        Shared.Program.Main(args);
    }
}