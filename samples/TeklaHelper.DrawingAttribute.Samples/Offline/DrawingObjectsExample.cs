using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Writes the property files of objects of the drawing editor: a text (.drtxt), a line (.gln) and the part marks of a
    /// view as a part mark file (.pm).
    /// </summary>
    internal sealed class DrawingObjectsExample : Example
    {
        public override string Title => "Create property files of drawing objects (.drtxt, .gln, .pm)";

        public override string Description => "Text, line and part mark properties, as saved from the dialogs of the drawing editor.";

        public override void Run(ExampleContext context)
        {
            context.Step("Text properties: blue, 3.5 high");
            TextAttributeFile text = TextAttributeFile.CreateDefault();
            text.Properties.Appearance.Text.Height = 3.5;
            text.Properties.Appearance.Text.Color = DrawingColor.Blue;
            context.ShowFile(text.SaveTo(context.OutputDirectory, "sample_blue_text"), 12);

            context.Step("Line properties: an arrow at the end");
            LineAttributeFile line = LineAttributeFile.CreateDefault();
            line.Properties.Arrow.Position = ArrowPosition.End;
            context.ShowFile(line.SaveTo(context.OutputDirectory, "sample_arrow_line"), 12);

            context.Step("Part mark properties: the part mark page of a view, as a file of the part mark dialog");
            var view = ViewProperties.CreateDefault();
            view.Marks.PartMark.Content.Font.Height = 3;
            PartMarkAttributeFile mark = PartMarkAttributeFile.CreateDefault();
            mark.Properties.CopyFrom(view.Marks.PartMark);
            context.ShowFile(mark.SaveTo(context.OutputDirectory, "sample_view_marks"), 12);

            System.Console.WriteLine();
            System.Console.WriteLine("In Tekla: new Text.TextAttributes(\"sample_blue_text\"), new Line.LineAttributes(\"sample_arrow_line\"),");
            System.Console.WriteLine("          or Load in the dialogs of the drawing editor.");
        }
    }
}
