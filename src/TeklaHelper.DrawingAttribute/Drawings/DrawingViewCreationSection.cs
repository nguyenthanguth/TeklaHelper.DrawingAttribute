using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

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

        /// <summary>
        /// Whether the front view is created: off, on, or automatically when needed. Read from the view table (see
        /// <see cref="GetViewTable"/>; the highest value when a type has several rows) when it has a row of that type, else from <c>cre_front_view</c>. Setting it writes <c>cre_front_view</c> and
        /// every row of that type, adding one to a table that has none.
        /// </summary>
        public ViewCreation CreateFrontView
        {
            get => GetCreation(DrawingViewType.Front, "cre_front_view");
            set => SetCreation(DrawingViewType.Front, "cre_front_view", value);
        }

        /// <summary>
        /// Whether the top view is created: off, on, or automatically when needed. Read from the view table (see
        /// <see cref="GetViewTable"/>; the highest value when a type has several rows) when it has a row of that type, else from <c>cre_top_view</c>. Setting it writes <c>cre_top_view</c> and
        /// every row of that type, adding one to a table that has none.
        /// </summary>
        public ViewCreation CreateTopView
        {
            get => GetCreation(DrawingViewType.Top, "cre_top_view");
            set => SetCreation(DrawingViewType.Top, "cre_top_view", value);
        }

        /// <summary>
        /// Whether the back view is created: off, on, or automatically when needed. Read from the view table (see
        /// <see cref="GetViewTable"/>; the highest value when a type has several rows) when it has a row of that type, else from <c>cre_back_view</c>. Setting it writes <c>cre_back_view</c> and
        /// every row of that type, adding one to a table that has none.
        /// </summary>
        public ViewCreation CreateBackView
        {
            get => GetCreation(DrawingViewType.Back, "cre_back_view");
            set => SetCreation(DrawingViewType.Back, "cre_back_view", value);
        }

        /// <summary>
        /// Whether the bottom view is created: off, on, or automatically when needed. Read from the view table (see
        /// <see cref="GetViewTable"/>; the highest value when a type has several rows) when it has a row of that type, else from <c>cre_bottom_view</c>. Setting it writes <c>cre_bottom_view</c> and
        /// every row of that type, adding one to a table that has none.
        /// </summary>
        public ViewCreation CreateBottomView
        {
            get => GetCreation(DrawingViewType.Bottom, "cre_bottom_view");
            set => SetCreation(DrawingViewType.Bottom, "cre_bottom_view", value);
        }

        /// <summary>
        /// Whether section views are created: off, on, or automatically when needed. Read from the view table (see
        /// <see cref="GetViewTable"/>; the highest value when a type has several rows) when it has a row of that type, else from <c>cre_cuts</c>. Setting it writes <c>cre_cuts</c> and
        /// every row of that type, adding one to a table that has none.
        /// </summary>
        public ViewCreation CreateSectionViews
        {
            get => GetCreation(DrawingViewType.Section, "cre_cuts");
            set => SetCreation(DrawingViewType.Section, "cre_cuts", value);
        }

        /// <summary>
        /// Whether end views are created: off, on, or automatically when needed. Read from the view table (see
        /// <see cref="GetViewTable"/>; the highest value when a type has several rows) when it has a row of that type, else from <c>cre_end_views</c>. Setting it writes <c>cre_end_views</c> and
        /// every row of that type, adding one to a table that has none.
        /// </summary>
        public ViewCreation CreateEndViews
        {
            get => GetCreation(DrawingViewType.End, "cre_end_views");
            set => SetCreation(DrawingViewType.End, "cre_end_views", value);
        }

        /// <summary>
        /// Whether a 3D view is created. Read from the 3D row of the view table when there is one, else from
        /// <c>Create3dView</c> (stored as 2 for on and 0 for off, like the dialog). Setting it writes the key and the 3D rows of
        /// the table, adding one to a table that has none.
        /// </summary>
        public bool Create3dView
        {
            get => GetCreation(DrawingViewType.ThreeD, "Create3dView") != ViewCreation.Off;
            set
            {
                if (Create3dView != value)
                {
                    SetCreation(DrawingViewType.ThreeD, "Create3dView", value ? ViewCreation.On : ViewCreation.Off);
                }
            }
        }

        /// <summary>
        /// The table of views the drawing creates (<c>aDrawingViews</c>, the View creation table of the assembly, single-part
        /// and cast unit drawing dialogs): for each view its type, whether it is created, its label and the view properties
        /// file (.vi) it uses. Tekla follows this table rather than the <c>cre_*</c> switches when it creates the views (checked
        /// in Tekla 2025). Views take their settings from the named file. With no file, the views use the object and mark
        /// settings of the drawing file, but the 3D view uses the standard view properties of the environment: name a file
        /// (<see cref="SetViewAttributeFile"/>) to control every view. Empty when the file has no table (GA drawings usually
        /// have none).
        /// </summary>
        /// <exception cref="FormatException">The table is not a list of rows <c>type,creation,label,file,</c>.</exception>
        public IList<DrawingViewTableRow> GetViewTable()
        {
            string text = GetString("aDrawingViews");
            var rows = new List<DrawingViewTableRow>();
            if (text.Length == 0)
            {
                return rows;
            }

            string[] fields = text.EndsWith(",", StringComparison.Ordinal) ? text.Substring(0, text.Length - 1).Split(',') : text.Split(',');
            if (fields.Length % 4 != 0)
            {
                throw new FormatException($"The view table '{text}' does not hold rows of four fields.");
            }

            for (int i = 0; i < fields.Length; i += 4)
            {
                if (!int.TryParse(fields[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int type)
                    || !int.TryParse(fields[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int creation))
                {
                    throw new FormatException($"Row {(i / 4) + 1} of the view table '{text}' has no numeric view type or creation.");
                }

                try
                {
                    rows.Add(new DrawingViewTableRow((DrawingViewType)type, (ViewCreation)creation, fields[i + 2], fields[i + 3]));
                }
                catch (ArgumentException exception)
                {
                    throw new FormatException($"Row {(i / 4) + 1} of the view table '{text}' has a label or file name the table cannot hold.", exception);
                }
            }

            return rows;
        }

        /// <summary>
        /// Writes the table of views the drawing creates (<c>aDrawingViews</c>), see <see cref="GetViewTable"/>. Each row ends
        /// with a comma, as the dialog writes it, unless the table it replaces ended without one (a few shipped files).
        /// </summary>
        public void SetViewTable(IEnumerable<DrawingViewTableRow> rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            string current = GetString("aDrawingViews");
            bool finalComma = current.Length == 0 || current.EndsWith(",", StringComparison.Ordinal);
            var text = new StringBuilder();
            foreach (DrawingViewTableRow row in rows)
            {
                text.Append(((int)row.ViewType).ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(((int)row.Creation).ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(row.Label).Append(',')
                    .Append(row.ViewAttributeFile).Append(',');
            }

            if (!finalComma && text.Length > 0)
            {
                text.Length--;
            }

            SetString("aDrawingViews", text.ToString());
        }

        /// <summary>
        /// Makes every view of the table, and the 3D view (<see cref="View3dAttributeFile"/>), use the view properties file
        /// <paramref name="viewAttributeFile"/> (a .vi name without extension, e.g. one saved from
        /// <c>ViewProperties.CreateDefault()</c>).
        /// </summary>
        /// <exception cref="InvalidOperationException">The file has no view table: start from <c>CreateDefault()</c> or write
        /// one with <see cref="SetViewTable"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="viewAttributeFile"/> is null (pass an empty text for no file).</exception>
        public void SetViewAttributeFile(string viewAttributeFile)
        {
            if (viewAttributeFile == null)
            {
                throw new ArgumentNullException(nameof(viewAttributeFile));
            }

            IList<DrawingViewTableRow> rows = GetViewTable();
            if (rows.Count == 0)
            {
                throw new InvalidOperationException("The file has no view table (aDrawingViews): start from CreateDefault() or write one with SetViewTable.");
            }

            SetViewTable(rows.Select(row => new DrawingViewTableRow(row.ViewType, row.Creation, row.Label, viewAttributeFile)).ToList());
            View3dAttributeFile = viewAttributeFile;
        }

        /// <summary>
        /// Writes the <c>cre_*</c> switches (and <c>Create3dView</c>) from the rows of the view table, so that the two places
        /// that tell whether a view is created agree.
        /// </summary>
        internal void SyncSwitchesWithViewTable()
        {
            foreach (IGrouping<DrawingViewType, DrawingViewTableRow> rows in GetViewTable().GroupBy(r => r.ViewType))
            {
                if (SwitchKeys.TryGetValue(rows.Key, out string key))
                {
                    ViewCreation creation = rows.Max(r => r.Creation);
                    SetEnum(key, rows.Key == DrawingViewType.ThreeD && creation != ViewCreation.Off ? ViewCreation.On : creation);
                }
            }
        }

        private static readonly Dictionary<DrawingViewType, string> SwitchKeys = new Dictionary<DrawingViewType, string>
        {
            [DrawingViewType.Front] = "cre_front_view",
            [DrawingViewType.Top] = "cre_top_view",
            [DrawingViewType.Back] = "cre_back_view",
            [DrawingViewType.Bottom] = "cre_bottom_view",
            [DrawingViewType.Section] = "cre_cuts",
            [DrawingViewType.End] = "cre_end_views",
            [DrawingViewType.ThreeD] = "Create3dView",
        };

        // Some shipped tables repeat a type (two section views, Auto and On, with cre_cuts 2): the highest creation counts
        private ViewCreation GetCreation(DrawingViewType type, string key)
        {
            List<DrawingViewTableRow> rows = TryGetViewTable()?.Where(r => r.ViewType == type).ToList();
            return rows != null && rows.Count > 0 ? rows.Max(r => r.Creation) : GetEnum(key, ViewCreation.Off);
        }

        private void SetCreation(DrawingViewType type, string key, ViewCreation value)
        {
            // Assigning a property its own value changes no byte, even where the switch and the table disagree
            if (GetCreation(type, key) == value)
            {
                return;
            }

            SetEnum(key, value);
            IList<DrawingViewTableRow> rows = TryGetViewTable();
            if (rows != null && rows.Count > 0)
            {
                // Every row of the type; a table without a row of the type gets one
                List<DrawingViewTableRow> updated = rows.Select(r => r.ViewType == type ? new DrawingViewTableRow(r.ViewType, value, r.Label, r.ViewAttributeFile) : r).ToList();
                if (!updated.Any(r => r.ViewType == type))
                {
                    updated.Add(new DrawingViewTableRow(type, value));
                }

                SetViewTable(updated);
            }
        }

        // A table that cannot be read leaves the switches to the cre_* keys
        private IList<DrawingViewTableRow> TryGetViewTable()
        {
            try
            {
                return GetViewTable();
            }
            catch (FormatException)
            {
                return null;
            }
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
