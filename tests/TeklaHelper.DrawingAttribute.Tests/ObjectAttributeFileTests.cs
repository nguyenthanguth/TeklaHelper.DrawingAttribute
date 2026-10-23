using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>
    /// The attribute files of the drawing objects (the files the property dialogs of the drawing editor save). Each was
    /// saved from its Tekla 2025 dialog (OBJECT-TEST.&lt;ext&gt;), and the extension each attribute class of the Open API
    /// reads was checked in a running Tekla 2025.
    /// </summary>
    public sealed class ObjectAttributeFileTests : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "TeklaHelper.DrawingAttribute.Tests", Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }

        public static IEnumerable<object[]> Types() => GeneratedPagesTests.ObjectFileTypes().Select(type => new object[] { type });

        [Fact]
        public void EveryDialogOfTheDrawingEditor_HasAFileClass()
        {
            var expected = new Dictionary<string, string>
            {
                ["DimensionAttributeFile"] = ".dim", ["ReinforcementDimensionAttributeFile"] = ".rdim", ["TextAttributeFile"] = ".drtxt",
                ["SymbolAttributeFile"] = ".sbl", ["LevelMarkAttributeFile"] = ".lev", ["RevisionMarkAttributeFile"] = ".rev",
                ["AssociativeNoteAttributeFile"] = ".note", ["PartMarkAttributeFile"] = ".pm", ["BoltMarkAttributeFile"] = ".sm",
                ["ConnectionMarkAttributeFile"] = ".jm", ["ReinforcementMarkAttributeFile"] = ".rm", ["SurfaceTreatmentMarkAttributeFile"] = ".surfm",
                ["PourMarkAttributeFile"] = ".pom", ["WeldMarkAttributeFile"] = ".wel", ["DrawingWeldMarkAttributeFile"] = ".wls",
                ["LineAttributeFile"] = ".gln", ["ArcAttributeFile"] = ".gar", ["PolylineAttributeFile"] = ".gpl", ["CircleAttributeFile"] = ".gci",
                ["RectangleAttributeFile"] = ".grt", ["PolygonAttributeFile"] = ".gpg", ["TextFileAttributeFile"] = ".fas", ["DwgAttributeFile"] = ".fdg",
                ["ImageAttributeFile"] = ".idg", ["HyperlinkAttributeFile"] = ".fhl", ["LinkedDrawingAttributeFile"] = ".ldr",
                ["PartAttributeFile"] = ".dprt", ["SurfaceTreatmentAttributeFile"] = ".dsrf", ["PourAttributeFile"] = ".po",
                ["PourBreakAttributeFile"] = ".pbr", ["BoltAttributeFile"] = ".sc", ["WeldAttributeFile"] = ".welo",
                ["EdgeChamferAttributeFile"] = ".dcha", ["ReinforcementAttributeFile"] = ".drbr", ["ReinforcementMeshAttributeFile"] = ".drmsh",
                ["ReferenceObjectAttributeFile"] = ".drmp", ["GridAttributeFile"] = ".dgr", ["GridLineAttributeFile"] = ".grl",
                ["SectionMarkAttributeFile"] = ".cs", ["DetailMarkAttributeFile"] = ".detail", ["GADimensionSetupAttributeFile"] = ".ga_dim_setup",
            };

            var actual = GeneratedPagesTests.ObjectFileTypes().ToDictionary(t => t.Name, t => ((AttributeFile)Activator.CreateInstance(t)).Extension);

            Assert.Equal(expected.OrderBy(p => p.Key), actual.OrderBy(p => p.Key));
            foreach (Type type in GeneratedPagesTests.ObjectFileTypes())
            {
                Assert.Equal(actual[type.Name], type.GetField("FileExtension").GetValue(null));
            }
        }

        [Theory]
        [MemberData(nameof(Types))]
        public void CreateDefault_TypesEveryKey_AndSavesWhatItLoads(Type type)
        {
            var file = (AttributeFile)type.GetMethod("CreateDefault").Invoke(null, null);
            var typed = new HashSet<string>(StringComparer.Ordinal);
            foreach (AttributeSection page in PagesOf(file))
            {
                typed.UnionWith(StaticStrings(page.GetType(), "Keys").Concat(StaticStrings(page.GetType(), "EnableKeys")).Select(key => page.Prefix + key));
            }

            Assert.NotEmpty(file.Values);
            Assert.DoesNotContain(file.Values.Keys, key => !typed.Contains(key) && !IsFileKey(key) && !key.EndsWith("_en", StringComparison.Ordinal) && !key.EndsWith("_enable", StringComparison.Ordinal));

            string path = file.SaveTo(_directory, "default");
            Assert.EndsWith(file.Extension, path, StringComparison.Ordinal);
            byte[] saved = File.ReadAllBytes(path);
            var copy = (AttributeFile)Activator.CreateInstance(type);
            copy.Load(path);
            copy.Save(path);
            Assert.Equal(saved, File.ReadAllBytes(path));
        }

        [Theory]
        [MemberData(nameof(Types))]
        public void Load_StripsTheDialogPrefixOfLegacyFiles(Type type)
        {
            var file = (AttributeFile)Activator.CreateInstance(type);
            string legacy = (string)type.GetProperty("LegacyKeyPrefix", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(file);
            string key = ((AttributeFile)type.GetMethod("CreateDefault").Invoke(null, null)).Values.Keys.First(k => !IsFileKey(k));
            Directory.CreateDirectory(_directory);
            string path = Path.Combine(_directory, "old" + file.Extension);
            File.WriteAllText(path, legacy + key + " 1\r\n");

            file.Load(path);

            Assert.Matches("^[a-z_]+_dial\\.$", legacy);
            Assert.Equal(new[] { key }, file.Values.Keys);
        }

        [Fact]
        public void Text_WritesTheKeysOfTheDialog()
        {
            var text = new TextAttributeFile();

            text.Properties.Appearance.Text.Height = 3.5;
            text.Properties.Appearance.Text.Color = DrawingColor.Blue;
            text.Properties.Appearance.Arrow.Type = TextArrowType.OpenArrow;
            text.Properties.Appearance.Placing.Place.MinimumDistance = 4;

            Assert.Equal(new[] { "height 3.500000", "text_colour 162", "arrow_type 2", "txpl.text_freepl_min 4.000000" }, Lines(text));
        }

        [Fact]
        public void LineAndPolygon_WriteArrowAndFill()
        {
            var line = new LineAttributeFile();
            line.Properties.Line.Color = DrawingColor.Red;
            line.Properties.Arrow.Position = ArrowPosition.Both;
            var polygon = new PolygonAttributeFile();
            polygon.Properties.Line.Type = DrawingLineType.Dashed;

            Assert.Equal(new[] { "color 160", "arrow 3" }, Lines(line));
            Assert.Equal(new[] { "linetype 2" }, Lines(polygon));
            Assert.Equal(".gpg", PolygonAttributeFile.FileExtension);
        }

        [Fact]
        public void Part_WritesTheHatchesUnderTheirStructures()
        {
            var part = new PartAttributeFile();

            part.Properties.Content.Representation.Representation = Representation.Exact;
            part.Properties.Fill.PartFaces.Color = DrawingColor.Red;
            part.Properties.Fill.Sections.Type = "ANSI31";

            Assert.Equal(new[] { "draw_as 8", "PartHatch.hatch_color 160", "SectionHatch.hatch_name \"ANSI31\"" }, Lines(part));
        }

        [Fact]
        public void DrawingWeldMark_TypesTheWeldSymbols()
        {
            var weld = new DrawingWeldMarkAttributeFile();

            weld.Properties.Content.AboveLine.Type = WeldType.Fillet;
            weld.Properties.Content.BelowLine.Contour = WeldContour.Convex;
            weld.Properties.Content.Other.WorkshopSite = WeldWorkshopSite.Site;

            Assert.Equal(new[] { "w_type 10", "w_ftype2 2", "w_workshop 1" }, Lines(weld));
            Assert.Equal(new[] { 23, 24, 25, 26 }, new[] { WeldType.Edge, WeldType.IsoSurfacing, WeldType.Fold, WeldType.Inclined }.Select(t => (int)t));
            Assert.Equal(new[] { 4, 5 }, new[] { WeldFinish.FinishedWeld, WeldFinish.SmoothTransition }.Select(f => (int)f));
            Assert.Equal(27, Enum.GetValues(typeof(WeldType)).Length);
            Assert.Equal(".wls", weld.Extension);
            Assert.Equal(".wel", new WeldMarkAttributeFile().Extension);
        }

        [Fact]
        public void SectionAndDetailMarks_HaveTheirLabelRows()
        {
            var section = new SectionMarkAttributeFile();
            section.Properties.SectionMarkA1.Content.Font.Height = 5;
            section.Properties.LabelMarkA2.Content.Font.Height = 4;
            section.Properties.CuttingLine.Properties.Color = DrawingColor.Blue;
            var detail = new DetailMarkAttributeFile();
            detail.Properties.LabelMarkA1.Content.Font.Height = 3;
            detail.Properties.DetailMarkA5.Content.Font.Height = 2;

            Assert.Equal(new[] { "SectionMarkA1.FontAttr.height 5.000000", "ViewLabelMarkA2.FontAttr.height 4.000000", "line_colour 162" }, Lines(section));
            Assert.Equal(new[] { "ViewMarkA1.FontAttr.height 3.000000", "DetailMarkA5.FontAttr.height 2.000000" }, Lines(detail));
        }

        [Fact]
        public void Marks_UseThePagesOfTheViewProperties()
        {
            var view = new ViewProperties();
            view.Marks.PartMark.Content.Font.Height = 3;
            var mark = new PartMarkAttributeFile();

            mark.Properties.CopyFrom(view.Marks.PartMark);
            var grid = new GridLineAttributeFile();
            grid.Properties.GridLine.Color = DrawingColor.Red;

            Assert.Equal(new[] { "FontAttr.height 3.000000" }, Lines(mark));

            // A page shared with the view properties: CreateDefault() writes the values of the .pm files, not of the .vi files
            Assert.Equal(155, PartMarkAttributeFile.CreateDefault().GetInt32("text_colour"));
            Assert.IsType<PartMark>(mark.Properties);
            Assert.IsType<GridObject>(new GridAttributeFile().Properties);
            Assert.Contains("line_colour 160", Lines(grid));
        }

        [Fact]
        public void ReinforcementDimension_HoldsTheKeysOfADimensionFile()
        {
            var dimension = new ReinforcementDimensionAttributeFile();

            dimension.Properties.General.DimensionTypes.Straight = StraightDimensionType.Absolute;
            dimension.PrefixMark.Font.Height = 2.5;

            Assert.Equal(new[] { "dim_type 2", "PrefixMark.FontAttr.height 2.500000" }, Lines(dimension));
            Assert.Equal(9, dimension.Marks.Distinct().Count());
            Assert.Equal(".rdim", dimension.Extension);
        }

        /// <summary>
        /// TEKLA_VIEW_DIALOG_SAMPLES names the folder with OBJECT-TEST.&lt;ext&gt;, each saved from its Tekla 2025 dialog:
        /// <c>CreateDefault()</c> writes its keys (without the page file names) in its order. Passes without checking
        /// anything when the variable is not set.
        /// </summary>
        [Theory]
        [MemberData(nameof(Types))]
        public void CreateDefault_WritesTheKeysOfTheFileSavedFromTheDialog(Type type)
        {
            string root = Environment.GetEnvironmentVariable("TEKLA_VIEW_DIALOG_SAMPLES");
            var file = (AttributeFile)type.GetMethod("CreateDefault").Invoke(null, null);
            string sample = root == null ? null : Path.Combine(root, "OBJECT-TEST" + file.Extension);
            if (sample == null || !File.Exists(sample))
            {
                return;
            }

            List<string> dialogKeys = AttributeFileFormat.Read(sample).Values.Keys.Where(k => k != "in_file" && k != "out_file").ToList();

            // After them only keys of a page that the dialog no longer writes and shipped files hold (the rebar dimension
            // marks of the note and of every dimension mark), which every other test checks are typed
            Assert.Equal(dialogKeys, file.Values.Keys.Where(dialogKeys.Contains));
            Assert.All(file.Values.Keys.Except(dialogKeys), key => Assert.Contains("RebarGroupMarkAttributes.", key, StringComparison.Ordinal));
        }

        /// <summary>
        /// TEKLA_ATTRIBUTE_SAMPLES names a folder of files shipped with Tekla (the environments): every key of every file of
        /// each type that a recent Tekla wrote (with an encoding line) has a property. Not typed: check boxes (<c>*_en</c>,
        /// <c>GridObjectGridOnEn</c>), page file names,
        /// and <c>RebarGroupMarkAttributes.CloseRebarDimMarkToFather</c> / <c>TaggedRebarDimMarkLocation</c> in the mark files
        /// that use a page of the view properties (.pm, .rm): no dialog of Tekla 2025 or 2026 has them, and the released
        /// view pages do not either. Passes without checking anything when the variable is not set.
        /// </summary>
        [Fact]
        public void EveryKeyOfTheShippedFiles_IsTyped()
        {
            string root = Environment.GetEnvironmentVariable("TEKLA_ATTRIBUTE_SAMPLES");
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return;
            }

            var failures = new List<string>();
            var extensions = GeneratedPagesTests.ObjectFileTypes().ToDictionary(t => ((AttributeFile)Activator.CreateInstance(t)).Extension, t => t, StringComparer.OrdinalIgnoreCase);
            foreach (string path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                if (!extensions.TryGetValue(Path.GetExtension(path), out Type type) || !GeneratedPagesTests.HasEncodingLine(path))
                {
                    continue;
                }

                var file = (AttributeFile)Activator.CreateInstance(type);
                file.Load(path);
                var typed = new HashSet<string>(StringComparer.Ordinal);
                foreach (AttributeSection page in PagesOf(file))
                {
                    typed.UnionWith(StaticStrings(page.GetType(), "Keys").Select(key => page.Prefix + key));
                }

                bool viewMarkPage = PagesOf(file).Any(page => page.GetType().Namespace == typeof(PartMark).Namespace && page.GetType() != typeof(ViewLabelMark));
                foreach (string key in file.Values.Keys)
                {
                    bool obsolete = viewMarkPage && (key.StartsWith("RebarGroupMarkAttributes.CloseRebarDimMarkToFather", StringComparison.Ordinal)
                        || key.StartsWith("RebarGroupMarkAttributes.TaggedRebarDimMarkLocation", StringComparison.Ordinal));
                    // "GridObjectGridOnEn" is the check box of "GridObjectGridOn", in the naming of the newer pages
                    bool camelEnableKey = key.EndsWith("En", StringComparison.Ordinal) && typed.Contains(key.Substring(0, key.Length - 2));
                    if (!typed.Contains(key) && !IsFileKey(key) && !key.EndsWith("_en", StringComparison.Ordinal) && !key.EndsWith("_enable", StringComparison.Ordinal) && !camelEnableKey && !obsolete)
                    {
                        failures.Add(type.Name + ": " + key + " (" + path.Substring(root.Length) + ")");
                    }
                }
            }

            Assert.True(failures.Count == 0, failures.Count + " keys without a property:" + Environment.NewLine + string.Join(Environment.NewLine, failures.Distinct().Take(40)));
        }

        /// <summary>
        /// TEKLA_ATTRIBUTE_SAMPLES names a folder of files shipped with Tekla: every legacy file of a type (no encoding line,
        /// every key prefixed with the dialog name) uses the prefix of its class, and loads without it. Passes without
        /// checking anything when the variable is not set.
        /// </summary>
        [Fact]
        public void LegacyShippedFiles_UseTheDialogPrefixOfTheirClass()
        {
            string root = Environment.GetEnvironmentVariable("TEKLA_ATTRIBUTE_SAMPLES");
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return;
            }

            var failures = new List<string>();
            var extensions = GeneratedPagesTests.ObjectFileTypes().ToDictionary(t => ((AttributeFile)Activator.CreateInstance(t)).Extension, t => t, StringComparer.OrdinalIgnoreCase);
            foreach (string path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                if (!extensions.TryGetValue(Path.GetExtension(path), out Type type) || GeneratedPagesTests.HasEncodingLine(path))
                {
                    continue;
                }

                byte[] start = File.ReadAllBytes(path);
                System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(System.Text.Encoding.ASCII.GetString(start, 0, Math.Min(start.Length, 64)), @"^(\w+_dial\.)");
                if (!match.Success)
                {
                    continue;
                }

                var file = (AttributeFile)Activator.CreateInstance(type);
                string legacy = (string)type.GetProperty("LegacyKeyPrefix", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(file);
                file.Load(path);
                if (match.Groups[1].Value != legacy || file.Values.Keys.Any(key => key.StartsWith(legacy, StringComparison.Ordinal)))
                {
                    failures.Add(type.Name + ": " + match.Groups[1].Value + " (" + path.Substring(root.Length) + ")");
                }
            }

            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures.Take(30)));
        }

        private static IEnumerable<string> Lines(AttributeFile file) => file.Values.Select(p => p.Key + " " + p.Value.ToFileText());

        /// <summary>The pages of an object file and their label rows.</summary>
        private static IEnumerable<AttributeSection> PagesOf(AttributeFile file)
        {
            foreach (PropertyInfo property in file.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (property.GetValue(file) is AttributeSection page)
                {
                    yield return page;
                    foreach (PropertyInfo row in page.GetType().GetProperties().Where(p => p.PropertyType == typeof(ViewLabelMark)))
                    {
                        yield return (AttributeSection)row.GetValue(page);
                    }
                }
            }
        }

        private static IEnumerable<string> StaticStrings(Type page, string name) =>
            (string[])page.GetField(name, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

        private static bool IsFileKey(string key)
        {
            string last = key.Substring(key.LastIndexOf('.') + 1);
            return last == "in_file" || last == "out_file" || last == "aInFile" || last == "aOutFile" || last == "InFile" || last == "OutFile";
        }
    }
}
