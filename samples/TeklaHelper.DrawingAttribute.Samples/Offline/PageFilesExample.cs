using System;
using System.IO;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// The files of single pages (.vp, .vpm, .vg, ...): the "Save as" of one page of the dialog. Tekla's API does not read
    /// them, but people load them by hand in the dialogs, so it is useful to write them, and to read them into a view or
    /// a drawing.
    /// </summary>
    internal sealed class PageFilesExample : Example
    {
        public override string Title => "Page files (.vp, .vpm, .vg, ...): export, import, share between view and drawing";

        public override string Description => "Write the file of one page for the dialogs; read a page file into a view or a GA drawing.";

        public override void Run(ExampleContext context)
        {
            var view = new ViewProperties();
            view.Objects.Part.Content.Representation.Representation = Representation.Exact;
            view.Objects.Part.Appearance.VisibleLines.Color = DrawingColor.Blue;
            view.Objects.Grid.Grid = Grid.Visible;
            view.Objects.Grid.GridLine.Color = DrawingColor.Red;
            view.Marks.PartMark.Content.Font.Height = 3;

            context.Step("Export pages: the keys of the page without its prefix, as the dialog's Save as writes them");
            string partFile = view.Objects.Part.ExportTo(context.OutputDirectory, "sample_exact", PartObject.ViewFileExtension);
            string gridFile = view.Objects.Grid.ExportTo(context.OutputDirectory, "sample_red", GridObject.ViewFileExtension);
            string markFile = view.Marks.PartMark.ExportTo(context.OutputDirectory, "sample_marks", PartMark.ViewFileExtension);
            context.ShowFile(gridFile);
            Console.WriteLine("    also " + Path.GetFileName(partFile) + ", " + Path.GetFileName(markFile));

            context.Step("Import the grid page into a GA drawing (dgrid.*) and the part mark page into an assembly drawing (dpm.*)");
            var ga = new GADrawingAttributeFile();
            ga.Objects.Grid.ImportFrom(gridFile);
            var assembly = new AssemblyDrawingAttributeFile();
            assembly.Marks.PartMark.ImportFrom(markFile);
            Console.WriteLine($"    GA grid: {ga.Objects.Grid.Grid}, colour {ga.Objects.Grid.GridLine.Color}, loaded from '{ga.Objects.Grid.GetString("in_file")}'");
            Console.WriteLine($"    Assembly part mark height: {assembly.Marks.PartMark.Content.Font.Height}");

            context.Step("The same page file extension in a drawing dialog: GridObject.GADrawingFileExtension");
            string gaGridFile = ga.Objects.Grid.ExportTo(context.OutputDirectory, "sample_red", GridObject.GADrawingFileExtension);
            Console.WriteLine("    " + gaGridFile);
        }
    }
}
