using System;
using System.IO;
using System.Windows.Forms;

namespace Bannerlord.BLSE;

public static class NETFrameworkLoader
{
    private static Type MessageBox = typeof(MessageBox);

    public static void Launch(string[] args)
    {
        Blank(MessageBox.FullName ?? string.Empty); // Force load System.Windows.Forms
        Shared.Program.Main(args);
    }

    private static void Blank(string name)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms);
        writer.Write(name);
    }
}