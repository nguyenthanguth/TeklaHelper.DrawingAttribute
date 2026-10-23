using System;
using System.IO;
using System.Linq;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Pages;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>
    /// The pages of the drawing properties that the View properties dialog does not have: View (<c>dview.</c> / <c>dv.</c>),
    /// Section view (<c>dc.</c>), Detail view (<c>DetailLabel.</c>), Dimension (<c>dd.</c>) and the two Dimensioning
    /// pages (<c>Dimensioning.</c> in GA drawings, <c>dcd.</c> in the assembly family). Key sets checked against
    /// DRAWING-TEST.gd and test-ad.ad saved from the Tekla 2025 dialogs.
    /// </summary>
    public sealed class DrawingPagesTests
    {
        [Fact]
        public void ViewPage_UsesThePrefixOfEachFamily()
        {
            var ga = new GADrawingAttributeFile();
            var assembly = new AssemblyDrawingAttributeFile();

            ga.Views.Attributes.ViewSize.ViewExtensionForNeighborParts = 321;
            assembly.Views.Attributes.ViewSize.ViewExtensionForNeighborParts = 321;
            ga.Views.LabelMarkA2.Content.Font.Height = 5;
            assembly.Views.Label.ViewName.FrontView = "FRONT";

            Assert.Equal("321.000000", ga.Values["dview.np_extrema_size"].RawText);
            Assert.Equal("321.000000", assembly.Values["dv.np_extrema_size"].RawText);
            Assert.Equal("5.000000", ga.Values["dview.ViewLabelMarkA2.FontAttr.height"].RawText);
            Assert.Equal("\"FRONT\"", assembly.Values["dv.aLabelFrontView"].ToFileText());
        }

        [Fact]
        public void ViewPage_KeepsTheHandWrittenProperties()
        {
            var drawing = new AssemblyDrawingAttributeFile();

            drawing.Views.Scale = 20;
            drawing.Views.CreateTopView = ViewCreation.On;
            drawing.Views.Shortening.CutParts.CutSkewParts = true;

            Assert.Equal(new[] { "dv.main_scale", "dv.cre_top_view", "dv.cut_skew_parts" }, drawing.Values.Keys);
        }

        [Fact]
        public void SectionViewPage_WritesTheSectionMarkAndItsLabelRows()
        {
            var drawing = new GADrawingAttributeFile();

            drawing.SectionView.Attributes.Scale.Size = SectionViewSize.FitByParts;
            drawing.SectionView.CuttingLine.Properties.Color = DrawingColor.Blue;
            drawing.SectionView.CuttingLine.Line.LineArrowLength = 33;
            drawing.SectionView.SectionMarkA1.Content.Font.Height = 4;
            drawing.SectionView.LabelMarkA5.Content.Font.Height = 6;

            Assert.Equal(
                new[]
                {
                    "dc.size_mode 1", "dc.SectionMark.line_colour 162", "dc.SectionMark.ArrowLength 33.000000",
                    "dc.SectionMark.SectionMarkA1.FontAttr.height 4.000000", "dc.SectionMark.ViewLabelMarkA5.FontAttr.height 6.000000",
                },
                drawing.Values.Select(p => p.Key + " " + p.Value.ToFileText()));
        }

        [Fact]
        public void DetailViewPage_WritesTheBoundaryAndTheDetailMarkRows()
        {
            var drawing = new SinglePartDrawingAttributeFile();

            drawing.DetailView.DetailBoundary.Shape.ShapeOfTheBoundary = DetailBoundaryShape.Rectangle;
            drawing.DetailView.DetailMarkA3.Content.Font.Height = 3;

            Assert.Equal("1", drawing.Values["DetailLabel.SymbolShape"].RawText);
            Assert.Equal("3.000000", drawing.Values["DetailLabel.DetailMarkA3.FontAttr.height"].RawText);
        }

        [Fact]
        public void DimensionPage_WritesTheValuesOfTheDimensionTypes()
        {
            var drawing = new CastUnitDrawingAttributeFile();

            drawing.Dimension.General.DimensionTypes.Straight = DimensionType.Absolute;
            drawing.Dimension.General.DimensionTypes.Precision.Straight = DimensionPrecision.OnePerEight;
            drawing.Dimension.Appearance.Text.Height = 5;

            Assert.Equal(new[] { "dd.dim_type 2", "dd.dim_precision 8", "dd.dim_text_height 5.000000" }, drawing.Values.Select(p => p.Key + " " + p.Value.ToFileText()));
        }

        [Fact]
        public void DimensioningPages_BelongToTheirFamily()
        {
            var ga = new GADrawingAttributeFile();
            var multi = new MultiDrawingAttributeFile();
            var assembly = new AssemblyDrawingAttributeFile();

            Assert.NotNull(ga.Dimensioning);
            Assert.NotNull(multi.Dimensioning);
            Assert.Null(ga.AssemblyDimensioning);
            Assert.Null(assembly.Dimensioning);
            Assert.NotNull(new SinglePartDrawingAttributeFile().AssemblyDimensioning);
            Assert.NotNull(new CastUnitDrawingAttributeFile().AssemblyDimensioning);

            ga.Dimensioning.Grid.DimensionCreation.GridLineDimensions = true;
            assembly.AssemblyDimensioning.PartDimensions.MainPartOverallDimensions = MainPartOverallDimensions.None;

            Assert.Equal("1", ga.Values["Dimensioning.GridLineDims"].RawText);
            Assert.Equal("0", assembly.Values["dcd.main_part_extrema"].RawText);
        }

        [Fact]
        public void CastUnitDefinitionMethod_ReadsAndWritesBoundType()
        {
            var drawing = new CastUnitDrawingAttributeFile();
            Assert.Equal(CastUnitDefinitionMethod.ByCastUnitPosition, drawing.CastUnitDefinitionMethod);

            drawing.CastUnitDefinitionMethod = CastUnitDefinitionMethod.ByCastUnitId;

            Assert.Equal("2", drawing.Values["bound_type"].RawText);
        }

        [Fact]
        public void SetDefaults_OfADrawingPage_WritesOnlyItsKeysAndItsLabelRows()
        {
            var drawing = new GADrawingAttributeFile();

            drawing.SectionView.SetDefaults();

            Assert.All(drawing.Values.Keys, key => Assert.StartsWith("dc.", key, StringComparison.Ordinal));
            Assert.Contains("dc.SectionMark.SectionMarkA1.FontAttr.height", drawing.Values.Keys);
            Assert.Contains("dc.SectionMark.ViewLabelMarkA5.ContentString", drawing.Values.Keys);
            Assert.Equal("1", drawing.Values["dc.SectionMark.line_colour_en"].RawText);
        }

        [Fact]
        public void CreateDefault_KeepsTheValuesOfItsOwnFileType()
        {
            // The drawing pages take the value most shipped files of that drawing type hold, not SetDefaults(), which serves
            // every host: GA and assembly drawings do not agree on dimensions
            var ga = GADrawingAttributeFile.CreateDefault();
            var assembly = AssemblyDrawingAttributeFile.CreateDefault();

            Assert.True(ga.Values.ContainsKey("dd.dim_type"));
            Assert.True(assembly.Values.ContainsKey("dcd.main_part_extrema"));
            Assert.True(ga.Values.ContainsKey("Dimensioning.GridLineDims"));
            Assert.False(assembly.Values.ContainsKey("Dimensioning.GridLineDims"));
            Assert.False(ga.Values.ContainsKey("dcd.main_part_extrema"));
        }

        [Fact]
        public void PageFiles_RoundTripThroughExportAndImport()
        {
            string directory = Path.Combine(Path.GetTempPath(), "TeklaHelper.DrawingAttribute.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                var source = AssemblyDrawingAttributeFile.CreateDefault();
                source.Dimension.Appearance.Text.Height = 7;
                string path = source.Dimension.ExportTo(directory, "dims", SectionFileExtensions.SinglePartDrawingDimensioning);
                var target = new SinglePartDrawingAttributeFile();

                target.Dimension.ImportFrom(path);

                Assert.Equal(7, target.Dimension.Appearance.Text.Height);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
