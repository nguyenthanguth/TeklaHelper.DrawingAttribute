using System;
using System.IO;
using System.Linq;
using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>
    /// The examples of README.md, without the lines that need Tekla Structures running. Keep them in step.
    /// </summary>
    public sealed class ReadmeExamplesTests : IDisposable
    {
        private readonly string _attributes = Path.Combine(Path.GetTempPath(), "TeklaHelper.DrawingAttribute.Tests", Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_attributes))
            {
                Directory.Delete(_attributes, recursive: true);
            }
        }

        [Fact]
        public void QuickStart()
        {
            string attributes = _attributes;
            new GADrawingAttributeFile { Title1 = "TEMPLATE" }.SaveTo(attributes, "standard");

            var ga = AttributeFile.FromFile<GADrawingAttributeFile>(Path.Combine(attributes, "standard.gd"));
            ga.Title1 = "PLAN +3.600";
            ga.Views.Scale = 100;
            ga.Objects.Grid.Text.Height = 3.5;
            ga.Objects.Grid.GridLine.Color = DrawingColor.Red;
            ga.Marks.PartMark.Content.Font.Height = 3;
            ga.UserAttributes.DrawnBy = "NT";
            ga.SaveTo(attributes, "plan_100");

            var vi = new ViewProperties();
            vi.Attributes.Attributes1.View.Scale = 20;
            vi.Attributes.Attributes2.CutParts.Shortening.CutParts = CutParts.BothDirections;
            vi.Objects.Grid.Grid = Grid.NotVisible;
            vi.Objects.Part.Content.Representation.Representation = Representation.Exact;
            vi.Objects.Part.Content.HiddenLines.HiddenLines = true;
            vi.Objects.Part.Appearance.VisibleLines.Color = DrawingColor.Blue;
            vi.Filter.SetExpression(new FilterGroup()
                .Add(FilterProperties.Part.Name.IsEqual("BEAM"), FilterLogic.Or)
                .Add(FilterProperties.Part.Name.IsEqual("COLUMN")));
            vi.SaveTo(attributes, "detail_20");

            Assert.True(File.Exists(Path.Combine(attributes, "plan_100.gd.more")));
            Assert.True(File.Exists(Path.Combine(attributes, "detail_20.vi.filter")));
        }

        [Fact]
        public void DefaultSettings()
        {
            string attributes = _attributes;

            ViewProperties view = ViewProperties.CreateDefault();
            view.Attributes.Attributes1.View.Scale = 50;
            view.SaveTo(attributes, "plan_50");

            GADrawingAttributeFile ga = GADrawingAttributeFile.CreateDefault();
            view.Objects.Part.SetDefaults();

            Assert.Contains("view_scale 50.000000", File.ReadAllText(Path.Combine(attributes, "plan_50.vi")));
            Assert.True(ga.Values.Count > 2000);
            Assert.Equal(Representation.Outline, view.Objects.Part.Content.Representation.Representation);
        }

        [Fact]
        public void Pages()
        {
            string attributes = _attributes;
            var view = new ViewProperties();
            view.Marks.PartMark.Content.Font.Font = "Arial";
            AssemblyDrawingAttributeFile assemblyDrawing = AssemblyDrawingAttributeFile.CreateDefault();
            var ga = new GADrawingAttributeFile();

            assemblyDrawing.Marks.PartMark.CopyFrom(view.Marks.PartMark);

            view.Objects.Grid.ExportTo(attributes, "red_grid", GridObject.ViewFileExtension);
            ga.Objects.Grid.ImportFrom(Path.Combine(attributes, "red_grid.vg"));

            ViewProperties.CreateDefault().SaveTo(attributes, "assembly_views");
            assemblyDrawing.Views.SetViewAttributeFile("assembly_views");
            assemblyDrawing.Views.CreateTopView = ViewCreation.On;
            Assert.All(assemblyDrawing.Views.GetViewTable(), row => Assert.Equal("assembly_views", row.ViewAttributeFile));
            Assert.Equal(ViewCreation.On, assemblyDrawing.Views.GetViewTable().Single(row => row.ViewType == DrawingViewType.Top).Creation);

            assemblyDrawing.Dimension.General.DimensionTypes.Straight = DimensionType.Absolute;
            assemblyDrawing.Dimension.Appearance.Text.Height = 3.5;
            assemblyDrawing.SectionView.CuttingLine.Properties.Color = DrawingColor.Blue;
            assemblyDrawing.AssemblyDimensioning.PartDimensions.MainPartOverallDimensions = MainPartOverallDimensions.All;
            ga.Views.Attributes.ViewSize.ViewExtensionForNeighborParts = 500;
            ga.Dimensioning.Grid.DimensionCreation.GridLineDimensions = true;

            ga.Layout.SetInt32("placing_direction", 0);

            Assert.Equal("Arial", assemblyDrawing.Marks.PartMark.Content.Font.Font);
            Assert.Equal(2, assemblyDrawing.GetInt32("dd.dim_type"));
            Assert.Equal(162, assemblyDrawing.GetInt32("dc.SectionMark.line_colour"));
            Assert.Equal(500, ga.GetDouble("dview.np_extrema_size"));
            Assert.Equal(0, ga.GetInt32("dl.placing_direction"));
        }

        [Fact]
        public void DimensionProperties()
        {
            string attributes = _attributes;
            DimensionAttributeFile dimension = DimensionAttributeFile.CreateDefault();
            dimension.Properties.General.DimensionTypes.Straight = StraightDimensionType.Absolute;
            dimension.Properties.General.DimensionFormat.Precision = DimensionPrecision.OnePerEight;
            dimension.Properties.Appearance.Text.Height = 3.5;
            dimension.Properties.Marks.Exaggeration.Exaggeration = Exaggeration.Specified;
            dimension.PrefixMark.Font.Height = 2.5;
            dimension.SaveTo(attributes, "absolute");

            var read = AttributeFile.FromFile<DimensionAttributeFile>(Path.Combine(attributes, "absolute.dim"));
            Assert.Equal(StraightDimensionType.Absolute, read.Properties.General.DimensionTypes.Straight);
            Assert.Equal(2.5, read.GetDouble("PrefixMark.FontAttr.height"));
        }

        [Fact]
        public void Filters()
        {
            string attributes = _attributes;
            var expression = new FilterGroup()
                .Add(FilterProperties.Part.Name.IsEqual("BEAM"), FilterLogic.And)
                .Add(new FilterGroup()
                    .Add(FilterProperties.Part.Profile.BeginsWith("HEA"), FilterLogic.Or)
                    .Add(FilterProperties.Part.Phase.GreaterOrEqual(2)));

            FilterFile.FromExpression(FilterFileExtensions.ViewFilter, expression).SaveTo(attributes, "standard");

            FilterFile filter = FilterFile.FromFile(Path.Combine(attributes, "standard.vf"));
            filter.Rows[0].Value = "COLUMN";
            filter.Save(Path.Combine(attributes, "columns.vf"));

            FilterProperties.Assembly.CustomString("USER_FIELD_1").Contains("A");

            FilterCondition noRebar = FilterProperties.Object.Type.IsNotEqual(FilterObjectType.ReinforcingBar);
            FilterCondition partsOrBolts = FilterProperties.Object.Type.IsEqual(FilterObjectType.Part, FilterObjectType.BoltGroup);

            Assert.Equal("COLUMN", FilterFile.FromFile(Path.Combine(attributes, "columns.vf")).Rows[0].Value);
            Assert.Equal("albl_Reinforcement_bar", noRebar.Value);
            Assert.Equal("albl_Part albl_BoltGroup", partsOrBolts.Value);
        }
    }
}
