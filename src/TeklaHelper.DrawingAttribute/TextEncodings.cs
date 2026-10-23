using System.Text;

namespace TeklaHelper.DrawingAttribute
{
    /// <summary>
    /// The encodings of attribute and filter files: Windows code pages (1252, 932, 949, ...) and strict UTF-8.
    /// </summary>
    internal static class TextEncodings
    {
        /// <summary>Code page number of UTF-8.</summary>
        public const int Utf8CodePage = 65001;

        /// <summary>UTF-8 without byte order mark that throws on invalid bytes and on characters it cannot encode.</summary>
        public static readonly Encoding StrictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        static TextEncodings()
        {
#if !NETFRAMEWORK
            // .NET Core / .NET 5+ only know UTF-* and Latin-1 without this provider; .NET Framework has every code page
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
        }

        /// <summary>Returns the encoding for a code page, for reading.</summary>
        public static Encoding Get(int codePage) => codePage == Utf8CodePage ? StrictUtf8 : Encoding.GetEncoding(codePage);

        /// <summary>
        /// Returns the encoding for a code page, for writing: it throws instead of silently writing '?' for characters the
        /// code page does not have.
        /// </summary>
        public static Encoding GetForWriting(int codePage)
        {
            if (codePage == Utf8CodePage)
            {
                return StrictUtf8;
            }

            var encoding = (Encoding)Encoding.GetEncoding(codePage).Clone();
            encoding.EncoderFallback = EncoderFallback.ExceptionFallback;
            return encoding;
        }

        /// <summary>True when <paramref name="count"/> bytes from <paramref name="start"/> are valid UTF-8.</summary>
        public static bool IsUtf8(byte[] bytes, int start, int count) => TryDecodeUtf8(bytes, start, count, out _);

        /// <summary>Decodes UTF-8; false when the bytes are not valid UTF-8.</summary>
        public static bool TryDecodeUtf8(byte[] bytes, int start, int count, out string text)
        {
            try
            {
                text = StrictUtf8.GetString(bytes, start, count);
                return true;
            }
            catch (DecoderFallbackException)
            {
                text = null;
                return false;
            }
        }
    }
}
