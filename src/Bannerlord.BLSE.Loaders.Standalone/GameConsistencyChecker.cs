using System;
using System.IO;
using System.Windows.Forms;

namespace Bannerlord.BLSE.Shared.Utils;

internal static class GameConsistencyChecker
{
    public static void Verify()
    {
        var assemblyFile = new FileInfo(typeof(GameConsistencyChecker).Assembly.Location);

        var platform = assemblyFile.Directory!.Name;
        if (platform is not "Win64_Shipping_Client" and not "Gaming.Desktop.x64_Shipping_Client")
        {
            MessageBox.Show($"""
BLSE was installed in the wrong location!
The BLSE containing folder is named '{platform}'
Make sure BLSE containing folder name is:
* 'Win64_Shipping_Client' for Steam/GOG/Epic Games
* 'Gaming.Desktop.x64_Shipping_Client' for Xbox Game Pass PC
""", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }

        var twLibrary = new FileInfo("TaleWorlds.Library.dll");
        if (!twLibrary.Exists)
        {
            MessageBox.Show($"""
Failed to find the necessary game files!
Make sure BLSE is installed in '%GAME FOLDER%/bin/%PLATFORM%'
Where %PLATFORM% is:
* 'Win64_Shipping_Client' for Steam/GOG/Epic Games
* 'Gaming.Desktop.x64_Shipping_Client' for Xbox Game Pass PC
""", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }
    }
}