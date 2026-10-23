# TeklaHelper.DrawingAttribute samples

A console application with a menu of examples. Each example is one file and reads from top to bottom.

```shell
dotnet build samples/TeklaHelper.DrawingAttribute.Samples -c Release
samples/TeklaHelper.DrawingAttribute.Samples/bin/Release/net48/TeklaHelper.DrawingAttribute.Samples.exe            # menu
samples/TeklaHelper.DrawingAttribute.Samples/bin/Release/net48/TeklaHelper.DrawingAttribute.Samples.exe offline    # examples 1-20
samples/TeklaHelper.DrawingAttribute.Samples/bin/Release/net48/TeklaHelper.DrawingAttribute.Samples.exe 1 --out C:\temp\attributes
```

The offline examples write to `bin/Release/net48/output/attributes` unless you pass `--out`. Open the files next to the
code that wrote them.

| # | File | Shows |
| --- | --- | --- |
| 1 | [Offline/ViewPropertiesExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/ViewPropertiesExample.cs) | A `.vi` built page by page like the View properties dialog: Attributes tabs, Objects (part, bolt, neighbor part, grid, weld), Marks (part mark, bolt mark), Filter; saved as `.vi` + `.vi.filter` |
| 2 | [Offline/FilterExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/FilterExample.cs) | Filter expressions with groups, numbers, dates and user-defined attributes; the neighbor part filter; reading rows and the expression tree back; editing rows; stand-alone `.vf` / `.SObjGrp` files |
| 3 | [Offline/GADrawingExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/GADrawingExample.cs) | GA drawing properties: titles, layout, view creation, object and mark pages, protection, `.gd.more` user-defined attributes, `.gd.filter` and `.gd.npfilter` |
| 4 | [Offline/AssemblyDrawingExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/AssemblyDrawingExample.cs) | Assembly, single-part and cast unit drawings from `CreateDefault()` (assembly prefixes `dv.`, `grid.`, `dnp.`, ...); copying the part mark page of a view into each of them; the View creation table: which views, and the view properties file they use |
| 5 | [Offline/LoadAndModifyExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/LoadAndModifyExample.cs) | Loading an existing `.vi`, reading its pages, checking that an unchanged file is written back byte for byte, changing a few values |
| 6 | [Offline/PageFilesExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/PageFilesExample.cs) | The files of single pages (`.vp`, `.vpm`, `.vg`, `.gdgr`, ...) for the dialogs: export, import into a view or a drawing |
| 7 | [Offline/UnmodelledKeysExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/UnmodelledKeysExample.cs) | Keys without a typed property: the `Advanced` groups, `GetSection` and typed `Get*` / `Set*` by key, `GenericAttributeFile` for any extension |
| 8 | [Offline/ModifyCheckBoxesExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/ModifyCheckBoxesExample.cs) | The check boxes of the dialogs (`*_en`): `SetAllEnabled`, `SetEnabled` |
| 9 | [Offline/DrawingObjectsExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/DrawingObjectsExample.cs) | Property files of drawing editor objects: a text (`.drtxt`), a line (`.gln`), and the part mark page of a view as a part mark file (`.pm`) |
| 10 | [Offline/MarkContentExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/MarkContentExample.cs) | The "Elements in mark" (`ContentString`) from typed elements: a view label; the tables of the part mark (main, secondary, sub-assembly parts), bolt mark (site, workshop) and reinforcement mark (single, group, mesh); an associative note (`.note`); symbols, templates, units; changing content that is already in a file |
| 11 | [Offline/DimensionPropertiesExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/DimensionPropertiesExample.cs) | Dimension properties (`.dim`): dimension type, units, precision, format, placing, text and arrows, prefix / postfix, exaggeration, tags, the dimension marks |
| 12 | [Offline/GADrawingPagesExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/GADrawingPagesExample.cs) | The pages of the GA drawing properties: layout, view, section view (size, cutting line, mark, label), detail view, dimension, automatic dimensioning, protection |
| 13 | [Offline/FilterObjectTypesExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/FilterObjectTypesExample.cs) | Filters on the object type (`FilterObjectType`: no reinforcement, parts or bolts), model selection (`.SObjGrp`) and model view (`.VObjGrp`) filters, the filters of every drawing type |
| 14 | [Offline/ModelObjectPropertiesExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/ModelObjectPropertiesExample.cs) | Model objects in a drawing: part (`.dprt`), bolt (`.sc`), weld (`.welo`), reinforcement (`.drbr`), mesh (`.drmsh`), surface treatment (`.dsrf`), pour (`.po`), pour break (`.pbr`), grid (`.dgr`), grid line (`.grl`), edge chamfer (`.dcha`), reference object (`.drmp`) |
| 15 | [Offline/GraphicObjectsExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/GraphicObjectsExample.cs) | Graphic objects: line (`.gln`), arc (`.gar`), polyline (`.gpl`), circle (`.gci`), rectangle (`.grt`), polygon and cloud (`.gpg`): line types, arrows, hatches |
| 16 | [Offline/AnnotationsExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/AnnotationsExample.cs) | Annotations: text (`.drtxt`), symbol (`.sbl`), level mark (`.lev`), revision mark (`.rev`), drawing weld mark (`.wls`), section mark (`.cs`), detail mark (`.detail`) |
| 17 | [Offline/EmbeddedObjectsExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/EmbeddedObjectsExample.cs) | Embedded and linked objects: DWG (`.fdg`), image (`.idg`), hyperlink (`.fhl`), linked drawing (`.ldr`), text file (`.fas`) |
| 18 | [Offline/SharedStyleExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/SharedStyleExample.cs) | One style for every host: a part mark and a part page copied (`CopyFrom`) into a view, a GA drawing, the assembly family, a `.pm` file and a `.vpm` page file |
| 19 | [Offline/CodePageExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/CodePageExample.cs) | Code pages: Japanese (932) and Korean (949) files, UTF-8 mark content inside them, and the error for a character the code page cannot hold |
| 20 | [Offline/InspectAndCompareExample.cs](TeklaHelper.DrawingAttribute.Samples/Offline/InspectAndCompareExample.cs) | Compare two attribute files key by key; list the key-value files of a folder with their code page and number of keys (`GenericAttributeFile`) |
| 21 | [Online/CreateGADrawingExample.cs](TeklaHelper.DrawingAttribute.Samples/Online/CreateGADrawingExample.cs) | **Needs Tekla Structures 2025 with a model open.** Writes `.gd` and `.vi` files to the model's attributes folder, creates a GA drawing with two plan views (all parts / only the parts of one name) and counts the parts the Drawing API finds in each view |
| 22 | [Online/ApplyToOpenDrawingExample.cs](TeklaHelper.DrawingAttribute.Samples/Online/ApplyToOpenDrawingExample.cs) | **Needs a drawing open.** For the selected views (or all), writes a `.vi` from `standard.vi` with hidden lines and a profile filter, loads it with `view.Attributes.LoadAttributes`, and counts the parts before and after |
| 23 | [Online/PlaceAnnotationsExample.cs](TeklaHelper.DrawingAttribute.Samples/Online/PlaceAnnotationsExample.cs) | **Needs a drawing open.** Inserts a text, a line, a level mark and a dimension set into a view with `.drtxt`, `.gln`, `.lev` and `.dim` files the library wrote, and reads their properties back |
| 24 | [Online/MarkContentInDrawingExample.cs](TeklaHelper.DrawingAttribute.Samples/Online/MarkContentInDrawingExample.cs) | **Needs a drawing open.** Writes a `.vi` whose mark tables name each kind of object, inserts a copy of a view with it, and reads the content of every mark back (the marks a view already has keep their own content) |
| 25 | [Online/RestyleDrawingObjectsExample.cs](TeklaHelper.DrawingAttribute.Samples/Online/RestyleDrawingObjectsExample.cs) | **Needs a drawing open.** Gives every part, bolt and grid of a view the properties of a `.dprt`, `.sc` and `.dgr` file (`drawingObject.Attributes = new Part.PartAttributes(name)` + `Modify`) |
| 26 | [Online/CreateAssemblyDrawingExample.cs](TeklaHelper.DrawingAttribute.Samples/Online/CreateAssemblyDrawingExample.cs) | **Needs a part or assembly selected in the model.** Creates an assembly drawing from a generated `.ad` whose View creation table names a generated `.vi`, lists its views, and offers to delete it |

The online examples ask before they change anything, and offer to undo what they did (close the drawing without
saving, delete the drawing they created). The answers can also be piped, one per line, e.g.
`printf 'COLUMN\ny\nn\n' | ...Samples.exe 21`; with no more input, a question is answered no (or with its default).
Example 21 lists the part names of the model and example 22 the profiles of each view, to choose a filter from.

## How the project references Tekla

- The Tekla Open API comes from the `Tekla.Structures.Drawing` and `Tekla.Structures.Model` NuGet packages with
  `ExcludeAssets=runtime`: they are used to compile, and no Tekla DLL is copied next to the executable.
- At run time [TeklaHelper.AssemblyResolver](https://www.nuget.org/packages/TeklaHelper.AssemblyResolver) loads the DLLs
  of the Tekla Structures 2025 installed on the machine. `TeklaAssemblyResolver.Register()` is the first line of `Main`,
  and the code that uses Tekla types is in methods that run after it (`Program.Run` is `NoInlining`).
- Tekla Structures 2025 needs an x64 .NET Framework 4.8 application.
- TeklaHelper.DrawingAttribute itself has no Tekla reference: examples 1 to 20 run on a machine without Tekla.

## Things to know when you use the files with Tekla

- Tekla reads an attribute file once per session and keeps it by name. A file written again under a name Tekla already
  loaded is not read again until Tekla restarts, so the online examples use a new name for every run.
- Tekla finds attribute files by name in the model's `attributes` folder first, then in the project, firm and system
  folders.
- `view.Attributes.LoadAttributes(name)` replaces every setting of the view with the file's: start from a complete file
  (one saved from the dialog) and change what you need.
- The check boxes of the dialogs (`*_en`) only matter for the dialog's Modify button; the Open API applies every value.
- A filter file without rows would make Tekla show no objects at all, so the library writes no filter file when the
  filter has no rows, and deletes an old one of the same name.
