# Changelog

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
