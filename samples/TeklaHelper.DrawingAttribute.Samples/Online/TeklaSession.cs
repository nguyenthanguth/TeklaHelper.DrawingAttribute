using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using TSD = Tekla.Structures.Drawing;

namespace TeklaHelper.DrawingAttribute.Samples.Online
{
    /// <summary>
    /// Helpers shared by the examples that talk to Tekla Structures.
    /// </summary>
    internal static class TeklaSession
    {
        /// <summary>
        /// Connects to the running Tekla Structures; null (with a message) when no model is open.
        /// </summary>
        public static Model Connect()
        {
            var model = new Model();
            if (!model.GetConnectionStatus() || string.IsNullOrEmpty(model.GetInfo().ModelPath))
            {
                Console.WriteLine("    Start Tekla Structures 2025 and open a model first.");
                return null;
            }

            Console.WriteLine($"    Connected to model '{model.GetInfo().ModelName}' in {model.GetInfo().ModelPath}");
            return model;
        }

        /// <summary>The attributes folder of the model: Tekla finds attribute files there first.</summary>
        public static string AttributesDirectory(Model model) => Path.Combine(model.GetInfo().ModelPath, "attributes");

        /// <summary>
        /// A new file name for every run. Tekla Structures reads an attribute file once per session and keeps it by name,
        /// so a file written again under a name it already loaded is not read again until Tekla is restarted.
        /// </summary>
        public static string UniqueName(string prefix) => prefix + "_" + DateTime.Now.ToString("yyMMdd_HHmmss", CultureInfo.InvariantCulture);

        /// <summary>
        /// Finds an attribute file the way Tekla looks for it: the model's attributes folder, then the project, firm and
        /// system folders of the advanced options. Null when there is none.
        /// </summary>
        public static string FindAttributeFile(Model model, string fileName)
        {
            var directories = new List<string> { AttributesDirectory(model) };
            foreach (string option in new[] { "XS_PROJECT", "XS_FIRM", "XS_SYSTEM" })
            {
                string value = string.Empty;
                if (TeklaStructuresSettings.GetAdvancedOption(option, ref value) && !string.IsNullOrEmpty(value))
                {
                    directories.AddRange(value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                }
            }

            return directories
                .Select(directory => Path.Combine(directory.Trim(), fileName))
                .FirstOrDefault(File.Exists);
        }

        /// <summary>The box around every part of the model, in global coordinates; null when the model has no parts.</summary>
        public static AABB PartsBox(Model model)
        {
            Point min = null;
            Point max = null;
            ModelObjectEnumerator parts = model.GetModelObjectSelector().GetAllObjectsWithType(new[] { typeof(Part) });
            while (parts.MoveNext())
            {
                if (!(parts.Current is Part part))
                {
                    continue;
                }

                Solid solid = part.GetSolid();
                min = min == null ? new Point(solid.MinimumPoint) : new Point(Math.Min(min.X, solid.MinimumPoint.X), Math.Min(min.Y, solid.MinimumPoint.Y), Math.Min(min.Z, solid.MinimumPoint.Z));
                max = max == null ? new Point(solid.MaximumPoint) : new Point(Math.Max(max.X, solid.MaximumPoint.X), Math.Max(max.Y, solid.MaximumPoint.Y), Math.Max(max.Z, solid.MaximumPoint.Z));
            }

            return min == null ? null : new AABB(min, max);
        }

        /// <summary>The names of the parts of the model, with how many parts have each.</summary>
        public static SortedDictionary<string, int> PartNames(Model model)
        {
            var names = new SortedDictionary<string, int>(StringComparer.Ordinal);
            ModelObjectEnumerator parts = model.GetModelObjectSelector().GetAllObjectsWithType(new[] { typeof(Part) });
            while (parts.MoveNext())
            {
                if (parts.Current is Part part)
                {
                    names[part.Name] = names.TryGetValue(part.Name, out int count) ? count + 1 : 1;
                }
            }

            return names;
        }

        /// <summary>The views selected in the open drawing.</summary>
        public static List<TSD.View> SelectedViews(TSD.DrawingHandler handler)
        {
            var views = new List<TSD.View>();
            TSD.DrawingObjectEnumerator selected = handler.GetDrawingObjectSelector().GetSelected();
            while (selected.MoveNext())
            {
                if (selected.Current is TSD.View view)
                {
                    views.Add(view);
                }
            }

            return views;
        }

        /// <summary>The first selected view of the open drawing, else its first view; null when it has none.</summary>
        public static TSD.View SelectedOrFirstView(TSD.DrawingHandler handler, TSD.Drawing drawing)
        {
            TSD.View view = SelectedViews(handler).FirstOrDefault();
            TSD.DrawingObjectEnumerator views = drawing.GetSheet().GetViews();
            while (view == null && views.MoveNext())
            {
                view = views.Current as TSD.View;
            }

            return view;
        }

        /// <summary>
        /// Deletes the files an example wrote under <paramref name="name"/> (name.vi, name.vi.filter, name.ad.more, ...). The
        /// names are unique per run, so nothing else matches.
        /// </summary>
        public static void DeleteFiles(string directory, string name)
        {
            foreach (string file in Directory.GetFiles(directory, name + ".*"))
            {
                File.Delete(file);
            }
        }

        /// <summary>
        /// Counts the drawing objects of a type in a view. The Drawing API only returns the objects the view shows: a part
        /// hidden by the view filter cannot be found through the view at all.
        /// </summary>
        public static int Count(TSD.View view, Type type)
        {
            int count = 0;
            TSD.DrawingObjectEnumerator objects = view.GetAllObjects(type);
            while (objects.MoveNext())
            {
                count++;
            }

            return count;
        }
    }
}
