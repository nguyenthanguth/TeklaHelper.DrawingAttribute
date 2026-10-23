using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TeklaHelper.DrawingAttribute.Marks;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>The rules of mark content: the numbers Tekla reads, the damage of shipped files, and what the writer refuses.</summary>
    public sealed class MarkContentRulesTests
    {
        private const string Head = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n<!DOCTYPE Mark><Mark>\n";

        private static string Part(string inner) => "<MarkPart groupid=\"0\">\n" + inner + "\n</MarkPart>\n";

        /// <summary>
        /// The element Tekla 2025 returned through the Open API for a file holding only sub-type N (the probe of 1 to 110,
        /// the same in Mark.MarkAttributes, View.ViewAttributes, SectionMarkAttributes and DetailMarkAttributes), by the name
        /// of the API's PropertyTypes.
        /// </summary>
        private static readonly Dictionary<string, int> ReadByTekla = new Dictionary<string, int>
        {
            ["GR_BOLT_LENGTH"] = 7, ["GR_BOLT_DIAMETER"] = 8, ["GR_HOLE_DIAMETER"] = 9, ["GR_MATERIAL"] = 10, ["GR_BOLT_STANDARD"] = 11,
            ["GR_SHORT_NAME"] = 12, ["GR_FULL_NAME"] = 13, ["GR_BOLT_ASSEMBLY_TYPE"] = 14, ["GR_BOLT_NUMBER"] = 15, ["GR_LONGHOLE_X"] = 17,
            ["GR_LONGHOLE_Y"] = 18, ["GR_LONGHOLE_MIN"] = 19, ["GR_LONGHOLE_MAX"] = 20, ["GR_COUNTERSUNK"] = 21, ["GR_ASSEMBLY_POSITION"] = 22,
            ["GR_PART_POSITION"] = 23, ["GR_PROFILE"] = 24, ["GR_NAME"] = 25, ["GR_LENGTH"] = 26, ["GR_CAMBER"] = 27, ["GR_SIZE"] = 28,
            ["GR_FITTINGS_NS_FS"] = 29, ["GR_JOINT_CODE"] = 30, ["GR_DSTV_CODE"] = 31, ["GR_JOINT_NUMBER"] = 32, ["GR_JOINT_GROUP"] = 33,
            ["GR_JOINT_ERROR"] = 34, ["GR_OBJECT_NAME"] = 35, ["GR_GRADE"] = 36, ["GR_DIAMETER"] = 37, ["GR_CLASS"] = 38,
            ["GR_OBJECT_LENGTH"] = 40, ["GR_NUMBER"] = 41, ["GR_OBJECT_POSITION"] = 42, ["GR_SHAPE"] = 43, ["GR_WEIGHT"] = 44, ["GR_CC"] = 45,
            ["GR_BOLT_SIZE"] = 47, ["GR_GOL"] = 48, ["GR_GAGE"] = 49, ["GR_CC_MIN"] = 50, ["GR_CC_MAX"] = 51, ["GR_CC_EXACT"] = 52,
            ["GR_FACE_DIRECTION"] = 57, ["GR_FINISH"] = 58, ["GR_LABEL"] = 59, ["GR_REBAR_MESH_LENGTH"] = 61, ["GR_REBAR_MESH_WIDTH"] = 62,
            ["GR_CC_LONG"] = 63, ["GR_CC_MIN_LONG"] = 64, ["GR_CC_MAX_LONG"] = 65, ["GR_CC_EXACT_LONG"] = 66, ["GR_CC_CROSS"] = 67,
            ["GR_CC_MIN_CROSS"] = 68, ["GR_CC_MAX_CROSS"] = 69, ["GR_CC_EXACT_CROSS"] = 70, ["GR_MESH_SIZE"] = 71, ["GR_MESH_DIAMETER_LONG"] = 72,
            ["GR_MESH_DIAMETER_CROSS"] = 73, ["GR_SURFACING_NAME"] = 74, ["GR_SURFACING_CODE"] = 75, ["GR_JOINT_RUNNING_NUMBER"] = 78,
            ["GR_SCALE"] = 79, ["GR_DETAIL_NAME"] = 80, ["GR_SECTION_NAME"] = 81, ["GR_VIEW_NAME"] = 82, ["GR_DRAWING_NAME"] = 83,
            ["GR_SOURCE_DRAWING_NAME"] = 84, ["GR_SOURCE_DRAWING_NAME_WHEN_MOVED"] = 85, ["GR_CHAMFER_DX"] = 87, ["GR_CHAMFER_DY"] = 88,
            ["GR_CC_TARGET"] = 92, ["GR_POUR_NUMBER"] = 93, ["GR_POUR_TYPE"] = 94, ["GR_POUR_CONCRETE_MIXTURE"] = 95, ["GR_ROTATION_ANGLE"] = 96,
            ["GR_LAYER"] = 97, ["GR_HOLE_DEPTH"] = 98,
        };

        /// <summary>
        /// The element of each entry of the catalog, by the Open API's *PropertyElementTypes (Tekla 2025): e.g.
        /// BoltMarkPropertyElementTypes.Size() is GR_BOLT_SIZE, SectionViewLabelMarkPropertyElementTypes.SectionName() GR_VIEW_NAME.
        /// </summary>
        private static readonly Dictionary<string, string> ApiElement = new Dictionary<string, string>
        {
            ["PartMark.AssemblyPosition"] = "GR_ASSEMBLY_POSITION", ["PartMark.PartPosition"] = "GR_PART_POSITION", ["PartMark.Profile"] = "GR_PROFILE",
            ["PartMark.Material"] = "GR_MATERIAL", ["PartMark.Name"] = "GR_NAME", ["PartMark.Class"] = "GR_CLASS", ["PartMark.Finish"] = "GR_FINISH",
            ["PartMark.Size"] = "GR_SIZE", ["PartMark.Length"] = "GR_LENGTH", ["PartMark.Camber"] = "GR_CAMBER", ["PartMark.FittingsNsFs"] = "GR_FITTINGS_NS_FS",
            ["PartMark.FaceDirection"] = "GR_FACE_DIRECTION", ["PartMark.GageOfOutstandingLeg"] = "GR_GOL", ["PartMark.CenterToCenterDistance"] = "GR_GAGE",
            ["PartMark.RotationAngle"] = "GR_ROTATION_ANGLE",
            ["BoltMark.BoltLength"] = "GR_BOLT_LENGTH", ["BoltMark.BoltDiameter"] = "GR_BOLT_DIAMETER", ["BoltMark.HoleDiameter"] = "GR_HOLE_DIAMETER",
            ["BoltMark.Material"] = "GR_MATERIAL", ["BoltMark.Standard"] = "GR_BOLT_STANDARD", ["BoltMark.ShortName"] = "GR_SHORT_NAME",
            ["BoltMark.FullName"] = "GR_FULL_NAME", ["BoltMark.AssemblyType"] = "GR_BOLT_ASSEMBLY_TYPE", ["BoltMark.NumberOfBolts"] = "GR_BOLT_NUMBER",
            ["BoltMark.SlotLengthX"] = "GR_LONGHOLE_X", ["BoltMark.SlotLengthY"] = "GR_LONGHOLE_Y", ["BoltMark.SlotHeight"] = "GR_LONGHOLE_MIN",
            ["BoltMark.SlotLength"] = "GR_LONGHOLE_MAX", ["BoltMark.Size"] = "GR_BOLT_SIZE", ["BoltMark.Countersunk"] = "GR_COUNTERSUNK",
            ["BoltMark.GageOfOutstandingLeg"] = "GR_GOL", ["BoltMark.CenterToCenterDistance"] = "GR_GAGE", ["BoltMark.HoleDepth"] = "GR_HOLE_DEPTH",
            ["ConnectionMark.Code"] = "GR_JOINT_CODE", ["ConnectionMark.Name"] = "GR_NAME", ["ConnectionMark.DstvCode"] = "GR_DSTV_CODE",
            ["ConnectionMark.ConnectionNumber"] = "GR_JOINT_NUMBER", ["ConnectionMark.RunningNumber"] = "GR_JOINT_RUNNING_NUMBER",
            ["ConnectionMark.Group"] = "GR_JOINT_GROUP", ["ConnectionMark.ConnectionError"] = "GR_JOINT_ERROR",
            ["ReinforcementMark.Name"] = "GR_OBJECT_NAME", ["ReinforcementMark.Grade"] = "GR_GRADE", ["ReinforcementMark.Diameter"] = "GR_DIAMETER",
            ["ReinforcementMark.Class"] = "GR_CLASS", ["ReinforcementMark.Length"] = "GR_OBJECT_LENGTH", ["ReinforcementMark.Number"] = "GR_NUMBER",
            ["ReinforcementMark.Position"] = "GR_OBJECT_POSITION", ["ReinforcementMark.Shape"] = "GR_SHAPE", ["ReinforcementMark.Weight"] = "GR_WEIGHT",
            ["ReinforcementMark.Cc"] = "GR_CC", ["ReinforcementMark.CcMin"] = "GR_CC_MIN", ["ReinforcementMark.CcMax"] = "GR_CC_MAX",
            ["ReinforcementMark.CcExact"] = "GR_CC_EXACT", ["ReinforcementMark.CcTarget"] = "GR_CC_TARGET",
            ["ReinforcementMeshMark.Name"] = "GR_OBJECT_NAME", ["ReinforcementMeshMark.Grade"] = "GR_GRADE", ["ReinforcementMeshMark.Size"] = "GR_MESH_SIZE",
            ["ReinforcementMeshMark.Class"] = "GR_CLASS", ["ReinforcementMeshMark.MeshLength"] = "GR_REBAR_MESH_LENGTH",
            ["ReinforcementMeshMark.MeshWidth"] = "GR_REBAR_MESH_WIDTH", ["ReinforcementMeshMark.Number"] = "GR_NUMBER",
            ["ReinforcementMeshMark.Position"] = "GR_OBJECT_POSITION", ["ReinforcementMeshMark.Shape"] = "GR_SHAPE", ["ReinforcementMeshMark.Weight"] = "GR_WEIGHT",
            ["ReinforcementMeshMark.CcLongitudinal"] = "GR_CC_LONG", ["ReinforcementMeshMark.CcMinLongitudinal"] = "GR_CC_MIN_LONG",
            ["ReinforcementMeshMark.CcMaxLongitudinal"] = "GR_CC_MAX_LONG", ["ReinforcementMeshMark.CcExactLongitudinal"] = "GR_CC_EXACT_LONG",
            ["ReinforcementMeshMark.CcCrossing"] = "GR_CC_CROSS", ["ReinforcementMeshMark.CcMinCrossing"] = "GR_CC_MIN_CROSS",
            ["ReinforcementMeshMark.CcMaxCrossing"] = "GR_CC_MAX_CROSS", ["ReinforcementMeshMark.CcExactCrossing"] = "GR_CC_EXACT_CROSS",
            ["ReinforcementMeshMark.DiameterLongitudinal"] = "GR_MESH_DIAMETER_LONG", ["ReinforcementMeshMark.DiameterCrossing"] = "GR_MESH_DIAMETER_CROSS",
            ["SurfaceTreatmentMark.Name"] = "GR_OBJECT_NAME", ["SurfaceTreatmentMark.Material"] = "GR_MATERIAL", ["SurfaceTreatmentMark.Class"] = "GR_CLASS",
            ["SurfaceTreatmentMark.Code"] = "GR_SURFACING_CODE", ["SurfaceTreatmentMark.SurfaceTreatmentName"] = "GR_SURFACING_NAME",
            ["EdgeChamferMark.Name"] = "GR_OBJECT_NAME", ["EdgeChamferMark.Length"] = "GR_LENGTH", ["EdgeChamferMark.Dx"] = "GR_CHAMFER_DX",
            ["EdgeChamferMark.Dy"] = "GR_CHAMFER_DY",
            ["PourMark.Material"] = "GR_MATERIAL", ["PourMark.Name"] = "GR_NAME", ["PourMark.PourNumber"] = "GR_POUR_NUMBER",
            ["PourMark.PourType"] = "GR_POUR_TYPE", ["PourMark.ConcreteMixture"] = "GR_POUR_CONCRETE_MIXTURE",
            ["ViewLabel.ViewName"] = "GR_VIEW_NAME", ["ViewLabel.Scale"] = "GR_SCALE", ["ViewLabel.DrawingName"] = "GR_DRAWING_NAME",
            ["ViewLabel.SourceDrawingName"] = "GR_SOURCE_DRAWING_NAME", ["ViewLabel.SourceDrawingNameWhenMoved"] = "GR_SOURCE_DRAWING_NAME_WHEN_MOVED",
            ["SectionMark.SectionName"] = "GR_SECTION_NAME", ["SectionMark.ViewName"] = "GR_VIEW_NAME", ["SectionMark.Scale"] = "GR_SCALE",
            ["SectionMark.DrawingName"] = "GR_DRAWING_NAME", ["SectionMark.SourceDrawingName"] = "GR_SOURCE_DRAWING_NAME",
            ["SectionMark.SourceDrawingNameWhenMoved"] = "GR_SOURCE_DRAWING_NAME_WHEN_MOVED",
            ["DetailMark.DetailName"] = "GR_DETAIL_NAME", ["DetailMark.ViewName"] = "GR_VIEW_NAME", ["DetailMark.Scale"] = "GR_SCALE",
            ["DetailMark.DrawingName"] = "GR_DRAWING_NAME", ["DetailMark.SourceDrawingName"] = "GR_SOURCE_DRAWING_NAME",
            ["DetailMark.SourceDrawingNameWhenMoved"] = "GR_SOURCE_DRAWING_NAME_WHEN_MOVED",
            ["Other.Label"] = "GR_LABEL", ["Other.Layer"] = "GR_LAYER",
        };

        [Fact]
        public void EveryElementOfTheCatalog_HasTheNumberTeklaReads()
        {
            var catalog = typeof(MarkElementTypes).GetNestedTypes()
                .SelectMany(kind => kind.GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => (Name: kind.Name + "." + f.Name, Type: (MarkElementType)f.GetValue(null))))
                .ToList();

            Assert.Equal(ApiElement.Keys.OrderBy(k => k), catalog.Select(c => c.Name).OrderBy(k => k));
            Assert.All(catalog, c => Assert.True(ReadByTekla[ApiElement[c.Name]] == c.Type.SubType, $"{c.Name}: {c.Type.SubType}, Tekla read {ApiElement[c.Name]} as {ReadByTekla[ApiElement[c.Name]]}"));
        }

        [Fact]
        public void ElementAndSizeElement47_AreTheSameType()
        {
            MarkContent content = MarkContent.Parse(Head + Part("<Element name=\"\" value=\"\" subType=\"47\">\n<Format font=\"Arial\" color=\"3\" height=\"2.500\"/>\n</Element>") + "</Mark>\n");

            var element = Assert.IsType<MarkPropertyElement>(Assert.Single(content));
            Assert.Equal(MarkElementTypes.BoltMark.Size, element.Type);
            Assert.False(element.Type.IsSizeElement);
            Assert.Contains("<Element name=\"\" value=\"\" subType=\"47\">", content.ToXml());
        }

        [Fact]
        public void Parse_ReadsTheDamageOfShippedFiles()
        {
            // An attribute without its value (390 shipped values), read as empty and written back with one
            MarkContent empty = MarkContent.Parse(Head + Part("<Element name=\"\" value= subType=\"45\">\n<Format font=\"Arial\" color=\"155\" height=\"3.000\"/>\n</Element>") + "</Mark>\n");
            Assert.Equal(MarkElementTypes.ReinforcementMark.Cc, Assert.IsType<MarkPropertyElement>(Assert.Single(empty)).Type);
            Assert.Contains("value=\"\" subType=\"45\"", empty.ToXml());

            // A raw "<" in a text (18 shipped values), written back raw
            string raw = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><!DOCTYPE Mark><Mark><MarkPart groupid=\"0\"><UserText><< Mark >><Format font=\"Arial\" color=\"155\" height=\"2.500\"/></UserText></MarkPart></Mark>";
            MarkContent text = MarkContent.Parse(raw);
            Assert.Equal("<< Mark >>", Assert.IsType<MarkTextElement>(Assert.Single(text)).Text);
            Assert.Equal(raw, text.ToXml());
        }

        [Fact]
        public void Writer_KeepsTheWayOfTheParsedText()
        {
            // Two line feeds per line in some older files, ">" escaped or not as read
            string doubled = Head.Replace("\n", "\n\n") + "<MarkPart groupid=\"0\">\n\n<UserText>\n\nA > B\n\n<Format font=\"Arial\" color=\"3\" height=\"2.500\"/>\n\n</UserText>\n\n</MarkPart>\n\n</Mark>\n\n";
            Assert.Equal(doubled, MarkContent.Parse(doubled).ToXml());
            string escaped = Head + Part("<UserText>\nA &gt; B\n<Format font=\"Arial\" color=\"3\" height=\"2.500\" hidden=\"0\"/>\n</UserText>") + "</Mark>\n";
            Assert.Equal(escaped, MarkContent.Parse(escaped).ToXml());
        }

        [Fact]
        public void Writer_KeepsElementsWhereTheyWere()
        {
            // Two elements in one MarkPart, and an element outside any
            string xml = Head + "<MarkPart groupid=\"0\">\n<LineFeed/>\n<Space/>\n</MarkPart>\n<Template filename=\"x\"/>\n</Mark>\n";
            MarkContent content = MarkContent.Parse(xml);

            Assert.Equal(3, content.Count);
            Assert.Equal(xml, content.ToXml());
        }

        [Fact]
        public void Frames_AreNumberedWhenWritten_WithoutChangingTheContent()
        {
            var frame = new MarkFrameElement(FrameAroundElementsType.Round, DrawingColor.Red, new MarkContent().Add(MarkElementTypes.PartMark.PartPosition));
            var content = new MarkContent { Layout = MarkContentLayout.Compact }.Add(frame).Add(frame);

            string xml = content.ToXml();

            Assert.Contains("frame_id=\"101\"", xml);
            Assert.Contains("frame_id=\"102\"", xml);
            Assert.Equal(xml, content.ToXml());

            // A frame Tekla would empty (id 0, written by an earlier build) is renumbered and is not the same as a kept one
            MarkContent broken = MarkContent.Parse(Head + Part("<Frame type=\"3\" color=\"160\" frame_id=\"0\">\n<Mark/>\n</Frame>") + "</Mark>\n");
            Assert.Contains("frame_id=\"101\"", broken.ToXml());
            MarkContent kept = MarkContent.Parse(Head + Part("<Frame type=\"3\" color=\"160\" frame_id=\"103\">\n<Mark/>\n</Frame>") + "</Mark>\n");
            Assert.NotEqual(broken, kept);

            var loop = new MarkContent();
            loop.AddFrame(FrameAroundElementsType.Line, DrawingColor.Red, loop);
            Assert.Throws<InvalidOperationException>(() => loop.ToXml());
        }

        [Fact]
        public void Writer_RefusesWhatItCannotWrite()
        {
            Assert.Throws<ArgumentException>(() => new MarkContent().AddText("a\u0002b").ToXml());
            Assert.Throws<ArgumentOutOfRangeException>(() => new MarkFormat { Height = double.NaN });
            Assert.Throws<ArgumentNullException>(() => new MarkContent().Add((MarkElement)null));
            Assert.Throws<ArgumentNullException>(() => new MarkContent().DefaultFormat = null);
            Assert.Throws<ArgumentNullException>(() => new MarkFrameElement(FrameAroundElementsType.Line, DrawingColor.Red, new MarkContent()).Content = null);
            Assert.Throws<ArgumentException>(() => new MarkXmlElement("not xml"));
        }

        [Fact]
        public void Elements_OfAnEmptyRow_AreNull_AndNullEmptiesARow()
        {
            var view = new ViewProperties();
            view.Attributes.LabelMarkA1.Elements = view.Attributes.LabelMarkA1.Elements;
            Assert.False(view.Values.ContainsKey("ViewLabelMarkA1.ContentString"));

            view.Attributes.LabelMarkA1.Elements = new MarkContent().Add(MarkElementTypes.ViewLabel.Scale);
            view.Attributes.LabelMarkA1.Elements = null;
            Assert.Equal(string.Empty, view.GetString("ViewLabelMarkA1.ContentString"));

            // Text that is no mark content is kept: a getter returning null for it must not destroy it
            view.SetString("ViewLabelMarkA1.ContentString", "free text");
            view.Attributes.LabelMarkA1.Elements = view.Attributes.LabelMarkA1.Elements;
            Assert.Equal("free text", view.GetString("ViewLabelMarkA1.ContentString"));
        }

        [Fact]
        public void SetMarkContent_WritesAChosenLayout()
        {
            var view = new ViewProperties();
            view.Attributes.LabelMarkA1.Elements = new MarkContent { Layout = MarkContentLayout.Compact }.Add(MarkElementTypes.ViewLabel.Scale);
            Assert.DoesNotContain("\n", view.GetString("ViewLabelMarkA1.ContentString"));

            MarkContent content = view.Attributes.LabelMarkA1.Elements;
            content.Layout = MarkContentLayout.Lines;
            view.Attributes.LabelMarkA1.Elements = content;

            Assert.Contains("\n", view.GetString("ViewLabelMarkA1.ContentString"));
        }

        [Fact]
        public void Add_ChainsWithElements()
        {
            MarkContent content = new MarkContent().Add(new MarkTextElement("A")).AddSpace().Add(new MarkSeparatorElement(MarkSeparatorKind.LineFeed));

            Assert.Equal(3, content.Count);
            Assert.Equal(MarkSeparatorKind.LineFeed, Assert.IsType<MarkSeparatorElement>(content[2]).Kind);
        }
    }
}
