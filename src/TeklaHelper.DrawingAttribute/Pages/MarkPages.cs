namespace TeklaHelper.DrawingAttribute.Pages
{
    /// <summary>
    /// The "Marks" pages of the view or drawing properties: one page per kind of mark, each with the tabs and group boxes
    /// of the Tekla dialog.
    /// <code>
    /// view.Marks.PartMark.Content.Font.Height = 3.5;
    /// view.Marks.PartMark.General.FrameAroundMark.Type = FrameAroundMarkType.Rectangular;
    /// </code>
    /// </summary>
    public sealed class MarkPages
    {
        /// <summary>
        /// Creates the mark pages of a file.
        /// </summary>
        /// <param name="values">Values of the attribute file.</param>
        /// <param name="host">Kind of the attribute file, which decides the prefixes.</param>
        public MarkPages(AttributeCollection values, PageHost host)
        {
            PartMark = new PartMark(values, PartMark.PrefixFor(host));
            BoltMark = new BoltMark(values, BoltMark.PrefixFor(host));
            NeighborPartMark = new NeighborPartMark(values, NeighborPartMark.PrefixFor(host));
            SurfaceTreatmentMark = new SurfaceTreatmentMark(values, SurfaceTreatmentMark.PrefixFor(host));
            ConnectionMark = new ConnectionMark(values, ConnectionMark.PrefixFor(host));
            WeldMark = new WeldMark(values, WeldMark.PrefixFor(host));
            ReinforcementMark = new ReinforcementMark(values, ReinforcementMark.PrefixFor(host));
            NeighborReinforcementMark = new NeighborReinforcementMark(values, NeighborReinforcementMark.PrefixFor(host));
            PourMark = new PourMark(values, PourMark.PrefixFor(host));
        }

        /// <summary>Sets every mark page to its defaults (see <c>SetDefaults()</c> of each page).</summary>
        public void SetDefaults()
        {
            PartMark.SetDefaults();
            BoltMark.SetDefaults();
            NeighborPartMark.SetDefaults();
            SurfaceTreatmentMark.SetDefaults();
            ConnectionMark.SetDefaults();
            WeldMark.SetDefaults();
            ReinforcementMark.SetDefaults();
            NeighborReinforcementMark.SetDefaults();
            PourMark.SetDefaults();
        }

        /// <summary>Part mark.</summary>
        public PartMark PartMark { get; }

        /// <summary>Bolt mark.</summary>
        public BoltMark BoltMark { get; }

        /// <summary>Neighbor part mark.</summary>
        public NeighborPartMark NeighborPartMark { get; }

        /// <summary>Surface treatment mark.</summary>
        public SurfaceTreatmentMark SurfaceTreatmentMark { get; }

        /// <summary>Connection mark.</summary>
        public ConnectionMark ConnectionMark { get; }

        /// <summary>Weld mark.</summary>
        public WeldMark WeldMark { get; }

        /// <summary>Reinforcement mark.</summary>
        public ReinforcementMark ReinforcementMark { get; }

        /// <summary>Neighbor reinforcement mark.</summary>
        public NeighborReinforcementMark NeighborReinforcementMark { get; }

        /// <summary>Pour mark (concrete pours; not shown in the dialogs of steel environments).</summary>
        public PourMark PourMark { get; }
    }
}
