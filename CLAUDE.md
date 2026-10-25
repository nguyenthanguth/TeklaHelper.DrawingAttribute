# CLAUDE.md

C# library that models Tekla Structures drawing attribute files (`.gd`, `.ad`, `.wd`, `.cud`, `.md`, `.vi`, their `.more`
files, sub-dialog files such as `.vg` / `.vpm`), the files of the property dialogs of the drawing editor (`.dim`,
`.drtxt`, `.pm`, `.gln`, `.dprt`, ... 41 types) and filter files (`.vf`, `.vi.filter`, `.gdf`, `.SObjGrp`, ...) as
objects, so settings the Drawing Open API does not expose can be written to a file and loaded by name
(`new GADrawing(name, "file")`, `view.Attributes.LoadAttributes("file")`).

`docs/how-it-was-built.md` records how the formats and the dialog structure were found and checked; keep it in step
when a finding changes.

The maintainer writes in Vietnamese: reply in Vietnamese. Code, XML comments, README and commit messages are in English.

## Commands

```shell
dotnet build TeklaHelper.DrawingAttribute.slnx -c Release -warnaserror   # CI builds with -warnaserror
dotnet test  TeklaHelper.DrawingAttribute.slnx -c Release                # net48 and net10.0
dotnet pack  src/TeklaHelper.DrawingAttribute/TeklaHelper.DrawingAttribute.csproj -c Release -o artifacts
```

Round-trip every real attribute and filter file in a folder (read, write back, compare bytes; filter rows to a tree and
back). Run it after any change to `AttributeValue`, `AttributeCollection`, `AttributeFileFormat`, `FilterFile` or `FilterRow`:

```shell
TEKLA_ATTRIBUTE_SAMPLES="D:/Tekla/Structures/2025.0/Environments" dotnet test TeklaHelper.DrawingAttribute.slnx -c Release --filter "FullyQualifiedName~SampleFileTests"
TEKLA_ATTRIBUTE_SAMPLES="D:/Tekla/Models" dotnet test TeklaHelper.DrawingAttribute.slnx -c Release --filter "FullyQualifiedName~SampleFileTests"
```

Check that every page file saved from the view dialog matches its page in `ViewProperties`, and that every key of the
pages in the `.vi` / `.gd` files there has a typed property (only for folders where a view and its pages were saved
together; shipped environments share names without being saved together):

```shell
TEKLA_VIEW_DIALOG_SAMPLES="D:/Tekla/Models/New model/attributes" dotnet test TeklaHelper.DrawingAttribute.slnx -c Release --filter "FullyQualifiedName~SubDialogFiles|FullyQualifiedName~GeneratedPagesTests|FullyQualifiedName~ObjectAttributeFileTests"
```

Check that every key of every drawing object file (`.dim`, `.drtxt`, `.pm`, ...) a recent Tekla wrote has a typed property:

```shell
TEKLA_ATTRIBUTE_SAMPLES="D:/Tekla/Structures/2025.0/Environments" dotnet test TeklaHelper.DrawingAttribute.slnx -c Release --filter "FullyQualifiedName~EveryKeyOfTheShippedFiles"
```

The dialog pages (`Pages/Generated/*.g.cs`, and in `DrawingObjects/Generated/*.g.cs` the drawing object files with their
own pages; the generator puts each class in the folder of its namespace) are generated (the generator compiles only the format classes of the library,
so it runs even when the library does not build until the pages are regenerated); change `tools/DialogClassGenerator` (rules in `Program.cs`,
pages in `PageSpecs.cs`, enum renames in `EnumRenames`) or its input, regenerate, and review the diff. The writers name
every type in full (`global::...`); `CodeShortener` then gives each file the `using`s it needs and short names, keeping
the namespace where a member of the page has the type's name (`Pages.Units` next to a property `Units`). Input: the dialog
definitions decoded from Tekla's `.aid` files (`tools/DialogClassGenerator/Dialogs/*.json`, made with
`tools/DialogDecoder`; its README explains the `.aid` format and how to decode another Tekla version, and `compare.py`
diffs two decodings) and the files saved from the dialogs, for value types and keys the dialog does not show. The
drawing object files are listed in `ObjectFileSpecs.cs` (one spec per file type: its pages and prefixes); the generator
writes their classes too (except the hand-written `DimensionAttributeFile`). The
pages of the drawing properties come from `DRAWING_PAGES` in `extract.py` (a dialog or panel per page plus other dialogs
with the same keys, decoded with the samples `DRAWING-TEST.gd` and `test-ad.ad` passed after `--drawing`); controls of the
other dialogs go into the tabs and groups of their own dialog (`dialogPath`). Fix decoding errors in `extract.py`, never
by hand in the JSON files:

The JSON files are decoded from the **latest** Tekla dialog (decision: follow the newest version; older versions are
expected to cope). Currently Tekla 2026 (`TEKLA_AID=C:/Users/InuYasha/Downloads/test/2026/TeklaStructures.aid`, Tekla
2026 is not installed here) with the 2025 dialog files as samples. Keys an older dialog lacks are listed in
`KeysAddedIn` (generator), which notes the version in the XML comment.

```shell
dotnet run --project tools/DialogClassGenerator -c Release -- tools/DialogClassGenerator/Dialogs "D:/Tekla/Models/New model/attributes/VIEW-TEST.vi" "D:/Tekla/Models/New model/attributes/DRAWING-TEST.gd" src/TeklaHelper.DrawingAttribute/Pages/Generated "D:/Tekla/Models/New model/attributes/test-ad.ad" "D:/Tekla/Structures/2025.0/Environments"
```

The sample application (`samples/TeklaHelper.DrawingAttribute.Samples`, net48 x64, Tekla NuGet packages with
`ExcludeAssets=runtime` + TeklaHelper.AssemblyResolver) is in the solution; keep it building. `... .Samples.exe offline`
runs the examples that do not need Tekla.

Oracle tests (`TeklaOracleTests`): on net48 the test project references Tekla 2025's `Tekla.Structures.dll` when the
registry (`HKLM\SOFTWARE\Trimble\Tekla Structures\2025.0\setup\MainDir`) or `-p:TeklaBinDirectory=...` points at it, and
checks that `FilterProperties` and every filter file the library writes match `Tekla.Structures.Filtering` byte for
byte. Without Tekla (CI) they are not compiled. A mutation of one token or one format rule makes them fail.

`FilterProperties.cs` is generated; regenerate it (for a new Tekla version, adjust the registry key in the project file)
and review the diff:

```shell
dotnet run --project tools/FilterCatalogGenerator -c Release -- src/TeklaHelper.DrawingAttribute/Filters/FilterProperties.cs 2025
```

## Reference material on this machine

- Tekla Structures 2025 is installed under `D:\Tekla\Structures\2025.0` (`bin` = Open API DLLs and dependencies).
  `C:\TeklaStructures\2025.0` only holds environments.
- Sample files: `D:\Tekla\Structures\2025.0\Environments\**` (about 2000 key-value files in 190 extensions, 3378 filter
  files, incl. Japanese 932 / Korean 949) and `D:\Tekla\Models\*\attributes`.
- `D:\Tekla\Models\New model\attributes` holds files the maintainer saved from the Tekla 2025 dialogs: `TEST.vi` plus one
  file per view page (`.vg`, `.vp`, `.vpm`, ...) and filters (`.vf`, `.vnf`, `.vi.filter`, `.vi.npfilter`);
  `VIEW-TEST.vi` (+ `.vi.filter`, `.vi.npfilter`) and `DRAWING-TEST.gd` (+ `.gd.more`, `.gd.filter`, `.gd.npfilter`),
  the files the Open API reads (`TEST.gd` was replaced by `DRAWING-TEST.gd`). `OBJECT-TEST.<ext>` (40 files, every
  extension of `ObjectFileSpecs` but `.dim`) were saved from the property dialogs of the Tekla 2025 drawing editor by a
  drawing macro (`akit.Callback("acmd_display_attr_dialog", "text_dial", "main_frame")`, then the Save as field and
  button of the dialog, names from the `.aid`; `Operation.RunMacro("..\\drawings\\x.cs")` with a drawing open, macro file
  in `environments/common/macros/drawings`, removed afterwards). `OBJECT-TEST.dim` is not kept there: it would change the
  sample counts of the released `.dim` pages. `test-ad.ad` (+ `.ad.more`) was saved
  from the Tekla 2025 assembly drawing dialog: every page has exactly the key set of `DRAWING-TEST.gd` under the
  assembly prefixes (`dv.`, `grid.`, `dnp.`, `drmark.`, `dsurfmark.`), there is no `drefobj.` and no `dfilter.` /
  `dnp_filter.`, and `dcd.` (146 keys, assembly dimensioning) exists only there. The `tda_*` files there were written by
  the sample application, not by the dialogs. The maintainer can save more samples on
  request, and allowed probing in the open drawing of that model.
- `lib/` holds Tekla 2025 `Tekla.Structures*.dll` for reflection only. It is git-ignored (proprietary); the library must
  not reference it.
- To see what the Open API writes, build a throw-away net48 x64 console in the scratchpad that references
  `D:\Tekla\Structures\2025.0\bin\Tekla.Structures.dll` (`<Private>false</Private>`) and resolves the other DLLs from that
  folder in `AppDomain.AssemblyResolve`. File APIs such as `Filter.CreateFile` work without Tekla running. Costura and
  some tools fail on the scratchpad path (over 260 characters); use a short folder under `%TEMP%` then.

## Architecture

`src/TeklaHelper.DrawingAttribute` targets `net48`, `net8.0` and `netstandard2.0` (decided: only `netstandard2.0` needs
`System.Text.Encoding.CodePages`, whose version differs from the 7.0 Tekla 2025 ships; `TextEncodings` registers the
provider outside .NET Framework only; no Costura) and has no Tekla reference (decided: the filter part of
`Tekla.Structures.dll` needs `BinaryFormatter`, so it fails on .NET 8+, and embedding it with Costura would ship five
Trimble DLLs and load them side by side in other Tekla versions).

- `AttributeValue` — one value (String / Integer / Double), keeps the raw text and, when read from a file, the original
  bytes, so an unchanged value is written back byte for byte.
- `AttributeCollection` — ordered keys of one file; setting an existing key keeps its position, new keys are appended.
- `AttributeFileFormat` — reads/writes the key-value format.
- `AttributeSection` — keys sharing a prefix, typed `Get*/Set*` helpers (public, so unmodelled keys stay reachable),
  `GetSection(prefix)`, `GetNames`, `CopyFrom`, `ExportTo` / `ImportFrom` (a sub-dialog file such as `.vg`). Sections
  are views over the file's collection, not copies. `SectionFileExtensions` lists the sub-dialog extensions of the
  pages that have no page class (layout, views, dimensioning, ...).
- `AttributeFile : AttributeSection` — a file type: `Extension`, `Load`, `Save`, `SaveTo(dir, name)`, `CompanionFiles`
  (`IAttributeFileCompanion`: the `.more` file and filter files load/save with the main file), `LegacyKeyPrefix`.
  `Load` is atomic: every file is parsed (`PrepareLoad`) before any is applied. `SaveTo` likewise encodes every file of
  the set (`PrepareSave` → `AttributeFileSave`) before writing any, and undoes the in-memory changes (`in_file` /
  `out_file`) when one cannot be encoded. `TextEncodings` holds the code page / strict UTF-8 helpers shared by the
  key-value and filter formats; `AttributeFileName` the name rules. `CodePage` lives on the
  `AttributeCollection`, so sections export/import sub-dialog files in the host's code page. A filter companion is
  written only when it has rows; without rows an existing file of that name is deleted, because Tekla 2025 shows no
  objects at all in a view loaded with an empty `.vi.filter` (`Count= 0`) — verified in the open drawing with a probe view.
- `UserAttributeFile` (adopts the object type of a loaded file), `DrawingUserAttributeFile` (comment, user fields,
  `DR_*`), `GenericAttributeFile` (any extension, by key).
- `Pages/Generated/` — one generated class per page of the View properties dialog (`ViewAttributes`, `ViewLabelMark`,
  `ViewDimensioning` in the `Views` namespace; `ProtectionPage`, `PartMark` ... `PourMark`, `PartObject` ... `PourObject`
  in `Pages`), with the tabs, group boxes and sub-dialogs as nested classes, a typed property per control (toggles →
  `bool` with the dialog's on/off values, options → generated enums in `Enums.g.cs`, colours → `DrawingColor`, line
  types → `DrawingLineType`), an `Advanced` group for keys the dialog does not show (other pages, older dialogs, keys
  only seen in files), `SetAllEnabled`, internal `Keys` / `EnableKeys`, and the prefix and page file extension constants
  of each host (`ViewPrefix`, `GADrawingPrefix`, `AssemblyDrawingPrefix`, `*FileExtension`). The pages of the drawing
  properties are in the `Drawings` namespace: `DrawingViewCreationSection` (generated part of the hand-written class:
  `HandWrittenPart`, its keys in `HandWrittenKeys`), `SectionViewPage`, `DetailViewPage`, `DimensionPage`,
  `DimensioningPage` (GA family) and `AssemblyDimensioningPage` (assembly family); their label text rows
  (`LabelMarkA1` ..., `SectionMarkA1` ..., `DetailMarkA1` ...) are `ViewLabelMark` pages (`PageSpec.LabelMarks`).
- `Pages/` hand-written: `PageHost` (View / GADrawing / AssemblyDrawing → prefixes), `MarkPages` and `ObjectPages`
  (the Marks and Objects nodes; `ObjectPages.ReferenceObject` is null in the assembly family).
- `Sections/FilterSection` — `vfilter.` / `dfilter.` keys + the rows of the companion filter file.
- Defaults: every page has a generated `SetDefaults()` (one property assignment per control, then `SetAllEnabled(true)`).
  `Pages/Generated/FileDefaults.g.cs` lists every key of each file type in the order of the dialog files (`VIEW-TEST.vi`;
  `DRAWING-TEST.gd` for `.gd`/`.md`; `test-ad.ad` for `.ad`/`.wd`/`.cud`, whose newer shipped files have the same `dcd.`
  keys) plus keys of the newer dialog; a null value marks a page key. Page values: the most common ASCII value in at least 3 shipped files (no empty font, zero text height or
  zero text colour), else the dialog default (the line type menus declare their first item, Solid, which no shipped
  file holds), else the dialog file value. Other keys: most common shipped value (same type,
  then `.md`→`.gd`, `.wd`/`.cud`→`.ad`), else the dialog file value; `EnvironmentFreeValues` sets layouts, attribute
  files, `StyleName`, titles and descriptions to names every environment has or to empty text. `CreateDefault()` applies
  the table, calls `SetDefaults()` of every page, then `Pages/DefaultSetup` (the library's own choices) and, for drawings,
  the empty user-defined attribute fields. Mark content defaults are built with `MarkContent` in `SetDefaults()`,
  assigned through the page's `MarkContent` property (`Content.MainPartElements = new MarkContent();`,
  `Advanced.PourElements = ...`; an empty default is written as the empty text the dialogs store). The pour
  tables (`PourContentString`, `PourBreakContentString`) of the mark pages and files are empty by the library's choice
  (`EmptyPourTable` in `Defaults.cs`), where the shipped files hold MATERIAL and "POUR BREAK"; the pour mark page and
  `.pom` keep MATERIAL in `PourContentString`, the table their dialog edits
  (`tools/DialogClassGenerator/MarkContentCode.cs`: element types from the catalog in `Marks/MarkElementType.cs`, the
  kind of mark from the table the key is for); the generator writes the code only when the rules of `MarkContent.ToXml`
  give the default back byte for byte, keeps an element they do not as `MarkXmlElement` (the pull-out picture) and a
  value laid out otherwise as a literal (none now). The `FileDefaults` table keeps raw file text, XML included. The pages of the drawing properties only keep the table's values of their
  own file type (GA and assembly files do not agree on dimensions; `SetDefaults()` serves every host), so their keys are
  not null in the table. Change values in `tools/DialogClassGenerator/Defaults.cs` or `DefaultSetup`.
- `DrawingObjects/` — the files of the property dialogs of the drawing editor: `DimensionAttributeFile` (hand-written)
  and the generated `*AttributeFile.g.cs` and their own pages in `DrawingObjects/Generated` (`Properties` = the page; `.dim` / `.rdim` add nine
  `DimensionMark`s). Files whose key set equals a page of the view properties (`.pm` `.sm` `.jm` `.rm` `.surfm` `.pom`
  `.wel` `.welo` `.sc` `.pbr` `.drmp` `.dgr` `.grl`) use that page's class; the others have their own pages
  (`TextProperties`, `PartProperties`, ...), decoded from their dialog with the sibling dialogs of the same structure as
  other sources (`FILE_PAGES` in `extract.py`). In `FileDefaults` the keys of a file's own pages are null (their
  `SetDefaults()` writes them); a page shared with another type (view mark page in `.pm`, dimension pages in `.rdim`)
  gets the values of the file's own type in the table, and `CreateDefault()` does not call its `SetDefaults()`.
- `Drawings/` — `DrawingAttributeFile` base with `Layout`, `Views`, `SectionView`, `DetailView`, `Dimension`,
  `Dimensioning` (null in the assembly family), `AssemblyDimensioning` (null in the GA family), `Protection`, `Marks`,
  `Objects`, titles and `UserAttributes`; `GADrawingAttributeFile`, `MultiDrawingAttributeFile` (GA prefixes) and `AssemblyDrawingAttributeFile`,
  `SinglePartDrawingAttributeFile`, `CastUnitDrawingAttributeFile` (assembly prefixes); `DrawingLayoutSection`,
  `DrawingViewCreationSection`.
- `Views/ViewProperties` — `.vi` organised like the View properties dialog tree: `Attributes` (`ViewAttributes`, top-level
  keys, + `LabelMarkA1` ... `A5` in `Views/ViewAttributes.cs`), `Filter` / `NeighborPartFilter` (`FilterSection`:
  `vfilter.` / `vnp_filter.` keys + rows in `.vi.filter` / `.vi.npfilter`), `Protection`, `Dimensioning`, `Marks` (9),
  `Objects` (11). Saves only `.vi`, `.vi.filter`, `.vi.npfilter` — what the dialog's Save button writes and the Open
  API reads.
- `DrawingObjects/` — files of the drawing editor's object dialogs: `DimensionAttributeFile` (`.dim`, legacy prefix
  `dim_dial.`): generated `DimensionProperties` (top-level keys, dialog `gr_dim_attrib` + rebar keys from
  `gr_rebar_dim_attrib`, "Place..." `dimpl.`) and nine `DimensionMark` pages (`PrefixMark.`, `PostfixMark.`, the seven
  `*TagMark.`; dialog `gr_dim_mark_attrib`, whose bare members `height`, `frame_type` ... are the `FontAttr.` /
  `FrameAttr.` keys). Decoded from `FILE_PAGES` (`extract.py`, samples `--file standard.dim` and `--file-folders`), generated
  from `PageSpec.FileExtension` / `FilePrefixes` / `SampleFile`; `FileDefaults.Dimension` for `CreateDefault()`.
- `Marks/` — `MarkContent` (the XML of the `ContentString` keys as elements: `MarkPropertyElement` with a
  `MarkElementType` from `MarkElementTypes`, `MarkUserAttributeElement`, `MarkTextElement`, `MarkSeparatorElement`,
  `MarkSymbolElement`, `MarkFrameElement`, `MarkTemplateElement`, `MarkXmlElement` for the rest; `MarkFormat`). Each
  element keeps its XML attributes in the order read (`XmlAttributeList`), and the content its layout (a line per tag, or
  one line), line break and way of writing `>`, so content Tekla wrote is written back as it was.
  `AttributeSection.GetMarkContent` / `SetMarkContent` (no change when the elements are the same), `ViewLabelMark.Content.Elements`,
  and the content tables as `MarkContent` properties, generated: those of the pages from
  `tools/DialogClassGenerator/ContentProperties.cs` (the tables probed by kind of object, and `PourElements`,
  `PourBreakElements`, `EdgeChamferElements`, `ReferenceObjectElements` on every mark page, named after the key; the
  tables of pages not probed get no property), the `Elements` of the mark files of the drawing editor from
  `ObjectFileSpec.DialogContent` (the key the dialog reads and the keys it writes). A key with a `MarkContent` property
  has no string property (removed in 2.0.0, so that nobody mixes `MainPartElements` and `ContentString1`); the raw text
  stays reachable with `GetString` / `SetString`. A property goes where the dialog shows its table (3.0.0): the tables of
  the "Mark content of" menu and single tables in the container of the elements table (`Content` tab; the page itself
  for the dimension marks), the merged mark in the container of the merge table (`Merging` tab), keys the dialog does
  not show where their control was (`Advanced`). The generator marks those containers from the `*selected_elements`
  tables of the decoded dialogs before it prunes the tables (`Container.ContentTable` / `MergeTable`).
  `ContentProperties_AreWhereTheDialogShowsTheirTable` pins the result.
- `Filters/` — `FilterFile` (rows, byte-preserving), `FilterRow`, expression tree (`FilterExpression`, `FilterCondition`,
  `FilterGroup`), `FilterOperator`, typed `FilterProperty` subclasses, `FilterProperties` (generated catalog of
  `Tekla.Structures.Filtering.Categories`), `FilterFileExtensions`, `FilterObjectType` (hand-written: the label keys of
  the object type row, as extension methods on the numeric `Object.Type` so the released field keeps its type).

Typed property pattern for hand-written classes — always name the key in the XML comment:

```csharp
/// <summary>Scale, e.g. 20 for 1:20 (<c>view_scale</c>).</summary>
public double Scale
{
    get => GetDouble("view_scale", 1);
    set => SetDouble("view_scale", value);
}
```

## Files the Open API reads vs. dialog-only files

The Open API (`new GADrawing(name, file)`, `View.ViewAttributes(file)`) reads only `name.vi` + `.vi.filter` +
`.vi.npfilter` for a view and `name.gd` + `.gd.more` + `.gd.filter` + `.gd.npfilter` for a GA drawing — exactly what
the Save button of the drawing / view properties dialog writes (confirmed by the maintainer). Page files (`.vf`, `.vnf`,
`.vg`, `.vpm`, `.gdf`, `.gdgr`, ...) are only for the Save / Load buttons of single pages in the dialogs; the library
writes them only through `ExportTo` / `FilterFile.SaveTo`. The view dialog writes no `.vi.more`.

## Verified in a running Tekla 2025 (probe view in the open drawing)

A temporary view was inserted in the maintainer's open GA drawing, loaded from copies of `TEST.vi` with one key changed
at a time (`View.Attributes.LoadAttributes` + `Modify`), and `Part.PartAttributes` read back. Findings:

- **Tekla caches attribute files by name for the session.** Re-saving a file under a name Tekla already loaded is not
  picked up; a probe needs a new name per load. Users who regenerate a file while Tekla runs should use a new name.
- **An empty filter file (`Count= 0`) shows no objects at all.** Without filter rows, write no `.vi.filter` (the library
  deletes a stale one). The dialogs write "no filter" as one row `co_part proNAME == Empty`.
- **Keys a loaded file lacks take the value of the environment's `standard.vi`**, not the view's current value (a view
  with red dashed lines lost them when a file with only `view_scale` was loaded, and got colour 6 of the Korea
  `standard.vi`). A file made with `CreateDefault()` was applied as written (black lines, not colour 6).
- **Drawings made with `CreateDefault()`** (GA, assembly, single-part, cast unit) were inserted in the model and their
  views showed the file's part settings. Views of the assembly family take their settings from the .vi named in the
  View creation table `dv.aDrawingViews` (rows `type,creation,label,file,`: types 1 front, 2 top, 3 back, 4 bottom,
  5 end, 7 section, 16 3D; creation Off 0 / Auto 1 / On 2, also for `cre_*`; `Create3dView` On = 2), else from the
  drawing's own object and mark settings — except the 3D view, which with an empty name took the environment's
  `standard.vi` (Korea: 1:75, outline, colour 6). `CreateDefault()` clears the names (the shipped ones exist only in some
  environments); naming a `CreateDefault()` .vi made every view, 3D included, use it. Several rows of one type: the
  highest creation counts (Tekla wrote `cre_cuts 2` for Auto + On); `SetViewTable` keeps a table's final-comma form. `DrawingViewCreationSection.GetViewTable/SetViewTable/SetViewAttributeFile`.
- **The fixed fields of the old filter page are ignored:** a view loaded with `vfilter.partname "BEAM"` (or `profile
  "I*"`), its `_enable` 1 and `filtering_type` 0, 1 or 2, without `.vi.filter` rows, showed all 85 parts; the rows
  (`co_part proNAME == BEAM`) showed 9. Hence no properties for `vfilter.partname` ... (60 keys per filter page).
- **Dimension properties are applied:** `new StraightDimensionSetAttributes("name")` read from a `CreateDefault()` .dim
  with changes, and a dimension created with it in a probe GA drawing, both showed dimension type Absolute, precision
  OnePerEight, format TwoDecimals, short dimensions Inside, text height 5, frame RoundedRectangle, colour Red,
  exaggeration on / RightUp (stored 1) / width 7.
- **The drawing pages are applied:** an assembly drawing of a two-part assembly created from a `CreateDefault()` .ad with
  changed values showed them through the Drawing API: `dv.np_extrema_size` 321, `HoleSymbol`, `Elevation_datum_point`
  1234, `cut_skew_parts` on the front and bottom views (`View.Attributes`; the section view keeps `dc.` values);
  `dd.dim_type` Absolute, `dim_line_colour` Red, `dim_text_height` 5 (`StraightDimensionSet.Attributes`);
  `dc.SectionMark.line_colour` Blue, `SectionMarkSymbolColor` Green, `ArrowLength` 33 (`SectionMarkBase.Attributes`
  `LineColor`, `SymbolColor`, `LineLength`); `dcd.part_internal` / `main_part_extrema` / `screw_main_internal` None gave
  3 dimension sets instead of 4 in the front view.
- **Mark `create` is ignored:** a view loaded with `vpm.create 0` still showed its 85 part marks; `vpm.in_all_views 3`
  (`VisibilityInView.None`) removed them (245 → 160 marks). `KeyNotes` in the generator says so in the XML comment of the
  part mark page only (`create` of the weld mark page is the visible "Welds" menu; the others were not probed).
- **`_en` check boxes do not gate `LoadAttributes`:** with `draw_hidden 1` + `draw_hidden_en 0` the value is applied.
  They only matter to the dialog's Modify.
- Colour numbers pass through unchanged into `DrawingColors` (`3` stays 3, `160` is Red); the small numbers are valid,
  unnamed `DrawingColors` values.
- Line types: `0` Undefined, `1` Solid, `2` Dashed, `3` Slashed, `4` DashDot, `5` Dotted, `6` DashDoubleDot, `7` SlashDash;
  `8`+ makes Tekla drop the whole file.
- Part `draw_as` = `Part.Representation` (flags): `1` Outline, `2` Symbol, `4` WorkshopForm, `8` Exact, `16` BoundingBox,
  `32` BaseBox, `64` SolidWithPartialProfile.
- Part keys → API: `draw_hidden`/`draw_own` → `DrawHiddenLines`/`DrawOwnHiddenLines`; `offset` → `SymbolOffset`;
  `PartialProfileLength/Offset`; `cl_*`/`rl_*` → `DrawCenterLine`/`DrawReferenceLine` (the cell for the part's type);
  `draw_north_mark` → `DrawOrientationMark`; `AddPopMarks` → `DrawPopMarks`; `AddChamfers` → `DrawChamfers`;
  `visible|section|hidden|own_hidden|reference|center` + `_colour`/`_linetype` → `VisibleLines`, `SectionLines`,
  `HiddenLines`, `OwnHiddenLines`, `ReferenceLine`, `CenterLine` `.Color`/`.Type`; `PartHatch.*` → `FaceHatch`,
  `SectionHatch.*` → `SectionFaceHatch` (`HatchAutomaticOrCustom` → `AutomaticScaling`, `HatchBackgroundColor` ≠ 0 →
  `DrawBackgroundColor`); `aCustomPresentation` → `CustomPresentation`. Not exposed by the API (file only):
  `inner_contours`, `FilletEdges`, `connecting_side_mark`, `bolt_*`, `np_*`, `hatch_type`, `hatch_factor_type`.
- Content tab of the Part page (screenshot from the maintainer, values matched with `TEST.vp`): Representation
  (`draw_as`, Partial profile `PartialProfileLength`/`PartialProfileOffset`, Symbol offset `offset`, Inner contours
  `inner_contours`), Hidden lines (`draw_hidden`, `draw_own`), Center line (`cl_en`, `cl_main|second_beam|plate|polygon`),
  Reference lines (`rl_*` likewise), Additional marks (Orientation `draw_north_mark`, Connecting side
  `connecting_side_mark`, Pop-marks `AddPopMarks`, Edge chamfers `AddChamfers`, Fillet edges `FilletEdges`).
- **Drawing object files and the Open API** (each type written by `CreateDefault()` with one value changed, loaded with
  every attribute class of Tekla.Structures.Drawing that takes a file name): `Text.TextAttributes` `.drtxt`,
  `Symbol.SymbolAttributes` `.sbl`, `LevelMark.LevelMarkAttributes` `.lev`, `Line`/`Arc`/`Polyline`/`Circle`/`Rectangle`
  `.gln`/`.gar`/`.gpl`/`.gci`/`.grt`, `Polygon` and `Cloud` `.gpg`, `EmbeddedObjectAttributes` `.fdg` (DWG and images),
  `LinkAttributes` `.fhl`, `Part.PartAttributes` `.dprt`, `PourObject.PourAttributes` `.po`, `PourBreak` `.pbr`, `Bolt`
  `.sc`, `Weld` `.welo`, `WeldMark` `.wel`, `EdgeChamfer` `.dcha`, every `ReinforcementBase.*Attributes` `.drbr` (meshes
  too, not `.drmsh`), `Surfacing` `.dsrf`, `Grid` `.dgr`, `GridLine` `.grl`, `SectionMarkBase.SectionMarkAttributes`
  `.cs`, `DetailMark` `.detail`, the dimension attributes `.dim`; `Mark.MarkAttributes(drawingObject, name)` reads `.note`.
  Nothing reads `.rev` `.fas` `.idg` `.ldr` `.drmsh` `.wls` `.drmp` `.ga_dim_setup` `.rdim` or the mark files `.pm` `.sm`
  `.jm` `.rm` `.surfm` `.pom` by name. Values: `arrow` 1/2/3 = `Arrowhead.ArrowPosition` Start/End/Both;
  `scaling_type` = `EmbeddedObjectScalingOptions` / `ScalingOptions`; `PartHatch.*` / `SectionHatch.*` = `FaceHatch` /
  `SectionFaceHatch`; a mark's `Frame` is the top-level `line_colour` (frame around the mark), `FrameAttr.*` the frame
  around its elements.
- **Mark content (`ContentString*`)**: a file with one element `subType` N, loaded with `Mark.MarkAttributes`,
  `View.ViewAttributes`, `SectionMarkAttributes` and `DetailMarkAttributes`, gave the same element in all four for N = 1
  to 110: 1 text, 2 line feed, 3 symbol, 5 frame, 16 user-defined attribute, 7–98 property elements (22 assembly
  position, 23 part position, 24 profile, 25 name, 79 scale, 80 detail name, 81 section name, 82 view name, 85 source
  drawing name when moved, 89 template, 90 space, 99 `REBAR_SEQ_POS`; the numbers equal the Open API's `PropertyTypes`
  for 1–53 and 57–70 only). Tekla reads the number only: `name` and `value` are ignored. `Format` gives font,
  colour, height (and `unit` / `format` of a user-defined attribute); `override` and `hidden` change nothing the API
  reads. `BackSpace` is dropped by the API; it inserts a `SpaceElement` between elements otherwise. A frame needs a
  `frame_id` other than 0 (with 0 or none its elements read as following it; the files number them from 101). Two layouts: a line per tag with
  `hidden="0"` (mark dialogs, drawing properties) or one line without `hidden` (the 2025 View properties dialog).
- **Content tables** (TFIL_MODE=marktables of the harness: every content key of a `.vi` holds its own name, a view of a
  probe GA drawing is inserted with it and every mark read back; a single bar and a mesh were added to a slab, one bolt
  made a workshop bolt and one assembly a sub-assembly for the run, all put back): part mark `vpm.` main part 1,
  secondary 2, sub-assembly main part 3, sub-assembly secondary part 4; bolt mark `vsm.` site 1, workshop 2;
  reinforcement mark `vrm.` single 1, group 2, mesh 4 (the "Mark content of" menu values are Single 0 / Group 1 / Mesh 2:
  not the table numbers); connection mark 1. Marks merged by `MergeMarks` + `leader_line_type` 1–4 (159 → 68 marks) still
  show table 2. Loading other view properties (`LoadAttributes` + `Modify`): the part marks of a view Tekla had just
  inserted took the new content (sample 24 shows it, in two drawings); the reinforcement group marks of the existing view
  `C BOT` of drawing FOOTING [25] kept theirs. Why (marks placed or edited in the drawing, or reinforcement marks) was not
  looked at. Table 3 of the reinforcement mark holds only `<MergeElement subType="53"..."56">` in the files (the
  Open API's `MergedMarkPropertyElementTypes`: block prefix, single mark content, distances between groups, separator):
  the merged mark set of `Operation.MergeMarks`, whose content the API cannot read. The associative note
  (`Mark.MarkAttributes(object, name)` on a `.note`) has a table per kind of object: every part 1, every bolt 2, every
  reinforcement (single, group, mesh) 4, a reference model `ReferenceObjectContentString`; surface treatment (3 by the
  files) was not checked. The typed properties: `PartMark.Content.MainPartElements` ..., `BoltMark.Content.SiteBoltElements`
  ..., `ReinforcementMark.Content.SingleElements` ..., `AssociativeNoteProperties.Content.PartElements` ....
  The other mark pages (TFIL_FILTERNAME=GIRDER with `vnp.np_collect_by 4` (by extreme) for neighbour parts and bars,
  TFIL_SURFACE=1 for a surface treatment added to a slab, all put back): neighbour part mark `vnpm.` main part 1;
  neighbour reinforcement mark `NeighborRebarMark.` single 1, group 2, mesh 4; connection mark and surface treatment mark
  (no menu, one table) 1. No mark was made for a neighbouring secondary part (also with `for_sec` 1 and a secondary part
  added to a column's assembly), so `vnpm.ContentString2` = "Secondary part" comes from the menu (Main part 0,
  Secondary part 1, as on the part mark page). The dimension mark's menu (Part, Bolt, Rebar, Surface treatment) and the
  shipped files give part 1, bolt 2, surface treatment 3, reinforcement 4, like the note; not probed.
- **The mark dialogs of the drawing editor have one table** (TFIL_MODE=dlgtables: a file whose every content key holds
  its own name, loaded and saved again by each dialog; TFIL_MODE=marktables with TFIL_PMDIALOG=1: the part mark dialog
  loaded with such a .pm and Modify pressed on every part mark of a view). Part mark (`.pm`) and bolt mark (`.sm`) read
  `ContentString1` and save it into `ContentString1` ... `4`; connection (`.jm`), reinforcement (`.rm`) and surface
  treatment (`.surfm`) marks read `ContentString1` and save it into 1, 2 and 4, leaving 3 (the merged mark of
  reinforcement marks); the pour mark (`.pom`) reads `PourContentString` and saves it there and into `ContentString1`.
  Modify gave every selected part mark (main, secondary, of a sub-assembly) table 1, whatever `CurrentContentTableNumber`
  or the menu said; the .aid of 2025 and 2026 shows no "Mark content of" menu in these dialogs (`gr_pmark_attrib` ...),
  only in the note and dimension mark dialogs. So `ContentString1` ... `4` mean main / secondary / sub-assembly main /
  sub-assembly secondary part on the part mark page of the view and drawing properties only; the files' `Elements` write
  the one table like the dialogs.
- **Drawing object files in the dialogs**: every `CreateDefault()` object file (41 types), with one value changed, was
  loaded in its dialog by a macro (Load) and saved again (Save as): the dialogs kept the value (rounded to the field's
  decimals) and every key, except mark contents they normalise, the obsolete `RebarGroupMarkAttributes.*` keys of `.dim`
  and values they compute (`text_scale` of `.fas`).
- **Picture-only menus**: a macro selected each value of a menu (`akit.ValueChange(dialog, widget, "<value>")`, the
  value, not the index; `akit.TabChange` for other tabs) and `PrintWindow` captured the dialog: that named the leader
  line menus of text, symbol, level mark, revision mark and note (`EnumRenames` / `MemberRenames`). The leader line type
  (`text_type`) changes nothing the Open API reads; `arrow_type` 0/1/2 = `ArrowheadTypes` NoArrow/FilledArrow/LineArrow.
- View and drawing share the object/mark/filter pages: 24 sections of `VIEW-TEST.vi` and `DRAWING-TEST.gd` have
  identical key sets (`vp`↔`dp`, `vpm`↔`dpm`, `vg`↔`dgrid`, `vfilter`↔`dfilter`, ...). Only the view attributes and the
  drawing pages (`dl`, `dview`, `dc`, `dd`, `DetailLabel`, ...) differ.

## Key-value file format (verified on real files)

- `key value` per line, CRLF. First line `encoding <codepage>` (1252, 932, 949) except in `.more` files. Some shipped
  files lack the final CRLF.
- Strings are quoted, inner quotes are NOT escaped (value = first to last quote). Line breaks inside strings are `\x01`.
  Doubles are `F6` invariant (`20.000000`). Booleans are 0/1. `<name>_en` = the dialog check box for that setting.
- Values that are XML declaring `encoding="UTF-8"` (`*.ContentString*`, mark contents) are UTF-8 bytes even in a 932 file;
  some Japanese files even mix both encodings in one value — hence the byte preservation.
- Legacy files: no encoding line and every key prefixed with the dialog name (`view_dial.`, `gdraw_dial.`, `adraw_dial.`,
  `wdraw_dial.`, `cudraw_dial.`, `mdraw_dial.`); stripped on load.
- `.more`: keys `<objecttype>.<UDA>`. Object types disagree between shipped environments: `.gd.more`
  `gadrawing_attributes`; `.ad.more` `adraw_dial_attributes` / `assemblydrawing_attributes`; `.wd.more`
  `singledrawing_attributes` / `wdraw_dial_attributes`; `.cud.more` `cudraw_dial_attributes` / `castunitdrawing_attributes`;
  `.md.more`, `.vi.more`, `.gdp.more` `joint_attributes`. New files use the first; loading adopts the file's. Dates are
  Unix seconds, `-2147483648` = empty.
- Colours: `DrawingColor` values (153 black ... 165 magenta; 154–159, the Open API's `NewLine1` ... `NewLine6`, are
  named after their colour, the RGB of `TeklaDrawingColor` in each XML comment). Files saved from the dialogs often hold small numbers
  (`visible_colour 6`, `line_colour 1`); Tekla uses them unchanged (verified), so they are typed `DrawingColor` too and
  read as unnamed values. `TEST.vi` (saved from the dialog) holds `label_colour 0xFFFFFFFF`: integers are read as
  hexadecimal too, and `AttributeSection.Set` keeps the current value when the new one means the same, so assigning a
  property its own value changes no byte (hex, `-0.000000`, `for_sec 3`, mixed-encoding strings).
- Older page files (`common/system/standard.vpm`, ...) have no encoding line and prefix every key with the dialog name
  (`vpm_dial.`, `gdpm_dial.`, ...); `ImportFrom` strips it.
- File name keys differ by page: `in_file` / `out_file` in most, `aInFile` / `aOutFile` in `vref.`, `InFile` /
  `OutFile` in `PourBreaks.`.
- `.opt` / `.admodel` are not drawing attribute files (several blocks, each with its own encoding line).

## Drawing families and sections

| | GA `.gd`, multi `.md` | assembly `.ad`, single-part `.wd`, cast unit `.cud` |
| --- | --- | --- |
| views (227 keys) | `dview.` | `dv.` |
| grid (35) | `dgrid.` | `grid.` |
| neighbour parts | `dnpart.` | `dnp.` |
| reinforcement / surface marks | `drebar_mark.` / `dsurf_mark.` | `drmark.` / `dsurfmark.` |
| shared | `dl.` `dp.` `ds.` `dpm.` `dnpm.` `dsm.` `djm.` `dc.` `dd.` `dwel.` `DetailLabel.` ... | same |
| automatic dimensioning | `Dimensioning.` (19, dialog `gr_gdcd_attrib`) | `dcd.` (146, `gr_adcd_attrib`) |

Drawing page sources (2026 `.aid`): view `gr_ga_view_attrib` (+ `gr_dv_attrib`, the assembly panel
`PanelContentViewCreation`), section view the assembly panel `PanelContentSectionView` (+ `gr_dc_attrib`,
`gr_csym_attrib` under `SectionMark.`, `gr_section_label_attrib`; the GA dialog shows only `StartNumberOrLetter`), detail
view `gr_detail_symbol_attrib` (+ `gr_detail_label_attrib`), dimension `gr_dd_attrib` (+ "Place..." `gr_dd_placing`
under `dimpl.`). Label rows `ViewLabelMarkA1`..`A5`, `SectionMark.SectionMarkA1`.., `SectionMark.ViewLabelMarkA1`..,
`DetailMarkA1`.. all have the 26 keys of the view's `ViewLabelMarkA1.`. The top-level `*_en` keys (`dview_en`, `dc_en`,
...) are the check boxes in front of the page buttons. Cast unit `bound_type`: 1 by position, 2 by ID.

`dl.` is the struct `gr_dl_at_t`, shared by the GA layout dialog `gr_dl_attrib` (layout, sheet size) and the assembly
family's `gr_wdl_attrib` (also the Scale and Other tabs), so GA files hold every key; `dl.sheet` / `dl.size` are ids
in the environment's layout files, not formats. `dl.placing_direction` has no control in the 2020, 2025 or 2026
dialogs (only in files, 0 / 1): left untyped. `dl.in_file` / `out_file` are the page Save / Load names.

A sub-dialog saved on its own writes exactly the keys of one section without the prefix (checked value by value against
`TEST.vi`), and the host records the loaded name in `<prefix>.in_file`. View: `.vg` `vg`, `.vp` `vp`, `.vnp` `vnp`,
`.vs` `vs`, `.vpm` `vpm`, `.vnpm` `vnpm`, `.vsm` `vsm`, `.vjm` `vjm`, `.vrm` `vrm`, `.vnrm` `NeighborRebarMark`,
`.vsurfm` `vsurfm`, `.vr` `vrebar`, `.vnr` `NeighborRebar`, `.vw` `vwel`, `.vwo` `ViewWeld`, `.vsurf` `vsurf`,
`.vrmp` `vref`, `.vpbr` `PourBreaks`. Drawing extensions (`.gdgr`, `.gpm`, `.adgr`, `.apm`, ...) were matched by key set;
all mark kinds share one key set, so their kind comes from the extension name only.

## Filter files (block format, verified with Tekla.Structures.Filtering 2025)

```text
TITLE_OBJECT_GROUP 
{
    Version= 1.05 
    Count= <rows> 
    SECTION_OBJECT_GROUP 
    {
        <open parentheses>
        <enabled 1/0>
        co_part              category
        proNAME              property (UDA name for Custom*)
        albl_Name            property label (UDA name for Custom*)
        ==                   condition symbol
        albl_Equals          condition label
        <value>
        <close parentheses>
        &&                   && | || | Empty
        }
    }
```

- Token lines end with a space, braces do not, CRLF, final CRLF. Spaces inside a token are `\x02`; line breaks are
  impossible (rejected by the library). A file is read as UTF-8 when its bytes are valid UTF-8, else in the fallback
  code page; a UTF-8 BOM is kept.
- `Filter.CreateFile` writes the same bytes for every `FilterExpressionFileType`; only the extension differs
  (`.SObjGrp`, `.VObjGrp`, `.wdf`, `.adf`, `.cuf`, `.gdf`). The API writes UTF-8; 1257 shipped Japanese files are cp932.
- Values: numbers `G15`, dates `MM/dd/yyyy` (time dropped), booleans `True`/`False`, empty text `Empty`. The API uses the
  current culture (`2,5` in vi-VN); dialog files and this library use the invariant culture.
- Operators: string `==` `!=` `*CONTAINS*` `*NOTCONTAINS*` `*BEGINS*` `*NOTBEGINS*` `*ENDS*` `*NOTENDS*`; numeric `==` `!=`
  `<` `<=` `>` `>=` (`albl_LessThan` ...); date the same symbols with `albl_EarlierThan` ...; boolean `==` `!=`. Shipped
  files sometimes hold a translated label instead of the key; only the symbol matters.
- Structure: a root collection is wrapped in parentheses, a single root condition is not; each nested collection adds a
  pair; the last row of a group takes the operator of the outermost item ending there. Tekla writes an unclosed
  parenthesis for an empty group (the library skips empty groups) and ignores `IsEnable` of collections. Dialog files do
  not wrap the root.
- Tekla's reader (`new Filter(path, provider)`) cannot read `Custom*` (UDA) rows, not even its own.
- Limits of Tekla, not of Tekla.Structures.Filtering (which writes and reads back 5000-character values and 1500 rows
  without error): a value holds at most 1023 bytes in the file's encoding (the `Value` column of the dialog table
  `RuleTable` in TeklaStructures.aid is `char LENGTH=1023`, a 1024-byte buffer), and a filter at most 1000 rows (tested by
  the maintainer in Tekla). `FilterRow.MaxValueLength` / `FilterFile.MaxRows`: setting a longer value or an expression with more
  rows throws, saving checks the bytes; rows read from a file are not checked, so existing files still round-trip. The
  same table has `Category` / `Property` 127, `Condition` / `AndOr` 19 bytes and parentheses 0–3 per row (not enforced:
  shipped files hold translated labels, and the API writes deeper nesting).
- `.copt` / `TITLE_CLASSIFIER` (object level settings) is another block format, not supported yet.
- Evaluation, verified in Tekla 2025 on "New model". The harness `%TEMP%\tfiltest` wrote about 110 filters with the
  library and compared what Tekla kept with the properties of every object: `GetObjectsByFilterName` and
  `ObjectMatchesToFilter` on `.SObjGrp`, the parts of drawing views loaded with a `.vi.filter`, and assembly drawings
  with a `.ad.filter`. All agreed, over repeated runs. Findings: text is case-sensitive; `*` and `?` are wildcards;
  space-separated values are alternatives; numbers are single precision (`>= 3992.925` excludes 3992.92504); in a flat
  group And binds before Or; a row of another category passes (Part rows keep bolts, `Part.Name == X || Bolt.Size == 20`
  keeps every part); a missing numeric UDA passes `>` but not `<`, a missing text UDA equals `Empty`; disabled rows are
  skipped (all disabled = everything); `proPrimaryPart` 1/0, `proBOLT_TYPE` 0 = site, `proAssemblyType` 0 precast /
  1 cast in place / 2 steel, `proOBJECT_TYPE` takes the twelve label keys hard-coded in `bin\TeklaModel.dll` (`albl_Part` ...
  `albl_Reinforcement_bar`; `albl_Joint` matched a connection, not plug-ins), several separated by spaces, or the numbers
  of `TeklaStructuresDatabaseTypeEnum` (2 = part); `proNUMBERING_POSITION`
  matches the full `PART_POS` (`Concrete_G-1/4`). The selection methods read `.SObjGrp` only (not `.VObjGrp`) and re-read
  the file on each call. A 1000-row filter, 1023-character values and UTF-8 values (Vietnamese, Japanese) work. A
  `.gd.filter` does not affect views inserted through the API with their own view file.

## Conventions

- Verify every key against real files before adding a property; do not invent keys or guess types. When the meaning is
  a guess, say so in the XML comment or leave it out.
- One class per shared section, created with the prefix of each host.
- Keep the core free of Tekla references; anything that needs the Open API belongs in a separate project.
- Saving must throw rather than silently write `?` for characters missing from the code page.
- Add tests for every format rule; the samples tests must stay green on the environment and model folders, and the
  oracle tests on a machine with Tekla.
- Do not commit anything from `lib/` or files copied from the Tekla installation.
