using System;
using System.IO;
using System.Linq;
using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Pages;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>
    /// Dimension properties (.dim): the dialog gr_dim_attrib at the top level and nine marks under their prefixes. Key set
    /// checked against common/system/standard.dim of the Tekla 2025 environments.
    /// </summary>
    public sealed class DimensionAttributeFileTests : IDisposable
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
        public void Properties_WriteTheTopLevelKeys_AndMarksTheirPrefix()
        {
            var dimension = new DimensionAttributeFile();

            dimension.Properties.General.DimensionTypes.Straight = StraightDimensionType.Elevation;
            dimension.Properties.Appearance.Text.Height = 3.5;
            dimension.Properties.General.Placing.Place.MinimumDistance = 4;
            dimension.PrefixMark.Font.Height = 2.5;
            dimension.RightLowerTagMark.FrameAroundElements.Color = DrawingColor.Red;

            Assert.Equal(
                new[]
                {
                    "dim_type 8", "dim_text_height 3.500000", "dimpl.dim_freepl_min 4.000000", "PrefixMark.FontAttr.height 2.500000",
                    "RightLowerTagMark.FrameAttr.line_colour 160",
                },
                dimension.Values.Select(p => p.Key + " " + p.Value.ToFileText()));
            Assert.Equal(".dim", dimension.Extension);
            Assert.Equal(9, dimension.Marks.Distinct().Count());
        }

        [Fact]
        public void CreateDefault_WritesEveryKeyOfTheTable()
        {
            DimensionAttributeFile dimension = DimensionAttributeFile.CreateDefault();

            Assert.Equal(FileDefaults.Dimension.Table.Select(row => row.Key), dimension.Values.Keys);
            Assert.True(dimension.Values.ContainsKey("LeftMiddleTagMark.FontAttr.font_name"));
            Assert.True(dimension.Values.ContainsKey("dimpl.dim_placing_mode"));
            Assert.Equal("1", dimension.Values["dim_type_en"].RawText);
        }

        [Fact]
        public void Load_StripsTheLegacyDialogPrefix_AndSavesByteForByte()
        {
            Directory.CreateDirectory(_directory);
            string legacy = Path.Combine(_directory, "old.dim");
            File.WriteAllText(legacy, "dim_dial.dim_type 2\r\ndim_dial.PrefixMark.FontAttr.height 3.000000\r\n");
            var dimension = new DimensionAttributeFile();

            dimension.Load(legacy);

            Assert.Equal(StraightDimensionType.Absolute, dimension.Properties.General.DimensionTypes.Straight);
            Assert.Equal(3, dimension.PrefixMark.Font.Height);

            string modern = Path.Combine(_directory, "new.dim");
            const string content = "encoding 1252\r\nin_file \"new\"\r\nout_file \"new\"\r\ndim_type 1\r\ndim_text_height 2.500000\r\n";
            File.WriteAllText(modern, content);
            var copy = AttributeFile.FromFile<DimensionAttributeFile>(modern);
            copy.Properties.General.DimensionTypes.Straight = copy.Properties.General.DimensionTypes.Straight;
            copy.Save(modern);
            Assert.Equal(content, File.ReadAllText(modern));
        }

        [Fact]
        public void Mark_ExportsAPageFile()
        {
            Directory.CreateDirectory(_directory);
            var dimension = new DimensionAttributeFile();
            dimension.PostfixMark.Font.Font = "Arial";

            string path = dimension.PostfixMark.ExportTo(_directory, "postfix", DimensionMark.PageFileExtension);
            var other = new DimensionAttributeFile();
            other.LeftUpperTagMark.ImportFrom(path);

            Assert.EndsWith(".dimension_mark", path, StringComparison.Ordinal);
            Assert.Equal("Arial", other.LeftUpperTagMark.Font.Font);
        }
    }
}
