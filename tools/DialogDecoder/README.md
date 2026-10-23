# DialogDecoder

Decodes the View properties dialog of Tekla Structures, and the pages of the drawing properties dialogs it does not
have, from the compiled dialog resources into one JSON file per page (`tools/DialogClassGenerator/Dialogs/*.json`). `tools/DialogClassGenerator` turns those files into the page classes of
the library (`src/TeklaHelper.DrawingAttribute/Pages/Generated`, and `DrawingObjects/Generated` for the drawing object
files). Run it again for another Tekla version.

Needs Python 3 and a shell (`sh` from Git for Windows). No Python packages.

## Where the dialogs come from

Tekla Structures keeps its dialogs in `<Tekla folder>\bin\TeklaStructures.aid`, e.g.
`D:\Tekla\Structures\2025.0\bin\TeklaStructures.aid`. In 2025 `TeklaStructuresObjects.aid` next to it holds an
identical copy of the View properties dialog. The decoder reads:

- the dialog `gratViewAttributes`: the tree on the left, one panel per page on the right (`PanelPart`,
  `PanelPartMark`, ...), with tabs, group boxes, controls and the sub-dialogs their buttons open;
- the older dialogs with the same keys (`gr_vp_attrib`, `gr_vpm_attrib`, ...), for keys the 2025 panels no longer show;
- the English labels (`string_utf8` objects, `albl_*` names, `enu` entries);
- for the drawing pages (`DRAWING_PAGES` in `extract.py`): per page the newest dialog or panel that shows it (the GA
  dialogs `gr_ga_view_attrib`, `gr_detail_symbol_attrib`, `gr_dd_attrib`, `gr_gdcd_attrib`; the panels of
  `gratAssemblyDrawingAttributes`; `gr_adcd_attrib`) and other dialogs with the same keys for the keys it does not show.
  Their controls keep the tab and group of their own dialog (`dialogPath`). The rows of label texts
  (`ViewLabelMarkA1.`, `SectionMark.SectionMarkA1.`, `DetailMarkA1.`, ...) are listed under `labelMarks`; they are the
  structure of the page `ViewLabelMark`;
- for the files of drawing objects (`FILE_PAGES`): the dialog of the object (`gr_dim_attrib` for `.dim`, the mark
  dialog `gr_dim_mark_attrib`, `gr_text_attrib` for `.drtxt`, ...), with the keys of a sample file (`--file`:
  `common/system/standard.dim` and the files `OBJECT-TEST.<ext>` saved from each dialog of Tekla 2025) and of every file
  of that type under the folders after `--file-folders`. A member without its struct (`height`) takes the struct of the
  unique file key ending with it (`FontAttr.height`); on the pages of `STRUCT_PREFIX_PAGES`, a widget's `STRUCT`
  (`gr_part_hatch_at_t`) gives the prefix its members' file keys share, between equal ones the one spelling the
  structure (`PartHatch.`). Dialogs of the same structure are each other's other sources (`TEXT_SIBLINGS`, ...); a
  source may name a panel of the View properties with a fourth element, the prefix its members carry (`vpm.`).

The `.aid` format ("RES 1.1") is a header, a pool of NUL-terminated strings and a token stream of objects and
attributes; `aid.py` describes and parses it. In a control, `MEMBER` is the key in the attribute file, `VALUEINT` the
value an option stores, and `CALLBACKPARAMETER` names the check box (`*_en`) in front of the control.

What is decoded exactly (keys, option values, the tree) and what is inferred (which text label belongs to which control)
is recorded per control (`labelSource`, `confidence`), and the generator keeps the difference in its XML comments.

## Files

| File | Does |
| --- | --- |
| `run.sh` | The whole decoding: `extract.py` then `annotate.py`. |
| `aid.py` | Parser of the `.aid` format. `python aid.py file.aid` prints statistics. |
| `extract.py` | Writes one JSON file per page: tabs, groups, controls, options, sub-dialogs, keys of older dialogs, keys only seen in files. `PAGES` maps each panel to its page name, `.vi` prefix and older dialogs; `TREE_KEYS` the tree check boxes. |
| `annotate.py` | Adds the Tekla Open API enum of each option list (`ENUMS`, from `Tekla.Structures.Drawing` 2025; `DIMENSION_ENUMS` for the dimension pages) and the value statistics of real `.vi` files, or of the drawing files for the drawing pages (`samples`). |
| `compare.py` | Compares two decodings page by page: keys added and removed, every other change. |
| `validate.py` | Checks the option values of the JSON files against real `.vi` files. |
| `condensed.py`, `dump.py`, `show.py` | Text views for investigation: one dialog with English labels, the whole `.aid` as a tree, one page JSON. |

## Decoding another Tekla version

1. **Install the version** and note its folder, e.g. `C:\TeklaStructures\2026.0`.
2. **Save sample files from its dialogs** into the `attributes` folder of a model:
   - `VIEW-TEST.vi`: View properties dialog, Save as `VIEW-TEST` (one file with every page). Optional but useful:
     open each page and save it as `TEST` too, which writes `TEST.vi` plus the page files (`TEST.vp`, `TEST.vpm`, ...).
     The decoder needs `TEST.vi`; a copy of `VIEW-TEST.vi` saved as `TEST.vi` works.
   - `DRAWING-TEST.gd`: GA drawing properties, Save as `DRAWING-TEST`, and `test-ad.ad`: assembly drawing properties,
     Save as `test-ad`. The decoder takes the keys of the drawing pages from them (`--drawing`), the generator value
     types and drawing-only keys.
3. **Decode into a new folder**, not over the current files:

   ```shell
   TEKLA_ROOT="C:/TeklaStructures/2026.0" \
   TEKLA_SAMPLE_ATTRIBUTES="D:/Tekla/Models/New model/attributes" \
   TEKLA_MODELS="D:/Tekla/Models" \
   sh tools/DialogDecoder/run.sh /tmp/dialogs-2026
   ```

   The table it prints lists, per page, the keys found in the panel, in sibling pages, in older dialogs, and the keys of
   the sample files no control writes (`unmapped`). A `WARNING` about panels means the version added or removed a
   page: add it to `PAGES` (and `TREE_KEYS`) in `extract.py` and to `tools/DialogClassGenerator/PageSpecs.cs`.
4. **Compare** with the current version and read the changes:

   ```shell
   python tools/DialogDecoder/compare.py tools/DialogClassGenerator/Dialogs /tmp/dialogs-2026
   ```

5. **Take the new files and regenerate** the page classes (command in CLAUDE.md), then review the diff of
   `Pages/Generated` and `DrawingObjects/Generated`: renamed properties and enums break users of the library.
6. **Check** with the samples of that version:

   ```shell
   dotnet build TeklaHelper.DrawingAttribute.slnx -c Release -warnaserror
   TEKLA_VIEW_DIALOG_SAMPLES="<attributes folder>" dotnet test TeklaHelper.DrawingAttribute.slnx -c Release
   ```

   `annotate.py` checks the option lists against the `ENUMS` of Tekla 2025; if the Open API enums changed, update
   `ENUMS` from the new `Tekla.Structures.Drawing.dll` (reflection, e.g. with a throw-away net48 console).

The files in `tools/DialogClassGenerator/Dialogs` follow the newest Tekla version (currently 2026, decoded with the 2025
dialog files as samples); see CLAUDE.md for the exact command.

Running the decoder on the current files reproduces them except the value statistics, which change whenever the sample
folders do (`compare.py` ignores them unless `--samples` is given).

## Known corrections

`extract.py` corrects what the `.aid` gets wrong, checked against the files saved from the dialog:

- a check box whose member lacks the struct of its control (`LineType_en` for `RebarGroupMarkAttributes.LineType` on the
  neighbor reinforcement mark page) gets the key the files hold (`enableKeyNote`);
- a check box that names another control's key is reported (`aidNote`).

Correct the decoder rather than the JSON files, so that the next decoding keeps the correction.
