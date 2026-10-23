using System;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Assembly, single-part and cast unit drawing properties, and copying a page between a view and a drawing.
    /// </summary>
    internal sealed class AssemblyDrawingExample : Example
    {
        public override string Title => "Assembly, single-part and cast unit drawings; copy pages between hosts";

        public override string Description => ".ad / .wd / .cud with the assembly prefixes (dv., grid., dnp., ...); CopyFrom between a view and a drawing.";

        public override void Run(ExampleContext context)
        {
            context.Step("A view whose part marks we want in every drawing type");
            var view = new ViewProperties();
            PartMark viewMarks = view.Marks.PartMark;
            viewMarks.Content.Font.Font = "Arial Narrow";
            viewMarks.Content.Font.Height = 2.5;
            viewMarks.Content.Font.Color = DrawingColor.Green;
            viewMarks.General.FrameAroundMark.Type = FrameAroundMarkType.Round;

            context.Step("Assembly drawing (.ad, .ad.more, .ad.filter)");
            AssemblyDrawingAttributeFile assembly = AssemblyDrawingAttributeFile.CreateDefault();
            assembly.Title1 = "ASSEMBLY";
            assembly.SheetNumber = 1;
            assembly.Views.Scale = 15;
            assembly.Views.CreateFrontView = ViewCreation.On;
            assembly.Views.CreateTopView = ViewCreation.On;
            assembly.Objects.Grid.Grid = Grid.NotVisible;              // grid.grid_on in the assembly family
            assembly.Objects.NeighborPart.Content.Representation.Representation = NeighborPartRepresentation.Outline;
            assembly.Marks.PartMark.CopyFrom(viewMarks);                 // vpm.* of the view -> dpm.* of the drawing
            assembly.Filter.SetExpression(FilterProperties.Part.Name.IsNotEqual("ERECTION BOLT"));
            assembly.UserAttributes.DrawnBy = "NT";

            // The pages of the drawing properties: dimensions (dd.), automatic dimensioning (dcd.), section views (dc.)
            assembly.Dimension.General.DimensionTypes.Straight = DimensionType.Absolute;
            assembly.Dimension.Appearance.Text.Height = 3;
            assembly.AssemblyDimensioning.PartDimensions.MainPartOverallDimensions = MainPartOverallDimensions.All;
            assembly.SectionView.CuttingLine.Properties.Color = DrawingColor.Blue;

            // The views Tekla creates take their settings from the view properties named in the View creation table
            ViewProperties.CreateDefault().SaveTo(context.OutputDirectory, "sample_assembly_views");
            assembly.Views.SetViewAttributeFile("sample_assembly_views");
            foreach (DrawingViewTableRow row in assembly.Views.GetViewTable())
            {
                Console.WriteLine("    " + row);
            }

            string assemblyPath = assembly.SaveTo(context.OutputDirectory, "sample_assembly");
            context.ShowFile(assemblyPath, 12);

            context.Step("Single-part drawing (.wd, .wd.more, .wd.filter)");
            var singlePart = new SinglePartDrawingAttributeFile { Title1 = "PLATE" };
            singlePart.Views.Scale = 10;
            singlePart.Marks.PartMark.CopyFrom(viewMarks);
            singlePart.Objects.Part.Content.AdditionalMarks.EdgeChamfers = true;
            context.ShowFile(singlePart.SaveTo(context.OutputDirectory, "sample_single_part"), 12);

            context.Step("Cast unit drawing (.cud, .cud.more)");
            var castUnit = new CastUnitDrawingAttributeFile { Title1 = "PRECAST" };
            castUnit.Views.Scale = 25;
            castUnit.Objects.Reinforcement.BarContent.ReinforcingBarRepresentation.Representation = ReinforcingBarRepresentation.DoubleLine;
            castUnit.Marks.ReinforcementMark.Content.Font.Height = 2.5;
            castUnit.CastUnitDefinitionMethod = CastUnitDefinitionMethod.ByCastUnitId;
            context.ShowFile(castUnit.SaveTo(context.OutputDirectory, "sample_cast_unit"), 12);

            Console.WriteLine();
            Console.WriteLine("In Tekla: new AssemblyDrawing(assembly.Identifier, \"sample_assembly\"),");
            Console.WriteLine("          new SinglePartDrawing(part.Identifier, \"sample_single_part\"),");
            Console.WriteLine("          new CastUnitDrawing(castUnit.Identifier, CastUnitDrawing.CastUnitDrawingCreationType.CastUnitDrawingById, \"sample_cast_unit\").");
        }
    }
}
