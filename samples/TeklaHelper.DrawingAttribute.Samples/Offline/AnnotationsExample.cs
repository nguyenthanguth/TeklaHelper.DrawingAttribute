using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Annotations of the drawing editor: text, symbol, level mark, revision mark, weld mark, section mark and detail mark.
    /// </summary>
    internal sealed class AnnotationsExample : Example
    {
        public override string Title => "Annotations: text, symbol, level mark, revision mark, weld mark, section and detail marks";

        public override string Description => ".drtxt .sbl .lev .rev .wls .cs .detail: fonts, frames, leader lines, symbols and contents.";

        public override void Run(ExampleContext context)
        {
            string output = context.OutputDirectory;

            context.Step("Text (.drtxt): word-wrapped text in a rounded frame with a leader line and a base line");
            TextAttributeFile text = TextAttributeFile.CreateDefault();
            text.Properties.Appearance.Text.Font = "Arial";
            text.Properties.Appearance.Text.Height = 3.5;
            text.Properties.Appearance.Text.Alignment = PlacingAlignment.Center;
            text.Properties.Appearance.Text.WordWrapping = true;
            text.Properties.Appearance.Text.RulerWidth = 60;
            text.Properties.Appearance.Frame.Type = FrameAroundMarkType.Round;
            text.Properties.Appearance.Frame.LeaderLine = TextLeaderLineType.BaseLineArrowRight;
            text.Properties.Appearance.Arrow.Type = TextArrowType.SolidArrow;
            text.SaveTo(output, "sample_note_text");

            context.Step("Symbol (.sbl): symbol 3 of xsteel, 5 high, with a leader line");
            SymbolAttributeFile symbol = SymbolAttributeFile.CreateDefault();
            symbol.Properties.SymbolSelection.File = "xsteel";
            symbol.Properties.SymbolSelection.Number = 3;
            symbol.Properties.Appearance.Symbol.Height = 5;
            symbol.Properties.Appearance.Symbol.Color = DrawingColor.Red;
            symbol.Properties.Appearance.Frame.LeaderLine = SymbolLeaderLineType.LeaderLine;
            symbol.SaveTo(output, "sample_symbol");

            context.Step("Level mark (.lev): \"+\" for positive levels, 3 decimals, a leader line with the level symbol");
            LevelMarkAttributeFile level = LevelMarkAttributeFile.CreateDefault();
            level.Properties.General.LevelMarkContent.PrefixForPositiveLevel = PositiveLevelPrefix.Plus;
            level.Properties.General.LevelMarkFormat.Format = AngleDimensionFormat.ThreeDecimals;
            level.Properties.General.LevelMarkContent.Postfix = " m";
            level.Properties.Appearance.Frame.LeaderLine = LevelMarkLeaderLineType.LeaderLineWithSymbol;
            level.Properties.Appearance.Text.Height = 3;
            context.ShowFile(level.SaveTo(output, "sample_level"), 10);

            context.Step("Revision mark (.rev): the fields of a revision and a triangle frame");
            RevisionMarkAttributeFile revision = RevisionMarkAttributeFile.CreateDefault();
            revision.Properties.Revision.Revision.Mark = "A";
            revision.Properties.Revision.Revision.Description = "Beam B12 moved 200 mm";
            revision.Properties.Revision.Revision.CreatedBy = "NT";
            revision.Properties.Appearance.Frame.Type = FrameAroundMarkType.Triangle;
            revision.Properties.Appearance.Frame.LeaderLine = RevisionMarkLeaderLineType.LeaderLine;
            revision.SaveTo(output, "sample_revision_a");

            context.Step("Drawing weld mark (.wls): a 6 mm fillet weld above the line, around, at site");
            DrawingWeldMarkAttributeFile weldMark = DrawingWeldMarkAttributeFile.CreateDefault();
            weldMark.Properties.Content.AboveLine.Type = WeldType.Fillet;
            weldMark.Properties.Content.AboveLine.Size = 6;
            weldMark.Properties.Content.AboveLine.Contour = WeldContour.Flush;
            weldMark.Properties.Content.Other.EdgeAround = WeldEdgeAround.Around;
            weldMark.Properties.Content.Other.WorkshopSite = WeldWorkshopSite.Site;
            weldMark.Properties.Content.Other.ReferenceText = "ISO 2553";
            weldMark.SaveTo(output, "sample_fillet_6_site");

            context.Step("Section mark (.cs) and detail mark (.detail)");
            SectionMarkAttributeFile section = SectionMarkAttributeFile.CreateDefault();
            section.Properties.CuttingLine.Properties.Color = DrawingColor.Blue;
            section.Properties.CuttingLine.Line.ArrowLength = 8;
            section.Properties.SectionMark.Symbol.Color = DrawingColor.Blue;
            section.Properties.SectionName = "A";
            section.SaveTo(output, "sample_section_a");

            DetailMarkAttributeFile detail = DetailMarkAttributeFile.CreateDefault();
            detail.Properties.DetailBoundary.Shape.OfTheBoundary = DetailBoundaryShape.Circle;
            detail.Properties.DetailBoundary.BoundingLine.LineType = DrawingLineType.Dashed;
            detail.Properties.DetailName = "D1";
            detail.SaveTo(output, "sample_detail_d1");

            System.Console.WriteLine();
            System.Console.WriteLine("In Tekla: new Text(view, point, \"Check\", new Text.TextAttributes(\"sample_note_text\")).Insert();");
            System.Console.WriteLine("          new Symbol(view, point, new SymbolInfo(\"xsteel\", 3), new Symbol.SymbolAttributes(\"sample_symbol\"));");
            System.Console.WriteLine("          new LevelMark(view, insertion, base, new LevelMark.LevelMarkAttributes(\"sample_level\"));");
            System.Console.WriteLine("          View.CreateSectionView(..., new SectionMarkBase.SectionMarkAttributes(\"sample_section_a\"), ...).");
        }
    }
}
