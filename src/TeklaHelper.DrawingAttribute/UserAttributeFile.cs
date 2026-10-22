using System;
using System.Collections.Generic;

namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// Base class of the .more files, which hold the user-defined attributes (the "More..." / UDA tab) of an attribute
    /// file. Every key starts with the object type, e.g. <c>gadrawing_attributes.DR_DRAWN_BY ""</c>; the Get/Set
    /// methods take the attribute name without it, e.g. <c>SetString("DR_DRAWN_BY", "NT")</c>.
    /// <para>
    /// Tekla environments do not agree on the object type of some files (an .ad.more file starts with either
    /// <c>adraw_dial_attributes.</c> or <c>assemblydrawing_attributes.</c>), so loading a file adopts the object type the
    /// file uses, and saving keeps it.
    /// </para>
    /// </summary>
    public abstract class UserAttributeFile : AttributeFile
    {
        private readonly string _defaultPrefix;

        /// <summary>
        /// Creates an empty file.
        /// </summary>
        /// <param name="objectType">Object type in front of every key, e.g. <c>gadrawing_attributes</c>; also the one used
        /// again when a loaded file is empty or missing.</param>
        protected UserAttributeFile(string objectType)
            : base(ToPrefix(objectType))
        {
            _defaultPrefix = Prefix;
        }

        /// <summary>
        /// Object type in front of every key, e.g. <c>gadrawing_attributes</c>. Setting it renames the existing keys.
        /// </summary>
        /// <exception cref="InvalidOperationException">The file already has a key under the new object type for an
        /// attribute that is renamed.</exception>
        public string ObjectType
        {
            get => Prefix.Substring(0, Prefix.Length - 1);
            set
            {
                string prefix = ToPrefix(value);
                if (prefix == Prefix)
                {
                    return;
                }

                var pairs = new List<KeyValuePair<string, AttributeValue>>(Values);
                foreach (KeyValuePair<string, AttributeValue> pair in pairs)
                {
                    if (pair.Key.StartsWith(Prefix, StringComparison.Ordinal) && Values.ContainsKey(prefix + pair.Key.Substring(Prefix.Length)))
                    {
                        throw new InvalidOperationException($"Cannot rename '{pair.Key}': the file already has '{prefix + pair.Key.Substring(Prefix.Length)}'.");
                    }
                }

                Values.Clear();
                foreach (KeyValuePair<string, AttributeValue> pair in pairs)
                {
                    string key = pair.Key.StartsWith(Prefix, StringComparison.Ordinal)
                        ? prefix + pair.Key.Substring(Prefix.Length)
                        : pair.Key;
                    Values.Set(key, pair.Value);
                }

                Prefix = prefix;
            }
        }

        /// <inheritdoc />
        protected override bool WritesEncodingLine => false;

        /// <summary>
        /// Returns the names of the user-defined attributes in the file, without the object type.
        /// </summary>
        public IEnumerable<string> GetAttributeNames()
        {
            foreach (string key in Values.Keys)
            {
                if (key.Length > Prefix.Length && key.StartsWith(Prefix, StringComparison.Ordinal))
                {
                    yield return key.Substring(Prefix.Length);
                }
            }
        }

        /// <inheritdoc />
        protected override void OnLoaded(bool hadEncodingLine)
        {
            // Adopt the object type of the file when every key carries the same one
            string objectType = null;
            foreach (string key in Values.Keys)
            {
                int dot = key.IndexOf('.');
                string type = dot > 0 ? key.Substring(0, dot) : null;
                if (type == null || (objectType != null && type != objectType))
                {
                    // Mixed or undotted keys: no object type to adopt, so not the one of a file loaded before either
                    Prefix = _defaultPrefix;
                    return;
                }

                objectType = type;
            }

            Prefix = objectType != null ? objectType + "." : _defaultPrefix;
        }

        /// <inheritdoc />
        protected override void OnCleared() => Prefix = _defaultPrefix;

        private static string ToPrefix(string objectType)
        {
            if (string.IsNullOrEmpty(objectType) || objectType.IndexOf('.') >= 0)
            {
                throw new ArgumentException("Object type must be a non-empty name without dots, e.g. \"gadrawing_attributes\".", nameof(objectType));
            }

            return objectType + ".";
        }
    }
}
