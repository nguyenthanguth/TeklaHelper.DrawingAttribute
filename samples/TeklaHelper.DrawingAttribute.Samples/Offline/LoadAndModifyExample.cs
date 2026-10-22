using System;
using System.IO;
using System.Linq;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Loads an existing .vi (one saved from the Tekla dialog is best: it holds every setting), reads its pages, changes a
    /// few values and saves it under a new name. Everything else is written back byte for byte.
    /// </summary>
    internal sealed class LoadAndModifyExample : Example
    {
        public override string Title => "Load an existing .vi, read and change it, save under a new name";

        public override string Description => "Start from a file saved in Tekla (e.g. model\\attributes\\standard.vi); unchanged values keep their bytes.";

        public override void Run(ExampleContext context)
        {
            string template = Path.Combine(context.OutputDirectory, "sample_template.vi");
            if (!File.Exists(template))
            {
                context.Step("No template yet: write one (in practice, use a file saved from the View properties dialog)");
                var start = new ViewProperties();
                start.Attributes.Attributes1.View.Scale = 25;
                start.Objects.Part.Content.Representation.Representation = Representation.Outline;
                start.SetString("SomeKeyThisLibraryDoesNotKnow", "kept as it is");
                start.SaveTo(context.OutputDirectory, "sample_template");
            }

            string path = context.Ask("Path of a .vi file to load", template);

            context.Step("Load " + path);
            var view = AttributeFile.FromFile<ViewProperties>(path);
            Console.WriteLine($"    {view.Values.Count} keys, code page {view.CodePage}");
            Console.WriteLine($"    Scale 1:{view.Attributes.Attributes1.View.Scale}");
            Console.WriteLine($"    Part representation: {view.Objects.Part.Content.Representation.Representation}");
            Console.WriteLine($"    Part hidden lines: {view.Objects.Part.Content.HiddenLines.HiddenLines}");
            Console.WriteLine($"    Grid: {view.Objects.Grid.Grid}, text height {view.Objects.Grid.Text.Height}");
            Console.WriteLine($"    Part mark font: '{view.Marks.PartMark.Content.Font.Font}' {view.Marks.PartMark.Content.Font.Height}");
            Console.WriteLine($"    Filter rows: {view.Filter.Rows.Count}, neighbor part filter rows: {view.NeighborPartFilter.Rows.Count}");

            context.Step("Nothing changed yet: the bytes it would write are the bytes it read");
            bool identical = File.ReadAllBytes(path).SequenceEqual(view.ToBytes());
            Console.WriteLine("    Identical: " + identical);

            context.Step("Change a few settings and save as a new file");
            view.Attributes.Attributes1.View.Scale = 10;
            view.Objects.Part.Content.Representation.Representation = Representation.Exact;
            view.Objects.Part.Content.HiddenLines.HiddenLines = true;
            string saved = view.SaveTo(context.OutputDirectory, "sample_template_1_10");

            // Keys keep their order; a key set for the first time is added at the end
            string[] before = File.ReadAllLines(path);
            string[] after = File.ReadAllLines(saved);
            foreach (string line in after.Except(before))
            {
                Console.WriteLine("    + " + line);
            }

            foreach (string line in before.Except(after))
            {
                Console.WriteLine("    - " + line);
            }
        }
    }
}
