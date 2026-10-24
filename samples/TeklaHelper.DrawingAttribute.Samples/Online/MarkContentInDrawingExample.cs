using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using TeklaHelper.DrawingAttribute.Marks;
using TeklaHelper.DrawingAttribute.Views;
using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;

namespace TeklaHelper.DrawingAttribute.Samples.Online
{
    /// <summary>
    /// Mark content written with <see cref="MarkContent"/> in the views of the open drawing. Every table of the part, bolt
    /// and reinforcement marks names the kind of object it is for; two copies of a view (same coordinate systems and box)
    /// are inserted: the first with the template and then loaded with the file (<c>LoadAttributes</c> + <c>Modify</c>), the
    /// second inserted with the file. The content of every mark of both copies is read back through the Drawing API,
    /// grouped by the kind of object it marks.
    /// <para>
    /// In Tekla 2025 the marks Tekla had just created for the first copy took the new content when the file was loaded,
    /// like those of the second; marks placed or edited in a drawing may keep theirs (the reinforcement group marks of an
    /// existing view did, see CLAUDE.md).
    /// </para>
    /// </summary>
    internal sealed class MarkContentInDrawingExample : Example
    {
        public override string Title => "Mark content (part / bolt / rebar mark tables) in new views; read the marks back";

        public override string Description => "MainPartElements, SiteBoltElements, GroupElements, MeshElements ... in a .vi: loaded into a view vs. a view inserted with it.";

        public override bool NeedsTekla => true;

        public override void Run(ExampleContext context)
        {
            context.Step("Connect to Tekla Structures and pick a view of the open drawing");
            TSM.Model model = TeklaSession.Connect();
            if (model == null)
            {
                return;
            }

            var handler = new DrawingHandler();
            Drawing drawing = handler.GetActiveDrawing();
            TSD.View view = drawing == null ? null : TeklaSession.SelectedOrFirstView(handler, drawing);
            if (view == null)
            {
                Console.WriteLine("    Open a drawing with a view first (and select a view to use that one).");
                return;
            }

            var kinds = new Dictionary<int, string>();
            Console.WriteLine($"    View '{view.Name}' of drawing '{drawing.Name}'");
            Print("Marks of the view", view, model, kinds);

            context.Step("A view file whose mark tables name what each mark is for");
            string templatePath = TeklaSession.FindAttributeFile(model, "standard" + ViewProperties.FileExtension);
            ViewProperties template = templatePath == null ? ViewProperties.CreateDefault() : AttributeFile.FromFile<ViewProperties>(templatePath);
            template.Attributes.Attributes1.View.Scale = view.Attributes.Scale;

            ViewProperties properties = templatePath == null ? ViewProperties.CreateDefault() : AttributeFile.FromFile<ViewProperties>(templatePath);
            properties.Attributes.Attributes1.View.Scale = view.Attributes.Scale;
            properties.Marks.PartMark.MainPartElements = new MarkContent().AddText("MAIN ").Add(MarkElementTypes.PartMark.AssemblyPosition);
            properties.Marks.PartMark.SecondaryPartElements = new MarkContent().AddText("SECONDARY ").Add(MarkElementTypes.PartMark.PartPosition);
            properties.Marks.PartMark.SubAssemblyMainPartElements = new MarkContent().AddText("SUB MAIN ").Add(MarkElementTypes.PartMark.AssemblyPosition);
            properties.Marks.PartMark.SubAssemblySecondaryPartElements = new MarkContent().AddText("SUB SECONDARY ").Add(MarkElementTypes.PartMark.PartPosition);
            properties.Marks.ReinforcementMark.SingleElements = new MarkContent().AddText("SINGLE ").Add(MarkElementTypes.ReinforcementMark.Diameter);
            properties.Marks.ReinforcementMark.GroupElements = new MarkContent().AddText("GROUP ").Add(MarkElementTypes.ReinforcementMark.Number)
                .AddText("x").Add(MarkElementTypes.ReinforcementMark.Diameter);
            properties.Marks.ReinforcementMark.MeshElements = new MarkContent().AddText("MESH ").Add(MarkElementTypes.ReinforcementMeshMark.Size);
            properties.Marks.BoltMark.SiteBoltElements = new MarkContent().AddText("SITE ").Add(MarkElementTypes.BoltMark.Size);
            properties.Marks.BoltMark.WorkshopBoltElements = new MarkContent().AddText("SHOP ").Add(MarkElementTypes.BoltMark.Size);

            if (!context.Confirm($"Write two view files and insert two copies of view '{view.Name}'?"))
            {
                Console.WriteLine("    Cancelled: nothing was written.");
                return;
            }

            string attributes = TeklaSession.AttributesDirectory(model);
            string templateName = TeklaSession.UniqueName("tda_marks_template");
            string name = TeklaSession.UniqueName("tda_marks");
            template.SaveTo(attributes, templateName);
            properties.SaveTo(attributes, name);

            context.Step("A copy inserted with the template, then loaded with the file (LoadAttributes + Modify)");
            var loaded = new TSD.View(drawing.GetSheet(), view.ViewCoordinateSystem, view.DisplayCoordinateSystem, view.RestrictionBox, templateName);
            if (loaded.Insert())
            {
                loaded.Attributes.LoadAttributes(name);
                loaded.Modify();
                drawing.CommitChanges();
                Print("Marks after LoadAttributes + Modify", loaded, model, kinds);
            }

            context.Step("A copy inserted with the file: Tekla creates its marks from its tables");
            var inserted = new TSD.View(drawing.GetSheet(), view.ViewCoordinateSystem, view.DisplayCoordinateSystem, view.RestrictionBox, name);
            if (inserted.Insert())
            {
                drawing.CommitChanges();
                Print("Marks of the view inserted with the file", inserted, model, kinds);
            }

            // Only what the example added goes: the rest of the drawing, saved or not, stays as it was
            if (context.Confirm("Undo (delete the two views and the view files)?"))
            {
                Console.WriteLine("    Deleted: " + (loaded.Delete() & inserted.Delete()));
                drawing.CommitChanges();
                TeklaSession.DeleteFiles(attributes, templateName);
                TeklaSession.DeleteFiles(attributes, name);
            }
        }

        /// <summary>The content of the marks of a view (three at most per kind of object), by the kind of object they mark.</summary>
        private static void Print(string title, TSD.View view, TSM.Model model, Dictionary<int, string> kinds)
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
                        kind = Kind(modelObject, model, kinds);
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
                Console.WriteLine($"      {pair.Key,-32} {string.Join("  |  ", pair.Value)}");
            }
        }

        /// <summary>
        /// The kind of object, as the mark tables tell them apart: main / secondary part, of a sub-assembly or not, site /
        /// workshop bolt, single bar / group / mesh. Kept per object, since each costs calls to the model.
        /// </summary>
        private static string Kind(TSD.ModelObject modelObject, TSM.Model model, Dictionary<int, string> kinds)
        {
            int id = modelObject.ModelIdentifier.ID;
            if (kinds.TryGetValue(id, out string kind))
            {
                return kind;
            }

            kind = modelObject.GetType().Name;
            TSM.ModelObject selected = modelObject is TSD.Part || modelObject is TSD.Bolt ? model.SelectModelObject(modelObject.ModelIdentifier) : null;
            if (selected is TSM.Part part)
            {
                Assembly assembly = part.GetAssembly();
                bool main = assembly != null && assembly.GetMainPart() is TSM.Part mainPart && mainPart.Identifier.ID == part.Identifier.ID;
                kind = (assembly?.GetAssembly() != null ? "sub-assembly " : string.Empty) + (main ? "main part" : "secondary part");
            }
            else if (selected is BoltGroup bolt)
            {
                kind = bolt.BoltType == BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE ? "site bolt" : "workshop bolt";
            }

            kinds[id] = kind;
            return kind;
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
    }
}
