using System;

namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// An attribute file of any type that has no class of its own yet (.ad, .wd, .cud, .dim, ...), read and written
    /// through the Get/Set methods by key name.
    /// <code>
    /// var dimensions = new GenericAttributeFile(".dim");
    /// dimensions.Load(@"...\system\standard.dim");
    /// dimensions.SetDouble("dim_text_height", 3.0);
    /// dimensions.SaveTo(attributesDirectory, "my_dimensions");
    /// </code>
    /// </summary>
    public sealed class GenericAttributeFile : AttributeFile
    {
        private readonly bool _writesEncodingLine;

        /// <summary>
        /// Creates an empty file.
        /// </summary>
        /// <param name="extension">File extension including the leading dot, e.g. <c>.ad</c> or <c>.ad.more</c>.</param>
        /// <param name="writesEncodingLine">Whether the file starts with an encoding line; false for .more files.</param>
        public GenericAttributeFile(string extension, bool writesEncodingLine = true)
        {
            if (string.IsNullOrEmpty(extension) || extension[0] != '.')
            {
                throw new ArgumentException("Extension must start with a dot, e.g. \".ad\".", nameof(extension));
            }

            Extension = extension;
            _writesEncodingLine = writesEncodingLine;
        }

        /// <inheritdoc />
        public override string Extension { get; }

        /// <inheritdoc />
        protected override bool WritesEncodingLine => _writesEncodingLine;
    }
}
