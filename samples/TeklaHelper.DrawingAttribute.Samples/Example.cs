using System;
using System.IO;
using System.Linq;
using System.Text;

namespace TeklaHelper.DrawingAttribute.Samples
{
    /// <summary>
    /// One example of the menu.
    /// </summary>
    internal abstract class Example
    {
        /// <summary>Title shown in the menu.</summary>
        public abstract string Title { get; }

        /// <summary>One line on what the example shows.</summary>
        public abstract string Description { get; }

        /// <summary>True when the example talks to a running Tekla Structures.</summary>
        public virtual bool NeedsTekla => false;

        /// <summary>Runs the example.</summary>
        public abstract void Run(ExampleContext context);
    }

    /// <summary>
    /// Where the offline examples write their files, and helpers to show them.
    /// </summary>
    internal sealed class ExampleContext
    {
        public ExampleContext(string outputDirectory)
        {
            OutputDirectory = outputDirectory;
        }

        /// <summary>The folder the offline examples write to (an "attributes" folder, like the one of a model).</summary>
        public string OutputDirectory { get; }

        /// <summary>Prints a step of the example.</summary>
        public void Step(string text)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-- " + text);
            Console.ResetColor();
        }

        /// <summary>Prints a file the example wrote: its name, size and the first lines.</summary>
        public void ShowFile(string path, int maxLines = 25)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{path} ({new FileInfo(path).Length} bytes)");
            Console.ResetColor();

            // Attribute files are in the ANSI code page of their "encoding" line; filter files are UTF-8. Latin-1 shows
            // both well enough for a preview.
            string[] lines = File.ReadAllLines(path, Encoding.GetEncoding(28591));
            foreach (string line in lines.Take(maxLines))
            {
                Console.WriteLine("    " + (line.Length > 110 ? line.Substring(0, 110) + "..." : line).Replace('\u0001', '¶').Replace('\u0002', '·'));
            }

            if (lines.Length > maxLines)
            {
                Console.WriteLine($"    ... {lines.Length - maxLines} more lines");
            }
        }

        /// <summary>
        /// Asks a yes/no question. Answers can also come from redirected input, one per line
        /// (<c>echo y| ...Samples.exe 22</c>); no more input means no.
        /// </summary>
        public bool Confirm(string question)
        {
            Console.Write(question + " [y/N] ");
            string answer = ReadAnswer();
            return string.Equals(answer?.Trim(), "y", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Asks for a text, with a default (also when there is no more redirected input).</summary>
        public string Ask(string question, string defaultValue)
        {
            Console.Write($"{question} [{defaultValue}]: ");
            string answer = ReadAnswer();
            return string.IsNullOrWhiteSpace(answer) ? defaultValue : answer.Trim();
        }

        private static string ReadAnswer()
        {
            string answer = Console.ReadLine();

            // Redirected answers are not echoed by the console: show them, so that the output reads like a session
            if (Console.IsInputRedirected)
            {
                Console.WriteLine(answer ?? string.Empty);
            }

            return answer;
        }
    }
}
