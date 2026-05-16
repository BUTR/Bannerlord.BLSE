using System;
using System.IO;

using AsmResolver;
using AsmResolver.PE;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE.Exports;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace Bannerlord.BLSE.BuildTasks;

// Post-build PE finalizer for the BLSE loader exes. Does two things in a single
// PE rewrite so they can't undo each other:
//
//   1. Dedicated-GPU hint. Adds an export directory with NvOptimusEnablement and
//      AmdPowerXpressRequestHighPerformance both set to 1. NVIDIA Optimus and
//      AMD PowerXpress look these symbols up by name and route the process to
//      the discrete GPU. Requires the assembly to NOT be IL-only because a
//      pure-IL assembly can't have native exports.
//
//   2. Stack/heap reserve patch. Matches Bannerlord.exe's 4 MB main-thread
//      stack reserve and 16 KB commit. The .NET SDK winexe default is 1 MB,
//      which the video decoder overflows on the first PlayVideo call
//      when our loader hosts the game (manifests as a SO during intro).
//
// Console app rather than an MSBuild task assembly: MSBuild's <UsingTask>
// element is resolved at project-load time, so referencing a DLL that the
// same build session is going to produce creates a chicken-and-egg. <Exec>
// invocation has no such problem — the binary just needs to exist by the
// time the Exec line runs.
internal static class Program
{
    private const ulong StackReserve = 4UL * 1024 * 1024; // 4 MB
    private const ulong StackCommit = 16UL * 1024;       // 16 KB
    private const ulong HeapReserve = 1UL * 1024 * 1024; // 1 MB
    private const ulong HeapCommit = 8UL * 1024;        // 8 KB

    private static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("usage: dotnet Bannerlord.BLSE.BuildTasks.dll <loader.exe>");
            return 1;
        }

        var targetFile = args[0];
        if (!File.Exists(targetFile))
        {
            Console.Error.WriteLine($"FinalizeLoaderExe: target file does not exist: {targetFile}");
            return 1;
        }

        var image = PEImage.FromFile(targetFile);
        if (image.DotNetDirectory is null)
        {
            Console.Error.WriteLine($"FinalizeLoaderExe: '{targetFile}' is not a managed assembly (no CLR header)");
            return 1;
        }

        // Force mixed-mode — required so native exports can be added below.
        image.DotNetDirectory.Flags &= ~DotNetDirectoryFlags.ILOnly;

        // Two DWORDs that NVIDIA/AMD drivers look up by name in the export
        // directory; each set to 1 tells the driver "prefer discrete GPU".
        var nvOptimusEnablement = new DataSegment(BitConverter.GetBytes(1));
        var amdPowerXpressRequestHighPerformance = new DataSegment(BitConverter.GetBytes(1));

        image.Exports = new ExportDirectory(Path.GetFileName(targetFile))
        {
            Entries =
            {
                new ExportedSymbol(nvOptimusEnablement.ToReference(),                  "NvOptimusEnablement"),
                new ExportedSymbol(amdPowerXpressRequestHighPerformance.ToReference(), "AmdPowerXpressRequestHighPerformance"),
            }
        };

        var newFile = new ManagedPEFileBuilder().CreateFile(image);

        // ManagedPEFileBuilder doesn't add custom data sections — drop the two
        // DWORDs into a separate ".extra" section that the export entries reference.
        newFile.Sections.Add(new PESection(
            ".extra",
            SectionFlags.ContentInitializedData | SectionFlags.MemoryRead,
            new SegmentBuilder
            {
                nvOptimusEnablement,
                amdPowerXpressRequestHighPerformance,
            }));

        // ManagedPEFileBuilder.CreateFile produces a fresh PEFile with default
        // stack/heap reserves (1 MB / 1 MB). Re-apply what we want after the
        // rebuild but before writing.
        newFile.OptionalHeader.SizeOfStackReserve = StackReserve;
        newFile.OptionalHeader.SizeOfStackCommit = StackCommit;
        newFile.OptionalHeader.SizeOfHeapReserve = HeapReserve;
        newFile.OptionalHeader.SizeOfHeapCommit = HeapCommit;

        newFile.Write(targetFile);

        Console.WriteLine(
            $"FinalizeLoaderExe: '{Path.GetFileName(targetFile)}' -> stack {StackReserve / (1024 * 1024)} MB / commit {StackCommit / 1024} KB, GPU hint added");
        return 0;
    }
}
