namespace TeklaHelper.DrawingAttribute.Drawings
{
    /// <summary>
    /// Layout settings of a drawing (<c>dl.*</c>): layout name, sheet size and view scales.
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

        /// <summary>Size definition mode (<c>size_searching_mode</c>).</summary>
        public int SizeSearchingMode
        {
            get => GetInt32("size_searching_mode");
            set => SetInt32("size_searching_mode", value);
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

        /// <summary>Projection type (<c>projection_type</c>).</summary>
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
    }
}
