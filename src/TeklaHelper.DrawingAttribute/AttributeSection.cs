using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// A group of keys in an attribute file that share a prefix. The same settings are often stored in several file
    /// types under different prefixes, e.g. the grid settings are <c>dgrid.*</c> in a .gd file and <c>vg.*</c> in a .vi
    /// file, so one section class describes them once and is reused with the prefix of each file.
    /// <para>
    /// Sections do not copy values: they read and write the <see cref="AttributeCollection"/> of the file they belong to.
    /// Strongly typed properties of derived classes call the Get/Set methods below with the key name without the prefix.
    /// The same methods are public, so keys that have no property yet can still be read and written.
    /// </para>
    /// </summary>
    public abstract class AttributeSection
    {
        /// <summary>
        /// Value Tekla stores in a date attribute that has no date.
        /// </summary>
        public const int EmptyDateValue = int.MinValue;

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Creates a section over <paramref name="values"/>.
        /// </summary>
        /// <param name="values">Values of the file the section belongs to.</param>
        /// <param name="prefix">Prefix of the section keys including the trailing dot, e.g. <c>"dgrid."</c>; empty for the top level.</param>
        protected AttributeSection(AttributeCollection values, string prefix)
        {
            Values = values ?? throw new ArgumentNullException(nameof(values));
            Prefix = prefix ?? string.Empty;
        }

        /// <summary>
        /// All values of the file the section belongs to, including those of other sections.
        /// </summary>
        public AttributeCollection Values { get; }

        /// <summary>
        /// Prefix of the section keys, e.g. <c>"dgrid."</c>. Empty for the top level of a file.
        /// </summary>
        public string Prefix { get; private protected set; }

        /// <summary>
        /// Returns the full key of <paramref name="name"/> in the file, i.e. <see cref="Prefix"/> + <paramref name="name"/>.
        /// </summary>
        public string GetKey(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name must not be empty.", nameof(name));
            }

            return Prefix + name;
        }

        /// <summary>
        /// Returns a section over the keys that start with <see cref="Prefix"/> + <paramref name="subPrefix"/>, for
        /// groups that have no class of their own yet, e.g. <c>GetSection("dpm.")</c> for the part mark settings.
        /// </summary>
        public AttributeSection GetSection(string subPrefix) => new GenericAttributeSection(Values, Prefix + subPrefix);

        /// <summary>
        /// Returns true if the section contains <paramref name="name"/>.
        /// </summary>
        public bool Contains(string name) => Values.ContainsKey(GetKey(name));

        /// <summary>
        /// Gets the value of <paramref name="name"/> if it exists.
        /// </summary>
        public bool TryGetValue(string name, out AttributeValue value) => Values.TryGetValue(GetKey(name), out value);

        /// <summary>
        /// Removes <paramref name="name"/>. Returns false if it did not exist.
        /// </summary>
        public bool Remove(string name) => Values.Remove(GetKey(name));

        /// <summary>
        /// Returns the names (keys without <see cref="Prefix"/>) of every key in the section, in file order, including
        /// the keys of nested groups such as <c>FontAttr.height</c>.
        /// </summary>
        public IEnumerable<string> GetNames()
        {
            foreach (string key in Values.Keys)
            {
                if (key.Length > Prefix.Length && key.StartsWith(Prefix, StringComparison.Ordinal) && OwnsName(key.Substring(Prefix.Length)))
                {
                    yield return key.Substring(Prefix.Length);
                }
            }
        }

        /// <summary>
        /// True when a key under <see cref="Prefix"/> belongs to this section. A section with an empty prefix that is not
        /// the whole file (the top-level keys of a view) leaves out the keys of the other sections.
        /// </summary>
        private protected virtual bool OwnsName(string name) => true;

        /// <summary>
        /// Copies every key of <paramref name="source"/> into this section, e.g. the part mark settings of a view into a
        /// drawing: <c>drawing.Marks.PartMark.CopyFrom(view.Marks.PartMark)</c>. Keys of this section that the source does not have
        /// are kept.
        /// </summary>
        public void CopyFrom(AttributeSection source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (string name in new List<string>(source.GetNames()))
            {
                Set(name, source.Values[source.GetKey(name)]);
            }
        }

        /// <summary>
        /// Saves the section as a file of its own, the way the Save button of a Tekla sub-dialog does: the keys without
        /// <see cref="Prefix"/> in <paramref name="directory"/>\<paramref name="name"/><paramref name="extension"/>, e.g.
        /// <c>view.Grid.ExportTo(attributes, "grid_red", ".vg")</c>. <see cref="SectionFileExtensions"/> lists the
        /// extensions Tekla uses.
        /// </summary>
        /// <param name="directory">Folder to write to.</param>
        /// <param name="name">File name without extension.</param>
        /// <param name="extension">File extension including the leading dot, e.g. <c>.vg</c>.</param>
        /// <param name="codePage">Code page of the file; by default the one of the file the section belongs to.</param>
        /// <returns>The path of the file that was written.</returns>
        public string ExportTo(string directory, string name, string extension, int? codePage = null)
        {
            if (Prefix.Length == 0)
            {
                throw new InvalidOperationException("The top level of a file is saved with AttributeFile.SaveTo, not exported.");
            }

            AttributeFileName.Validate(name);
            Directory.CreateDirectory(directory);

            // Like the dialog: the keys of the page, the loaded name set to this name and the "Save as" name empty
            var file = new AttributeCollection();
            foreach (string sectionName in GetNames())
            {
                file.Set(sectionName, Values[GetKey(sectionName)]);
            }

            file.Set(PageFileInKey, AttributeValue.FromString(name));
            file.Set(PageFileOutKey, AttributeValue.FromString(string.Empty));

            string path = Path.Combine(directory, name + extension);
            AttributeFileFormat.Write(path, file, codePage ?? Values.CodePage);
            return path;
        }

        /// <summary>Key of the page file name in a page file: <c>in_file</c>, or <c>aInFile</c> / <c>InFile</c> in some pages.</summary>
        private protected virtual string PageFileInKey => "in_file";

        /// <summary>Key of the "Save as" name in a page file: <c>out_file</c>, or <c>aOutFile</c> / <c>OutFile</c> in some pages.</summary>
        private protected virtual string PageFileOutKey => "out_file";

        /// <summary>
        /// Loads a file saved by a Tekla sub-dialog (or by <see cref="ExportTo"/>) into this section, e.g.
        /// <c>view.Grid.ImportFrom(@"...\attributes\grid_red.vg")</c>. Keys the file does not have are kept.
        /// </summary>
        /// <param name="path">File to read.</param>
        /// <param name="fallbackCodePage">Code page for a file without encoding line; by default the one of the file the
        /// section belongs to.</param>
        public void ImportFrom(string path, int? fallbackCodePage = null)
        {
            if (Prefix.Length == 0)
            {
                throw new InvalidOperationException("The top level of a file is loaded with AttributeFile.Load, not imported.");
            }

            AttributeFileContent content = AttributeFileFormat.Read(path, fallbackCodePage ?? Values.CodePage);

            // Older page files have no encoding line and prefix every key with the dialog name (vpm_dial.FontAttr.height)
            string legacyPrefix = content.HasEncodingLine ? null : LegacyPagePrefix(content.Values);
            if (legacyPrefix != null)
            {
                content.Values.StripPrefix(legacyPrefix);
            }

            foreach (KeyValuePair<string, AttributeValue> pair in content.Values)
            {
                Set(pair.Key, pair.Value);
            }
        }

        /// <summary>Returns the text value of <paramref name="name"/>, or <paramref name="defaultValue"/> if it is missing.</summary>
        public string GetString(string name, string defaultValue = "") =>
            TryGetValue(name, out AttributeValue value) ? value.AsString() : defaultValue;

        /// <summary>Returns the integer value of <paramref name="name"/>, or <paramref name="defaultValue"/> if it is missing.</summary>
        public int GetInt32(string name, int defaultValue = 0) =>
            TryGetValue(name, out AttributeValue value) ? value.AsInt32(defaultValue) : defaultValue;

        /// <summary>Returns the double value of <paramref name="name"/>, or <paramref name="defaultValue"/> if it is missing.</summary>
        public double GetDouble(string name, double defaultValue = 0) =>
            TryGetValue(name, out AttributeValue value) ? value.AsDouble(defaultValue) : defaultValue;

        /// <summary>Returns the 0 / 1 value of <paramref name="name"/> as a boolean, or <paramref name="defaultValue"/> if it is missing.</summary>
        public bool GetBoolean(string name, bool defaultValue = false) =>
            TryGetValue(name, out AttributeValue value) ? value.AsBoolean(defaultValue) : defaultValue;

        /// <summary>Returns the integer value of <paramref name="name"/> as an enum, or <paramref name="defaultValue"/> if it is missing.</summary>
        public TEnum GetEnum<TEnum>(string name, TEnum defaultValue = default)
            where TEnum : struct, Enum
        {
            if (!TryGetValue(name, out AttributeValue value))
            {
                return defaultValue;
            }

            return (TEnum)Enum.ToObject(typeof(TEnum), value.AsInt32(Convert.ToInt32(defaultValue)));
        }

        /// <summary>
        /// Returns the date of <paramref name="name"/>, stored as seconds since 1970-01-01 UTC, or null if it is missing
        /// or empty (<see cref="EmptyDateValue"/>).
        /// </summary>
        public DateTime? GetDate(string name)
        {
            if (!TryGetValue(name, out AttributeValue value))
            {
                return null;
            }

            int seconds = value.AsInt32(EmptyDateValue);
            return seconds == EmptyDateValue ? (DateTime?)null : UnixEpoch.AddSeconds(seconds);
        }

        /// <summary>
        /// Sets <paramref name="name"/> to a value. A value that means the same as the current one (see
        /// <see cref="AttributeValue.HasSameMeaning"/>) leaves the current one, so that assigning a property its own value
        /// changes no byte of the file.
        /// </summary>
        public void Set(string name, AttributeValue value)
        {
            string key = GetKey(name);
            if (Values.TryGetValue(key, out AttributeValue current) && current.HasSameMeaning(value))
            {
                return;
            }

            Values.Set(key, value);
        }

        /// <summary>
        /// Sets a check box stored as <paramref name="on"/> / <paramref name="off"/>, unless the current value already reads
        /// as <paramref name="value"/>: files saved by Tekla hold values the dialog does not offer (<c>for_sec 3</c>), and
        /// assigning a property its own value must not change them.
        /// </summary>
        private protected void SetBooleanUnlessSame(string name, bool value, int on, int off)
        {
            const int unset = int.MinValue + 1;
            if (TryGetValue(name, out AttributeValue current) && current.Kind != AttributeValueKind.String)
            {
                int stored = current.AsInt32(unset);
                if (stored != unset && (stored != off) == value)
                {
                    return;
                }
            }

            SetInt32(name, value ? on : off);
        }

        /// <summary>The dialog prefix (<c>vpm_dial.</c>) every key of an older page file starts with, or null.</summary>
        private static string LegacyPagePrefix(AttributeCollection values)
        {
            string first = values.Keys.FirstOrDefault();
            int dot = first?.IndexOf('.') ?? -1;
            if (dot <= 0 || !first.Substring(0, dot).EndsWith("_dial", StringComparison.Ordinal))
            {
                return null;
            }

            string prefix = first.Substring(0, dot + 1);
            return values.Keys.All(key => key.StartsWith(prefix, StringComparison.Ordinal)) ? prefix : null;
        }

        /// <summary>Sets <paramref name="name"/> to a quoted text value.</summary>
        public void SetString(string name, string value) => Set(name, AttributeValue.FromString(value));

        /// <summary>Sets <paramref name="name"/> to an integer value.</summary>
        public void SetInt32(string name, int value) => Set(name, AttributeValue.FromInt32(value));

        /// <summary>Sets <paramref name="name"/> to a double value (six decimals).</summary>
        public void SetDouble(string name, double value) => Set(name, AttributeValue.FromDouble(value));

        /// <summary>
        /// Sets <paramref name="name"/> to 1 or 0, unless the current value already reads as <paramref name="value"/>: files
        /// saved by Tekla hold other non-zero values too (<c>dv.cre_front_view 2</c>), and assigning a property its own
        /// value must not change them.
        /// </summary>
        public void SetBoolean(string name, bool value) => SetBooleanUnlessSame(name, value, 1, 0);

        /// <summary>Sets <paramref name="name"/> to the integer value of an enum.</summary>
        public void SetEnum<TEnum>(string name, TEnum value)
            where TEnum : struct, Enum => SetInt32(name, Convert.ToInt32(value));

        /// <summary>
        /// Sets <paramref name="name"/> to a date, stored as seconds since 1970-01-01 UTC; null stores <see cref="EmptyDateValue"/>.
        /// </summary>
        public void SetDate(string name, DateTime? value)
        {
            if (value == null)
            {
                SetInt32(name, EmptyDateValue);
                return;
            }

            DateTime utc = value.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value.Value.ToUniversalTime();
            double seconds = Math.Floor((utc - UnixEpoch).TotalSeconds);

            // Stored as a 32-bit number of seconds whose lowest value means "no date"
            if (seconds <= EmptyDateValue || seconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Tekla stores dates from 1901-12-13 20:45:53 to 2038-01-19 03:14:07 UTC.");
            }

            SetInt32(name, (int)seconds);
        }

        /// <summary>
        /// Returns the state of the check box <paramref name="name"/> + <c>_en</c> in the Tekla dialog. A checked box
        /// means the setting is applied when the attribute file is loaded into an existing object with Modify. Missing
        /// flags count as checked.
        /// </summary>
        public bool IsEnabled(string name) => GetBoolean(name + "_en", true);

        /// <summary>
        /// Sets the check box <paramref name="name"/> + <c>_en</c>. The names of check boxes do not always match a
        /// single key (for example <c>cut_param_en</c> covers every shortening key), so pass the check box name.
        /// </summary>
        public void SetEnabled(string name, bool enabled) => SetBoolean(name + "_en", enabled);

        private sealed class GenericAttributeSection : AttributeSection
        {
            public GenericAttributeSection(AttributeCollection values, string prefix)
                : base(values, prefix)
            {
            }
        }
    }
}
