# Changelog

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
