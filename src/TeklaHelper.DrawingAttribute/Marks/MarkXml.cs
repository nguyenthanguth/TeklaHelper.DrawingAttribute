using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TeklaHelper.DrawingAttribute.Marks
{
    /// <summary>
    /// The attributes of one XML element of mark content, in the order they were read or, for new ones, in the order
    /// Tekla writes them, so that content Tekla wrote is written back the same.
    /// </summary>
    internal sealed class XmlAttributeList
    {
        private readonly List<KeyValuePair<string, string>> _items = new List<KeyValuePair<string, string>>();
        private readonly string[] _order;

        internal XmlAttributeList(params string[] order)
        {
            _order = order;
        }

        /// <summary>The attributes of <paramref name="element"/> in the order read; new ones go where <paramref name="order"/> puts them.</summary>
        internal static XmlAttributeList From(XElement element, params string[] order)
        {
            var list = new XmlAttributeList(order);
            foreach (XAttribute attribute in element.Attributes())
            {
                list.Add(attribute.Name.LocalName, attribute.Value);
            }

            return list;
        }

        /// <summary>A copy, with the same order for new attributes.</summary>
        internal XmlAttributeList Clone()
        {
            var copy = new XmlAttributeList(_order);
            copy._items.AddRange(_items);
            return copy;
        }

        internal IEnumerable<KeyValuePair<string, string>> Items => _items;

        internal string Get(string name) => _items.Where(p => p.Key == name).Select(p => p.Value).FirstOrDefault();

        internal void Add(string name, string value) => _items.Add(new KeyValuePair<string, string>(name, value));

        /// <summary>Sets an attribute in place, or inserts it where Tekla's order puts it; null removes it.</summary>
        internal void Set(string name, string value)
        {
            int index = _items.FindIndex(p => p.Key == name);
            if (value == null)
            {
                if (index >= 0)
                {
                    _items.RemoveAt(index);
                }

                return;
            }

            if (index >= 0)
            {
                _items[index] = new KeyValuePair<string, string>(name, value);
                return;
            }

            int rank = Array.IndexOf(_order, name);
            int position = _items.Count;
            if (rank >= 0)
            {
                position = _items.FindIndex(p => Array.IndexOf(_order, p.Key) > rank);
                if (position < 0)
                {
                    position = _items.Count;
                }
            }

            _items.Insert(position, new KeyValuePair<string, string>(name, value));
        }

        /// <summary>Both null, or the same attributes.</summary>
        internal static bool Same(XmlAttributeList a, XmlAttributeList b) => a == null ? b == null : b != null && a.SameAs(b);

        internal bool SameAs(XmlAttributeList other, params string[] ignore) =>
            Normalized(this, ignore).SequenceEqual(Normalized(other, ignore));

        private static IEnumerable<KeyValuePair<string, string>> Normalized(XmlAttributeList list, string[] ignore) =>
            list._items.Where(p => Array.IndexOf(ignore, p.Key) < 0).OrderBy(p => p.Key, StringComparer.Ordinal);
    }

    /// <summary>Writes mark content the way Tekla does: one line per tag (the mark dialogs) or none (the view dialog).</summary>
    internal sealed class MarkXmlWriter
    {
        private readonly StringBuilder _text = new StringBuilder();

        internal MarkXmlWriter(bool lines, string lineBreak = "\n")
        {
            Lines = lines;
            LineBreak = lines ? lineBreak : string.Empty;
        }

        internal bool Lines { get; }

        /// <summary>What ends a line: one line feed, or two in some older files.</summary>
        internal string LineBreak { get; }

        /// <summary>Whether text writes <c>&gt;</c> as <c>&amp;gt;</c>: Tekla wrote both, so parsed content keeps its own way.</summary>
        internal bool EscapeGreaterThan { get; set; } = true;

        /// <summary>
        /// Whether text writes every <c>&lt;</c> as <c>&amp;lt;</c>. False for content read from a file that held it raw
        /// (<c>&lt;&lt; Mark &gt;&gt;</c>): a <c>&lt;</c> no name can follow stays raw, as Tekla wrote it, and the others are
        /// escaped all the same, so that new or changed text cannot turn into a tag.
        /// </summary>
        internal bool EscapeLessThan { get; set; } = true;

        /// <summary>The id the next frame without one takes.</summary>
        internal int FrameId { get; set; } = 101;

        internal int NextFrameId() => FrameId++;

        internal void Raw(string text) => _text.Append(text);

        internal void EndLine()
        {
            _text.Append(LineBreak);
        }

        internal void Open(string tag, XmlAttributeList attributes, bool selfClosing = false)
        {
            _text.Append('<').Append(tag);
            if (attributes != null)
            {
                foreach (KeyValuePair<string, string> pair in attributes.Items)
                {
                    _text.Append(' ').Append(pair.Key).Append("=\"").Append(Escape(pair.Value, attribute: true, greaterThan: false, lessThan: true)).Append('"');
                }
            }

            _text.Append(selfClosing ? "/>" : ">");
        }

        internal void Close(string tag) => _text.Append("</").Append(tag).Append('>');

        internal void Text(string text) => _text.Append(Escape(text, attribute: false, EscapeGreaterThan, EscapeLessThan));

        public override string ToString() => _text.ToString();

        /// <exception cref="ArgumentException">The text holds a character XML cannot carry (a control character).</exception>
        private static string Escape(string text, bool attribute, bool greaterThan, bool lessThan)
        {
            var builder = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if ((c < ' ' && c != '\t' && c != '\n' && c != '\r') || c == '￾' || c == '￿')
                {
                    throw new ArgumentException($"Mark content cannot hold the character U+{(int)c:X4}: \"{text}\".");
                }

                switch (c)
                {
                    case '&':
                        builder.Append("&amp;");
                        break;
                    case '<' when lessThan || i + 1 < text.Length && MarkContent.StartsTag(text[i + 1]):
                        builder.Append("&lt;");
                        break;
                    case '>' when greaterThan:
                        builder.Append("&gt;");
                        break;
                    case '"' when attribute:
                        builder.Append("&quot;");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            return builder.ToString();
        }
    }

    internal static class MarkXmlNumbers
    {
        internal static string Height(double value) => value.ToString("F3", CultureInfo.InvariantCulture);

        internal static string Int(int value) => value.ToString(CultureInfo.InvariantCulture);

        internal static int? ParseInt(string text) =>
            int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : (int?)null;

        internal static double? ParseDouble(string text) =>
            double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ? value : (double?)null;
    }
}
