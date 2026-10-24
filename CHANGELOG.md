# Changelog

## 2.0.0 - 2026-10-24

### Breaking changes
- A mark content key with a `MarkContent` property has no string property besides it: `Advanced.ContentString1` ...
  `ContentString4`, `PourContentString`, `PourBreakContentString`, `ChamferContentString` and
  `ReferenceObjectContentString` of the part, bolt, reinforcement, neighbour part, neighbour reinforcement, connection,
  surface treatment and dimension marks and of the associative note (the tables named below), the four common keys of
  every mark page, and `Advanced.ContentString` of `ViewLabelMark` are gone. Use the `MarkContent`
  property (`MainPartElements`, `PourElements`, `Elements`, ...), or `GetString` / `SetString` with the key for the raw
  text. Tables no dialog menu offers (`ContentString3` / `4` of the bolt and neighbour part marks, `ContentString2` ...
  of the connection, surface treatment and pour marks) keep their string properties.
- `SetMarkContent` (and every `MarkContent` property) writes content without elements as an empty text, as the dialogs
  store an empty table, instead of `<?xml ...><Mark></Mark>`; a key that already holds such XML keeps it.
- `CreateDefault()` of view and drawing files writes the part mark table `ContentString1` it sets (the assembly
  position) in the line layout of the mark dialogs instead of one line; the elements are the same.
- `CreateDefault()` and `SetDefaults()` leave the pour tables (`PourContentString`, `PourBreakContentString`) of the mark
  pages and files empty (`PourElements = new MarkContent();`), where the shipped files hold MATERIAL and "POUR BREAK";
  the pour mark page and `.pom` keep MATERIAL in `PourContentString`, the table their dialog edits.

### Mark content
- The tables of the other mark pages: `NeighborPartMark.MainPartElements` / `SecondaryPartElements`,
  `NeighborReinforcementMark.SingleElements` / `GroupElements` / `MergedMarkElements` / `MeshElements`,
  `ConnectionMark.Elements`, `SurfaceTreatmentMark.Elements` (checked in Tekla 2025, apart from the neighbouring
  secondary part and the merged mark), and `DimensionMark.PartElements` / `BoltElements` / `SurfaceTreatmentElements` /
  `ReinforcementElements` (from the dialog's menu and the shipped files).
- `PourElements`, `PourBreakElements`, `EdgeChamferElements` and `ReferenceObjectElements` on every mark page
  (`PourContentString`, `PourBreakContentString`, `ChamferContentString`, `ReferenceObjectContentString` as
  `MarkContent`; named after the key, the pour mark dialog edits `PourContentString`).
- `Elements` on the mark files of the drawing editor (`PartMarkAttributeFile`, `BoltMarkAttributeFile`,
  `ConnectionMarkAttributeFile`, `ReinforcementMarkAttributeFile`, `SurfaceTreatmentMarkAttributeFile`,
  `PourMarkAttributeFile`): their dialogs have one content table, not one per kind of object. `Elements` reads the key
  the dialog reads and writes the keys it writes (checked in Tekla 2025): `ContentString1` into all four for part and
  bolt marks, into 1, 2 and 4 for connection, reinforcement and surface treatment marks, `PourContentString` and
  `ContentString1` for pour marks.
- The table properties of 1.4.0 (`PartMark.MainPartElements` ..., `AssociativeNoteProperties.PartElements` ...) are
  generated now; their names and behaviour are the same.
- `SetDefaults()` builds the mark content defaults with `MarkContent` and assigns them through these properties instead
  of XML literals (`PourElements = new MarkContent(...)...;`, `MainPartElements = new MarkContent();` for an empty
  table).

## 1.4.0 - 2026-10-23

### Mark content tables
- The content tables of the mark pages by kind of object, as Tekla 2025 showed them: `PartMark.MainPartElements`,
  `SecondaryPartElements`, `SubAssemblyMainPartElements`, `SubAssemblySecondaryPartElements` (`ContentString1` ... `4`);
  `BoltMark.SiteBoltElements`, `WorkshopBoltElements`; `ReinforcementMark.SingleElements`, `GroupElements`,
  `MergedMarkElements`, `MeshElements` (the mesh table is `ContentString4`, the merged mark `ContentString3`); and
  `AssociativeNoteProperties.PartElements`, `BoltElements`, `SurfaceTreatmentElements`, `ReinforcementElements`,
  `ReferenceObjectElements`.
- The documentation no longer says that every mark file shows parts in `ContentString1` and reinforcement in
  `ContentString4`: that holds for the associative note (`.note`) only.

## 1.3.1 - 2026-10-23

### Fixed
- `MarkContent.Equals` (and so `SetMarkContent` / `Elements = ...` with content read from a file) threw
  `NullReferenceException` for content holding an element outside any `<MarkPart>`.
- `MarkContent.Equals` compares how the elements are grouped into `<MarkPart>`s, so `SetMarkContent` writes content that
  differs from the file's only in its grouping.
- Content read from a file with a raw `<` in a text (`<< Mark >>`) wrote new or changed text holding `<` followed by a
  letter raw, which made the value unreadable; only a `<` no name can follow stays raw now.
- One-line content (`MarkContentLayout.Compact`) whose text holds a line feed was read back in the line layout and lost
  the line feed at the start or end of the text.
- `MarkContent.TryParse(null, ...)` returns false instead of throwing.
- The comments of enum members found only in files name the pages whose files hold them (`LeaderLineArrow.Value16` /
  `Value17` come from the note dialog's `.note` files, not from the mark pages that share the enum).

### Documentation
- `ViewLabelMark.Elements` and `AttributeSection.GetMarkContent` return a copy: changing it changes nothing until it is
  assigned back.

## 1.3.0 - 2026-10-23

### Packaging
- The package targets `net48` and `net8.0` besides `netstandard2.0`: on .NET Framework 4.8 (Tekla plug-ins and tools) and
  .NET 8 it has no dependency, since they have the code pages built in; `System.Text.Encoding.CodePages` is a dependency
  of `netstandard2.0` only (its 8.0 differs from the 7.0 Tekla 2025 ships).

### Page files
- `ViewDimensioning.ViewFileExtension` (`.viewdim`), `DetailViewPage.GADrawingFileExtension` /
  `AssemblyDrawingFileExtension` (`.detail_label`) and the row files of `ViewLabelMark` (`.view_mark`, `.section_mark`,
  `.section_view_mark`, `.detail_mark`, `.detail_view_mark`), matched by key set with the shipped files.

### Mark and label content
- `MarkContent` (namespace `Marks`): the elements of a mark or a label row instead of the XML of the `ContentString`
  keys — property elements of each kind of mark (`MarkElementTypes`), user-defined attributes with unit and format,
  texts, line feeds, spaces, backspaces, symbols, frames, report templates — with `MarkFormat` (font, colour, height).
  `ViewLabelMark.Elements` for the label rows of views, section and detail marks; `AttributeSection.GetMarkContent` /
  `SetMarkContent` for any key. The sub-type of every element was read back through the Open API of Tekla 2025, and
  content written by the library in view labels, part marks, section and detail marks was read by it element by
  element. Content Tekla wrote is written back as it was, including two kinds of damage Tekla's own files hold.

### Drawing object properties
- A class for the file of every property dialog of the drawing editor (`DrawingObjects` namespace), generated from the
  decoded dialogs like the other pages, each with `CreateDefault()` and the legacy dialog prefix stripped on load:
  - new pages: `TextAttributeFile` (`.drtxt`), `SymbolAttributeFile` (`.sbl`), `LevelMarkAttributeFile` (`.lev`),
    `RevisionMarkAttributeFile` (`.rev`), `AssociativeNoteAttributeFile` (`.note`), `LineAttributeFile` (`.gln`),
    `ArcAttributeFile` (`.gar`), `PolylineAttributeFile` (`.gpl`), `CircleAttributeFile` (`.gci`),
    `RectangleAttributeFile` (`.grt`), `PolygonAttributeFile` (`.gpg`, clouds too), `TextFileAttributeFile` (`.fas`),
    `DwgAttributeFile` (`.fdg`), `ImageAttributeFile` (`.idg`), `HyperlinkAttributeFile` (`.fhl`),
    `LinkedDrawingAttributeFile` (`.ldr`), `PartAttributeFile` (`.dprt`), `SurfaceTreatmentAttributeFile` (`.dsrf`),
    `PourAttributeFile` (`.po`), `EdgeChamferAttributeFile` (`.dcha`), `ReinforcementAttributeFile` (`.drbr`),
    `ReinforcementMeshAttributeFile` (`.drmsh`), `DrawingWeldMarkAttributeFile` (`.wls`, weld marks drawn in the
    drawing), `SectionMarkAttributeFile` (`.cs`) and `DetailMarkAttributeFile` (`.detail`) with their label rows,
    `GADimensionSetupAttributeFile` (`.ga_dim_setup`);
  - files that hold the keys of a page of the view properties use that page's class: `PartMarkAttributeFile` (`.pm`),
    `BoltMarkAttributeFile` (`.sm`), `ConnectionMarkAttributeFile` (`.jm`), `ReinforcementMarkAttributeFile` (`.rm`),
    `SurfaceTreatmentMarkAttributeFile` (`.surfm`), `PourMarkAttributeFile` (`.pom`), `WeldMarkAttributeFile` (`.wel`),
    `WeldAttributeFile` (`.welo`), `BoltAttributeFile` (`.sc`), `PourBreakAttributeFile` (`.pbr`),
    `ReferenceObjectAttributeFile` (`.drmp`), `GridAttributeFile` (`.dgr`), `GridLineAttributeFile` (`.grl`);
  - `ReinforcementDimensionAttributeFile` (`.rdim`) holds the keys of a `.dim` file (`DimensionProperties` and the nine
    `DimensionMark`s).
- Every key of the files saved from the Tekla 2025 dialogs and of the shipped files has a property (except four rebar
  dimension mark keys of older versions in mark files, `RebarGroupMarkAttributes.CloseRebarDimMarkToFather` /
  `TaggedRebarDimMarkLocation`, which no dialog has). `CreateDefault()` writes the value most shipped files of the type
  hold, also for the files that use a view page's class. Checked in Tekla
  2025: a file of each type written by `CreateDefault()` with one value changed was loaded with each attribute class of
  the Open API that takes a file name, and the class showed the value (36 checks). The class summaries name the class
  that reads each file; `Mark.MarkAttributes(modelObject, name)` reads `.note`, `ReinforcementMeshAttributes` reads
  `.drbr` and `CloudAttributes` reads `.gpg`. Every file `CreateDefault()` writes was also loaded in its dialog
  (Load, then Save as) and kept its values.
- New enums for the menus of these dialogs: `WeldType`, `WeldContour` and `WeldFinish` (named after
  `Tekla.Structures.Model.BaseWeld`, whose values they store), `ArrowPosition` (`Arrowhead.ArrowPosition`, checked),
  `EmbeddedObjectScaling` and `ForeignObjectScaling` (the API's scaling options, checked), `TextArrowType`,
  `LineArrowType` (0 is `NoArrow`, checked), the leader line types of texts, symbols, level marks, revision marks and
  notes (named after the dialogs' pictures, captured from Tekla 2025), `GADimension*`, and others; the part and pour
  properties use the released
  `Representation`. `LeaderLineArrow` gains `Value16` and `Value17`, which shipped files hold.

## 1.2.0 - 2026-10-23

### Dimension properties
- `DimensionAttributeFile` (`.dim`): the file of the Dimension properties dialog of the drawing editor, generated from
  the decoded dialog like the other pages. `Properties` holds the tabs General, Appearance, Marks and Tags and the Place
  dialog; `PrefixMark`, `PostfixMark` and the seven tag marks are `DimensionMark` pages (content, frame, font, units,
  rotation). Every key of the shipped .dim files is typed; `CreateDefault()` writes the value most of them hold. Checked
  in Tekla 2025 by loading the file into dimension attributes and by creating a dimension with it.
- Enum members of the dialogs named by constants (`DIM_REINFORCEMENT_STRAIGHT`) get readable names (`Straight`).

### Documentation
- The fixed fields of the old filter pages (`vfilter.partname`, ...) stay without properties: Tekla 2025 ignores them
  (checked with a probe view); only the filter rows filter.

## 1.1.0 - 2026-10-23

### Filters
- `FilterObjectType` and `FilterObjectTypes`: the object type row of the filter dialog,
  `FilterProperties.Object.Type.IsNotEqual(FilterObjectType.ReinforcingBar)`, written with the label keys the dialog
  writes (`albl_Reinforcement_bar`, ...); several types in one row; `TryParse` reads a row's value back. The twelve keys
  are the list Tekla Structures 2025 evaluates, and every one was checked in Tekla with `==` and `!=`.

### Drawings
- Every page of the drawing properties is typed, generated from the decoded drawing dialogs like the view pages (tabs
  and groups as nested classes, a property per control, `SetDefaults()`, `SetAllEnabled`, `ExportTo` / `ImportFrom`):
  - `Views` (`dview.` / `dv.`): the "View..." page — view size, shortening, labels with the rows `LabelMarkA1` ...
    `LabelMarkA5`, anchor bolt plans, coordinate system, view names — next to the existing view creation properties;
  - `SectionView` (`dc.`, new `SectionViewPage`): scale and direction, cutting line, section mark with the rows
    `SectionMarkA1` ..., section view label with the rows `LabelMarkA1` ...;
  - `DetailView` (`DetailLabel.`, new `DetailViewPage`): detail view label, detail boundary, detail mark rows;
  - `Dimension` (`dd.`, new `DimensionPage`): dimension types, units, precision, format, placing, appearance;
  - `Dimensioning` (`Dimensioning.`, GA and multi drawings, new `DimensioningPage`) and `AssemblyDimensioning` (`dcd.`,
    assembly, single-part and cast unit drawings, new `AssemblyDimensioningPage`);
  - `CastUnitDrawingAttributeFile.CastUnitDefinitionMethod` (`bound_type`).
  Checked in Tekla 2025 with an assembly drawing created from a file with changed values. `CreateDefault()` writes the
  same files as 1.0.0, byte for byte. `Views.SetDefaults()` also sets the view creation switches, scale, shortening and
  label properties.
- The enums `BackgroundMask` and `ViewLabelSymbol` gain a member each for a value the drawing files hold.
- `DrawingAttributeFile.UseDetailedObjectLevelSettings`, and on `DrawingLayoutSection` the keys of the layout dialogs:
  `ListHiddenObjectsInTemplates`, `DrawingSize`, `TableLayout`, `ScaleChangeMode` (new enum), `PreferredWidth` /
  `PreferredHeight`, `AlignSectionViewsWithMainView`, `AlignEndViewsWithMainView`, `ExpandShortenedPartsToFit`,
  `IncludeSingleParts`, `SinglePartAttributeFile`; the values of `SizeSearchingMode` and `ProjectionType` documented.

### Documentation
- `PartMark.Advanced.Create` (`*.create` of the part mark page): Tekla 2025 ignores it when it loads a file; hide the
  part marks with `General.VisibilityOfMark.VisibilityInView` (verified in Tekla).
- How Tekla applies filter files, as checked in a running Tekla 2025: case-sensitive text, wildcards, single-precision
  numbers, And before Or, rows of other categories, missing user-defined attributes, disabled rows, coded values, and
  which files the selection methods of the Open API read (README, `FilterGroup`, `StringFilterProperty`,
  `NumericFilterProperty`, `FilterFileExtensions`).

## 1.0.0 - 2026-10-23

First release.

### Attribute files
- Read and write the key-value attribute files of Tekla Structures (`.vi`, `.gd`, `.md`, `.ad`, `.wd`, `.cud`, their
  `.more` files, page files such as `.vg` / `.vpm`, any other type through `GenericAttributeFile`) byte for byte:
  ANSI code pages, UTF-8 mark contents inside them, legacy dialog prefixes, hexadecimal values. Assigning a property its
  own value changes no byte.
- Loading parses every file of a set before applying any; saving encodes every file before writing any.

### Dialog pages
- Every page of the Tekla View properties dialog (Attributes, 9 mark pages, 11 object pages, Protection, Dimensioning)
  with its tabs, group boxes and a typed property per control, generated from the decoded Tekla 2026 dialog. The same
  page classes serve views and every drawing type under their own prefixes.
- `SetDefaults()` on every page and `CreateDefault()` on every file type: complete files, so that Tekla takes nothing
  from the `standard` file of the environment, with a setup that shows objects to the Open API.
- The View creation table of drawings (`GetViewTable`, `SetViewTable`, `SetViewAttributeFile`) and the Off / Auto / On view
  switches, checked in Tekla 2025.

### Filters
- Filter files as rows or as an expression tree, written like `Tekla.Structures.Filtering` byte for byte, with the
  generated catalog of filter properties. Values over 1023 bytes and filters over 1000 rows are rejected.

### Tools
- `tools/DialogDecoder` and `tools/DialogClassGenerator` decode the dialogs of a Tekla version and regenerate the pages;
  `tools/FilterCatalogGenerator` regenerates the filter catalog.
