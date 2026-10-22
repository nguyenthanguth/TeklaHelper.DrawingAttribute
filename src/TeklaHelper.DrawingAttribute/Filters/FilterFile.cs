using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TeklaHelper.DrawingAttribute.Filters
{
    /// <summary>
    /// A Tekla filter file: the view filter of a drawing view (<c>.vf</c>, <c>.vi.filter</c>), the neighbour part filter
    /// (<c>.vnf</c>, <c>.vi.npfilter</c>), drawing filters (<c>.gd.filter</c>, <c>.gdf</c>, ...) and model filters
    /// (<c>.SObjGrp</c>, <c>.VObjGrp</c>). They all share one block format, the one
    /// <c>Tekla.Structures.Filtering.Filter.CreateFile</c> writes:
    /// <code>
    /// TITLE_OBJECT_GROUP
    /// {
    ///     Version= 1.05
    ///     Count= 1
    ///     SECTION_OBJECT_GROUP
    ///     {
    ///         0                (opening parentheses)
    ///         1                (enabled)
    ///         co_part          (category)
    ///         proNAME          (property)
    ///         albl_Name        (property label)
    ///         ==               (condition)
    ///         albl_Equals      (condition label)
    ///         BEAM             (value)
    ///         0                (closing parentheses)
    ///         Empty            (&amp;&amp;, || or Empty)
    ///         }
    ///     }
    /// </code>
    /// Every token line ends with a space and lines end with CRLF. Spaces inside a token are written as <c>\u0002</c>.
    /// <para>
    /// Work with the rows (<see cref="Rows"/>), which keep the file exactly as Tekla wrote it, or with an expression tree
    /// (<see cref="SetExpression"/>, <see cref="ToExpression"/>):
    /// <code>
    /// var filter = new FilterFile(FilterFileExtensions.ViewFilter);
    /// filter.SetExpression(new FilterGroup()
    ///     .Add(FilterProperties.Part.Name.IsEqual("BEAM"), FilterLogic.Or)
    ///     .Add(FilterProperties.Part.Name.IsEqual("COLUMN")));
    /// filter.SaveTo(attributesDirectory, "beams_and_columns"); // beams_and_columns.vf
    /// </code>
    /// </para>
    /// </summary>
    public sealed class FilterFile : IAttributeFileCompanion
    {
        /// <summary>Version Tekla Structures 2025 writes.</summary>
        public const string DefaultVersion = "1.05";

        /// <summary>UTF-8, the code page <c>Tekla.Structures.Filtering</c> writes.</summary>
        public const int Utf8CodePage = TextEncodings.Utf8CodePage;

        /// <summary>
        /// Most rows a filter Tekla uses can have. Tekla.Structures.Filtering writes more without error (checked with
        /// 1500), so the limit is Tekla's own; <see cref="SetExpression"/> and <see cref="ToBytes"/> throw beyond it.
        /// </summary>
        public const int MaxRows = 1000;

        private const char SpaceMarker = '\u0002';
        private const string TitleToken = "TITLE_OBJECT_GROUP";
        private const string SectionToken = "SECTION_OBJECT_GROUP";
        private const string VersionToken = "Version=";
        private const string CountToken = "Count=";

        private static readonly byte[] NewLine = { (byte)'\r', (byte)'\n' };

        // The header lines as read and the values they held, so an unchanged header is written back unchanged
        private List<byte[]> _sourceHeader;
        private string _sourceVersion;
        private int _sourceCount;
        private int _sourceCodePage;
        private bool _byteOrderMark;
        private string _version = DefaultVersion;


        /// <summary>
        /// Creates an empty filter file.
        /// </summary>
        /// <param name="extension">File extension including the leading dot, e.g. <see cref="FilterFileExtensions.ViewFilter"/>.</param>
        public FilterFile(string extension)
        {
            if (string.IsNullOrEmpty(extension) || extension[0] != '.')
            {
                throw new ArgumentException("Extension must start with a dot, e.g. \".vf\".", nameof(extension));
            }

            Extension = extension;
        }

        /// <summary>
        /// File extension including the leading dot, e.g. <c>.vf</c> or <c>.vi.filter</c>.
        /// </summary>
        public string Extension { get; }

        /// <summary>
        /// Format version, <c>1.05</c> in Tekla Structures 2025 (older files have 1.04 or 1.02).
        /// </summary>
        public string Version
        {
            get => _version;
            set => _version = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Version must not be empty.", nameof(value)) : value.Trim();
        }

        /// <summary>
        /// Rows in order.
        /// </summary>
        public IList<FilterRow> Rows { get; } = new List<FilterRow>();

        /// <summary>
        /// Code page for the tokens: <see cref="Utf8CodePage"/> (what the Tekla Open API writes, and the default), or the
        /// ANSI code page of an older file that is not valid UTF-8 (e.g. 932 for Japanese environments). Loading decides by
        /// the bytes: a file whose bytes are valid UTF-8 is read as UTF-8, otherwise in the fallback code page.
        /// </summary>
        public int CodePage { get; set; } = Utf8CodePage;

        /// <summary>
        /// Loads a filter file. The extension is taken from the file name: the last two parts for <c>name.vi.filter</c>
        /// and <c>name.vi.npfilter</c>, the last part otherwise.
        /// </summary>
        /// <param name="path">File to read.</param>
        /// <param name="fallbackCodePage">Code page for a file that is not valid UTF-8.</param>
        public static FilterFile FromFile(string path, int fallbackCodePage = AttributeFileFormat.DefaultCodePage)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var file = new FilterFile(GetExtension(path));
            file.Load(path, fallbackCodePage);
            return file;
        }

        /// <summary>
        /// Creates a filter file holding <paramref name="expression"/> (see <see cref="SetExpression"/>).
        /// </summary>
        public static FilterFile FromExpression(string extension, FilterExpression expression)
        {
            var file = new FilterFile(extension);
            file.SetExpression(expression);
            return file;
        }

        /// <summary>
        /// Replaces the content with the file at <paramref name="path"/>.
        /// </summary>
        /// <param name="path">File to read.</param>
        /// <param name="fallbackCodePage">Code page for a file that is not valid UTF-8.</param>
        /// <exception cref="FormatException">The file is not a filter file.</exception>
        public void Load(string path, int fallbackCodePage = AttributeFileFormat.DefaultCodePage)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Load(File.ReadAllBytes(path), fallbackCodePage);
        }

        /// <summary>
        /// Replaces the content with a filter file's bytes.
        /// </summary>
        /// <param name="bytes">File content.</param>
        /// <param name="fallbackCodePage">Code page for a file that is not valid UTF-8.</param>
        /// <exception cref="FormatException">The bytes are not a filter file.</exception>
        public void Load(byte[] bytes, int fallbackCodePage = AttributeFileFormat.DefaultCodePage) => Apply(Parse(bytes, fallbackCodePage));

        private static ParsedFilter Parse(byte[] bytes, int fallbackCodePage)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            // No file shipped with Tekla starts with a byte order mark, but editors add one
            bool byteOrderMark = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            if (byteOrderMark)
            {
                var content = new byte[bytes.Length - 3];
                Buffer.BlockCopy(bytes, 3, content, 0, content.Length);
                bytes = content;
            }

            bool isUtf8 = TextEncodings.IsUtf8(bytes, 0, bytes.Length);
            if (byteOrderMark && !isUtf8)
            {
                throw new FormatException("The filter file is not valid UTF-8.");
            }

            int codePage = isUtf8 ? Utf8CodePage : fallbackCodePage;
            Encoding encoding = TextEncodings.Get(codePage);
            List<byte[]> lines = SplitLines(bytes);

            int index = 0;
            var header = new List<byte[]>();
            string version = null;
            int? count = null;

            Expect(lines, ref index, TitleToken, header);
            Expect(lines, ref index, "{", header);
            while (index < lines.Count)
            {
                string token = Decode(lines[index], encoding);
                if (token == SectionToken || token == "}")
                {
                    break;
                }

                if (token.StartsWith(VersionToken, StringComparison.Ordinal))
                {
                    version = token.Substring(VersionToken.Length).Trim();
                }
                else if (token.StartsWith(CountToken, StringComparison.Ordinal)
                    && int.TryParse(token.Substring(CountToken.Length).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int parsedCount))
                {
                    count = parsedCount;
                }
                else
                {
                    throw new FormatException($"Unexpected line '{token}' in the header of a filter file.");
                }

                header.Add(lines[index]);
                index++;
            }

            var rows = new List<FilterRow>();
            while (index < lines.Count && Decode(lines[index], encoding) == SectionToken)
            {
                index++;
                Expect(lines, ref index, "{", null);
                if (index + FilterRow.TokenCount > lines.Count)
                {
                    throw new FormatException("A filter row is cut short.");
                }

                var tokens = new string[FilterRow.TokenCount];
                var tokenLines = new byte[FilterRow.TokenCount][];
                for (int i = 0; i < FilterRow.TokenCount; i++)
                {
                    tokenLines[i] = lines[index];
                    tokens[i] = Decode(lines[index], encoding).Replace(SpaceMarker, ' ');
                    index++;
                }

                Expect(lines, ref index, "}", null);
                try
                {
                    rows.Add(FilterRow.FromTokens(tokens, tokenLines, codePage));
                }
                catch (ArgumentException ex)
                {
                    throw new FormatException($"Filter row {rows.Count + 1} is not valid: {ex.Message}", ex);
                }
            }

            Expect(lines, ref index, "}", null);
            while (index < lines.Count)
            {
                if (lines[index].Length != 0)
                {
                    throw new FormatException("Unexpected content after the end of a filter file.");
                }

                index++;
            }

            if (version == null || count == null)
            {
                throw new FormatException("A filter file must have a Version and a Count line.");
            }

            if (count.Value != rows.Count)
            {
                throw new FormatException($"A filter file says Count= {count.Value} but has {rows.Count} rows.");
            }

            return new ParsedFilter(rows, header, version, count.Value, codePage, byteOrderMark);
        }

        private void Apply(ParsedFilter parsed)
        {
            Rows.Clear();
            foreach (FilterRow row in parsed.Rows)
            {
                Rows.Add(row);
            }

            _version = parsed.Version;
            CodePage = parsed.CodePage;
            _sourceHeader = parsed.Header;
            _sourceVersion = parsed.Version;
            _sourceCount = parsed.Count;
            _sourceCodePage = parsed.CodePage;
            _byteOrderMark = parsed.ByteOrderMark;
        }

        private void Clear()
        {
            Rows.Clear();
            _sourceHeader = null;
            _byteOrderMark = false;
            _version = DefaultVersion;
            CodePage = Utf8CodePage;
        }

        /// <summary>
        /// Saves the filter. The name is the file name of <paramref name="path"/> without <see cref="Extension"/>.
        /// </summary>
        /// <returns>The path of the file that was written.</returns>
        public string Save(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return SaveTo(Path.GetDirectoryName(Path.GetFullPath(path)) ?? string.Empty, AttributeFileName.FromPath(path, Extension));
        }

        /// <summary>
        /// Saves the filter as <paramref name="directory"/>\<paramref name="name"/><see cref="Extension"/>. The directory is
        /// created if needed.
        /// </summary>
        /// <returns>The path of the file that was written.</returns>
        public string SaveTo(string directory, string name)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            AttributeFileName.Validate(name);
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, name + Extension);
            File.WriteAllBytes(path, ToBytes());
            return path;
        }

        /// <summary>
        /// Returns the bytes the filter is saved as.
        /// </summary>
        /// <exception cref="EncoderFallbackException">A token has a character <see cref="CodePage"/> cannot represent.</exception>
        public byte[] ToBytes()
        {
            if (Rows.Count > MaxRows)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "A filter can have at most {0} rows; this one has {1}.", MaxRows, Rows.Count));
            }

            Encoding encoding = TextEncodings.GetForWriting(CodePage);
            using (var stream = new MemoryStream())
            {
                if (_byteOrderMark && CodePage == Utf8CodePage)
                {
                    stream.Write(new byte[] { 0xEF, 0xBB, 0xBF }, 0, 3);
                }

                void WriteLine(byte[] line)
                {
                    stream.Write(line, 0, line.Length);
                    stream.Write(NewLine, 0, NewLine.Length);
                }

                void WriteText(string text) => WriteLine(encoding.GetBytes(text));

                if (_sourceHeader != null && CodePage == _sourceCodePage && Version == _sourceVersion && Rows.Count == _sourceCount)
                {
                    foreach (byte[] line in _sourceHeader)
                    {
                        WriteLine(line);
                    }
                }
                else
                {
                    WriteText(TitleToken + " ");
                    WriteText("{");
                    WriteText("    " + VersionToken + " " + Version + " ");
                    WriteText("    " + CountToken + " " + Rows.Count.ToString(CultureInfo.InvariantCulture) + " ");
                }

                for (int rowIndex = 0; rowIndex < Rows.Count; rowIndex++)
                {
                    FilterRow row = Rows[rowIndex];
                    WriteText("    " + SectionToken + " ");
                    WriteText("    {");
                    string[] tokens = row.GetTokens();
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        byte[] source = row.GetSourceLine(i, tokens[i], CodePage);
                        if (source != null)
                        {
                            WriteLine(source);
                        }
                        else
                        {
                            string token = tokens[i].Replace(' ', SpaceMarker);
                            if (i == FilterRow.ValueTokenIndex && encoding.GetByteCount(token) > FilterRow.MaxValueLength)
                            {
                                throw new InvalidOperationException(string.Format(
                                    CultureInfo.InvariantCulture,
                                    "The value of row {0} takes {1} bytes in code page {2}; Tekla accepts at most {3}.",
                                    rowIndex + 1,
                                    encoding.GetByteCount(token),
                                    CodePage,
                                    FilterRow.MaxValueLength));
                            }

                            WriteText("        " + token + " ");
                        }
                    }

                    WriteText("        }");
                }

                WriteText("    }");
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Returns the text the filter is saved as.
        /// </summary>
        public string ToText() => TextEncodings.GetForWriting(CodePage).GetString(ToBytes());

        /// <summary>
        /// Replaces the rows with <paramref name="expression"/>, the way <c>Tekla.Structures.Filtering</c> writes it: a
        /// group becomes parentheses around its rows (the outermost group too), and the last row of a group takes the
        /// operator of the group's item. Empty groups are left out (Tekla would write an opening parenthesis that is
        /// never closed).
        /// </summary>
        public void SetExpression(FilterExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            List<FilterRow> rows;
            if (expression is FilterCondition condition)
            {
                rows = new List<FilterRow> { new FilterRow(condition) };
            }
            else
            {
                rows = Flatten((FilterGroup)expression);
                if (rows.Count > MaxRows)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The expression makes {0} rows; a filter can have at most {1}.", rows.Count, MaxRows), nameof(expression));
                }

                if (rows.Count > 0)
                {
                    rows[0].OpenParentheses++;
                    rows[rows.Count - 1].CloseParentheses++;
                }
            }

            Rows.Clear();
            foreach (FilterRow row in rows)
            {
                Rows.Add(row);
            }
        }

        /// <summary>
        /// Returns the rows as an expression tree: parentheses become nested groups. A single row without parentheses
        /// becomes a <see cref="FilterCondition"/>; anything else a <see cref="FilterGroup"/> (the parentheses around all
        /// rows that <c>Tekla.Structures.Filtering</c> writes are not an extra level). Parentheses still open after the
        /// last row are closed there.
        /// </summary>
        /// <exception cref="FormatException">A row closes a parenthesis that is not open, or is not a valid condition.</exception>
        public FilterExpression ToExpression()
        {
            var root = new FilterGroup();
            var stack = new Stack<FilterGroup>();
            var owners = new Stack<FilterGroupItem>();
            stack.Push(root);

            foreach (FilterRow row in Rows)
            {
                for (int i = 0; i < row.OpenParentheses; i++)
                {
                    var group = new FilterGroup();
                    var item = new FilterGroupItem(group);
                    stack.Peek().Items.Add(item);
                    stack.Push(group);
                    owners.Push(item);
                }

                FilterCondition condition;
                try
                {
                    condition = row.ToCondition();
                }
                catch (ArgumentException ex)
                {
                    throw new FormatException($"Filter row {Rows.IndexOf(row) + 1} is not a valid condition: {ex.Message}", ex);
                }

                var conditionItem = new FilterGroupItem(condition);
                stack.Peek().Items.Add(conditionItem);

                // The operator of a row belongs to the outermost item that ends with it
                FilterGroupItem last = conditionItem;
                for (int i = 0; i < row.CloseParentheses; i++)
                {
                    if (owners.Count == 0)
                    {
                        throw new FormatException("A filter row closes a parenthesis that is not open.");
                    }

                    stack.Pop();
                    last = owners.Pop();
                }

                last.Logic = row.Logic;
            }

            // A parenthesis still open after the last row (one file shipped with Tekla has it) ends with the filter

            if (root.Items.Count == 1)
            {
                FilterGroupItem only = root.Items[0];
                if (only.Expression is FilterGroup wrapped && wrapped.Items.Count > 0)
                {
                    // Closing the outer parentheses gave the last row's operator to the wrapper; an unclosed wrapper kept it
                    if (only.Logic != FilterLogic.None)
                    {
                        wrapped.Items[wrapped.Items.Count - 1].Logic = only.Logic;
                    }

                    return wrapped;
                }

                if (only.Expression is FilterCondition && only.Logic == FilterLogic.None)
                {
                    return only.Expression;
                }
            }

            return root;
        }

        Action IAttributeFileCompanion.PrepareLoad(string path, int codePage)
        {
            if (path == null)
            {
                return Clear;
            }

            ParsedFilter parsed = Parse(File.ReadAllBytes(path), codePage);
            return () => Apply(parsed);
        }

        AttributeFileSave IAttributeFileCompanion.PrepareSave(string path, string name, int codePage)
        {
            if (Rows.Count > 0)
            {
                byte[] bytes = ToBytes();
                return new AttributeFileSave(() => File.WriteAllBytes(path, bytes));
            }

            // No rows means no filter. An empty filter file is not that: Tekla 2025 shows no objects at all in a view loaded
            // with one. So the companion is removed, which also keeps an older filter of that name from coming back.
            return new AttributeFileSave(() =>
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            });
        }

        private sealed class ParsedFilter
        {
            public ParsedFilter(List<FilterRow> rows, List<byte[]> header, string version, int count, int codePage, bool byteOrderMark)
            {
                Rows = rows;
                Header = header;
                Version = version;
                Count = count;
                CodePage = codePage;
                ByteOrderMark = byteOrderMark;
            }

            public List<FilterRow> Rows { get; }

            public List<byte[]> Header { get; }

            public string Version { get; }

            public int Count { get; }

            public int CodePage { get; }

            public bool ByteOrderMark { get; }
        }

        private static List<FilterRow> Flatten(FilterGroup group)
        {
            var rows = new List<FilterRow>();
            FilterLogic? skippedLogic = null;
            foreach (FilterGroupItem item in group.Items)
            {
                if (item.Expression is FilterCondition condition)
                {
                    rows.Add(new FilterRow(condition, item.Logic));
                    skippedLogic = null;
                    continue;
                }

                List<FilterRow> nested = Flatten((FilterGroup)item.Expression);
                if (nested.Count == 0)
                {
                    skippedLogic = item.Logic;
                    continue;
                }

                skippedLogic = null;

                nested[0].OpenParentheses++;
                FilterRow last = nested[nested.Count - 1];
                last.CloseParentheses++;
                last.Logic = item.Logic;
                rows.AddRange(nested);
            }

            // Empty groups at the end were left out: the last row takes the operator of the last item, like it would
            if (skippedLogic != null && rows.Count > 0)
            {
                rows[rows.Count - 1].Logic = skippedLogic.Value;
            }

            return rows;
        }

        private static string GetExtension(string path)
        {
            string fileName = Path.GetFileName(path);
            int last = fileName.LastIndexOf('.');
            if (last < 0)
            {
                throw new ArgumentException($"'{fileName}' has no extension.", nameof(path));
            }

            string extension = fileName.Substring(last);
            if (extension.Equals(".filter", StringComparison.OrdinalIgnoreCase) || extension.Equals(".npfilter", StringComparison.OrdinalIgnoreCase))
            {
                int previous = fileName.LastIndexOf('.', last - 1);
                if (previous > 0)
                {
                    extension = fileName.Substring(previous);
                }
            }

            return extension;
        }



        private static List<byte[]> SplitLines(byte[] bytes)
        {
            var lines = new List<byte[]>();
            int start = 0;
            while (start <= bytes.Length)
            {
                int end = Array.IndexOf(bytes, (byte)'\n', start);
                if (end < 0)
                {
                    end = bytes.Length;
                }

                int length = (end > start && bytes[end - 1] == '\r' ? end - 1 : end) - start;
                var line = new byte[length];
                Buffer.BlockCopy(bytes, start, line, 0, length);
                lines.Add(line);
                start = end + 1;
            }

            // A file ending with a line break gives one empty line after it
            if (lines.Count > 0 && lines[lines.Count - 1].Length == 0)
            {
                lines.RemoveAt(lines.Count - 1);
            }

            return lines;
        }

        private static string Decode(byte[] line, Encoding encoding)
        {
            int start = 0;
            int end = line.Length;
            while (start < end && line[start] == ' ')
            {
                start++;
            }

            while (end > start && line[end - 1] == ' ')
            {
                end--;
            }

            return encoding.GetString(line, start, end - start);
        }

        private static void Expect(List<byte[]> lines, ref int index, string token, List<byte[]> keep)
        {
            // The structural tokens are ASCII, so they can be compared before the code page matters
            string actual = index < lines.Count ? Decode(lines[index], Encoding.ASCII) : "<end of file>";
            if (actual != token)
            {
                throw new FormatException($"Expected '{token}' in a filter file but found '{actual}'.");
            }

            keep?.Add(lines[index]);
            index++;
        }
    }
}
