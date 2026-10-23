using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>
    /// The pages generated from the decoded Tekla dialogs (Pages/Generated, DrawingObjects/Generated), walked by reflection through every tab and
    /// group box of every host.
    /// </summary>
    public sealed class GeneratedPagesTests
    {
        public static IEnumerable<object[]> Hosts()
        {
            yield return new object[] { new Func<AttributeFile>(() => new ViewProperties()) };
            yield return new object[] { new Func<AttributeFile>(() => new GADrawingAttributeFile()) };
            yield return new object[] { new Func<AttributeFile>(() => new MultiDrawingAttributeFile()) };
            yield return new object[] { new Func<AttributeFile>(() => new AssemblyDrawingAttributeFile()) };
            yield return new object[] { new Func<AttributeFile>(() => new SinglePartDrawingAttributeFile()) };
            yield return new object[] { new Func<AttributeFile>(() => new CastUnitDrawingAttributeFile()) };
            foreach (Type type in ObjectFileTypes())
            {
                yield return new object[] { new Func<AttributeFile>(() => (AttributeFile)Activator.CreateInstance(type)) };
            }
        }

        /// <summary>The classes of the drawing object files (.dim, .drtxt, .pm, ...): every attribute file of the DrawingObjects namespace.</summary>
        internal static IEnumerable<Type> ObjectFileTypes() =>
            typeof(DimensionAttributeFile).Assembly.GetExportedTypes()
                .Where(t => t.Namespace == typeof(DimensionAttributeFile).Namespace && typeof(AttributeFile).IsAssignableFrom(t) && !t.IsAbstract)
                .OrderBy(t => t.Name, StringComparer.Ordinal);

        private static bool IsObjectFile(AttributeFile file) => file.GetType().Namespace == typeof(DimensionAttributeFile).Namespace;

        [Theory]
        [MemberData(nameof(Hosts))]
        public void EveryProperty_ReadsBackWhatItWrote_UnderItsPagePrefix(Func<AttributeFile> create)
        {
            AttributeFile file = create();
            var failures = new List<string>();
            int count = 0;

            foreach (AttributeSection page in Pages(file))
            {
                var keys = new HashSet<string>(PageKeys(page.GetType()).Select(key => page.Prefix + key), StringComparer.Ordinal);
                foreach ((string path, AttributeSection owner, PropertyInfo property) in Properties(page, page.GetType().Name))
                {
                    object value = TestValue(property.PropertyType, property.GetValue(owner));
                    int before = file.Values.Count;
                    property.SetValue(owner, value);
                    object read = property.GetValue(owner);
                    count++;

                    if (!Equals(value, read))
                    {
                        failures.Add($"{path}: wrote {value}, read {read}");
                    }

                    if (file.Values.Count != before + 1 && file.Values.Count != before)
                    {
                        failures.Add($"{path}: wrote {file.Values.Count - before} keys");
                    }
                }

                foreach (string key in file.Values.Keys.Where(k => k.StartsWith(page.Prefix, StringComparison.Ordinal) && page.Prefix.Length > 0))
                {
                    if (!keys.Contains(key) && !IsOtherPageKey(file, page, key))
                    {
                        failures.Add($"{page.GetType().Name}: {key} written but not in Keys");
                    }
                }
            }

            // The small object files (an edge chamfer has two settings) only need properties at all
            int minimum = IsObjectFile(file) && !(file is DimensionAttributeFile) ? 1 : 300;
            Assert.True(count >= minimum, "only " + count + " properties");
            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures.Take(30)));
        }

        [Fact]
        public void SetAllEnabled_WritesTheCheckBoxOfEveryControl()
        {
            var view = new ViewProperties();

            view.Objects.Part.SetAllEnabled(true);

            Assert.Equal("1", view.Values["vp.draw_as_en"].RawText);
            Assert.Equal("1", view.Values["vp.visible_colour_en"].RawText);
            Assert.All(view.Values, pair => Assert.True(IsEnableKey(pair.Key), pair.Key));
        }

        [Fact]
        public void Properties_ReadTheValuesTeklaWrites()
        {
            var view = new ViewProperties();

            // for_main is 2 in the bolt mark menu, but files saved by Tekla hold 1 for both pages
            view.Values.Set("vpm.for_main", AttributeValue.FromInt32(1));
            view.Values.Set("vsm.for_main", AttributeValue.FromInt32(1));
            view.Values.Set("vg.GridObjectGridOn", AttributeValue.FromInt32(0));

            Assert.True(view.Marks.PartMark.Advanced.InMainParts);
            Assert.True(view.Marks.BoltMark.General.VisibilityOfBoltMarks.InMainParts);
            Assert.True(Enum.IsDefined(typeof(GridObjectGridOn), view.Objects.Grid.Advanced.GridObjectGridOn));

            // A missing hatch background is "None" (0 in every shipped file), not black
            Assert.Equal(DrawingColor.None, view.Objects.Part.Fill.PartFaces.Background);
        }

        [Fact]
        public void AssigningAPropertyItsOwnValue_ChangesNoByte()
        {
            const string content = "encoding 1252\r\nlabel_colour 0xFFFFFFFF\r\ndepth_neg -0.000000\r\nvsm.for_sec 3\r\nvpm.for_main 1\r\n";
            var view = new ViewProperties();
            view.Load(WriteTemp("own.vi", content));

            var color = view.Attributes.Label.ViewLabel.Symbol.Color;
            Assert.Equal(-1, (int)color);
            view.Attributes.Label.ViewLabel.Symbol.Color = color;
            view.Attributes.Attributes1.View.DepthDown = view.Attributes.Attributes1.View.DepthDown;
            view.Marks.BoltMark.General.VisibilityOfBoltMarks.InSecondaryParts = view.Marks.BoltMark.General.VisibilityOfBoltMarks.InSecondaryParts;
            view.Marks.PartMark.Advanced.InMainParts = view.Marks.PartMark.Advanced.InMainParts;

            Assert.Equal(content, System.Text.Encoding.GetEncoding(1252).GetString(view.ToBytes()));
        }

        [Fact]
        public void ImportFrom_ReadsAnOlderPageFileWithTheDialogPrefix()
        {
            string path = WriteTemp("old.vpm", "vpm_dial.in_file \"old\"\r\nvpm_dial.FontAttr.height 3.500000\r\n");
            var view = new ViewProperties();

            view.Marks.PartMark.ImportFrom(path);

            Assert.Equal(3.5, view.Marks.PartMark.Content.Font.Height);
            Assert.DoesNotContain(view.Values.Keys, key => key.Contains("_dial."));
        }

        [Fact]
        public void AttributesPage_CopyFrom_CopiesOnlyTheTopLevelKeys()
        {
            var source = new ViewProperties();
            source.Attributes.Attributes1.View.Scale = 20;
            source.Objects.Part.Content.HiddenLines.HiddenLines = true;
            var target = new ViewProperties();

            target.Attributes.CopyFrom(source.Attributes);

            Assert.Equal(new[] { "view_scale" }, target.Values.Keys);
        }

        [Fact]
        public void SetAllEnabled_CoversTheCheckBoxesTheDialogSaves()
        {
            var view = new ViewProperties();

            view.Marks.PartMark.SetAllEnabled(true);

            Assert.True(view.Values.ContainsKey("vpm.FontAttr.font_enable"));
            Assert.True(view.Values.ContainsKey("vpm.in_all_views_en"));
        }

        private string WriteTemp(string name, string content)
        {
            string directory = Path.Combine(Path.GetTempPath(), "TeklaHelper.DrawingAttribute.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, name);
            File.WriteAllText(path, content, System.Text.Encoding.GetEncoding(1252));
            return path;
        }

        [Fact]
        public void Tekla2026FillColor_WritesTheKeysOfBarsAndMeshes()
        {
            var view = new ViewProperties();
            var drawing = new AssemblyDrawingAttributeFile();

            view.Objects.Reinforcement.BarAppearance.VisibleLines.FillColor = DrawingColor.Red;
            drawing.Objects.NeighborReinforcement.MeshAppearance.VisibleLines.FillColor = DrawingColor.Blue;

            Assert.Equal("160", view.Values["vrebar.BarAttr.fill_color"].RawText);
            Assert.Equal("162", drawing.Values["NeighborRebar.MeshAttr.fill_color"].RawText);
        }

        [Fact]
        public void ObjectPages_HaveNoReferenceObjectInTheAssemblyFamily()
        {
            Assert.NotNull(new GADrawingAttributeFile().Objects.ReferenceObject);
            Assert.NotNull(new ViewProperties().Objects.ReferenceObject);
            Assert.Null(new AssemblyDrawingAttributeFile().Objects.ReferenceObject);
            Assert.Null(new SinglePartDrawingAttributeFile().Objects.ReferenceObject);
            Assert.Null(new CastUnitDrawingAttributeFile().Objects.ReferenceObject);
        }

        [Fact]
        public void Pages_HaveTheDialogStructure()
        {
            var view = new ViewProperties();
            view.Objects.Part.Content.Representation.Representation = Representation.Exact;
            view.Objects.Part.Content.Representation.PartialProfile.Length = 100;
            view.Objects.Part.Content.HiddenLines.HiddenLines = true;
            view.Objects.Part.Appearance.VisibleLines.Color = DrawingColor.Blue;
            view.Objects.Part.Appearance.VisibleLines.Type = DrawingLineType.Dashed;
            view.Objects.Part.Fill.PartFaces.Type = "ANSI31";
            view.Objects.Part.Fill.PartFaces.Scale = HatchScale.Automatic;
            view.Marks.PartMark.General.FrameAroundMark.Type = FrameAroundMarkType.Rectangular;
            view.Marks.PartMark.General.Placing.Place.Quarter.Quarter1 = true;
            view.Attributes.LabelMarkA2.Content.Font.Height = 5;

            Assert.Equal(
                new[]
                {
                    "vp.draw_as 8", "vp.PartialProfileLength 100.000000", "vp.draw_hidden 1", "vp.visible_colour 162",
                    "vp.visible_linetype 2", "vp.PartHatch.hatch_name \"ANSI31\"", "vp.PartHatch.HatchAutomaticOrCustom 1",
                    "vpm.frame_type 1", "vpm.txpl.quarter_1 1", "ViewLabelMarkA2.FontAttr.height 5.000000",
                },
                view.Values.Select(p => p.Key + " " + p.Value.ToFileText()));
        }

        /// <summary>
        /// TEKLA_VIEW_DIALOG_SAMPLES names a folder with view and drawing files saved from the Tekla 2025 dialogs (.vi, .gd, .ad, .wd, .cud, like
        /// VIEW-TEST.vi and DRAWING-TEST.gd in D:\Tekla\Models\New model\attributes). Every key of a page in them must have
        /// a property (or be a check box or a file name key). Passes without checking anything when the variable is not set.
        /// </summary>
        [Fact]
        public void EveryKeyOfAFileSavedFromTekla_HasAProperty()
        {
            string root = Environment.GetEnvironmentVariable("TEKLA_VIEW_DIALOG_SAMPLES");
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return;
            }

            var failures = new List<string>();
            var files = Directory.EnumerateFiles(root, "*.vi").Select(path => (path, (AttributeFile)new ViewProperties()))
                .Concat(Directory.EnumerateFiles(root, "*.gd").Select(path => (path, (AttributeFile)new GADrawingAttributeFile())))
                .Concat(Directory.EnumerateFiles(root, "*.ad").Select(path => (path, (AttributeFile)new AssemblyDrawingAttributeFile())))
                .Concat(Directory.EnumerateFiles(root, "*.wd").Select(path => (path, (AttributeFile)new SinglePartDrawingAttributeFile())))
                .Concat(Directory.EnumerateFiles(root, "*.cud").Select(path => (path, (AttributeFile)new CastUnitDrawingAttributeFile())));
            foreach ((string path, AttributeFile file) in files)
            {
                if (!AttributeFileFormat.Read(path).HasEncodingLine)
                {
                    continue;
                }

                file.Load(path);
                foreach (AttributeSection page in Pages(file).Where(p => p.Prefix.Length > 0))
                {
                    Type type = page.GetType();
                    var known = new HashSet<string>(PageKeys(type).Concat(EnableKeys(type)), StringComparer.Ordinal);
                    foreach (string name in page.GetNames())
                    {
                        // "GridObjectGridOnEn" is the check box of the older "GridObjectGridOn"
                        bool camelEnableKey = name.EndsWith("En", StringComparison.Ordinal) && known.Contains(name.Substring(0, name.Length - 2));
                        if (!known.Contains(name) && !IsFileKey(name) && !IsEnableKey(name) && !camelEnableKey && !IsOtherPageKey(file, page, page.Prefix + name))
                        {
                            failures.Add($"{Path.GetFileName(path)}: {page.Prefix}{name}");
                        }
                    }
                }
            }

            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures.Take(30)));
        }

        /// <summary>
        /// Every key of a file saved from the Tekla dialogs (TEKLA_VIEW_DIALOG_SAMPLES) is read and written by a property:
        /// of a generated page (its <c>Keys</c>), or of the hand-written file, layout and filter sections (found by setting
        /// each of them on an empty file). Not typed: check boxes (<c>*_en</c>, <c>*_enable</c>, the page check boxes at the
        /// top level), the names of page files (<c>in_file</c> ...), <c>dl.placing_direction</c> (no Tekla dialog of 2020,
        /// 2025 or 2026 shows it), the fixed filter fields of the filter pages (<c>vfilter.partname_enable</c> ...: no dialog
        /// shows them since the filter rows moved to the .vi.filter file) and <c>bound_type</c> in assembly and single-part
        /// drawings (only the cast unit drawing properties show it).
        /// </summary>
        [Fact]
        public void EveryKeyOfAFileSavedFromTekla_IsTyped()
        {
            string root = Environment.GetEnvironmentVariable("TEKLA_VIEW_DIALOG_SAMPLES");
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return;
            }

            var failures = new List<string>();
            var types = new (string Pattern, Func<AttributeFile> Create)[]
            {
                ("*.vi", () => new ViewProperties()),
                ("*.gd", () => new GADrawingAttributeFile()),
                ("*.ad", () => new AssemblyDrawingAttributeFile()),
                ("*.wd", () => new SinglePartDrawingAttributeFile()),
                ("*.cud", () => new CastUnitDrawingAttributeFile()),
            }.Concat(ObjectFileTypes().Select(type => ("*" + ((AttributeFile)Activator.CreateInstance(type)).Extension,
                new Func<AttributeFile>(() => (AttributeFile)Activator.CreateInstance(type)))));
            foreach ((string pattern, Func<AttributeFile> create) in types)
            {
                HashSet<string> handWritten = KeysOfHandWrittenProperties(create);

                // The exact extension: on Windows *.wel also finds .welo files
                foreach (string path in Directory.EnumerateFiles(root, pattern).Where(p => string.Equals(Path.GetExtension(p), pattern.Substring(1), StringComparison.OrdinalIgnoreCase)))
                {
                    if (!HasEncodingLine(path))
                    {
                        continue;
                    }

                    AttributeFile file = create();
                    file.Load(path);
                    var typed = new HashSet<string>(handWritten, StringComparer.Ordinal);
                    foreach (AttributeSection page in Pages(file))
                    {
                        typed.UnionWith(PageKeys(page.GetType()).Select(key => page.Prefix + key));
                    }

                    foreach (string key in file.Values.Keys)
                    {
                        bool filterField = Regex.IsMatch(key, @"^(vfilter|vnp_filter|dfilter|dnp_filter)\.");
                        bool castUnitOnly = key == "bound_type" && !(file is CastUnitDrawingAttributeFile);
                        if (!typed.Contains(key) && !IsEnableKey(key) && !key.EndsWith("En", StringComparison.Ordinal) && !IsFileKey(key)
                            && key != "dl.placing_direction" && !filterField && !castUnitOnly)
                        {
                            failures.Add($"{Path.GetFileName(path)}: {key}");
                        }
                    }
                }
            }

            Assert.True(failures.Count == 0, failures.Count + " keys without a property:" + Environment.NewLine + string.Join(Environment.NewLine, failures.Distinct().Take(40)));
        }

        /// <summary>
        /// The keys the hand-written properties of a file type write: those of the file itself and of its layout and filter
        /// sections, each set on an empty file.
        /// </summary>
        private static HashSet<string> KeysOfHandWrittenProperties(Func<AttributeFile> create)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            var owners = new List<Func<AttributeFile, object>> { file => file };
            foreach (string name in new[] { "Layout", "Filter", "NeighborPartFilter" })
            {
                PropertyInfo section = create().GetType().GetProperty(name);
                if (section != null && typeof(AttributeSection).IsAssignableFrom(section.PropertyType))
                {
                    owners.Add(file => section.GetValue(file));
                }
            }

            foreach (Func<AttributeFile, object> owner in owners)
            {
                foreach (PropertyInfo property in owner(create()).GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    Type type = property.PropertyType;
                    bool simple = type == typeof(bool) || type == typeof(int) || type == typeof(double) || type == typeof(string) || type.IsEnum;
                    if (!property.CanWrite || !simple || property.GetIndexParameters().Length > 0 || property.Name == nameof(AttributeFile.CodePage))
                    {
                        continue;
                    }

                    AttributeFile file = create();
                    object target = owner(file);
                    property.SetValue(target, TestValue(type, property.GetValue(target)));
                    keys.UnionWith(file.Values.Keys);
                }
            }

            return keys;
        }

        private static IEnumerable<AttributeSection> Pages(AttributeFile file)
        {
            var roots = new List<object>();
            if (file is ViewProperties view)
            {
                roots.Add(view.Attributes);
                roots.Add(view.Attributes.LabelMarkA1);
                roots.Add(view.Attributes.LabelMarkA2);
                roots.Add(view.Attributes.LabelMarkA3);
                roots.Add(view.Attributes.LabelMarkA4);
                roots.Add(view.Attributes.LabelMarkA5);
                roots.Add(view.Protection);
                roots.Add(view.Dimensioning);
                roots.Add(view.Marks);
                roots.Add(view.Objects);
            }
            else if (IsObjectFile(file))
            {
                // Its pages (Properties, the marks of a dimension) and their label rows (SectionMarkA1, LabelMarkA1, ...)
                foreach (PropertyInfo property in file.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (property.GetValue(file) is AttributeSection page)
                    {
                        roots.Add(page);
                        roots.AddRange(page.GetType().GetProperties().Where(p => p.PropertyType == typeof(ViewLabelMark)).Select(p => p.GetValue(page)));
                    }
                }
            }
            else if (file is DrawingAttributeFile drawing)
            {
                roots.Add(drawing.Protection);
                roots.Add(drawing.Marks);
                roots.Add(drawing.Objects);
                foreach (AttributeSection page in new AttributeSection[] { drawing.Views, drawing.SectionView, drawing.DetailView, drawing.Dimension, drawing.Dimensioning, drawing.AssemblyDimensioning })
                {
                    if (page == null)
                    {
                        continue;
                    }

                    // The page and its label rows (LabelMarkA1, SectionMarkA1, DetailMarkA1, ...)
                    roots.Add(page);
                    roots.AddRange(page.GetType().GetProperties().Where(p => p.PropertyType == typeof(ViewLabelMark)).Select(p => p.GetValue(page)));
                }
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

        /// <summary>The value properties of a page and of its tabs and groups, depth first.</summary>
        private static IEnumerable<(string Path, AttributeSection Owner, PropertyInfo Property)> Properties(AttributeSection section, string path)
        {
            foreach (PropertyInfo property in section.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (typeof(AttributeSection).IsAssignableFrom(property.PropertyType))
                {
                    // Tabs and groups are nested classes; the label marks of ViewAttributes are pages of their own
                    if (property.PropertyType.DeclaringType != null)
                    {
                        foreach (var item in Properties((AttributeSection)property.GetValue(section), path + "." + property.Name))
                        {
                            yield return item;
                        }
                    }
                }
                else if (property.CanWrite)
                {
                    yield return (path + "." + property.Name, section, property);
                }
            }
        }

        private static object TestValue(Type type, object current)
        {
            if (type == typeof(bool))
            {
                return !(bool)current;
            }

            if (type == typeof(int))
            {
                return 7;
            }

            if (type == typeof(double))
            {
                return 12.5;
            }

            if (type == typeof(string))
            {
                return "TEST VALUE";
            }

            if (type == typeof(DrawingColor))
            {
                return DrawingColor.Red;
            }

            if (type == typeof(Marks.MarkContent))
            {
                return new Marks.MarkContent().AddText("TEST VALUE");
            }

            if (type.IsEnum)
            {
                Array values = Enum.GetValues(type);
                return values.GetValue(values.Length - 1);
            }

            throw new InvalidOperationException("Unexpected property type " + type);
        }

        private static IEnumerable<string> PageKeys(Type page) => StaticStrings(page, "Keys");

        private static IEnumerable<string> EnableKeys(Type page) => StaticStrings(page, "EnableKeys");

        private static IEnumerable<string> StaticStrings(Type page, string name)
        {
            FieldInfo field = page.GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.True(field != null, page.Name + " has no " + name);
            return (string[])field.GetValue(null);
        }

        /// <summary>
        /// The label marks share no prefix with ViewAttributes (whose prefix is empty), but the PourMark page of a drawing
        /// shares its prefix with the view's; keys of another page with a longer prefix are that page's.
        /// </summary>
        private static bool IsOtherPageKey(AttributeFile file, AttributeSection page, string key) =>
            Pages(file).Any(other => other.Prefix.Length > page.Prefix.Length && key.StartsWith(other.Prefix, StringComparison.Ordinal));

        /// <summary>
        /// Whether a file starts with an encoding line (a file a recent Tekla wrote), read from its first bytes: .cs is also
        /// the extension of the C# macros, which are no attribute files.
        /// </summary>
        internal static bool HasEncodingLine(string path)
        {
            var start = new byte[8];
            using (FileStream stream = File.OpenRead(path))
            {
                return stream.Read(start, 0, start.Length) == start.Length && System.Text.Encoding.ASCII.GetString(start) == "encoding";
            }
        }

        private static bool IsEnableKey(string key) =>
            key.EndsWith("_en", StringComparison.Ordinal) || key.EndsWith("_enable", StringComparison.Ordinal);

        private static bool IsFileKey(string name)
        {
            string last = name.Substring(name.LastIndexOf('.') + 1);
            return last == "in_file" || last == "out_file" || last == "aInFile" || last == "aOutFile" || last == "InFile" || last == "OutFile";
        }
    }
}
