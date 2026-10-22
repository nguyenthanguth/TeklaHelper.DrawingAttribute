namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// Line types, as stored in attribute files (e.g. <c>visible_linetype 1</c>). Checked in Tekla Structures 2025: the
    /// values load as the matching <c>Tekla.Structures.Drawing.LineTypes</c>; from 8 up Tekla rejects the whole file.
    /// </summary>
    public enum DrawingLineType
    {
        /// <summary>Undefined.</summary>
        Undefined = 0,

        /// <summary>Solid line.</summary>
        Solid = 1,

        /// <summary>Dashed line.</summary>
        Dashed = 2,

        /// <summary>Slashed line.</summary>
        Slashed = 3,

        /// <summary>Dash-dot line.</summary>
        DashDot = 4,

        /// <summary>Dotted line.</summary>
        Dotted = 5,

        /// <summary>Dash-double-dot line.</summary>
        DashDoubleDot = 6,

        /// <summary>Slash-dash line.</summary>
        SlashDash = 7,
    }
}
