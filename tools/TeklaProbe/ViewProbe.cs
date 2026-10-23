using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using TeklaHelper.DrawingAttribute;
using TeklaHelper.DrawingAttribute.Views;
using T = Tekla.Structures.Drawing;

namespace TeklaProbe
{
    /// <summary>
    /// A temporary view in the active drawing, with the coordinate systems and restriction box of an existing view, that
    /// loads probe copies of a template .vi file.
    /// <para>
    /// Tekla caches attribute files by name for the whole session, so every load writes a new file name. The template's
    /// filters are removed so that every object of the source view is shown (an empty filter file would hide them all;
    /// without filter rows the library writes no filter file).
    /// </para>
    /// </summary>
    internal sealed class ViewProbe
    {
        private const string ProbeName = "TDA_PROBE";

        private readonly string _attributesDirectory;
        private readonly string _templatePath;
        private readonly string _sourceViewName;
        private readonly string _runId = DateTime.Now.ToString("HHmmss", CultureInfo.InvariantCulture);
        private int _counter;

        public ViewProbe(string attributesDirectory, string templatePath, string sourceViewName)
        {
            _attributesDirectory = attributesDirectory;
            _templatePath = templatePath;
            _sourceViewName = sourceViewName;
        }

        /// <summary>
        /// Lists the types of the drawing objects the probe view shows.
        /// </summary>
        public int ListObjects()
        {
            return WithProbeView(view =>
            {
                var counts = new SortedDictionary<string, int>(StringComparer.Ordinal);
                var all = view.GetAllObjects();
                while (all.MoveNext())
                {
                    string name = all.Current.GetType().Name;
                    counts[name] = counts.TryGetValue(name, out int count) ? count + 1 : 1;
                }

                foreach (KeyValuePair<string, int> pair in counts)
                {
                    Console.WriteLine($"{pair.Key}: {pair.Value}");
                }
            });
        }

        /// <summary>
        /// Changes every key under <paramref name="prefix"/> in turn and writes, per change, the properties of the first
        /// object of <paramref name="objectType"/> that changed: name, old value, new value, changes (tab separated).
        /// </summary>
        public int Map(string prefix, string objectType, string outputPath)
        {
            Type type = DrawingType(objectType);
            ViewProperties template = LoadTemplate();
            var lines = new List<string>();
            int result = WithProbeView(view =>
            {
                Dictionary<string, string> baseline = Snapshot(view, type);
                Console.WriteLine($"{objectType} objects: {Count(view, type)}, properties read: {baseline.Count}");
                if (baseline.Count == 0)
                {
                    return;
                }

                foreach (string key in template.Values.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList())
                {
                    string name = key.Substring(prefix.Length);
                    if (name == "in_file" || name == "out_file")
                    {
                        continue;
                    }

                    AttributeValue original = template.Values[key];
                    foreach (AttributeValue variant in Variants(name, original))
                    {
                        ViewProperties changed = LoadTemplate();
                        changed.Values.Set(key, variant);
                        Apply(view, changed);
                        string line = $"{name}\t{original.ToFileText()}\t{variant.ToFileText()}\t{Diff(baseline, Snapshot(view, type))}";
                        lines.Add(line);
                        Console.WriteLine(line);
                        Apply(view, template);
                    }
                }
            });

            File.WriteAllLines(outputPath, lines);
            return result;
        }

        /// <summary>
        /// Applies each variant (a list of key=value changes made together) and prints what changed.
        /// </summary>
        public int Combos(string objectType, string[] variants)
        {
            Type type = DrawingType(objectType);
            ViewProperties template = LoadTemplate();
            return WithProbeView(view =>
            {
                Dictionary<string, string> baseline = Snapshot(view, type);
                foreach (string variant in variants)
                {
                    ViewProperties changed = LoadTemplate();
                    foreach (string pair in variant.Split(','))
                    {
                        string[] keyValue = pair.Split(new[] { '=' }, 2);
                        changed.Values.Set(keyValue[0], AttributeValue.Parse(keyValue[1]));
                    }

                    Apply(view, changed);
                    Console.WriteLine($"{variant}\t{Diff(baseline, Snapshot(view, type))}");
                    Apply(view, template);
                }
            });
        }

        /// <summary>
        /// What Tekla does with keys a file does not have: loads the template with a few distinct values, then a file with
        /// only <c>view_scale</c>, and prints what changed (kept = missing keys keep the view's values). Then inserts a new
        /// view from that file alone and prints how it differs from the template (Tekla's values for missing keys).
        /// </summary>
        public int Partial(string objectType)
        {
            Type type = DrawingType(objectType);
            return WithProbeView(view =>
            {
                ViewProperties distinct = LoadTemplate();
                distinct.Attributes.Attributes1.View.Scale = 37;
                distinct.Values.Set("vp.visible_colour", AttributeValue.FromInt32(160));
                distinct.Values.Set("vp.draw_hidden", AttributeValue.FromInt32(distinct.GetInt32("vp.draw_hidden") == 0 ? 1 : 0));
                distinct.Values.Set("vp.visible_linetype", AttributeValue.FromInt32(2));
                Apply(view, distinct);
                Dictionary<string, string> before = Combined(view, type);

                var partial = new ViewProperties();
                partial.Attributes.Attributes1.View.Scale = 50;
                Apply(view, partial);
                Dictionary<string, string> after = Combined(view, type);
                Console.WriteLine($"Loading a file with only view_scale into a view (properties read: {before.Count}):");
                Console.WriteLine("  changed: " + Diff(before, after));

                Dictionary<string, string> template = Combined(view, type);
                T.Drawing drawing = view.GetDrawing();
                var fresh = new T.View(drawing.GetSheet(), view.ViewCoordinateSystem, view.DisplayCoordinateSystem, view.RestrictionBox, Save(partial));
                if (!fresh.Insert())
                {
                    Console.WriteLine("Could not insert a view from the partial file.");
                    return;
                }

                try
                {
                    drawing.CommitChanges();
                    Apply(view, LoadTemplate());
                    template = Combined(view, type);
                    Console.WriteLine("New view from a file with only view_scale, compared with the template:");
                    Console.WriteLine("  differs: " + Diff(template, Combined(fresh, type)));
                }
                finally
                {
                    fresh.Delete();
                    drawing.CommitChanges();
                }
            });
        }

        /// <summary>
        /// Prints the attributes of a view loaded from the template (with its filters) and of its first object of the type,
        /// and how many objects of each type the view shows.
        /// </summary>
        public int Show(string objectType)
        {
            Type type = DrawingType(objectType);
            return WithProbeView(view =>
            {
                foreach (KeyValuePair<string, string> pair in Combined(view, type).OrderBy(p => p.Key, StringComparer.Ordinal))
                {
                    Console.WriteLine($"{pair.Key} = {pair.Value}");
                }

                var counts = new SortedDictionary<string, int>(StringComparer.Ordinal);
                var all = view.GetAllObjects();
                while (all.MoveNext())
                {
                    string name = all.Current.GetType().Name;
                    counts[name] = counts.TryGetValue(name, out int count) ? count + 1 : 1;
                }

                Console.WriteLine("objects: " + string.Join(", ", counts.Select(p => $"{p.Key} {p.Value}")));
            });
        }

        /// <summary>The view's own attributes (prefixed view.) and those of its first object of the type.</summary>
        private static Dictionary<string, string> Combined(T.View view, Type type)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            var own = new Dictionary<string, string>(StringComparer.Ordinal);
            Flatten(view.Attributes, string.Empty, own, 0);
            foreach (KeyValuePair<string, string> pair in own)
            {
                result["view" + pair.Key] = pair.Value;
            }

            foreach (KeyValuePair<string, string> pair in Snapshot(view, type))
            {
                result[type.Name + pair.Key] = pair.Value;
            }

            return result;
        }

        private int WithProbeView(Action<T.View> work)
        {
            T.Drawing drawing = new T.DrawingHandler().GetActiveDrawing();
            if (drawing == null)
            {
                Console.Error.WriteLine("Open a drawing in Tekla Structures first.");
                return 1;
            }

            T.View source = FindView(drawing, _sourceViewName);
            T.View probe = null;
            try
            {
                probe = new T.View(drawing.GetSheet(), source.ViewCoordinateSystem, source.DisplayCoordinateSystem, source.RestrictionBox, Save(LoadTemplate()));
                if (!probe.Insert())
                {
                    Console.Error.WriteLine("Could not insert the probe view.");
                    return 1;
                }

                drawing.CommitChanges();
                work(probe);
                return 0;
            }
            finally
            {
                if (probe != null)
                {
                    probe.Delete();
                    drawing.CommitChanges();
                }

                foreach (string file in Directory.GetFiles(_attributesDirectory, ProbeName + "_*.vi*"))
                {
                    File.Delete(file);
                }
            }
        }

        private ViewProperties LoadTemplate()
        {
            var file = AttributeFile.FromFile<ViewProperties>(_templatePath);
            file.Filter.Rows.Clear();
            file.NeighborPartFilter.Rows.Clear();
            return file;
        }

        private string Save(ViewProperties file)
        {
            string name = $"{ProbeName}_{_runId}_{++_counter}";
            file.SaveTo(_attributesDirectory, name);
            return name;
        }

        private void Apply(T.View view, ViewProperties file)
        {
            view.Attributes.LoadAttributes(Save(file));
            view.Modify();
        }

        private static IEnumerable<AttributeValue> Variants(string name, AttributeValue original)
        {
            string lower = name.ToLowerInvariant();
            if (original.Kind == AttributeValueKind.String)
            {
                yield return AttributeValue.FromString(lower.Contains("hatch_name") ? "ANSI31" : "TDA");
                yield break;
            }

            if (original.Kind == AttributeValueKind.Double)
            {
                yield return AttributeValue.FromDouble(original.AsDouble() + 12.5);
                yield break;
            }

            int value = original.AsInt32();
            if (lower == "draw_as" || lower.EndsWith("linetype", StringComparison.Ordinal) || lower.EndsWith("_type", StringComparison.Ordinal) || lower == "boltdrawas")
            {
                // A sweep tells what every value means
                for (int i = 0; i <= 10; i++)
                {
                    if (i != value)
                    {
                        yield return AttributeValue.FromInt32(i);
                    }
                }

                yield break;
            }

            if (lower.Contains("colour") || lower.Contains("color"))
            {
                foreach (int colour in new[] { 3, 160 }.Where(c => c != value))
                {
                    yield return AttributeValue.FromInt32(colour);
                }

                yield break;
            }

            yield return AttributeValue.FromInt32(value == 0 ? 1 : value == 1 ? 0 : value + 1);
        }

        private static Type DrawingType(string name) => typeof(T.Part).Assembly.GetType("Tekla.Structures.Drawing." + name, true);

        private static T.View FindView(T.Drawing drawing, string name)
        {
            var views = drawing.GetSheet().GetViews();
            while (views.MoveNext())
            {
                if (views.Current is T.View view && view.Name == name)
                {
                    return view;
                }
            }

            var names = new List<string>();
            views.Reset();
            while (views.MoveNext())
            {
                if (views.Current is T.View view)
                {
                    names.Add("'" + view.Name + "'");
                }
            }

            throw new InvalidOperationException($"The active drawing has no view named '{name}'. Its views: {string.Join(", ", names)}");
        }

        private static int Count(T.View view, Type type)
        {
            int count = 0;
            var objects = view.GetAllObjects(type);
            while (objects.MoveNext())
            {
                count++;
            }

            return count;
        }

        private static string Diff(Dictionary<string, string> before, Dictionary<string, string> after)
        {
            string diff = string.Join("; ", after
                .Where(p => !before.TryGetValue(p.Key, out string old) || old != p.Value)
                .Select(p => $"{p.Key}: {(before.TryGetValue(p.Key, out string old) ? old : "?")} -> {p.Value}"));
            return diff.Length == 0 ? "-" : diff;
        }

        /// <summary>
        /// The attributes of the first object of the type in the view, flattened to path = value.
        /// </summary>
        private static Dictionary<string, string> Snapshot(T.View view, Type type)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            var objects = view.GetAllObjects(type);
            if (!objects.MoveNext())
            {
                return result;
            }

            object drawingObject = objects.Current;

            // Part declares its own Attributes over the one of its base class: take the most derived
            PropertyInfo attributes = drawingObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name == "Attributes")
                .OrderByDescending(p => Depth(p.DeclaringType))
                .First();
            Flatten(attributes.GetValue(drawingObject), string.Empty, result, 0);
            return result;
        }

        private static int Depth(Type type)
        {
            int depth = 0;
            for (; type != null; type = type.BaseType)
            {
                depth++;
            }

            return depth;
        }

        private static void Flatten(object value, string path, Dictionary<string, string> into, int depth)
        {
            if (value == null)
            {
                into[path] = "null";
                return;
            }

            Type type = value.GetType();
            if (value is string || type.IsPrimitive || type.IsEnum || value is decimal)
            {
                into[path] = Convert.ToString(value, CultureInfo.InvariantCulture) + (type.IsEnum ? "(" + Convert.ToInt64(value, CultureInfo.InvariantCulture) + ")" : string.Empty);
                return;
            }

            // TrueColor and friends throw for the unnamed colour numbers and only repeat the colour
            if (depth > 4 || type.Name == "Color" || type.Name == "TeklaDrawingColor")
            {
                return;
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0 && !p.Name.StartsWith("True", StringComparison.Ordinal))
                .ToList();
            if (properties.Count == 0)
            {
                // Value objects such as LineTypes keep their state in fields
                FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                into[path] = fields.Length == 0 ? value.ToString() : string.Join(",", fields.Select(f => f.Name + "=" + f.GetValue(value)));
                return;
            }

            foreach (PropertyInfo property in properties)
            {
                object child;
                try
                {
                    child = property.GetValue(value);
                }
                catch (TargetInvocationException)
                {
                    continue;
                }

                Flatten(child, path.Length == 0 ? property.Name : path + "." + property.Name, into, depth + 1);
            }
        }
    }
}
