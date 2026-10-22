using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Views
{
    public sealed partial class ViewAttributes
    {
        private ViewLabelMark _labelMarkA1;
        private ViewLabelMark _labelMarkA2;
        private ViewLabelMark _labelMarkA3;
        private ViewLabelMark _labelMarkA4;
        private ViewLabelMark _labelMarkA5;

        /// <summary>
        /// The dialog that the "A1" button of the Label tab opens: the content and position of the first view label row
        /// (<c>ViewLabelMarkA1.*</c>).
        /// </summary>
        public ViewLabelMark LabelMarkA1 => _labelMarkA1 ?? (_labelMarkA1 = LabelMark(1));

        /// <summary>The second view label row (<c>ViewLabelMarkA2.*</c>), see <see cref="LabelMarkA1"/>.</summary>
        public ViewLabelMark LabelMarkA2 => _labelMarkA2 ?? (_labelMarkA2 = LabelMark(2));

        /// <summary>The third view label row (<c>ViewLabelMarkA3.*</c>), see <see cref="LabelMarkA1"/>.</summary>
        public ViewLabelMark LabelMarkA3 => _labelMarkA3 ?? (_labelMarkA3 = LabelMark(3));

        /// <summary>The fourth view label row (<c>ViewLabelMarkA4.*</c>), see <see cref="LabelMarkA1"/>.</summary>
        public ViewLabelMark LabelMarkA4 => _labelMarkA4 ?? (_labelMarkA4 = LabelMark(4));

        /// <summary>The fifth view label row (<c>ViewLabelMarkA5.*</c>), see <see cref="LabelMarkA1"/>.</summary>
        public ViewLabelMark LabelMarkA5 => _labelMarkA5 ?? (_labelMarkA5 = LabelMark(5));

        /// <summary>
        /// The page has the top-level keys of the file: <see cref="AttributeSection.CopyFrom"/> and
        /// <see cref="AttributeSection.GetNames"/> leave out the keys of the other pages (<c>vp.draw_as</c>, ...).
        /// </summary>
        private protected override bool OwnsName(string name) => name.IndexOf('.') < 0;

        private ViewLabelMark LabelMark(int row)
        {
            return new ViewLabelMark(Values, Prefix + "ViewLabelMarkA" + row + ".");
        }
    }
}
