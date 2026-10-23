namespace TeklaHelper.DrawingAttribute.Filters
{
    /// <summary>
    /// Extensions of the filter files Tekla uses: files in the block format of <see cref="FilterFile"/>, not the attribute
    /// files they belong to (a GA drawing's settings are <c>name.gd</c>, its filter <c>name.gdf</c> or <c>name.gd.filter</c>).
    /// <para>
    /// Sources: <c>.vf</c>, <c>.vnf</c>, <c>.vi.filter</c> and <c>.vi.npfilter</c> were saved from the Tekla 2025 view
    /// dialogs; <c>Tekla.Structures.Filtering.Filter.CreateFile</c> gives <c>.gdf</c>, <c>.adf</c>, <c>.wdf</c>,
    /// <c>.cuf</c>, <c>.SObjGrp</c> and <c>.VObjGrp</c> for its <c>FilterExpressionFileType</c> values; the other
    /// extensions are files shipped in the Tekla 2025 environments, where one filter is saved from every dialog as
    /// <c>.vf</c>/<c>.vnf</c>, <c>.gdf</c>/<c>.gdnf</c>, <c>.adf</c>/<c>.adnf</c>, <c>.wdf</c>/<c>.wdnf</c> and
    /// <c>.cuf</c>/<c>.cunf</c> (the <c>n</c> files are neighbour part filters, by analogy with the view).
    /// </para>
    /// <para>
    /// These files are for the Save / Load buttons of the single pages in the Tekla dialogs. The Open API reads only the
    /// attribute files with their companions: <c>name.vi</c> + <c>.vi.filter</c> + <c>.vi.npfilter</c> for a view,
    /// <c>name.gd</c> + <c>.gd.more</c> + <c>.gd.filter</c> + <c>.gd.npfilter</c> for a GA drawing.
    /// </para>
    /// </summary>
    public static class FilterFileExtensions
    {
        /// <summary>View filter, saved from the filter tab of the view properties (<c>.vf</c>).</summary>
        public const string ViewFilter = ".vf";

        /// <summary>View neighbour part filter, saved from the view properties (<c>.vnf</c>).</summary>
        public const string ViewNeighborPartFilter = ".vnf";

        /// <summary>GA drawing filter (<c>.gdf</c>; <c>FilterExpressionFileType.DRAWING_GENERAL</c>).</summary>
        public const string GADrawingFilter = ".gdf";

        /// <summary>GA drawing neighbour part filter (<c>.gdnf</c>).</summary>
        public const string GADrawingNeighborPartFilter = ".gdnf";

        /// <summary>Assembly drawing filter (<c>.adf</c>; <c>FilterExpressionFileType.DRAWING_ASSEMBLY</c>).</summary>
        public const string AssemblyDrawingFilter = ".adf";

        /// <summary>Assembly drawing neighbour part filter (<c>.adnf</c>).</summary>
        public const string AssemblyDrawingNeighborPartFilter = ".adnf";

        /// <summary>Single-part drawing filter (<c>.wdf</c>; <c>FilterExpressionFileType.DRAWING_SINGLE_PART</c>).</summary>
        public const string SinglePartDrawingFilter = ".wdf";

        /// <summary>Single-part drawing neighbour part filter (<c>.wdnf</c>).</summary>
        public const string SinglePartDrawingNeighborPartFilter = ".wdnf";

        /// <summary>Cast unit drawing filter (<c>.cuf</c>; <c>FilterExpressionFileType.DRAWING_CAST_UNIT</c>).</summary>
        public const string CastUnitDrawingFilter = ".cuf";

        /// <summary>Cast unit drawing neighbour part filter (<c>.cunf</c>).</summary>
        public const string CastUnitDrawingNeighborPartFilter = ".cunf";

        /// <summary>
        /// Model selection filter (<c>.SObjGrp</c>; <c>FilterExpressionFileType.OBJECT_GROUP_SELECTION</c>). The filter
        /// <c>ModelObjectSelector.GetObjectsByFilterName</c> and <c>Operation.ObjectMatchesToFilter</c> look up by name;
        /// in Tekla 2025 they read the file again on each call (not checked with the filter cache of the Open API on).
        /// </summary>
        public const string ModelSelectionFilter = ".SObjGrp";

        /// <summary>
        /// Model view filter (<c>.VObjGrp</c>; <c>FilterExpressionFileType.OBJECT_GROUP_VIEW</c>), for
        /// <c>Model.UI.View.ViewFilter</c>. The selection methods of the Open API do not find it: save a
        /// <see cref="ModelSelectionFilter"/> for them.
        /// </summary>
        public const string ModelViewFilter = ".VObjGrp";

        /// <summary>View filter saved with a view attribute file (<c>name.vi</c> + <c>name.vi.filter</c>), for <c>vfilter.*</c>.</summary>
        public const string ViewAttributeFilter = ".vi.filter";

        /// <summary>Neighbour part filter saved with a view attribute file, for <c>vnp_filter.*</c>.</summary>
        public const string ViewAttributeNeighborPartFilter = ".vi.npfilter";

        /// <summary>Filter saved with a GA drawing attribute file (<c>name.gd</c> + <c>name.gd.filter</c>), for <c>dfilter.*</c>.</summary>
        public const string GADrawingAttributeFilter = ".gd.filter";

        /// <summary>Neighbour part filter saved with a GA drawing attribute file, for <c>dnp_filter.*</c>.</summary>
        public const string GADrawingAttributeNeighborPartFilter = ".gd.npfilter";

        /// <summary>Filter saved with an assembly drawing attribute file (<c>name.ad</c> + <c>name.ad.filter</c>).</summary>
        public const string AssemblyDrawingAttributeFilter = ".ad.filter";

        /// <summary>Filter saved with a single-part drawing attribute file (<c>name.wd</c> + <c>name.wd.filter</c>).</summary>
        public const string SinglePartDrawingAttributeFilter = ".wd.filter";
    }
}
