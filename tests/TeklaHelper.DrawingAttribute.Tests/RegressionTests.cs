using System;
using System.IO;
using System.Linq;
using System.Text;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>
    /// Cases found in review: each test reproduces one defect that was fixed.
    /// </summary>
    public sealed class RegressionTests : IDisposable
    {
        private const string OneRow =
            "TITLE_OBJECT_GROUP \r\n{\r\n    Version= 1.05 \r\n    Count= 1 \r\n    SECTION_OBJECT_GROUP \r\n    {\r\n" +
            "        0 \r\n        1 \r\n        co_part \r\n        proNAME \r\n        albl_Name \r\n        == \r\n" +
            "        albl_Equals \r\n        A \r\n        0 \r\n        Empty \r\n        }\r\n    }\r\n";

        private readonly string _directory = Path.Combine(Path.GetTempPath(), "TeklaHelper.DrawingAttribute.Tests", Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }

        [Fact]
        public void LineBreaksInFilterTokens_AreRejected()
        {
            // Written as is, the line break split the token and the file could not be read back
            Assert.Throws<ArgumentException>(() => FilterProperties.Part.Name.IsEqual("A\r\nB"));
            Assert.Throws<ArgumentException>(() => new FilterRow().Value = "A\nB");
            Assert.Throws<ArgumentException>(() => new FilterOperator("==", "a\rb"));
        }

        [Fact]
        public void SavingWithoutFilter_ReplacesAnOldFilterFile()
        {
            var first = new ViewProperties();
            first.Filter.SetExpression(FilterProperties.Part.Name.IsEqual("OLD"));
            first.SaveTo(_directory, "x");

            new ViewProperties().SaveTo(_directory, "x");

            // Not an empty filter file: Tekla shows no objects at all with one
            Assert.False(File.Exists(Path.Combine(_directory, "x.vi.filter")));
            Assert.Empty(AttributeFile.FromFile<ViewProperties>(Path.Combine(_directory, "x.vi")).Filter.Rows);
        }

        [Fact]
        public void SetBoolean_KeepsANonZeroValueThatReadsTrue()
        {
            Directory.CreateDirectory(_directory);
            string path = Path.Combine(_directory, "a.ad");
            File.WriteAllText(path, "encoding 1252\r\ndv.cre_front_view 2\r\ndv.cre_top_view 2\r\n");
            var drawing = AttributeFile.FromFile<AssemblyDrawingAttributeFile>(path);

            drawing.Views.CreateFrontView = drawing.Views.CreateFrontView;
            drawing.Views.CreateTopView = false;

            Assert.Equal("2", drawing.Values["dv.cre_front_view"].RawText);
            Assert.Equal("0", drawing.Values["dv.cre_top_view"].RawText);
        }

        [Fact]
        public void ExportTo_WritesTheFileNameKeysOfThePage_LikeTheDialog()
        {
            var view = new ViewProperties();
            view.Objects.ReferenceObject.Appearance.VisibleLines.Color = DrawingColor.Red;

            string path = view.Objects.ReferenceObject.ExportTo(_directory, "x", ReferenceObject.ViewFileExtension);

            Assert.Equal("encoding 1252\r\nColour 160\r\naInFile \"x\"\r\naOutFile \"\"\r\n", File.ReadAllText(path));
            Assert.DoesNotContain(view.Values.Keys, key => key.EndsWith("in_file", StringComparison.Ordinal));
        }

        [Fact]
        public void KeyWithoutValue_IsWrittenBackWithoutASpace()
        {
            Directory.CreateDirectory(_directory);
            string path = Path.Combine(_directory, "k.vi");
            byte[] bytes = Encoding.ASCII.GetBytes("encoding 1252\r\nsome_flag\r\nview_scale 20.000000\r\n");
            File.WriteAllBytes(path, bytes);

            Assert.Equal(bytes, AttributeFile.FromFile<ViewProperties>(path).ToBytes());
        }

        [Fact]
        public void UserAttributes_WithMixedObjectTypes_FallBackToTheDefaultObjectType()
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(Path.Combine(_directory, "a.ad"), "encoding 1252\r\n");
            File.WriteAllText(Path.Combine(_directory, "a.ad.more"), "assemblydrawing_attributes.DR_DRAWN_BY \"A\"\r\n");
            File.WriteAllText(Path.Combine(_directory, "b.ad"), "encoding 1252\r\n");
            File.WriteAllText(Path.Combine(_directory, "b.ad.more"), "adraw_dial_attributes.DR_DRAWN_BY \"B\"\r\nother.X 1\r\n");
            var drawing = AttributeFile.FromFile<AssemblyDrawingAttributeFile>(Path.Combine(_directory, "a.ad"));
            Assert.Equal("assemblydrawing_attributes", drawing.UserAttributes.ObjectType);

            drawing.Load(Path.Combine(_directory, "b.ad"));

            Assert.Equal("adraw_dial_attributes", drawing.UserAttributes.ObjectType);
            Assert.Equal("B", drawing.UserAttributes.DrawnBy);
        }

        [Fact]
        public void SaveTo_WithACompanionThatCannotBeEncoded_WritesNothingAndChangesNothing()
        {
            var drawing = new GADrawingAttributeFile { Title1 = "OLD" };
            drawing.SaveTo(_directory, "plan");
            byte[] before = File.ReadAllBytes(Path.Combine(_directory, "plan.gd"));

            drawing.Title1 = "NEW";
            drawing.UserAttributes.DrawnBy = "ＮＴ";   // not in code page 1252
            Assert.Throws<EncoderFallbackException>(() => drawing.SaveTo(_directory, "other"));

            Assert.False(File.Exists(Path.Combine(_directory, "other.gd")));
            Assert.Equal(before, File.ReadAllBytes(Path.Combine(_directory, "plan.gd")));
            Assert.Equal("plan", drawing.GetString("in_file"));
        }

        [Fact]
        public void ExportTo_UsesTheCodePageOfTheFile()
        {
            var view = new ViewProperties { CodePage = 932 };
            view.Marks.PartMark.Content.Font.Font = "ＭＳ ゴシック";

            string path = view.Marks.PartMark.ExportTo(_directory, "m", PartMark.ViewFileExtension);

            Assert.StartsWith("encoding 932", File.ReadAllText(path));
            var drawing = new GADrawingAttributeFile { CodePage = 932 };
            drawing.Marks.PartMark.ImportFrom(path);
            Assert.Equal("ＭＳ ゴシック", drawing.Marks.PartMark.Content.Font.Font);
        }

        [Fact]
        public void Load_WithABrokenCompanion_ChangesNothing()
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(Path.Combine(_directory, "a.vi"), "encoding 1252\r\nview_scale 20.000000\r\n");
            File.WriteAllText(Path.Combine(_directory, "a.vi.filter"), OneRow);
            File.WriteAllText(Path.Combine(_directory, "a.vi.npfilter"), "TITLE_OBJECT_GROUP \r\n{\r\n    Version= 1.05 \r\n");
            var view = new ViewProperties();
            view.Attributes.Attributes1.View.Scale = 5;
            view.Filter.SetExpression(FilterProperties.Part.Name.IsEqual("OLD"));
            view.NeighborPartFilter.SetExpression(FilterProperties.Part.Name.IsEqual("OLD NEIGHBOR"));

            Assert.Throws<FormatException>(() => view.Load(Path.Combine(_directory, "a.vi")));

            // a.vi and a.vi.filter could be read, but nothing is applied because a.vi.npfilter is broken
            Assert.Equal(5, view.Attributes.Attributes1.View.Scale);
            Assert.Equal("OLD", view.Filter.Rows.Single().Value);
            Assert.Equal("OLD NEIGHBOR", view.NeighborPartFilter.Rows.Single().Value);
        }

        [Fact]
        public void ObjectType_RenameThatWouldOverwriteAKey_Throws()
        {
            var file = new DrawingUserAttributeFile(".ad.more", "adraw_dial_attributes");
            file.SetString("X", "1");
            file.Values.Set("assemblydrawing_attributes.X", AttributeValue.FromString("2"));

            Assert.Throws<InvalidOperationException>(() => file.ObjectType = "assemblydrawing_attributes");
            Assert.Equal("1", file.GetString("X"));
        }

        [Fact]
        public void ObjectType_IsResetWhenTheNextFileHasNoMoreFile()
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(Path.Combine(_directory, "a.ad"), "encoding 1252\r\n");
            File.WriteAllText(Path.Combine(_directory, "a.ad.more"), "assemblydrawing_attributes.DR_DRAWN_BY \"NT\"\r\n");
            File.WriteAllText(Path.Combine(_directory, "b.ad"), "encoding 1252\r\n");
            var drawing = new AssemblyDrawingAttributeFile();

            drawing.Load(Path.Combine(_directory, "a.ad"));
            Assert.Equal("assemblydrawing_attributes", drawing.UserAttributes.ObjectType);

            drawing.Load(Path.Combine(_directory, "b.ad"));
            Assert.Equal("adraw_dial_attributes", drawing.UserAttributes.ObjectType);
        }

        [Fact]
        public void MalformedFilters_ThrowFormatException()
        {
            var file = new FilterFile(FilterFileExtensions.ViewFilter);

            Assert.Throws<FormatException>(() => file.Load(Encoding.ASCII.GetBytes(OneRow.Replace("Count= 1", "Count= 99999999999"))));
            Assert.Throws<FormatException>(() => file.Load(new byte[] { 0x54, 0xC3 }, fallbackCodePage: FilterFile.Utf8CodePage));

            file.Rows.Add(new FilterRow());
            Assert.Throws<FormatException>(() => file.ToExpression());
        }

        [Fact]
        public void ByteOrderMark_IsReadAndWrittenBack()
        {
            byte[] original = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Encoding.ASCII.GetBytes(OneRow)).ToArray();
            var file = new FilterFile(FilterFileExtensions.ViewFilter);

            file.Load(original);

            Assert.Equal("A", file.Rows.Single().Value);
            Assert.Equal(original, file.ToBytes());
        }

        [Fact]
        public void TrailingEmptyGroup_DoesNotLeaveADanglingOperator()
        {
            FilterFile file = FilterFile.FromExpression(
                FilterFileExtensions.ViewFilter,
                new FilterGroup().Add(FilterProperties.Part.Name.IsEqual("A"), FilterLogic.Or).Add(new FilterGroup()));

            Assert.Equal(FilterLogic.None, file.Rows.Single().Logic);
        }

        [Fact]
        public void UnclosedGroup_KeepsTheOperatorOfItsLastRow()
        {
            var file = new FilterFile(FilterFileExtensions.ViewFilter);
            file.Rows.Add(new FilterRow(FilterProperties.Part.Name.IsEqual("A"), FilterLogic.And) { OpenParentheses = 1 });
            file.Rows.Add(new FilterRow(FilterProperties.Part.Name.IsEqual("B"), FilterLogic.Or));

            var group = Assert.IsType<FilterGroup>(file.ToExpression());

            Assert.Equal(FilterLogic.Or, group.Items.Last().Logic);
        }

        [Fact]
        public void SetDate_OutsideTheStoredRange_ThrowsArgumentOutOfRange()
        {
            var file = new DrawingUserAttributeFile(".gd.more", "gadrawing_attributes");

            Assert.Throws<ArgumentOutOfRangeException>(() => file.CheckedDate = new DateTime(2040, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Throws<ArgumentOutOfRangeException>(() => file.CheckedDate = new DateTime(1901, 12, 13, 20, 45, 52, DateTimeKind.Utc));
        }

        [Fact]
        public void NegativeZero_IsWrittenAsZero()
        {
            Assert.Equal("0", FilterProperties.Part.Phase.IsEqual(-0.0).Value);
        }
    }
}
