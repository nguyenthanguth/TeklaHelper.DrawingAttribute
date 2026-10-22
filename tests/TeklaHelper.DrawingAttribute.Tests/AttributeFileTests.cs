using System;
using System.IO;
using System.Linq;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    public sealed class AttributeFileTests : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "TeklaHelper.DrawingAttribute.Tests", Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }

        [Fact]
        public void ViewProperties_SaveTo_WritesOnlyTheViFile_LikeTheDialog()
        {
            var settings = new ViewProperties();
            settings.Attributes.Attributes1.View.Scale = 25;
            settings.Attributes.Attributes2.CutParts.Shortening.CutParts = CutParts.BothDirections;
            settings.Objects.Grid.GridLine.Color = DrawingColor.Red;

            string path = settings.SaveTo(_directory, "detail_25");

            Assert.Equal(Path.Combine(_directory, "detail_25.vi"), path);
            Assert.Equal(
                "encoding 1252\r\nview_scale 25.000000\r\ncut_on 1\r\nvg.line_colour 160\r\nin_file \"detail_25\"\r\nout_file \"detail_25\"\r\n",
                File.ReadAllText(path));

            // No .vi.more, and the filter files only when there are filter rows
            Assert.Equal(new[] { "detail_25.vi" }, Directory.GetFiles(_directory).Select(Path.GetFileName));
        }

        [Fact]
        public void ViewProperties_Load_ReadsLegacyPrefixedKeys()
        {
            Directory.CreateDirectory(_directory);
            string path = Path.Combine(_directory, "old.vi");
            File.WriteAllText(path, "view_dial.in_file \"old\"\r\nview_dial.view_scale 50.000000\r\nview_dial.vg.grid_on 1\r\n");

            var settings = AttributeFile.FromFile<ViewProperties>(path);

            Assert.Equal(50, settings.Attributes.Attributes1.View.Scale);
            Assert.Equal(Grid.Visible, settings.Objects.Grid.Grid);
            Assert.Equal(new[] { "in_file", "view_scale", "vg.grid_on" }, settings.Values.Keys);
        }

        [Fact]
        public void Load_ThenSave_KeepsKeyOrderAndUnknownKeys()
        {
            Directory.CreateDirectory(_directory);
            string path = Path.Combine(_directory, "template.gd");
            File.WriteAllText(path, "encoding 1252\r\nin_file \"template\"\r\nout_file \"template\"\r\ntitle_1 \"A\"\r\nSomeFutureKey 7\r\ndview.main_scale 50.000000\r\n");

            var settings = AttributeFile.FromFile<GADrawingAttributeFile>(path);
            settings.Title1 = "B";
            settings.Views.Scale = 100;
            settings.SaveTo(_directory, "copy");

            Assert.Equal(
                "encoding 1252\r\nin_file \"copy\"\r\nout_file \"copy\"\r\ntitle_1 \"B\"\r\nSomeFutureKey 7\r\ndview.main_scale 100.000000\r\n",
                File.ReadAllText(Path.Combine(_directory, "copy.gd")));
        }

        [Fact]
        public void GADrawingUserAttributeFile_StoresDatesAsUnixSeconds()
        {
            var settings = new GADrawingAttributeFile();
            settings.UserAttributes.CheckedDate = new DateTime(2026, 10, 21, 0, 0, 0, DateTimeKind.Utc);
            settings.UserAttributes.ApprovalDate = null;

            Assert.Equal(1792540800, settings.UserAttributes.GetInt32("DR_CHECKED_DATE"));
            Assert.Equal(int.MinValue, settings.UserAttributes.GetInt32("DR_APPROVAL_DATE"));
            Assert.Equal(new DateTime(2026, 10, 21, 0, 0, 0, DateTimeKind.Utc), settings.UserAttributes.CheckedDate);
            Assert.Null(settings.UserAttributes.ApprovalDate);
        }

        [Fact]
        public void Sections_ReuseTheSameKeysUnderDifferentPrefixes()
        {
            var drawing = new GADrawingAttributeFile();
            var view = new ViewProperties();

            drawing.Objects.Grid.Text.Height = 3.5;
            view.Objects.Grid.Text.Height = 3.5;

            Assert.True(drawing.Values.ContainsKey("dgrid.text_height"));
            Assert.True(view.Values.ContainsKey("vg.text_height"));
        }

        [Fact]
        public void GetSection_GivesAccessToKeysWithoutTypedProperties()
        {
            var drawing = new GADrawingAttributeFile();

            drawing.GetSection("dpm.").SetInt32("text_colour", 160);

            Assert.Equal(160, drawing.GetInt32("dpm.text_colour"));
        }

        [Fact]
        public void SaveTo_RejectsInvalidNames()
        {
            var settings = new ViewProperties();

            Assert.Throws<ArgumentException>(() => settings.SaveTo(_directory, "a/b"));
            Assert.Throws<ArgumentException>(() => settings.SaveTo(_directory, " "));
        }
    }
}
