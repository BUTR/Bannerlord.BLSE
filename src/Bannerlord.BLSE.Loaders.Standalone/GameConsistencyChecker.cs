using System;
using System.IO;
using System.Windows.Forms;

namespace Bannerlord.BLSE.Shared.Utils;

internal static class GameConsistencyChecker
{
    public static void Verify()
    {
        var assemblyFile = new FileInfo(AppContext.BaseDirectory);
        var binDirectory = assemblyFile.Directory!;

        var platform = binDirectory.Name;
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

        // Shortcuts and external tools can start us from another folder, while BLSE and the game
        // resolve everything relative to the working directory
        Directory.SetCurrentDirectory(binDirectory.FullName);

        var twLibrary = new FileInfo(Path.Combine(binDirectory.FullName, "TaleWorlds.Library.dll"));
        if (!twLibrary.Exists)
        {
            // A common mistake is extracting the archive's 'bin/%PLATFORM%' into the game's 'bin/%PLATFORM%'
            var outerBinDirectory = binDirectory.Parent?.Parent;
            var isNested = binDirectory.Parent?.Name == "bin" && outerBinDirectory?.Name == platform &&
                           File.Exists(Path.Combine(outerBinDirectory.FullName, "TaleWorlds.Library.dll"));
            var hint = isNested
                ? $"""
BLSE was installed one folder too deep!
Move the BLSE files from
'{binDirectory.FullName}'
to
'{outerBinDirectory!.FullName}'
"""
                : """
Make sure BLSE is installed in '%GAME FOLDER%/bin/%PLATFORM%'
Where %PLATFORM% is:
* 'Win64_Shipping_Client' for Steam/GOG/Epic Games
* 'Gaming.Desktop.x64_Shipping_Client' for Xbox Game Pass PC
""";
            MessageBox.Show($"""
Failed to find the necessary game files!
'TaleWorlds.Library.dll' was not found in
'{binDirectory.FullName}'
{hint}
""", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }
    }
}