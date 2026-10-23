using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Marks
{
    /// <summary>
    /// The text format of an element of mark content (<c>&lt;Format&gt;</c>): font, colour and height, and for a
    /// user-defined attribute its unit and number format. Checked in Tekla 2025: the Open API read the font, colour and
    /// height of each element, and the unit and format of user-defined attributes. The attributes it does not model
    /// (<c>override</c>, whose effect the Open API does not show) are kept as read.
    /// </summary>
    public sealed class MarkFormat : IEquatable<MarkFormat>
    {
        internal readonly XmlAttributeList Attributes = new XmlAttributeList("font", "unit", "format", "color", "height", "hidden", "override");

        /// <summary>Creates a format: Arial, black, 2.5.</summary>
        public MarkFormat()
        {
            Font = "Arial";
            Color = DrawingColor.Black;
            Height = 2.5;
        }

        private MarkFormat(bool empty)
        {
        }

        /// <summary>Read from a file (keeps its attributes as they were, without adding <c>hidden</c>).</summary>
        internal bool FromFile { get; private set; }

        /// <summary>Font name (<c>font</c>).</summary>
        public string Font
        {
            get => Attributes.Get("font") ?? string.Empty;
            set => Attributes.Set("font", value ?? string.Empty);
        }

        /// <summary>Colour (<c>color</c>; black when the format has none).</summary>
        public DrawingColor Color
        {
            get => (DrawingColor)(MarkXmlNumbers.ParseInt(Attributes.Get("color")) ?? (int)DrawingColor.Black);
            set => Attributes.Set("color", MarkXmlNumbers.Int((int)value));
        }

        /// <summary>Text height (<c>height</c>, written with three decimals; 0 when the format has none).</summary>
        /// <exception cref="ArgumentOutOfRangeException">The height is not a finite number.</exception>
        public double Height
        {
            get => MarkXmlNumbers.ParseDouble(Attributes.Get("height")) ?? 0;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "A height must be a finite number.");
                }

                Attributes.Set("height", MarkXmlNumbers.Height(value));
            }
        }

        /// <summary>Unit of a user-defined attribute (<c>unit</c>); null for none.</summary>
        public Units? Unit
        {
            get => (Units?)MarkXmlNumbers.ParseInt(Attributes.Get("unit"));
            set => Attributes.Set("unit", value == null ? null : MarkXmlNumbers.Int((int)value.Value));
        }

        /// <summary>Number format of a user-defined attribute (<c>format</c>); null for none.</summary>
        public UnitsFormat? NumberFormat
        {
            get => (UnitsFormat?)MarkXmlNumbers.ParseInt(Attributes.Get("format"));
            set => Attributes.Set("format", value == null ? null : MarkXmlNumbers.Int((int)value.Value));
        }

        /// <summary>A copy of this format.</summary>
        public MarkFormat Clone()
        {
            var copy = new MarkFormat(true);
            foreach (var pair in Attributes.Items)
            {
                copy.Attributes.Add(pair.Key, pair.Value);
            }

            return copy;
        }

        internal static MarkFormat Read(XElement element)
        {
            var format = new MarkFormat(true) { FromFile = true };
            foreach (XAttribute attribute in element.Attributes())
            {
                format.Attributes.Add(attribute.Name.LocalName, attribute.Value);
            }

            return format;
        }

        internal void Write(MarkXmlWriter w)
        {
            XmlAttributeList attributes = Attributes;
            if (!FromFile && w.Lines && Attributes.Get("hidden") == null)
            {
                // The mark dialogs write hidden="0"; the view dialog writes no hidden at all
                attributes = Clone().Attributes;
                attributes.Set("hidden", "0");
            }

            w.Open("Format", attributes, selfClosing: true);
        }

        /// <summary>
        /// The same attributes: font, colour, height, unit and format with the same values where they are given (a missing
        /// attribute equals only a missing one), the others as text; <c>hidden</c> missing equals <c>hidden="0"</c>.
        /// </summary>
        public bool Equals(MarkFormat other)
        {
            if (other == null)
            {
                return false;
            }

            bool SameNumber(string name)
            {
                string a = Attributes.Get(name), b = other.Attributes.Get(name);
                if (a == null || b == null)
                {
                    return a == b;
                }

                double? x = MarkXmlNumbers.ParseDouble(a), y = MarkXmlNumbers.ParseDouble(b);
                return x != null && y != null ? Math.Abs(x.Value - y.Value) < 0.0005 : a == b;
            }

            string Hidden(MarkFormat f) => f.Attributes.Get("hidden") ?? "0";
            return Attributes.Get("font") == other.Attributes.Get("font") && SameNumber("color") && SameNumber("height") && SameNumber("unit")
                && SameNumber("format") && Hidden(this) == Hidden(other)
                && Attributes.SameAs(other.Attributes, "font", "color", "height", "unit", "format", "hidden");
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as MarkFormat);

        /// <summary>A hash of the font: the format is mutable, so do not change one that is a key of a hash set.</summary>
        public override int GetHashCode() => (Attributes.Get("font") ?? string.Empty).GetHashCode();

        /// <inheritdoc />
        public override string ToString() => $"{Font}, {Color}, {Height:0.###}";
    }

    /// <summary>An element of mark content: one row of "Elements in mark" of a mark dialog.</summary>
    public abstract class MarkElement
    {
        /// <summary>
        /// The attributes of the <c>&lt;MarkPart&gt;</c> around the element (<c>groupid="0"</c>). Elements read from one
        /// MarkPart share it and are written back in one; null for an element read outside any MarkPart.
        /// </summary>
        internal XmlAttributeList PartAttributes { get; set; } = NewPartAttributes();

        internal static XmlAttributeList NewPartAttributes()
        {
            var attributes = new XmlAttributeList("groupid");
            attributes.Add("groupid", "0");
            return attributes;
        }

        internal abstract void Write(MarkXmlWriter w);

        internal abstract bool SameAs(MarkElement other);

        internal static void WriteFormat(MarkXmlWriter w, MarkFormat format)
        {
            if (format != null)
            {
                format.Write(w);
                w.EndLine();
            }
        }
    }

    /// <summary>A property of the object (part position, profile, view name, scale, ...): <c>&lt;Element subType="..."&gt;</c>.</summary>
    public sealed class MarkPropertyElement : MarkElement
    {
        internal readonly XmlAttributeList Attributes = new XmlAttributeList("name", "value", "subType");
        private MarkElementType _type;

        /// <summary>Creates a property element; without a format it takes Arial, black, 2.5.</summary>
        public MarkPropertyElement(MarkElementType type, MarkFormat format = null)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Format = format ?? new MarkFormat();
        }

        internal MarkPropertyElement(XmlAttributeList attributes, MarkElementType type, MarkFormat format)
        {
            Attributes = attributes;
            _type = type;
            Format = format;
        }

        /// <summary>
        /// The property (<c>subType</c>); setting it also writes the name the dialogs write with it (<c>name</c>, which Tekla
        /// does not read).
        /// </summary>
        public MarkElementType Type
        {
            get => _type;
            set
            {
                _type = value ?? throw new ArgumentNullException(nameof(value));
                Attributes.Set("name", value.IsSizeElement ? null : value.Name);
                Attributes.Set("value", Attributes.Get("value") ?? string.Empty);
                Attributes.Set("subType", MarkXmlNumbers.Int(value.SubType));
            }
        }

        /// <summary>The format of the element; null writes none.</summary>
        public MarkFormat Format { get; set; }

        internal override void Write(MarkXmlWriter w)
        {
            w.Open(Type.Tag, Attributes);
            w.EndLine();
            WriteFormat(w, Format);
            w.Close(Type.Tag);
            w.EndLine();
        }

        internal override bool SameAs(MarkElement other) =>
            other is MarkPropertyElement o && o.Type.Equals(Type) && Equals(o.Format, Format);

        /// <inheritdoc />
        public override string ToString() => Type.Description;
    }

    /// <summary>A user-defined attribute or template attribute of the object: <c>&lt;Element name="..." subType="16"&gt;</c>.</summary>
    public sealed class MarkUserAttributeElement : MarkElement
    {
        internal readonly XmlAttributeList Attributes = new XmlAttributeList("name", "value", "subType");

        /// <summary>Creates the element of the attribute <paramref name="name"/> (e.g. <c>USER_FIELD_1</c>).</summary>
        public MarkUserAttributeElement(string name, MarkFormat format = null)
        {
            Attributes.Add("name", name ?? throw new ArgumentNullException(nameof(name)));
            Attributes.Add("value", string.Empty);
            Attributes.Add("subType", "16");
            Format = format ?? new MarkFormat();
        }

        internal MarkUserAttributeElement(XmlAttributeList attributes, MarkFormat format)
        {
            Attributes = attributes;
            Format = format;
        }

        /// <summary>Name of the attribute (<c>name</c>; the Open API reads it as <c>UserDefinedElement.Name</c>).</summary>
        public string Name
        {
            get => Attributes.Get("name") ?? string.Empty;
            set => Attributes.Set("name", value ?? string.Empty);
        }

        /// <summary>The format of the element, with the attribute's unit and number format.</summary>
        public MarkFormat Format { get; set; }

        internal override void Write(MarkXmlWriter w)
        {
            w.Open("Element", Attributes);
            w.EndLine();
            WriteFormat(w, Format);
            w.Close("Element");
            w.EndLine();
        }

        internal override bool SameAs(MarkElement other) => other is MarkUserAttributeElement o && o.Name == Name && Equals(o.Format, Format);

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>Text typed in the dialog: <c>&lt;UserText&gt;</c>.</summary>
    public sealed class MarkTextElement : MarkElement
    {
        /// <summary>Creates a text element.</summary>
        public MarkTextElement(string text, MarkFormat format = null)
        {
            Text = text ?? string.Empty;
            Format = format ?? new MarkFormat();
        }

        internal MarkTextElement(string text, MarkFormat format, bool breakBefore, bool breakAfter)
        {
            Text = text;
            Format = format;
            BreakBefore = breakBefore;
            BreakAfter = breakAfter;
        }

        /// <summary>The line break before and after the text in the line layout (missing in a few damaged files).</summary>
        internal bool BreakBefore { get; } = true;

        internal bool BreakAfter { get; } = true;

        /// <summary>The text; control characters other than tab and line breaks cannot be written.</summary>
        public string Text { get; set; }

        /// <summary>The format of the text.</summary>
        public MarkFormat Format { get; set; }

        internal override void Write(MarkXmlWriter w)
        {
            // The mark dialogs put the text on a line of its own: <UserText>\nTEXT\n<Format .../>\n</UserText>
            w.Open("UserText", null);
            if (BreakBefore)
            {
                w.EndLine();
            }

            w.Text(Text ?? string.Empty);
            if (BreakAfter)
            {
                w.EndLine();
            }

            WriteFormat(w, Format);
            w.Close("UserText");
            w.EndLine();
        }

        internal override bool SameAs(MarkElement other) => other is MarkTextElement o && o.Text == Text && Equals(o.Format, Format);

        /// <inheritdoc />
        public override string ToString() => "\"" + Text + "\"";
    }

    /// <summary>The kinds of <see cref="MarkSeparatorElement"/>.</summary>
    public enum MarkSeparatorKind
    {
        /// <summary>A new line (<c>&lt;LineFeed/&gt;</c>).</summary>
        LineFeed,

        /// <summary>A space (<c>&lt;Space/&gt;</c>).</summary>
        Space,

        /// <summary>
        /// No gap between the elements around it (<c>&lt;BackSpace/&gt;</c>). The Open API of Tekla 2025 drops it when it
        /// reads mark content; the dialogs write it.
        /// </summary>
        BackSpace,
    }

    /// <summary>A line break, a space or a backspace between elements.</summary>
    public sealed class MarkSeparatorElement : MarkElement
    {
        /// <summary>Creates a separator.</summary>
        public MarkSeparatorElement(MarkSeparatorKind kind)
        {
            Kind = kind;
        }

        /// <summary>A new line.</summary>
        public static MarkSeparatorElement LineFeed() => new MarkSeparatorElement(MarkSeparatorKind.LineFeed);

        /// <summary>A space.</summary>
        public static MarkSeparatorElement Space() => new MarkSeparatorElement(MarkSeparatorKind.Space);

        /// <summary>No gap between the elements around it.</summary>
        public static MarkSeparatorElement BackSpace() => new MarkSeparatorElement(MarkSeparatorKind.BackSpace);

        /// <summary>The kind of separator.</summary>
        public MarkSeparatorKind Kind { get; set; }

        internal static bool TryParse(string tag, out MarkSeparatorKind kind) => Enum.TryParse(tag, ignoreCase: false, out kind) && Enum.IsDefined(typeof(MarkSeparatorKind), kind);

        internal override void Write(MarkXmlWriter w)
        {
            w.Open(Kind.ToString(), null, selfClosing: true);
            w.EndLine();
        }

        internal override bool SameAs(MarkElement other) => other is MarkSeparatorElement o && o.Kind == Kind;

        /// <inheritdoc />
        public override string ToString() => Kind.ToString();
    }

    /// <summary>A symbol: <c>&lt;Symbol symbol="xsteel@85"&gt;</c> (symbol file and index); its format gives colour and height.</summary>
    public sealed class MarkSymbolElement : MarkElement
    {
        internal readonly XmlAttributeList Attributes = new XmlAttributeList("symbol");

        /// <summary>Creates a symbol element: symbol <paramref name="index"/> of <paramref name="file"/> (e.g. <c>xsteel</c>, 85).</summary>
        public MarkSymbolElement(string file, int index, MarkFormat format = null)
        {
            Attributes.Add("symbol", (file ?? throw new ArgumentNullException(nameof(file))) + "@" + MarkXmlNumbers.Int(index));
            Format = format ?? new MarkFormat();
        }

        internal MarkSymbolElement(XmlAttributeList attributes, MarkFormat format)
        {
            Attributes = attributes;
            Format = format;
        }

        /// <summary>Symbol file without extension (before <c>@</c>).</summary>
        public string File
        {
            get
            {
                string symbol = Attributes.Get("symbol") ?? string.Empty;
                int at = symbol.LastIndexOf('@');
                return at < 0 ? symbol : symbol.Substring(0, at);
            }

            set => Attributes.Set("symbol", (value ?? throw new ArgumentNullException(nameof(value))) + "@" + MarkXmlNumbers.Int(Index));
        }

        /// <summary>Index of the symbol in the file (after <c>@</c>).</summary>
        public int Index
        {
            get
            {
                string symbol = Attributes.Get("symbol") ?? string.Empty;
                int at = symbol.LastIndexOf('@');
                return at < 0 ? 0 : MarkXmlNumbers.ParseInt(symbol.Substring(at + 1)) ?? 0;
            }

            set => Attributes.Set("symbol", File + "@" + MarkXmlNumbers.Int(value));
        }

        /// <summary>Colour and height of the symbol.</summary>
        public MarkFormat Format { get; set; }

        internal override void Write(MarkXmlWriter w)
        {
            w.Open("Symbol", Attributes);
            w.EndLine();
            WriteFormat(w, Format);
            w.Close("Symbol");
            w.EndLine();
        }

        internal override bool SameAs(MarkElement other) =>
            other is MarkSymbolElement o && o.Attributes.SameAs(Attributes) && Equals(o.Format, Format);

        /// <inheritdoc />
        public override string ToString() => File + "@" + Index;
    }

    /// <summary>
    /// A frame around elements: <c>&lt;Frame type color frame_id&gt;</c> holding mark content of its own. A frame gets its
    /// <c>frame_id</c> when the content is written: Tekla 2025 read the elements of a frame with id 0, or none, as if they
    /// followed the frame; with another id they were inside it. The files number them from 101.
    /// </summary>
    public sealed class MarkFrameElement : MarkElement
    {
        internal readonly XmlAttributeList Attributes = new XmlAttributeList("type", "color", "frame_id");
        private MarkContent _content;

        /// <summary>Creates a frame around <paramref name="content"/>.</summary>
        public MarkFrameElement(FrameAroundElementsType type, DrawingColor color, MarkContent content)
        {
            Attributes.Add("type", MarkXmlNumbers.Int((int)type));
            Attributes.Add("color", MarkXmlNumbers.Int((int)color));
            Attributes.Add("frame_id", "0");
            Content = content;
        }

        internal MarkFrameElement(XmlAttributeList attributes, MarkContent content)
        {
            Attributes = attributes;
            Content = content;
        }

        /// <summary>Shape of the frame (<c>type</c>; the Open API read 3 as Round).</summary>
        public FrameAroundElementsType Type
        {
            get => (FrameAroundElementsType)(MarkXmlNumbers.ParseInt(Attributes.Get("type")) ?? 0);
            set => Attributes.Set("type", MarkXmlNumbers.Int((int)value));
        }

        /// <summary>Colour of the frame (<c>color</c>).</summary>
        public DrawingColor Color
        {
            get => (DrawingColor)(MarkXmlNumbers.ParseInt(Attributes.Get("color")) ?? (int)DrawingColor.Black);
            set => Attributes.Set("color", MarkXmlNumbers.Int((int)value));
        }

        /// <summary>The elements inside the frame.</summary>
        public MarkContent Content
        {
            get => _content;
            set => _content = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>The id of the frame, if it is one Tekla keeps the elements inside with (not 0, not missing).</summary>
        internal int? ValidId
        {
            get
            {
                int? id = MarkXmlNumbers.ParseInt(Attributes.Get("frame_id"));
                return id != null && id.Value != 0 ? id : null;
            }
        }

        internal override void Write(MarkXmlWriter w)
        {
            XmlAttributeList attributes = Attributes;
            if (ValidId == null)
            {
                // A new frame, or one Tekla would empty: the next free id, for this writing only
                var copy = new XmlAttributeList("type", "color", "frame_id");
                foreach (var pair in Attributes.Items)
                {
                    copy.Add(pair.Key, pair.Value);
                }

                copy.Set("frame_id", MarkXmlNumbers.Int(w.NextFrameId()));
                attributes = copy;
            }

            w.Open("Frame", attributes);
            w.EndLine();
            if (Content.Count == 0)
            {
                // Tekla writes an empty frame as <Mark/>
                w.Raw("<Mark/>");
                w.EndLine();
            }
            else
            {
                w.Raw("<Mark>");
                w.EndLine();
                Content.WriteParts(w);
                w.Raw("</Mark>");
                w.EndLine();
            }

            w.Close("Frame");
            w.EndLine();
        }

        /// <summary>Same shape, colour and elements; a frame Tekla would empty (id 0) differs from one it keeps.</summary>
        internal override bool SameAs(MarkElement other) =>
            other is MarkFrameElement o && o.Type == Type && o.Color == Color && (o.ValidId == null) == (ValidId == null) && o.Content.Equals(Content);

        /// <inheritdoc />
        public override string ToString() => $"Frame {Type} [{string.Join(" ", Content.Select(e => e.ToString()))}]";
    }

    /// <summary>A report template in the mark: <c>&lt;Template filename="..."/&gt;</c>.</summary>
    public sealed class MarkTemplateElement : MarkElement
    {
        internal readonly XmlAttributeList Attributes = new XmlAttributeList("filename", "ObjectId");

        /// <summary>Creates the element of the template <paramref name="fileName"/> (without extension).</summary>
        public MarkTemplateElement(string fileName)
        {
            Attributes.Add("filename", fileName ?? throw new ArgumentNullException(nameof(fileName)));
        }

        internal MarkTemplateElement(XmlAttributeList attributes)
        {
            Attributes = attributes;
        }

        /// <summary>Template file name (<c>filename</c>).</summary>
        public string FileName
        {
            get => Attributes.Get("filename") ?? string.Empty;
            set => Attributes.Set("filename", value ?? string.Empty);
        }

        internal override void Write(MarkXmlWriter w)
        {
            w.Open("Template", Attributes, selfClosing: true);
            w.EndLine();
        }

        internal override bool SameAs(MarkElement other) => other is MarkTemplateElement o && o.Attributes.SameAs(Attributes);

        /// <inheritdoc />
        public override string ToString() => "Template " + FileName;
    }

    /// <summary>
    /// An element this library does not model (the merged mark settings <c>&lt;MergeElement&gt;</c> of reinforcement marks,
    /// <c>&lt;PullOut&gt;</c> pictures, ...), kept as XML.
    /// </summary>
    public sealed class MarkXmlElement : MarkElement
    {
        private string _xml;

        /// <summary>Creates an element from its XML (one element).</summary>
        /// <exception cref="ArgumentException">The text is not one XML element.</exception>
        public MarkXmlElement(string xml)
        {
            Xml = xml;
        }

        /// <summary>The XML of the element.</summary>
        /// <exception cref="ArgumentException">The text is not one XML element.</exception>
        public string Xml
        {
            get => _xml;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                try
                {
                    XElement.Parse(value);
                }
                catch (XmlException exception)
                {
                    throw new ArgumentException("Not one XML element: " + exception.Message, nameof(value), exception);
                }

                _xml = value;
            }
        }

        /// <summary>The tag of the element.</summary>
        public string Tag
        {
            get
            {
                int end = Xml.IndexOfAny(new[] { ' ', '>', '/', '\n', '\t', '\r' }, 1);
                return end > 1 ? Xml.Substring(1, end - 1) : Xml;
            }
        }

        internal override void Write(MarkXmlWriter w)
        {
            w.Raw(Xml);
            w.EndLine();
        }

        internal override bool SameAs(MarkElement other) => other is MarkXmlElement o && o.Xml == Xml;

        /// <inheritdoc />
        public override string ToString() => Tag;
    }
}
