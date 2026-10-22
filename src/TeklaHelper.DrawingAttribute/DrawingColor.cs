namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// Colors of drawing objects, as stored in attribute files (e.g. <c>line_colour 160</c>).
    /// Same values as <c>Tekla.Structures.Drawing.DrawingColors</c>. Checked in Tekla Structures 2025: the number in the
    /// file is the colour Tekla uses, so numbers without a name here (small ones such as 1 to 7 are found in files saved
    /// from the dialogs) are valid too: cast them, e.g. <c>(DrawingColor)6</c>.
    /// </summary>
    public enum DrawingColor
    {
        /// <summary>No colour: the "None" background of a hatch (<c>HatchBackgroundColor 0</c>).</summary>
        None = 0,

        /// <summary>30 % gray.</summary>
        Gray30 = 130,

        /// <summary>50 % gray.</summary>
        Gray50 = 131,

        /// <summary>70 % gray.</summary>
        Gray70 = 132,

        /// <summary>90 % gray.</summary>
        Gray90 = 133,

        /// <summary>Not drawn.</summary>
        Invisible = 152,

        /// <summary>Black.</summary>
        Black = 153,

        /// <summary>Pen color 1 of the plotter settings.</summary>
        NewLine1 = 154,

        /// <summary>Pen color 2 of the plotter settings.</summary>
        NewLine2 = 155,

        /// <summary>Pen color 3 of the plotter settings.</summary>
        NewLine3 = 156,

        /// <summary>Pen color 4 of the plotter settings.</summary>
        NewLine4 = 157,

        /// <summary>Pen color 5 of the plotter settings.</summary>
        NewLine5 = 158,

        /// <summary>Pen color 6 of the plotter settings.</summary>
        NewLine6 = 159,

        /// <summary>Red.</summary>
        Red = 160,

        /// <summary>Green.</summary>
        Green = 161,

        /// <summary>Blue.</summary>
        Blue = 162,

        /// <summary>Cyan.</summary>
        Cyan = 163,

        /// <summary>Yellow.</summary>
        Yellow = 164,

        /// <summary>Magenta.</summary>
        Magenta = 165,
    }
}
