using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>
    /// <c>SetDefaults()</c> of the pages and <c>CreateDefault()</c> of the files.
    /// </summary>
    public sealed class DefaultsTests : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "TeklaHelper.DrawingAttribute.Tests", Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }

        public static IEnumerable<object[]> Files()
        {
            yield return new object[] { new Func<AttributeFile>(ViewProperties.CreateDefault), "VIEW-TEST.vi" };
            yield return new object[] { new Func<AttributeFile>(GADrawingAttributeFile.CreateDefault), "DRAWING-TEST.gd" };
            yield return new object[] { new Func<AttributeFile>(MultiDrawingAttributeFile.CreateDefault), "DRAWING-TEST.gd" };
            yield return new object[] { new Func<AttributeFile>(AssemblyDrawingAttributeFile.CreateDefault), "test-ad.ad" };
            yield return new object[] { new Func<AttributeFile>(SinglePartDrawingAttributeFile.CreateDefault), "test-ad.ad" };
            yield return new object[] { new Func<AttributeFile>(CastUnitDrawingAttributeFile.CreateDefault), "test-ad.ad" };
        }

        [Fact]
        public void SetDefaults_WritesEveryKeyOfThePage_AndTicksEveryCheckBox()
        {
            var view = new ViewProperties();

            view.Objects.Part.SetDefaults();

            string[] keys = Strings(typeof(PartObject), "Keys");
            string[] enableKeys = Strings(typeof(PartObject), "EnableKeys");
            Assert.Equal(keys.Length + enableKeys.Length, view.Values.Count);
            Assert.All(keys, key => Assert.True(view.Values.ContainsKey("vp." + key), key));
            Assert.All(enableKeys, key => Assert.Equal("1", view.Values["vp." + key].RawText));
        }

        [Fact]
        public void SetDefaults_OnAnyPrefix_KeepsTheSameValues()
        {
            var view = new ViewProperties();
            var drawing = new AssemblyDrawingAttributeFile();

            view.Marks.SetDefaults();
            view.Objects.SetDefaults();
            drawing.Marks.SetDefaults();
            drawing.Objects.SetDefaults();

            AttributeSection[] viewPages = Pages(view).ToArray();
            foreach (AttributeSection drawingPage in Pages(drawing))
            {
                AttributeSection viewPage = viewPages.Single(p => p.GetType() == drawingPage.GetType());
                Assert.Equal(
                    viewPage.GetNames().Select(name => name + " " + viewPage.Values[viewPage.GetKey(name)].ToFileText()),
                    drawingPage.GetNames().Select(name => name + " " + drawingPage.Values[drawingPage.GetKey(name)].ToFileText()));
            }
        }

        [Theory]
        [MemberData(nameof(Files))]
        public void CreateDefault_EveryPageKeyIsWrittenByItsPage(Func<AttributeFile> create, string reference)
        {
            AttributeFile file = create();
            AttributeFile fresh = (AttributeFile)Activator.CreateInstance(file.GetType());
            AttributeSection[] freshPages = Pages(fresh).ToArray();
            AttributeSection[] pages = Pages(file).ToArray();

            for (int i = 0; i < pages.Length; i++)
            {
                freshPages[i].GetType().GetMethod("SetDefaults").Invoke(freshPages[i], null);
                foreach (string name in freshPages[i].GetNames())
                {
                    // The setup of the library changes values, never their kind; a placeholder left by the table would be text
                    Assert.True(
                        freshPages[i].Values[freshPages[i].GetKey(name)].Kind == pages[i].Values[pages[i].GetKey(name)].Kind,
                        $"{reference}: {pages[i].GetKey(name)} = {pages[i].Values[pages[i].GetKey(name)].ToFileText()}");
                }
            }
        }

        [Theory]
        [MemberData(nameof(Files))]
        public void CreateDefault_ValuesNeedNoEnvironmentResource_AndAreValid(Func<AttributeFile> create, string reference)
        {
            AttributeFile file = create();

            // The field switches of the old filter dialog stay off, as in every shipped file; only objects_enable and
            // objects_in_joint_enable are 1 there (and in the files saved from the dialog)
            Assert.All(file.Values.Where(p => p.Key.Contains("filter.") && p.Key.EndsWith("_enable", StringComparison.Ordinal)
                    && !p.Key.EndsWith(".objects_enable", StringComparison.Ordinal) && !p.Key.EndsWith(".objects_in_joint_enable", StringComparison.Ordinal)),
                pair => Assert.Equal("0", pair.Value.RawText));

            // Every unquoted value is a finite number (one shipped .md file holds "inf")
            Assert.All(file.Values.Where(p => p.Value.Kind != AttributeValueKind.String),
                pair => Assert.True(double.TryParse(pair.Value.RawText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double number) && !double.IsInfinity(number), reference + ": " + pair.Key + " " + pair.Value.RawText));

            if (file is DrawingAttributeFile drawing)
            {
                Assert.Contains(drawing.Layout.LayoutName, new[] { "ga", "multi", "assembly", "single", "cu" });
                Assert.Equal(string.Empty, drawing.Title1);
                Assert.Equal(string.Empty, drawing.Description);
            }

            Assert.Equal("standard", file.GetString("StyleName"));
        }

        [Theory]
        [MemberData(nameof(Files))]
        public void CreateDefault_LineTypesAndTexts_FollowTheFilesTeklaShips(Func<AttributeFile> create, string reference)
        {
            AttributeFile file = create();
            PartObject part = file is ViewProperties view ? view.Objects.Part : ((DrawingAttributeFile)file).Objects.Part;
            MarkPages marks = file is ViewProperties v ? v.Marks : ((DrawingAttributeFile)file).Marks;

            // Every shipped file with these keys holds a dash-dot center line and no solid own hidden or reference lines
            Assert.Equal(DrawingLineType.DashDot, part.Appearance.CenterLine.Type);
            Assert.NotEqual(DrawingLineType.Solid, part.Appearance.OwnHiddenLines.Type);
            Assert.NotEqual(DrawingLineType.Solid, part.Appearance.ReferenceLines.Type);

            // Mark texts have a font and a height (many shipped files leave the reinforcement marks empty)
            foreach (AttributeSection mark in new AttributeSection[] { marks.ReinforcementMark, marks.NeighborReinforcementMark, marks.PartMark, marks.BoltMark })
            {
                Assert.NotEqual(string.Empty, mark.GetString("FontAttr.font_name"));
                Assert.NotEqual(0, mark.GetDouble("FontAttr.height"));
            }

            Assert.True(part.Content.CenterLine.MainPartBeam, reference);
        }

        [Fact]
        public void MissingKey_ReadsWhatSetDefaultsWrites_ForANewTekla2026Key()
        {
            var view = new ViewProperties();
            DrawingColor missing = view.Objects.Reinforcement.BarAppearance.VisibleLines.FillColor;

            view.Objects.Reinforcement.SetDefaults();

            Assert.Equal(missing, view.Objects.Reinforcement.BarAppearance.VisibleLines.FillColor);
        }

        [Fact]
        public void CreateDefault_AssemblyFamily_SharesTheDimensioningOfTheDialog()
        {
            foreach (AttributeFile file in new AttributeFile[] { AssemblyDrawingAttributeFile.CreateDefault(), SinglePartDrawingAttributeFile.CreateDefault(), CastUnitDrawingAttributeFile.CreateDefault() })
            {
                Assert.True(file.Values.ContainsKey("dcd.RebarDimensionMarkSettings"), file.GetType().Name);

                // The shipped tables name view files only some environments have: the defaults name none
                var drawing = (DrawingAttributeFile)file;
                Assert.NotEmpty(drawing.Views.GetViewTable());
                Assert.All(drawing.Views.GetViewTable(), row => Assert.Equal(string.Empty, row.ViewAttributeFile));
                Assert.Equal(string.Empty, drawing.Views.View3dAttributeFile);
                Assert.DoesNotContain(file.Values.Keys, key => key.StartsWith("dcd.RebarDimensionMark.", StringComparison.Ordinal));
            }
        }

        [Theory]
        [MemberData(nameof(Files))]
        public void CreateDefault_HasEveryKeyOfEveryPage_AndSavesAndReloadsUnchanged(Func<AttributeFile> create, string reference)
        {
            AttributeFile file = create();

            Assert.True(file.Values.Count > 2000, file.Values.Count + " keys");
            foreach (AttributeSection page in Pages(file))
            {
                foreach (string key in Strings(page.GetType(), "Keys").Concat(Strings(page.GetType(), "EnableKeys")))
                {
                    Assert.True(file.Values.ContainsKey(page.Prefix + key), $"{reference}: {page.Prefix}{key}");
                }
            }

            // Every value is ASCII, so the default code page 1252 can save it
            string path = file.SaveTo(_directory, "defaults");
            var reloaded = (AttributeFile)Activator.CreateInstance(file.GetType());
            reloaded.Load(path);
            Assert.Equal(File.ReadAllBytes(path), reloaded.ToBytes());

            // No filter rows: no filter file, so the view shows every object
            Assert.Empty(Directory.GetFiles(_directory, "*filter"));
        }

        [Fact]
        public void CreateDefault_View_HasTheSetupOfTheLibrary()
        {
            ViewProperties view = ViewProperties.CreateDefault();

            Assert.Equal(20, view.Attributes.Attributes1.View.Scale);
            Assert.Equal(ViewSize.FitByParts, view.Attributes.Attributes1.View.Size);
            Assert.Equal(CutParts.No, view.Attributes.Attributes2.CutParts.Shortening.CutParts);
            Assert.Equal(Representation.Exact, view.Objects.Part.Content.Representation.Representation);
            Assert.True(view.Objects.Part.Content.HiddenLines.HiddenLines);
            Assert.Equal(DrawingColor.Black, view.Objects.Part.Appearance.VisibleLines.Color);
            Assert.Equal(DrawingLineType.Dashed, view.Objects.Part.Appearance.HiddenLines.Type);
            Assert.Equal(Grid.Visible, view.Objects.Grid.Grid);
            Assert.Equal(WeldVisibility.BothVisible, view.Objects.Weld.Content.Visibility.Welds);
            Assert.Equal(NpCollectBy.ConnectedParts, view.Objects.NeighborPart.Visibility.NeighborParts);
            Assert.Equal(VisibilityInView.Always, view.Marks.PartMark.General.VisibilityOfMark.VisibilityInView);
            Assert.Contains("ASSEMBLY_POS", view.Marks.PartMark.Advanced.ContentString1);
            Assert.Equal(VisibilityInView.None, view.Marks.BoltMark.General.VisibilityOfBoltMarks.VisibilityInView);
            Assert.True(view.Objects.SurfaceTreatment.Content.Visibility);
            Assert.Equal(DrawingColor.Black, view.Attributes.Label.ViewLabel.Symbol.Color);
            Assert.Empty(view.Filter.Rows);
        }

        [Fact]
        public void CreateDefault_Drawing_HasTheSetupAndTheEmptyUserAttributes()
        {
            GADrawingAttributeFile drawing = GADrawingAttributeFile.CreateDefault();

            Assert.Equal(Representation.Exact, drawing.Objects.Part.Content.Representation.Representation);
            Assert.True(drawing.Values.ContainsKey("dp.draw_as"));
            Assert.Equal(17, drawing.UserAttributes.Values.Count);
            Assert.Equal("gadrawing_attributes.comment", drawing.UserAttributes.Values.Keys.First());
            Assert.Null(drawing.UserAttributes.CheckedDate);
        }

        [Fact]
        public void CreateDefault_ReturnsANewFileEachTime()
        {
            ViewProperties first = ViewProperties.CreateDefault();
            first.Attributes.Attributes1.View.Scale = 99;

            Assert.Equal(20, ViewProperties.CreateDefault().Attributes.Attributes1.View.Scale);
        }

        /// <summary>
        /// TEKLA_VIEW_DIALOG_SAMPLES: every key of a file saved from the dialog (VIEW-TEST.vi, DRAWING-TEST.gd, test-ad.ad)
        /// is in the file CreateDefault makes, so that Tekla takes no value from its standard file. Passes without checking
        /// anything when the variable is not set.
        /// </summary>
        [Theory]
        [MemberData(nameof(Files))]
        public void CreateDefault_HasEveryKeyOfAFileSavedFromTheDialog(Func<AttributeFile> create, string reference)
        {
            string root = Environment.GetEnvironmentVariable("TEKLA_VIEW_DIALOG_SAMPLES");
            string path = root == null ? null : Path.Combine(root, reference);
            if (path == null || !File.Exists(path))
            {
                return;
            }

            AttributeFile file = create();
            var missing = AttributeFileFormat.Read(path).Values.Keys
                .Where(key => key != "in_file" && key != "out_file" && !file.Values.ContainsKey(key))
                .ToList();

            Assert.True(missing.Count == 0, string.Join(", ", missing.Take(20)));
        }

        private static IEnumerable<AttributeSection> Pages(AttributeFile file)
        {
            var roots = new List<object>();
            if (file is ViewProperties view)
            {
                roots.AddRange(new object[] { view.Attributes, view.Attributes.LabelMarkA1, view.Attributes.LabelMarkA5, view.Protection, view.Dimensioning, view.Marks, view.Objects });
            }
            else if (file is DrawingAttributeFile drawing)
            {
                roots.AddRange(new object[] { drawing.Protection, drawing.Marks, drawing.Objects });
            }

            foreach (object root in roots)
            {
                if (root is AttributeSection section)
                {
                    yield return section;
                    continue;
                }

                foreach (PropertyInfo property in root.GetType().GetProperties())
                {
                    if (property.GetValue(root) is AttributeSection page)
                    {
                        yield return page;
                    }
                }
            }
        }

        private static string[] Strings(Type page, string name) =>
            (string[])page.GetField(name, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
    }
}
