using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace DialogClassGenerator
{
    /// <summary>
    /// Writes the default of a mark content key (<c>ContentString1</c>, <c>PourContentString</c>, ...) as code that builds it
    /// with <c>MarkContent</c> instead of an XML literal. The code is written only when it gives the default back byte for
    /// byte: the rules of <c>MarkContent.ToXml</c> for new content (a <c>MarkPart</c> per element, a line per tag, a
    /// <c>Format</c> of font, colour, height and <c>hidden="0"</c>) are applied here to the parsed elements and the result
    /// compared with the default; an element they do not give back is kept as a <c>MarkXmlElement</c>, and content that
    /// is laid out otherwise stays a literal.
    /// </summary>
    internal static class MarkContentCode
    {
        private const string Head = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n<!DOCTYPE Mark><Mark>\n";
        private const string Tail = "</Mark>\n";
        private const string PartOpen = "<MarkPart groupid=\"0\">\n";
        private const string PartClose = "\n</MarkPart>\n";
        private const string Marks = "global::TeklaHelper.DrawingAttribute.Marks.";

        // The catalog of the library (Marks/MarkElementType.cs): the element types of each kind of mark
        private static readonly List<(string Kind, string Member, int SubType, string Name, bool Size)> Catalog = new List<(string, string, int, string, bool)>();

        /// <summary>Reads the catalog from the source of the library (<paramref name="library"/> = src/TeklaHelper.DrawingAttribute).</summary>
        public static void Load(string library)
        {
            string source = File.ReadAllText(Path.Combine(library, "Marks", "MarkElementType.cs"));
            var fields = new Dictionary<string, (int SubType, string Name, bool Size)>(StringComparer.Ordinal);
            foreach (Match m in Regex.Matches(source, @"private static readonly MarkElementType (\w+) = MarkElementType\.Define\((\d+), (string\.Empty|""[^""]*""), ""[^""]*""(, sizeElement: true)?\);"))
            {
                string name = m.Groups[3].Value == "string.Empty" ? string.Empty : m.Groups[3].Value.Trim('"');
                fields[m.Groups[1].Value] = (int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture), name, m.Groups[4].Success);
            }

            // The kinds are the classes nested in MarkElementTypes (indented twice)
            foreach (Match kind in Regex.Matches(source, @"\n        public static class (\w+)\s*\{(.*?)\n        \}", RegexOptions.Singleline))
            {
                foreach (Match member in Regex.Matches(kind.Groups[2].Value, @"public static readonly MarkElementType (\w+) = (?:MarkElementTypes\.)?(\w+);"))
                {
                    if (fields.TryGetValue(member.Groups[2].Value, out var field))
                    {
                        Catalog.Add((kind.Groups[1].Value, member.Groups[1].Value, field.SubType, field.Name, field.Size));
                    }
                }
            }

            if (Catalog.Count == 0)
            {
                throw new InvalidOperationException("No mark element types found in Marks/MarkElementType.cs.");
            }
        }

        /// <summary>
        /// The statement that sets <paramref name="key"/> of a page to <paramref name="text"/>, as lines of code; null when
        /// the text is no mark content <c>MarkContent</c> writes back the same.
        /// </summary>
        public static List<string> TryWrite(string pageClass, string key, string text, Func<int, string> color)
        {
            string xml = text.Replace("\r\n", "\n");
            if (!xml.StartsWith(Head, StringComparison.Ordinal) || !xml.EndsWith(Tail, StringComparison.Ordinal))
            {
                return null;
            }

            // Each element in a MarkPart of its own, as new content is written
            var elements = new List<string>();
            int position = Head.Length;
            while (position < xml.Length - Tail.Length)
            {
                if (string.CompareOrdinal(xml, position, PartOpen, 0, PartOpen.Length) != 0)
                {
                    return null;
                }

                int start = position + PartOpen.Length;
                int end = xml.IndexOf(PartClose, start, StringComparison.Ordinal);
                if (end < 0 || xml.IndexOf("<MarkPart", start, end - start, StringComparison.Ordinal) >= 0)
                {
                    return null;
                }

                elements.Add(xml.Substring(start, end - start));
                position = end + PartClose.Length;
            }

            if (position != xml.Length - Tail.Length)
            {
                return null;
            }

            var calls = new List<(string Call, string Format)>();
            foreach (string element in elements)
            {
                calls.Add(Element(pageClass, key, element, color));
            }

            // One format shared by every element is the content's default format; otherwise each element has its own
            List<string> formats = calls.Where(c => c.Format != null).Select(c => c.Format).Distinct().ToList();
            string shared = formats.Count == 1 ? formats[0] : null;
            string defaultFormat = "new " + Marks + "MarkFormat { Font = \"Arial\", Color = global::TeklaHelper.DrawingAttribute.DrawingColor.Black, Height = 2.5 }";
            var lines = new List<string>
            {
                $"SetMarkContent({Program.Literal(key)}, new {Marks}MarkContent({(shared == null || shared == defaultFormat ? string.Empty : shared)})",
            };
            foreach ((string call, string format) in calls)
            {
                lines.Add("    " + string.Format(CultureInfo.InvariantCulture, call, format == null || format == shared ? string.Empty : ", " + format));
            }

            lines[lines.Count - 1] += ");";
            return lines;
        }

        /// <summary>The call that adds one element (with <c>{0}</c> where a format argument goes), and its format.</summary>
        private static (string Call, string Format) Element(string pageClass, string key, string text, Func<int, string> color)
        {
            (string Call, string Format) xml = ($".Add(new {Marks}MarkXmlElement({Program.Literal(text)}))", null);
            XElement element;
            try
            {
                element = XElement.Parse(text, LoadOptions.PreserveWhitespace);
            }
            catch (XmlException)
            {
                return xml;
            }

            string tag = element.Name.LocalName;
            if (!element.HasElements && !element.Nodes().Any() && !element.HasAttributes && (tag == "LineFeed" || tag == "Space" || tag == "BackSpace"))
            {
                return text == $"<{tag}/>" ? ($".Add{tag}()", null) : xml;
            }

            XElement formatElement = element.Element("Format");
            if (formatElement == null || element.Elements().Count() != 1)
            {
                return xml;
            }

            string font = (string)formatElement.Attribute("font");
            int? number = int.TryParse((string)formatElement.Attribute("color"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int c) ? c : (int?)null;
            double? height = double.TryParse((string)formatElement.Attribute("height"), NumberStyles.Float, CultureInfo.InvariantCulture, out double h) ? h : (double?)null;
            if (font == null || number == null || height == null || font.Contains('"') || font.Contains('\\'))
            {
                return xml;
            }

            string formatText = $"<Format font=\"{Escape(font, attribute: true)}\" color=\"{number.Value.ToString(CultureInfo.InvariantCulture)}\" height=\"{height.Value.ToString("F3", CultureInfo.InvariantCulture)}\" hidden=\"0\"/>";
            string format = $"new {Marks}MarkFormat {{ Font = {Program.Literal(font)}, Color = {color(number.Value)}, Height = {height.Value.ToString("R", CultureInfo.InvariantCulture)} }}";
            string expected;
            string call;
            switch (tag)
            {
                case "UserText":
                    {
                        string value = string.Concat(element.Nodes().TakeWhile(n => !(n is XElement)).OfType<XText>().Select(t => t.Value));
                        if (!value.StartsWith("\n", StringComparison.Ordinal) || !value.EndsWith("\n", StringComparison.Ordinal) || value.Length < 2)
                        {
                            return xml;
                        }

                        value = value.Substring(1, value.Length - 2);
                        expected = $"<UserText>\n{Escape(value)}\n{formatText}\n</UserText>";
                        call = $".AddText({Program.Literal(value)}{{0}})";
                        break;
                    }

                case "Element":
                case "SizeElement":
                    {
                        bool size = tag == "SizeElement";
                        string name = (string)element.Attribute("name") ?? string.Empty;
                        if (!int.TryParse((string)element.Attribute("subType"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int subType))
                        {
                            return xml;
                        }

                        string attributes = size ? $"value=\"\" subType=\"{subType}\"" : $"name=\"{Escape(name, attribute: true)}\" value=\"\" subType=\"{subType}\"";
                        expected = $"<{tag} {attributes}>\n{formatText}\n</{tag}>";
                        if (subType == 16 && !size)
                        {
                            call = $".AddUserAttribute({Program.Literal(name)}{{0}})";
                            break;
                        }

                        string type = TypeExpression(pageClass, key, subType, name, size);
                        if (type == null)
                        {
                            return xml;
                        }

                        call = $".Add({type}{{0}})";
                        break;
                    }

                default:
                    return xml;
            }

            return expected == text ? (call, format) : xml;
        }

        /// <summary>
        /// <c>MarkElementTypes.Kind.Member</c> of the element, from the kinds of mark the key is for, else any kind; the name
        /// the type writes must be the one in the default.
        /// </summary>
        private static string TypeExpression(string pageClass, string key, int subType, string name, bool size)
        {
            var matches = Catalog.Where(e => e.SubType == subType && e.Size == size && (size || e.Name == name)).ToList();
            foreach (string kind in Kinds(pageClass, key))
            {
                var match = matches.FirstOrDefault(e => e.Kind == kind);
                if (match.Member != null)
                {
                    return Marks + "MarkElementTypes." + match.Kind + "." + match.Member;
                }
            }

            return matches.Count == 0 ? null : Marks + "MarkElementTypes." + matches[0].Kind + "." + matches[0].Member;
        }

        /// <summary>The kinds of mark a content key is for (the tables checked in Tekla 2025, see CLAUDE.md).</summary>
        private static IEnumerable<string> Kinds(string pageClass, string key)
        {
            switch (key)
            {
                case "PourContentString":
                    return new[] { "PourMark" };
                case "ChamferContentString":
                    return new[] { "EdgeChamferMark" };
            }

            if (pageClass == "AssociativeNoteProperties" || pageClass == "DimensionMark")
            {
                switch (key)
                {
                    case "ContentString1": return new[] { "PartMark" };
                    case "ContentString2": return new[] { "BoltMark" };
                    case "ContentString3": return new[] { "SurfaceTreatmentMark" };
                    case "ContentString4": return new[] { "ReinforcementMark", "ReinforcementMeshMark" };
                }
            }

            if (pageClass.Contains("Reinforcement"))
            {
                return key == "ContentString4" ? new[] { "ReinforcementMeshMark", "ReinforcementMark" } : new[] { "ReinforcementMark" };
            }

            foreach (string kind in new[] { "PartMark", "BoltMark", "ConnectionMark", "SurfaceTreatmentMark", "PourMark" })
            {
                if (pageClass.EndsWith(kind, StringComparison.Ordinal))
                {
                    return new[] { kind };
                }
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// Text as <c>MarkXmlWriter</c> writes it in new content: <c>&amp;</c> and <c>&lt;</c> always escaped, <c>&gt;</c> in
        /// text, <c>"</c> in attributes.
        /// </summary>
        private static string Escape(string text, bool attribute = false)
        {
            var builder = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                switch (c)
                {
                    case '&': builder.Append("&amp;"); break;
                    case '<': builder.Append("&lt;"); break;
                    case '>' when !attribute: builder.Append("&gt;"); break;
                    case '"' when attribute: builder.Append("&quot;"); break;
                    default: builder.Append(c); break;
                }
            }

            return builder.ToString();
        }
    }
}
