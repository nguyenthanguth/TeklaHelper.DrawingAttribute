namespace TeklaHelper.DrawingAttribute.Pages
{
    public sealed partial class ViewLabelMark
    {
        /// <summary>
        /// Extension of the file the "..." dialog of a view label row saves in the view properties (matched by key set with
        /// the shipped files; only for the dialogs). The drawing properties put the letter of the drawing family in front
        /// (<c>.gview_mark</c>, <c>.aview_mark</c>, <c>.wview_mark</c>, <c>.cuview_mark</c>, after the dialog table of
        /// TeklaStructures_application.dll); the same holds for the other row files.
        /// </summary>
        public const string ViewLabelFileExtension = ".view_mark";

        /// <summary>Extension of the file of a section mark text row (matched by key set; only for the dialogs).</summary>
        public const string SectionMarkFileExtension = ".section_mark";

        /// <summary>Extension of the file of a section view label row (matched by key set; only for the dialogs).</summary>
        public const string SectionViewLabelFileExtension = ".section_view_mark";

        /// <summary>Extension of the file of a detail mark text row (after the dialog table; no shipped file to match).</summary>
        public const string DetailMarkFileExtension = ".detail_mark";

        /// <summary>Extension of the file of a detail view label row (matched by key set; only for the dialogs).</summary>
        public const string DetailViewLabelFileExtension = ".detail_view_mark";
    }
}
