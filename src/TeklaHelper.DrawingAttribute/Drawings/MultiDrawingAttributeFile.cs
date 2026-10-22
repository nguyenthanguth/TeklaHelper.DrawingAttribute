using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Drawings
{
    /// <summary>
    /// Settings of a multi drawing (.md), with its user-defined attributes (.md.more, object type
    /// <c>joint_attributes</c> like in the Default environment of Tekla 2025). Multi drawings use the same section
    /// prefixes as GA drawings.
    /// </summary>
    public sealed class MultiDrawingAttributeFile : DrawingAttributeFile
    {
        /// <summary>File extension.</summary>
        public const string FileExtension = ".md";

        /// <summary>
        /// Creates an empty file.
        /// </summary>
        public MultiDrawingAttributeFile()
            : base(PageHost.GADrawing, ".md.more", "joint_attributes")
        {
        }

        /// <inheritdoc />
        public override string Extension => FileExtension;

        /// <inheritdoc />
        protected override string LegacyKeyPrefix => "mdraw_dial.";
    }
}
