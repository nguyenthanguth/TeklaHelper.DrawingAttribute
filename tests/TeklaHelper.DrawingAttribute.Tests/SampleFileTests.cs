using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TeklaHelper.DrawingAttribute.Filters;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>
    /// Reads and writes back every attribute and filter file under the folder in the TEKLA_ATTRIBUTE_SAMPLES environment
    /// variable, e.g. C:\TeklaStructures\2025.0\Environments. The tests do nothing when the variable is not set (as on CI).
    /// </summary>
    public class SampleFileTests
    {
        // Model option (.opt) and analysis (.admodel) files are several blocks with an encoding line each, which is not a
        // drawing attribute file
        private static readonly string[] OtherFormats = { ".opt", ".admodel" };

        [Fact]
        public void AttributeFiles_ReadThenWrite_AreIdentical()
        {
            // Key-value attribute files have many extensions (.gd, .vi, .vg, .vpm, .dim, ...), so they are recognized by
            // their encoding line; .more files have none
            var failures = new List<string>();
            foreach (string file in EnumerateSamples(f => f.EndsWith(".more", StringComparison.OrdinalIgnoreCase) || StartsWith(f, "encoding ")))
            {
                byte[] original = File.ReadAllBytes(file);
                AttributeFileContent content = AttributeFileFormat.Read(original);
                byte[] written = AttributeFileFormat.ToBytes(content.Values, content.CodePage, content.HasEncodingLine);

                // Some files shipped with Tekla have no line break after the last line
                string difference = FirstDifference(TrimEnd(original), TrimEnd(written));
                if (difference != null)
                {
                    failures.Add(file + Environment.NewLine + difference);
                }
            }

            AssertNoFailures(failures);
        }

        [Fact]
        public void FilterFiles_ReadThenWrite_AreIdentical()
        {
            var failures = new List<string>();
            foreach (string file in EnumerateSamples(f => StartsWith(f, "TITLE_OBJECT_GROUP")))
            {
                byte[] original = File.ReadAllBytes(file);
                try
                {
                    FilterFile filter = FilterFile.FromFile(file);
                    string difference = FirstDifference(original, filter.ToBytes());
                    if (difference != null)
                    {
                        failures.Add(file + Environment.NewLine + difference);
                    }
                }
                catch (FormatException ex)
                {
                    failures.Add(file + Environment.NewLine + "  " + ex.Message);
                }
            }

            AssertNoFailures(failures);
        }

        [Fact]
        public void FilterFiles_ToExpression_KeepsConditionsOperatorsAndParentheses()
        {
            var failures = new List<string>();
            foreach (string file in EnumerateSamples(f => StartsWith(f, "TITLE_OBJECT_GROUP")))
            {
                try
                {
                    FilterFile filter = FilterFile.FromFile(file);
                    FilterFile rewritten = FilterFile.FromExpression(filter.Extension, filter.ToExpression());

                    string expected = Normalize(filter.Rows);
                    string actual = Normalize(rewritten.Rows);
                    if (expected != actual)
                    {
                        failures.Add(file + Environment.NewLine + "  expected: " + expected + Environment.NewLine + "  actual:   " + actual);
                    }
                }
                catch (FormatException ex)
                {
                    failures.Add(file + Environment.NewLine + "  " + ex.Message);
                }
            }

            AssertNoFailures(failures);
        }

        /// <summary>
        /// The rows as one line of conditions, parentheses and operators, ignoring the operator of the last row (it joins
        /// nothing), parentheses never closed (closed at the end) and parentheses around everything (Tekla.Structures.Filtering
        /// adds them, the dialogs do not).
        /// </summary>
        private static string Normalize(IList<FilterRow> rows)
        {
            var parts = new List<string>();
            int depth = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                FilterRow row = rows[i];
                depth += row.OpenParentheses - row.CloseParentheses;
                parts.Add(new string('(', row.OpenParentheses)
                    + $"{(row.Enabled ? string.Empty : "!")}{row.Category}|{row.Property}|{row.Condition}|{row.Value}".Replace('(', '[').Replace(')', ']')
                    + new string(')', row.CloseParentheses)
                    + (i < rows.Count - 1 ? " " + row.Logic : string.Empty));
            }

            string text = string.Join(" ", parts) + new string(')', Math.Max(0, depth));
            while (IsWrapped(text))
            {
                text = text.Substring(1, text.Length - 2);
            }

            return text;
        }

        private static bool IsWrapped(string text)
        {
            if (text.Length < 2 || text[0] != '(' || text[text.Length - 1] != ')')
            {
                return false;
            }

            // The first parenthesis must close at the very end
            int depth = 0;
            for (int i = 0; i < text.Length; i++)
            {
                depth += text[i] == '(' ? 1 : text[i] == ')' ? -1 : 0;
                if (depth == 0)
                {
                    return i == text.Length - 1;
                }
            }

            return false;
        }

        private static IEnumerable<string> EnumerateSamples(Func<string, bool> predicate)
        {
            string root = Environment.GetEnvironmentVariable("TEKLA_ATTRIBUTE_SAMPLES");
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return Enumerable.Empty<string>();
            }

            return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Where(f => !OtherFormats.Any(e => f.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
                .Where(predicate);
        }

        private static void AssertNoFailures(List<string> failures) =>
            Assert.True(failures.Count == 0, failures.Count + " file(s) failed:" + Environment.NewLine + string.Join(Environment.NewLine, failures.Take(10)));

        private static bool StartsWith(string path, string text)
        {
            var buffer = new byte[text.Length];
            try
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    return stream.Read(buffer, 0, buffer.Length) == buffer.Length && Encoding.ASCII.GetString(buffer) == text;
                }
            }
            catch (IOException)
            {
                // Files Tekla keeps open, e.g. the log of a running session
                return false;
            }
        }

        private static string FirstDifference(byte[] expected, byte[] actual)
        {
            int length = Math.Min(expected.Length, actual.Length);
            int index = 0;
            while (index < length && expected[index] == actual[index])
            {
                index++;
            }

            if (index == expected.Length && index == actual.Length)
            {
                return null;
            }

            int lineStart = Array.LastIndexOf(expected, (byte)'\n', Math.Max(0, index - 1)) + 1;
            int lineEnd = Array.IndexOf(expected, (byte)'\n', lineStart);
            string key = Encoding.ASCII.GetString(expected, lineStart, Math.Max(0, Math.Min((lineEnd < 0 ? expected.Length : lineEnd) - lineStart, 60)));
            int from = Math.Max(lineStart, index - 16);
            string Bytes(byte[] bytes) => BitConverter.ToString(bytes, Math.Min(from, bytes.Length), Math.Max(0, Math.Min(48, bytes.Length - from)));

            return "  line:     " + key + Environment.NewLine + "  expected: " + Bytes(expected) + Environment.NewLine + "  actual:   " + Bytes(actual);
        }

        private static byte[] TrimEnd(byte[] bytes)
        {
            int length = bytes.Length;
            while (length > 0 && (bytes[length - 1] == '\r' || bytes[length - 1] == '\n'))
            {
                length--;
            }

            return bytes.Take(length).ToArray();
        }
    }
}
