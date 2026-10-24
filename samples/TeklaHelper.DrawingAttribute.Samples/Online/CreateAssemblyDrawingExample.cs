using System;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using TSM = Tekla.Structures.Model;

namespace TeklaHelper.DrawingAttribute.Samples.Online
{
    /// <summary>
    /// Creates an assembly drawing of the assembly selected in the model from a .ad the library wrote: scale, the views
    /// to create and the .vi they use (View creation table), dimensions and marks; then lists the views Tekla made.
    /// </summary>
    internal sealed class CreateAssemblyDrawingExample : Example
    {
        public override string Title => "Create an assembly drawing of the selected assembly from a generated .ad";

        public override string Description => "AssemblyDrawingAttributeFile + the View creation table naming a generated .vi; new AssemblyDrawing(id, name).Insert().";

        public override bool NeedsTekla => true;

        public override void Run(ExampleContext context)
        {
            context.Step("Connect to Tekla Structures and find the selected assembly");
            TSM.Model model = TeklaSession.Connect();
            if (model == null)
            {
                return;
            }

            TSM.Assembly assembly = null;
            ModelObjectEnumerator selected = new TSM.UI.ModelObjectSelector().GetSelectedObjects();
            while (assembly == null && selected.MoveNext())
            {
                assembly = selected.Current as TSM.Assembly ?? (selected.Current as TSM.Part)?.GetAssembly();
            }

            if (assembly == null)
            {
                Console.WriteLine("    Select a part or an assembly in the model first.");
                return;
            }

            string position = string.Empty;
            assembly.GetReportProperty("ASSEMBLY_POS", ref position);
            Console.WriteLine($"    Assembly {position} (id {assembly.Identifier.ID})");

            context.Step("The view properties of every view of the drawing");
            ViewProperties views = ViewProperties.CreateDefault();
            views.Objects.Part.Content.HiddenLines.HiddenLines = true;
            views.Marks.PartMark.MainPartElements = new Marks.MarkContent().Add(Marks.MarkElementTypes.PartMark.AssemblyPosition);
            views.Marks.PartMark.SecondaryPartElements = new Marks.MarkContent().Add(Marks.MarkElementTypes.PartMark.PartPosition);
            string viewName = TeklaSession.UniqueName("tda_ad_views");

            context.Step("The assembly drawing properties");
            AssemblyDrawingAttributeFile drawing = AssemblyDrawingAttributeFile.CreateDefault();
            drawing.Title1 = "ASSEMBLY " + position;
            drawing.Views.Scale = 15;
            drawing.Views.CreateFrontView = ViewCreation.On;
            drawing.Views.CreateTopView = ViewCreation.On;
            drawing.Views.CreateEndViews = ViewCreation.Auto;
            drawing.Views.SetViewAttributeFile(viewName);                      // every row of the View creation table
            drawing.Dimension.General.DimensionTypes.Straight = DimensionType.Absolute;
            drawing.Dimension.Appearance.Text.Height = 3;
            string drawingName = TeklaSession.UniqueName("tda_ad");

            if (!context.Confirm($"Write a .vi and a .ad and create an assembly drawing of {position}?"))
            {
                Console.WriteLine("    Cancelled: nothing was written.");
                return;
            }

            string attributes = TeklaSession.AttributesDirectory(model);
            views.SaveTo(attributes, viewName);
            drawing.SaveTo(attributes, drawingName);

            var assemblyDrawing = new AssemblyDrawing(assembly.Identifier, drawingName);
            if (!assemblyDrawing.Insert())
            {
                Console.WriteLine("    Insert failed.");
                return;
            }

            Console.WriteLine($"    Drawing {assemblyDrawing.Mark} created: '{assemblyDrawing.Title1}'");
            ListViews(assemblyDrawing);

            if (context.Confirm("Delete the drawing and the two files again?"))
            {
                Console.WriteLine("    Deleted: " + assemblyDrawing.Delete());
                TeklaSession.DeleteFiles(attributes, viewName);
                TeklaSession.DeleteFiles(attributes, drawingName);
            }
        }

        /// <summary>
        /// Opens the new drawing in the background, lists its views and closes it again; leaves a drawing the user has open
        /// alone (opening another would close it).
        /// </summary>
        private static void ListViews(AssemblyDrawing assemblyDrawing)
        {
            var handler = new DrawingHandler();
            if (handler.GetActiveDrawing() != null)
            {
                Console.WriteLine("    A drawing is open in the drawing editor: the views of the new drawing are not listed, so that it stays open.");
                return;
            }

            bool opened = false;
            try
            {
                opened = handler.SetActiveDrawing(assemblyDrawing, false);
                if (opened)
                {
                    DrawingObjectEnumerator sheetViews = handler.GetActiveDrawing().GetSheet().GetViews();
                    while (sheetViews.MoveNext())
                    {
                        if (sheetViews.Current is Tekla.Structures.Drawing.View view)
                        {
                            Console.WriteLine($"    View '{view.Name}' ({view.ViewType}), scale 1:{view.Attributes.Scale}");
                        }
                    }
                }
            }
            catch (CannotPerformOperationDrawingNotUpToDateException)
            {
                // A model whose numbering is not up to date (positions such as C/0(?)) gives drawings that must be updated
                // in the Document manager before they open
                Console.WriteLine("    The drawing is not up to date (number the model first), so its views cannot be listed.");
            }
            finally
            {
                if (opened && handler.GetActiveDrawing() != null)
                {
                    handler.CloseActiveDrawing(false);
                }
            }
        }
    }
}
