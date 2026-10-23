using System;
using System.Linq;

namespace TeklaHelper.DrawingAttribute.Drawings
{
    /// <summary>
    /// Whether a drawing creates a view: the Off / On / Auto menus of the drawing dialogs (values decoded from
    /// TeklaStructures.aid).
    /// </summary>
    public enum ViewCreation
    {
        /// <summary>Not created.</summary>
        Off = 0,

        /// <summary>Created when the drawing needs it.</summary>
        Auto = 1,

        /// <summary>Always created.</summary>
        On = 2,
    }

    /// <summary>The view types of the View creation table (values decoded from TeklaStructures.aid).</summary>
    public enum DrawingViewType
    {
        /// <summary>Front view.</summary>
        Front = 1,

        /// <summary>Top view.</summary>
        Top = 2,

        /// <summary>Back view.</summary>
        Back = 3,

        /// <summary>Bottom view.</summary>
        Bottom = 4,

        /// <summary>End views.</summary>
        End = 5,

        /// <summary>Section views.</summary>
        Section = 7,

        /// <summary>3D view.</summary>
        ThreeD = 16,
    }

    /// <summary>
    /// One row of the View creation table of a drawing (<see cref="DrawingViewCreationSection.GetViewTable"/>).
    /// </summary>
    public sealed class DrawingViewTableRow
    {
        /// <summary>
        /// Creates a row.
        /// </summary>
        /// <param name="viewType">Type of the view.</param>
        /// <param name="creation">Whether the view is created.</param>
        /// <param name="label">Label of the view; empty for the default label.</param>
        /// <param name="viewAttributeFile">Name of the view properties file (.vi) without extension; empty for none.</param>
        public DrawingViewTableRow(DrawingViewType viewType, ViewCreation creation, string label = "", string viewAttributeFile = "")
        {
            ViewType = viewType;
            Creation = creation;
            Label = Check(label, nameof(label));
            ViewAttributeFile = Check(viewAttributeFile, nameof(viewAttributeFile));
        }

        /// <summary>Type of the view.</summary>
        public DrawingViewType ViewType { get; }

        /// <summary>Whether the view is created.</summary>
        public ViewCreation Creation { get; }

        /// <summary>Label of the view; empty for the default label.</summary>
        public string Label { get; }

        /// <summary>Name of the view properties file (.vi) without extension; empty for none.</summary>
        public string ViewAttributeFile { get; }

        /// <inheritdoc />
        public override string ToString() => $"{ViewType} {Creation} '{Label}' {ViewAttributeFile}";

        // The table is one comma-separated text in quotes on one line: a comma would start another field
        private static string Check(string text, string parameterName)
        {
            text = text ?? string.Empty;
            if (text.IndexOf(',') >= 0 || text.IndexOf('"') >= 0 || text.Any(char.IsControl))
            {
                throw new ArgumentException("A label or file name of the view table cannot contain commas, quotes or control characters.", parameterName);
            }

            return text;
        }
    }
}
