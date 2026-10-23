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

- **Drawing and view settings**: `GADrawingAttributeFile` (`.gd`), `AssemblyDrawingAttributeFile` (`.ad`),
  `SinglePartDrawingAttributeFile` (`.wd`), `CastUnitDrawingAttributeFile` (`.cud`), `MultiDrawingAttributeFile` (`.md`)
  and `ViewProperties` (`.vi`), each saved with the companion files Tekla writes (`.gd.more`, `.gd.filter`,
  `.vi.filter`, ...).
- **Every setting of the dialogs, typed**: `ViewProperties` has the tree of the View properties dialog — `Attributes`,
  `Filter`, `NeighborPartFilter`, `Protection`, `Dimensioning`, `Marks.PartMark` ... `Marks.PourMark`, `Objects.Part`
  ... `Objects.PourObject` — and each page has the tabs and group boxes of the dialog as nested classes, with a typed
  property for every control: `view.Objects.Part.Content.HiddenLines.HiddenLines = true`. The pages are generated from
  the Tekla Structures dialog definition (currently Tekla 2026) and checked against files saved from the dialogs; every key of a page has
  a property (keys the dialog does not show are in the page's `Advanced` group).
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
- No reference to the Tekla Open API: targets `netstandard2.0`, works in plugins, extensions and standalone tools of any
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

Drawings have the same `Marks`, `Objects` and `Protection` pages under their own prefixes, plus `Layout`, `Views`, the
titles and the user-defined attributes:

| Page | GA / multi (`.gd`, `.md`) | Assembly / single-part / cast unit (`.ad`, `.wd`, `.cud`) |
| --- | --- | --- |
| `Objects.Grid` | `dgrid.` | `grid.` |
| `Objects.Part` / `Objects.NeighborPart` | `dp.` / `dnpart.` | `dp.` / `dnp.` |
| `Objects.Bolt`, `Weld`, `Reinforcement`, `SurfaceTreatment` | `ds.`, `DrawingWeld.`, `drebar.`, `dsurf.` | same |
| `Objects.ReferenceObject` | `drefobj.` | — (null) |
| `Marks.PartMark`, `NeighborPartMark`, `BoltMark`, `ConnectionMark`, `WeldMark` | `dpm.`, `dnpm.`, `dsm.`, `djm.`, `dwel.` | same |
| `Marks.ReinforcementMark`, `SurfaceTreatmentMark` | `drebar_mark.`, `dsurf_mark.` | `drmark.`, `dsurfmark.` |
| `Protection` | `dpro.` | `dpro.` |
| `Layout`, `Views` | `dl.`, `dview.` | `dl.`, `dv.` |

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

// Keys without a typed property: dimension text height and colour
var dimensioning = ga.GetSection("dd.");
dimensioning.SetDouble("dim_text_height", 2.5);
dimensioning.SetEnum("dim_text_colour", DrawingColor.Blue);
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
```

Tekla accepts at most 1000 rows and 1023 bytes per value (the buffer of its filter dialog); a longer value or an
expression with more rows throws instead of writing a filter Tekla cannot use.

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
| `.gd`, `.ad`, `.wd`, `.cud`, `.md` and their `.more` / filter files | Marks, objects and protection pages typed; layout, view creation (with the View creation table), titles and user-defined attributes typed (common settings); every key by name; `CreateDefault()` |
| Page files (`.vg`, `.vpm`, `.gdgr`, ...) | `ExportTo` / `ImportFrom` on pages |
| Drawing layout, dimensioning, detail and section views (`dl.`, `dd.`, `dc.`, `DetailLabel.`, ...) | Every key by name; typed pages planned (decoding the drawing dialogs) |
| Filters (`.vf`, `.vnf`, `.vi.filter`, `.gd.filter`, `.gdf`, `.SObjGrp`, ...) | Supported |
| Dimensions (`.dim`) and other object attribute files | `GenericAttributeFile` |
| Object level settings (`.copt`, `TITLE_CLASSIFIER`) | Not supported |

## Requirements

- Any .NET implementation of `netstandard2.0` (.NET Framework 4.6.1+, .NET 6/8+). Tekla Open API applications target
  .NET Framework 4.8.
- The code pages used by attribute files are registered automatically through `System.Text.Encoding.CodePages`.

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

The page classes in `src/TeklaHelper.DrawingAttribute/Pages/Generated` are generated by `tools/DialogClassGenerator` from
the decoded dialog definitions in `tools/DialogClassGenerator/Dialogs` (see CLAUDE.md); do not edit them by hand.

### Packing

```shell
dotnet pack src/TeklaHelper.DrawingAttribute/TeklaHelper.DrawingAttribute.csproj -c Release -o artifacts
```

## License

[MIT](LICENSE)

Tekla and Tekla Structures are trademarks of Trimble Inc. This project is not affiliated with or endorsed by Trimble.
