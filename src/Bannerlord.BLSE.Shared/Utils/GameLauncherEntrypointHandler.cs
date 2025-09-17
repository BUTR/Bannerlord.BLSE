namespace Bannerlord.BLSE.Shared.Utils;

public class GameLauncherEntrypointHandler
{
    public static void Entrypoint(string[] args)
    {
        TaleWorlds.MountAndBlade.Launcher.Library.Program.Main(args);
    }
}