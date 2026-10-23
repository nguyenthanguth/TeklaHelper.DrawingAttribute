# TeklaHelper.DrawingAttribute samples

A console application with a menu of examples. Each example is one file and reads from top to bottom.

```shell
dotnet build samples/TeklaHelper.DrawingAttribute.Samples -c Release
samples/TeklaHelper.DrawingAttribute.Samples/bin/Release/net48/TeklaHelper.DrawingAttribute.Samples.exe            # menu
samples/TeklaHelper.DrawingAttribute.Samples/bin/Release/net48/TeklaHelper.DrawingAttribute.Samples.exe offline    # examples 1-8
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
| 9 | [Online/CreateGADrawingExample.cs](TeklaHelper.DrawingAttribute.Samples/Online/CreateGADrawingExample.cs) | **Needs Tekla Structures 2025 with a model open.** Writes `.gd` and `.vi` files to the model's attributes folder, creates a GA drawing with two plan views (all parts / only the parts of one name) and counts the parts the Drawing API finds in each view |
| 10 | [Online/ApplyToOpenDrawingExample.cs](TeklaHelper.DrawingAttribute.Samples/Online/ApplyToOpenDrawingExample.cs) | **Needs a drawing open.** For the selected views (or all), writes a `.vi` from `standard.vi` with hidden lines and a profile filter, loads it with `view.Attributes.LoadAttributes`, and counts the parts before and after |

The online examples ask before they change anything. The answers can also be piped, one per line, e.g.
`printf 'COLUMN\ny\nn\n' | ...Samples.exe 9`; with no more input, a question is answered no (or with its default).
Example 9 lists the part names of the model and example 10 the profiles of each view, to choose a filter from.

## How the project references Tekla

- The Tekla Open API comes from the `Tekla.Structures.Drawing` and `Tekla.Structures.Model` NuGet packages with
  `ExcludeAssets=runtime`: they are used to compile, and no Tekla DLL is copied next to the executable.
- At run time [TeklaHelper.AssemblyResolver](https://www.nuget.org/packages/TeklaHelper.AssemblyResolver) loads the DLLs
  of the Tekla Structures 2025 installed on the machine. `TeklaAssemblyResolver.Register()` is the first line of `Main`,
  and the code that uses Tekla types is in methods that run after it (`Program.Run` is `NoInlining`).
- Tekla Structures 2025 needs an x64 .NET Framework 4.8 application.
- TeklaHelper.DrawingAttribute itself has no Tekla reference: examples 1 to 8 run on a machine without Tekla.

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
