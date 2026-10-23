using TeklaHelper.DrawingAttribute.Marks;

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

        /// <summary>
        /// The elements of the label row (<c>ContentString</c>): what the row shows, e.g. the view name and the scale
        /// (<see cref="MarkElementTypes.ViewLabel"/>, <see cref="MarkElementTypes.SectionMark"/>,
        /// <see cref="MarkElementTypes.DetailMark"/>), texts, symbols and frames; null when the row has none, or text that is
        /// no mark content (<see cref="AttributeSection.GetMarkContent"/> says why). Null empties the row. Checked in Tekla
        /// 2025: the Open API read the elements of a view label, a section mark and a detail mark the library wrote.
        /// <para>
        /// Each get reads the key afresh and returns a copy: changing it changes nothing in the file until it is assigned
        /// back.
        /// </para>
        /// <code>
        /// MarkContent elements = view.Attributes.LabelMarkA1.Elements;
        /// elements.AddText(" (preliminary)");
        /// view.Attributes.LabelMarkA1.Elements = elements;
        /// </code>
        /// </summary>
        public MarkContent Elements
        {
            get => GetMarkContentOrNull("ContentString");
            set => SetMarkContent("ContentString", value);
        }
    }
}
