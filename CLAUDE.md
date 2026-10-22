# CLAUDE.md

C# library that models Tekla Structures drawing attribute files (`.gd`, `.ad`, `.wd`, `.cud`, `.md`, `.vi`, their `.more`
files, sub-dialog files such as `.vg` / `.vpm`) and filter files (`.vf`, `.vi.filter`, `.gdf`, `.SObjGrp`, ...) as
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
TEKLA_VIEW_DIALOG_SAMPLES="D:/Tekla/Models/New model/attributes" dotnet test TeklaHelper.DrawingAttribute.slnx -c Release --filter "FullyQualifiedName~SubDialogFiles|FullyQualifiedName~GeneratedPagesTests"
```

The dialog pages (`Pages/Generated/*.g.cs`) are generated (the generator compiles only the format classes of the library,
so it runs even when the library does not build until the pages are regenerated); change `tools/DialogClassGenerator` (rules in `Program.cs`,
pages in `PageSpecs.cs`, enum renames in `EnumRenames`) or its input, regenerate, and review the diff. Input: the dialog
definitions decoded from Tekla's `.aid` files (`tools/DialogClassGenerator/Dialogs/*.json`, made with
`tools/DialogDecoder`; its README explains the `.aid` format and how to decode another Tekla version, and `compare.py`
diffs two decodings) and two files saved from the dialogs, for value types and keys the dialog does not show. Fix
decoding errors in `extract.py`, never by hand in the JSON files:

The JSON files are decoded from the **latest** Tekla dialog (decision: follow the newest version; older versions are
expected to cope). Currently Tekla 2026 (`TEKLA_AID=C:/Users/InuYasha/Downloads/test/2026/TeklaStructures.aid`, Tekla
2026 is not installed here) with the 2025 dialog files as samples. Keys an older dialog lacks are listed in
`KeysAddedIn` (generator), which notes the version in the XML comment.

```shell
dotnet run --project tools/DialogClassGenerator -c Release -- tools/DialogClassGenerator/Dialogs "D:/Tekla/Models/New model/attributes/VIEW-TEST.vi" "D:/Tekla/Models/New model/attributes/DRAWING-TEST.gd" src/TeklaHelper.DrawingAttribute/Pages/Generated
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
  the files the Open API reads (`TEST.gd` was replaced by `DRAWING-TEST.gd`). `test-ad.ad` (+ `.ad.more`) was saved
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

`src/TeklaHelper.DrawingAttribute` targets `netstandard2.0` and has no Tekla reference (decided: the filter part of
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
  of each host (`ViewPrefix`, `GADrawingPrefix`, `AssemblyDrawingPrefix`, `*FileExtension`).
- `Pages/` hand-written: `PageHost` (View / GADrawing / AssemblyDrawing → prefixes), `MarkPages` and `ObjectPages`
  (the Marks and Objects nodes; `ObjectPages.ReferenceObject` is null in the assembly family).
- `Sections/FilterSection` — `vfilter.` / `dfilter.` keys + the rows of the companion filter file.
- `Drawings/` — `DrawingAttributeFile` base with `Layout`, `Views`, `Protection`, `Marks`, `Objects`, titles and
  `UserAttributes`; `GADrawingAttributeFile`, `MultiDrawingAttributeFile` (GA prefixes) and `AssemblyDrawingAttributeFile`,
  `SinglePartDrawingAttributeFile`, `CastUnitDrawingAttributeFile` (assembly prefixes); `DrawingLayoutSection`,
  `DrawingViewCreationSection`.
- `Views/ViewProperties` — `.vi` organised like the View properties dialog tree: `Attributes` (`ViewAttributes`, top-level
  keys, + `LabelMarkA1` ... `A5` in `Views/ViewAttributes.cs`), `Filter` / `NeighborPartFilter` (`FilterSection`:
  `vfilter.` / `vnp_filter.` keys + rows in `.vi.filter` / `.vi.npfilter`), `Protection`, `Dimensioning`, `Marks` (9),
  `Objects` (11). Saves only `.vi`, `.vi.filter`, `.vi.npfilter` — what the dialog's Save button writes and the Open
  API reads.
- `Filters/` — `FilterFile` (rows, byte-preserving), `FilterRow`, expression tree (`FilterExpression`, `FilterCondition`,
  `FilterGroup`), `FilterOperator`, typed `FilterProperty` subclasses, `FilterProperties` (generated catalog of
  `Tekla.Structures.Filtering.Categories`), `FilterFileExtensions`.

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
- Colours: `DrawingColor` values (153 black ... 165 magenta). Files saved from the dialogs often hold small numbers
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

## Conventions

- Verify every key against real files before adding a property; do not invent keys or guess types. When the meaning is
  a guess, say so in the XML comment or leave it out.
- One class per shared section, created with the prefix of each host.
- Keep the core free of Tekla references; anything that needs the Open API belongs in a separate project.
- Saving must throw rather than silently write `?` for characters missing from the code page.
- Add tests for every format rule; the samples tests must stay green on the environment and model folders, and the
  oracle tests on a machine with Tekla.
- Do not commit anything from `lib/` or files copied from the Tekla installation.
