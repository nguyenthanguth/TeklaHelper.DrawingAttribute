using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Views;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>
    /// The expected texts are what Tekla.Structures.Filtering 2025 writes for the same expressions (see
    /// TeklaOracleTests, which compares with Tekla itself when it is installed).
    /// </summary>
    public sealed class FilterFileTests : IDisposable
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
        public void SetExpression_WritesNestedGroupsLikeTekla()
        {
            // A AND (B OR C) OR D
            var expression = new FilterGroup()
                .Add(Name("A"), FilterLogic.And)
                .Add(new FilterGroup().Add(Name("B"), FilterLogic.Or).Add(Name("C")), FilterLogic.Or)
                .Add(Name("D"));

            string text = FilterFile.FromExpression(FilterFileExtensions.ViewFilter, expression).ToText();

            Assert.Equal(
                File(
                    Row(1, 1, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "A", 0, "&&"),
                    Row(1, 1, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "B", 0, "||"),
                    Row(0, 1, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "C", 1, "||"),
                    Row(0, 1, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "D", 1, "Empty")),
                text);
        }

        [Fact]
        public void SetExpression_LastRowOfAGroupTakesTheOperatorOfItsOutermostItem()
        {
            // Tekla: (A && (B || (C || D)) || E) — D's own And and the inner group's And lose to the middle group's Or
            var expression = new FilterGroup()
                .Add(Name("A"), FilterLogic.And)
                .Add(new FilterGroup()
                    .Add(Name("B"), FilterLogic.Or)
                    .Add(new FilterGroup().Add(Name("C"), FilterLogic.Or).Add(Name("D"), FilterLogic.And), FilterLogic.And), FilterLogic.Or)
                .Add(Name("E"));

            FilterFile file = FilterFile.FromExpression(FilterFileExtensions.ViewFilter, expression);

            Assert.Equal(
                new[] { "1 A 0 And", "1 B 0 Or", "1 C 0 Or", "0 D 2 Or", "0 E 1 None" },
                file.Rows.Select(r => $"{r.OpenParentheses} {r.Value} {r.CloseParentheses} {r.Logic}"));
        }

        [Fact]
        public void SetExpression_SingleConditionHasNoParentheses_DisabledRowsAreZero()
        {
            FilterCondition condition = Name("A");
            condition.Enabled = false;

            string text = FilterFile.FromExpression(FilterFileExtensions.ViewFilter, condition).ToText();

            Assert.Equal(File(Row(0, 0, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "A", 0, "Empty")), text);
        }

        [Fact]
        public void Values_SpacesBecomeMarkers_TextIsUtf8_EmptyTextIsTheWordEmpty()
        {
            var expression = new FilterGroup()
                .Add(FilterProperties.Part.Name.IsEqual("  a  b\t\"q\" Äö 図面 x  "), FilterLogic.And)
                .Add(FilterProperties.Part.Name.IsEqual(string.Empty));

            byte[] bytes = FilterFile.FromExpression(FilterFileExtensions.ViewFilter, expression).ToBytes();
            string text = Encoding.UTF8.GetString(bytes);

            Assert.Contains("        \u0002\u0002a\u0002\u0002b\t\"q\"\u0002Äö\u0002図面\u0002x\u0002\u0002 \r\n", text);
            Assert.Contains("        Empty \r\n        1 \r\n", text);
        }

        [Fact]
        public void Values_NumbersDatesAndBooleansUseInvariantFormats()
        {
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("vi-VN");

                Assert.Equal("2.5", FilterProperties.Part.Phase.IsEqual(2.5).Value);
                Assert.Equal("10000000000", FilterProperties.Part.Phase.IsEqual(1e10).Value);
                Assert.Equal("1234567.891", FilterProperties.Part.Phase.IsEqual(1234567.891).Value);
                Assert.Equal("02/03/2001", FilterProperties.Task.ActualStartDate.IsEqual(new DateTime(2001, 2, 3, 13, 45, 30)).Value);
                Assert.Equal("True", FilterProperties.Part.CustomBoolean("MY_BOOL").IsEqual(true).Value);
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [Fact]
        public void Operators_MatchTheTokensTeklaWrites()
        {
            Assert.Equal("*NOTENDS* albl_DoesNotEnd", Describe(FilterProperties.Part.Name.NotEndsWith("A")));
            Assert.Equal("*NOTBEGINS* albl_DoesNotBeginW", Describe(FilterProperties.Part.Name.NotBeginsWith("A")));
            Assert.Equal("<= albl_LessOrEqual", Describe(FilterProperties.Part.Phase.LessOrEqual(1)));
            Assert.Equal("<= albl_EarlierOrEqual", Describe(FilterProperties.Task.PlannedEndDate.EarlierOrEqual(DateTime.Today)));
            Assert.Equal("!= albl_DoesNotEqual", Describe(FilterProperties.Part.CustomBoolean("B").IsNotEqual(true)));
        }

        [Fact]
        public void CustomProperties_UseTheAttributeNameAsPropertyAndLabel()
        {
            FilterCondition condition = FilterProperties.Assembly.CustomString("USER_FIELD_1").Contains("A");

            Assert.Equal("co_assembly", condition.Category);
            Assert.Equal("USER_FIELD_1", condition.Property);
            Assert.Equal("USER_FIELD_1", condition.PropertyLabel);
        }

        [Fact]
        public void Load_ReadsRowsAndSpaceMarkers_AndWritesTheFileBackUnchanged()
        {
            // Saved from the Tekla 2025 view filter dialog: value ABC "X Y"
            byte[] original = Encoding.ASCII.GetBytes(File(
                Row(0, 1, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "ABC\u0002\"X\u0002Y\"", 0, "&&"),
                Row(0, 1, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "ABC\u0002\"X\u0002Y\"", 0, "&&")));
            var file = new FilterFile(FilterFileExtensions.ViewFilter);

            file.Load(original);

            Assert.Equal(2, file.Rows.Count);
            Assert.Equal("ABC \"X Y\"", file.Rows[0].Value);
            Assert.Equal(FilterLogic.And, file.Rows[1].Logic);
            Assert.Equal(original, file.ToBytes());
        }

        [Fact]
        public void Load_KeepsLinesOfUnchangedTokens_AndRewritesChangedOnes()
        {
            // "Count= 1" without its trailing space, and a property label without one: hand-edited files shipped with Tekla
            string original = File(Row(0, 1, "co_part", "proPROFILE", "albl_Profile", "==", "albl_Equals", "HEA", 0, "Empty"))
                .Replace("    Count= 1 \r\n", "    Count= 1\r\n")
                .Replace("        albl_Profile \r\n", "        albl_Profile\r\n");
            var file = new FilterFile(FilterFileExtensions.ViewFilter);
            file.Load(Encoding.ASCII.GetBytes(original));

            Assert.Equal(original, file.ToText());

            file.Rows[0].Value = "IPE";
            Assert.Equal(original.Replace("        HEA \r\n", "        IPE \r\n"), file.ToText());
        }

        [Fact]
        public void Load_ReadsAnsiFilesInTheFallbackCodePage_AndKeepsWritingInIt()
        {
            Encoding shiftJis = AttributeFileFormat.GetEncoding(932);
            byte[] original = shiftJis.GetBytes(File(Row(0, 1, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "柱", 0, "Empty")));
            var file = new FilterFile(".gdf");

            file.Load(original, fallbackCodePage: 932);
            file.Rows[0].Value = "梁";

            Assert.Equal(932, file.CodePage);
            Assert.Equal(shiftJis.GetBytes(File(Row(0, 1, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "梁", 0, "Empty"))), file.ToBytes());
        }

        [Fact]
        public void Load_RejectsFilesThatAreNotFilters()
        {
            var file = new FilterFile(FilterFileExtensions.ViewFilter);
            string countMismatch = File(Row(0, 1, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "A", 0, "Empty")).Replace("Count= 1", "Count= 2");

            Assert.Throws<FormatException>(() => file.Load(Encoding.ASCII.GetBytes("encoding 1252\r\nin_file \"x\"\r\n")));
            Assert.Throws<FormatException>(() => file.Load(Encoding.ASCII.GetBytes(countMismatch)));
        }

        [Fact]
        public void ToExpression_IsTheInverseOfSetExpression()
        {
            var expression = new FilterGroup()
                .Add(new FilterGroup().Add(Name("A"), FilterLogic.Or).Add(Name("B")), FilterLogic.And)
                .Add(new FilterGroup().Add(Name("C"), FilterLogic.Or).Add(Name("D")), FilterLogic.Or);
            FilterFile file = FilterFile.FromExpression(FilterFileExtensions.ViewFilter, expression);

            FilterFile again = FilterFile.FromExpression(FilterFileExtensions.ViewFilter, file.ToExpression());

            Assert.Equal(file.ToText(), again.ToText());
        }

        [Fact]
        public void ToExpression_ReadsDialogFilesWithoutOuterParentheses()
        {
            var file = new FilterFile(FilterFileExtensions.ViewFilter);
            file.Load(Encoding.ASCII.GetBytes(File(
                Row(0, 1, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "A", 0, "||"),
                Row(1, 0, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "B", 0, "&&"),
                Row(0, 1, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "C", 1, "Empty"))));

            var group = Assert.IsType<FilterGroup>(file.ToExpression());

            Assert.Equal(2, group.Items.Count);
            Assert.Equal(FilterLogic.Or, group.Items[0].Logic);
            var nested = Assert.IsType<FilterGroup>(group.Items[1].Expression);
            Assert.False(((FilterCondition)nested.Items[0].Expression).Enabled);
            Assert.Throws<FormatException>(() =>
            {
                file.Rows[0].CloseParentheses = 1;
                file.ToExpression();
            });
        }

        [Fact]
        public void FromFile_TakesTwoPartExtensionsForCompanionFilters()
        {
            Directory.CreateDirectory(_directory);
            string path = Path.Combine(_directory, "my.view.vi.filter");
            System.IO.File.WriteAllText(path, File(Row(0, 1, "co_part", "proNAME", "albl_Name", "==", "albl_Equals", "A", 0, "Empty")));

            Assert.Equal(".vi.filter", FilterFile.FromFile(path).Extension);
        }

        [Fact]
        public void ViewProperties_SavesAndLoadsItsFilters()
        {
            var view = new ViewProperties();
            view.Attributes.Attributes1.View.Scale = 20;
            view.Filter.SetExpression(FilterProperties.Part.Name.IsEqual("BEAM"));
            view.SaveTo(_directory, "beams");

            Assert.True(System.IO.File.Exists(Path.Combine(_directory, "beams.vi.filter")));
            Assert.False(System.IO.File.Exists(Path.Combine(_directory, "beams.vi.npfilter")));

            var loaded = AttributeFile.FromFile<ViewProperties>(Path.Combine(_directory, "beams.vi"));

            Assert.Equal("BEAM", loaded.Filter.Rows.Single().Value);
            Assert.Empty(loaded.NeighborPartFilter.Rows);
        }

        private static FilterCondition Name(string value) => FilterProperties.Part.Name.IsEqual(value);

        private static string Describe(FilterCondition condition) => condition.Operator.Symbol + " " + condition.Operator.Label;

        private static string Row(int open, int enabled, string category, string property, string label, string condition, string conditionLabel, string value, int close, string logic)
        {
            var builder = new StringBuilder();
            builder.Append("    SECTION_OBJECT_GROUP \r\n    {\r\n");
            foreach (object token in new object[] { open, enabled, category, property, label, condition, conditionLabel, value, close, logic })
            {
                builder.Append("        ").Append(token).Append(" \r\n");
            }

            return builder.Append("        }\r\n").ToString();
        }

        private static string File(params string[] rows) =>
            "TITLE_OBJECT_GROUP \r\n{\r\n    Version= 1.05 \r\n    Count= " + rows.Length + " \r\n" + string.Concat(rows) + "    }\r\n";
    }
}
