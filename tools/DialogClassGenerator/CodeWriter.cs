using System;
using System.Text;

namespace DialogClassGenerator
{
    /// <summary>
    /// Writes C# with four-space indentation, LF line endings and wrapped XML summaries.
    /// </summary>
    internal sealed class CodeWriter
    {
        private const int Width = 120;

        private readonly StringBuilder _text = new StringBuilder();
        private int _indent;

        public void Line(string line = "")
        {
            // No blank line right after an opening brace
            if (line.Length == 0 && _text.ToString().EndsWith("{\n", StringComparison.Ordinal))
            {
                return;
            }

            _text.Append(line.Length == 0 ? string.Empty : new string(' ', _indent * 4) + line).Append('\n');
        }

        public void Open()
        {
            Line("{");
            _indent++;
        }

        public void Close(string after = "")
        {
            TrimBlankLine();
            _indent--;
            Line("}" + after);
        }

        public void TrimBlankLine()
        {
            while (_text.ToString().EndsWith("\n\n", StringComparison.Ordinal))
            {
                _text.Length--;
            }
        }

        public void Summary(string text)
        {
            Line("/// <summary>");
            string prefix = "/// ";
            int room = Width - _indent * 4 - prefix.Length;
            var line = new StringBuilder();
            foreach (string word in text.Split(' '))
            {
                if (line.Length > 0 && line.Length + 1 + word.Length > room)
                {
                    Line(prefix + line);
                    line.Clear();
                }

                if (line.Length > 0)
                {
                    line.Append(' ');
                }

                line.Append(word);
            }

            if (line.Length > 0)
            {
                Line(prefix + line);
            }

            Line("/// </summary>");
        }

        public override string ToString() => _text.ToString();
    }
}
