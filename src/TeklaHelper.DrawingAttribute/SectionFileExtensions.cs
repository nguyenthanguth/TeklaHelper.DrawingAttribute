namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// Extensions of the files a Tekla sub-dialog saves on its own, for <see cref="AttributeSection.ExportTo"/> and
    /// <see cref="AttributeSection.ImportFrom"/>. Each file holds exactly the keys of one section, without its prefix.
    /// <para>
    /// The view extensions were checked against files saved from the Tekla 2025 view dialogs (same keys, same values as
    /// the section in the .vi file). The drawing extensions were matched by key set against the files shipped with
    /// Tekla; for marks and parts, whose sections all share one key set, the kind is taken from the extension name.
    /// </para>
    /// <para>
    /// These files are for the Save / Load buttons of the single pages in the Tekla dialogs. The Open API reads only the
    /// attribute files with their companions: <c>name.vi</c> + <c>.vi.filter</c> + <c>.vi.npfilter</c> for a view,
    /// <c>name.gd</c> + <c>.gd.more</c> + <c>.gd.filter</c> + <c>.gd.npfilter</c> for a GA drawing.
    /// </para>
    /// </summary>
    public static class SectionFileExtensions
    {
        /// <summary>View grid (<c>vg.*</c>).</summary>
        public const string ViewGrid = ".vg";

        /// <summary>View parts (<c>vp.*</c>).</summary>
        public const string ViewParts = ".vp";

        /// <summary>View neighbour parts (<c>vnp.*</c>).</summary>
        public const string ViewNeighborParts = ".vnp";

        /// <summary>View bolts (<c>vs.*</c>).</summary>
        public const string ViewBolts = ".vs";

        /// <summary>View part marks (<c>vpm.*</c>).</summary>
        public const string ViewPartMarks = ".vpm";

        /// <summary>View neighbour part marks (<c>vnpm.*</c>).</summary>
        public const string ViewNeighborPartMarks = ".vnpm";

        /// <summary>View bolt marks (<c>vsm.*</c>).</summary>
        public const string ViewBoltMarks = ".vsm";

        /// <summary>View connection marks (<c>vjm.*</c>).</summary>
        public const string ViewConnectionMarks = ".vjm";

        /// <summary>View reinforcement marks (<c>vrm.*</c>).</summary>
        public const string ViewReinforcementMarks = ".vrm";

        /// <summary>View neighbour reinforcement marks (<c>NeighborRebarMark.*</c>).</summary>
        public const string ViewNeighborReinforcementMarks = ".vnrm";

        /// <summary>View surface treatment marks (<c>vsurfm.*</c>).</summary>
        public const string ViewSurfaceMarks = ".vsurfm";

        /// <summary>View reinforcement (<c>vrebar.*</c>).</summary>
        public const string ViewReinforcement = ".vr";

        /// <summary>View neighbour reinforcement (<c>NeighborRebar.*</c>).</summary>
        public const string ViewNeighborReinforcement = ".vnr";

        /// <summary>View weld marks (<c>vwel.*</c>).</summary>
        public const string ViewWeldMarks = ".vw";

        /// <summary>View welds (<c>ViewWeld.*</c>).</summary>
        public const string ViewWelds = ".vwo";

        /// <summary>View surface treatment (<c>vsurf.*</c>).</summary>
        public const string ViewSurfaces = ".vsurf";

        /// <summary>View reference objects (<c>vref.*</c>).</summary>
        public const string ViewReferenceObjects = ".vrmp";

        /// <summary>View pour breaks (<c>PourBreaks.*</c>).</summary>
        public const string ViewPourBreaks = ".vpbr";

        /// <summary>GA drawing grid (<c>dgrid.*</c>).</summary>
        public const string GADrawingGrid = ".gdgr";

        /// <summary>GA drawing layout (<c>dl.*</c>).</summary>
        public const string GADrawingLayout = ".gdl";

        /// <summary>GA drawing views (<c>dview.*</c>).</summary>
        public const string GADrawingViews = ".gdv";

        /// <summary>GA drawing parts (<c>dp.*</c>); files shipped with Tekla have the older, smaller key set.</summary>
        public const string GADrawingParts = ".gdp";

        /// <summary>GA drawing neighbour parts (<c>dnpart.*</c>).</summary>
        public const string GADrawingNeighborParts = ".gdnp";

        /// <summary>GA drawing bolts (<c>ds.*</c>).</summary>
        public const string GADrawingBolts = ".gds";

        /// <summary>GA drawing part marks (<c>dpm.*</c>).</summary>
        public const string GADrawingPartMarks = ".gpm";

        /// <summary>GA drawing neighbour part marks (<c>dnpm.*</c>).</summary>
        public const string GADrawingNeighborPartMarks = ".gnpm";

        /// <summary>GA drawing bolt marks (<c>dsm.*</c>).</summary>
        public const string GADrawingBoltMarks = ".gsm";

        /// <summary>GA drawing connection marks (<c>djm.*</c>).</summary>
        public const string GADrawingConnectionMarks = ".gjm";

        /// <summary>GA drawing weld marks (<c>dwel.*</c>).</summary>
        public const string GADrawingWeldMarks = ".gdw";

        /// <summary>GA drawing welds (<c>DrawingWeld.*</c>).</summary>
        public const string GADrawingWelds = ".gdwo";

        /// <summary>GA drawing dimensioning (<c>dd.*</c>).</summary>
        public const string GADrawingDimensioning = ".gdd";

        /// <summary>Assembly drawing grid (<c>grid.*</c>).</summary>
        public const string AssemblyDrawingGrid = ".adgr";

        /// <summary>Assembly drawing layout (<c>dl.*</c>).</summary>
        public const string AssemblyDrawingLayout = ".adl";

        /// <summary>Assembly drawing views (<c>dv.*</c>).</summary>
        public const string AssemblyDrawingViews = ".adv";

        /// <summary>Assembly drawing section views (<c>dc.*</c>).</summary>
        public const string AssemblyDrawingSectionViews = ".adc";

        /// <summary>Assembly drawing part marks (<c>dpm.*</c>).</summary>
        public const string AssemblyDrawingPartMarks = ".apm";

        /// <summary>Assembly drawing bolt marks (<c>dsm.*</c>).</summary>
        public const string AssemblyDrawingBoltMarks = ".asm";

        /// <summary>Single-part drawing layout (<c>dl.*</c>).</summary>
        public const string SinglePartDrawingLayout = ".wdl";

        /// <summary>Single-part drawing bolts (<c>ds.*</c>).</summary>
        public const string SinglePartDrawingBolts = ".wds";

        /// <summary>Single-part drawing part marks (<c>dpm.*</c>).</summary>
        public const string SinglePartDrawingPartMarks = ".wpm";

        /// <summary>Single-part drawing bolt marks (<c>dsm.*</c>).</summary>
        public const string SinglePartDrawingBoltMarks = ".wsm";

        /// <summary>Single-part drawing dimensioning (<c>dd.*</c>).</summary>
        public const string SinglePartDrawingDimensioning = ".wdd";

        /// <summary>Cast unit drawing views (<c>dv.*</c>).</summary>
        public const string CastUnitDrawingViews = ".cudv";

        /// <summary>Cast unit drawing section views (<c>dc.*</c>).</summary>
        public const string CastUnitDrawingSectionViews = ".cudc";

        /// <summary>Multi drawing layout (<c>dl.*</c>).</summary>
        public const string MultiDrawingLayout = ".mdl";
    }
}
