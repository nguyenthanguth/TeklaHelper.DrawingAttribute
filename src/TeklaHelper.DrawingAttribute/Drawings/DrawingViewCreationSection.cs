namespace TeklaHelper.DrawingAttribute.Drawings
{
    /// <summary>
    /// View settings of a drawing: which views are created and how they look. The same keys are <c>dview.*</c> in GA and
    /// multi drawings and <c>dv.*</c> in assembly, single-part and cast unit drawings.
    /// </summary>
    public sealed class DrawingViewCreationSection : AttributeSection
    {
        /// <summary>Prefix in GA (.gd) and multi (.md) drawing files.</summary>
        public const string GAPrefix = "dview.";

        /// <summary>Prefix in assembly (.ad), single-part (.wd) and cast unit (.cud) drawing files.</summary>
        public const string AssemblyPrefix = "dv.";

        /// <summary>
        /// Creates the view section of a file.
        /// </summary>
        public DrawingViewCreationSection(AttributeCollection values, string prefix)
            : base(values, prefix)
        {
        }

        /// <summary>Scale of the views, e.g. 50 for 1:50 (<c>main_scale</c>).</summary>
        public double Scale
        {
            get => GetDouble("main_scale", 1);
            set => SetDouble("main_scale", value);
        }

        /// <summary>Create the front view (<c>cre_front_view</c>). Shipped files also hold 2 (meaning not known); it reads as true, and assigning true keeps it.</summary>
        public bool CreateFrontView
        {
            get => GetBoolean("cre_front_view");
            set => SetBoolean("cre_front_view", value);
        }

        /// <summary>Create the top view (<c>cre_top_view</c>). Shipped files also hold 2 (meaning not known); it reads as true, and assigning true keeps it.</summary>
        public bool CreateTopView
        {
            get => GetBoolean("cre_top_view");
            set => SetBoolean("cre_top_view", value);
        }

        /// <summary>Create the back view (<c>cre_back_view</c>). Shipped files also hold 2 (meaning not known); it reads as true, and assigning true keeps it.</summary>
        public bool CreateBackView
        {
            get => GetBoolean("cre_back_view");
            set => SetBoolean("cre_back_view", value);
        }

        /// <summary>Create the bottom view (<c>cre_bottom_view</c>). Shipped files also hold 2 (meaning not known); it reads as true, and assigning true keeps it.</summary>
        public bool CreateBottomView
        {
            get => GetBoolean("cre_bottom_view");
            set => SetBoolean("cre_bottom_view", value);
        }

        /// <summary>Create section views (<c>cre_cuts</c>). Shipped files also hold 2 (meaning not known); it reads as true, and assigning true keeps it.</summary>
        public bool CreateSectionViews
        {
            get => GetBoolean("cre_cuts");
            set => SetBoolean("cre_cuts", value);
        }

        /// <summary>Create end views (<c>cre_end_views</c>). Shipped files also hold 2 (meaning not known); it reads as true, and assigning true keeps it.</summary>
        public bool CreateEndViews
        {
            get => GetBoolean("cre_end_views");
            set => SetBoolean("cre_end_views", value);
        }

        /// <summary>Create a 3D view (<c>Create3dView</c>).</summary>
        public bool Create3dView
        {
            get => GetBoolean("Create3dView");
            set => SetBoolean("Create3dView", value);
        }

        /// <summary>Name of the view attribute file (.vi) used for the 3D view (<c>aCreate3dViewAttrib</c>).</summary>
        public string View3dAttributeFile
        {
            get => GetString("aCreate3dViewAttrib");
            set => SetString("aCreate3dViewAttrib", value);
        }

        /// <summary>Shorten the views (<c>ViewShorteningOn</c>).</summary>
        public bool ShorteningOn
        {
            get => GetBoolean("ViewShorteningOn");
            set => SetBoolean("ViewShorteningOn", value);
        }

        /// <summary>Length of the shortening cut in paper millimeters (<c>cut_length</c>).</summary>
        public double ShorteningCutLength
        {
            get => GetDouble("cut_length");
            set => SetDouble("cut_length", value);
        }

        /// <summary>Minimum length of a part portion that is cut away, in model millimeters (<c>cut_min_dist</c>).</summary>
        public double ShorteningMinimumDistance
        {
            get => GetDouble("cut_min_dist");
            set => SetDouble("cut_min_dist", value);
        }

        /// <summary>View label text height (<c>label_height</c>).</summary>
        public double LabelHeight
        {
            get => GetDouble("label_height");
            set => SetDouble("label_height", value);
        }

        /// <summary>View label position (<c>label_position</c>).</summary>
        public int LabelPosition
        {
            get => GetInt32("label_position");
            set => SetInt32("label_position", value);
        }

        /// <summary>View label color (<c>label_colour</c>).</summary>
        public DrawingColor LabelColor
        {
            get => GetEnum("label_colour", DrawingColor.Black);
            set => SetEnum("label_colour", value);
        }
    }
}
