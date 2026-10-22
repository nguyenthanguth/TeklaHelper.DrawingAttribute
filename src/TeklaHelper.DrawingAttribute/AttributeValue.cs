using System;
using System.Globalization;

namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// Kind of a value in an attribute file, as told by how it is written.
    /// </summary>
    public enum AttributeValueKind
    {
        /// <summary>Quoted text, e.g. <c>StyleName "standard"</c>.</summary>
        String,

        /// <summary>Whole number, e.g. <c>cut_on 0</c>. Check boxes are stored as 0 / 1.</summary>
        Integer,

        /// <summary>Number with a decimal point, e.g. <c>view_scale 20.000000</c>.</summary>
        Double,
    }

    /// <summary>
    /// A single value of an attribute file. Keeps the text exactly as it was read, so a file that is loaded and saved
    /// again without changes is written byte for byte the same.
    /// </summary>
    public readonly struct AttributeValue : IEquatable<AttributeValue>
    {
        // Tekla writes line breaks inside a quoted value as the SOH control character
        internal const char LineBreakMarker = '\u0001';

        private readonly byte[] _fileBytes;
        private readonly int _fileCodePage;
        private readonly bool _withoutSeparator;

        private AttributeValue(AttributeValueKind kind, string rawText, byte[] fileBytes = null, int fileCodePage = 0, bool withoutSeparator = false)
        {
            Kind = kind;
            RawText = rawText ?? string.Empty;
            _fileBytes = fileBytes;
            _fileCodePage = fileCodePage;
            _withoutSeparator = withoutSeparator;
        }

        /// <summary>
        /// Kind of the value.
        /// </summary>
        public AttributeValueKind Kind { get; }

        /// <summary>
        /// The value as written in the file, without the quotes of a string value. Line breaks are still the SOH
        /// (<c>\u0001</c>) characters Tekla uses.
        /// </summary>
        public string RawText { get; }

        /// <summary>
        /// Creates a string value. Line breaks (<c>\r\n</c> or <c>\n</c>) are stored the way Tekla stores them.
        /// </summary>
        public static AttributeValue FromString(string value)
        {
            value = (value ?? string.Empty).Replace("\r\n", "\n").Replace('\n', LineBreakMarker);
            return new AttributeValue(AttributeValueKind.String, value);
        }

        /// <summary>
        /// Creates an integer value.
        /// </summary>
        public static AttributeValue FromInt32(int value) =>
            new AttributeValue(AttributeValueKind.Integer, value.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        /// Creates a boolean value, stored as 1 or 0.
        /// </summary>
        public static AttributeValue FromBoolean(bool value) => FromInt32(value ? 1 : 0);

        /// <summary>
        /// Creates a floating point value, written with six decimals like Tekla does (e.g. <c>20.000000</c>).
        /// </summary>
        public static AttributeValue FromDouble(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Attribute files cannot store NaN or infinity.");
            }

            return new AttributeValue(AttributeValueKind.Double, value.ToString("F6", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Creates a value from its text in a file, e.g. <c>"standard"</c>, <c>1</c> or <c>3.500000</c>.
        /// </summary>
        public static AttributeValue Parse(string text)
        {
            text = text ?? string.Empty;

            // Quotes inside a string are not escaped, so the value runs from the first quote to the last one
            if (text.Length > 0 && text[0] == '"')
            {
                int end = text.LastIndexOf('"');
                string inner = end > 0 ? text.Substring(1, end - 1) : text.Substring(1);
                return new AttributeValue(AttributeValueKind.String, inner);
            }

            if (!IsHexadecimal(text) && (text.IndexOf('.') >= 0 || text.IndexOf('e') >= 0 || text.IndexOf('E') >= 0))
            {
                return new AttributeValue(AttributeValueKind.Double, text);
            }

            return new AttributeValue(AttributeValueKind.Integer, text);
        }

        /// <summary>
        /// Creates a value read from a file, remembering its bytes. Some files shipped with Tekla mix encodings inside one
        /// value (UTF-8 and the file's code page), which no decoding can write back unchanged, so an unchanged value is
        /// written back from these bytes.
        /// </summary>
        internal static AttributeValue FromFile(string text, byte[] bytes, int codePage)
        {
            AttributeValue parsed = Parse(text);
            return new AttributeValue(parsed.Kind, parsed.RawText, bytes, codePage);
        }

        /// <summary>
        /// Creates the value of a line that holds a key and nothing else, not even the space after it, so that the line is
        /// written back the same.
        /// </summary>
        internal static AttributeValue FromFileWithoutSeparator(int codePage) =>
            new AttributeValue(AttributeValueKind.Integer, string.Empty, new byte[0], codePage, withoutSeparator: true);

        /// <summary>
        /// True when the value was read from a line without the space after the key, in <paramref name="codePage"/>.
        /// </summary>
        internal bool IsWithoutSeparator(int codePage) => _withoutSeparator && _fileCodePage == codePage;

        /// <summary>
        /// Returns the bytes the value was read from, if it was read from a file in <paramref name="codePage"/>.
        /// </summary>
        internal byte[] GetFileBytes(int codePage) => _fileBytes != null && _fileCodePage == codePage ? _fileBytes : null;

        /// <summary>
        /// The value as text, with SOH markers turned back into <see cref="Environment.NewLine"/>.
        /// </summary>
        public string AsString() => RawText.Replace(LineBreakMarker.ToString(), Environment.NewLine);

        /// <summary>
        /// The value as an integer. A double is rounded; hexadecimal text such as <c>0xFFFFFFFF</c> (found in files saved
        /// from the dialogs, e.g. <c>label_colour</c>) is read as the 32 bits it stands for; text that is not a number
        /// returns <paramref name="defaultValue"/>.
        /// </summary>
        public int AsInt32(int defaultValue = 0)
        {
            if (int.TryParse(RawText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                return result;
            }

            if (IsHexadecimal(RawText)
                && uint.TryParse(RawText.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out uint bits))
            {
                return unchecked((int)bits);
            }

            if (double.TryParse(RawText, NumberStyles.Float, CultureInfo.InvariantCulture, out double d)
                && d >= int.MinValue && d <= int.MaxValue)
            {
                return (int)Math.Round(d);
            }

            return defaultValue;
        }

        /// <summary>
        /// The value as a double; text that is not a number returns <paramref name="defaultValue"/>.
        /// </summary>
        public double AsDouble(double defaultValue = 0)
        {
            return double.TryParse(RawText, NumberStyles.Float, CultureInfo.InvariantCulture, out double result)
                ? result
                : defaultValue;
        }

        /// <summary>
        /// The value as a boolean: any non-zero number is true.
        /// </summary>
        public bool AsBoolean(bool defaultValue = false)
        {
            if (double.TryParse(RawText, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
            {
                return result != 0;
            }

            return defaultValue;
        }

        /// <summary>
        /// True when the value means the same as <paramref name="other"/>: the same text, or the same number however it is
        /// written (<c>-0.000000</c> and <c>0.000000</c>, <c>0xFFFFFFFF</c> and <c>-1</c>).
        /// </summary>
        public bool HasSameMeaning(AttributeValue other)
        {
            if (Kind == AttributeValueKind.String || other.Kind == AttributeValueKind.String)
            {
                return Kind == other.Kind && string.Equals(RawText, other.RawText, StringComparison.Ordinal);
            }

            if (Kind == AttributeValueKind.Integer && other.Kind == AttributeValueKind.Integer)
            {
                const int unset = int.MinValue + 1;
                int value = AsInt32(unset);
                return value != unset && value == other.AsInt32(unset);
            }

            return double.TryParse(RawText, NumberStyles.Float, CultureInfo.InvariantCulture, out double a)
                && double.TryParse(other.RawText, NumberStyles.Float, CultureInfo.InvariantCulture, out double b)
                && Kind == other.Kind
                && a == b;
        }

        private static bool IsHexadecimal(string text) =>
            text.Length > 2 && text[0] == '0' && (text[1] == 'x' || text[1] == 'X');

        /// <summary>
        /// The value as it is written in a file after the key: strings in quotes, numbers as they are.
        /// </summary>
        public string ToFileText() => Kind == AttributeValueKind.String ? "\"" + RawText + "\"" : RawText;

        /// <inheritdoc />
        public override string ToString() => ToFileText();

        /// <inheritdoc />
        public bool Equals(AttributeValue other) => Kind == other.Kind && string.Equals(RawText, other.RawText, StringComparison.Ordinal);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is AttributeValue other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Kind * 397) ^ StringComparer.Ordinal.GetHashCode(RawText ?? string.Empty);
            }
        }

        /// <summary>Compares two values.</summary>
        public static bool operator ==(AttributeValue left, AttributeValue right) => left.Equals(right);

        /// <summary>Compares two values.</summary>
        public static bool operator !=(AttributeValue left, AttributeValue right) => !left.Equals(right);
    }
}
