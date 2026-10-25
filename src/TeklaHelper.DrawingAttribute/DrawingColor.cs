namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// Colors of drawing objects, as stored in attribute files (e.g. <c>line_colour 160</c>).
    /// Same values as <c>Tekla.Structures.Drawing.DrawingColors</c>; the members of 154 to 159, which the Open API calls
    /// <c>NewLine1</c> ... <c>NewLine6</c>, are named after their colour. The RGB of each is what
    /// <c>Tekla.Structures.Drawing.TeklaDrawingColor</c> of Tekla Structures 2025 gives for it. Checked in Tekla Structures
    /// 2025: the number in the file is the colour Tekla uses, so numbers without a name here (small ones such as 1 to 7 are
    /// found in files saved from the dialogs) are valid too: cast them, e.g. <c>(DrawingColor)6</c>.
    /// </summary>
    public enum DrawingColor
    {
        /// <summary>No colour: the "None" background of a hatch (<c>HatchBackgroundColor 0</c>).</summary>
        None = 0,

        /// <summary>30 % gray (RGB 76, 76, 76; #4C4C4C).</summary>
        Gray30 = 130,

        /// <summary>50 % gray (RGB 127, 127, 127; #7F7F7F).</summary>
        Gray50 = 131,

        /// <summary>70 % gray (RGB 178, 178, 178; #B2B2B2).</summary>
        Gray70 = 132,

        /// <summary>90 % gray (RGB 229, 229, 229; #E5E5E5).</summary>
        Gray90 = 133,

        /// <summary>Not drawn (shown as RGB 231, 231, 231; #E7E7E7).</summary>
        Invisible = 152,

        /// <summary>Black (RGB 0, 0, 0; #000000).</summary>
        Black = 153,

        /// <summary>Brown (RGB 127, 63, 63; #7F3F3F). The Open API's <c>DrawingColors.NewLine1</c>.</summary>
        Brown = 154,

        /// <summary>Dark green (RGB 0, 127, 0; #007F00). The Open API's <c>DrawingColors.NewLine2</c>.</summary>
        DarkGreen = 155,

        /// <summary>Dark blue (RGB 50, 50, 152; #323298). The Open API's <c>DrawingColors.NewLine3</c>.</summary>
        DarkBlue = 156,

        /// <summary>
        /// Teal (RGB 0, 127, 127; #007F7F). The Open API's <c>DrawingColors.NewLine4</c>, which its documentation calls
        /// forest green.
        /// </summary>
        Teal = 157,

        /// <summary>Orange (RGB 254, 101, 0; #FE6500). The Open API's <c>DrawingColors.NewLine5</c>.</summary>
        Orange = 158,

        /// <summary>Gray (RGB 149, 149, 149; #959595). The Open API's <c>DrawingColors.NewLine6</c>.</summary>
        Gray = 159,

        /// <summary>Red (RGB 254, 0, 0; #FE0000).</summary>
        Red = 160,

        /// <summary>Green (RGB 0, 231, 0; #00E700).</summary>
        Green = 161,

        /// <summary>Blue (RGB 0, 0, 254; #0000FE).</summary>
        Blue = 162,

        /// <summary>Cyan (RGB 0, 254, 254; #00FEFE).</summary>
        Cyan = 163,

        /// <summary>Yellow (RGB 127, 127, 0; #7F7F00: an olive yellow, dark enough to read on white paper).</summary>
        Yellow = 164,

        /// <summary>Magenta (RGB 254, 0, 254; #FE00FE).</summary>
        Magenta = 165,
    }
}
