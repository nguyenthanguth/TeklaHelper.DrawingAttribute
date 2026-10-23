using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using TeklaHelper.DrawingAttribute;

namespace DialogClassGenerator
{
    /// <summary>
    /// Writes one class per page of the Tekla View properties dialog: tabs, group boxes and sub-dialogs become nested
    /// classes, controls become typed properties named after their dialog labels, and the keys of the page that the
    /// dialog does not show go to an Advanced group. Input: the decoded dialogs (Dialogs/*.json, see tools/DialogDecoder)
    /// complete files saved from Tekla (.vi, .gd, .ad), which give the number types and the keys the dialog does not show,
    /// and the environments shipped with Tekla, whose most common values are the defaults (see Defaults.cs).
    /// <code>
    /// DialogClassGenerator &lt;Dialogs folder&gt; &lt;sample .vi&gt; &lt;sample .gd&gt; &lt;output folder&gt; &lt;sample .ad&gt; &lt;environments folder&gt;
    /// </code>
    /// </summary>
    internal static partial class Program
    {
        private static readonly HashSet<string> FileKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "in_file", "out_file", "aInFile", "aOutFile", "InFile", "OutFile",
        };

        // Members of AttributeSection and of the generated page classes, which properties must not hide
        private static readonly HashSet<string> Reserved = new HashSet<string>(StringComparer.Ordinal)
        {
            "Values", "Prefix", "GetKey", "GetSection", "Contains", "TryGetValue", "Remove", "GetNames", "CopyFrom",
            "ExportTo", "ImportFrom", "GetString", "GetInt32", "GetDouble", "GetBoolean", "GetEnum", "GetDate", "Set",
            "SetString", "SetInt32", "SetDouble", "SetBoolean", "SetEnum", "SetDate", "IsEnabled", "SetEnabled",
            "SetAllEnabled", "EmptyDateValue", "Equals", "GetHashCode", "ToString", "GetType", "MemberwiseClone",
            "Finalize", "Keys", "EnableKeys", "ViewPrefix", "GADrawingPrefix", "AssemblyDrawingPrefix",
            "ViewFileExtension", "GADrawingFileExtension", "AssemblyDrawingFileExtension", "SetDefaults", "DefaultValues",
            "PrefixFor", "SetBooleanUnlessSame", "PageFileInKey", "PageFileOutKey",
        };

        // Control labels that say nothing on their own: their enum takes the group label in front
        /// <summary>Enum names that the rules above make awkward: name chosen by the rules → name used.</summary>
        private static readonly Dictionary<string, string> EnumRenames = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ContentRepresentation"] = "SurfaceTreatmentRepresentation",
            ["VisibilityOfMarkVisibilityInView"] = "PourMarkVisibilityInView",
            ["IdenticalMarksInSameCastUnit"] = "ReinforcementMarkMerging",
            ["MergingIdenticalMarksInSameCastUnit"] = "NeighborReinforcementMarkMerging",
            ["VisibilityWelds"] = "WeldVisibility",
            ["Welds"] = "WeldMarkVisibility",
            ["ReinforcementReinforcingBarRepresentation"] = "MeshRepresentation",
            ["AssAttrRepresentation"] = "RebarAssemblyRepresentation",
            ["PartFacesScale"] = "HatchScale",
            ["LineType"] = "WeldMarkLineShape",
            ["Symbol"] = "ViewLabelSymbol",

            // Pages of the drawing properties
            ["Straight"] = "DimensionType",
            ["InXDirection"] = "DimensionTypeInXDirection",
            ["Angle"] = "AngleDimensionType",
            ["PrecisionStraight"] = "DimensionPrecision",
            ["PrecisionAngle"] = "AngleDimensionPrecision",
            ["FormatStraight"] = "DimensionFormat",
            ["FormatAngle"] = "AngleDimensionFormat",
            ["TextPlace"] = "DimensionTextPlacing",
            ["ArrowShape"] = "DimensionArrowShape",
            ["ShortDimensions"] = "ShortDimensionPlacing",
            ["Origin"] = "ExaggerationOrigin",
            ["PlateSideMarksType"] = "PlateSideMarkType",
            ["DimensionPositioningHorizontal"] = "GridDimensionHorizontalPosition",
            ["DimensionPositioningVertical"] = "GridDimensionVerticalPosition",
            ["LeftSection"] = "SectionDirection",
            ["ScaleSize"] = "SectionViewSize",
            ["LeftSymbol"] = "SectionMarkLeftSymbol",
            ["RightSymbol"] = "SectionMarkRightSymbol",
            ["ShapeOfTheBoundary"] = "DetailBoundaryShape",
            ["DetailViewSymbol"] = "DetailMarkSymbol",
            ["CoordSys"] = "ViewCoordinateSystem",
            ["CoordSysXRotate"] = "ViewRotationAroundX",
            ["CoordSysYRotate"] = "ViewRotationAroundY",
            ["Internal"] = "PartInternalDimensions",
            ["Options"] = "CombineDimensionsOption",
            ["SecondaryPart"] = "SecondaryPartPosition",
            ["PreferredDimSide"] = "PartPreferredDimensionSide",
            ["BoltDimensionsPreferredDimSide"] = "BoltPreferredDimensionSide",
            ["PositionBoltsTo"] = "PositionTo",
            ["CenteredPart"] = "CenteredPosition",
            ["CombineBoltDimensions"] = "CombineDimensions",
        };

        /// <summary>Member names for options that the dialog shows as a picture only, by the picture's name.</summary>
        private static readonly Dictionary<string, string> PictureMembers = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["gr_pb_no_arrow"] = "NoArrow",
            ["gr_pb_solid_arrow2"] = "SolidArrow",
            ["gr_pb_open_arrow2"] = "OpenArrow",
            ["gr_pb_circle"] = "Circle",
            ["weld_single_line"] = "SingleLine",
            ["weld_double_line"] = "DoubleLine",
            ["gr_mark_parallel_on_rebar_leader_line"] = "ParallelOnLeaderLine",
            ["om_pb_dim_arrow_filled"] = "FilledArrow",
            ["om_pb_dim_arrow_line"] = "LineArrow",
            ["om_pb_dim_arrow_circle"] = "CircleArrow",
            ["om_pb_dim_arrow_open"] = "OpenArrow",
            ["knock_off_dimension_line"] = "DimensionLine",
            ["knock_off_dimension_parenthesis"] = "Parenthesis",
            ["adcd_part_prefered_side_view"] = "View",
            ["adcd_part_prefered_side_side"] = "Side",
            ["adcd_screw_prefered_side_view"] = "View",
            ["adcd_screw_prefered_side_side"] = "Side",
        };

        private static readonly HashSet<string> GenericLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Type", "Color", "Format", "Units", "Size", "Symbol", "Visibility", "Position", "Frame", "Arrow", "Content",
            "Vertical", "Horizontal", "Shape", "Placing", "Scale", "Background", "Visible", "Representation", "Merge",
            "Text", "Lines", "Direction", "Alignment", "Mode", "Line length",
        };

        /// <summary>
        /// Keys that the dialog of an earlier Tekla version does not have (found by decoding older TeklaStructures.aid files
        /// with tools/DialogDecoder/compare.py): key → first version that has it.
        /// </summary>
        private static readonly Dictionary<string, string> KeysAddedIn = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["BarAttr.fill_color"] = "2026",
            ["MeshAttr.fill_color"] = "2026",
        };

        /// <summary>
        /// What a running Tekla does with a key, verified with a probe view (see CLAUDE.md): "page class:key" → note for its
        /// XML comment. Only the pages that were probed: the same key of another page can mean something else.
        /// </summary>
        private static readonly Dictionary<string, string> KeyNotes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["PartMark:create"] = "Tekla 2025 ignores it when it loads the file: with 0 the part marks of a view still showed."
                + " Hide the marks with <c>General.VisibilityOfMark.VisibilityInView</c> (<c>in_all_views</c>).",
        };

        /// <summary>The page being written, for <see cref="KeyNotes"/>.</summary>
        private static string CurrentPage;

        /// <summary>Check boxes (page class, key) found in the files saved from the dialog.</summary>
        private static readonly List<(string Page, string Key)> FileEnableKeys = new List<(string Page, string Key)>();

        /// <summary>A value the menu does not offer gets an enum member when this many sample files hold it.</summary>
        private const int MinimumFilesForMember = 3;

        private static readonly Dictionary<string, EnumModel> EnumsBySignature = new Dictionary<string, EnumModel>(StringComparer.Ordinal);
        private static readonly Dictionary<string, EnumModel> EnumsByName = new Dictionary<string, EnumModel>(StringComparer.Ordinal);
        private static readonly Dictionary<string, SortedSet<string>> EnumKeys = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);

        private static int Main(string[] args)
        {
            if (args.Length != 6)
            {
                Console.Error.WriteLine("Usage: DialogClassGenerator <Dialogs folder> <sample .vi> <sample .gd> <output folder> <sample .ad> <Tekla environments folder>");
                return 2;
            }

            AttributeCollection vi = AttributeFileFormat.Read(args[1]).Values;
            AttributeCollection gd = AttributeFileFormat.Read(args[2]).Values;
            AttributeCollection ad = AttributeFileFormat.Read(args[4]).Values;
            SampleAd = ad;
            LoadStatistics(args[5]);
            Directory.CreateDirectory(args[3]);
            foreach (string old in Directory.GetFiles(args[3], "*.g.cs"))
            {
                File.Delete(old);
            }

            foreach (PageSpec spec in PageSpecs.All)
            {
                using (JsonDocument document = JsonDocument.Parse(File.ReadAllText(Path.Combine(args[0], spec.Json + ".json"))))
                {
                    spec.FileInKey = Str(document.RootElement, "fileMenuKey") ?? spec.FileInKey;
                    spec.FileOutKey = Str(document.RootElement, "saveAsKey") ?? spec.FileOutKey;
                    Container page = BuildPage(spec, document.RootElement, vi, gd);
                    File.WriteAllText(Path.Combine(args[3], spec.Class + ".g.cs"), WritePage(spec, page, vi, gd), new UTF8Encoding(false));
                    Console.WriteLine($"{spec.Class}: {CountControls(page)} properties");
                }
            }

            File.WriteAllText(Path.Combine(args[3], "Enums.g.cs"), WriteEnums(), new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(args[3], "FileDefaults.g.cs"), WriteFileDefaults(vi, gd, ad), new UTF8Encoding(false));
            Console.WriteLine($"Enums: {EnumsByName.Count}");
            return 0;
        }

        // ---------------------------------------------------------------- model

        private static Container BuildPage(PageSpec spec, JsonElement json, AttributeCollection vi, AttributeCollection gd)
        {
            var page = new Container { Kind = "Page", ClassName = spec.Class, Label = spec.Title };
            foreach (JsonElement tab in Array(json, "tabs"))
            {
                page.Items.Add(ReadContainer(tab, "Tab"));
            }

            foreach (object item in ReadItems(json))
            {
                page.Items.Add(item);
            }

            // Sub-dialogs go where their button is
            foreach (JsonElement sub in Array(json, "subDialogs"))
            {
                string openedBy = Str(sub, "openedBy");
                var dialog = ReadContainer(sub, "Dialog");
                dialog.Label = (openedBy ?? Str(sub, "dialog") ?? "Dialog").TrimEnd('.').Trim();
                if (!ReplaceButton(page, openedBy, dialog))
                {
                    page.Items.Add(dialog);
                }
            }

            Prune(page);
            var handWritten = new HashSet<string>(spec.HandWrittenKeys, StringComparer.Ordinal);
            RemoveControls(page, control => handWritten.Contains(control.Key));

            // Everything the dialog does not show on this page, so that every key has a property
            var covered = new HashSet<string>(AllControls(page).Select(c => c.Key), StringComparer.Ordinal);
            covered.UnionWith(handWritten);
            var enableKeys = new HashSet<string>(AllControls(page).Where(c => c.EnableKey != null).Select(c => c.EnableKey), StringComparer.Ordinal);
            var advanced = new Container { Kind = "Group", Label = "Advanced", Description = "Keys of this page that the dialog does not show (or shows on another page)." };
            foreach ((string array, string origin) in new[] { ("siblingPageControls", "sibling"), ("legacyControls", "legacy") })
            {
                foreach (JsonElement element in Array(json, array))
                {
                    Control control = ReadControl(element);
                    if (control.Key != null && covered.Add(control.Key))
                    {
                        control.Origin = origin;
                        if (Str(element, "confidence") == "other-dialog")
                        {
                            // A control of another dialog with the same keys goes where that dialog shows it
                            control.Origin = "other";
                            InsertByPath(page, Array(element, "dialogPath").Select(p => p.GetString()).ToList(), control);
                            continue;
                        }

                        advanced.Items.Add(control);
                    }
                }
            }

            foreach ((string key, AttributeValue value) in SampleKeys(spec, vi, gd))
            {
                // "GridObjectGridOnEn" is the check box of "GridObjectGridOn", in the naming of the newer pages
                bool camelEnableKey = key.EndsWith("En", StringComparison.Ordinal) && covered.Contains(key.Substring(0, key.Length - 2));
                if (IsEnableKey(key) || camelEnableKey || enableKeys.Contains(key))
                {
                    FileEnableKeys.Add((spec.Class, key));
                }

                if (covered.Contains(key) || enableKeys.Contains(key) || IsEnableKey(key) || camelEnableKey || FileKeys.Contains(LastSegment(key)))
                {
                    continue;
                }

                covered.Add(key);
                advanced.Items.Add(new Control
                {
                    Label = key,
                    Key = key,
                    Type = value.Kind == AttributeValueKind.String ? "string" : value.Kind == AttributeValueKind.Double ? "double" : "int",
                    Origin = "file",
                });
            }

            if (advanced.Items.Count > 0)
            {
                page.Items.Add(advanced);
            }

            RecordGroupLabels(page);
            foreach (Control control in AllControls(page))
            {
                Resolve(spec, control, vi, gd);
            }

            // Names the hand-written part and the label rows take are taken at the level of the page only
            var reserved = new HashSet<string>(spec.HandWrittenMembers, StringComparer.Ordinal);
            reserved.UnionWith(LabelMarkProperties(spec).Select(row => row.Property));
            AssignNames(page, new HashSet<string>(StringComparer.Ordinal) { spec.Class }, reserved, spec.DrawingPage);
            return page;
        }

        /// <summary>Adds <paramref name="control"/> to the tab and groups named by <paramref name="path"/>, creating the missing ones.</summary>
        private static void InsertByPath(Container page, List<string> path, Control control)
        {
            Container target = page;
            foreach (string label in path)
            {
                Container found = target.Items.OfType<Container>().FirstOrDefault(c => c.Kind != "Dialog" && c.Label == label);
                if (found == null)
                {
                    bool tabs = target == page && page.Items.OfType<Container>().Any(c => c.Kind == "Tab");
                    found = new Container { Kind = tabs ? "Tab" : "Group", Label = label };
                    target.Items.Add(found);
                }

                target = found;
            }

            target.Items.Add(control);
        }

        /// <summary>The label rows of a page: property name, key stem relative to the prefix (ending with the row and a dot), summary.</summary>
        private static IEnumerable<(string Property, string KeyStem, string Summary, int Row)> LabelMarkProperties(PageSpec spec) =>
            spec.LabelMarks.SelectMany(mark => Enumerable.Range(1, 5).Select(row =>
                (mark.Property + row.ToString(CultureInfo.InvariantCulture), mark.KeyStem + row.ToString(CultureInfo.InvariantCulture) + ".", mark.Summary, row)));

        /// <summary>Whether a key of the page belongs to one of its label rows (typed by <c>ViewLabelMark</c>).</summary>
        private static bool IsLabelMarkKey(PageSpec spec, string key) =>
            LabelMarkProperties(spec).Any(row => key.StartsWith(row.KeyStem, StringComparison.Ordinal));

        private static void RemoveControls(Container container, Func<Control, bool> predicate)
        {
            container.Items.RemoveAll(item => item is Control control && predicate(control));
            foreach (Container child in container.Items.OfType<Container>())
            {
                RemoveControls(child, predicate);
            }

            container.Items.RemoveAll(item => item is Container child && child.Items.Count == 0);
        }

        private static IEnumerable<(string Key, AttributeValue Value)> SampleKeys(PageSpec spec, AttributeCollection vi, AttributeCollection gd)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);

            // The assembly drawing file only for a page GA drawings do not have (dcd.): the other pages keep the keys they had
            string assemblyOnly = spec.GADrawingPrefix == null ? spec.AssemblyDrawingPrefix : null;
            foreach ((AttributeCollection file, string prefix) in new[] { (vi, spec.ViewPrefix), (gd, spec.GADrawingPrefix), (SampleAd, assemblyOnly) })
            {
                if (prefix == null)
                {
                    continue;
                }

                foreach (KeyValuePair<string, AttributeValue> pair in file)
                {
                    if (!pair.Key.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string key = pair.Key.Substring(prefix.Length);
                    if (key.Length == 0 || (spec.TopLevelOnly && key.IndexOf('.') >= 0) || IsLabelMarkKey(spec, key))
                    {
                        continue;
                    }

                    if (seen.Add(key))
                    {
                        yield return (key, pair.Value);
                    }
                }
            }
        }

        private static Container ReadContainer(JsonElement json, string kind)
        {
            var container = new Container { Kind = kind, Label = Str(json, "label") };
            foreach (object item in ReadItems(json))
            {
                container.Items.Add(item);
            }

            return container;
        }

        private static IEnumerable<object> ReadItems(JsonElement json)
        {
            var items = new List<(double Position, int Order, object Item)>();
            int order = 0;
            foreach (JsonElement control in Array(json, "controls"))
            {
                Control c = ReadControl(control);
                items.Add((c.Position, order++, c));
            }

            foreach (JsonElement group in Array(json, "groups"))
            {
                Container g = ReadContainer(group, "Group");
                items.Add((Num(group, "position") ?? double.MaxValue, order++, g));
            }

            return items.OrderBy(i => i.Position).ThenBy(i => i.Order).Select(i => i.Item);
        }

        private static Control ReadControl(JsonElement json)
        {
            var control = new Control
            {
                Label = Str(json, "label"),
                Key = Str(json, "key"),
                Type = Str(json, "type"),
                EnableKey = Str(json, "enableKey"),
                ValueOn = Int(json, "valueOn"),
                ValueOff = Int(json, "valueOff"),
                DialogDefault = Int(json, "dialogDefault"),
                Confidence = Str(json, "confidence"),
                Format = Str(json, "format"),
                Position = Num(json, "position") ?? double.MaxValue,
                AidDialog = Str(json, "aidDialog"),
            };

            if (json.TryGetProperty("samples", out JsonElement samples) && samples.ValueKind == JsonValueKind.Object)
            {
                foreach ((string name, Dictionary<int, int> target) in new[] { ("inMenu", control.SamplesInMenu), ("outsideMenu", control.SamplesOutsideMenu) })
                {
                    if (samples.TryGetProperty(name, out JsonElement counts) && counts.ValueKind == JsonValueKind.Object)
                    {
                        foreach (JsonProperty count in counts.EnumerateObject())
                        {
                            if (int.TryParse(count.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) && count.Value.ValueKind == JsonValueKind.Number)
                            {
                                target[value] = count.Value.GetInt32();
                            }
                        }
                    }
                }
            }

            var values = new HashSet<int>();
            foreach (JsonElement option in Array(json, "options"))
            {
                int? value = Int(option, "value");
                if (value != null && values.Add(value.Value))
                {
                    control.Options.Add(new Option { Label = Str(option, "label"), Value = value.Value, ApiName = Str(option, "apiName"), Bitmap = Str(option, "bitmap"), Note = Str(option, "note") });
                }
            }

            return control;
        }

        private static bool ReplaceButton(Container container, string label, Container dialog)
        {
            for (int i = 0; i < container.Items.Count; i++)
            {
                if (container.Items[i] is Control control && control.Key == null && control.Type == "button" && control.Label == label)
                {
                    container.Items[i] = dialog;
                    return true;
                }

                if (container.Items[i] is Container child && ReplaceButton(child, label, dialog))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Removes controls that write no key (tables, buttons, selectors) and containers left empty.</summary>
        private static void Prune(Container container)
        {
            container.Items.RemoveAll(item => item is Control control && (control.Key == null || control.Type == "table" || control.Type == "button"));
            foreach (Container child in container.Items.OfType<Container>())
            {
                Prune(child);
            }

            container.Items.RemoveAll(item => item is Container child && child.Items.Count == 0);
        }

        private static void Resolve(PageSpec spec, Control control, AttributeCollection vi, AttributeCollection gd)
        {
            AttributeValueKind? kind = SampleKind(spec, control.Key, vi, gd);
            switch (control.Type)
            {
                case "toggle":
                    control.ClrType = "bool";
                    control.BoolOn = control.ValueOn ?? 1;
                    control.BoolOff = control.ValueOff ?? 0;
                    AdoptOnValueOfFiles(control);
                    return;
                case "option" when control.Options.Count > 0:
                    if (TryBool(control))
                    {
                        AdoptOnValueOfFiles(control);
                        return;
                    }

                    control.ClrType = "enum";
                    control.EnumName = RegisterEnum(spec, control);
                    return;
                case "color":
                    control.ClrType = "global::TeklaHelper.DrawingAttribute.DrawingColor";
                    return;
                case "linetype":
                    control.ClrType = "global::TeklaHelper.DrawingAttribute.DrawingLineType";
                    return;
                case "text":
                case "font":
                case "hatch":
                case "string":
                    control.ClrType = "string";
                    return;
                case "double":
                    control.ClrType = "double";
                    return;
                case "int":
                    control.ClrType = "int";
                    return;
                case "number":
                    control.ClrType = kind == AttributeValueKind.Double ? "double"
                        : kind == AttributeValueKind.Integer ? "int"
                        : kind == AttributeValueKind.String ? "string"
                        : control.Format != null && control.Format.Contains("f") ? "double" : "int";
                    return;
                default:
                    control.ClrType = kind == AttributeValueKind.Double ? "double" : kind == AttributeValueKind.Integer ? "int" : "string";
                    return;
            }
        }

        private static AttributeValueKind? SampleKind(PageSpec spec, string key, AttributeCollection vi, AttributeCollection gd)
        {
            if (spec.ViewPrefix != null && vi.TryGetValue(spec.ViewPrefix + key, out AttributeValue value))
            {
                return value.Kind;
            }

            if (spec.GADrawingPrefix != null && gd.TryGetValue(spec.GADrawingPrefix + key, out value))
            {
                return value.Kind;
            }

            if (spec.GADrawingPrefix == null && spec.AssemblyDrawingPrefix != null && SampleAd.TryGetValue(spec.AssemblyDrawingPrefix + key, out value))
            {
                return value.Kind;
            }

            return null;
        }

        /// <summary>
        /// A control copied from another page can store "on" differently there: <c>for_main</c> is 2 in the bolt mark menu
        /// but 1 in 260 part mark files. When the files of this page mostly hold another non-off value, write that one.
        /// </summary>
        private static void AdoptOnValueOfFiles(Control control)
        {
            KeyValuePair<int, int> common = control.SamplesOutsideMenu.Where(pair => pair.Key != control.BoolOff)
                .OrderByDescending(pair => pair.Value).FirstOrDefault();
            int onCount = control.SamplesInMenu.TryGetValue(control.BoolOn, out int count) ? count : 0;
            if (common.Value > onCount)
            {
                control.BoolOn = common.Key;
            }
        }

        /// <summary>The value a missing key reads as: the dialog's default, else the value most files hold.</summary>
        private static int? MissingValue(Control control)
        {
            if (control.DialogDefault != null)
            {
                return control.DialogDefault;
            }

            KeyValuePair<int, int> common = control.SamplesInMenu.Concat(control.SamplesOutsideMenu).OrderByDescending(pair => pair.Value).FirstOrDefault();
            return common.Value > 0 ? common.Key : (int?)null;
        }

        private static bool TryBool(Control control)
        {
            if (control.Options.Count != 2)
            {
                return false;
            }

            string[] yes = { "yes", "on", "visible", "show" };
            string[] no = { "no", "off", "not visible", "hide" };
            Option on = control.Options.FirstOrDefault(o => yes.Contains((o.Label ?? string.Empty).ToLowerInvariant()));
            Option off = control.Options.FirstOrDefault(o => no.Contains((o.Label ?? string.Empty).ToLowerInvariant()));
            if (on == null || off == null || on == off)
            {
                return false;
            }

            control.ClrType = "bool";
            control.BoolOn = on.Value;
            control.BoolOff = off.Value;
            return true;
        }

        private static string RegisterEnum(PageSpec spec, Control control)
        {
            string signature = string.Join("|", control.Options.Select(o => o.Value.ToString(CultureInfo.InvariantCulture) + ":" + o.Label));
            if (!EnumsBySignature.TryGetValue(signature, out EnumModel model))
            {
                string label = TrimLabel(control.Label);
                string own = Pascal(label);
                string group = Pascal(GroupOf(control));

                // "Representation" in the group "Representation" is one word, not two
                string withGroup = group.EndsWith(own, StringComparison.Ordinal) ? group : group + own;
                string first = own.Length < 2 || GenericLabels.Contains(label) ? withGroup : own;
                string stem = PageStem(spec.Class);
                string[] candidates = control.Origin == null
                    ? new[] { first, withGroup, stem + first, KeyPascal(control.Key), stem + KeyPascal(control.Key) }
                    // controls of other pages or older dialogs: their label alone says little ("Grid"), the key says more
                    : new[] { KeyPascal(control.Key), stem + KeyPascal(control.Key), first, withGroup };
                string name = candidates
                    .Select(candidate => EnumRenames.TryGetValue(candidate, out string renamed) ? renamed : candidate)
                    .Where(candidate => candidate.Length >= 2)
                    .FirstOrDefault(candidate => !EnumsByName.ContainsKey(candidate)) ?? stem + KeyPascal(control.Key);

                for (int n = 2; EnumsByName.ContainsKey(name); n++)
                {
                    name = name.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9') + n.ToString(CultureInfo.InvariantCulture);
                }

                model = new EnumModel { Name = name, Description = label };
                var used = new HashSet<string>(StringComparer.Ordinal);
                foreach (Option option in control.Options)
                {
                    string member = MemberName(option);
                    if (!used.Add(member))
                    {
                        member = member + Math.Abs(option.Value).ToString(CultureInfo.InvariantCulture);
                        used.Add(member);
                    }

                    model.Members.Add((member, option.Value, option.Label));
                }

                EnumsBySignature[signature] = model;
                EnumsByName[name] = model;
                EnumKeys[name] = new SortedSet<string>(StringComparer.Ordinal);
            }

            // Values Tekla writes that the menu does not offer (vg.GridObjectGridOn 0, DrawingWeld.DrawAs 0, ...): a member
            // each, so that they have a name
            foreach (KeyValuePair<int, int> pair in control.SamplesOutsideMenu.Where(pair => pair.Value >= MinimumFilesForMember).OrderBy(pair => pair.Key))
            {
                if (model.Members.All(member => member.Value != pair.Key))
                {
                    model.Members.Add(("Value" + (pair.Key < 0 ? "Minus" : string.Empty) + Math.Abs(pair.Key).ToString(CultureInfo.InvariantCulture), pair.Key, null));
                    model.FromFiles.Add(pair.Key);
                }
            }

            EnumKeys[model.Name].Add(spec.Class + ": " + control.Key);
            return model.Name;
        }

        private static string PageStem(string pageClass)
        {
            foreach (string suffix in new[] { "Object", "Mark", "Page" })
            {
                if (pageClass.EndsWith(suffix, StringComparison.Ordinal) && pageClass.Length > suffix.Length)
                {
                    return pageClass.Substring(0, pageClass.Length - suffix.Length) + (suffix == "Mark" ? "Mark" : string.Empty);
                }
            }

            return pageClass;
        }

        // The label of the group a control is in is not kept on the control; the generator keeps a lookup while building
        private static readonly Dictionary<Control, string> GroupLabels = new Dictionary<Control, string>();

        private static string GroupOf(Control control) => GroupLabels.TryGetValue(control, out string label) ? label : string.Empty;

        private static void RecordGroupLabels(Container container)
        {
            foreach (object item in container.Items)
            {
                if (item is Control control)
                {
                    GroupLabels[control] = container.Kind == "Page" ? string.Empty : container.Label ?? string.Empty;
                }
                else
                {
                    RecordGroupLabels((Container)item);
                }
            }
        }

        private static string MemberName(Option option)
        {
            string label = option.Label ?? string.Empty;
            bool picture = label.Length == 0 || System.Text.RegularExpressions.Regex.IsMatch(label, "^[a-z]+(_[a-z0-9]+)+$") || label.StartsWith("SYMBOL_BMP", StringComparison.Ordinal);
            string name = !string.IsNullOrEmpty(option.ApiName) && IsIdentifier(option.ApiName) ? option.ApiName
                : picture ? (option.Bitmap != null && PictureMembers.TryGetValue(option.Bitmap, out string member) ? member : string.Empty)
                : Pascal(label);
            if (name.Length == 0 || char.IsDigit(name[0]))
            {
                name = "Value" + (option.Value < 0 ? "Minus" : string.Empty) + Math.Abs(option.Value).ToString(CultureInfo.InvariantCulture);
            }

            return name;
        }

        private static void AssignNames(Container container, HashSet<string> enclosing, HashSet<string> reserved = null, bool drawingPage = false)
        {
            var used = new HashSet<string>(reserved ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
            int groupIndex = 0;
            foreach (object item in container.Items)
            {
                if (item is Control control)
                {
                    string name = Pascal(TrimLabel(control.Label));

                    // In the Advanced group a generic label ("Color") does not say which setting it is, and which control
                    // gets it would depend on the order of the dialog: the key says it for good (ReferenceColour).
                    // Controls of another dialog sit in the groups of that dialog, where the label says it.
                    bool generic = control.Origin != null && control.Origin != "other" && GenericLabels.Contains(TrimLabel(control.Label));
                    if (name.Length < 2 || control.Origin == "file" || generic)
                    {
                        // A nested key repeats its struct (SectionMark.SectionMarkLeftSymbolPositionX): the drawing pages name
                        // a control by the last part of its key
                        name = KeyPascal(drawingPage ? LastSegment(control.Key) : control.Key);
                    }

                    name = Unique(name, used, enclosing, () => KeyPascal(control.Key));
                    control.PropertyName = name;
                }
                else if (item is Container child)
                {
                    groupIndex++;
                    string name = Pascal(TrimLabel(child.Label));
                    if (name.Length < 2)
                    {
                        name = child.Kind + groupIndex.ToString(CultureInfo.InvariantCulture);
                    }

                    name = Unique(name, used, enclosing, () => name + child.Kind);
                    child.PropertyName = name;
                    child.ClassName = Unique(name + child.Kind, used, enclosing, () => name + child.Kind + "Section");
                    var inner = new HashSet<string>(enclosing, StringComparer.Ordinal) { child.ClassName };
                    AssignNames(child, inner, null, drawingPage);
                }
            }
        }

        private static string Unique(string name, HashSet<string> used, HashSet<string> enclosing, Func<string> alternative)
        {
            if (Reserved.Contains(name) || enclosing.Contains(name) || used.Contains(name))
            {
                string other = alternative();
                name = other != name && !Reserved.Contains(other) && !enclosing.Contains(other) && !used.Contains(other) ? other : name + "Value";
                for (int n = 2; Reserved.Contains(name) || enclosing.Contains(name) || used.Contains(name); n++)
                {
                    name = name.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9') + n.ToString(CultureInfo.InvariantCulture);
                }
            }

            used.Add(name);
            return name;
        }

        // ---------------------------------------------------------------- code

        // The assembly drawing file saved from the dialog, for default values
        private static AttributeCollection SampleAd;

        private static string WritePage(PageSpec spec, Container page, AttributeCollection vi, AttributeCollection gd)
        {
            CurrentPage = spec.Class;
            var w = new CodeWriter();
            w.Line("// <auto-generated>");
            w.Line($"//     Generated by tools/DialogClassGenerator from tools/DialogClassGenerator/Dialogs/{spec.Json}.json, the decoded");
            w.Line("//     Tekla Structures View properties dialog (see CLAUDE.md for the version), files saved from its dialogs and the files");
            w.Line("//     shipped with Tekla. Do not edit: change the generator or its input and run it again (see CLAUDE.md).");
            w.Line("// </auto-generated>");
            w.Line();
            w.Line($"namespace {spec.Namespace}");
            w.Open();
            if (!spec.HandWrittenPart)
            {
                w.Summary(PageSummary(spec));
            }

            w.Line($"public sealed partial class {spec.Class} : global::TeklaHelper.DrawingAttribute.AttributeSection");
            w.Open();
            WriteConstant(w, "ViewPrefix", spec.ViewPrefix, "Prefix of the page in view attribute files (.vi).");
            WriteConstant(w, "GADrawingPrefix", spec.GADrawingPrefix, "Prefix of the page in GA (.gd) and multi (.md) drawing attribute files.");
            WriteConstant(w, "AssemblyDrawingPrefix", spec.AssemblyDrawingPrefix, "Prefix of the page in assembly (.ad), single-part (.wd) and cast unit (.cud) drawing attribute files.");
            WriteConstant(w, "ViewFileExtension", spec.ViewFile, "Extension of the file the page's Save button writes in the view properties (only for the dialogs; the Open API reads the .vi file).");
            WriteConstant(w, "GADrawingFileExtension", spec.GADrawingFile, "Extension of the page file in GA drawing properties (matched by key set; only for the dialogs).");
            WriteConstant(w, "AssemblyDrawingFileExtension", spec.AssemblyDrawingFile, "Extension of the page file in assembly drawing properties (matched by key set; only for the dialogs).");
            string Constant(string name, string value) => value == null ? "null" : name;
            w.Summary("Returns the prefix of the page in a file of <paramref name=\"host\"/>; null when that kind of file has no such page.");
            w.Line($"public static string PrefixFor(global::TeklaHelper.DrawingAttribute.Pages.PageHost host) => global::TeklaHelper.DrawingAttribute.Pages.PageHostPrefix.Select(host, {Constant("ViewPrefix", spec.ViewPrefix)}, {Constant("GADrawingPrefix", spec.GADrawingPrefix)}, {Constant("AssemblyDrawingPrefix", spec.AssemblyDrawingPrefix)});");
            if (!spec.HandWrittenPart)
            {
                w.Line();
                w.Summary("Creates the page over the keys of <paramref name=\"values\"/> under <paramref name=\"prefix\"/>.");
                w.Line("/// <param name=\"values\">Values of the attribute file.</param>");
                string example = spec.ViewPrefix != null ? "ViewPrefix" : spec.GADrawingPrefix != null ? "GADrawingPrefix" : "AssemblyDrawingPrefix";
                w.Line($"/// <param name=\"prefix\">Prefix of the page in that file, e.g. <see cref=\"{example}\"/>.</param>");
                w.Line($"public {spec.Class}(global::TeklaHelper.DrawingAttribute.AttributeCollection values, string prefix)");
                w.Line("    : base(values, prefix)");
                w.Open();
                w.Close();
            }

            WriteMembers(w, page);
            foreach ((string property, string keyStem, string summary, int row) in LabelMarkProperties(spec))
            {
                string field = "_" + char.ToLowerInvariant(property[0]) + property.Substring(1);
                w.Line();
                w.Line($"private global::TeklaHelper.DrawingAttribute.Pages.ViewLabelMark {field};");
                w.Line();
                w.Summary($"Row A{row.ToString(CultureInfo.InvariantCulture)} of the {Xml(summary)} texts: the dialog of its \"...\" button (<c>{keyStem}*</c>).");
                w.Line($"public global::TeklaHelper.DrawingAttribute.Pages.ViewLabelMark {property} => {field} ?? ({field} = new global::TeklaHelper.DrawingAttribute.Pages.ViewLabelMark(Values, Prefix + \"{keyStem}\"));");
            }

            if (spec.FileInKey != "in_file" || spec.FileOutKey != "out_file")
            {
                w.Line();
                w.Line("/// <inheritdoc />");
                w.Line($"private protected override string PageFileInKey => \"{spec.FileInKey}\";");
                w.Line();
                w.Line("/// <inheritdoc />");
                w.Line($"private protected override string PageFileOutKey => \"{spec.FileOutKey}\";");
            }

            List<Control> controls = AllControls(page).ToList();
            IEnumerable<string> fileEnableKeys = FileEnableKeys.Where(item => item.Page == spec.Class).Select(item => item.Key);
            List<string> enableKeys = controls.Where(c => c.EnableKey != null && c.Origin == null).Select(c => c.EnableKey).Concat(fileEnableKeys).Distinct().ToList();
            WriteSetDefaults(w, spec, page, enableKeys, vi, gd, SampleAd);
            w.Line();
            w.Summary("Sets every check box of the page (the boxes in front of the controls, keys <c>*_en</c>). They decide what the dialog's Modify button applies to existing objects; loading the file into a view through the Open API applies every value whatever the boxes say.");
            w.Line("public void SetAllEnabled(bool enabled)");
            w.Open();
            w.Line("foreach (string key in EnableKeys)");
            w.Open();
            w.Line("SetBoolean(key, enabled);");
            w.Close();
            w.Close();
            w.Line();
            w.Summary("Keys the properties of the page read and write, relative to the page prefix.");
            w.Line("internal static readonly string[] Keys =");
            w.Open();
            foreach (string key in controls.Select(c => c.Key).Concat(spec.HandWrittenKeys).Distinct())
            {
                w.Line($"\"{key}\",");
            }

            w.Close(";");
            w.Line();
            w.Summary("The check boxes (<c>*_en</c>) of the controls of the page.");
            w.Line("internal static readonly string[] EnableKeys =");
            w.Open();
            foreach (string key in enableKeys)
            {
                w.Line($"\"{key}\",");
            }

            w.Close(";");
            WriteNestedClasses(w, page, spec);
            w.Close();
            w.Close();
            return w.ToString();
        }

        private static void WriteConstant(CodeWriter w, string name, string value, string summary)
        {
            if (value == null)
            {
                return;
            }

            w.Summary(summary);
            w.Line($"public const string {name} = \"{value}\";");
            w.Line();
        }

        private static void WriteMembers(CodeWriter w, Container container)
        {
            foreach (object item in container.Items)
            {
                w.Line();
                if (item is Container child)
                {
                    string field = "_" + char.ToLowerInvariant(child.PropertyName[0]) + child.PropertyName.Substring(1);
                    w.Line($"private {child.ClassName} {field};");
                    w.Line();
                    w.Summary(ContainerSummary(child));
                    w.Line($"public {child.ClassName} {child.PropertyName} => {field} ?? ({field} = new {child.ClassName}(Values, Prefix));");
                }
                else
                {
                    WriteProperty(w, (Control)item);
                }
            }
        }

        private static void WriteNestedClasses(CodeWriter w, Container container, PageSpec spec)
        {
            foreach (Container child in container.Items.OfType<Container>())
            {
                w.Line();
                w.Summary(ContainerSummary(child));
                w.Line($"public sealed class {child.ClassName} : global::TeklaHelper.DrawingAttribute.AttributeSection");
                w.Open();
                w.Line($"internal {child.ClassName}(global::TeklaHelper.DrawingAttribute.AttributeCollection values, string prefix)");
                w.Line("    : base(values, prefix)");
                w.Open();
                w.Close();
                WriteMembers(w, child);
                WriteNestedClasses(w, child, spec);
                w.Close();
            }
        }

        private static void WriteProperty(CodeWriter w, Control control)
        {
            w.Summary(ControlSummary(control));
            string key = control.Key;
            switch (control.ClrType)
            {
                case "bool" when control.BoolOn == 1 && control.BoolOff == 0:
                    w.Line($"public bool {control.PropertyName}");
                    w.Open();
                    w.Line(MissingValue(control) is int missing && missing != 0 ? $"get => GetBoolean(\"{key}\", true);" : $"get => GetBoolean(\"{key}\");");
                    w.Line($"set => SetBooleanUnlessSame(\"{key}\", value, {control.BoolOn}, {control.BoolOff});");
                    w.Close();
                    break;
                case "bool":
                    w.Line($"public bool {control.PropertyName}");
                    w.Open();
                    w.Line($"get => GetInt32(\"{key}\", {MissingValue(control) ?? control.BoolOff}) != {control.BoolOff};");
                    w.Line($"set => SetBooleanUnlessSame(\"{key}\", value, {control.BoolOn}, {control.BoolOff});");
                    w.Close();
                    break;
                case "enum":
                    string type = "global::TeklaHelper.DrawingAttribute.Pages." + control.EnumName;
                    w.Line($"public {type} {control.PropertyName}");
                    w.Open();
                    w.Line($"get => GetEnum(\"{key}\", ({type}){(MissingValue(control) ?? control.Options[0].Value)});");
                    w.Line($"set => SetEnum(\"{key}\", value);");
                    w.Close();
                    break;
                case "global::TeklaHelper.DrawingAttribute.DrawingColor":
                case "global::TeklaHelper.DrawingAttribute.DrawingLineType":
                    w.Line($"public {control.ClrType} {control.PropertyName}");
                    w.Open();
                    w.Line($"get => GetEnum(\"{key}\", ({control.ClrType}){(MissingValue(control) ?? (control.Type == "color" ? (control.Options.Count > 0 ? control.Options[0].Value : 153) : 1))});");
                    w.Line($"set => SetEnum(\"{key}\", value);");
                    w.Close();
                    break;
                case "double":
                    w.Line($"public double {control.PropertyName}");
                    w.Open();
                    w.Line($"get => GetDouble(\"{key}\");");
                    w.Line($"set => SetDouble(\"{key}\", value);");
                    w.Close();
                    break;
                case "int":
                    w.Line($"public int {control.PropertyName}");
                    w.Open();
                    w.Line($"get => GetInt32(\"{key}\");");
                    w.Line($"set => SetInt32(\"{key}\", value);");
                    w.Close();
                    break;
                default:
                    w.Line($"public string {control.PropertyName}");
                    w.Open();
                    w.Line($"get => GetString(\"{key}\");");
                    w.Line($"set => SetString(\"{key}\", value);");
                    w.Close();
                    break;
            }
        }

        private static string PageSummary(PageSpec spec)
        {
            var hosts = new List<string>();
            if (spec.ViewPrefix != null)
            {
                hosts.Add(spec.ViewPrefix.Length == 0 ? "the top-level keys of .vi files" : $"<c>{spec.ViewPrefix}*</c> in .vi files");
            }

            if (spec.GADrawingPrefix != null)
            {
                hosts.Add($"<c>{spec.GADrawingPrefix}*</c> in GA and multi drawing files");
            }

            if (spec.AssemblyDrawingPrefix != null)
            {
                hosts.Add($"<c>{spec.AssemblyDrawingPrefix}*</c> in assembly, single-part and cast unit drawing files");
            }

            if (spec.DrawingPage)
            {
                return $"{spec.Description}, with its tabs and group boxes as nested classes: {string.Join(", ", hosts)}. Every key of the page in files saved from the Tekla 2025 dialogs has a property; those the dialog does not show are in <c>Advanced</c>, and any other key is reachable by name (<c>GetInt32</c>, <c>SetString</c>, ...).";
            }

            return $"The \"{Xml(spec.Title)}\" page of the Tekla View properties dialog, with its tabs and group boxes as nested classes: {string.Join(", ", hosts)}. Every key of the page in files saved from the Tekla 2025 dialogs has a property; those the dialog does not show are in <c>Advanced</c>, and any other key is reachable by name (<c>GetInt32</c>, <c>SetString</c>, ...).";
        }

        private static string ContainerSummary(Container container)
        {
            switch (container.Kind)
            {
                case "Tab":
                    return $"The \"{Xml(container.Label)}\" tab.";
                case "Dialog":
                    return $"The dialog opened by the \"{Xml(container.Label)}...\" button.";
                default:
                    return container.Description ?? $"The \"{Xml(container.Label)}\" group.";
            }
        }

        private static string ControlSummary(Control control)
        {
            var text = new StringBuilder();
            text.Append(control.Origin == "file" ? "Key" : Xml(TrimLabel(control.Label)));
            text.Append($" (<c>{Xml(control.Key)}</c>).");
            if (KeysAddedIn.TryGetValue(control.Key, out string version))
            {
                text.Append($" New in Tekla Structures {version}: the dialogs of earlier versions do not have it.");
            }

            if (KeyNotes.TryGetValue(CurrentPage + ":" + control.Key, out string note))
            {
                text.Append(' ').Append(note);
            }

            if (control.ClrType == "bool" && (control.BoolOn != 1 || control.BoolOff != 0))
            {
                text.Append($" Stored as {control.BoolOn} (true) and {control.BoolOff} (false).");
            }

            List<Option> uncertain = control.Options.Where(option => !string.IsNullOrEmpty(option.Note)).ToList();
            foreach (Option option in uncertain)
            {
                text.Append($" {Xml(option.Label)} = {option.Value.ToString(CultureInfo.InvariantCulture)} is not certain: {Xml(option.Note)}.");
            }

            if (control.Confidence == "order" && uncertain.Count == 0)
            {
                text.Append(" The stored values follow the order of the menu: Tekla's dialog definition does not list them.");
            }

            switch (control.Origin)
            {
                case "sibling":
                    text.Append(" The dialog shows it on another page with the same keys, not on this one.");
                    break;
                case "legacy":
                    text.Append(" Shown in an older Tekla dialog, not in the current View properties.");
                    break;
                case "other":
                    text.Append($" Shown in another Tekla dialog with the same keys (<c>{Xml(control.AidDialog)}</c>).");
                    break;
                case "file":
                    text.Append(" Not shown in the dialog.");
                    break;
            }

            return text.ToString();
        }

        private static string WriteEnums()
        {
            var w = new CodeWriter();
            w.Line("// <auto-generated>");
            w.Line("//     Generated by tools/DialogClassGenerator from the option menus of the decoded Tekla Structures View");
            w.Line("//     properties dialog. Do not edit: change the generator or its input and run it again (see CLAUDE.md).");
            w.Line("// </auto-generated>");
            w.Line();
            w.Line("namespace TeklaHelper.DrawingAttribute.Pages");
            w.Open();
            bool first = true;
            foreach (EnumModel model in EnumsByName.Values.OrderBy(m => m.Name, StringComparer.Ordinal))
            {
                if (!first)
                {
                    w.Line();
                }

                first = false;
                string keys = string.Join(", ", EnumKeys[model.Name].Take(6));
                w.Summary($"Values of the \"{Xml(model.Description)}\" menu, as Tekla stores them. Used by {Xml(keys)}{(EnumKeys[model.Name].Count > 6 ? " and others" : string.Empty)}.");
                w.Line($"public enum {model.Name}");
                w.Open();
                foreach ((string name, int value, string label) in model.Members)
                {
                    w.Summary(model.FromFiles.Contains(value) ? "Not an item of the menu, but written by Tekla in many files; meaning not known." : string.IsNullOrEmpty(label) || name.StartsWith("Value", StringComparison.Ordinal) && label != name ? $"Item \"{Xml(label)}\" of the menu (a picture in the dialog)." : $"{Xml(label)}.");
                    w.Line($"{name} = {value.ToString(CultureInfo.InvariantCulture)},");
                    w.Line();
                }

                w.TrimBlankLine();
                w.Close();
            }

            w.Close();
            return w.ToString();
        }

        // ---------------------------------------------------------------- helpers

        private static IEnumerable<Control> AllControls(Container container)
        {
            foreach (object item in container.Items)
            {
                if (item is Control control)
                {
                    yield return control;
                }
                else
                {
                    foreach (Control nested in AllControls((Container)item))
                    {
                        yield return nested;
                    }
                }
            }
        }

        private static int CountControls(Container container) => AllControls(container).Count();

        private static bool IsEnableKey(string key)
        {
            string last = LastSegment(key);
            return last.EndsWith("_en", StringComparison.Ordinal) || last.EndsWith("_enable", StringComparison.Ordinal);
        }

        private static string LastSegment(string key) => key.Substring(key.LastIndexOf('.') + 1);

        private static string TrimLabel(string label) => (label ?? string.Empty).Trim().TrimEnd(':', '.').Trim();

        private static string Pascal(string text)
        {
            var builder = new StringBuilder();
            foreach (string word in System.Text.RegularExpressions.Regex.Split(text ?? string.Empty, "[^A-Za-z0-9]+"))
            {
                if (word.Length > 0)
                {
                    builder.Append(char.ToUpperInvariant(word[0])).Append(word.Substring(1));
                }
            }

            string name = builder.ToString();
            return name.Length > 0 && char.IsDigit(name[0]) ? string.Empty : name;
        }

        private static string KeyPascal(string key)
        {
            var builder = new StringBuilder();
            foreach (string word in key.Split(new[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries))
            {
                builder.Append(char.ToUpperInvariant(word[0])).Append(word.Substring(1));
            }

            string name = builder.ToString();
            return char.IsDigit(name[0]) ? "Key" + name : name;
        }

        private static bool IsIdentifier(string name) => System.Text.RegularExpressions.Regex.IsMatch(name, "^[A-Za-z_][A-Za-z0-9_]*$");

        private static string Xml(string text) => (text ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        private static IEnumerable<JsonElement> Array(JsonElement json, string name) =>
            json.ValueKind == JsonValueKind.Object && json.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.Array
                ? value.EnumerateArray()
                : Enumerable.Empty<JsonElement>();

        private static string Str(JsonElement json, string name) =>
            json.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

        private static int? Int(JsonElement json, string name) =>
            json.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int result) ? result : (int?)null;

        private static double? Num(JsonElement json, string name) =>
            json.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.Number ? value.GetDouble() : (double?)null;
    }
}
