using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Tools around attribute files: list the files of a folder by extension with their code page and number of keys, and
    /// compare two files key by key (what a dialog changed, or how a file differs from <c>CreateDefault()</c>).
    /// </summary>
    internal sealed class InspectAndCompareExample : Example
    {
        public override string Title => "Inspect a folder of attribute files; compare two files key by key";

        public override string Description => "GenericAttributeFile reads any key-value file; the comparison lists keys only in one file and changed values.";

        public override void Run(ExampleContext context)
        {
            context.Step("Compare CreateDefault() with a copy in which three settings were changed");
            ViewProperties original = ViewProperties.CreateDefault();
            ViewProperties changed = ViewProperties.CreateDefault();
            changed.Attributes.Attributes1.View.Scale = 25;
            changed.Objects.Part.Content.HiddenLines.HiddenLines = !original.Objects.Part.Content.HiddenLines.HiddenLines;
            changed.Marks.PartMark.Content.Font.Color = DrawingColor.Red;
            Compare(original, changed);

            string folder = context.Ask("A folder of attribute files to list", context.OutputDirectory);
            if (!Directory.Exists(folder))
            {
                Console.WriteLine("    No such folder.");
                return;
            }

            context.Step("The key-value files of " + folder + " (three per extension)");
            var byExtension = new SortedDictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (string file in Directory.GetFiles(folder))
            {
                string extension = Path.GetExtension(file);
                if (extension.Length == 0)
                {
                    continue;
                }

                if (!byExtension.TryGetValue(extension, out List<string> files))
                {
                    byExtension[extension] = files = new List<string>();
                }

                files.Add(file);
            }

            foreach (KeyValuePair<string, List<string>> pair in byExtension)
            {
                int listed = 0, checkedFiles = 0;
                foreach (string file in pair.Value)
                {
                    if (listed == 3)
                    {
                        break;
                    }

                    checkedFiles++;
                    if (!MayBeKeyValueFile(file))
                    {
                        continue;
                    }

                    try
                    {
                        var generic = new GenericAttributeFile(pair.Key, writesEncodingLine: !pair.Key.Equals(".more", StringComparison.OrdinalIgnoreCase));
                        generic.Load(file);
                        Console.WriteLine($"    {pair.Key,-14} {Path.GetFileName(file),-40} code page {generic.CodePage,5}, {generic.Values.Count,5} keys");
                        listed++;
                    }
                    catch (Exception exception) when (exception is ArgumentException || exception is FormatException || exception is IOException || exception is UnauthorizedAccessException)
                    {
                        // A line that is no "key value" pair (a key with spaces or quotes, ...): not an attribute file
                    }
                }

                if (listed > 0 && pair.Value.Count > checkedFiles)
                {
                    Console.WriteLine($"    {pair.Key,-14} ... {pair.Value.Count - checkedFiles} more files");
                }
            }
        }

        /// <summary>
        /// Whether a file may be a key-value attribute file, from its first line. Filter files (.vf, .vi.filter, .SObjGrp, ...)
        /// are blocks that FilterFile reads; logs, XML, binary and locked files are not attribute files.
        /// </summary>
        private static bool MayBeKeyValueFile(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    // "encoding 1252" (current files), "view_dial.view_scale ..." (older files without an encoding line) or
                    // "gadrawing_attributes.comment ..." (.more files)
                    string first = (reader.ReadLine() ?? string.Empty).TrimStart('\uFEFF');
                    return Regex.IsMatch(first, @"^(encoding \d+$|[A-Za-z]+_dial\.|[A-Za-z_]+_attributes\.)");
                }
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>Prints the keys only in one file and the keys whose values differ in meaning.</summary>
        private static void Compare(AttributeFile a, AttributeFile b)
        {
            foreach (string key in a.Values.Keys.Where(k => !b.Values.ContainsKey(k)))
            {
                Console.WriteLine($"    only in the first:  {key} {a.Values[key]}");
            }

            foreach (string key in b.Values.Keys.Where(k => !a.Values.ContainsKey(k)))
            {
                Console.WriteLine($"    only in the second: {key} {b.Values[key]}");
            }

            foreach (string key in a.Values.Keys.Where(k => b.Values.ContainsKey(k) && !a.Values[k].HasSameMeaning(b.Values[k])))
            {
                Console.WriteLine($"    changed: {key,-36} {a.Values[key]} -> {b.Values[key]}");
            }
        }
    }
}
