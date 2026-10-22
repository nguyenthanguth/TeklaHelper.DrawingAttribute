using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// Base class of an attribute file type (.gd, .vi, .gd.more, ...): the file is the top-level section of its own values.
    /// <para>
    /// Tekla Open API objects take the name of an attribute file without extension, e.g.
    /// <c>new GADrawing("Plan", "my_settings")</c> or <c>new View.ViewAttributes("my_settings")</c>. Tekla looks for the
    /// file in the model <c>attributes</c> folder first, then in the project, firm and system folders, so saving to
    /// <c>&lt;model folder&gt;\attributes</c> makes the file available to the model:
    /// <code>
    /// var settings = new ViewProperties { Attributes = { Scale = 25 } };
    /// settings.SaveTo(Path.Combine(modelPath, "attributes"), "my_settings"); // writes my_settings.vi
    /// </code>
    /// </para>
    /// <para>
    /// A file created from scratch contains only the keys that were set. Tekla fills the missing keys with its own
    /// defaults, so to control every setting load a complete file as a template first (<see cref="Load"/>), change it and
    /// save it under a new name.
    /// </para>
    /// </summary>
    public abstract class AttributeFile : AttributeSection, IAttributeFileCompanion
    {
        /// <summary>
        /// Creates an empty file.
        /// </summary>
        /// <param name="prefix">Prefix of every key, including the trailing dot; empty for most file types.</param>
        protected AttributeFile(string prefix = "")
            : base(new AttributeCollection(), prefix)
        {
        }

        /// <summary>
        /// File extension including the leading dot, e.g. <c>.gd</c> or <c>.gd.more</c>.
        /// </summary>
        public abstract string Extension { get; }

        /// <summary>
        /// Code page the file is read and written in. Loading a file takes it from the file's encoding line. The default is
        /// 1252; Tekla environments also use for example 932 (Japanese) and 949 (Korean). Saving throws
        /// <see cref="System.Text.EncoderFallbackException"/> if a value has a character the code page cannot represent.
        /// </summary>
        public int CodePage
        {
            get => Values.CodePage;
            set => Values.CodePage = value;
        }

        /// <summary>
        /// Whether the file starts with an encoding line. True for the main attribute files, false for .more files.
        /// </summary>
        protected virtual bool WritesEncodingLine => true;

        /// <summary>
        /// Prefix that older files put in front of every key, e.g. <c>view_dial.</c> in old .vi files. It is removed on load,
        /// and files are always saved without it.
        /// </summary>
        protected virtual string LegacyKeyPrefix => null;

        /// <summary>
        /// Files that are loaded and saved together with this one under the same name, e.g. the .gd.more file of a .gd file.
        /// </summary>
        protected virtual IEnumerable<IAttributeFileCompanion> CompanionFiles => Enumerable.Empty<IAttributeFileCompanion>();

        /// <summary>
        /// Loads a file of type <typeparamref name="T"/> and its companion files.
        /// </summary>
        public static T FromFile<T>(string path)
            where T : AttributeFile, new()
        {
            var file = new T();
            file.Load(path);
            return file;
        }

        /// <summary>
        /// Replaces the values with those of <paramref name="path"/>, and loads the companion files with the same name
        /// next to it. A companion file that does not exist is left empty. Every file is read before anything changes, so
        /// if one cannot be read the object keeps its previous content.
        /// </summary>
        /// <param name="path">File to load, e.g. <c>C:\TeklaStructures\...\system\standard.vi</c>.</param>
        public void Load(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            AttributeFileContent content = ReadContent(path, CodePage);
            var apply = new List<Action> { () => Apply(content) };

            string basePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path)) ?? string.Empty, AttributeFileName.FromPath(path, Extension));
            foreach (IAttributeFileCompanion companion in CompanionFiles)
            {
                string companionPath = basePath + companion.Extension;
                apply.Add(companion.PrepareLoad(File.Exists(companionPath) ? companionPath : null, content.CodePage));
            }

            foreach (Action action in apply)
            {
                action();
            }
        }

        /// <summary>
        /// Saves the file and its companion files. The name is the file name of <paramref name="path"/> without
        /// <see cref="Extension"/>.
        /// </summary>
        /// <returns>The path of the main file that was written.</returns>
        public string Save(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            string directory = Path.GetDirectoryName(Path.GetFullPath(path)) ?? string.Empty;
            return SaveTo(directory, AttributeFileName.FromPath(path, Extension));
        }

        /// <summary>
        /// Saves the file as <paramref name="directory"/>\<paramref name="name"/><see cref="Extension"/>, together with its
        /// companion files. The directory is created if needed.
        /// </summary>
        /// <param name="directory">Folder to write to, usually <c>&lt;model folder&gt;\attributes</c>.</param>
        /// <param name="name">Attribute file name without extension, the name passed to the Tekla Open API.</param>
        /// <returns>The path of the main file that was written.</returns>
        public string SaveTo(string directory, string name)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            AttributeFileName.Validate(name);
            Directory.CreateDirectory(directory);

            // Every file of the set is encoded before any is written, so a value the code page cannot hold leaves the files
            // on disk and this object as they were
            string path = Path.Combine(directory, name + Extension);
            var saves = new List<AttributeFileSave>();
            try
            {
                saves.Add(PrepareSave(path, name, CodePage));
                foreach (IAttributeFileCompanion companion in CompanionFiles)
                {
                    saves.Add(companion.PrepareSave(Path.Combine(directory, name + companion.Extension), name, CodePage));
                }
            }
            catch
            {
                for (int i = saves.Count - 1; i >= 0; i--)
                {
                    saves[i].Undo();
                }

                throw;
            }

            foreach (AttributeFileSave save in saves)
            {
                save.Write();
            }

            return path;
        }

        /// <summary>
        /// Returns the bytes this file is saved as (without companion files).
        /// </summary>
        public byte[] ToBytes() => AttributeFileFormat.ToBytes(Values, CodePage, WritesEncodingLine);

        /// <summary>
        /// Returns the text this file is saved as (without companion files).
        /// </summary>
        public string ToText() => AttributeFileFormat.ToText(Values, CodePage, WritesEncodingLine);

        Action IAttributeFileCompanion.PrepareLoad(string path, int codePage)
        {
            if (path == null)
            {
                return () =>
                {
                    Values.Clear();
                    OnCleared();
                };
            }

            AttributeFileContent content = ReadContent(path, codePage);
            return () => Apply(content);
        }

        AttributeFileSave IAttributeFileCompanion.PrepareSave(string path, string name, int codePage) => PrepareSave(path, name, codePage);

        /// <summary>
        /// Called before the file is written under <paramref name="name"/>. By default, files with an encoding line record
        /// the name in <c>in_file</c> and <c>out_file</c> like Tekla does.
        /// </summary>
        protected virtual void OnSaving(string name)
        {
            if (WritesEncodingLine)
            {
                SetString("in_file", name);
                SetString("out_file", name);
            }
        }

        /// <summary>
        /// Called after the values of a file were loaded, before the companion files are read.
        /// </summary>
        /// <param name="hadEncodingLine">Whether the file started with an encoding line (false for legacy files).</param>
        protected virtual void OnLoaded(bool hadEncodingLine)
        {
        }

        /// <summary>
        /// Called after the values were emptied because a companion file did not exist.
        /// </summary>
        protected virtual void OnCleared()
        {
        }

        private AttributeFileContent ReadContent(string path, int fallbackCodePage)
        {
            AttributeFileContent content = AttributeFileFormat.Read(path, fallbackCodePage);
            if (!content.HasEncodingLine && !string.IsNullOrEmpty(LegacyKeyPrefix))
            {
                content.Values.StripPrefix(LegacyKeyPrefix);
            }

            return content;
        }

        private void Apply(AttributeFileContent content)
        {
            Values.Clear();
            foreach (KeyValuePair<string, AttributeValue> pair in content.Values)
            {
                Values.Set(pair.Key, pair.Value);
            }

            CodePage = content.CodePage;
            OnLoaded(content.HasEncodingLine);
        }

        private AttributeFileSave PrepareSave(string path, string name, int codePage)
        {
            var before = new List<KeyValuePair<string, AttributeValue>>(Values);
            int codePageBefore = CodePage;
            void Undo()
            {
                Values.Clear();
                foreach (KeyValuePair<string, AttributeValue> pair in before)
                {
                    Values.Set(pair.Key, pair.Value);
                }

                CodePage = codePageBefore;
            }

            try
            {
                CodePage = codePage;
                OnSaving(name);
                byte[] bytes = ToBytes();
                return new AttributeFileSave(() => File.WriteAllBytes(path, bytes), Undo);
            }
            catch
            {
                Undo();
                throw;
            }
        }
    }
}
