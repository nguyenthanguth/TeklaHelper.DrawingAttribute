using System;
using System.Collections.Generic;
using System.IO;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using TSD = Tekla.Structures.Drawing;

namespace TeklaHelper.DrawingAttribute.Samples.Online
{
    /// <summary>
    /// Changes the views of the open drawing through attribute files: for each selected view (or every view), writes a
    /// .vi that shows hidden lines and only the parts of one profile, loads it into the view and counts the parts the API
    /// sees before and after.
    /// <para>
    /// LoadAttributes replaces every setting of the view with the file's, so the file starts from a complete one
    /// (standard.vi, found the way Tekla finds it) and keeps the view's scale.
    /// </para>
    /// </summary>
    internal sealed class ApplyToOpenDrawingExample : Example
    {
        public override string Title => "Apply generated view properties to the views of the open drawing";

        public override string Description => "view.Attributes.LoadAttributes(file) + Modify on the selected views; counts the parts before and after.";

        public override bool NeedsTekla => true;

        public override void Run(ExampleContext context)
        {
            context.Step("Connect to Tekla Structures and find the views");
            Model model = TeklaSession.Connect();
            if (model == null)
            {
                return;
            }

            var handler = new DrawingHandler();
            Drawing drawing = handler.GetActiveDrawing();
            if (drawing == null)
            {
                Console.WriteLine("    Open a drawing first (and select views to change only those).");
                return;
            }

            List<TSD.View> views = TeklaSession.SelectedViews(handler);
            if (views.Count == 0)
            {
                DrawingObjectEnumerator all = drawing.GetSheet().GetViews();
                while (all.MoveNext())
                {
                    if (all.Current is TSD.View view)
                    {
                        views.Add(view);
                    }
                }
            }

            Console.WriteLine($"    {views.Count} view{(views.Count == 1 ? string.Empty : "s")} of drawing '{drawing.Name}'");
            string template = TeklaSession.FindAttributeFile(model, "standard" + ViewProperties.FileExtension);
            Console.WriteLine("    Template: " + (template ?? "none found, starting from ViewProperties.CreateDefault()"));
            foreach (TSD.View view in views)
            {
                Console.WriteLine($"    View '{view.Name}', profiles of its parts: {string.Join(", ", Profiles(model, view))}");
            }

            string profile = context.Ask("Show only the parts whose profile begins with (empty = no filter)", "HEA");
            if (!context.Confirm($"Change {views.Count} view{(views.Count == 1 ? string.Empty : "s")}?"))
            {
                Console.WriteLine("    Cancelled.");
                return;
            }

            string attributes = TeklaSession.AttributesDirectory(model);
            for (int i = 0; i < views.Count; i++)
            {
                TSD.View view = views[i];
                int before = TeklaSession.Count(view, typeof(TSD.Part));

                ViewProperties properties = template == null ? ViewProperties.CreateDefault() : AttributeFile.FromFile<ViewProperties>(template);
                properties.Attributes.Attributes1.View.Scale = view.Attributes.Scale;
                properties.Objects.Part.Content.HiddenLines.HiddenLines = true;
                properties.Objects.Part.Appearance.HiddenLines.Type = DrawingLineType.Dashed;
                properties.Filter.Rows.Clear();
                properties.NeighborPartFilter.Rows.Clear();
                if (!string.IsNullOrEmpty(profile))
                {
                    properties.Filter.SetExpression(FilterProperties.Part.Profile.BeginsWith(profile));
                }

                string name = TeklaSession.UniqueName("tda_apply_" + (i + 1));
                properties.SaveTo(attributes, name);

                view.Attributes.LoadAttributes(name);
                view.Modify();
                drawing.CommitChanges();

                int after = TeklaSession.Count(view, typeof(TSD.Part));
                Console.WriteLine($"    View '{view.Name}': {before} parts before, {after} after ({Path.Combine(attributes, name)}.vi)");
            }

            // The changes are in the open drawing only; closing it without saving undoes them
            if (context.Confirm("Undo the changes (close the drawing without saving)?"))
            {
                handler.CloseActiveDrawing(false);
                Console.WriteLine("    Closed without saving.");
            }
            else if (context.Confirm("Save and close the drawing?"))
            {
                handler.CloseActiveDrawing(true);
            }
        }

        /// <summary>The distinct profiles of the parts a view shows, read from the model.</summary>
        private static IEnumerable<string> Profiles(Model model, TSD.View view)
        {
            var profiles = new SortedSet<string>(StringComparer.Ordinal);
            DrawingObjectEnumerator parts = view.GetAllObjects(typeof(TSD.Part));
            while (parts.MoveNext())
            {
                if (parts.Current is TSD.Part part && model.SelectModelObject(part.ModelIdentifier) is Tekla.Structures.Model.Part modelPart)
                {
                    profiles.Add(modelPart.Profile.ProfileString);
                }
            }

            return profiles;
        }
    }
}
