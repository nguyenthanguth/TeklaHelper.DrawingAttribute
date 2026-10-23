using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using TeklaHelper.DrawingAttribute.Marks;
using TeklaHelper.DrawingAttribute.Views;
using TSD = Tekla.Structures.Drawing;

namespace TeklaHelper.DrawingAttribute.Samples.Online
{
    /// <summary>
    /// Mark content written with <see cref="MarkContent"/> in a view of the open drawing: the part marks of main and
    /// secondary parts, the marks of single bars, reinforcement groups, meshes, site and workshop bolts get different
    /// elements; a copy of a view (same coordinate systems and box) is inserted with that file, and the content of every
    /// mark of both views is read back through the Drawing API, grouped by the kind of object it marks.
    /// <para>
    /// The marks a view already has keep their own content when other view properties are loaded: the view properties
    /// give the content of the marks Tekla creates, hence the new view.
    /// </para>
    /// </summary>
    internal sealed class MarkContentInDrawingExample : Example
    {
        public override string Title => "Mark content (part / bolt / rebar mark tables) in a new view; read the marks back";

        public override string Description => "MainPartElements, GroupElements, MeshElements ... in a .vi; a copy of a view inserted with it; Mark.Attributes.Content of each mark.";

        public override bool NeedsTekla => true;

        public override void Run(ExampleContext context)
        {
            context.Step("Connect to Tekla Structures and pick a view of the open drawing");
            Model model = TeklaSession.Connect();
            if (model == null)
            {
                return;
            }

            var handler = new DrawingHandler();
            Drawing drawing = handler.GetActiveDrawing();
            TSD.View view = drawing == null ? null : FirstView(handler, drawing);
            if (view == null)
            {
                Console.WriteLine("    Open a drawing with a view first (and select a view to use that one).");
                return;
            }

            Console.WriteLine($"    View '{view.Name}' of drawing '{drawing.Name}'");
            Print("Marks now", view, model);

            context.Step("A view file whose mark tables name what each mark is for");
            string template = TeklaSession.FindAttributeFile(model, "standard" + ViewProperties.FileExtension);
            ViewProperties properties = template == null ? ViewProperties.CreateDefault() : AttributeFile.FromFile<ViewProperties>(template);
            properties.Attributes.Attributes1.View.Scale = view.Attributes.Scale;
            properties.Marks.PartMark.MainPartElements = new MarkContent().AddText("MAIN ").Add(MarkElementTypes.PartMark.AssemblyPosition);
            properties.Marks.PartMark.SecondaryPartElements = new MarkContent().AddText("SECONDARY ").Add(MarkElementTypes.PartMark.PartPosition);
            properties.Marks.ReinforcementMark.SingleElements = new MarkContent().AddText("SINGLE ").Add(MarkElementTypes.ReinforcementMark.Diameter);
            properties.Marks.ReinforcementMark.GroupElements = new MarkContent().AddText("GROUP ").Add(MarkElementTypes.ReinforcementMark.Number)
                .AddText("x").Add(MarkElementTypes.ReinforcementMark.Diameter);
            properties.Marks.ReinforcementMark.MeshElements = new MarkContent().AddText("MESH ").Add(MarkElementTypes.ReinforcementMeshMark.Size);
            properties.Marks.BoltMark.SiteBoltElements = new MarkContent().AddText("SITE ").Add(MarkElementTypes.BoltMark.Size);
            properties.Marks.BoltMark.WorkshopBoltElements = new MarkContent().AddText("SHOP ").Add(MarkElementTypes.BoltMark.Size);
            string name = TeklaSession.UniqueName("tda_marks");
            properties.SaveTo(TeklaSession.AttributesDirectory(model), name);

            if (!context.Confirm($"Insert a copy of view '{view.Name}' with '{name}'?"))
            {
                Console.WriteLine("    Cancelled.");
                return;
            }

            var copy = new TSD.View(drawing.GetSheet(), view.ViewCoordinateSystem, view.DisplayCoordinateSystem, view.RestrictionBox, name);
            if (!copy.Insert())
            {
                Console.WriteLine("    Insert failed.");
                return;
            }

            drawing.CommitChanges();
            Print("Marks of the new view", copy, model);

            if (context.Confirm("Undo (close the drawing without saving)?"))
            {
                handler.CloseActiveDrawing(false);
                Console.WriteLine("    Closed without saving.");
            }
        }

        /// <summary>The content of the marks of the view, by the kind of object they mark.</summary>
        private static void Print(string title, TSD.View view, Model model)
        {
            var contents = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);
            DrawingObjectEnumerator marks = view.GetAllObjects(typeof(Mark));
            while (marks.MoveNext())
            {
                if (!(marks.Current is Mark mark))
                {
                    continue;
                }

                string kind = "(nothing related)";
                DrawingObjectEnumerator related = mark.GetRelatedObjects();
                while (related.MoveNext())
                {
                    if (related.Current is TSD.ModelObject modelObject)
                    {
                        kind = Kind(modelObject, model);
                        break;
                    }
                }

                if (!contents.TryGetValue(kind, out SortedSet<string> set))
                {
                    contents[kind] = set = new SortedSet<string>(StringComparer.Ordinal);
                }

                if (set.Count < 3)
                {
                    set.Add(Describe(mark.Attributes.Content));
                }
            }

            Console.WriteLine($"    {title}:");
            foreach (KeyValuePair<string, SortedSet<string>> pair in contents)
            {
                Console.WriteLine($"      {pair.Key,-28} {string.Join("  |  ", pair.Value)}");
            }
        }

        private static string Kind(TSD.ModelObject modelObject, Model model)
        {
            if (modelObject is TSD.Part && model.SelectModelObject(modelObject.ModelIdentifier) is Tekla.Structures.Model.Part part)
            {
                Assembly assembly = part.GetAssembly();
                bool main = assembly != null && assembly.GetMainPart() is Tekla.Structures.Model.Part mainPart && mainPart.Identifier.ID == part.Identifier.ID;
                return main ? "main part" : "secondary part";
            }

            return modelObject.GetType().Name;
        }

        /// <summary>The elements of mark content as the Drawing API returns them.</summary>
        private static string Describe(ContainerElement container)
        {
            var parts = new List<string>();
            foreach (object element in (IEnumerable)container)
            {
                switch (element)
                {
                    case TextElement text:
                        parts.Add("\"" + text.Value + "\"");
                        break;
                    case PropertyElement property:
                        parts.Add(property.Name);
                        break;
                    case ContainerElement frame:
                        parts.Add("[" + Describe(frame) + "]");
                        break;
                    default:
                        parts.Add(element.GetType().Name.Replace("Element", string.Empty));
                        break;
                }
            }

            return parts.Count == 0 ? "(empty)" : string.Join(" ", parts.Where(p => p != "Space"));
        }

        private static TSD.View FirstView(DrawingHandler handler, Drawing drawing)
        {
            DrawingObjectEnumerator selected = handler.GetDrawingObjectSelector().GetSelected();
            while (selected.MoveNext())
            {
                if (selected.Current is TSD.View view)
                {
                    return view;
                }
            }

            DrawingObjectEnumerator views = drawing.GetSheet().GetViews();
            while (views.MoveNext())
            {
                if (views.Current is TSD.View view)
                {
                    return view;
                }
            }

            return null;
        }
    }
}
