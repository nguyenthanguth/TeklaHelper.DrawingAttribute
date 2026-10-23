"""Post-process the page JSON files in place:
- apiEnum / apiName: Tekla.Structures.Drawing 2025 enum whose values equal (or are offset from) the stored values
  (enum values read by reflection from Tekla.Structures.Drawing.dll of Tekla 2025, written out in ENUMS below);
- samples: value counts of every option key in real .vi files (drawing pages: .gd, .md, .ad, .wd and .cud files under
  both drawing prefixes), values outside the menu listed separately.

python annotate.py <dialogs dir> <folder with .vi files> ...
"""
import sys, os, re, json, glob, collections

OD = collections.OrderedDict

ENUMS = {
    'DrawingColors': {130: 'Gray30', 131: 'Gray50', 132: 'Gray70', 133: 'Gray90', 152: 'Invisible', 153: 'Black',
                      154: 'NewLine1', 155: 'NewLine2', 156: 'NewLine3', 157: 'NewLine4', 158: 'NewLine5', 159: 'NewLine6',
                      160: 'Red', 161: 'Green', 162: 'Blue', 163: 'Cyan', 164: 'Yellow', 165: 'Magenta'},
    'LineTypesEnum': {0: 'UndefinedLine', 1: 'SolidLine', 2: 'DashedLine', 3: 'SlashedLine', 4: 'DashDot', 5: 'DottedLine',
                      6: 'DashDoubleDot', 7: 'SlashDash'},
    'Part.Representation': {1: 'Outline', 2: 'Symbol', 4: 'WorkshopForm', 8: 'Exact', 16: 'BoundingBox', 32: 'BaseBox',
                            64: 'SolidWithPartialProfile'},
    'Surfacing.Representation': {1: 'Outline', 2: 'Symbol', 4: 'WorkshopForm', 8: 'Exact', 16: 'BoundingBox', 32: 'BaseBox'},
    'Bolt.Representation': {1: 'Solid', 2: 'Symbol', 4: 'Symbol2', 5: 'Symbol3', 7: 'DINSymbol', 8: 'UserDefined', 9: 'ExactSolid'},
    'FrameTypes': {0: 'None', 1: 'Rectangular', 2: 'Line', 3: 'Round', 4: 'Circle', 5: 'Diamond', 6: 'Hexagon', 7: 'Triangle', 8: 'Sharpened'},
    'Units': {0: 'Automatic', 1: 'Millimeters', 2: 'Centimeters', 3: 'Meters', 4: 'FeetAndInches', 5: 'CentimetersOrMeters', 6: 'Inches', 7: 'Feet'},
    'FormatTypes': {0: 'Automatic', 1: 'WholeNumber', 2: 'OneDecimalIfValidDecimal', 3: 'OneDecimal', 4: 'TwoDecimalsIfValidDecimals',
                    5: 'TwoDecimals', 6: 'ThreeDecimalsIfValidDecimals', 7: 'ThreeDecimals', 8: 'Fractional'},
    'TextAlignment': {0: 'Left', 1: 'Center', 2: 'Right', 3: 'LeaderLine'},
    'Weld.Representation': {1: 'Outline', 2: 'Path'},
    'ReinforcementBase.ReinforcementRepresentationTypes': {1: 'SingleLine', 2: 'DoubleLine', 3: 'Filled', 4: 'Stick', 5: 'OutLine',
                                                           6: 'DoubleLineFilledEnds', 7: 'SingleLineFilledEnds', 8: 'OutlineIgnoreHoles'},
    'ReinforcementBase.ReinforcementVisibilityTypes': {1: 'All', 2: 'First', 3: 'Last', 4: 'FirstAndLast', 5: 'OneInTheMiddle',
                                                       6: 'TwoInTheMiddle', 7: 'Customized'},
    'ReinforcementBase.StraightEndSymbolTypes': {1: 'NoSymbol', 2: 'HooksOnInnerSide', 3: 'HooksOnOuterSide',
                                                 4: 'HookOnReinforcementStartOuterSide', 5: 'HookOnReinforcementStartInnerSide', 6: 'FullHooks'},
    'ReinforcementBase.HookedEndSymbolTypes': {1: 'NoHook', 2: 'HalfHook', 3: 'FullHook'},
    'View.LabelLineLengthType': {1: 'Minimal', 2: 'Custom'},
    'View.ShorteningCutPartType': {1: 'BothDirections', 2: 'X_Direction', 3: 'Y_Direction'},
    # DimensionSetBaseAttributes, for the Dimension page of the drawing properties
    'DimensionSetBaseAttributes.DimensionTypes': {1: 'Relative', 2: 'Absolute', 3: 'RelativeAndAbsolute', 4: 'USAbsolute', 8: 'Elevation',
                                                  16: 'USAbsolute2', 35: 'AbsoluteWithShortRelatives', 99: 'AbsoluteWithAllRelativesAbove'},
    'DimensionSetBaseAttributes.DimensionValuePrecisions': {1: 'Whole', 2: 'OnePerTwo', 3: 'OnePerThree', 4: 'OnePerFour', 8: 'OnePerEight',
                                                            10: 'OnePerTen', 16: 'OnePerSixteen', 32: 'OnePerThirtytwo', 100: 'OnePerHundred',
                                                            1000: 'OnePerThousand', -100: 'TenPerOne', -50: 'FivePerOne', -25: 'TwoAndHalfPerOne'},
    'DimensionSetBaseAttributes.DimensionValueFormats': {0: 'NoDecimals', 1: 'OneOptionalDecimal', 2: 'OneDecimal', 3: 'TwoOptionalDecimals',
                                                         4: 'TwoDecimals', 5: 'ThreeOptionalDecimals', 6: 'ThreeDecimals', 7: 'RationalPart',
                                                         8: 'SuperscriptEnding'},
    'DimensionSetBaseAttributes.FrameTypes': {0: 'None', 1: 'Rectangle', 2: 'Underline', 3: 'RoundedRectangle', 8: 'SharpenedRectangle'},
    'DimensionSetBaseAttributes.DimensionTextPlacings': {0: 'AboveDimensionLine', 1: 'OnDimensionLine'},
    'DimensionSetBaseAttributes.ShortDimensionTypes': {0: 'Inside', 1: 'Outside'},
    'DimensionSetBaseAttributes.ExtensionLineTypes': {0: 'No', 1: 'Yes', 2: 'NotOnGridlines'},
    'DimensionSetBaseAttributes.ExaggerationDirections': {1: 'LeftDown', 2: 'RightUp', 3: 'Both'},
    'DimensionSetBaseAttributes.ExaggerationOrigins': {1: 'NearDimensionLine', 2: 'NearMeasuredObject'},
    'DimensionSetBaseAttributes.CombineFormats': {0: 'Off', 1: 'NumberOfItemsTimesLength', 2: 'NumberOfItemsTimesLengthEqualsResult'},
}

# Dimension pages (dd., dcd.): key → (enum, API value - stored value), checked value by value against the menus
DIMENSION_ENUMS = {
    'dim_type': ('DimensionSetBaseAttributes.DimensionTypes', 0),
    'dim_type_x': ('DimensionSetBaseAttributes.DimensionTypes', 0),
    'dim_precision': ('DimensionSetBaseAttributes.DimensionValuePrecisions', 0),
    'AnglePrecision': ('DimensionSetBaseAttributes.DimensionValuePrecisions', 0),
    'dim_format': ('DimensionSetBaseAttributes.DimensionValueFormats', 0),
    'AngleFormat': ('DimensionSetBaseAttributes.DimensionValueFormats', 0),
    'dim_frame': ('DimensionSetBaseAttributes.FrameTypes', 0),
    'dim_text_on_line': ('DimensionSetBaseAttributes.DimensionTextPlacings', 0),
    'short_dim_type': ('DimensionSetBaseAttributes.ShortDimensionTypes', 0),
    'ShortExtensionLine': ('DimensionSetBaseAttributes.ExtensionLineTypes', 0),
    'ExaggerationDirection': ('DimensionSetBaseAttributes.ExaggerationDirections', 1),
    'ExaggerationOrigin': ('DimensionSetBaseAttributes.ExaggerationOrigins', 1),
    'dim_units': ('Units', 0),
    'DimTypeInXDirection': ('DimensionSetBaseAttributes.DimensionTypes', 0),
    'combine_bolt_dim': ('DimensionSetBaseAttributes.CombineFormats', 0),
    'combine_part_dimensions': ('DimensionSetBaseAttributes.CombineFormats', 0),
}

def enum_for(page, key, ctl):
    k = key.split('.')[-1]
    t = ctl['type']
    if page in ('Dimensioning', 'AssemblyDimensioning') and key in DIMENSION_ENUMS:
        return DIMENSION_ENUMS[key]
    if t == 'color':
        return 'DrawingColors', 0
    if t == 'linetype':
        return 'LineTypesEnum', 0
    if k in ('draw_as',) and page in ('PartObject', 'NeighborPartObject', 'PourObject'):
        return 'Part.Representation', 0
    if k == 'draw_as' and page == 'SurfaceTreatmentObject':
        return 'Surfacing.Representation', 0
    if (k == 'draw_as' and page == 'BoltObject') or k == 'BoltDrawAs':
        return 'Bolt.Representation', 0
    if k in ('frame_type', 'text_frame'):
        return 'FrameTypes', 0
    if k == 'Unit' and 'UnitAttr' in key:
        return 'Units', 0
    if k == 'Format' and 'UnitAttr' in key:
        return 'FormatTypes', 0
    if k == 'Alignment':
        return 'TextAlignment', 0
    if k == 'DrawAs' and page == 'WeldObject':
        return 'Weld.Representation', 0
    if k == 'representation' and ('BarAttr' in key or 'MeshAttr' in key):
        return 'ReinforcementBase.ReinforcementRepresentationTypes', 1
    if k in ('visibility_in_groups', 'visibility_longitudinal', 'visibility_crossing'):
        return 'ReinforcementBase.ReinforcementVisibilityTypes', 0
    if k == 'straight_symbol':
        return 'ReinforcementBase.StraightEndSymbolTypes', 0
    if k == 'hooked_symbol':
        return 'ReinforcementBase.HookedEndSymbolTypes', 0
    if k == 'ViewLabelLineLengthType':
        return 'View.LabelLineLengthType', 1
    if k == 'cut_on' and page == 'Attributes':
        return 'View.ShorteningCutPartType', 0
    return None, 0

def walk(n):
    for c in n.get('controls', []):
        yield c
    for g in n.get('groups', []):
        yield from walk(g)
    for t in n.get('tabs', []):
        yield from walk(t)

def main():
    ddir = sys.argv[1]
    files = []
    for a in sys.argv[2:]:
        files += glob.glob(os.path.join(a, '**', '*.vi'), recursive=True)
    vals = collections.defaultdict(collections.Counter)
    drawing_files = []
    for a in sys.argv[2:]:
        for ext in ('gd', 'md', 'ad', 'wd', 'cud'):
            drawing_files += glob.glob(os.path.join(a, '**', '*.' + ext), recursive=True)
    drawing_vals = collections.defaultdict(collections.Counter)
    for f in drawing_files:
        try:
            data = open(f, 'rb').read()
        except OSError:
            continue
        for line in data.split(b'\r\n'):
            k, _, v = line.partition(b' ')
            k = re.sub(r'^(gdraw|mdraw|adraw|wdraw|cudraw)_dial\.', '', k.decode('latin1'))
            drawing_vals[k][v.decode('latin1')] += 1
    for f in files:
        try:
            data = open(f, 'rb').read()
        except OSError:
            continue
        for line in data.split(b'\r\n'):
            k, _, v = line.partition(b' ')
            k = k.decode('latin1')
            if k.startswith('view_dial.'):
                k = k[10:]
            vals[k][v.decode('latin1')] += 1
    for f in sorted(glob.glob(os.path.join(ddir, '*.json'))):
        d = json.load(open(f, encoding='utf-8'), object_pairs_hook=OD)
        page, prefix = d['page'], d['viewPrefix']
        drawing = prefix is None
        prefixes = [p for p in (d.get('gaDrawingPrefix'), d.get('assemblyDrawingPrefix')) if p] if drawing else [prefix]
        items = list(walk(d))
        for sd in d.get('subDialogs', []):
            items += list(walk(sd))
        items += d.get('siblingPageControls', []) + d.get('legacyControls', [])
        for c in items:
            if not c.get('key'):
                continue
            counts = collections.Counter()
            for p in dict.fromkeys(prefixes):
                counts.update((drawing_vals if drawing else vals).get(p + c['key'], collections.Counter()))
            if c.get('options') and c['type'] in ('option', 'color', 'linetype'):
                en, off = enum_for(page, c['key'], c)
                if en:
                    table = ENUMS[en]
                    c['apiEnum'] = 'Tekla.Structures.Drawing.' + en
                    if off:
                        c['apiEnumOffset'] = off
                        c['apiEnumNote'] = 'API value = stored value + %d (checked on every item)' % off
                    for o in c['options']:
                        if isinstance(o['value'], int) and (o['value'] + off) in table:
                            o['apiName'] = table[o['value'] + off]
                            if c['type'] == 'color' and o.get('labelSource') == 'bitmap':
                                o['label'] = table[o['value']]
                                o['labelSource'] = 'bitmap; name from DrawingColors'
                allowed = {str(o['value']) for o in c['options']}
                seen = counts
                if seen:
                    c['samples'] = OD([('files', sum(seen.values())),
                                       ('inMenu', OD(sorted(((v, n) for v, n in seen.items() if v in allowed), key=lambda x: -x[1]))),
                                       ('outsideMenu', OD(sorted(((v, n) for v, n in seen.items() if v not in allowed), key=lambda x: -x[1])))])
            elif c['type'] in ('toggle', 'number', 'text', 'hatch', 'font'):
                seen = counts
                if seen:
                    c['samples'] = OD([('files', sum(seen.values())), ('top', OD(seen.most_common(5)))])
        d['samplesSource'] = '%d %s files under %s' % (len(drawing_files) if drawing else len(files),
                                                       'drawing' if drawing else '.vi', ', '.join(sys.argv[2:]))
        with open(f, 'w', encoding='utf-8', newline='\n') as fh:
            json.dump(d, fh, indent=2, ensure_ascii=False)
            fh.write('\n')
    print('annotated', len(glob.glob(os.path.join(ddir, '*.json'))), 'files from', len(files), '.vi files')

if __name__ == '__main__':
    main()
