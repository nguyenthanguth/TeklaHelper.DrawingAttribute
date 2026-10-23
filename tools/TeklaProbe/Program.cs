using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TeklaProbe
{
    /// <summary>
    /// Probes a running Tekla Structures: inserts a temporary view in the active drawing, loads copies of a complete .vi
    /// file into it with one key changed at a time, and reports which properties of the drawing objects changed. The
    /// temporary view and every probe file are removed at the end.
    /// <code>
    /// TeklaProbe objects &lt;attributes folder&gt; &lt;template .vi&gt; &lt;source view name&gt;
    /// TeklaProbe map     &lt;attributes folder&gt; &lt;template .vi&gt; &lt;source view name&gt; &lt;key prefix, e.g. vp.&gt; &lt;object type, e.g. Part&gt; &lt;output .tsv&gt;
    /// TeklaProbe show    &lt;attributes folder&gt; &lt;template .vi&gt; &lt;source view name&gt; &lt;object type, e.g. Part&gt;
    /// TeklaProbe partial &lt;attributes folder&gt; &lt;template .vi&gt; &lt;source view name&gt; &lt;object type, e.g. Part&gt;
    /// TeklaProbe combo   &lt;attributes folder&gt; &lt;template .vi&gt; &lt;source view name&gt; &lt;object type&gt; "key=value,key=value" ...
    /// </code>
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            // Tekla.Structures*.dll are not copied next to the probe; they and their dependencies load from the Tekla bin folder
            string bin = typeof(Program).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(a => a.Key == "TeklaBinDirectory").Value;
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                string path = Path.Combine(bin, new AssemblyName(e.Name).Name + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };

            if (args.Length < 4)
            {
                Console.Error.WriteLine("Usage: TeklaProbe objects|map|combo <attributes folder> <template .vi> <source view> ...");
                return 2;
            }

            return Run(args);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int Run(string[] args)
        {
            var probe = new ViewProbe(args[1], args[2], args[3]);
            switch (args[0])
            {
                case "objects":
                    return probe.ListObjects();
                case "map":
                    return probe.Map(args[4], args[5], args[6]);
                case "show":
                    return probe.Show(args[4]);
                case "partial":
                    return probe.Partial(args[4]);
                case "combo":
                    return probe.Combos(args[4], args.Skip(5).ToArray());
                default:
                    Console.Error.WriteLine("Unknown mode " + args[0]);
                    return 2;
            }
        }
    }
}
