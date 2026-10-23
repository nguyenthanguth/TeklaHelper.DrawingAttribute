using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TeklaHelper.DrawingAttribute;

namespace DialogClassGenerator
{
    /// <summary>
    /// The default values written by <c>SetDefaults()</c> of the pages and <c>CreateDefault()</c> of the files. For each key:
    /// the value most files shipped with Tekla hold (ASCII only, so that every code page can store it, in at least three
    /// files), else the dialog's own default, else the value of the file saved from the dialog, else a neutral value of the
    /// property's type.
    /// </summary>
    internal static partial class Program
    {
        // Legacy files have no encoding line and prefix every key with the dialog name
        private static readonly Dictionary<string, string> LegacyPrefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["vi"] = "view_dial.",
            ["gd"] = "gdraw_dial.",
            ["md"] = "mdraw_dial.",
            ["ad"] = "adraw_dial.",
            ["wd"] = "wdraw_dial.",
            ["cud"] = "cudraw_dial.",
        };

        // extension → key → file text of the value → number of files
        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, int>>> ValueCounts =
            new Dictionary<string, Dictionary<string, Dictionary<string, int>>>(StringComparer.OrdinalIgnoreCase);

        // extension → keys in the order they were first seen, and in how many files each is
        private static readonly Dictionary<string, List<string>> KeyOrder = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, int> FileCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // page class → its keys (relative to the page prefix) with their default values, and its check boxes
        private static readonly Dictionary<string, (List<string> Values, List<string> EnableKeys)> PageDefaults =
            new Dictionary<string, (List<string>, List<string>)>(StringComparer.Ordinal);

        private static void LoadStatistics(string environments)
        {
            foreach (string path in Directory.EnumerateFiles(environments, "*", SearchOption.AllDirectories))
            {
                string extension = Path.GetExtension(path).TrimStart('.');
                if (!LegacyPrefixes.ContainsKey(extension))
                {
                    continue;
                }

                AttributeFileContent content;
                try
                {
                    content = AttributeFileFormat.Read(path);
                }
                catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is FormatException)
                {
                    continue;
                }

                if (!content.HasEncodingLine)
                {
                    content.Values.StripPrefix(LegacyPrefixes[extension]);
                }

                extension = extension.ToLowerInvariant();
                if (!ValueCounts.TryGetValue(extension, out Dictionary<string, Dictionary<string, int>> keys))
                {
                    ValueCounts[extension] = keys = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
                    KeyOrder[extension] = new List<string>();
                    FileCounts[extension] = 0;
                }

                FileCounts[extension]++;
                foreach (KeyValuePair<string, AttributeValue> pair in content.Values)
                {
                    if (!keys.TryGetValue(pair.Key, out Dictionary<string, int> values))
                    {
                        keys[pair.Key] = values = new Dictionary<string, int>(StringComparer.Ordinal);
                        KeyOrder[extension].Add(pair.Key);
                    }

                    string text = pair.Value.ToFileText();
                    values[text] = values.TryGetValue(text, out int count) ? count + 1 : 1;
                }
            }

            Console.WriteLine("Statistics: " + string.Join(", ", FileCounts.OrderBy(p => p.Key).Select(p => $"{p.Value} .{p.Key}")));
        }

        /// <summary>A shipped value counts only when this many files of the type hold it: one odd file is no rule.</summary>
        private const int MinimumFilesForDefault = 3;

        /// <summary>
        /// The ASCII value most shipped files of <paramref name="extension"/> hold for <paramref name="key"/>; only values that
        /// at least <see cref="MinimumFilesForDefault"/> files hold count, so null when no value is that common. Unquoted values must be finite numbers (one shipped
        /// file holds <c>inf</c>).
        /// </summary>
        private static string MostCommon(string extension, string key)
        {
            if (key == null || !ValueCounts.TryGetValue(extension, out Dictionary<string, Dictionary<string, int>> keys)
                || !keys.TryGetValue(key, out Dictionary<string, int> values))
            {
                return null;
            }

            return values.Where(pair => pair.Value >= MinimumFilesForDefault && IsAscii(pair.Key) && IsValidValue(pair.Key) && IsUsefulValue(key, pair.Key))
                .OrderByDescending(pair => pair.Value)
                .ThenBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => pair.Key)
                .FirstOrDefault();
        }

        private static bool IsAscii(string text) => text.All(c => c < 128);

        /// <summary>
        /// Text without a font, with height 0 or with colour 0 is never a useful default; many shipped files hold such
        /// values for the reinforcement marks (<c>NeighborRebarMark.FontAttr.*</c>), where the dialog files hold
        /// <c>romsim</c>, 2.5 and 155.
        /// </summary>
        private static bool IsUsefulValue(string key, string text)
        {
            string last = LastSegment(key);
            if (last == "font_name" || last == "FontName")
            {
                return text != "\"\"";
            }

            if (last == "height" || last == "text_height" || last == "text_colour")
            {
                return AttributeValue.Parse(text).AsDouble(0) != 0;
            }

            return true;
        }

        private static bool IsValidValue(string text) =>
            text.StartsWith("\"", StringComparison.Ordinal)
            || (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number) && !double.IsInfinity(number) && !double.IsNaN(number))
            || AttributeValue.Parse(text).AsInt32(int.MinValue) != int.MinValue;

        /// <summary>
        /// Keys that name a resource of the environment (a layout, an attribute file) or hold the user's text: the most
        /// common shipped value would name something only some environments have. Extension → key → value.
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, string>> EnvironmentFreeValues = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal)
        {
            ["vi"] = Values(("StyleName", "standard")),
            ["gd"] = Values(("dl.layout", "ga"), ("StyleName", "standard"), ("description", ""), ("title_1", ""), ("title_2", ""), ("title_3", "")),
            ["md"] = Values(("dl.layout", "multi"), ("StyleName", "standard"), ("description", ""), ("title_1", ""), ("title_2", ""), ("title_3", "")),
            ["ad"] = Values(("dl.layout", "assembly"), ("dl.single_attrib_file", "standard"), ("StyleName", "standard"), ("description", ""), ("title_1", ""), ("title_2", ""), ("title_3", "")),
            ["wd"] = Values(("dl.layout", "single"), ("dl.single_attrib_file", "standard"), ("StyleName", "standard"), ("description", ""), ("title_1", ""), ("title_2", ""), ("title_3", "")),
            ["cud"] = Values(("dl.layout", "cu"), ("dl.single_attrib_file", "standard"), ("StyleName", "standard"), ("description", ""), ("title_1", ""), ("title_2", ""), ("title_3", "")),
        };

        private static Dictionary<string, string> Values(params (string Key, string Text)[] values) =>
            values.ToDictionary(v => v.Key, v => AttributeValue.FromString(v.Text).ToFileText(), StringComparer.Ordinal);

        // A file type whose shipped files are few falls back to the type with the same keys
        private static readonly Dictionary<string, string> SiblingTypes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["md"] = "gd",
            ["wd"] = "ad",
            ["cud"] = "ad",
        };

        /// <summary>
        /// The default value of a control, as file text: the value most shipped files hold first (the environments shipped
        /// with Tekla agree with each other more than with the dialog's declared defaults: the line type menus declare their
        /// first item, Solid, where every shipped file holds another type), then the dialog's own default.
        /// </summary>
        private static string ControlDefault(PageSpec spec, Control control, AttributeCollection vi, AttributeCollection gd, AttributeCollection ad)
        {
            string common = MostCommon("vi", Prefixed(spec.ViewPrefix, control.Key))
                ?? MostCommon("gd", Prefixed(spec.GADrawingPrefix, control.Key))
                ?? MostCommon("ad", Prefixed(spec.AssemblyDrawingPrefix, control.Key));
            if (common != null)
            {
                return common;
            }

            if (control.DialogDefault is int dialogDefault && control.ClrType != "string")
            {
                switch (control.ClrType)
                {
                    case "double":
                        return AttributeValue.FromDouble(dialogDefault).ToFileText();
                    case "bool":
                        return (dialogDefault != control.BoolOff ? control.BoolOn : control.BoolOff).ToString(CultureInfo.InvariantCulture);
                    default:
                        return dialogDefault.ToString(CultureInfo.InvariantCulture);
                }
            }

            foreach ((AttributeCollection file, string prefix) in new[] { (vi, spec.ViewPrefix), (gd, spec.GADrawingPrefix), (ad, spec.AssemblyDrawingPrefix) })
            {
                if (prefix != null && file.TryGetValue(prefix + control.Key, out AttributeValue value) && IsAscii(value.ToFileText()))
                {
                    return value.ToFileText();
                }
            }

            switch (control.ClrType)
            {
                case "bool":
                    return control.BoolOff.ToString(CultureInfo.InvariantCulture);
                case "enum":
                    return control.Options[0].Value.ToString(CultureInfo.InvariantCulture);
                case "global::TeklaHelper.DrawingAttribute.DrawingColor":
                    return control.Options.Count > 0 ? control.Options[0].Value.ToString(CultureInfo.InvariantCulture) : "153";
                case "global::TeklaHelper.DrawingAttribute.DrawingLineType":
                    return "1";
                case "double":
                    return "0.000000";
                case "int":
                    return "0";
                default:
                    return "\"\"";
            }
        }

        private static string Prefixed(string prefix, string key) => prefix == null ? null : prefix + key;

        /// <summary>Writes <c>SetDefaults()</c> of a page: one assignment per property, then every check box ticked.</summary>
        private static void WriteSetDefaults(CodeWriter w, PageSpec spec, Container page, List<string> enableKeys, AttributeCollection vi, AttributeCollection gd, AttributeCollection ad)
        {
            var keys = new List<string>();
            w.Line();
            w.Summary("Sets every property of the page to its default and ticks every check box. A default is the value most files shipped with Tekla hold (at least three), else the dialog's own default, else the value of a file saved from the dialog."
                + (spec.HandWrittenPart ? " The View creation table and the 3D view properties file are left as they are: their shipped values name files of an environment." : string.Empty));
            w.Line("public void SetDefaults()");
            w.Open();
            foreach ((string property, string key, string type) in spec.HandWrittenDefaults)
            {
                keys.Add(key);
                w.Line($"{property} = {HandWrittenExpression(spec, key, type)};");
            }
            foreach ((string path, Control control) in ControlPaths(page, string.Empty))
            {
                if (keys.Contains(control.Key))
                {
                    continue;
                }

                keys.Add(control.Key);
                w.Line($"{path} = {Expression(control, ControlDefault(spec, control, vi, gd, ad))};");
            }

            foreach ((string property, _, _, _) in LabelMarkProperties(spec))
            {
                w.Line($"{property}.SetDefaults();");
            }

            w.Line();
            w.Line("SetAllEnabled(true);");
            w.Close();
            PageDefaults[spec.Class] = (keys, enableKeys);
        }

        /// <summary>The default of a property of the hand-written part of a page, as a C# expression.</summary>
        private static string HandWrittenExpression(PageSpec spec, string key, string type)
        {
            string text = MostCommon("gd", Prefixed(spec.GADrawingPrefix, key)) ?? MostCommon("ad", Prefixed(spec.AssemblyDrawingPrefix, key))
                ?? throw new InvalidOperationException($"No shipped value for the hand-written key {key} of {spec.Class}.");
            AttributeValue value = AttributeValue.Parse(text);
            if (type.EndsWith(".ViewCreation", StringComparison.Ordinal))
            {
                string[] names = { "Off", "Auto", "On" };
                int number = value.AsInt32();
                return Named(type, number, number >= 0 && number < names.Length ? names[number] : null);
            }

            if (type == "bool")
            {
                return value.AsInt32() != 0 ? "true" : "false";
            }

            return Expression(new Control { ClrType = type }, text);
        }

        /// <summary>The value properties of a page with their path from the page (<c>Content.Representation.Representation</c>).</summary>
        private static IEnumerable<(string Path, Control Control)> ControlPaths(Container container, string path)
        {
            foreach (object item in container.Items)
            {
                if (item is Control control)
                {
                    yield return (path + control.PropertyName, control);
                }
                else
                {
                    var child = (Container)item;
                    foreach ((string Path, Control Control) nested in ControlPaths(child, path + child.PropertyName + "."))
                    {
                        yield return nested;
                    }
                }
            }
        }

        private static readonly Dictionary<int, string> ColorNames = new Dictionary<int, string>
        {
            [0] = "None", [130] = "Gray30", [131] = "Gray50", [132] = "Gray70", [133] = "Gray90", [152] = "Invisible", [153] = "Black",
            [154] = "NewLine1", [155] = "NewLine2", [156] = "NewLine3", [157] = "NewLine4", [158] = "NewLine5", [159] = "NewLine6",
            [160] = "Red", [161] = "Green", [162] = "Blue", [163] = "Cyan", [164] = "Yellow", [165] = "Magenta",
        };

        private static readonly string[] LineTypeNames = { "Undefined", "Solid", "Dashed", "Slashed", "DashDot", "Dotted", "DashDoubleDot", "SlashDash" };

        /// <summary>The C# expression of a default value (file text) for the type of the property.</summary>
        private static string Expression(Control control, string text)
        {
            AttributeValue value = AttributeValue.Parse(text);
            switch (control.ClrType)
            {
                case "bool":
                    return value.AsInt32(control.BoolOff) != control.BoolOff ? "true" : "false";
                case "enum":
                    return Named("global::TeklaHelper.DrawingAttribute.Pages." + control.EnumName, value.AsInt32(),
                        EnumsByName[control.EnumName].Members.Where(m => m.Value == value.AsInt32()).Select(m => m.Name).FirstOrDefault());
                case "global::TeklaHelper.DrawingAttribute.DrawingColor":
                    return Named(control.ClrType, value.AsInt32(), ColorNames.TryGetValue(value.AsInt32(), out string color) ? color : null);
                case "global::TeklaHelper.DrawingAttribute.DrawingLineType":
                    int lineType = value.AsInt32();
                    return Named(control.ClrType, lineType, lineType >= 0 && lineType < LineTypeNames.Length ? LineTypeNames[lineType] : null);
                case "double":
                    return value.AsDouble().ToString("R", CultureInfo.InvariantCulture);
                case "int":
                    return value.AsInt32().ToString(CultureInfo.InvariantCulture);
                default:
                    return Literal(value.Kind == AttributeValueKind.String ? value.AsString() : value.RawText);
            }
        }

        private static string Named(string type, int number, string member) =>
            member != null ? type + "." + member : $"({type})({number.ToString(CultureInfo.InvariantCulture)})";

        /// <summary>Writes the tables of whole files that <c>CreateDefault()</c> starts from.</summary>
        private static string WriteFileDefaults(AttributeCollection vi, AttributeCollection gd, AttributeCollection ad)
        {
            var w = new CodeWriter();
            w.Line("// <auto-generated>");
            w.Line("//     Generated by tools/DialogClassGenerator: the default value of every key of each attribute file type, in the order");
            w.Line("//     of a file saved from the Tekla dialog. Do not edit: change the generator or its input and run it again.");
            w.Line("// </auto-generated>");
            w.Line();
            w.Line("namespace TeklaHelper.DrawingAttribute");
            w.Open();
            w.Summary("Every key of each attribute file type, in the order of a file saved from the Tekla dialog (see <c>CreateDefault()</c> of the files). A null value marks a key of a page, which its <c>SetDefaults()</c> writes. The other keys hold the value most files shipped with Tekla hold (at least three files), else the value of the dialog file; keys that name a layout or an attribute file use the names every environment has.");
            w.Line("internal static partial class FileDefaults");
            w.Open();
            WriteTable(w, "View", "view properties (.vi)", FileTable("vi", vi, PageHost.View));
            WriteTable(w, "GADrawing", "GA drawings (.gd)", FileTable("gd", gd, PageHost.GADrawing));
            WriteTable(w, "MultiDrawing", "multi drawings (.md)", FileTable("md", gd, PageHost.GADrawing));
            WriteTable(w, "AssemblyDrawing", "assembly drawings (.ad)", FileTable("ad", ad, PageHost.AssemblyDrawing));
            WriteTable(w, "SinglePartDrawing", "single-part drawings (.wd)", FileTable("wd", ad, PageHost.AssemblyDrawing));
            WriteTable(w, "CastUnitDrawing", "cast unit drawings (.cud)", FileTable("cud", ad, PageHost.AssemblyDrawing));
            w.TrimBlankLine();
            w.Close();
            w.Close();
            return w.ToString();
        }

        private enum PageHost
        {
            View,
            GADrawing,
            AssemblyDrawing,
        }

        /// <summary>
        /// The keys of <paramref name="reference"/> (a file saved from the dialog), plus the keys of the pages a newer dialog
        /// has, with a null value for the keys of a page and a default for the others. Single-part and cast unit drawings
        /// use the keys of the assembly drawing file: the newer shipped files of those types have the same <c>dcd.</c> keys.
        /// </summary>
        private static List<(string Key, string Value)> FileTable(string extension, AttributeCollection reference, PageHost host)
        {
            var pageValues = new Dictionary<string, string>(StringComparer.Ordinal);
            var pagePrefixes = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (PageSpec spec in PageSpecs.All)
            {
                string prefix = host == PageHost.View ? spec.ViewPrefix : host == PageHost.GADrawing ? spec.GADrawingPrefix : spec.AssemblyDrawingPrefix;

                // The pages of the drawing properties only keep the values of their own file type (the shipped GA and
                // assembly files do not agree on dimensions and views), which SetDefaults(), written for every host, cannot
                if (prefix == null || spec.DrawingPage || !PageDefaults.TryGetValue(spec.Class, out var defaults))
                {
                    continue;
                }

                IEnumerable<string> prefixes = spec.Class == "ViewLabelMark"
                    ? Enumerable.Range(1, 5).Select(i => "ViewLabelMarkA" + i.ToString(CultureInfo.InvariantCulture) + ".")
                    : new[] { prefix };
                foreach (string p in prefixes)
                {
                    foreach (string key in defaults.Values)
                    {
                        pageValues[p + key] = null;
                        pagePrefixes[p + key] = p;
                    }

                    foreach (string key in defaults.EnableKeys)
                    {
                        pageValues[p + key] = null;
                        pagePrefixes[p + key] = p;
                    }
                }

                // The label rows of the page: the keys of ViewLabelMark under each row
                foreach ((_, string keyStem, _, _) in LabelMarkProperties(spec))
                {
                    var label = PageDefaults["ViewLabelMark"];
                    foreach (string key in label.Values.Concat(label.EnableKeys))
                    {
                        pageValues[prefix + keyStem + key] = null;
                        pagePrefixes[prefix + keyStem + key] = prefix + keyStem;
                    }
                }
            }

            var keys = reference.Keys.Where(k => k != "in_file" && k != "out_file").ToList();

            // Keys of a newer dialog than the file saved from it (BarAttr.fill_color of Tekla 2026): after their page
            var known = new HashSet<string>(keys, StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> pair in pagePrefixes.Where(p => !known.Contains(p.Key)))
            {
                int last = keys.FindLastIndex(k => k.StartsWith(pair.Value, StringComparison.Ordinal) && (pair.Value.Length > 0 || k.IndexOf('.') < 0));
                keys.Insert(last + 1, pair.Key);
                known.Add(pair.Key);
            }

            var table = new List<(string Key, string Value)>();
            foreach (string key in keys)
            {
                if (pageValues.ContainsKey(key))
                {
                    table.Add((key, null));
                    continue;
                }

                string value;
                if (FileKeys.Contains(LastSegment(key)))
                {
                    value = "\"\"";
                }
                else if (EnvironmentFreeValues[extension].TryGetValue(key, out string environmentFree))
                {
                    value = environmentFree;
                }
                else
                {
                    value = MostCommon(extension, key)
                        ?? (SiblingTypes.TryGetValue(extension, out string sibling) ? MostCommon(sibling, key) : null)
                        ?? (reference.TryGetValue(key, out AttributeValue saved) && IsAscii(saved.ToFileText()) ? saved.ToFileText() : null);
                }

                if (value == null)
                {
                    throw new InvalidOperationException(
                        $"No default for {key} in .{extension}: no shipped value held by {MinimumFilesForDefault} files and no ASCII value in the dialog file. Add one to EnvironmentFreeValues.");
                }

                table.Add((key, value));
            }

            return table;
        }

        private static void WriteTable(CodeWriter w, string name, string description, List<(string Key, string Value)> table)
        {
            // A class per file type: its table is built the first time that type is created, not with the others
            w.Summary($"Every key of {description} with its default value.");
            w.Line($"internal static class {name}");
            w.Open();
            w.Summary($"Every key of {description}, in file order; null for a key of a page.");
            w.Line("internal static readonly (string Key, string Value)[] Table =");
            w.Open();
            foreach ((string key, string value) in table)
            {
                w.Line($"({Literal(key)}, {(value == null ? "null" : Literal(value))}),");
            }

            w.Close(";");
            w.Close();
            w.Line();
            Console.WriteLine($"File defaults {name}: {table.Count} keys, {table.Count(e => e.Value != null)} not on a page");
        }

        /// <summary>A C# string literal.</summary>
        private static string Literal(string text)
        {
            var builder = new StringBuilder("\"");
            foreach (char c in text)
            {
                switch (c)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    default:
                        if (c < 32 || c > 126)
                        {
                            builder.Append("\\u").Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            builder.Append(c);
                        }

                        break;
                }
            }

            return builder.Append('"').ToString();
        }
    }
}
