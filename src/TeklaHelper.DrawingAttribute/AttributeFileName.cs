using System;
using System.IO;

namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// Checks attribute file names: the name passed to the Tekla Open API, without folder or extension.
    /// </summary>
    internal static class AttributeFileName
    {
        /// <summary>
        /// The attribute file name of <paramref name="path"/>: its file name without <paramref name="extension"/> (compared
        /// without case), or the whole file name when it has another extension.
        /// </summary>
        public static string FromPath(string path, string extension)
        {
            string fileName = Path.GetFileName(path);
            return fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                ? fileName.Substring(0, fileName.Length - extension.Length)
                : fileName;
        }

        public static void Validate(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name must not be empty.", nameof(name));
            }

            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException($"Name '{name}' contains characters that are not allowed in a file name.", nameof(name));
            }
        }
    }
}
