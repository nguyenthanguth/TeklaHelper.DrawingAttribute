using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// Reads and writes the text format shared by Tekla Structures attribute files (.gd, .vi, .ad, .dim, .gd.more, ...).
    /// <para>
    /// Each line is a key, one space and a value: <c>view_scale 20.000000</c>, <c>cut_on 0</c>, <c>StyleName "standard"</c>.
    /// Lines end with CRLF. Strings are quoted, quotes inside them are not escaped, and line breaks inside them are written
    /// as the SOH character (<c>\u0001</c>). Doubles have six decimals.
    /// </para>
    /// <para>
    /// Current files start with <c>encoding 1252</c>, the ANSI code page the rest of the file is written in (932 for
    /// Japanese, 949 for Korean, ...). Older files have no encoding line and may prefix every key with the dialog name,
    /// e.g. <c>view_dial.view_scale</c>. The .more files (user-defined attributes) never have an encoding line.
    /// Mark contents are XML documents declaring <c>encoding="UTF-8"</c>, and those values are written in UTF-8 whatever
    /// the code page of the file.
    /// </para>
    /// </summary>
    public static class AttributeFileFormat
    {
        /// <summary>
        /// Code page used when a file does not name one: Windows-1252 (Western European).
        /// </summary>
        public const int DefaultCodePage = 1252;

        private const string EncodingKey = "encoding";
        private const string NewLine = "\r\n";

        /// <summary>
        /// Returns the encoding for a Windows code page, e.g. 1252, 932, 949 or 1258.
        /// </summary>
        public static Encoding GetEncoding(int codePage) => TextEncodings.Get(codePage);

        /// <summary>
        /// Reads an attribute file.
        /// </summary>
        /// <param name="path">File to read.</param>
        /// <param name="fallbackCodePage">Code page used when the file has no encoding line.</param>
        public static AttributeFileContent Read(string path, int fallbackCodePage = DefaultCodePage)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return Read(File.ReadAllBytes(path), fallbackCodePage);
        }

        /// <summary>
        /// Reads an attribute file from a stream.
        /// </summary>
        public static AttributeFileContent Read(Stream stream, int fallbackCodePage = DefaultCodePage)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var buffer = new MemoryStream())
            {
                stream.CopyTo(buffer);
                return Read(buffer.ToArray(), fallbackCodePage);
            }
        }

        /// <summary>
        /// Reads an attribute file from its bytes.
        /// </summary>
        public static AttributeFileContent Read(byte[] bytes, int fallbackCodePage = DefaultCodePage)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            // The encoding line itself is ASCII, so it can be read before the code page is known
            int? codePage = ReadCodePage(bytes);
            Encoding encoding = GetEncoding(codePage ?? fallbackCodePage);

            var content = new AttributeFileContent
            {
                CodePage = codePage ?? fallbackCodePage,
                HasEncodingLine = codePage.HasValue,
            };

            // Decoded line by line because a value can be in UTF-8 instead of the file's code page (see IsUtf8Xml)
            int position = 0;
            bool first = true;
            while (position < bytes.Length)
            {
                int end = Array.IndexOf(bytes, (byte)'\n', position);
                if (end < 0)
                {
                    end = bytes.Length;
                }

                int lineEnd = end > position && bytes[end - 1] == '\r' ? end - 1 : end;
                if (!(first && codePage.HasValue) && lineEnd > position)
                {
                    ParseLine(bytes, position, lineEnd - position, encoding, content.Values);
                }

                first = false;
                position = end + 1;
            }

            return content;
        }

        /// <summary>
        /// Parses the text of an attribute file that is already decoded. An encoding line, if any, sets
        /// <see cref="AttributeFileContent.CodePage"/>.
        /// </summary>
        public static AttributeFileContent Parse(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var content = new AttributeFileContent { CodePage = DefaultCodePage };
            string firstLine = ReadFirstLine(text);
            if (TryParseEncodingLine(firstLine, out int codePage))
            {
                content.CodePage = codePage;
                content.HasEncodingLine = true;
            }

            ParseLines(text, content.Values, content.HasEncodingLine);
            return content;
        }

        /// <summary>
        /// Writes an attribute file.
        /// </summary>
        /// <param name="path">File to write; the folder must exist.</param>
        /// <param name="values">Keys and values to write, in order.</param>
        /// <param name="codePage">Code page the file is written in.</param>
        /// <param name="writeEncodingLine">Whether to start the file with <c>encoding &lt;codePage&gt;</c>.</param>
        /// <exception cref="EncoderFallbackException">A value contains a character the code page cannot represent.</exception>
        public static void Write(string path, IEnumerable<KeyValuePair<string, AttributeValue>> values, int codePage = DefaultCodePage, bool writeEncodingLine = true)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            File.WriteAllBytes(path, ToBytes(values, codePage, writeEncodingLine));
        }

        /// <summary>
        /// Returns the bytes of an attribute file.
        /// </summary>
        /// <exception cref="EncoderFallbackException">A value contains a character the code page cannot represent.</exception>
        public static byte[] ToBytes(IEnumerable<KeyValuePair<string, AttributeValue>> values, int codePage = DefaultCodePage, bool writeEncodingLine = true)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            // Fail instead of silently writing '?' for characters the code page does not have
            Encoding encoding = TextEncodings.GetForWriting(codePage);

            using (var stream = new MemoryStream())
            {
                void WriteText(Encoding textEncoding, string text)
                {
                    byte[] bytes = textEncoding.GetBytes(text);
                    stream.Write(bytes, 0, bytes.Length);
                }

                if (writeEncodingLine)
                {
                    WriteText(encoding, EncodingKey + " " + codePage.ToString(CultureInfo.InvariantCulture) + NewLine);
                }

                foreach (KeyValuePair<string, AttributeValue> pair in values)
                {
                    AttributeValue value = pair.Value;
                    if (value.IsWithoutSeparator(codePage))
                    {
                        WriteText(encoding, pair.Key + NewLine);
                        continue;
                    }

                    WriteText(encoding, pair.Key + " ");

                    byte[] fileBytes = value.GetFileBytes(codePage);
                    if (fileBytes != null)
                    {
                        stream.Write(fileBytes, 0, fileBytes.Length);
                    }
                    else
                    {
                        bool isUtf8 = value.Kind == AttributeValueKind.String && IsUtf8Xml(value.RawText);
                        WriteText(isUtf8 ? TextEncodings.StrictUtf8 : encoding, value.ToFileText());
                    }

                    WriteText(encoding, NewLine);
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Returns the text of an attribute file.
        /// </summary>
        public static string ToText(IEnumerable<KeyValuePair<string, AttributeValue>> values, int codePage = DefaultCodePage, bool writeEncodingLine = true)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var builder = new StringBuilder();
            if (writeEncodingLine)
            {
                builder.Append(EncodingKey).Append(' ').Append(codePage.ToString(CultureInfo.InvariantCulture)).Append(NewLine);
            }

            foreach (KeyValuePair<string, AttributeValue> pair in values)
            {
                builder.Append(pair.Key).Append(' ').Append(pair.Value.ToFileText()).Append(NewLine);
            }

            return builder.ToString();
        }

        private static void ParseLines(string text, AttributeCollection values, bool skipEncodingLine)
        {
            using (var reader = new StringReader(text))
            {
                string line;
                bool first = true;
                while ((line = reader.ReadLine()) != null)
                {
                    if (first)
                    {
                        first = false;
                        if (skipEncodingLine)
                        {
                            continue;
                        }
                    }

                    if (line.Length == 0)
                    {
                        continue;
                    }

                    int space = line.IndexOf(' ');
                    string key = space < 0 ? line : line.Substring(0, space);
                    string valueText = space < 0 ? string.Empty : line.Substring(space + 1);
                    if (key.Length == 0)
                    {
                        continue;
                    }

                    values.Set(key, AttributeValue.Parse(valueText));
                }
            }
        }

        private static void ParseLine(byte[] bytes, int start, int count, Encoding encoding, AttributeCollection values)
        {
            // Keys are ASCII, and no byte of a multi-byte character in the supported code pages is a space
            int space = Array.IndexOf(bytes, (byte)' ', start, count);
            int keyLength = space < 0 ? count : space - start;
            if (keyLength == 0)
            {
                return;
            }

            string key = encoding.GetString(bytes, start, keyLength);
            if (space < 0)
            {
                values.Set(key, AttributeValue.FromFileWithoutSeparator(encoding.CodePage));
                return;
            }

            int valueStart = space + 1;
            int valueCount = start + count - valueStart;
            if (!IsUtf8Xml(bytes, valueStart, valueCount) || !TextEncodings.TryDecodeUtf8(bytes, valueStart, valueCount, out string valueText))
            {
                valueText = encoding.GetString(bytes, valueStart, valueCount);
            }

            var valueBytes = new byte[valueCount];
            Buffer.BlockCopy(bytes, valueStart, valueBytes, 0, valueCount);
            values.Set(key, AttributeValue.FromFile(valueText, valueBytes, encoding.CodePage));
        }


        /// <summary>
        /// Mark contents (keys like <c>vpm.ContentString1</c>) are XML documents that declare
        /// <c>encoding="UTF-8"</c>, and Tekla writes them in UTF-8 even when the rest of the file uses another code page.
        /// </summary>
        private static bool IsUtf8Xml(string rawText)
        {
            if (!rawText.StartsWith("<?xml", StringComparison.Ordinal))
            {
                return false;
            }

            int end = rawText.IndexOf("?>", StringComparison.Ordinal);
            string declaration = end < 0 ? rawText : rawText.Substring(0, end);
            return declaration.IndexOf("encoding=\"UTF-8\"", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsUtf8Xml(byte[] bytes, int start, int count)
        {
            // The XML declaration is ASCII, so it can be checked before the value is decoded
            const string Start = "\"<?xml";
            if (count < Start.Length || bytes[start] != '"' || bytes[start + 1] != '<')
            {
                return false;
            }

            int length = Math.Min(count - 1, 128);
            return IsUtf8Xml(Encoding.ASCII.GetString(bytes, start + 1, length));
        }

        private static int? ReadCodePage(byte[] bytes)
        {
            int length = 0;
            while (length < bytes.Length && length < 64 && bytes[length] != '\r' && bytes[length] != '\n')
            {
                length++;
            }

            string firstLine = Encoding.ASCII.GetString(bytes, 0, length);
            return TryParseEncodingLine(firstLine, out int codePage) ? codePage : (int?)null;
        }

        private static string ReadFirstLine(string text)
        {
            int end = text.IndexOfAny(new[] { '\r', '\n' });
            return end < 0 ? text : text.Substring(0, end);
        }

        private static bool TryParseEncodingLine(string line, out int codePage)
        {
            codePage = 0;
            if (!line.StartsWith(EncodingKey + " ", StringComparison.Ordinal))
            {
                return false;
            }

            return int.TryParse(line.Substring(EncodingKey.Length + 1).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out codePage)
                && codePage > 0;
        }
    }

    /// <summary>
    /// What <see cref="AttributeFileFormat"/> read from a file.
    /// </summary>
    public sealed class AttributeFileContent
    {
        /// <summary>
        /// Keys and values, in file order. The encoding line is not included.
        /// </summary>
        public AttributeCollection Values { get; } = new AttributeCollection();

        /// <summary>
        /// Code page of the file: the one on its encoding line, or the fallback code page.
        /// </summary>
        public int CodePage { get; set; }

        /// <summary>
        /// Whether the file started with an encoding line.
        /// </summary>
        public bool HasEncodingLine { get; set; }
    }
}
