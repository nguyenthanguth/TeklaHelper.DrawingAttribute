using System;
using System.IO;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    public sealed class DrawingAttributeFileTests : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "TeklaHelper.DrawingAttribute.Tests", Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }

        [Theory]
        [InlineData(typeof(GADrawingAttributeFile), ".gd", "gadrawing_attributes", "dview.scale", "dgrid.grid_on", "dnpart.draw_as", "drebar_mark.create")]
        [InlineData(typeof(MultiDrawingAttributeFile), ".md", "joint_attributes", "dview.scale", "dgrid.grid_on", "dnpart.draw_as", "drebar_mark.create")]
        [InlineData(typeof(AssemblyDrawingAttributeFile), ".ad", "adraw_dial_attributes", "dv.scale", "grid.grid_on", "dnp.draw_as", "drmark.create")]
        [InlineData(typeof(SinglePartDrawingAttributeFile), ".wd", "singledrawing_attributes", "dv.scale", "grid.grid_on", "dnp.draw_as", "drmark.create")]
        [InlineData(typeof(CastUnitDrawingAttributeFile), ".cud", "cudraw_dial_attributes", "dv.scale", "grid.grid_on", "dnp.draw_as", "drmark.create")]
        public void DrawingTypes_UseThePrefixesOfTheirFamily(Type type, string extension, string objectType, string viewKey, string gridKey, string neighborPartKey, string reinforcementMarkKey)
        {
            var file = (DrawingAttributeFile)Activator.CreateInstance(type);

            file.Views.SetInt32("scale", 1);
            file.Objects.Grid.Grid = Grid.Visible;
            file.Objects.NeighborPart.Content.Representation.Representation = NeighborPartRepresentation.Symbol;
            file.Marks.ReinforcementMark.Advanced.Create = 1;
            file.UserAttributes.DrawnBy = "NT";

            Assert.Equal(extension, file.Extension);
            Assert.Equal(extension + ".more", file.UserAttributes.Extension);
            Assert.Equal(objectType, file.UserAttributes.ObjectType);
            Assert.True(file.Values.ContainsKey(viewKey));
            Assert.True(file.Values.ContainsKey(gridKey));
            Assert.True(file.Values.ContainsKey(neighborPartKey));
            Assert.True(file.Values.ContainsKey(reinforcementMarkKey));
            Assert.True(file.UserAttributes.Values.ContainsKey(objectType + ".DR_DRAWN_BY"));
        }

        [Fact]
        public void Sections_WriteNestedKeys()
        {
            var view = new ViewProperties();

            view.Marks.PartMark.Content.Font.Height = 3.5;
            view.Marks.PartMark.Content.Font.Color = DrawingColor.Blue;
            view.Objects.Part.Fill.PartFaces.Type = "ANSI31";
            view.Objects.Bolt.Content.BoltRepresentation.Axis = true;

            Assert.Equal("3.500000", view.Values["vpm.FontAttr.height"].RawText);
            Assert.Equal("162", view.Values["vpm.FontAttr.text_colour"].RawText);
            Assert.Equal("ANSI31", view.Values["vp.PartHatch.hatch_name"].AsString());
            Assert.Equal("1", view.Values["vs.sym_axis"].RawText);
        }

        [Fact]
        public void UserAttributeFile_AdoptsTheObjectTypeOfTheLoadedFile()
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(Path.Combine(_directory, "beam.ad"), "encoding 1252\r\nin_file \"beam\"\r\n");
            File.WriteAllText(Path.Combine(_directory, "beam.ad.more"), "assemblydrawing_attributes.DR_DRAWN_BY \"NT\"\r\n");

            var drawing = AttributeFile.FromFile<AssemblyDrawingAttributeFile>(Path.Combine(_directory, "beam.ad"));

            Assert.Equal("assemblydrawing_attributes", drawing.UserAttributes.ObjectType);
            Assert.Equal("NT", drawing.UserAttributes.DrawnBy);

            drawing.UserAttributes.ObjectType = "adraw_dial_attributes";
            Assert.True(drawing.UserAttributes.Values.ContainsKey("adraw_dial_attributes.DR_DRAWN_BY"));
        }

        [Theory]
        [InlineData(typeof(AssemblyDrawingAttributeFile), "adraw_dial.")]
        [InlineData(typeof(SinglePartDrawingAttributeFile), "wdraw_dial.")]
        [InlineData(typeof(CastUnitDrawingAttributeFile), "cudraw_dial.")]
        [InlineData(typeof(MultiDrawingAttributeFile), "mdraw_dial.")]
        public void DrawingTypes_ReadLegacyFiles(Type type, string legacyPrefix)
        {
            var file = (DrawingAttributeFile)Activator.CreateInstance(type);
            Directory.CreateDirectory(_directory);
            string path = Path.Combine(_directory, "old" + file.Extension);
            File.WriteAllText(path, legacyPrefix + "title_1 \"OLD\"\r\n" + legacyPrefix + "dl.width 841.000000\r\n");

            file.Load(path);

            Assert.Equal("OLD", file.Title1);
            Assert.Equal(841, file.Layout.Width);
        }

        [Fact]
        public void GADrawingAttributeFile_SavesItsFilters()
        {
            var drawing = new GADrawingAttributeFile();
            drawing.Filter.SetExpression(FilterProperties.Part.Profile.BeginsWith("HEA"));
            drawing.NeighborPartFilter.SetExpression(FilterProperties.Part.Name.IsNotEqual("PLATE"));

            drawing.SaveTo(_directory, "plan");

            Assert.True(File.Exists(Path.Combine(_directory, "plan.gd")));
            Assert.True(File.Exists(Path.Combine(_directory, "plan.gd.more")));
            Assert.True(File.Exists(Path.Combine(_directory, "plan.gd.filter")));
            Assert.True(File.Exists(Path.Combine(_directory, "plan.gd.npfilter")));
        }

        [Fact]
        public void Section_ExportTo_WritesTheKeysWithoutPrefix_AndImportFrom_ReadsThemBack()
        {
            var view = new ViewProperties();
            view.Objects.Grid.Grid = Grid.Visible;
            view.Objects.Grid.GridLine.Color = DrawingColor.Red;

            string path = view.Objects.Grid.ExportTo(_directory, "red_grid", GridObject.ViewFileExtension);

            Assert.Equal(Path.Combine(_directory, "red_grid.vg"), path);
            Assert.Equal(
                "encoding 1252\r\ngrid_on 1\r\nline_colour 160\r\nin_file \"red_grid\"\r\nout_file \"\"\r\n",
                File.ReadAllText(path));

            var drawing = new GADrawingAttributeFile();
            drawing.Objects.Grid.ImportFrom(path);

            Assert.Equal(Grid.Visible, drawing.Objects.Grid.Grid);
            Assert.Equal(DrawingColor.Red, drawing.Objects.Grid.GridLine.Color);
            Assert.Equal("red_grid", drawing.GetString("dgrid.in_file"));
        }

        [Fact]
        public void Section_CopyFrom_CopiesBetweenHosts()
        {
            var view = new ViewProperties();
            view.Marks.PartMark.Content.Font.Font = "Arial";
            view.Marks.PartMark.Advanced.ContentString1 = "<Mark/>";
            var drawing = new AssemblyDrawingAttributeFile();

            drawing.Marks.PartMark.CopyFrom(view.Marks.PartMark);

            Assert.Equal("Arial", drawing.Marks.PartMark.Content.Font.Font);
            Assert.Equal("<Mark/>", drawing.GetString("dpm.ContentString1"));
        }
    }
}
