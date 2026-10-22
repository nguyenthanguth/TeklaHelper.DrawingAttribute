using System;

namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// User-defined attributes of a drawing (.gd.more, .ad.more, .wd.more, .cud.more, .md.more): the comment,
    /// <c>DRAWING_USERFIELD_1</c> ... <c>DRAWING_USERFIELD_8</c> and the approval fields (<c>DR_*</c>). The .md.more files
    /// shipped with Tekla have no approval fields.
    /// </summary>
    public class DrawingUserAttributeFile : UserAttributeFile
    {
        /// <summary>Number of <c>DRAWING_USERFIELD_n</c> attributes.</summary>
        public const int UserFieldCount = 8;

        /// <summary>
        /// Creates an empty file.
        /// </summary>
        /// <param name="extension">File extension, e.g. <c>.gd.more</c>.</param>
        /// <param name="objectType">Object type in front of every key, e.g. <c>gadrawing_attributes</c>.</param>
        public DrawingUserAttributeFile(string extension, string objectType)
            : base(objectType)
        {
            if (string.IsNullOrEmpty(extension) || extension[0] != '.')
            {
                throw new ArgumentException("Extension must start with a dot, e.g. \".gd.more\".", nameof(extension));
            }

            Extension = extension;
        }

        /// <inheritdoc />
        public override string Extension { get; }

        /// <summary>Comment (<c>comment</c>).</summary>
        public string Comment
        {
            get => GetString("comment");
            set => SetString("comment", value);
        }

        /// <summary>Assigned to (<c>DR_ASSIGNED_TO</c>).</summary>
        public string AssignedTo
        {
            get => GetString("DR_ASSIGNED_TO");
            set => SetString("DR_ASSIGNED_TO", value);
        }

        /// <summary>Assigned by (<c>DR_ASSIGNED_BY</c>).</summary>
        public string AssignedBy
        {
            get => GetString("DR_ASSIGNED_BY");
            set => SetString("DR_ASSIGNED_BY", value);
        }

        /// <summary>Drawn by (<c>DR_DRAWN_BY</c>).</summary>
        public string DrawnBy
        {
            get => GetString("DR_DRAWN_BY");
            set => SetString("DR_DRAWN_BY", value);
        }

        /// <summary>Checked by (<c>DR_CHECKED_BY</c>).</summary>
        public string CheckedBy
        {
            get => GetString("DR_CHECKED_BY");
            set => SetString("DR_CHECKED_BY", value);
        }

        /// <summary>Checked date (<c>DR_CHECKED_DATE</c>); null when empty.</summary>
        public DateTime? CheckedDate
        {
            get => GetDate("DR_CHECKED_DATE");
            set => SetDate("DR_CHECKED_DATE", value);
        }

        /// <summary>Approval sent date (<c>DR_APPROVAL_SENT</c>); null when empty.</summary>
        public DateTime? ApprovalSent
        {
            get => GetDate("DR_APPROVAL_SENT");
            set => SetDate("DR_APPROVAL_SENT", value);
        }

        /// <summary>Approval date (<c>DR_APPROVAL_DATE</c>); null when empty.</summary>
        public DateTime? ApprovalDate
        {
            get => GetDate("DR_APPROVAL_DATE");
            set => SetDate("DR_APPROVAL_DATE", value);
        }

        /// <summary>Approved by (<c>DR_APPROVED_BY</c>).</summary>
        public string ApprovedBy
        {
            get => GetString("DR_APPROVED_BY");
            set => SetString("DR_APPROVED_BY", value);
        }

        /// <summary>
        /// Returns <c>DRAWING_USERFIELD_<paramref name="number"/></c>, <paramref name="number"/> from 1 to 8.
        /// </summary>
        public string GetUserField(int number) => GetString(GetUserFieldName(number));

        /// <summary>
        /// Sets <c>DRAWING_USERFIELD_<paramref name="number"/></c>, <paramref name="number"/> from 1 to 8.
        /// </summary>
        public void SetUserField(int number, string value) => SetString(GetUserFieldName(number), value);

        private static string GetUserFieldName(int number)
        {
            if (number < 1 || number > UserFieldCount)
            {
                throw new ArgumentOutOfRangeException(nameof(number), number, $"User field number must be between 1 and {UserFieldCount}.");
            }

            return "DRAWING_USERFIELD_" + number;
        }
    }
}
