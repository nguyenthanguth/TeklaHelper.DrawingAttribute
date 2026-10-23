using System;
using System.Collections.Generic;
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

            drawing.Views.SetBoolean("cre_front_view", drawing.Views.GetBoolean("cre_front_view"));
            drawing.Views.SetBoolean("cre_top_view", false);

            Assert.Equal("2", drawing.Values["dv.cre_front_view"].RawText);
            Assert.Equal("0", drawing.Values["dv.cre_top_view"].RawText);
        }

        [Fact]
        public void ViewCreation_ReadsOffAutoAndOn_FromTheViewTable()
        {
            var drawing = new AssemblyDrawingAttributeFile();
            drawing.SetString("dv.aDrawingViews", "1,2,,STL_FRONT,2,1,,STL_TOP,4,0,,STL_BOTTOM,7,1,,STL_SECTION,16,0,,STL_3D,");
            drawing.SetInt32("dv.cre_front_view", 0);
            drawing.SetInt32("dv.cre_back_view", 2);

            // The table wins where it has a row (Tekla follows it); the switch is read where it has none
            Assert.Equal(ViewCreation.On, drawing.Views.CreateFrontView);
            Assert.Equal(ViewCreation.Auto, drawing.Views.CreateTopView);
            Assert.Equal(ViewCreation.On, drawing.Views.CreateBackView);
            Assert.False(drawing.Views.Create3dView);

            IList<DrawingViewTableRow> table = drawing.Views.GetViewTable();
            Assert.Equal(new[] { DrawingViewType.Front, DrawingViewType.Top, DrawingViewType.Bottom, DrawingViewType.Section, DrawingViewType.ThreeD }, table.Select(r => r.ViewType));
            Assert.Equal("STL_TOP", table[1].ViewAttributeFile);

            drawing.Views.SetViewTable(table);
            Assert.Equal("1,2,,STL_FRONT,2,1,,STL_TOP,4,0,,STL_BOTTOM,7,1,,STL_SECTION,16,0,,STL_3D,", drawing.GetString("dv.aDrawingViews"));
        }

        [Fact]
        public void ViewCreation_SettingASwitch_WritesTheTableRowAndTheKey()
        {
            var drawing = new AssemblyDrawingAttributeFile();
            drawing.SetString("dv.aDrawingViews", "1,2,,,2,1,,,4,1,,,16,0,,standard,");

            drawing.Views.CreateTopView = ViewCreation.On;
            drawing.Views.CreateBottomView = ViewCreation.Off;
            drawing.Views.Create3dView = true;

            Assert.Equal("1,2,,,2,2,,,4,0,,,16,2,,standard,", drawing.GetString("dv.aDrawingViews"));
            Assert.Equal("2", drawing.Values["dv.cre_top_view"].RawText);
            Assert.Equal("0", drawing.Values["dv.cre_bottom_view"].RawText);
            Assert.Equal("2", drawing.Values["dv.Create3dView"].RawText);
        }

        [Fact]
        public void ViewCreation_AssigningItsOwnValue_ChangesNoByte_WhenTheSwitchAndTheTableDisagree()
        {
            var drawing = new AssemblyDrawingAttributeFile();
            drawing.SetString("dv.aDrawingViews", "2,1,,,4,1,,,16,1,,,");
            drawing.SetInt32("dv.cre_top_view", 0);
            drawing.SetInt32("dv.Create3dView", 0);
            byte[] before = drawing.ToBytes();

            drawing.Views.CreateTopView = drawing.Views.CreateTopView;
            drawing.Views.CreateBottomView = drawing.Views.CreateBottomView;
            drawing.Views.Create3dView = drawing.Views.Create3dView;

            Assert.Equal(before, drawing.ToBytes());
        }

        [Fact]
        public void ViewTable_SetViewAttributeFile_NamesTheFileForEveryViewAndTheThreeDView()
        {
            var drawing = new AssemblyDrawingAttributeFile();
            Assert.Throws<InvalidOperationException>(() => drawing.Views.SetViewAttributeFile("my_view"));

            drawing.SetString("dv.aDrawingViews", "1,2,,STL_FRONT,16,0,,STL_3D,");
            drawing.Views.SetViewAttributeFile("my_view");

            Assert.Equal("1,2,,my_view,16,0,,my_view,", drawing.GetString("dv.aDrawingViews"));
            Assert.Equal("my_view", drawing.Views.View3dAttributeFile);
            Assert.Throws<ArgumentException>(() => new DrawingViewTableRow(DrawingViewType.Front, ViewCreation.On, "A,B"));
        }

        [Fact]
        public void ViewCreation_TableWithoutTheType_GetsARow_AndRepeatedTypesAllChange()
        {
            var drawing = new AssemblyDrawingAttributeFile();
            drawing.SetString("dv.aDrawingViews", "1,2,,standard,");

            drawing.Views.CreateEndViews = ViewCreation.On;

            Assert.Equal("1,2,,standard,5,2,,,", drawing.GetString("dv.aDrawingViews"));
            Assert.Equal("2", drawing.Values["dv.cre_end_views"].RawText);

            // A shipped Korean table repeats the front view: the highest value is read, and a change sets every row
            drawing.SetString("dv.aDrawingViews", "1,2,F2,F_PLAN,1,1,C2,standard,");
            Assert.Equal(ViewCreation.On, drawing.Views.CreateFrontView);
            drawing.Views.CreateFrontView = ViewCreation.On;
            Assert.Equal("1,2,F2,F_PLAN,1,1,C2,standard,", drawing.GetString("dv.aDrawingViews"));
            drawing.Views.CreateFrontView = ViewCreation.Off;
            Assert.Equal("1,0,F2,F_PLAN,1,0,C2,standard,", drawing.GetString("dv.aDrawingViews"));

            Assert.Throws<ArgumentNullException>(() => drawing.Views.SetViewAttributeFile(null));
            Assert.Throws<ArgumentException>(() => new DrawingViewTableRow(DrawingViewType.Front, ViewCreation.On, "A\nB"));
        }

        [Fact]
        public void ViewTable_ShippedForms_AreKeptWhenNothingChanges()
        {
            // standard.cud: two section rows (Auto, On) with cre_cuts 2
            var castUnit = new CastUnitDrawingAttributeFile();
            castUnit.SetString("dv.aDrawingViews", "1,2,,a,7,1,,cu_section_formwork,7,2,,cu_section_reinforcement,");
            castUnit.SetInt32("dv.cre_cuts", 2);
            castUnit.SetInt32("dv.Create3dView", 1);
            byte[] before = castUnit.ToBytes();
            castUnit.Views.CreateSectionViews = castUnit.Views.CreateSectionViews;
            castUnit.Views.Create3dView = castUnit.Views.Create3dView;
            Assert.Equal(ViewCreation.On, castUnit.Views.CreateSectionViews);
            Assert.Equal(before, castUnit.ToBytes());

            // A few shipped tables end without a comma
            var singlePart = new SinglePartDrawingAttributeFile();
            singlePart.SetString("dv.aDrawingViews", "1,1,,,16,0,,single_3D_view");
            singlePart.Views.SetViewTable(singlePart.Views.GetViewTable());
            Assert.Equal("1,1,,,16,0,,single_3D_view", singlePart.GetString("dv.aDrawingViews"));
            singlePart.Views.CreateFrontView = ViewCreation.On;
            Assert.Equal("1,2,,,16,0,,single_3D_view", singlePart.GetString("dv.aDrawingViews"));

            // A label with a quote cannot be a row: the switches fall back to their keys
            var assembly = new AssemblyDrawingAttributeFile();
            assembly.SetString("dv.aDrawingViews", "1,2,5\" BEAM,x,");
            assembly.SetInt32("dv.cre_front_view", 1);
            Assert.Throws<FormatException>(() => assembly.Views.GetViewTable());
            Assert.Equal(ViewCreation.Auto, assembly.Views.CreateFrontView);
        }

        [Fact]
        public void Save_ACharacterTheCodePageCannotStore_NamesTheKey()
        {
            GADrawingAttributeFile drawing = GADrawingAttributeFile.CreateDefault();
            drawing.Title1 = "図面";

            EncoderFallbackException exception = Assert.Throws<EncoderFallbackException>(() => drawing.SaveTo(_directory, "jp"));

            Assert.Contains("title_1", exception.Message);
            drawing.CodePage = 932;
            Assert.True(File.Exists(drawing.SaveTo(_directory, "jp")));
        }

        [Fact]
        public void ViewTable_Unreadable_ThrowsAndLeavesTheSwitchesToTheKeys()
        {
            var drawing = new AssemblyDrawingAttributeFile();
            drawing.SetString("dv.aDrawingViews", "1,x,,STL_FRONT,");
            drawing.SetInt32("dv.cre_front_view", 1);

            Assert.Throws<FormatException>(() => drawing.Views.GetViewTable());
            Assert.Equal(ViewCreation.Auto, drawing.Views.CreateFrontView);

            drawing.SetString("dv.aDrawingViews", "1,2,STL_FRONT,");
            Assert.Throws<FormatException>(() => drawing.Views.GetViewTable());
        }

        [Fact]
        public void CreateDefault_ViewSwitches_AgreeWithTheViewTable()
        {
            foreach (DrawingAttributeFile drawing in new DrawingAttributeFile[] { AssemblyDrawingAttributeFile.CreateDefault(), SinglePartDrawingAttributeFile.CreateDefault(), CastUnitDrawingAttributeFile.CreateDefault() })
            {
                IList<DrawingViewTableRow> table = drawing.Views.GetViewTable();
                Assert.NotEmpty(table);
                var keys = new Dictionary<DrawingViewType, string>
                {
                    [DrawingViewType.Front] = "cre_front_view", [DrawingViewType.Top] = "cre_top_view", [DrawingViewType.Back] = "cre_back_view",
                    [DrawingViewType.Bottom] = "cre_bottom_view", [DrawingViewType.Section] = "cre_cuts", [DrawingViewType.End] = "cre_end_views",
                };
                foreach (DrawingViewTableRow row in table.Where(r => keys.ContainsKey(r.ViewType)))
                {
                    Assert.Equal((int)row.Creation, drawing.Views.GetInt32(keys[row.ViewType]));
                }
            }
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
