using System;
using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// One company style written once and applied everywhere: the same part mark and part representation in a view, a GA
    /// drawing, the assembly family, the part mark file of the drawing editor and the page file of the dialog. A page is
    /// one class whatever file it lives in, so <c>CopyFrom</c> moves it between hosts (each with its own key prefix).
    /// </summary>
    internal sealed class SharedStyleExample : Example
    {
        public override string Title => "One style for every host: view, GA, assembly family, .pm and page files";

        public override string Description => "CopyFrom of the part mark and part pages into .vi, .gd, .ad, .wd, .cud and .pm; ExportTo a .vpm page file.";

        public override void Run(ExampleContext context)
        {
            context.Step("The style: a part mark and a part page, set up once on a view");
            ViewProperties style = ViewProperties.CreateDefault();
            PartMark mark = style.Marks.PartMark;
            mark.Content.Font.Font = "Arial Narrow";
            mark.Content.Font.Height = 2.5;
            mark.Content.Font.Color = DrawingColor.Blue;
            mark.General.FrameAroundMark.Type = FrameAroundMarkType.Rectangular;
            mark.General.LeaderLine.Arrow = LeaderLineArrow.SolidArrow;
            mark.Content.MainPartElements = new Marks.MarkContent().Add(Marks.MarkElementTypes.PartMark.AssemblyPosition);
            PartObject part = style.Objects.Part;
            part.Content.Representation.Representation = Representation.Exact;
            part.Content.HiddenLines.HiddenLines = true;
            part.Appearance.HiddenLines.Type = DrawingLineType.Dashed;

            context.Step("Apply it to a GA drawing and to the drawings of the assembly family");
            GADrawingAttributeFile ga = GADrawingAttributeFile.CreateDefault();
            DrawingAttributeFile[] drawings =
            {
                ga,
                AssemblyDrawingAttributeFile.CreateDefault(),
                SinglePartDrawingAttributeFile.CreateDefault(),
                CastUnitDrawingAttributeFile.CreateDefault(),
            };
            foreach (DrawingAttributeFile drawing in drawings)
            {
                drawing.Marks.PartMark.CopyFrom(mark);                              // vpm.* -> dpm.*
                drawing.Objects.Part.CopyFrom(part);                                // vp.* -> dp.*
                string path = drawing.SaveTo(context.OutputDirectory, "sample_style");
                Console.WriteLine($"    {path}: dpm.ContentString1 = {drawing.Marks.PartMark.Content.MainPartElements}");
            }

            context.Step("The part mark file of the drawing editor (.pm) uses the same page");
            PartMarkAttributeFile pm = PartMarkAttributeFile.CreateDefault();
            pm.Properties.CopyFrom(mark);
            context.ShowFile(pm.SaveTo(context.OutputDirectory, "sample_style"), 8);

            context.Step("And the page file of the View properties dialog (.vpm), for its Load button");
            string page = mark.ExportTo(context.OutputDirectory, "sample_style", PartMark.ViewFileExtension);
            Console.WriteLine("    " + page);

            context.Step("Read the page file into another view");
            var other = new ViewProperties();
            other.Marks.PartMark.ImportFrom(page);
            Console.WriteLine($"    Imported: font {other.Marks.PartMark.Content.Font.Font}, colour {other.Marks.PartMark.Content.Font.Color}, " +
                              $"vpm.in_file = {other.GetString("vpm.in_file")}");
        }
    }
}
