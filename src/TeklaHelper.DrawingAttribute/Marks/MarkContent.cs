using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Marks
{
    /// <summary>How mark content is laid out in the file; Tekla reads both.</summary>
    public enum MarkContentLayout
    {
        /// <summary>A line per tag, as the mark dialogs and the drawing properties write it.</summary>
        Lines,

        /// <summary>One line, as the View properties dialog of Tekla 2025 writes its labels.</summary>
        Compact,
    }

    /// <summary>
    /// The content of a mark or a label: the "Elements in mark" of a mark dialog, which Tekla stores as XML in the
    /// <c>ContentString</c> keys (<c>ContentString</c> of a label row, <c>ContentString1</c> ... <c>ContentString4</c> and
    /// <c>PourContentString</c>, <c>ChamferContentString</c>, ... of a mark). Build it from elements instead of writing the
    /// XML:
    /// <code>
    /// var label = new MarkContent()
    ///     .Add(MarkElementTypes.ViewLabel.ViewName, new MarkFormat { Font = "Tahoma", Color = DrawingColor.Green, Height = 5 })
    ///     .AddText(" ")
    ///     .Add(MarkElementTypes.ViewLabel.Scale);
    /// view.Attributes.LabelMarkA1.Elements = label;
    /// </code>
    /// <see cref="Parse"/> reads the content Tekla wrote, keeping what it does not model, and <see cref="ToXml()"/> writes
    /// content Tekla wrote back as it was.
    /// </summary>
    public sealed class MarkContent : Collection<MarkElement>, IEquatable<MarkContent>
    {
        private const string Declaration = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>";
        private MarkContentLayout _layout;

        /// <summary>Creates empty content; elements added without a format take Arial, black, 2.5.</summary>
        public MarkContent()
            : this(new MarkFormat())
        {
        }

        /// <summary>Creates empty content whose elements take <paramref name="defaultFormat"/> when added without a format.</summary>
        public MarkContent(MarkFormat defaultFormat)
        {
            DefaultFormat = defaultFormat ?? throw new ArgumentNullException(nameof(defaultFormat));
        }

        private MarkFormat _defaultFormat;

        /// <summary>The format the <c>Add</c> methods give an element added without one (a copy each).</summary>
        public MarkFormat DefaultFormat
        {
            get => _defaultFormat;
            set => _defaultFormat = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The layout <see cref="ToXml()"/> writes: the layout of the parsed text, else <see cref="MarkContentLayout.Lines"/>
        /// (or the layout of the value it replaces, see <see cref="AttributeSection.SetMarkContent"/>).
        /// </summary>
        public MarkContentLayout Layout
        {
            get => _layout;
            set
            {
                _layout = value;
                LayoutChosen = true;
            }
        }

        /// <summary>What ends a line in <see cref="MarkContentLayout.Lines"/>: a line feed, or two where the parsed text has two.</summary>
        internal string LineBreak { get; private set; } = "\n";

        /// <summary>Whether text writes <c>&gt;</c> escaped: as the parsed text did, else escaped.</summary>
        internal bool EscapeGreaterThan { get; private set; } = true;

        /// <summary>Whether text writes <c>&lt;</c> escaped: false for content read with a raw <c>&lt;</c> in its text.</summary>
        internal bool EscapeLessThan { get; private set; } = true;

        /// <summary>Whether the layout was read or set, rather than left to the value the content replaces.</summary>
        internal bool LayoutChosen { get; private set; }

        /// <summary>Adds an element.</summary>
        public new MarkContent Add(MarkElement element)
        {
            base.Add(element);
            return this;
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, MarkElement item) => base.InsertItem(index, item ?? throw new ArgumentNullException(nameof(item)));

        /// <inheritdoc />
        protected override void SetItem(int index, MarkElement item) => base.SetItem(index, item ?? throw new ArgumentNullException(nameof(item)));

        /// <summary>Adds a property element (part position, view name, scale, ... from <see cref="MarkElementTypes"/>).</summary>
        public MarkContent Add(MarkElementType type, MarkFormat format = null)
        {
            Add(new MarkPropertyElement(type, format ?? DefaultFormat.Clone()));
            return this;
        }

        /// <summary>Adds text.</summary>
        public MarkContent AddText(string text, MarkFormat format = null)
        {
            Add(new MarkTextElement(text, format ?? DefaultFormat.Clone()));
            return this;
        }

        /// <summary>Adds a user-defined attribute or template attribute of the object (e.g. <c>USER_FIELD_1</c>).</summary>
        public MarkContent AddUserAttribute(string name, MarkFormat format = null)
        {
            Add(new MarkUserAttributeElement(name, format ?? DefaultFormat.Clone()));
            return this;
        }

        /// <summary>Adds a new line.</summary>
        public MarkContent AddLineFeed()
        {
            Add(MarkSeparatorElement.LineFeed());
            return this;
        }

        /// <summary>Adds a space.</summary>
        public MarkContent AddSpace()
        {
            Add(MarkSeparatorElement.Space());
            return this;
        }

        /// <summary>Adds a backspace (no gap between the elements around it).</summary>
        public MarkContent AddBackSpace()
        {
            Add(MarkSeparatorElement.BackSpace());
            return this;
        }

        /// <summary>Adds symbol <paramref name="index"/> of the symbol file <paramref name="file"/>.</summary>
        public MarkContent AddSymbol(string file, int index, MarkFormat format = null)
        {
            Add(new MarkSymbolElement(file, index, format ?? DefaultFormat.Clone()));
            return this;
        }

        /// <summary>Adds a frame around <paramref name="content"/>.</summary>
        public MarkContent AddFrame(FrameAroundElementsType type, DrawingColor color, MarkContent content)
        {
            Add(new MarkFrameElement(type, color, content));
            return this;
        }

        /// <summary>Adds a report template (file name without extension).</summary>
        public MarkContent AddTemplate(string fileName)
        {
            Add(new MarkTemplateElement(fileName));
            return this;
        }

        /// <summary>
        /// Reads mark content (the text of a <c>ContentString</c> key). Two kinds of damage files shipped with Tekla hold are
        /// read as Tekla meant them: an attribute without its value (<c>value= subType="45"</c>, read as empty) and a raw
        /// <c>&lt;</c> in a text (<c>&lt;&lt; Mark &gt;&gt;</c>, written back raw).
        /// </summary>
        /// <exception cref="FormatException">The text is not mark content.</exception>
        public static MarkContent Parse(string xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            string text = xml.Replace("\r\n", "\n");
            string emptyValues = Regex.Replace(text, @"(\s[A-Za-z_]+)=(?=\s+[A-Za-z_]+="")", "$1=\"\"");
            string RawLessThan(string t) => Regex.Replace(t, @"<(?=[^\p{L}_:/!?]|$)", "&lt;");
            XDocument document = null;
            bool rawLessThan = false;
            XmlException failure = null;
            foreach ((string candidate, bool raw) in new[] { (text, false), (emptyValues, false), (RawLessThan(text), true), (RawLessThan(emptyValues), true) })
            {
                try
                {
                    document = Load(candidate);
                    rawLessThan = raw;
                    break;
                }
                catch (XmlException exception)
                {
                    failure = failure ?? exception;
                }
            }

            if (document == null)
            {
                throw new FormatException("Not mark content: " + failure.Message, failure);
            }

            if (document.Root == null || document.Root.Name.LocalName != "Mark")
            {
                throw new FormatException("Not mark content: the root element is not <Mark>.");
            }

            // The line break is what follows the declaration (one line feed; two in some older files)
            int declarationEnd = text.IndexOf("?>", StringComparison.Ordinal) + 2;
            int lineBreakEnd = declarationEnd;
            while (declarationEnd >= 2 && lineBreakEnd < text.Length && text[lineBreakEnd] == '\n')
            {
                lineBreakEnd++;
            }

            // The layout is told by the declaration alone: in one-line content a text may hold line feeds of its own
            bool lines = declarationEnd >= 2 ? lineBreakEnd > declarationEnd : text.IndexOf('\n') >= 0;
            string lineBreak = lineBreakEnd > declarationEnd ? text.Substring(declarationEnd, lineBreakEnd - declarationEnd) : "\n";
            MarkContent content = ReadParts(document.Root, lineBreak, lines);
            content.Layout = lines ? MarkContentLayout.Lines : MarkContentLayout.Compact;
            content.EscapeGreaterThan = text.IndexOf("&gt;", StringComparison.Ordinal) >= 0;
            content.EscapeLessThan = !rawLessThan;
            return content;
        }

        /// <summary>Whether a <c>&lt;</c> followed by <paramref name="c"/> would open a tag rather than be text.</summary>
        internal static bool StartsTag(char c) => char.IsLetter(c) || c == '_' || c == ':' || c == '/' || c == '!' || c == '?';

        private static XDocument Load(string text)
        {
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
            using (var reader = XmlReader.Create(new StringReader(text), settings))
            {
                return XDocument.Load(reader, LoadOptions.PreserveWhitespace);
            }
        }

        /// <summary>Reads mark content; false when the text is null or not mark content.</summary>
        public static bool TryParse(string xml, out MarkContent content)
        {
            if (xml == null)
            {
                content = null;
                return false;
            }

            try
            {
                content = Parse(xml);
                return true;
            }
            catch (FormatException)
            {
                content = null;
                return false;
            }
        }

        /// <summary>The XML of the content, in <see cref="Layout"/>.</summary>
        public string ToXml() => ToXml(Layout);

        /// <summary>The XML of the content, in <paramref name="layout"/>.</summary>
        public string ToXml(MarkContentLayout layout)
        {
            // Frames without an id take those after the ids of the content, from 101 as in the files; the content itself
            // keeps no id, so each writing numbers them afresh
            int highest = Frames(this, new HashSet<MarkContent>(ReferenceComparer.Instance)).Select(f => f.ValidId ?? 0).DefaultIfEmpty(0).Max();
            var w = new MarkXmlWriter(layout == MarkContentLayout.Lines, LineBreak)
            {
                EscapeGreaterThan = EscapeGreaterThan,
                EscapeLessThan = EscapeLessThan,
                FrameId = Math.Max(100, highest) + 1,
            };
            w.Raw(Declaration);
            w.EndLine();
            w.Raw("<!DOCTYPE Mark><Mark>");
            w.EndLine();
            WriteParts(w);
            w.Raw("</Mark>");
            w.EndLine();
            return w.ToString();
        }

        /// <exception cref="InvalidOperationException">A frame holds the content it is in.</exception>
        private static IEnumerable<MarkFrameElement> Frames(MarkContent content, HashSet<MarkContent> path)
        {
            if (!path.Add(content))
            {
                throw new InvalidOperationException("A frame of the mark content holds the content it is in.");
            }

            foreach (MarkFrameElement frame in content.OfType<MarkFrameElement>().ToList())
            {
                yield return frame;
                foreach (MarkFrameElement inner in Frames(frame.Content, path))
                {
                    yield return inner;
                }
            }

            path.Remove(content);
        }

        internal void WriteParts(MarkXmlWriter w)
        {
            // Elements read from one MarkPart share its attributes and go back into one; new elements have a part each
            for (int i = 0; i < Count;)
            {
                XmlAttributeList part = this[i].PartAttributes;
                if (part == null)
                {
                    this[i++].Write(w);
                    continue;
                }

                w.Open("MarkPart", part);
                w.EndLine();
                while (i < Count && ReferenceEquals(this[i].PartAttributes, part))
                {
                    this[i++].Write(w);
                }

                w.Close("MarkPart");
                w.EndLine();
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer<MarkContent>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();

            public bool Equals(MarkContent x, MarkContent y) => ReferenceEquals(x, y);

            public int GetHashCode(MarkContent obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }

        private static MarkContent ReadParts(XElement mark, string lineBreak, bool lines)
        {
            var content = new MarkContent { LineBreak = lineBreak };
            foreach (XElement part in mark.Elements())
            {
                if (part.Name.LocalName != "MarkPart")
                {
                    // Kept where it was, outside any MarkPart
                    content.Add(new MarkXmlElement(Serialize(part)) { PartAttributes = null });
                    continue;
                }

                XmlAttributeList partAttributes = XmlAttributeList.From(part, "groupid");
                foreach (XElement child in part.Elements())
                {
                    MarkElement element = ReadElement(child, lineBreak, lines);
                    element.PartAttributes = partAttributes;
                    content.Add(element);
                }
            }

            return content;
        }

        private static MarkElement ReadElement(XElement element, string lineBreak, bool lines)
        {
            XmlAttributeList Attributes(params string[] order) => XmlAttributeList.From(element, order);

            XElement formatElement = element.Element("Format");
            MarkFormat format = formatElement == null ? null : MarkFormat.Read(formatElement);
            bool onlyFormat = element.Elements().All(e => e.Name.LocalName == "Format") && element.Elements().Count() <= 1;
            switch (element.Name.LocalName)
            {
                case "Element":
                case "SizeElement":
                    {
                        bool size = element.Name.LocalName == "SizeElement";
                        XmlAttributeList attributes = Attributes("name", "value", "subType");
                        int? subType = MarkXmlNumbers.ParseInt(attributes.Get("subType"));
                        if (subType == null || !onlyFormat)
                        {
                            break;
                        }

                        if (subType == 16 && !size)
                        {
                            return new MarkUserAttributeElement(attributes, format);
                        }

                        return new MarkPropertyElement(attributes, MarkElementType.FromXml(subType.Value, size), format);
                    }

                case "UserText":
                    if (onlyFormat && !element.Attributes().Any())
                    {
                        // The text before <Format>; in the line layout the line breaks around it are layout, in one line
                        // they are the text's own
                        string text = string.Concat(element.Nodes().TakeWhile(n => !(n is XElement)).OfType<XText>().Select(t => t.Value));
                        if (!lines)
                        {
                            return new MarkTextElement(text, format, breakBefore: true, breakAfter: true);
                        }

                        bool before = text.StartsWith(lineBreak, StringComparison.Ordinal);
                        if (before)
                        {
                            text = text.Substring(lineBreak.Length);
                        }

                        bool after = text.EndsWith(lineBreak, StringComparison.Ordinal);
                        if (after)
                        {
                            text = text.Substring(0, text.Length - lineBreak.Length);
                        }

                        return new MarkTextElement(text, format, before, after);
                    }

                    break;

                case "LineFeed":
                case "Space":
                case "BackSpace":
                    if (!element.Attributes().Any() && !element.Nodes().Any() && MarkSeparatorElement.TryParse(element.Name.LocalName, out MarkSeparatorKind kind))
                    {
                        return new MarkSeparatorElement(kind);
                    }

                    break;

                case "Symbol":
                    if (onlyFormat)
                    {
                        return new MarkSymbolElement(Attributes("symbol"), format);
                    }

                    break;

                case "Frame":
                    {
                        List<XElement> inner = element.Elements().ToList();
                        if (inner.Count == 1 && inner[0].Name.LocalName == "Mark")
                        {
                            return new MarkFrameElement(Attributes("type", "color", "frame_id"), ReadParts(inner[0], lineBreak, lines));
                        }

                        break;
                    }

                case "Template":
                    if (!element.Nodes().Any())
                    {
                        return new MarkTemplateElement(Attributes("filename", "ObjectId"));
                    }

                    break;
            }

            return new MarkXmlElement(Serialize(element));
        }

        /// <summary>An element as Tekla writes it: attributes as read, no space before <c>/&gt;</c>.</summary>
        private static string Serialize(XElement element)
        {
            XmlAttributeList attributes = XmlAttributeList.From(element);
            var w = new MarkXmlWriter(lines: false);
            if (!element.Nodes().Any())
            {
                w.Open(element.Name.LocalName, attributes, selfClosing: true);
                return w.ToString();
            }

            w.Open(element.Name.LocalName, attributes);
            foreach (XNode node in element.Nodes())
            {
                if (node is XElement child)
                {
                    w.Raw(Serialize(child));
                }
                else if (node is XText text)
                {
                    w.Text(text.Value);
                }
            }

            w.Close(element.Name.LocalName);
            return w.ToString();
        }

        /// <summary>
        /// Same elements with the same formats, in the same <c>&lt;MarkPart&gt;</c>s (the same grouping and part
        /// attributes, or none), whatever the layout.
        /// </summary>
        public bool Equals(MarkContent other)
        {
            if (other == null || other.Count != Count)
            {
                return false;
            }

            for (int i = 0; i < Count; i++)
            {
                MarkElement a = this[i], b = other[i];
                if (!a.SameAs(b) || !XmlAttributeList.Same(a.PartAttributes, b.PartAttributes) || StartsPart(i) != other.StartsPart(i))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Whether element <paramref name="index"/> opens a MarkPart of its own (as <see cref="WriteParts"/> writes it).</summary>
        private bool StartsPart(int index) =>
            this[index].PartAttributes == null || index == 0 || !ReferenceEquals(this[index].PartAttributes, this[index - 1].PartAttributes);

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as MarkContent);

        /// <inheritdoc />
        public override int GetHashCode() => Count;

        /// <inheritdoc />
        public override string ToString() => string.Join(" ", this.Select(e => e.ToString()));
    }
}
