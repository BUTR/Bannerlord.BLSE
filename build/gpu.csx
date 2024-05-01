#r "nuget: PE-Sharp, 1.0.0"

using System.Collections.Generic;
using PE_Sharp.PEStatic;
using PE_Sharp.Exceptions;
using PE_Sharp.PEFormat;
using System.Reflection;
using System.Linq;
using System.IO;
using System;

namespace EditBinPE;
class Program
{
    private static readonly string ExePath = AppDomain.CurrentDomain.BaseDirectory, ConfigPath = ExePath + "/config";

    private const string SectionName = ".patchPE";//.nvpatch

    private static readonly ExportSymbol[] GpuSymbols = {
        new("NvOptimusEnablement"),
        new("AmdPowerXpressRequestHighPerformance")
    };

    public static unsafe int Main(string[] args)
    {
        // Local vars
        List<ExportSymbol> exportSymbols = new();
        var inFile = string.Empty;
        var statusMode = false;
        var enableGpuMode = false;
        var disableGpuMode = false;
        var enableMode = false;
        var disableMode = false;
        var quietYesMode = false;
        var quietNoMode = false;

        // Work out input/output file names
        if (args.Length == 0)
        {
            ShowLogo();
            Console.WriteLine("type '--help' or '/help' to get help");
            return 0;
        }
        
        if (args.Length >= 1)
        {
            if (PEUtils.IsSwitch(args[0], out var name, out var value))
            {
                switch (name)
                {
                    case "enable-gpu":
                        enableGpuMode = true;
                        break;
                    case "disable-gpu":
                        disableGpuMode = true;
                        break;
                    case "enable":
                        enableMode = true;
                        break;
                    case "disable":
                        disableMode = true;
                        break;
                    case "status":
                        statusMode = true;
                        break;
                    case "quiet-yes":
                        quietYesMode = true;
                        break;
                    case "quiet-no":
                        quietNoMode = true;
                        break;
                    case "help":
                        ShowLogo();
                        ShowHelp();
                        return 0;
                    case "version":
                        ShowLogo();
                        return 0;
                }

                if (name != "version" && name != "help" && !quietNoMode && !quietYesMode && args.Length == 1)
                {
                    var exc = "only '--version' '--help' --'quite-no' ";
                    exc += "'--quite-yes' have one options";
                    throw new ArgException(exc);
                }
            }
            else
            {
                var exc = $"don´t exist this option '{args[0]}'";
                throw new ArgException(exc);
            }
        }
        if (args.Length >= 2)
        {
            if (!enableMode && !disableMode && !statusMode
                && !enableGpuMode && !disableGpuMode)
            {
                var exc = "only '--enableMode' '--disableMode' '--enableGpuMode' ";
                exc += "'--disableGpuMode' '--statusMode' have two or more options";
                throw new ArgException(exc);
            }

            Console.WriteLine("DEU BOM 01"); //AAAAAAH
            inFile = args[1];
            if (enableGpuMode || disableGpuMode || statusMode)
                exportSymbols.AddRange(GpuSymbols);
            else if ((enableMode || disableMode) && args.Length == 2)
                throw new ArgException("'--enableMode' '--disableMode' need more than three options");

        }
        if (args.Length >= 3)
        {
            if (enableMode || disableMode)
            {
                var expArgs = args[2..];
                foreach (var exp in expArgs)
                    exportSymbols.Add(new(exp));
            }
            else
            {
                var exc = "only '--enableMode' '--disableMode' have more than three options";
                throw new ArgException(exc);
            }
        }

        if (enableGpuMode) enableMode = true;
        if (disableGpuMode) disableMode = true;

        // config file
        var quietMode = false;
        if (File.Exists(ConfigPath))
        {
            var options = File.ReadAllBytes(ConfigPath);
            if (options.Length > 0)
            {
                if (options[0] == '0')
                {
                    quietMode = false;
                }
                else if (options[0] == '1')
                {
                    Console.WriteLine("DEU BOM 04"); //AAAAAAH
                    quietMode = true;
                }
            }
            else
            {
                quietMode = false;
            }
        }
        else
        {
            Console.WriteLine("DEU BOM 02"); //AAAAAAH
            quietMode = false;
        }

        // Read the file
        var pe = PEFileUtils.GetPEFile(inFile);

        // Get (or create) the export table)
        var exports = new PEExportTable(pe) ?? throw new NullException("exports");

        // Just check it?
        if (statusMode)
        {
            foreach (var symbol in exportSymbols)
            {
                var export = exports.Find(symbol.Name);
                if (export == null)
                {
                    Console.WriteLine($"Module doesn't export {symbol.Name} symbol");
                }
                else
                {
                    var value = *(uint*)pe.GetRVA(export.RVA);
                    Console.WriteLine($"Module exports {symbol.Name} symbol as 0x{value:X8}");
                }
            }
            return 0;
        }

        // Are all the symbols already present, update existing entries
        if (exportSymbols.All(x => exports.Find(x.Name) != null))
        {
            foreach (var symbol in exportSymbols)
            {
                var exportsEntry = exports.Find(symbol.Name) ?? throw new NullException("exportsEntry");
                *(uint*)pe.GetRVA(exportsEntry.RVA) = enableMode ? ExportSymbol.EnableValue : ExportSymbol.DisableValue;
            }
        }
        else
        {
            if (pe.FindSection(SectionName) != null)
            {
                throw new InvalidOperationException($"Can't patch as some symbols are missing and {SectionName} section has already been created");
            }

            // Create a new section into which we'll write the changes
            var newSection = pe.AddSection();
            newSection.Name = SectionName;
            newSection.Characteristics = SectionFlags.InitializedData | SectionFlags.MemRead;

            // Setup the module name
            exports.ModuleName = Path.GetFileName(args[1]);

            // Create entres
            foreach (var symbol in exportSymbols)
            {
                // Add export table entry
                exports.Add(new PEExportTable.Entry()
                {
                    Ordinal = exports.GetNextOrdinal(),
                    Name = symbol.Name,
                    RVA = newSection.CurrentRVA,
                });

                // Write it's value
                newSection.OutputStream.Write((uint)(enableMode ? 1 : 0));
            }

            // Write the new exports table
            var newExportDD = exports.Write(newSection);

            // Patch the data directories with the new export table
            pe.DataDirectories[(int)DataDirectoryIndex.ExportTable] = newExportDD;
        }

        // Clear the checksum (just in case)
        pe.WindowsHeader.CheckSum = 0;

        // Rewrite the file
        pe.Write(inFile);
        pe.Dispose();

        if (!quietMode)
            Console.WriteLine("OK");

        Console.WriteLine("DEU BOM 03"); //AAAAAAH

        return 0;
    }

    static void ShowLogo()
    {
        var show = $"""
                    EditBinPE v{Assembly.GetExecutingAssembly().GetName().Version}
                    PE-Sharp v{PEUtils.GetVersion()}
                    Copyright ©2023 Gabriel Frigo Software. All Rights Reserved

                    """;
        Console.WriteLine(show);
    }

    static void ShowHelp()
    {
        var help = """
                   Usage: BinPeEx <options> [<inputfile.exe>] [<exp..>]

                   Adds, updates or queries the export symbols 'NvOptimusEnablement'
                   and 'AmdPowerXpressRequestHighPerformance' in an existing .exe

                   Options:
                     --enable-gpu   <input>     sets GPU export symbols to 1 (adding if missing)
                     --disable-gpu  <input>     sets GPU export symbols to 0 (if it exists)
                     --enable   <input> <exp..> sets export symbols to 1 (adding if missing)
                     --disable  <input> <exp..> sets export symbols to 0 (if it exists)
                     --status   <input>         shows the current export symbols status
                     --help                     show this help, or help for a command
                     --version                  show version information
                     --quite-yes
                     --quite-no
                   """;
        Console.WriteLine(help);
    }
}