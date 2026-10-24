using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DialogClassGenerator
{
    /// <summary>
    /// Shortens the generated code: the writers name every type in full (<c>global::TeklaHelper.DrawingAttribute.Pages.X</c>),
    /// which is safe while the code is put together; the file then gets a <c>using</c> per namespace it needs and the
    /// short names. A member of a page named like a type would hide it, which the build of the library reports.
    /// </summary>
    internal static class CodeShortener
    {
        // Longest first, so that a namespace is not taken for the start of a longer one
        private static readonly string[] Namespaces =
        {
            "TeklaHelper.DrawingAttribute.DrawingObjects",
            "TeklaHelper.DrawingAttribute.Drawings",
            "TeklaHelper.DrawingAttribute.Pages",
            "TeklaHelper.DrawingAttribute.Marks",
            "TeklaHelper.DrawingAttribute.Views",
            "TeklaHelper.DrawingAttribute",
            "System.Collections.Generic",
        };

        public static string Shorten(string code)
        {
            Match declared = Regex.Match(code, @"^namespace ([\w.]+)", RegexOptions.Multiline);
            string own = declared.Success ? declared.Groups[1].Value : string.Empty;

            // Names of the members and nested types of the file (declared inside a type, indented twice or more): a type
            // of the same name keeps its namespace there (a property Units of a group type hides the enum Units). Since the
            // files are in TeklaHelper.DrawingAttribute.*, the pass over TeklaHelper.DrawingAttribute then leaves the
            // namespace relative to it: Pages.Units
            var members = new HashSet<string>(StringComparer.Ordinal);
            foreach (Match m in Regex.Matches(code, @"^ {8,}(?:public|private|internal|protected)\b(.*)$", RegexOptions.Multiline))
            {
                string declaration = m.Groups[1].Value;
                Match type = Regex.Match(declaration, @"\b(?:class|struct|enum|interface)\s+(\w+)");
                Match member = Regex.Match(declaration, @"^[^=({;]*?\b(\w+)\s*(?:=>|\{|=|;|\(|$)");
                if (type.Success)
                {
                    members.Add(type.Groups[1].Value);
                }
                else if (member.Success)
                {
                    members.Add(member.Groups[1].Value);
                }
            }

            var used = new SortedSet<string>(StringComparer.Ordinal);
            foreach (string name in Namespaces)
            {
                bool shortened = false;
                code = Regex.Replace(code, "global::" + Regex.Escape(name) + @"\.([A-Z]\w*)", m =>
                {
                    if (members.Contains(m.Groups[1].Value))
                    {
                        return m.Value;
                    }

                    shortened = true;
                    return m.Groups[1].Value;
                });

                // The file's own namespace and the namespaces around it need no using
                if (shortened && name != own && !own.StartsWith(name + ".", StringComparison.Ordinal))
                {
                    used.Add(name);
                }
            }

            if (used.Count == 0 || !declared.Success)
            {
                return code;
            }

            var usings = new StringBuilder();
            foreach (string name in used.OrderBy(n => n.StartsWith("System", StringComparison.Ordinal) ? 0 : 1).ThenBy(n => n, StringComparer.Ordinal))
            {
                usings.Append("using ").Append(name).Append(';').Append('\n');
            }

            usings.Append('\n');
            return code.Insert(declared.Index, usings.ToString());
        }
    }
}
