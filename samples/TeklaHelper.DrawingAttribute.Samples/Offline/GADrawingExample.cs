using System;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// GA drawing properties: the four files the Open API reads for <c>new GADrawing(name, attributeFile)</c>: name.gd,
    /// name.gd.more (user-defined attributes), name.gd.filter and name.gd.npfilter.
    /// </summary>
    internal sealed class GADrawingExample : Example
    {
        public override string Title => "Create GA drawing properties (.gd, .gd.more, .gd.filter, .gd.npfilter)";

        public override string Description => "Titles, layout, view creation, object and mark pages, protection, user-defined attributes, filters.";

        public override void Run(ExampleContext context)
        {
            GADrawingAttributeFile drawing = GADrawingAttributeFile.CreateDefault();
            drawing.Title1 = "PLAN +3.600";
            drawing.Title2 = "STEEL STRUCTURE";
            drawing.Description = "Sample GA drawing";

            context.Step("Layout and view creation");
            drawing.Layout.LayoutName = "ga";           // the GA layout every environment has
            drawing.Layout.AutoScale = false;
            drawing.Views.Scale = 100;

            context.Step("The object and mark pages have the same tabs and groups as in a view, under the drawing prefixes");
            drawing.Objects.Part.Content.Representation.Representation = Representation.Outline;
            drawing.Objects.Part.Content.HiddenLines.HiddenLines = false;
            drawing.Objects.Grid.Grid = Grid.Visible;
            drawing.Objects.Grid.Text.Height = 5;
            drawing.Objects.Grid.Text.Frame = TextFrame.Circle;
            drawing.Marks.PartMark.Content.Font.Height = 3.5;
            drawing.Marks.PartMark.General.FrameAroundMark.Type = FrameAroundMarkType.Rectangular;
            drawing.Marks.BoltMark.General.VisibilityOfBoltMarks.VisibilityInView = VisibilityInView.None;

            // The GA family has reference object settings; assembly drawings do not (ReferenceObject is null there)
            drawing.Objects.ReferenceObject.Appearance.VisibleLines.Color = DrawingColor.Gray50;

            context.Step("Protection: keep new marks away from the parts and the texts already drawn");
            drawing.Protection.TextPart = true;
            drawing.Protection.TextText = true;

            context.Step("User-defined attributes, saved as name.gd.more");
            drawing.UserAttributes.DrawnBy = "NT";
            drawing.UserAttributes.CheckedBy = "QA";
            drawing.UserAttributes.CheckedDate = new DateTime(2026, 10, 22);
            drawing.UserAttributes.Comment = "Issued for approval";
            drawing.UserAttributes.SetUserField(1, "REV A");

            context.Step("Drawing filter (name.gd.filter) and neighbor part filter (name.gd.npfilter)");
            drawing.Filter.SetExpression(FilterProperties.Part.Phase.IsEqual(1));
            drawing.NeighborPartFilter.SetExpression(FilterProperties.Part.Name.IsNotEqual("STIFFENER"));

            string path = drawing.SaveTo(context.OutputDirectory, "sample_plan_100");
            context.ShowFile(path, 15);
            context.ShowFile(path + ".more");
            context.ShowFile(path + ".filter", 15);

            Console.WriteLine();
            Console.WriteLine("In Tekla: var ga = new GADrawing(\"PLAN +3.600\", \"sample_plan_100\"); ga.Insert();");
        }
    }
}
