using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TeklaHelper.AssemblyResolver;
using TeklaHelper.DrawingAttribute.Samples.Offline;
using TeklaHelper.DrawingAttribute.Samples.Online;

namespace TeklaHelper.DrawingAttribute.Samples
{
    /// <summary>
    /// A menu of examples. Run without arguments for the menu, or pass the numbers of the examples to run:
    /// <code>
    /// TeklaHelper.DrawingAttribute.Samples.exe                  menu
    /// TeklaHelper.DrawingAttribute.Samples.exe 1 3              examples 1 and 3
    /// TeklaHelper.DrawingAttribute.Samples.exe offline          every example that does not need Tekla Structures
    /// TeklaHelper.DrawingAttribute.Samples.exe 1 --out C:\temp  write the files of the offline examples to C:\temp
    /// </code>
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            // Must run before any method that uses Tekla types is compiled: the resolver loads the Tekla Open API DLLs from
            // the Tekla Structures installed on this machine (none is copied next to this executable)
            TeklaAssemblyResolver.Register();
            return Run(args);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int Run(string[] args)
        {
            IReadOnlyList<Example> examples = new Example[]
            {
                new ViewPropertiesExample(),
                new FilterExample(),
                new GADrawingExample(),
                new AssemblyDrawingExample(),
                new LoadAndModifyExample(),
                new PageFilesExample(),
                new UnmodelledKeysExample(),
                new ModifyCheckBoxesExample(),
                new DrawingObjectsExample(),
                new MarkContentExample(),
                new DimensionPropertiesExample(),
                new GADrawingPagesExample(),
                new FilterObjectTypesExample(),
                new ModelObjectPropertiesExample(),
                new GraphicObjectsExample(),
                new AnnotationsExample(),
                new EmbeddedObjectsExample(),
                new SharedStyleExample(),
                new CodePageExample(),
                new InspectAndCompareExample(),
                new CreateGADrawingExample(),
                new ApplyToOpenDrawingExample(),
                new PlaceAnnotationsExample(),
                new MarkContentInDrawingExample(),
                new RestyleDrawingObjectsExample(),
                new CreateAssemblyDrawingExample(),
            };

            string output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", "attributes");
            var selected = new List<Example>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--out" && i + 1 < args.Length)
                {
                    output = Path.GetFullPath(args[++i]);
                }
                else if (args[i] == "offline")
                {
                    selected.AddRange(examples.Where(e => !e.NeedsTekla));
                }
                else if (int.TryParse(args[i], out int number) && number >= 1 && number <= examples.Count)
                {
                    selected.Add(examples[number - 1]);
                }
                else
                {
                    Console.Error.WriteLine($"Unknown argument '{args[i]}'.");
                    return 2;
                }
            }

            var context = new ExampleContext(output);
            if (selected.Count > 0)
            {
                return selected.Aggregate(0, (result, example) => Math.Max(result, RunExample(examples, example, context)));
            }

            while (true)
            {
                PrintMenu(examples, output);
                Console.Write("Example number (Enter to quit): ");
                string line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    return 0;
                }

                if (int.TryParse(line, out int number) && number >= 1 && number <= examples.Count)
                {
                    RunExample(examples, examples[number - 1], context);
                    Console.WriteLine();
                    Console.Write("Press Enter to go back to the menu.");
                    Console.ReadLine();
                }
            }
        }

        private static void PrintMenu(IReadOnlyList<Example> examples, string output)
        {
            Console.WriteLine();
            Console.WriteLine("TeklaHelper.DrawingAttribute examples");
            Console.WriteLine("Offline examples write to " + output);
            Console.WriteLine();
            for (int i = 0; i < examples.Count; i++)
            {
                Console.WriteLine($"{i + 1,3}. {examples[i].Title}{(examples[i].NeedsTekla ? "  [needs Tekla Structures]" : string.Empty)}");
                Console.WriteLine($"     {examples[i].Description}");
            }

            Console.WriteLine();
        }

        private static int RunExample(IReadOnlyList<Example> examples, Example example, ExampleContext context)
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', 100));
            Console.WriteLine($"{examples.ToList().IndexOf(example) + 1}. {example.Title}");
            Console.WriteLine(new string('=', 100));
            try
            {
                example.Run(context);
                return 0;
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exception);
                Console.ResetColor();
                return 1;
            }
        }
    }
}
