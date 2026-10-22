using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    public class AttributeFileFormatTests
    {
        private const string Sample =
            "encoding 1252\r\n" +
            "in_file \"standard\"\r\n" +
            "view_scale 20.000000\r\n" +
            "cut_on 0\r\n" +
            "vg.FontName \"Arial\"\r\n" +
            "vnpm.ContentString1 \"<?xml version=\"1.0\"?>\u0001<Mark>\u0001</Mark>\"\r\n";

        [Fact]
        public void Read_ParsesEncodingLineAndValues()
        {
            AttributeFileContent content = AttributeFileFormat.Read(Encoding.ASCII.GetBytes(Sample));

            Assert.True(content.HasEncodingLine);
            Assert.Equal(1252, content.CodePage);
            Assert.Equal(new[] { "in_file", "view_scale", "cut_on", "vg.FontName", "vnpm.ContentString1" }, content.Values.Keys);
            Assert.Equal(AttributeValueKind.String, content.Values["in_file"].Kind);
            Assert.Equal("standard", content.Values["in_file"].AsString());
            Assert.Equal(AttributeValueKind.Double, content.Values["view_scale"].Kind);
            Assert.Equal(20.0, content.Values["view_scale"].AsDouble());
            Assert.Equal(AttributeValueKind.Integer, content.Values["cut_on"].Kind);
        }

        [Fact]
        public void Read_KeepsUnescapedQuotesAndLineBreakMarkers()
        {
            AttributeFileContent content = AttributeFileFormat.Read(Encoding.ASCII.GetBytes(Sample));
            AttributeValue value = content.Values["vnpm.ContentString1"];

            Assert.Equal("<?xml version=\"1.0\"?>\u0001<Mark>\u0001</Mark>", value.RawText);
            Assert.Equal("<?xml version=\"1.0\"?>" + Environment.NewLine + "<Mark>" + Environment.NewLine + "</Mark>", value.AsString());
        }

        [Fact]
        public void ReadThenWrite_IsByteForByteIdentical()
        {
            byte[] original = Encoding.ASCII.GetBytes(Sample);
            AttributeFileContent content = AttributeFileFormat.Read(original);

            byte[] written = AttributeFileFormat.ToBytes(content.Values, content.CodePage, content.HasEncodingLine);

            Assert.Equal(original, written);
        }

        [Fact]
        public void Read_UsesCodePageFromEncodingLine()
        {
            byte[] bytes = AttributeFileFormat.ToBytes(new[] { Pair("title_1", AttributeValue.FromString("図面")) }, codePage: 932);

            AttributeFileContent content = AttributeFileFormat.Read(bytes);

            Assert.Equal(932, content.CodePage);
            Assert.Equal("図面", content.Values["title_1"].AsString());
        }

        [Fact]
        public void Utf8XmlValues_AreWrittenInUtf8InsideACodePageFile()
        {
            const string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\u0001<Format font=\"ＭＳ ゴシック\"/>";
            var values = new[]
            {
                Pair("in_file", AttributeValue.FromString("部材マーク")),
                Pair("vpm.ContentString1", AttributeValue.FromString(xml)),
            };

            byte[] bytes = AttributeFileFormat.ToBytes(values, codePage: 932);
            AttributeFileContent content = AttributeFileFormat.Read(bytes);

            Assert.Equal("部材マーク", content.Values["in_file"].RawText);
            Assert.Equal(xml, content.Values["vpm.ContentString1"].RawText);
            Assert.Contains("ＭＳ ゴシック", Encoding.UTF8.GetString(bytes));
        }

        [Fact]
        public void ToBytes_ThrowsForCharactersMissingFromCodePage()
        {
            var values = new[] { Pair("title_1", AttributeValue.FromString("Bản vẽ mặt bằng")) };

            Assert.Throws<EncoderFallbackException>(() => AttributeFileFormat.ToBytes(values, codePage: 1252));
        }

        [Fact]
        public void FromDouble_WritesSixDecimalsRegardlessOfCulture()
        {
            var previous = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("vi-VN");
                Assert.Equal("3.500000", AttributeValue.FromDouble(3.5).ToFileText());
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = previous;
            }
        }

        private static System.Collections.Generic.KeyValuePair<string, AttributeValue> Pair(string key, AttributeValue value) =>
            new System.Collections.Generic.KeyValuePair<string, AttributeValue>(key, value);
    }
}
