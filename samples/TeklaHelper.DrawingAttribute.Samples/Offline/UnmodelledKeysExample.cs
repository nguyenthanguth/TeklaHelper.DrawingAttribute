using System;
using System.IO;
using System.Linq;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Keys without a typed property: the Advanced group of each page, typed Get/Set by key, sections by prefix, and
    /// attribute files of any other type.
    /// </summary>
    internal sealed class UnmodelledKeysExample : Example
    {
        public override string Title => "Keys without a typed property, other file types";

        public override string Description => "Advanced groups, GetSection / Get* / Set* by key, enumerate keys, GenericAttributeFile for any extension.";

        public override void Run(ExampleContext context)
        {
            var drawing = new GADrawingAttributeFile();

            context.Step("Keys the dialog does not show are in the page's Advanced group");
            drawing.Marks.PartMark.Advanced.MergeMarks = true;
            drawing.Marks.PartMark.Advanced.Rotation = 90;
            Console.WriteLine($"    dpm.MergeMarks = {drawing.GetInt32("dpm.MergeMarks")}, dpm.Rotation = {drawing.GetInt32("dpm.Rotation")}");

            context.Step("Any key by name, typed: the dimensioning settings (dd.*) have no page class yet");
            AttributeSection dimensioning = drawing.GetSection("dd.");
            dimensioning.SetDouble("dim_text_height", 2.5);
            dimensioning.SetEnum("dim_text_colour", DrawingColor.Blue);
            Console.WriteLine($"    dd.dim_text_height = {drawing.GetDouble("dd.dim_text_height")}");
            Console.WriteLine($"    dd.dim_text_colour = {dimensioning.GetEnum<DrawingColor>("dim_text_colour")}");

            context.Step("Enumerate the keys of a page (relative to its prefix) or of the whole file");
            var view = new ViewProperties();
            view.Objects.Part.Content.HiddenLines.HiddenLines = true;
            view.Objects.Part.Appearance.HiddenLines.Color = DrawingColor.Gray70;
            Console.WriteLine("    vp.* : " + string.Join(", ", view.Objects.Part.GetNames()));
            Console.WriteLine("    file : " + string.Join(", ", view.Values.Keys));

            context.Step("Any other attribute file type, by key (here a dimension properties file, .dim)");
            var other = new GenericAttributeFile(".dim");
            other.SetDouble("dim_text_height", 3);
            other.SetEnum("dim_text_colour", DrawingColor.Blue);
            other.SetEnum("dim_line_colour", DrawingColor.Black);
            string path = other.SaveTo(context.OutputDirectory, "sample_generic");
            context.ShowFile(path);
            var reloaded = new GenericAttributeFile(".dim");
            reloaded.Load(path);
            Console.WriteLine("    Reloaded keys: " + string.Join(", ", reloaded.Values.Keys));
            Console.WriteLine("    Files written: " + string.Join(", ", Directory.GetFiles(context.OutputDirectory, "sample_generic*").Select(Path.GetFileName)));
        }
    }
}
