using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeklaHelper.DrawingAttribute.Marks;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>
    /// Mark content: the XML of the <c>ContentString</c> keys, read into elements and written back the way Tekla writes it.
    /// </summary>
    public sealed class MarkContentTests
    {
        private const string Head = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n<!DOCTYPE Mark><Mark>\n";

        [Fact]
        public void ViewLabel_WritesTheElementsLikeTheMarkDialogs()
        {
            var view = new ViewProperties();

            view.Attributes.LabelMarkA1.Elements = new MarkContent()
                .Add(MarkElementTypes.ViewLabel.ViewName, new MarkFormat { Font = "Tahoma", Color = DrawingColor.Green, Height = 5.158 })
                .AddText(" - ")
                .Add(MarkElementTypes.ViewLabel.Scale);

            Assert.Equal(
                Head
                + "<MarkPart groupid=\"0\">\n<Element name=\"NAME\" value=\"\" subType=\"82\">\n<Format font=\"Tahoma\" color=\"161\" height=\"5.158\" hidden=\"0\"/>\n</Element>\n</MarkPart>\n"
                + "<MarkPart groupid=\"0\">\n<UserText>\n - \n<Format font=\"Arial\" color=\"153\" height=\"2.500\" hidden=\"0\"/>\n</UserText>\n</MarkPart>\n"
                + "<MarkPart groupid=\"0\">\n<Element name=\"SCALE\" value=\"\" subType=\"79\">\n<Format font=\"Arial\" color=\"153\" height=\"2.500\" hidden=\"0\"/>\n</Element>\n</MarkPart>\n"
                + "</Mark>\n",
                view.GetString("ViewLabelMarkA1.ContentString").Replace("\r\n", "\n"));
        }

        [Fact]
        public void Compact_IsTheLayoutOfTheViewDialog()
        {
            var content = new MarkContent { Layout = MarkContentLayout.Compact }.Add(MarkElementTypes.ViewLabel.ViewName, new MarkFormat { Color = (DrawingColor)155 });

            Assert.Equal(
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><!DOCTYPE Mark><Mark><MarkPart groupid=\"0\"><Element name=\"NAME\" value=\"\" subType=\"82\"><Format font=\"Arial\" color=\"155\" height=\"2.500\"/></Element></MarkPart></Mark>",
                content.ToXml());
        }

        [Fact]
        public void Parse_ReadsEveryKindOfElement()
        {
            string xml = Head
                + "<MarkPart groupid=\"0\">\n<UserText>\nM\n<Format font=\"Arial\" color=\"161\" height=\"2.500\" hidden=\"0\"/>\n</UserText>\n</MarkPart>\n"
                + "<MarkPart groupid=\"0\">\n<BackSpace/>\n</MarkPart>\n"
                + "<MarkPart groupid=\"0\">\n<SizeElement value=\"\" subType=\"47\">\n<Format font=\"Arial\" color=\"161\" height=\"2.500\" hidden=\"0\"/>\n</SizeElement>\n</MarkPart>\n"
                + "<MarkPart groupid=\"0\">\n<Element name=\"USER_FIELD_1\" value=\"\" subType=\"16\">\n<Format font=\"Arial\" unit=\"1\" format=\"3\" color=\"153\" height=\"2.500\" hidden=\"0\" override=\"1\"/>\n</Element>\n</MarkPart>\n"
                + "<MarkPart groupid=\"0\">\n<Frame type=\"1\" color=\"153\" frame_id=\"145\">\n<Mark>\n<MarkPart groupid=\"0\">\n<Element name=\"\" value=\"\" subType=\"42\">\n<Format font=\"Arial\" color=\"164\" height=\"2.500\" hidden=\"0\"/>\n</Element>\n</MarkPart>\n</Mark>\n</Frame>\n</MarkPart>\n"
                + "<MarkPart groupid=\"0\">\n<Symbol symbol=\"xsteel@85\">\n<Format font=\"Arial\" color=\"153\" height=\"2.500\" hidden=\"0\" override=\"1\"/>\n</Symbol>\n</MarkPart>\n"
                + "<MarkPart groupid=\"0\">\n<Template filename=\"Pour_object_mark\"/>\n</MarkPart>\n"
                + "<MarkPart groupid=\"0\">\n<LineFeed/>\n</MarkPart>\n"
                + "<MarkPart groupid=\"0\">\n<MergeElement subType=\"53\" BlockPrefixMulti=\"%NUMBER%*\" BlockPrefixSingle=\"1*\">\n<Format font=\"Arial\" color=\"164\" height=\"2.500\" hidden=\"0\"/>\n</MergeElement>\n</MarkPart>\n"
                + "</Mark>\n";

            MarkContent content = MarkContent.Parse(xml);

            Assert.Equal("M", Assert.IsType<MarkTextElement>(content[0]).Text);
            Assert.Equal(MarkSeparatorKind.BackSpace, Assert.IsType<MarkSeparatorElement>(content[1]).Kind);
            Assert.Equal(MarkElementTypes.BoltMark.Size, Assert.IsType<MarkPropertyElement>(content[2]).Type);
            var user = Assert.IsType<MarkUserAttributeElement>(content[3]);
            Assert.Equal("USER_FIELD_1", user.Name);
            Assert.Equal(Units.Millimeters, user.Format.Unit);
            Assert.Equal(UnitsFormat.OneDecimal, user.Format.NumberFormat);
            var frame = Assert.IsType<MarkFrameElement>(content[4]);
            Assert.Equal(FrameAroundElementsType.Rectangular, frame.Type);
            Assert.Equal(MarkElementTypes.ReinforcementMark.Position, Assert.IsType<MarkPropertyElement>(Assert.Single(frame.Content)).Type);
            var symbol = Assert.IsType<MarkSymbolElement>(content[5]);
            Assert.Equal(("xsteel", 85), (symbol.File, symbol.Index));
            Assert.Equal("Pour_object_mark", Assert.IsType<MarkTemplateElement>(content[6]).FileName);
            Assert.Equal(MarkSeparatorKind.LineFeed, Assert.IsType<MarkSeparatorElement>(content[7]).Kind);
            Assert.Equal("MergeElement", Assert.IsType<MarkXmlElement>(content[8]).Tag);
            Assert.Equal(MarkContentLayout.Lines, content.Layout);
            Assert.Equal(xml, content.ToXml());
        }

        [Fact]
        public void SetMarkContent_KeepsTheBytesOfTheSameContent_AndTheLayoutOfTheValue()
        {
            var view = new ViewProperties();
            const string compact = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><!DOCTYPE Mark><Mark><MarkPart groupid=\"0\"><Element name=\"NAME\" value=\"\" subType=\"82\"><Format font=\"Arial\" color=\"155\" height=\"2.500\"/></Element></MarkPart></Mark>";
            view.SetString("ViewLabelMarkA1.ContentString", compact);

            view.Attributes.LabelMarkA1.Elements = view.Attributes.LabelMarkA1.Elements;
            Assert.Equal(compact, view.GetString("ViewLabelMarkA1.ContentString"));

            view.Attributes.LabelMarkA1.Elements = new MarkContent().Add(MarkElementTypes.ViewLabel.Scale);
            Assert.StartsWith("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><!DOCTYPE Mark><Mark><MarkPart", view.GetString("ViewLabelMarkA1.ContentString"), StringComparison.Ordinal);
        }

        [Fact]
        public void NewFrames_GetIdsAfterThoseOfTheContent()
        {
            // Tekla 2025 read the elements of a frame with frame_id 0 as following the frame, not inside it
            MarkContent content = MarkContent.Parse(Head + "<MarkPart groupid=\"0\">\n<Frame type=\"1\" color=\"153\" frame_id=\"104\">\n<Mark/>\n</Frame>\n</MarkPart>\n</Mark>\n");
            content.AddFrame(FrameAroundElementsType.Round, DrawingColor.Red, new MarkContent().Add(MarkElementTypes.PartMark.PartPosition)
                .AddFrame(FrameAroundElementsType.Line, DrawingColor.Blue, new MarkContent().Add(MarkElementTypes.PartMark.Profile)));

            string xml = content.ToXml();

            Assert.Contains("frame_id=\"104\"", xml);
            Assert.Contains("<Frame type=\"3\" color=\"160\" frame_id=\"105\">", xml);
            Assert.Contains("<Frame type=\"2\" color=\"162\" frame_id=\"106\">", xml);
            Assert.Contains("frame_id=\"101\"", new MarkContent().AddFrame(FrameAroundElementsType.Round, DrawingColor.Red, new MarkContent()).ToXml());
        }

        [Fact]
        public void EveryContentOfTheDefaultFiles_IsMarkContent()
        {
            var files = new List<AttributeFile>
            {
                ViewProperties.CreateDefault(), Drawings.GADrawingAttributeFile.CreateDefault(), Drawings.MultiDrawingAttributeFile.CreateDefault(),
                Drawings.AssemblyDrawingAttributeFile.CreateDefault(), Drawings.SinglePartDrawingAttributeFile.CreateDefault(),
                Drawings.CastUnitDrawingAttributeFile.CreateDefault(),
            };
            files.AddRange(GeneratedPagesTests.ObjectFileTypes().Select(t => (AttributeFile)t.GetMethod("CreateDefault").Invoke(null, null)));

            int count = 0;
            foreach (AttributeFile file in files)
            {
                foreach (string key in file.Values.Keys.Where(k => k.EndsWith("ContentString", StringComparison.Ordinal) || System.Text.RegularExpressions.Regex.IsMatch(k, @"ContentString\d$")))
                {
                    if (file.GetString(key).Length > 0)
                    {
                        Assert.NotNull(file.GetMarkContent(key));
                        count++;
                    }
                }
            }

            Assert.True(count > 100, "only " + count);
        }

        [Fact]
        public void ElementTypes_AreTheSubTypesTeklaReads()
        {
            Assert.Equal(23, MarkElementTypes.PartMark.PartPosition.SubType);
            Assert.Equal(79, MarkElementTypes.ViewLabel.Scale.SubType);
            Assert.Equal(82, MarkElementTypes.ViewLabel.ViewName.SubType);
            Assert.Equal(81, MarkElementTypes.SectionMark.SectionName.SubType);
            Assert.Equal(80, MarkElementTypes.DetailMark.DetailName.SubType);
            Assert.Equal(92, MarkElementTypes.ReinforcementMark.CcTarget.SubType);
            Assert.True(MarkElementTypes.BoltMark.Size.IsSizeElement);
            Assert.Same(MarkElementTypes.PartMark.Profile, MarkElementType.FromSubType(24));
            Assert.Equal("Sub-type 300", MarkElementType.FromSubType(300).Description);
        }

        [Fact]
        public void Parse_RejectsTextThatIsNoMarkContent()
        {
            Assert.Throws<FormatException>(() => MarkContent.Parse("plain text"));
            Assert.Throws<FormatException>(() => MarkContent.Parse("<Other/>"));
            Assert.False(MarkContent.TryParse("<Mark><MarkPart>", out _));
        }

        /// <summary>
        /// TEKLA_ATTRIBUTE_SAMPLES names a folder of files shipped with Tekla: every mark content in them that is XML is read,
        /// and written back exactly as it was (except values that mix two encodings, which are no text, and spaces around
        /// <c>=</c> in hand-edited files; <see cref="AttributeSection.SetMarkContent"/> keeps their bytes). Passes without
        /// checking anything when the variable is not set.
        /// </summary>
        [Fact]
        public void EveryMarkContentOfTheShippedFiles_IsWrittenBackAsItWas()
        {
            string root = Environment.GetEnvironmentVariable("TEKLA_ATTRIBUTE_SAMPLES");
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return;
            }

            int read = 0, invalid = 0, mixed = 0;
            var failures = new List<string>();
            foreach (string path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                byte[] bytes;
                try
                {
                    bytes = File.ReadAllBytes(path);
                }
                catch (IOException)
                {
                    continue;
                }

                if (bytes.Length > 50_000_000 || System.Text.Encoding.ASCII.GetString(bytes).IndexOf("<Mark>", StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                AttributeFileContent content;
                try
                {
                    content = AttributeFileFormat.Read(path);
                }
                catch (FormatException)
                {
                    continue;
                }

                foreach (KeyValuePair<string, AttributeValue> pair in content.Values)
                {
                    string text = pair.Value.AsString().Replace("\r\n", "\n");
                    if (pair.Value.Kind != AttributeValueKind.String || text.IndexOf("<Mark>", StringComparison.Ordinal) < 0)
                    {
                        continue;
                    }

                    // Values that mix UTF-8 and the file's code page (some Japanese files) are no text in either; the library
                    // keeps their bytes, and no reading of them can be written back the same
                    byte[] valueBytes = pair.Value.GetFileBytes(content.Values.CodePage);
                    if (valueBytes != null && !TextEncodings.TryDecodeUtf8(valueBytes, 0, valueBytes.Length, out _))
                    {
                        mixed++;
                        continue;
                    }

                    if (!MarkContent.TryParse(text, out MarkContent mark))
                    {
                        // Only text that is no mark content at all may fail (a DSTV converter file holds " <Mark>")
                        invalid++;
                        if (text.TrimStart().StartsWith("<?xml", StringComparison.Ordinal))
                        {
                            failures.Add(path.Substring(root.Length) + " " + pair.Key + ": not read" + Environment.NewLine + "  file:    " + text.Replace("\n", "|"));
                        }

                        continue;
                    }

                    read++;
                    string written = mark.ToXml();

                    // Hundreds of shipped files hold an attribute without its value (value= subType="45"), written back with
                    // an empty one; a few hand-edited files hold a space around "=" (font= "...") that no XML reading keeps
                    string repaired = System.Text.RegularExpressions.Regex.Replace(text, @"(\s[A-Za-z_]+)=(?=\s+[A-Za-z_]+="")", "$1=\"\"");
                    if (written != text && written != repaired)
                    {
                        repaired = System.Text.RegularExpressions.Regex.Replace(text, @"\s*=\s*""", "=\"");
                    }

                    if (written != text && written != repaired)
                    {
                        failures.Add(path.Substring(root.Length) + " " + pair.Key + Environment.NewLine + "  file:    " + text.Replace("\n", "|") + Environment.NewLine + "  written: " + written.Replace("\n", "|"));
                    }
                }
            }

            Assert.True(read > 500, "only " + read + " mark contents read");
            Assert.True(failures.Count == 0, $"{failures.Count} of {read} differ ({invalid} not XML, {mixed} of mixed encodings):" + Environment.NewLine + string.Join(Environment.NewLine, failures.Take(8)));
        }
    }
}
