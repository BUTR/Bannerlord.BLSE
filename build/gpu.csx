#r "nuget: AsmResolver.DotNet, 5.5.1"

using AsmResolver;
using AsmResolver.PE;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE.Exports;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

var image = PEImage.FromFile(Args[0]);

// Force mixed-mode.
image.DotNetDirectory!.Flags &= ~DotNetDirectoryFlags.ILOnly;

// Define new file segments that will hold the data.
var nvOptimusEnablement = new DataSegment(BitConverter.GetBytes(1));
var amdPowerXpressRequestHighPerformance = new DataSegment(BitConverter.GetBytes(1));

// Define exports pointing to the new symbols added to the section.
image.Exports = new ExportDirectory(Path.GetFileName(inputPath))
{
    Entries = 
    {
        new ExportedSymbol(nvOptimusEnablement.ToReference(), "NvOptimusEnablement"),
        new ExportedSymbol(amdPowerXpressRequestHighPerformance.ToReference(), "AmdPowerXpressRequestHighPerformance"),
    }
};

// Construct new PE file from image.
var newFile = new ManagedPEFileBuilder().CreateFile(image);

// Add extra data section with the new symbols (managed pe file builder doesn't do this out-of-the-box).
newFile.Sections.Add(new PESection(
    ".extra",
    SectionFlags.ContentInitializedData | SectionFlags.MemoryRead,
    new SegmentBuilder
    {
        nvOptimusEnablement,
        amdPowerXpressRequestHighPerformance,
    }
));

// Write to disk.
string outputPath = Path.ChangeExtension(inputPath, ".patched" + Path.GetExtension(inputPath));
newFile.Write(outputPath);