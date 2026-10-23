namespace TeklaHelper.DrawingAttribute.Drawings
{
    /// <summary>
    /// Layout settings of a drawing (<c>dl.*</c>): layout name, sheet size and view scales. GA and assembly drawings
    /// share the keys (dialog struct <c>gr_dl_at_t</c>); the GA layout dialog shows only the layout and the sheet size,
    /// the assembly, single-part and cast unit layout dialog also the Scale and Other tabs, but GA files hold those keys
    /// too.
    /// </summary>
    public sealed class DrawingLayoutSection : AttributeSection
    {
        /// <summary>Prefix of the layout settings.</summary>
        public const string DefaultPrefix = "dl.";

        /// <summary>
        /// Creates the layout section of a file.
        /// </summary>
        public DrawingLayoutSection(AttributeCollection values, string prefix = DefaultPrefix)
            : base(values, prefix)
        {
        }

        /// <summary>Name of the drawing layout, e.g. <c>ga</c> (<c>layout</c>).</summary>
        public string LayoutName
        {
            get => GetString("layout");
            set => SetString("layout", value);
        }

        /// <summary>
        /// List hidden objects in templates (<c>ListHiddenObjectsInReports</c>, Yes 1 / No 0).
        /// </summary>
        public bool ListHiddenObjectsInTemplates
        {
            get => GetBoolean("ListHiddenObjectsInReports");
            set => SetBoolean("ListHiddenObjectsInReports", value);
        }

        /// <summary>Size definition mode: 0 Autosize, 1 Specified size (<c>size_searching_mode</c>).</summary>
        public int SizeSearchingMode
        {
            get => GetInt32("size_searching_mode");
            set => SetInt32("size_searching_mode", value);
        }

        /// <summary>
        /// The entry chosen in the Drawing size list of the dialog (<c>size</c>): a number that identifies a size of the
        /// layout in the environment's layout files (0 in most shipped files), not a paper format. Copy it from a file
        /// saved from the dialog in the same environment.
        /// </summary>
        public int DrawingSize
        {
            get => GetInt32("size");
            set => SetInt32("size", value);
        }

        /// <summary>
        /// Table layout of the sheet size (<c>sheet</c>): a number that identifies a table layout in the environment's
        /// layout files (e.g. 446, 518263), shown as a name in the Fixed sizes dialog. Copy it from a file saved from the
        /// dialog in the same environment.
        /// </summary>
        public int TableLayout
        {
            get => GetInt32("sheet");
            set => SetInt32("sheet", value);
        }

        /// <summary>Use the fixed sheet size <see cref="Width"/> x <see cref="Height"/> (<c>take_fixed</c>).</summary>
        public bool UseFixedSize
        {
            get => GetBoolean("take_fixed");
            set => SetBoolean("take_fixed", value);
        }

        /// <summary>Calculate the sheet size from the content (<c>take_calc</c>).</summary>
        public bool UseCalculatedSize
        {
            get => GetBoolean("take_calc");
            set => SetBoolean("take_calc", value);
        }

        /// <summary>Sheet width in millimeters (<c>width</c>).</summary>
        public double Width
        {
            get => GetDouble("width");
            set => SetDouble("width", value);
        }

        /// <summary>Sheet height in millimeters (<c>height</c>).</summary>
        public double Height
        {
            get => GetDouble("height");
            set => SetDouble("height", value);
        }

        /// <summary>Projection type: 0 First angle, 1 Third angle (<c>projection_type</c>).</summary>
        public int ProjectionType
        {
            get => GetInt32("projection_type");
            set => SetInt32("projection_type", value);
        }

        /// <summary>Scale the views automatically (<c>autoscale</c>).</summary>
        public bool AutoScale
        {
            get => GetBoolean("autoscale");
            set => SetBoolean("autoscale", value);
        }

        /// <summary>Scales tried for the views when scaling automatically, space separated (<c>view_scales</c>).</summary>
        public string ViewScales
        {
            get => GetString("view_scales");
            set => SetString("view_scales", value);
        }

        /// <summary>Scales tried for the section views when scaling automatically (<c>cut_scales</c>).</summary>
        public string CutScales
        {
            get => GetString("cut_scales");
            set => SetString("cut_scales", value);
        }

        /// <summary>Scale change mode: how the section view scale follows the main view scale (<c>change_scale_mode</c>).</summary>
        public ScaleChangeMode ScaleChangeMode
        {
            get => GetEnum("change_scale_mode", ScaleChangeMode.MainLessOrEqualSection);
            set => SetEnum("change_scale_mode", value);
        }

        /// <summary>Width of the preferred size when scaling automatically, in millimeters (<c>als_width</c>).</summary>
        public double PreferredWidth
        {
            get => GetDouble("als_width");
            set => SetDouble("als_width", value);
        }

        /// <summary>Height of the preferred size when scaling automatically, in millimeters (<c>als_height</c>).</summary>
        public double PreferredHeight
        {
            get => GetDouble("als_height");
            set => SetDouble("als_height", value);
        }

        /// <summary>Align section views with main view (<c>CutViewsInLineWithMain</c>, Yes 1 / No 0).</summary>
        public bool AlignSectionViewsWithMainView
        {
            get => GetBoolean("CutViewsInLineWithMain");
            set => SetBoolean("CutViewsInLineWithMain", value);
        }

        /// <summary>Align end views with main view (<c>side_view_beside</c>, Yes 1 / No 0).</summary>
        public bool AlignEndViewsWithMainView
        {
            get => GetBoolean("side_view_beside");
            set => SetBoolean("side_view_beside", value);
        }

        /// <summary>Expand shortened parts to fit (<c>adjust_min_dist</c>, Yes 1 / No 0).</summary>
        public bool ExpandShortenedPartsToFit
        {
            get => GetBoolean("adjust_min_dist");
            set => SetBoolean("adjust_min_dist", value);
        }

        /// <summary>Include single-parts in the drawing (<c>include_single_parts</c>, Yes 1 / No 0).</summary>
        public bool IncludeSingleParts
        {
            get => GetBoolean("include_single_parts");
            set => SetBoolean("include_single_parts", value);
        }

        /// <summary>
        /// Single-part attributes: the single-part drawing properties (.wd) used for the included single-parts, without
        /// extension (<c>single_attrib_file</c>).
        /// </summary>
        public string SinglePartAttributeFile
        {
            get => GetString("single_attrib_file");
            set => SetString("single_attrib_file", value);
        }
    }

    /// <summary>
    /// The Scale change mode of the drawing layout (<c>dl.change_scale_mode</c>): how the scale of the section views
    /// relates to the scale of the main view when scaling automatically.
    /// </summary>
    public enum ScaleChangeMode
    {
        /// <summary>main = section (0).</summary>
        MainEqualsSection = 0,

        /// <summary>main &lt; section (1).</summary>
        MainLessThanSection = 1,

        /// <summary>main &lt;= section (2), the dialog default.</summary>
        MainLessOrEqualSection = 2,
    }
}
