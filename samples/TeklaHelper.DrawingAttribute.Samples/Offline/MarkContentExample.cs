using System;
using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Marks;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Builds the "Elements in mark" of labels and marks from typed elements instead of writing the XML of the
    /// <c>ContentString</c> keys by hand: a view label, the tables of the part, bolt and reinforcement marks (one per kind of
    /// part, bolt or bar), an associative note, and a change to content that is already in a file.
    /// </summary>
    internal sealed class MarkContentExample : Example
    {
        public override string Title => "Mark and label content (ContentString) from typed elements";

        public override string Description => "View label; part / bolt / rebar mark tables (main, secondary, site, shop, single, group, mesh); note; .pm / .pom; edit existing content.";

        public override void Run(ExampleContext context)
        {
            ViewProperties view = ViewProperties.CreateDefault();

            context.Step("View label: the view name in green Tahoma 5, the scale, USER_FIELD_1 on the next line");
            view.Attributes.LabelMarkA1.Content.Elements = new MarkContent()
                .Add(MarkElementTypes.ViewLabel.ViewName, new MarkFormat { Font = "Tahoma", Color = DrawingColor.Green, Height = 5 })
                .AddText(" ")
                .Add(MarkElementTypes.ViewLabel.Scale)
                .AddLineFeed()
                .AddUserAttribute("USER_FIELD_1");

            context.Step("Part marks: one table per kind of part (the \"Mark content of\" menu of the dialog)");
            PartMark partMark = view.Marks.PartMark;
            partMark.Content.MainPartElements = new MarkContent()                                      // ContentString1
                .AddFrame(FrameAroundElementsType.Rectangular, DrawingColor.Red, new MarkContent().Add(MarkElementTypes.PartMark.AssemblyPosition))
                .AddSpace()
                .Add(MarkElementTypes.PartMark.Profile);
            partMark.Content.SecondaryPartElements = new MarkContent()                                 // ContentString2
                .Add(MarkElementTypes.PartMark.PartPosition)
                .AddText(" - ")
                .Add(MarkElementTypes.PartMark.Material);
            partMark.Content.SubAssemblyMainPartElements = new MarkContent().Add(MarkElementTypes.PartMark.AssemblyPosition);
            partMark.Content.SubAssemblySecondaryPartElements = new MarkContent().Add(MarkElementTypes.PartMark.PartPosition);

            context.Step("Bolt marks: site and workshop bolts");
            view.Marks.BoltMark.Content.SiteBoltElements = new MarkContent()
                .Add(MarkElementTypes.BoltMark.NumberOfBolts)
                .AddText("x")
                .Add(MarkElementTypes.BoltMark.Size)                                           // written as <SizeElement>
                .AddText(" SITE");
            view.Marks.BoltMark.Content.WorkshopBoltElements = new MarkContent()
                .Add(MarkElementTypes.BoltMark.NumberOfBolts)
                .AddText("x")
                .Add(MarkElementTypes.BoltMark.Size);

            context.Step("Reinforcement marks: single bars, groups and meshes (the mesh table is ContentString4)");
            ReinforcementMark rebarMark = view.Marks.ReinforcementMark;
            rebarMark.Content.SingleElements = new MarkContent()
                .Add(MarkElementTypes.ReinforcementMark.Position)
                .AddText(" Ø")
                .Add(MarkElementTypes.ReinforcementMark.Diameter)
                .AddText(" L=")
                .Add(MarkElementTypes.ReinforcementMark.Length);
            rebarMark.Content.GroupElements = new MarkContent()
                .Add(MarkElementTypes.ReinforcementMark.Number)
                .AddText(" Ø")
                .Add(MarkElementTypes.ReinforcementMark.Diameter)
                .AddText(" c/c ")
                .Add(MarkElementTypes.ReinforcementMark.Cc);
            rebarMark.Content.MeshElements = new MarkContent()
                .Add(MarkElementTypes.ReinforcementMeshMark.Position)
                .AddSpace()
                .Add(MarkElementTypes.ReinforcementMeshMark.Size);

            string path = view.SaveTo(context.OutputDirectory, "sample_mark_tables");
            Console.WriteLine("    " + path);

            context.Step("Read it back: every element of the tables of the saved file");
            var loaded = AttributeFile.FromFile<ViewProperties>(path);
            Console.WriteLine("    Label:            " + loaded.Attributes.LabelMarkA1.Content.Elements);
            Console.WriteLine("    Main parts:       " + loaded.Marks.PartMark.Content.MainPartElements);
            Console.WriteLine("    Secondary parts:  " + loaded.Marks.PartMark.Content.SecondaryPartElements);
            Console.WriteLine("    Site bolts:       " + loaded.Marks.BoltMark.Content.SiteBoltElements);
            Console.WriteLine("    Rebar groups:     " + loaded.Marks.ReinforcementMark.Content.GroupElements);
            Console.WriteLine("    Meshes:           " + loaded.Marks.ReinforcementMark.Content.MeshElements);

            context.Step("Change content that is already there: get (a copy), change, assign back");
            MarkContent group = loaded.Marks.ReinforcementMark.Content.GroupElements;
            group.AddLineFeed().Add(MarkElementTypes.ReinforcementMark.Shape);
            foreach (MarkElement element in group)
            {
                if (element is MarkPropertyElement property && property.Type.Equals(MarkElementTypes.ReinforcementMark.Diameter))
                {
                    property.Format.Color = DrawingColor.Blue;                                 // the format of one element
                }
            }

            loaded.Marks.ReinforcementMark.Content.GroupElements = group;                              // nothing changes until this line
            loaded.SaveTo(context.OutputDirectory, "sample_mark_tables_changed");
            Console.WriteLine("    Rebar groups now: " + loaded.Marks.ReinforcementMark.Content.GroupElements);

            context.Step("Associative note (.note): one table per kind of object the note points at");
            AssociativeNoteAttributeFile note = AssociativeNoteAttributeFile.CreateDefault();
            note.Properties.Content.PartElements = new MarkContent()
                .Add(MarkElementTypes.PartMark.Name)
                .AddLineFeed()
                .Add(MarkElementTypes.PartMark.Profile)
                .AddUserAttribute("comment", new MarkFormat { Height = 2 });
            note.Properties.Content.BoltElements = new MarkContent().Add(MarkElementTypes.BoltMark.Standard).AddSpace().Add(MarkElementTypes.BoltMark.Size);
            note.Properties.Content.ReinforcementElements = new MarkContent().Add(MarkElementTypes.ReinforcementMark.Grade);
            note.Properties.General.FrameAroundMark.Type = FrameAroundMarkType.Rectangular;
            context.ShowFile(note.SaveTo(context.OutputDirectory, "sample_note"), 8);

            context.Step("Mark files of the drawing editor: their dialogs have one table, Elements writes it like the dialog");
            PartMarkAttributeFile pm = PartMarkAttributeFile.CreateDefault();
            pm.Elements = new MarkContent().Add(MarkElementTypes.PartMark.PartPosition).AddSpace().Add(MarkElementTypes.PartMark.Profile);
            pm.SaveTo(context.OutputDirectory, "sample_part_marks");                           // ContentString1 to 4
            PourMarkAttributeFile pom = PourMarkAttributeFile.CreateDefault();
            pom.Elements = new MarkContent().Add(MarkElementTypes.PourMark.PourNumber);         // PourContentString and ContentString1
            pom.Properties.Advanced.PourBreakElements = new MarkContent().AddText("JOINT");
            pom.SaveTo(context.OutputDirectory, "sample_pour_marks");
            Console.WriteLine("    .pm: " + pm.Elements + "; .pom: " + pom.Elements);

            context.Step("A symbol and a template in a mark; a user-defined attribute with a unit and a number format");
            MarkContent special = new MarkContent()
                .AddSymbol("xsteel", 85, new MarkFormat { Color = DrawingColor.Red, Height = 4 })
                .AddTemplate("my_mark_template")
                .AddUserAttribute("LENGTH", new MarkFormat { Unit = Units.Meters, NumberFormat = UnitsFormat.TwoDecimals });
            Console.WriteLine("    " + special);
            Console.WriteLine(special.ToXml());

            Console.WriteLine("In Tekla: View.ViewAttributes(\"sample_mark_tables\"), or Load in the View properties dialog;");
            Console.WriteLine("          Mark.MarkAttributes(drawingObject, \"sample_note\") for the note.");
        }
    }
}
