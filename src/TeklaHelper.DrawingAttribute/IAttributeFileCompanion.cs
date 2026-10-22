using System;

namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// A file that is loaded and saved together with an <see cref="AttributeFile"/> under the same name, e.g. the
    /// <c>.gd.more</c> (user-defined attributes) and <c>.gd.filter</c> (filter) files of a <c>.gd</c> file.
    /// </summary>
    public interface IAttributeFileCompanion
    {
        /// <summary>
        /// File extension including the leading dot, e.g. <c>.gd.more</c>.
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// Reads the companion file without changing this object, and returns the action that replaces the content with
        /// what was read. The main file reads all its files first and applies them only when every one could be read, so a
        /// broken companion leaves everything as it was.
        /// </summary>
        /// <param name="path">File to read, or null when it does not exist (the content is then emptied).</param>
        /// <param name="codePage">Code page of the main file, used when the companion does not name its own.</param>
        Action PrepareLoad(string path, int codePage);

        /// <summary>
        /// Prepares saving the companion as <paramref name="path"/>: everything that can fail on the content (a character
        /// missing from the code page) fails here, before any file of the set is written.
        /// </summary>
        /// <param name="path">Path of the companion file.</param>
        /// <param name="name">Attribute file name (without extension) of the main file.</param>
        /// <param name="codePage">Code page of the main file.</param>
        /// <returns>The write, and how to undo the changes preparing made to the object.</returns>
        AttributeFileSave PrepareSave(string path, string name, int codePage);
    }

    /// <summary>
    /// A file of an attribute file set that is ready to be written: <see cref="Write"/> writes it, <see cref="Undo"/> undoes
    /// what preparing changed in memory when another file of the set cannot be saved.
    /// </summary>
    public sealed class AttributeFileSave
    {
        /// <summary>
        /// Creates a prepared save.
        /// </summary>
        /// <param name="write">Writes (or deletes) the file.</param>
        /// <param name="undo">Undoes the in-memory changes of preparing; null when there are none.</param>
        public AttributeFileSave(Action write, Action undo = null)
        {
            Write = write ?? throw new ArgumentNullException(nameof(write));
            Undo = undo ?? (() => { });
        }

        /// <summary>Writes (or deletes) the file.</summary>
        public Action Write { get; }

        /// <summary>Undoes the in-memory changes of preparing.</summary>
        public Action Undo { get; }
    }
}
