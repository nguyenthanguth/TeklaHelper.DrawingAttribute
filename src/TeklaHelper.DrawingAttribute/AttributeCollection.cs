using System;
using System.Collections;
using System.Collections.Generic;

namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// The keys and values of an attribute file, in file order. New keys are appended at the end; changing an existing
    /// key keeps its position, so a template file keeps its layout when it is saved again.
    /// <para>Keys are compared case-sensitively, like Tekla does (<c>StyleName</c> and <c>stylename</c> are different keys).</para>
    /// </summary>
    public sealed class AttributeCollection : IEnumerable<KeyValuePair<string, AttributeValue>>
    {
        private readonly List<string> _keys = new List<string>();
        private readonly Dictionary<string, AttributeValue> _values = new Dictionary<string, AttributeValue>(StringComparer.Ordinal);

        /// <summary>
        /// Number of keys.
        /// </summary>
        public int Count => _keys.Count;

        /// <summary>
        /// Code page of the file the values belong to (<see cref="AttributeFile.CodePage"/>), so its sections export and
        /// import sub-dialog files in the same code page.
        /// </summary>
        internal int CodePage { get; set; } = AttributeFileFormat.DefaultCodePage;

        /// <summary>
        /// Keys in file order.
        /// </summary>
        public IReadOnlyList<string> Keys => _keys;

        /// <summary>
        /// Gets or sets the value of <paramref name="key"/>. Getting a missing key throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        public AttributeValue this[string key]
        {
            get => _values[key];
            set => Set(key, value);
        }

        /// <summary>
        /// Returns true if the collection contains <paramref name="key"/>.
        /// </summary>
        public bool ContainsKey(string key) => key != null && _values.ContainsKey(key);

        /// <summary>
        /// Gets the value of <paramref name="key"/> if it exists.
        /// </summary>
        public bool TryGetValue(string key, out AttributeValue value)
        {
            if (key == null)
            {
                value = default;
                return false;
            }

            return _values.TryGetValue(key, out value);
        }

        /// <summary>
        /// Sets the value of <paramref name="key"/>, adding the key at the end if it does not exist yet.
        /// </summary>
        public void Set(string key, AttributeValue value)
        {
            ValidateKey(key);

            if (!_values.ContainsKey(key))
            {
                _keys.Add(key);
            }

            _values[key] = value;
        }

        /// <summary>
        /// Removes <paramref name="key"/>. Returns false if it did not exist.
        /// </summary>
        public bool Remove(string key)
        {
            if (key == null || !_values.Remove(key))
            {
                return false;
            }

            _keys.Remove(key);
            return true;
        }

        /// <summary>
        /// Removes every key that starts with <paramref name="prefix"/> and returns how many were removed.
        /// </summary>
        public int RemoveWithPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                throw new ArgumentException("Prefix must not be empty.", nameof(prefix));
            }

            int removed = _keys.RemoveAll(k => k.StartsWith(prefix, StringComparison.Ordinal));
            if (removed > 0)
            {
                var keep = new HashSet<string>(_keys, StringComparer.Ordinal);
                var toRemove = new List<string>();
                foreach (string key in _values.Keys)
                {
                    if (!keep.Contains(key))
                    {
                        toRemove.Add(key);
                    }
                }

                foreach (string key in toRemove)
                {
                    _values.Remove(key);
                }
            }

            return removed;
        }

        /// <summary>
        /// Removes all keys.
        /// </summary>
        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
        }

        /// <summary>
        /// Renames keys that start with <paramref name="prefix"/> by removing that prefix, keeping their order.
        /// Used to read legacy files whose keys carry the dialog name, e.g. <c>view_dial.view_scale</c>.
        /// </summary>
        internal void StripPrefix(string prefix)
        {
            for (int i = 0; i < _keys.Count; i++)
            {
                string key = _keys[i];
                if (key.Length > prefix.Length && key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    string newKey = key.Substring(prefix.Length);
                    if (_values.ContainsKey(newKey))
                    {
                        continue;
                    }

                    _values[newKey] = _values[key];
                    _values.Remove(key);
                    _keys[i] = newKey;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, AttributeValue>> GetEnumerator()
        {
            foreach (string key in _keys)
            {
                yield return new KeyValuePair<string, AttributeValue>(key, _values[key]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must not be empty.", nameof(key));
            }

            foreach (char c in key)
            {
                if (char.IsWhiteSpace(c) || c == '"')
                {
                    throw new ArgumentException($"Key '{key}' must not contain white space or quotes.", nameof(key));
                }
            }
        }
    }
}
