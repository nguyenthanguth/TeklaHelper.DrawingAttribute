# TeklaHelper.DrawingAttribute

[![CI](https://github.com/nguyenthanguth/TeklaHelper.DrawingAttribute/actions/workflows/ci.yml/badge.svg)](https://github.com/nguyenthanguth/TeklaHelper.DrawingAttribute/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

The Tekla Open API exposes only part of the drawing and view settings as properties. Everything else is reachable only
through **attribute files** — `.gd`, `.ad`, `.vi`, `.vpm`, `.vf`, ... — that Open API objects load by name:

```csharp
var drawing = new GADrawing("PLAN +3.600", "plan_100");   // plan_100.gd + .gd.more + .gd.filter + .gd.npfilter
view.Attributes.LoadAttributes("detail_20");              // detail_20.vi + .vi.filter + .vi.npfilter
```

Those are the files the Open API reads, and the files the Save button of the drawing / view properties dialog writes.
The files of single pages (`.vf`, `.vg`, `.vpm`, `.gdf`, ...) are only for loading by hand in the dialogs.

This library models those files as C# objects. Build the settings in code, save them to the model `attributes` folder,
and pass the file name to the Open API.

## Features

- **Mark and label content**: `MarkContent` builds what a mark or a label row shows (view name, scale, part position,
  user-defined attributes, texts, symbols, frames) instead of the XML Tekla stores in the `ContentString` keys.
- **Drawing object properties**: a class for the file of every property dialog of the drawing editor — texts
  (`.drtxt`), symbols, level marks, associative notes, dimensions (`.dim`, `.rdim`), lines, arcs, polylines, circles,
  rectangles, polygons and clouds, text files, DWG/DXF, images, links, parts, pours, bolts, welds, reinforcement, marks,
  section and detail marks, grids, ... (41 file types, see [Drawing object properties](#drawing-object-properties)).
- **Drawing and view settings**: `GADrawingAttributeFile` (`.gd`), `AssemblyDrawingAttributeFile` (`.ad`),
  `SinglePartDrawingAttributeFile` (`.wd`), `CastUnitDrawingAttributeFile` (`.cud`), `MultiDrawingAttributeFile` (`.md`)
  and `ViewProperties` (`.vi`), each saved with the companion files Tekla writes (`.gd.more`, `.gd.filter`,
  `.vi.filter`, ...).
- **Every setting of the dialogs, typed**: `ViewProperties` has the tree of the View properties dialog — `Attributes`,
  `Filter`, `NeighborPartFilter`, `Protection`, `Dimensioning`, `Marks.PartMark` ... `Marks.PourMark`, `Objects.Part`
  ... `Objects.PourObject` — and each page has the tabs and group boxes of the dialog as nested classes, with a typed
  property for every control: `view.Objects.Part.Content.HiddenLines.HiddenLines = true`. The pages are generated from
  the Tekla Structures dialog definition (currently Tekla 2026) and checked against files saved from the dialogs; every key of a page has
  a property (keys the dialog does not show are in the page's `Advanced` group). Drawings add the pages of the drawing
  properties: `Views`, `SectionView`, `DetailView`, `Dimension`, `Dimensioning` / `AssemblyDimensioning`, `Layout`.
- **One page class for every host**: the object and mark pages have the same keys in views and in all drawing types,
  under different prefixes (`vp.` in a view, `dp.` in a drawing, `dgrid.` / `grid.` for the grid, ...), so
  `drawing.Objects.Part` and `view.Objects.Part` are the same `PartObject` class. Copy settings with `CopyFrom`.
- **Page files**: export a page the way its Save as in a Tekla dialog does
  (`view.Objects.Grid.ExportTo(dir, "red", GridObject.ViewFileExtension)`) and import such files back (`ImportFrom`).
- **Filters**: `FilterFile` reads and writes view filters (`.vf`, `.vi.filter`), neighbour part filters, drawing and
  model filters, from rows or from an expression tree built with `FilterProperties` — the same catalog as
  `Tekla.Structures.Filtering.Categories`.
- **Exact format**: an unchanged file is written back byte for byte. Checked against every attribute file (about 2000)
  and filter file (3378) of the Tekla Structures 2025 environments, and filter output is compared with
  `Tekla.Structures.Filtering` itself, byte for byte.
- Every key stays reachable, even without a typed property: `GetDouble("view_scale")`, `SetInt32("dpm.text_colour", 160)`.
- No reference to the Tekla Open API: targets `net48`, `net8.0` and `netstandard2.0`, works in plugins, extensions and standalone tools of any
  Tekla version, and on .NET 8+ (where `Tekla.Structures.Filtering` itself fails, because it needs `BinaryFormatter`).

## Quick start

```csharp
using TeklaHelper.DrawingAttribute;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;

string attributes = Path.Combine(new Model().GetInfo().ModelPath, "attributes");

// GA drawing: start from a complete template, change it, save it under a new name
var ga = AttributeFile.FromFile<GADrawingAttributeFile>(Path.Combine(attributes, "standard.gd"));
ga.Title1 = "PLAN +3.600";
ga.Views.Scale = 100;
ga.Objects.Grid.Text.Height = 3.5;
ga.Objects.Grid.GridLine.Color = DrawingColor.Red;
ga.Marks.PartMark.Content.Font.Height = 3;
ga.UserAttributes.DrawnBy = "NT";
ga.SaveTo(attributes, "plan_100");            // plan_100.gd + plan_100.gd.more

var drawing = new GADrawing("PLAN +3.600", "plan_100");
drawing.Insert();

// View: the pages, tabs and group boxes of the View properties dialog
var vi = AttributeFile.FromFile<ViewProperties>(Path.Combine(attributes, "standard.vi"));
vi.Attributes.Attributes1.View.Scale = 20;
vi.Attributes.Attributes2.CutParts.Shortening.CutParts = CutParts.BothDirections;
vi.Objects.Grid.Grid = Grid.NotVisible;
vi.Objects.Part.Content.Representation.Representation = Representation.Exact;
vi.Objects.Part.Content.HiddenLines.HiddenLines = true;
vi.Objects.Part.Appearance.VisibleLines.Color = DrawingColor.Blue;
vi.Filter.SetExpression(new FilterGroup()
    .Add(FilterProperties.Part.Name.IsEqual("BEAM"), FilterLogic.Or)
    .Add(FilterProperties.Part.Name.IsEqual("COLUMN")));
vi.SaveTo(attributes, "detail_20");           // detail_20.vi + detail_20.vi.filter

view.Attributes.LoadAttributes("detail_20");
view.Modify();
```

> [!TIP]
> A file created with `new` contains only the keys that were set. Tekla gives every key a file lacks the value of the
> `standard` file of its environment, so the same file draws differently in another environment. Start from
> `CreateDefault()` (below), or load a complete file as a template as in the GA example above.

## Default settings

`CreateDefault()` makes a file with every key the Tekla dialog saves, so that Tekla takes nothing from the environment
(one exception below):

```csharp
ViewProperties view = ViewProperties.CreateDefault();      // every key of a .vi saved from the dialog
view.Attributes.Attributes1.View.Scale = 50;
view.SaveTo(attributes, "plan_50");

GADrawingAttributeFile ga = GADrawingAttributeFile.CreateDefault();   // also .md, .ad, .wd, .cud
view.Objects.Part.SetDefaults();                            // one page to the Tekla-derived defaults (without the setup below)
```

The values: for each key of a page, what its `SetDefaults()` writes — the value most files shipped with Tekla hold (at
least three files, never an empty font or a zero text height), else the dialog's own default, else the value of a file
saved from the dialog. The other keys take the
most common shipped value, else the value of the dialog file; keys that name a layout or an attribute file use names
every environment has (layouts `ga`, `multi`, `assembly`, `single`, `cu`; attribute files `standard`), titles and
descriptions are empty. The files use code page 1252: set `CodePage` (932, 949, ...) before saving Japanese or Korean
text. On top, a setup chosen by this library so that views show their objects to the Open API:
parts exact with hidden lines and the center line of main beams; part, bolt and grid lines black;
bolts, welds, surface treatments, the grid and connected neighbor parts (outline) shown; part marks with the assembly
position, no bolt marks, no filter; view labels black; for view properties also 1:20, fitted to the parts, not shortened. The check boxes (`*_en`) of the pages are ticked.

The views Tekla creates in assembly, single-part and cast unit drawings follow the View creation table. `CreateDefault()`
names no view properties file there, so these views use the drawing file's settings, except a 3D view, which then uses
the environment's `standard.vi`. To control every view, name a view file made with `ViewProperties.CreateDefault()`:
`drawing.Views.SetViewAttributeFile("my_views")`.

The [samples](samples) folder has a console application with ten commented examples: view properties page by page,
filters, every drawing type, page files, and two examples that create a GA drawing with filtered views and change the
views of an open drawing in a running Tekla Structures.

## View properties

`ViewProperties` follows the tree of the Tekla View properties dialog. Each page is a section over the keys of one page
in the .vi file, with the tabs and group boxes of the dialog as nested classes:

```csharp
var view = new ViewProperties();
view.Objects.Part.Content.Representation.Representation = Representation.Exact;   // Part > Content > Representation
view.Objects.Part.Content.Representation.PartialProfile.Length = 100;
view.Objects.Part.Appearance.HiddenLines.Type = DrawingLineType.Dashed;            // Part > Appearance > Hidden lines
view.Objects.Part.Fill.PartFaces.Type = "ANSI31";                                   // Part > Fill > Part faces
view.Marks.PartMark.General.FrameAroundMark.Type = FrameAroundMarkType.Rectangular;
view.Marks.PartMark.General.Placing.Place.Quarter.Quarter1 = true;                  // the "Place..." sub-dialog
view.Attributes.LabelMarkA1.Content.Font.Height = 5;                                // view label, row A1
view.Objects.Part.SetAllEnabled(true);                                              // the *_en check boxes
```

| Page | Class | Keys in .vi | Page file (dialogs only) |
| --- | --- | --- | --- |
| `Attributes` | `ViewAttributes` (+ `LabelMarkA1` ... `A5`: `ViewLabelMark`) | top level, `ViewLabelMarkA1.` ... | — |
| `Filter`, `NeighborPartFilter` | `FilterSection` | `vfilter.`, `vnp_filter.` + rows in `.vi.filter`, `.vi.npfilter` | `.vf`, `.vnf` |
| `Protection`, `Dimensioning` | `ProtectionPage`, `ViewDimensioning` | `Protection.`, `ViewDimensioning.` | — |
| `Marks.PartMark`, `BoltMark`, `NeighborPartMark`, `SurfaceTreatmentMark`, `ConnectionMark` | `PartMark`, ... | `vpm.`, `vsm.`, `vnpm.`, `vsurfm.`, `vjm.` | `.vpm`, `.vsm`, `.vnpm`, `.vsurfm`, `.vjm` |
| `Marks.WeldMark`, `ReinforcementMark`, `NeighborReinforcementMark`, `PourMark` | `WeldMark`, ... | `vwel.`, `vrm.`, `NeighborRebarMark.`, `PourMark.` | `.vw`, `.vrm`, `.vnrm` |
| `Objects.Part`, `Bolt`, `NeighborPart`, `SurfaceTreatment`, `Weld` | `PartObject`, ... | `vp.`, `vs.`, `vnp.`, `vsurf.`, `ViewWeld.` | `.vp`, `.vs`, `.vnp`, `.vsurf`, `.vwo` |
| `Objects.Reinforcement`, `ReferenceObject`, `Grid`, `PourBreak`, `NeighborReinforcement`, `PourObject` | `ReinforcementObject`, ... | `vrebar.`, `vref.`, `vg.`, `PourBreaks.`, `NeighborRebar.`, `PourObjects.` | `.vr`, `.vrmp`, `.vg`, `.vpbr`, `.vnr`, `.vpo` |

Each page class has its prefixes and page file extensions as constants (`PartObject.ViewPrefix`,
`PartObject.GADrawingPrefix`, `PartObject.ViewFileExtension`, ...). A missing key reads as the type's default (0, false,
empty); the enum members and labels come from the dialog.

## Drawings

Drawings have the same `Marks`, `Objects` and `Protection` pages under their own prefixes, plus the pages of the drawing
properties, the titles and the user-defined attributes:

| Page | GA / multi (`.gd`, `.md`) | Assembly / single-part / cast unit (`.ad`, `.wd`, `.cud`) |
| --- | --- | --- |
| `Objects.Grid` | `dgrid.` | `grid.` |
| `Objects.Part` / `Objects.NeighborPart` | `dp.` / `dnpart.` | `dp.` / `dnp.` |
| `Objects.Bolt`, `Weld`, `Reinforcement`, `SurfaceTreatment` | `ds.`, `DrawingWeld.`, `drebar.`, `dsurf.` | same |
| `Objects.ReferenceObject` | `drefobj.` | — (null) |
| `Marks.PartMark`, `NeighborPartMark`, `BoltMark`, `ConnectionMark`, `WeldMark` | `dpm.`, `dnpm.`, `dsm.`, `djm.`, `dwel.` | same |
| `Marks.ReinforcementMark`, `SurfaceTreatmentMark` | `drebar_mark.`, `dsurf_mark.` | `drmark.`, `dsurfmark.` |
| `Protection` | `dpro.` | `dpro.` |
| `Layout` | `dl.` | `dl.` |
| `Views` (View page, view creation; label rows `LabelMarkA1` ... `A5`) | `dview.` | `dv.` |
| `SectionView` (section mark rows `SectionMarkA1` ..., label rows `LabelMarkA1` ...) | `dc.` | `dc.` |
| `DetailView` (detail mark rows `DetailMarkA1` ..., label rows `LabelMarkA1` ...) | `DetailLabel.` | `DetailLabel.` |
| `Dimension` | `dd.` | `dd.` |
| `Dimensioning` / `AssemblyDimensioning` | `Dimensioning.` / — (null) | — (null) / `dcd.` |

The drawing pages are generated from the drawing dialogs of Tekla 2026 like the view pages: the GA dialogs (View,
Detail view, Dimension, Dimensioning), the assembly drawing dialog (Section view panel, Dimensioning) and, for the keys
the newest dialog does not show, older dialogs with the same keys, whose controls sit in the tabs of that dialog. Checked
in Tekla 2025: an assembly drawing created from a file with changed values showed them (view extension for neighbor
parts, openings symbol, datum level, skew part shortening; dimension type, colour and text height; cutting line colour,
section mark symbol colour and arrow length; fewer automatic dimensions). `CreateDefault()` writes the value most files
of the same drawing type shipped with Tekla hold for these pages; `SetDefaults()` of a page writes the value most shipped
files hold (GA and assembly files together), else the dialog's default, the same for every drawing type.

```csharp
AssemblyDrawingAttributeFile assemblyDrawing = AssemblyDrawingAttributeFile.CreateDefault();

// Use the part marks of a view in an assembly drawing
assemblyDrawing.Marks.PartMark.CopyFrom(view.Marks.PartMark);

// Save the grid of a view as a page file (for the dialog), and load one into a drawing
view.Objects.Grid.ExportTo(attributes, "red_grid", GridObject.ViewFileExtension);    // red_grid.vg
ga.Objects.Grid.ImportFrom(Path.Combine(attributes, "red_grid.vg"));

// The views Tekla creates in assembly, single-part and cast unit drawings follow the View creation table: which views
// (Off / Auto / On) and the view properties (.vi) each one uses. Name a .vi made with ViewProperties.CreateDefault()
ViewProperties.CreateDefault().SaveTo(attributes, "assembly_views");
assemblyDrawing.Views.SetViewAttributeFile("assembly_views");     // every view of the table and the 3D view
assemblyDrawing.Views.CreateTopView = ViewCreation.On;         // the table row and the cre_top_view switch

// The pages of the drawing properties
assemblyDrawing.Dimension.General.DimensionTypes.Straight = DimensionType.Absolute;
assemblyDrawing.Dimension.Appearance.Text.Height = 3.5;
assemblyDrawing.SectionView.CuttingLine.Properties.Color = DrawingColor.Blue;
assemblyDrawing.AssemblyDimensioning.PartDimensions.MainPartOverallDimensions = MainPartOverallDimensions.All;
ga.Views.Attributes.ViewSize.ViewExtensionForNeighborParts = 500;
ga.Dimensioning.Grid.DimensionCreation.GridLineDimensions = true;

// A key no dialog shows keeps its name
ga.Layout.SetInt32("placing_direction", 0);
```

## Dimension properties

`DimensionAttributeFile` is the `.dim` file the Dimension properties dialog of the drawing editor saves, which a
dimension loads by name (`new StraightDimensionSet.StraightDimensionSetAttributes("absolute")` in the Open API):

```csharp
DimensionAttributeFile dimension = DimensionAttributeFile.CreateDefault();
dimension.Properties.General.DimensionTypes.Straight = StraightDimensionType.Absolute;
dimension.Properties.General.DimensionFormat.Precision = DimensionPrecision.OnePerEight;
dimension.Properties.Appearance.Text.Height = 3.5;
dimension.Properties.Marks.Exaggeration.Exaggeration = Exaggeration.Specified;
dimension.PrefixMark.Font.Height = 2.5;                        // PrefixMark.FontAttr.height
dimension.SaveTo(attributes, "absolute");                      // absolute.dim
```

`Properties` follows the dialog (tabs General, Appearance, Marks, Tags, and the Place dialog); the marks
(`PrefixMark`, `PostfixMark`, `LeftUpperTagMark` ... `RightLowerTagMark`) are `DimensionMark` pages: content, frame,
font, units and rotation, plus the keys every mark has. Every key of the 177 `.dim` files of the Tekla environments and
models on the maintainer's machine has a property. Checked in Tekla 2025: the attributes read from a file the library
wrote, and a dimension created with them, showed the type, precision, format, short dimension placing, text height and
frame, colour and exaggeration of the file.

## Drawing object properties

Each property dialog of the drawing editor saves its own file, which the dialog's Load button reads and, for most
types, an attribute class of the Open API reads by name. Every type has a class with the dialog's tabs and group boxes,
`CreateDefault()` and the legacy dialog prefix handled on load; every key of the files saved from the Tekla 2025 dialogs
and of the shipped files has a property (except four rebar dimension mark keys that older versions wrote into mark
files and no dialog has):

```csharp
TextAttributeFile text = TextAttributeFile.CreateDefault();
text.Properties.Appearance.Text.Height = 3.5;                  // height
text.Properties.Appearance.Text.Color = DrawingColor.Blue;     // text_colour
text.SaveTo(attributes, "blue_text");                          // blue_text.drtxt
// Open API: new Text.TextAttributes("blue_text")

LineAttributeFile line = LineAttributeFile.CreateDefault();
line.Properties.Arrow.Position = ArrowPosition.End;            // arrow 2 = Arrowhead.ArrowPosition End
line.SaveTo(attributes, "arrow_line");                         // arrow_line.gln

// The mark, bolt, weld, grid, pour break and reference object files hold the keys of a page of the view properties,
// so they use that page's class
PartMarkAttributeFile mark = PartMarkAttributeFile.CreateDefault();
mark.Properties.CopyFrom(view.Marks.PartMark);
mark.SaveTo(attributes, "view_marks");                         // view_marks.pm
```

| Class | File | Dialog | Read by the Open API with |
| --- | --- | --- | --- |
| `TextAttributeFile` | `.drtxt` | Text (also with leader line) | `Text.TextAttributes(name)` |
| `SymbolAttributeFile` | `.sbl` | Symbol | `Symbol.SymbolAttributes(name)` |
| `LevelMarkAttributeFile` | `.lev` | Level mark | `LevelMark.LevelMarkAttributes(name)` |
| `RevisionMarkAttributeFile` | `.rev` | Revision mark | — |
| `AssociativeNoteAttributeFile` | `.note` | Associative note | `Mark.MarkAttributes(modelObject, name)` |
| `DimensionAttributeFile` | `.dim` | Dimension | `StraightDimensionSet.StraightDimensionSetAttributes(name)` and the other dimension attributes |
| `ReinforcementDimensionAttributeFile` | `.rdim` | Reinforcement dimension (the keys of `.dim`) | — |
| `LineAttributeFile`, `ArcAttributeFile`, `PolylineAttributeFile` | `.gln`, `.gar`, `.gpl` | Line, Arc, Polyline | `Line.LineAttributes(name)`, `Arc.ArcAttributes`, `Polyline.PolylineAttributes` |
| `CircleAttributeFile`, `RectangleAttributeFile`, `PolygonAttributeFile` | `.gci`, `.grt`, `.gpg` | Circle, Rectangle, Polygon | `Circle.CircleAttributes(name)`, `Rectangle.RectangleAttributes`, `Polygon.PolygonAttributes` and `Cloud.CloudAttributes` (both `.gpg`) |
| `TextFileAttributeFile` | `.fas` | Text file (ASCII) | — |
| `DwgAttributeFile` | `.fdg` | DWG/DXF | `EmbeddedObjectAttributes(name)` (DWG/DXF objects and images) |
| `ImageAttributeFile` | `.idg` | Image | — (images read `.fdg`) |
| `HyperlinkAttributeFile` | `.fhl` | Hyperlink | `LinkAttributes(name)` |
| `LinkedDrawingAttributeFile` | `.ldr` | Linked drawing | — |
| `PartAttributeFile` | `.dprt` | Part | `Part.PartAttributes(name)` |
| `SurfaceTreatmentAttributeFile` | `.dsrf` | Surface treatment | `Surfacing.SurfacingAttributes(name)` |
| `PourAttributeFile` | `.po` | Pour object | `PourObject.PourAttributes(name)` |
| `PourBreakAttributeFile` | `.pbr` | Pour break (page class `PourBreakObject`) | `PourBreak.PourBreakAttributes(name)` |
| `BoltAttributeFile` | `.sc` | Bolt (`BoltObject`) | `Bolt.BoltAttributes(name)` |
| `WeldAttributeFile` | `.welo` | Weld (`WeldObject`) | `Weld.WeldAttributes(name)` |
| `WeldMarkAttributeFile` | `.wel` | Weld mark of a model weld (`WeldMark`) | `WeldMark.WeldMarkAttributes(name)` |
| `DrawingWeldMarkAttributeFile` | `.wls` | Weld mark drawn in the drawing | — |
| `EdgeChamferAttributeFile` | `.dcha` | Edge chamfer | `EdgeChamfer.EdgeChamferAttributes(name)` |
| `ReinforcementAttributeFile` | `.drbr` | Reinforcement | every `ReinforcementBase.*Attributes(name)`, meshes included |
| `ReinforcementMeshAttributeFile` | `.drmsh` | Reinforcement mesh | — (meshes read `.drbr`) |
| `ReferenceObjectAttributeFile` | `.drmp` | Reference object (`ReferenceObject`) | — |
| `GridAttributeFile`, `GridLineAttributeFile` | `.dgr`, `.grl` | Grid, Grid line (`GridObject`) | `Grid.GridAttributes(name)`, `GridLine.GridLineAttributes(name)` |
| `SectionMarkAttributeFile` | `.cs` | Section mark (label rows `SectionMarkA1` ..., `LabelMarkA1` ...) | `SectionMarkBase.SectionMarkAttributes(name)` |
| `DetailMarkAttributeFile` | `.detail` | Detail mark (label rows `DetailMarkA1` ..., `LabelMarkA1` ...) | `DetailMark.DetailMarkAttributes(name)` |
| `PartMarkAttributeFile`, `BoltMarkAttributeFile`, `ConnectionMarkAttributeFile`, `ReinforcementMarkAttributeFile`, `SurfaceTreatmentMarkAttributeFile`, `PourMarkAttributeFile` | `.pm`, `.sm`, `.jm`, `.rm`, `.surfm`, `.pom` | Marks (`PartMark`, `BoltMark`, ...) | — |
| `GADimensionSetupAttributeFile` | `.ga_dim_setup` | Semi-automatic GA dimensioning | — |

The extensions were checked in a running Tekla 2025: a file each type, written by `CreateDefault()` with one value
changed, was loaded with every attribute class of the Open API that takes a file name, and the class showed the value
(36 checks). "—" means no attribute class reads that file by name; the dialog's Load button does. `.dim` is described
under [Dimension properties](#dimension-properties).

## Mark and label content

What a mark or a label row shows — the "Elements in mark" of a mark dialog — is stored as XML in the `ContentString`
keys (and a few others, such as `aLabelContent`). `MarkContent` builds it from elements; the label rows (`LabelMarkA1` ..., `SectionMarkA1` ..., `DetailMarkA1` ...)
have it as `Content.Elements` (the Content tab of their dialog), and any section reads and writes it with
`GetMarkContent` / `SetMarkContent`:

```csharp
vi.Attributes.LabelMarkA1.Content.Elements = new MarkContent()
    .Add(MarkElementTypes.ViewLabel.ViewName, new MarkFormat { Font = "Tahoma", Color = DrawingColor.Green, Height = 5 })
    .AddText(" ")
    .Add(MarkElementTypes.ViewLabel.Scale)
    .AddLineFeed()
    .AddUserAttribute("USER_FIELD_1");

var mark = PartMarkAttributeFile.CreateDefault();
mark.Elements = new MarkContent()                                        // the one table of the part mark dialog
    .Add(MarkElementTypes.PartMark.PartPosition)
    .AddFrame(FrameAroundElementsType.Rectangular, DrawingColor.Red, new MarkContent().Add(MarkElementTypes.PartMark.Profile));

MarkContent read = vi.Attributes.LabelMarkA1.Content.Elements;          // the elements of an existing file
```

`Elements` and `GetMarkContent` return a copy: change it, then assign it back (`Content.Elements = read`).

- Elements: the properties of each kind of mark (`MarkElementTypes.PartMark`, `BoltMark`, `ConnectionMark`,
  `ReinforcementMark`, `ReinforcementMeshMark`, `SurfaceTreatmentMark`, `EdgeChamferMark`, `PourMark`, `ViewLabel`,
  `SectionMark`, `DetailMark`), user-defined attributes (with unit and format), texts, line feeds, spaces, backspaces,
  symbols (`xsteel@85`), frames and report templates; other XML (merged mark settings of reinforcement marks) is kept.
- Tekla identifies a property element by its number (`subType`) alone. The numbers were read back through the Open API
  of Tekla 2025 for every value from 1 to 110; they equal the Open API's own enum (`PropertyTypes`) for 1 to 53 and 57 to
  70 only (scale is 79 in the file, 80 in `PropertyTypes`; mesh size 71 and 73).
- Checked in Tekla 2025: content written by the library in a view label, a part mark (`.note`), a section mark and a
  detail mark was read by the Open API with every element, format, frame, symbol, unit and template.
- Content Tekla wrote is written back as it was: every content of the environments and models on the maintainer's
  machine (about 16 800), except 25 values that mix two encodings (no text in either). Two kinds of damage in shipped
  files are read as Tekla meant them: an attribute without its value (`value= subType="45"`, 390 values, read as empty)
  and a raw `<` in a text (`<< Mark >>`, 18 values, kept raw). Assigning content its own value changes no byte;
  assigning null empties the row.
- Each kind of object shows its own table of a mark page, by the "Mark content of" menu of the dialog (checked in Tekla
  2025: a view loaded with a different text in each table, its marks read through the Open API; model objects added
  for the probe and removed afterwards):

  The properties are where the dialog shows the table: in the Content tab (`page.Content.MainPartElements`), the merged
  mark in the Merging tab (`page.Merging.MergedMarkElements`), on the page itself for the dimension marks, which have no
  tabs:

  | Section | `ContentString1` | `ContentString2` | `ContentString3` | `ContentString4` |
  | --- | --- | --- | --- | --- |
  | `PartMark.Content` | `MainPartElements` | `SecondaryPartElements` | `SubAssemblyMainPartElements` | `SubAssemblySecondaryPartElements` |
  | `BoltMark.Content` | `SiteBoltElements` | `WorkshopBoltElements` | | |
  | `ReinforcementMark.Content` / `.Merging` | `SingleElements` | `GroupElements` | `Merging.MergedMarkElements` (not seen read) | `MeshElements` |
  | `NeighborPartMark.Content` | `MainPartElements` | `SecondaryPartElements` (not seen read) | | |
  | `NeighborReinforcementMark.Content` / `.Merging` | `SingleElements` | `GroupElements` | `Merging.MergedMarkElements` (not seen read) | `MeshElements` |
  | `ConnectionMark.Content`, `SurfaceTreatmentMark.Content` (one table) | `Elements` | | | |
  | `AssociativeNoteProperties.Content` (`.note`) | `PartElements` | `BoltElements` | `SurfaceTreatmentElements` (not checked) | `ReinforcementElements` |
  | `DimensionMark` (the marks of `.dim`; not probed, from the menu and the files) | `PartElements` | `BoltElements` | `SurfaceTreatmentElements` | `ReinforcementElements` |

  The associative note has a table per kind of object (every part reads `ContentString1`, every reinforcement
  `ContentString4`, an edge chamfer `Content.EdgeChamferElements`, a reference model `Content.ReferenceObjectElements`);
  the mark pages a table per kind of part, bolt or bar. Every mark page also has `Advanced.PourElements`,
  `Advanced.PourBreakElements`, `Advanced.EdgeChamferElements` and `Advanced.ReferenceObjectElements` (keys the dialog
  does not show, named after the key); the pour mark's own table, which its dialog edits, is `PourMark.Content.PourElements`. Like `Elements`, each
  property returns a copy. Keys with such a property have no string property; `GetString` / `SetString` with the key
  give the raw XML. Content without elements is written as an empty text, as the dialogs store an empty table.
- The mark dialogs of the drawing editor have **one** table, not one per kind of object: the file's `Elements` reads
  and writes it the way the dialog does (checked in Tekla 2025: a file with a different text in each key, loaded and
  saved again by the dialog; Modify of the part mark dialog gave every selected part mark that content):

  | File | `Elements` reads | and writes |
  | --- | --- | --- |
  | `PartMarkAttributeFile` (`.pm`), `BoltMarkAttributeFile` (`.sm`) | `ContentString1` | `ContentString1` ... `4` |
  | `ConnectionMarkAttributeFile` (`.jm`), `ReinforcementMarkAttributeFile` (`.rm`), `SurfaceTreatmentMarkAttributeFile` (`.surfm`) | `ContentString1` | `ContentString1`, `2`, `4` (`3` is left; for reinforcement marks it is the merged mark) |
  | `PourMarkAttributeFile` (`.pom`) | `PourContentString` | `PourContentString`, `ContentString1` |

  ```csharp
  var pm = PartMarkAttributeFile.CreateDefault();
  pm.Elements = new MarkContent().Add(MarkElementTypes.PartMark.PartPosition);   // what the part mark dialog shows
  ```

## Filters

```csharp
// Name = BEAM AND (Profile begins with HEA OR Phase >= 2)
var expression = new FilterGroup()
    .Add(FilterProperties.Part.Name.IsEqual("BEAM"), FilterLogic.And)
    .Add(new FilterGroup()
        .Add(FilterProperties.Part.Profile.BeginsWith("HEA"), FilterLogic.Or)
        .Add(FilterProperties.Part.Phase.GreaterOrEqual(2)));

FilterFile.FromExpression(FilterFileExtensions.ViewFilter, expression).SaveTo(attributes, "beams");   // beams.vf

// Read a filter saved from Tekla, change a row, save it again (unchanged rows stay byte for byte)
FilterFile filter = FilterFile.FromFile(Path.Combine(attributes, "standard.vf"));
filter.Rows[0].Value = "COLUMN";
filter.Save(Path.Combine(attributes, "columns.vf"));

// User-defined attributes
FilterProperties.Assembly.CustomString("USER_FIELD_1").Contains("A");

// Object type: every object but reinforcing bars (the row "Object type  Does not equal  Reinforcing bar")
FilterProperties.Object.Type.IsNotEqual(FilterObjectType.ReinforcingBar);
FilterProperties.Object.Type.IsEqual(FilterObjectType.Part, FilterObjectType.BoltGroup);   // parts or bolts
```

Tekla accepts at most 1000 rows and 1023 bytes per value (the buffer of its filter dialog); a longer value or an
expression with more rows throws instead of writing a filter Tekla cannot use.

How Tekla 2025 applies these files. Each point was checked in a running Tekla, with selection filters and with the
view filters of drawing views, against the properties of every object in the model:

- Text is compared case-sensitively. `*` and `?` are wildcards (`B*` equals `BEAM`), and several values separated by
  spaces are alternatives (`BEAM COLUMN`).
- Numbers are read in single precision, about seven digits: `LENGTH >= 3992.925` leaves out a part 3992.92504 long.
- Within a group And comes before Or: `A Or B And C` means `A Or (B And C)`. Nest a `FilterGroup` for `(A Or B) And C`.
- A row of another category does not filter an object: `Part.Name == BEAM` keeps every bolt, and
  `Part.Name == BEAM Or Bolt.Size == 20` keeps every part. Add an object type row,
  `FilterProperties.Object.Type.IsEqual(FilterObjectType.Part)`, to keep only parts.
- A numeric user-defined attribute an object does not have passes `>` but not `<`. A text attribute it does not have
  equals `Empty`.
- Disabled rows (`Enabled = false`) are skipped. A filter whose rows are all disabled keeps everything.
- Some values are codes: `Part.PrimaryPart` is `1` / `0`, `Bolt.SiteWorkshop` `0` for site bolts, `Assembly.Type`
  `0` precast, `1` cast in place, `2` steel. The dialog writes the object type as a label key (`albl_Reinforcement_bar`):
  `FilterObjectType` lists the twelve Tekla evaluates, `FilterObjectTypes.TryParse` reads them back. Tekla also takes
  the numbers of `TeklaStructuresDatabaseTypeEnum` (`Object.Type.IsEqual(2)` selects parts), but the dialog cannot
  show them.
- `ModelObjectSelector.GetObjectsByFilterName` and `Operation.ObjectMatchesToFilter` read `.SObjGrp` files only, not
  `.VObjGrp`. They read the file again on every call, so a rewritten filter needs no new name (unlike attribute files; not checked
  with the filter cache of the Open API on).

`FilterFile.Rows` is the file as written: each row with its parentheses, check box, tokens and And/Or. `SetExpression`
writes a tree the way `Tekla.Structures.Filtering.Filter.CreateFile` does, and `ToExpression` reads rows back into a
tree.

## The file formats

```text
encoding 1252
in_file "standard"
view_scale 20.000000
StyleName "standard"
vg.line_colour 160
```

- Attribute files: one `key value` pair per line, CRLF. Strings are quoted, inner quotes are **not** escaped, line breaks
  in a string are written as `\u0001`. Doubles have six decimals; check boxes are 0 / 1. The first line names the ANSI
  code page; mark contents (XML declaring UTF-8) are UTF-8 whatever the code page.
- `.more` files: `<object type>.<attribute> value`, no encoding line. Dates are seconds since 1970-01-01.
- Filter files: a `TITLE_OBJECT_GROUP` block with one `SECTION_OBJECT_GROUP` of ten tokens per row; spaces in values are
  written as `\u0002`.
- Older files without an encoding line, keys prefixed with the dialog name (`view_dial.view_scale`), are read too and
  saved in the current format.

## Roadmap

| File | Status |
| --- | --- |
| `.vi` (+ `.vi.filter`, `.vi.npfilter`) | Every page of the View properties dialog, every key typed; `CreateDefault()` |
| `.gd`, `.ad`, `.wd`, `.cud`, `.md` and their `.more` / filter files | Every page typed: marks, objects, protection, layout, view (with the View creation table), section view, detail view, dimension, dimensioning; titles and user-defined attributes; `CreateDefault()`. Not typed: check boxes (`SetAllEnabled`), page file names, `dl.placing_direction` and the fixed fields of the filter pages, which no dialog shows |
| Page files (`.vg`, `.vpm`, `.gdgr`, ...) | `ExportTo` / `ImportFrom` on pages |
| Filters (`.vf`, `.vnf`, `.vi.filter`, `.gd.filter`, `.gdf`, `.SObjGrp`, ...) | Supported |
| Drawing object files (`.dim`, `.drtxt`, `.note`, `.lev`, `.sbl`, `.rdim`, marks, graphics, links, parts, welds, reinforcement, section and detail marks, ...) | A class each (41 types): every key typed; `CreateDefault()` |
| Model object files | `GenericAttributeFile` |
| Object level settings (`.copt`, `TITLE_CLASSIFIER`) | Not supported |

## Requirements

- .NET Framework 4.8 (Tekla Open API applications), .NET 8+, or any other implementation of `netstandard2.0`.
- No dependency on .NET Framework 4.8 and .NET 8, which have the code pages of the attribute files built in; on
  `netstandard2.0` they come from `System.Text.Encoding.CodePages`, registered automatically. (The package does not
  embed that DLL: Tekla 2025 ships its own version 7.0, and a second copy in a plug-in would load side by side.)

## Building from source

```shell
git clone https://github.com/nguyenthanguth/TeklaHelper.DrawingAttribute.git
cd TeklaHelper.DrawingAttribute
dotnet build TeklaHelper.DrawingAttribute.slnx -c Release
dotnet test TeklaHelper.DrawingAttribute.slnx -c Release
```

The tests run on .NET Framework 4.8 and .NET 10. Two optional groups need Tekla data:

- With Tekla Structures 2025 installed, the net48 tests also compare the filter catalog and every filter file the library
  writes with `Tekla.Structures.Filtering` (the Tekla folder is read from the registry, or pass
  `-p:TeklaBinDirectory=<Tekla bin folder>`).
- `TEKLA_ATTRIBUTE_SAMPLES` makes the tests read and write back every attribute and filter file under a folder:

  ```powershell
  $env:TEKLA_ATTRIBUTE_SAMPLES = "C:\TeklaStructures\2025.0\Environments"
  dotnet test TeklaHelper.DrawingAttribute.slnx -c Release --filter "FullyQualifiedName~SampleFileTests"
  ```

- `TEKLA_VIEW_DIALOG_SAMPLES` names a folder with a view saved from the dialog together with its page files under the
  same name; the tests check that each page file matches its page in the .vi, and that every key of the pages has a
  property.

[docs/how-it-was-built.md](docs/how-it-was-built.md) tells how the formats and the dialog structure were worked out and
checked.

The page classes in `src/TeklaHelper.DrawingAttribute/Pages/Generated` and `src/TeklaHelper.DrawingAttribute/DrawingObjects/Generated`
(the drawing object files) are generated by `tools/DialogClassGenerator` from
the decoded dialog definitions in `tools/DialogClassGenerator/Dialogs` (see CLAUDE.md); do not edit them by hand.

### Packing

```shell
dotnet pack src/TeklaHelper.DrawingAttribute/TeklaHelper.DrawingAttribute.csproj -c Release -o artifacts
```

## License

[MIT](LICENSE)

Tekla and Tekla Structures are trademarks of Trimble Inc. This project is not affiliated with or endorsed by Trimble.
