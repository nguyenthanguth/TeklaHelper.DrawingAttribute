namespace TeklaHelper.DrawingAttribute.Pages
{
    /// <summary>
    /// The kind of attribute file a page belongs to. The object and mark pages have the same keys in all of them, under
    /// different prefixes (<c>vp.</c> in a view, <c>dp.</c> in a drawing, <c>dgrid.</c> or <c>grid.</c> for the grid, ...).
    /// </summary>
    public enum PageHost
    {
        /// <summary>View properties (.vi).</summary>
        View,

        /// <summary>GA (.gd) and multi (.md) drawing properties.</summary>
        GADrawing,

        /// <summary>Assembly (.ad), single-part (.wd) and cast unit (.cud) drawing properties.</summary>
        AssemblyDrawing,
    }

    internal static class PageHostPrefix
    {
        /// <summary>
        /// Returns the prefix for <paramref name="host"/> among the three of a page; null when the host has no such page.
        /// </summary>
        public static string Select(PageHost host, string view, string gaDrawing, string assemblyDrawing)
        {
            switch (host)
            {
                case PageHost.View:
                    return view;
                case PageHost.GADrawing:
                    return gaDrawing;
                default:
                    return assemblyDrawing;
            }
        }
    }
}
