namespace TeklaHelper.DrawingAttribute.Pages
{
    /// <summary>
    /// The "Objects" pages of the view or drawing properties: one page per kind of object, each with the tabs and group
    /// boxes of the Tekla dialog.
    /// <code>
    /// view.Objects.Part.Content.Representation.Representation = Representation.Exact;
    /// view.Objects.Part.Content.HiddenLines.HiddenLines = true;
    /// view.Objects.Part.Appearance.VisibleLines.Color = DrawingColor.Blue;
    /// view.Objects.Grid.Grid = Grid.Visible;
    /// </code>
    /// </summary>
    public sealed class ObjectPages
    {
        /// <summary>
        /// Creates the object pages of a file.
        /// </summary>
        /// <param name="values">Values of the attribute file.</param>
        /// <param name="host">Kind of the attribute file, which decides the prefixes.</param>
        public ObjectPages(AttributeCollection values, PageHost host)
        {
            Part = new PartObject(values, PartObject.PrefixFor(host));
            Bolt = new BoltObject(values, BoltObject.PrefixFor(host));
            NeighborPart = new NeighborPartObject(values, NeighborPartObject.PrefixFor(host));
            SurfaceTreatment = new SurfaceTreatmentObject(values, SurfaceTreatmentObject.PrefixFor(host));
            Weld = new WeldObject(values, WeldObject.PrefixFor(host));
            Reinforcement = new ReinforcementObject(values, ReinforcementObject.PrefixFor(host));
            Grid = new GridObject(values, GridObject.PrefixFor(host));
            PourBreak = new PourBreakObject(values, PourBreakObject.PrefixFor(host));
            NeighborReinforcement = new NeighborReinforcementObject(values, NeighborReinforcementObject.PrefixFor(host));
            PourObject = new PourObject(values, PourObject.PrefixFor(host));

            // Assembly, single-part and cast unit drawings have no reference object settings
            string referencePrefix = ReferenceObject.PrefixFor(host);
            ReferenceObject = referencePrefix == null ? null : new ReferenceObject(values, referencePrefix);
        }

        /// <summary>Part.</summary>
        public PartObject Part { get; }

        /// <summary>Bolt.</summary>
        public BoltObject Bolt { get; }

        /// <summary>Neighbor part.</summary>
        public NeighborPartObject NeighborPart { get; }

        /// <summary>Surface treatment.</summary>
        public SurfaceTreatmentObject SurfaceTreatment { get; }

        /// <summary>Weld.</summary>
        public WeldObject Weld { get; }

        /// <summary>Reinforcement.</summary>
        public ReinforcementObject Reinforcement { get; }

        /// <summary>Reference object; null in assembly, single-part and cast unit drawings, which have no such settings.</summary>
        public ReferenceObject ReferenceObject { get; }

        /// <summary>Grid.</summary>
        public GridObject Grid { get; }

        /// <summary>Pour break.</summary>
        public PourBreakObject PourBreak { get; }

        /// <summary>Neighbor reinforcement.</summary>
        public NeighborReinforcementObject NeighborReinforcement { get; }

        /// <summary>Pour object (concrete pours; not shown in the dialogs of steel environments).</summary>
        public PourObject PourObject { get; }
    }
}
