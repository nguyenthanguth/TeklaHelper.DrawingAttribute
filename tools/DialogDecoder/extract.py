"""Extract the Tekla Structures View properties dialog (gratViewAttributes) from TeklaStructures.aid
into one JSON file per page, and the pages of the drawing properties dialogs that it does not have (DRAWING_PAGES).

python extract.py <TeklaStructures.aid> <out_dir> <sample.vi> [<sample2.vi> ...] [--drawing <sample.gd> ...]

The .aid widget tree is decoded exactly (see aid.py). What is inferred here:
- which text label belongs to which control (row order), group headings drawn as
  "text label + separator", grid labels (row label / column header);
these carry labelSource / groupSource so they can be told apart from decoded facts.
"""
import sys, os, re, json, collections
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import aid
from condensed import english_table

OD = collections.OrderedDict

PAGES = OD([
    # panel row name                   page name, prefix, legacy dialogs (same struct) used for keys the panel lacks
    ('PanelAttributes',                ('Attributes', '', ['gr_view_attrib'])),
    ('PanelDimensioning',              ('ViewDimensioning', 'ViewDimensioning.', ['gr_view_dimensioning_attrib'])),
    ('PanelFilter',                    ('Filter', 'vfilter.', [])),
    ('PanelNeighborPartFilter',        ('NeighborPartFilter', 'vnp_filter.', [])),
    ('PanelProtection',                ('Protection', 'Protection.', ['gr_vprot_attrib'])),
    ('PanelPartMark',                  ('PartMark', 'vpm.', ['gr_vpm_attrib', ('gr_textpl_attrib', 'txpl.')])),
    ('PanelBoltMark',                  ('BoltMark', 'vsm.', ['gr_vsm_attrib', ('gr_textpl_attrib', 'txpl.')])),
    ('PanelNeighborPartMark',          ('NeighborPartMark', 'vnpm.', ['gr_vnpm_attrib', ('gr_textpl_attrib', 'txpl.')])),
    ('PanelSurfaceTreatmentMark',      ('SurfaceTreatmentMark', 'vsurfm.', ['gr_vsurfm_attrib', ('gr_textpl_attrib', 'txpl.')])),
    ('PanelConnectionMark',            ('ConnectionMark', 'vjm.', ['gr_vjm_attrib', ('gr_textpl_attrib', 'txpl.')])),
    ('PanelWeldMark',                  ('WeldMark', 'vwel.', ['gr_vwel_attrib'])),
    ('PanelReinforcementMark',         ('ReinforcementMark', 'vrm.', ['gr_vrm_attrib', ('gr_textpl_attrib', 'txpl.')])),
    ('PanelNeighborReinforcementMark', ('NeighborReinforcementMark', 'NeighborRebarMark.', ['gr_vnrm_attrib', ('gr_textpl_attrib', 'txpl.')])),
    ('PanelPourMark',                  ('PourMark', 'PourMark.', ['gr_vpour_mark_attrib', ('gr_textpl_attrib', 'txpl.')])),
    ('PanelPart',                      ('PartObject', 'vp.', ['gr_vp_attrib', 'gr_dp_attrib', 'gr_dnp_attrib'])),
    ('PanelBolt',                      ('BoltObject', 'vs.', ['gr_ds_attrib'])),
    ('PanelNeighborPart',              ('NeighborPartObject', 'vnp.', ['gr_vnp_attrib', 'gr_dnp_attrib'])),
    ('PanelWeld',                      ('WeldObject', 'ViewWeld.', ['gr_weld_attrib'])),
    ('PanelSurfaceTreatment',          ('SurfaceTreatmentObject', 'vsurf.', ['gr_vsurfacing_attrib'])),
    ('PanelReinforcement',             ('ReinforcementObject', 'vrebar.', ['gr_vrebar_attrib'])),
    ('PanelNeighborReinforcement',     ('NeighborReinforcementObject', 'NeighborRebar.', ['gr_vnrebar_attrib', 'gr_vrebar_attrib'])),
    ('PanelPour',                      ('PourObject', 'PourObjects.', ['grViewPourAttrib', 'gr_vp_attrib'])),
    ('PanelPourBreak',                 ('PourBreakObject', 'PourBreaks.', ['grViewPourBreakAttrib'])),
    ('PanelReferenceObject',           ('ReferenceObject', 'vref.', ['gr_vreference_object_attrib'])),
    ('PanelGrid',                      ('GridObject', 'vg.', ['gr_grid_attrib'])),
])

# Pages of the drawing properties dialogs that the View properties dialog does not have. The GA and the assembly family
# store them under the same keys (only the view page has two prefixes; the dimensioning pages differ); the samples are a
# .gd and an .ad file saved from the dialogs.
#   page: (GA prefix, assembly prefix, source, other sources with the same keys, page check box of the drawing dialog)
# A source is (dialog, panel row or None, nested key prefix). The first source is the newest dialog that shows the page;
# the others give the keys it does not show (older dialogs and the assembly drawing dialog of Tekla 2026).
DRAWING_PAGES = OD([
    ('DrawingView',  ('dview.', 'dv.', ('gr_ga_view_attrib', None, ''),
                      [('gr_dv_attrib', None, ''), ('gratAssemblyDrawingAttributes', 'PanelContentViewCreation', '')], 'dview_en')),
    ('SectionView',  ('dc.', 'dc.', ('gratAssemblyDrawingAttributes', 'PanelContentSectionView', ''),
                      [('gr_dc_attrib', None, ''), ('gr_csym_attrib', None, 'SectionMark.'), ('gr_section_label_attrib', None, '')], 'dc_en')),
    ('DetailView',   ('DetailLabel.', 'DetailLabel.', ('gr_detail_symbol_attrib', None, ''),
                      [('gr_detail_label_attrib', None, ''), ('gratAssemblyDrawingAttributes', 'PanelContentDetailView', '')], 'DetailLabel_en')),
    ('Dimensioning', ('dd.', 'dd.', ('gr_dd_attrib', None, ''), [], 'dd_en')),
    ('DimensioningRules', ('Dimensioning.', None, ('gr_gdcd_attrib', None, ''), [], 'Dimensioning_en')),
    ('AssemblyDimensioning', (None, 'dcd.', ('gr_adcd_attrib', None, ''),
                              [('gr_wdcd_attrib', None, ''), ('gr_cudcd_attrib', None, '')], 'dcd_en')),
])

# Rows of label texts: one structure (the "..." dialog gr_view_reference_mark_attrib) per row, typed by the page ViewLabelMark
LABEL_MARK_RE = re.compile(r'^((?:\w+\.)?(?:ViewLabelMark|SectionMark|DetailMark)A)[1-5]\.')

# Page check boxes of the tree control: top-level keys "<x>_en". Matched by name only.
TREE_KEYS = {
    'Filter': 'filter_en', 'NeighborPartFilter': 'np_filter_en', 'Protection': 'Protection_en',
    'PartMark': 'pmark_en', 'BoltMark': 'smark_en', 'NeighborPartMark': 'npmark_en',
    'SurfaceTreatmentMark': 'surf_mark_en', 'ConnectionMark': 'jmark_en', 'WeldMark': 'weld_en',
    'ReinforcementMark': 'rebar_mark_en', 'NeighborReinforcementMark': 'NeighborRebarMark_en', 'PourMark': 'PourMark_en',
    'PartObject': 'part_en', 'BoltObject': 'screw_en', 'NeighborPartObject': 'npart_en', 'WeldObject': 'ViewWeld_en',
    'SurfaceTreatmentObject': 'surf_en', 'ReinforcementObject': 'rebars_en', 'NeighborReinforcementObject': 'NeighborRebar_en',
    'PourObject': 'PourObjects_en', 'PourBreakObject': 'PourBreaks_en', 'ReferenceObject': 'ref_en', 'GridObject': 'grid_en',
    'ViewDimensioning': 'ViewDimensioning_en',
}

LINETYPE_BITMAPS = OD([('gr_pb_empty', 'Undefined'), ('gr_pb_solid', 'Solid'), ('gr_pb_dashed1', 'Dashed'),
                       ('gr_pb_dashed2', 'Slashed'), ('gr_pb_dot_dash', 'DashDot'), ('gr_pb_dotted', 'Dotted'),
                       ('gr_pb_dashed3', 'DashDoubleDot'), ('gr_pb_dashed4', 'SlashDash')])
COLOR_RE = re.compile(r'^(gr_pb_(invisible|black|red|green|blue|cyan|yellow|magenta|new|gray\d+)(_line)?(_\d+)*|fill_\w+)$')

def bitmap_label(bm):
    """Readable label for picture-only option items (our naming, not Tekla's)."""
    if bm is None:
        return None
    m = re.match(r'^gr_pb_(\w+?)_line(?:_\d+)?$', bm) or re.match(r'^fill_(\w+)$', bm)
    if m:
        s = m.group(1)
        s = {'invisible': 'Invisible (background)'}.get(s, s)
        s = re.sub(r'^new_(\d)$', r'Color \1', s)
        s = re.sub(r'^gray(\d+)$', r'Gray \1', s)
        return s[:1].upper() + s[1:]
    m = re.match(r'^om_pb_frame_(\w+)$', bm) or re.match(r'^frame_(\w+)$', bm)
    if m:
        return m.group(1).capitalize()
    return bm

def main():
    path, outdir = sys.argv[1], sys.argv[2]
    samples = sys.argv[3:]
    drawing_samples = []
    if '--drawing' in samples:
        i = samples.index('--drawing')
        samples, drawing_samples = samples[:i], samples[i + 1:]
    pool, st = aid.load(path)
    root = aid.parse(pool, st)
    en = english_table(pool, root)
    S = lambda k: aid.S(pool, k)
    dialogs = {S(o.name): o for o in root.children if S(o.cls) == 'dialog'}

    def cls(o): return S(o.cls)
    def name(o): return S(o.name)
    def attr(o, a, default=None):
        for k, v in o.attrs:
            if S(k) == a:
                T, x = v
                if T in ('I', 'D') or T.endswith('[]'):
                    return x
                return S(x)
        return default

    def clean(text):
        if text is None:
            return None
        t = ' '.join(text.replace('\x01', ' ').split()).replace('&', '')
        if t.endswith(':'):
            t = t[:-1].rstrip()
        return t

    def label_of(o):
        l = attr(o, 'LABEL')
        if l is None or l == 'UNDEF':
            return None
        if l in ('\x01', ''):
            return ''
        if l in en:
            return clean(en[l])
        return '#' + l if l.startswith('albl_') else clean(l)

    def walk(o):
        yield o
        for c in o.children:
            yield from walk(c)

    # ---------------------------------------------------------------- samples
    sample_keys = OD()
    sample_vals = OD()
    for sp in samples:
        for line in open(sp, 'rb').read().split(b'\r\n')[1:]:
            if not line:
                continue
            k, _, v = line.partition(b' ')
            k = k.decode('latin1')
            sample_keys.setdefault(k, sp)
            sample_vals.setdefault(k, v.decode('latin1'))

    # ---------------------------------------------------------------- builder
    class Builder:
        def __init__(self, scope, prefix, nested='', radio_text_labels=False):
            self.scope = scope
            self.prefix = prefix
            self.nested = nested
            # A radio button without a label of its own is followed by a text label in the drawing dialogs ("Fit by
            # parts"); off for the View properties pages, whose decoding the released classes follow
            self.radio_text_labels = radio_text_labels
            self.allw = list(walk(scope))
            self.parent = {id(c): w for w in self.allw for c in w.children}
            self.byname = {}
            for w in self.allw:
                if name(w):
                    self.byname.setdefault(name(w), w)
            self.refcount = collections.Counter()
            for w in self.allw:
                cp = attr(w, 'CALLBACKPARAMETER')
                if cp and attr(w, 'MEMBER') and cls(w) in ('optionmenu', 'text', 'checkbutton', 'radiobox'):
                    self.refcount[cp] += 1
            self.radios = collections.defaultdict(list)
            for w in self.allw:
                if cls(w) == 'radiobutton':
                    self.radios[attr(w, 'RADIOBOX')].append(w)
            self.members = []
            self.used_enables = set()
            self.buttons = []

        def is_enable(self, w):
            if cls(w) != 'checkbutton':
                return False
            m = attr(w, 'MEMBER') or ''
            return name(w) in self.refcount or m.endswith('_en') or m.endswith('_enable') or m.endswith('Enable')

        def key(self, m):
            if self.prefix and m.startswith(self.prefix):
                m = m[len(self.prefix):]
            return self.nested + m

        def options(self, items):
            opts = []
            for idx, it in enumerate(items):
                o = OD()
                lbl = label_of(it)
                bm = attr(it, 'BITMAP')
                vi = attr(it, 'VALUEINT'); vs = attr(it, 'VALUESTRING')
                text_label = None
                if not lbl and self.radio_text_labels and cls(it) == 'radiobutton' and id(it) in self.parent:
                    siblings = self.parent[id(it)].children
                    following = siblings[siblings.index(it) + 1:]
                    nxt = following[0] if following else None
                    if nxt is not None and cls(nxt) == 'textlabel' and label_of(nxt):
                        text_label = label_of(nxt)
                if lbl:
                    o['label'] = lbl
                    o['labelSource'] = 'aid'
                elif text_label:
                    o['label'] = text_label
                    o['labelSource'] = 'text label after the radio button'
                else:
                    o['label'] = bitmap_label(bm)
                    o['labelSource'] = 'bitmap'
                if bm:
                    o['bitmap'] = bm
                if vi is not None:
                    o['value'] = vi
                elif vs is not None:
                    o['value'] = vs
                else:
                    o['value'] = None
                    o['indexInMenu'] = idx
                o['aidName'] = name(it)
                opts.append(o)
            return opts

        def classify(self, w, k, items):
            c = cls(w); T = attr(w, 'TYPE')
            if c == 'checkbutton':
                return 'toggle'
            if c == 'radiobox':
                return 'option'
            if c == 'text':
                if T == 'char' and re.search(r'font', k, re.I):
                    return 'font'
                return 'number' if T in ('double', 'int', 'float') else 'text'
            if c == 'optionmenu':
                if k.endswith('hatch_name'):
                    return 'hatch'
                bms = [attr(i, 'BITMAP') for i in items]
                named = [b for b in bms if b]
                if named and all(COLOR_RE.match(b) for b in named) and len(named) >= len(items) - 1 and len(named) >= 8:
                    return 'color'
                if named and len(named) == len(items) and all(b in LINETYPE_BITMAPS for b in named):
                    return 'linetype'
                if T == 'char':
                    return 'text'
                return 'option'
            return 'other'

        def control(self, w, row_label_w, inherited, group_label, grid):
            m = attr(w, 'MEMBER')
            c = cls(w)
            ctl = OD()
            cp = attr(w, 'CALLBACKPARAMETER')
            enw = self.byname.get(cp) if cp else None
            if enw is not None and (cls(enw) != 'checkbutton' or enw is w):
                enw = None
            slip = None
            near = row_label_w if row_label_w is not None else inherited
            if near is not None and near is not enw and self.is_enable(near) and attr(near, 'MEMBER'):
                nm = attr(near, 'MEMBER')
                base = re.sub(r'(_en|_enable|Enable|_enabled)$', '', nm)
                if base and (m.endswith(base.split('.')[-1]) or self.refcount[name(near)] == 0):
                    if enw is not None:
                        slip = name(enw)
                    enw = near
            own = label_of(w)
            if own is not None and own.lower() in ('on/off', ''):
                own = None
            lbl = src = None
            cand = row_label_w if row_label_w is not None else inherited
            if c == 'checkbutton' and own:
                lbl, src = own, 'own'
            elif enw is not None and label_of(enw) and self.refcount[cp] == 1:
                lbl, src = label_of(enw), 'enableCheckBox'
                if cand is not None and cls(cand) == 'textlabel' and label_of(cand):
                    ctl['caption'] = label_of(cand)
            elif cand is not None and cand is enw and label_of(enw):
                lbl, src = label_of(enw), 'enableCheckBox'
            elif cand is not None and cls(cand) == 'textlabel' and label_of(cand):
                lbl, src = label_of(cand), 'textlabel'
            elif enw is not None and label_of(enw) and self.refcount[cp] == 1:
                lbl, src = label_of(enw), 'enableCheckBox'
            elif own:
                lbl, src = own, 'own'
            elif grid and (grid.get('row') or grid.get('col')):
                lbl, src = ' / '.join(x for x in (grid.get('row'), grid.get('col')) if x), 'grid'
            elif enw is not None and label_of(enw):
                lbl, src = label_of(enw), 'enableCheckBox(shared)'
            elif group_label:
                lbl, src = group_label, 'group'
            ctl['label'] = lbl
            ctl['labelSource'] = src
            ctl['key'] = self.key(m)
            if enw is not None and attr(enw, 'MEMBER'):
                ctl['enableKey'] = self.key(attr(enw, 'MEMBER'))
                if label_of(enw) and src != 'enableCheckBox':
                    ctl['enableLabel'] = label_of(enw)
                self.used_enables.add(name(enw))
            if cp and cp not in self.byname:
                ctl['aidNote'] = 'CALLBACKPARAMETER names %s, which is not a widget of this dialog (no enable check box)' % cp
            if slip:
                ctl['aidNote'] = 'CALLBACKPARAMETER names %s (greys this control with another check box); the check box in the same row writes enableKey' % slip
            items = self.radios.get(name(w), []) if c == 'radiobox' else \
                [i for i in w.children if cls(i) == 'optionbutton'] if c == 'optionmenu' else []
            t = self.classify(w, ctl['key'], items)
            ctl['type'] = t
            conf = 'decoded'
            if items:
                opts = self.options(items)
                runtime = all(o['value'] is None for o in opts) and len(opts) <= 1
                if runtime:
                    ctl['optionsFilledAtRuntime'] = True
                else:
                    if t == 'linetype':
                        for o in opts:
                            o['label'] = LINETYPE_BITMAPS[o['bitmap']]
                            o['labelSource'] = 'bitmap=DrawingLineType (verified by API)'
                    missing = [o for o in opts if o['value'] is None]
                    if missing:
                        conf = 'order'
                        if len(missing) == len(opts):
                            for o in opts:
                                o['value'] = o['indexInMenu']
                                o['valueSource'] = 'menu index (no VALUEINT in .aid)'
                            ctl['note'] = 'no item has VALUEINT; values taken as the 0-based menu position (consistent with shipped files)'
                        else:
                            for o in missing:
                                o['value'] = 0
                                o['valueSource'] = 'assumed 0 (no VALUEINT in .aid)'
                            ctl['note'] = 'one item has no VALUEINT; assumed 0 (2 shipped files hold 0, the rest the explicit value)'
                    ctl['options'] = opts
            if c == 'checkbutton':
                on = attr(w, 'VALUEONINT'); off = attr(w, 'VALUEOFFINT')
                ctl['valueOn'] = 1 if on is None else on
                ctl['valueOff'] = 0 if off is None else off
            d = attr(w, 'DEFAULT')
            if d is not None:
                if c in ('optionmenu', 'radiobox'):
                    dw = self.byname.get(d)
                    if dw is not None and attr(dw, 'VALUEINT') is not None:
                        ctl['dialogDefault'] = attr(dw, 'VALUEINT')
                    elif 'options' in ctl:
                        hit = [o for o in ctl['options'] if o['aidName'] == d]
                        if hit:
                            ctl['dialogDefault'] = hit[0]['value']
                else:
                    ctl['dialogDefault'] = {'ON': ctl.get('valueOn', 1), 'OFF': ctl.get('valueOff', 0)}.get(d, d)
            if c == 'text':
                if attr(w, 'FORMAT'):
                    ctl['format'] = attr(w, 'FORMAT')
                if attr(w, 'LENGTH'):
                    ctl['maxLength'] = attr(w, 'LENGTH')
            fc = attr(w, 'FORMATCALLBACK') or ''
            if 'ength' in fc:
                ctl['unit'] = 'length'
            ctl['aidWidget'] = c
            ctl['aidName'] = name(w)
            ctl['aidType'] = attr(w, 'TYPE')
            ctl['confidence'] = conf
            self.members.append(m)
            return ctl

        # ----- tree walk
        def pos(self, item):
            self.seq = getattr(self, 'seq', 0) + 1
            item['position'] = self.seq
            return item

        def node(self, label, w, src=None):
            n = OD()
            n['label'] = label
            n['aidName'] = name(w) if w is not None and name(w) else None
            if src:
                n['groupSource'] = src
            n['controls'] = []
            n['groups'] = []
            return n

        def heading(self, w):
            if cls(w) != 'container' or label_of(w):
                return None
            items = [x for x in walk(w) if x is not w and cls(x) not in ('row', 'dummyitem', 'separator', 'container')]
            seps = [x for x in walk(w) if cls(x) == 'separator']
            if len(items) == 1 and cls(items[0]) == 'textlabel' and label_of(items[0]):
                return label_of(items[0]), bool(seps)
            return None

        def build(self, top):
            body = self.node(None, top)
            self.container(top, body, None)
            self.dedupe(body)
            return body

        def dedupe(self, n):
            cnt = collections.Counter(c.get('label') for c in n['controls'] if c.get('key'))
            dup = [c for c in n['controls'] if c.get('key') and cnt[c.get('label')] > 1
                   and c.get('labelSource') in ('group', 'enableCheckBox(shared)', None)]
            by_label = collections.defaultdict(list)
            for c in dup:
                by_label[c.get('label')].append(c)
            for lbl, cs in by_label.items():
                tails = [c['key'].split('.')[-1] for c in cs]
                cands = set()
                for t in tails:
                    for i, ch in enumerate(t):
                        if ch == '_':
                            cands.add(t[:i + 1])
                good = [c_ for c_ in cands if sum(t.startswith(c_) for t in tails) * 2 >= len(tails) and len(tails) > 1]
                cp = max(good, key=len) if good else ''
                for c, t in zip(cs, tails):
                    t2 = t[len(cp):] if cp and t.startswith(cp) and len(t) > len(cp) else t
                    c['label'] = ('%s %s' % (lbl or '', t2.replace('_', ' '))).strip()
                    c['labelSource'] = (c.get('labelSource') or 'none') + '+key'
                    c['labelConfidence'] = 'key-name'
            for g in n['groups']:
                self.dedupe(g)
            for t in n.get('tabs', []):
                self.dedupe(t)

        def container(self, w, node, inherited):
            """Walk the cells of container w. Rows reset the label context; a heading cell starts a group."""
            state = {'node': node, 'header': None}
            kids = list(w.children)
            if not any(cls(k) == 'row' for k in kids):
                self.row(kids, state, inherited)
            else:
                for r in kids:
                    if cls(r) == 'row':
                        self.row(list(r.children), state, inherited)
                    else:
                        self.row([r], state, inherited)
                    inherited = None
            return state['node']

        def row(self, cells, state, inherited):
            target = state['node']
            labels = [label_of(x) for x in cells if cls(x) == 'textlabel' and label_of(x)]
            valued = [x for x in cells if attr(x, 'MEMBER') and not self.is_enable(x)]
            enables = [x for x in cells if self.is_enable(x)]
            if len(labels) >= 2 and not valued:
                state['header'] = labels
            last = None
            row_label = None
            col = 0
            for x in cells:
                c = cls(x)
                h = self.heading(x)
                if h is not None:
                    g = self.node(h[0], x, 'heading-textlabel' + ('+separator' if h[1] else ''))
                    root_node = state.get('root', state['node'])
                    state['root'] = root_node
                    root_node['groups'].append(self.pos(g))
                    state['node'] = target = g
                    state['header'] = None
                    continue
                if c in ('dummyitem', 'separator'):
                    continue
                if c == 'textlabel':
                    if label_of(x):
                        last = x
                        row_label = label_of(x)
                    continue
                if c == 'tabbedwindow':
                    tabs = target.setdefault('tabs', [])
                    for r in x.children:
                        for ti in ([r] if cls(r) == 'tabbeditem' else [y for y in r.children if cls(y) == 'tabbeditem']):
                            tab = self.node(label_of(ti), ti)
                            self.container(ti, tab, None)
                            tabs.append(self.pos(tab))
                    continue
                if c in ('container', 'bulletinboard', 'row', 'maincontainer'):
                    l = label_of(x) if c == 'container' else None
                    if l:
                        g = self.node(l, x)
                        target['groups'].append(self.pos(g))
                        self.container(x, g, None)
                    else:
                        self.container_into(x, target, last if last is not None else inherited)
                    continue
                if c == 'table':
                    cols = [y for y in x.children if cls(y) == 'column']
                    t = OD()
                    t['label'] = None
                    t['type'] = 'table'
                    t['aidName'] = name(x)
                    t['aidStruct'] = attr(x, 'STRUCT')
                    t['columns'] = [OD([('member', attr(y, 'MEMBER')), ('title', label_of(y) or (clean(en.get(attr(y, 'TITLE') or '', '')) or None))]) for y in cols]
                    t['note'] = 'runtime table: its rows are kept in the dialog structure, not under a single key of this page'
                    target['controls'].append(self.pos(t))
                    continue
                if c == 'pushbutton':
                    cb = attr(x, 'CALLBACK') or ''
                    cp = attr(x, 'CALLBACKPARAMETER')
                    if cb in ('gr_ok_attr_cb', 'gr_apply_attr_cb', 'gr_modify_cb', 'gr_set_from_select_cb', 'gr_on_off_attr_cb',
                              'gr_cancel_attr_cb', 'gr_save_attr_cb', 'gr_load_attr_cb', 'gr_saveas_attr_cb'):
                        continue
                    if 'SubPanelSave' in cb or 'ChooseColor' in cb or re.search(r'Select\w*Font', cb) or 'CreateHatchTable' in cb \
                            or 'ContentTable' in cb or 'MoveElement' in cb or cb.startswith('gratFilterPanel') \
                            or cb.startswith('gratPanelNeighborPartFilter') or 'DimensioningPanelRule' in cb:
                        continue
                    b = OD()
                    b['label'] = label_of(x)
                    b['type'] = 'button'
                    b['aidName'] = name(x)
                    b['callback'] = cb
                    if cp:
                        b['callbackParameter'] = cp
                    target['controls'].append(self.pos(b))
                    self.buttons.append(b)
                    continue
                if c == 'checkbutton' and self.is_enable(x):
                    last = x
                    continue
                if c in ('checkbutton', 'optionmenu', 'text', 'radiobox') and attr(x, 'MEMBER'):
                    m = attr(x, 'MEMBER')
                    if re.search(r'(^|\.)(in_file|aInFile|InFile|out_file|aOutFile|OutFile)$', m):
                        self.members.append(m)
                        continue
                    grid = None
                    if c == 'checkbutton' and state['header'] and not (label_of(x) or '').strip():
                        hdr = state['header']
                        grid = {'row': row_label, 'col': hdr[col] if col < len(hdr) else None}
                        col += 1
                    ctl = self.control(x, None if grid else last, None if grid else inherited, target.get('label'), grid)
                    target['controls'].append(self.pos(ctl))
                    last = None
                    inherited = None
                    continue
                if c == 'radiobutton':
                    continue
                if c in ('treecontrol', 'subframe', 'column'):
                    continue
                o = OD([('label', label_of(x) or (label_of(last) if last is not None and cls(last) == 'textlabel' else None)),
                        ('type', 'other'), ('aidWidget', c), ('aidName', name(x))])
                last = None
                if c == 'checkbutton':
                    o['note'] = 'check box without MEMBER: dialog-only state, writes no key'
                if c == 'optionmenu':
                    o['options'] = self.options([i for i in x.children if cls(i) == 'optionbutton'])
                    o['note'] = 'option menu without MEMBER: changes what the dialog shows, writes no key itself'
                    if 'tab_page_option' in name(x):
                        o['label'] = 'Mark content of'
                        o['labelSource'] = 'ours'
                        o['note'] += ('; selects which part kind the content table edits. Guess (not verified): the four mark contents are '
                                      'ContentString1..4 and CurrentContentTableNumber keeps the selection')
                target['controls'].append(self.pos(o))

        def container_into(self, w, target, inherited):
            """unlabelled container: its content belongs to the enclosing group."""
            sub = {'node': target, 'header': None}
            kids = list(w.children)
            if cls(w) == 'bulletinboard' or not any(cls(k) == 'row' for k in kids):
                self.row(kids, sub, inherited)
            else:
                for r in kids:
                    self.row(list(r.children) if cls(r) == 'row' else [r], sub, inherited)
                    inherited = None

    # ------------------------------------------------------------- main loop
    def collect(n, path):
        for c in n.get('controls', []):
            yield c, path
        for g in n.get('groups', []):
            yield from collect(g, path + [g['label']])
        for t in n.get('tabs', []):
            yield from collect(t, path + [t['label']])

    view = dialogs['gratViewAttributes']
    right = next(o for o in walk(view) if cls(o) == 'container' and name(o) == 'containerRightPanel')
    panels = [r for r in right.children if cls(r) == 'row' and name(r).startswith('Panel')]

    # Another Tekla version can add or drop pages: report them instead of failing, and decode the known ones
    unknown = [name(p) for p in panels if name(p) not in PAGES]
    missing = [p for p in PAGES if p not in {name(x) for x in panels}]
    if unknown:
        print('WARNING: panels not in PAGES (add them with their page name, .vi prefix and legacy dialogs; '
              'also to TREE_KEYS and to tools/DialogClassGenerator/PageSpecs.cs): ' + ', '.join(unknown), file=sys.stderr)
    if missing:
        print('WARNING: panels of PAGES that this .aid does not have: ' + ', '.join(missing), file=sys.stderr)
    panels = [p for p in panels if name(p) in PAGES]

    pages = OD()
    for panel in panels:
        pname = name(panel)
        page, prefix, legacy = PAGES[pname]
        b = Builder(panel, prefix)
        tree = b.build(panel)
        file_keys = [k for k in sample_keys if (k.startswith(prefix) if prefix else '.' not in k)]
        fk = (lambda pre: (lambda k: k[len(pre):] if pre else k))(prefix)
        dialog_keys = set(b.members)
        for w in b.allw:
            if attr(w, 'MEMBER') and b.is_enable(w):
                dialog_keys.add(attr(w, 'MEMBER'))
        subdialogs = []
        for btn in b.buttons:
            dname = btn.get('callbackParameter')
            if dname not in dialogs:
                continue
            d = dialogs[dname]
            mem = [attr(w, 'MEMBER') for w in walk(d) if attr(w, 'MEMBER')]
            cands = collections.Counter()
            for k in file_keys:
                rest = fk(k)
                for mm in mem:
                    if rest.endswith('.' + mm) or rest == mm:
                        cands[rest[:len(rest) - len(mm)]] += 1
            btn['opensDialog'] = dname
            if not cands:
                btn['note'] = 'members of this dialog are not stored under the page prefix (separate settings)'
                continue
            nested = cands.most_common(1)[0][0]
            sb = Builder(d, '', nested)
            st_ = sb.build(d)
            subdialogs.append(OD([('dialog', dname), ('openedBy', btn['label']), ('keyPrefix', nested),
                                  ('tabs', st_.get('tabs', [])), ('groups', st_['groups']), ('controls', st_['controls'])]))
            for mm in sb.members:
                dialog_keys.add(prefix + nested + mm)
            for w in sb.allw:
                if attr(w, 'MEMBER') and sb.is_enable(w):
                    dialog_keys.add(prefix + nested + attr(w, 'MEMBER'))
        pages[page] = dict(prefix=prefix, legacy=legacy, b=b, tree=tree, file_keys=file_keys, fk=fk,
                           dialog_keys=dialog_keys, subdialogs=subdialogs, pname=pname)

    FAMILIES = [
        ['PartMark', 'BoltMark', 'NeighborPartMark', 'SurfaceTreatmentMark', 'ConnectionMark',
         'ReinforcementMark', 'NeighborReinforcementMark', 'PourMark'],
        ['PartObject', 'NeighborPartObject', 'PourObject'],
        ['ReinforcementObject', 'NeighborReinforcementObject'],
        ['Filter', 'NeighborPartFilter'],
    ]
    family_of = {p: f for f in FAMILIES for p in f}

    summary = []
    for page, P in pages.items():
        prefix = P['prefix']; fk = P['fk']; tree = P['tree']; b = P['b']
        panel_keys = {c.get('key') for c, _ in collect(tree, []) if c.get('key')}
        panel_keys |= {c.get('enableKey') for c, _ in collect(tree, []) if c.get('enableKey')}
        for sd in P['subdialogs']:
            for c, _ in collect(sd, []):
                if c.get('key'):
                    panel_keys.add(c['key'])
                if c.get('enableKey'):
                    panel_keys.add(c['enableKey'])
        unmapped = [k for k in P['file_keys'] if k not in P['dialog_keys']]
        found = set()
        # 1. the same key shown on a sibling page with the same key set
        sibling_ctl = []
        for other in family_of.get(page, []):
            if other == page:
                continue
            Q = pages[other]
            items = list(collect(Q['tree'], []))
            for sd in Q['subdialogs']:
                items += [(c, ['(' + sd['dialog'] + ')']) for c, _ in collect(sd, [])]
            for c, pth in items:
                k = c.get('key')
                if not k or prefix + k not in unmapped or prefix + k in found:
                    continue
                c = OD(c)
                c['confidence'] = 'sibling-page'
                c['shownOnPage'] = other
                c['dialogPath'] = [x for x in pth if x]
                sibling_ctl.append(c)
                found.add(prefix + k)
                if c.get('enableKey'):
                    found.add(prefix + c['enableKey'])
        # 2. older standalone dialogs with the same structure
        legacy_ctl = []
        for entry in P['legacy']:
            ln, nested = (entry if isinstance(entry, tuple) else (entry, ''))
            if ln not in dialogs:
                continue
            lb = Builder(dialogs[ln], '', nested)
            ltree = lb.build(dialogs[ln])
            for c, pth in collect(ltree, []):
                k = c.get('key')
                if not k or prefix + k not in unmapped or prefix + k in found:
                    continue
                if re.search(r'(out_file|aOutFile|OutFile)$', k):
                    continue
                if any(pk.endswith('.' + k) for pk in panel_keys):
                    continue   # same member name inside a nested struct of the panel (FontAttr.height ...)
                c = OD(c)
                c['confidence'] = 'legacy-dialog'
                c['aidDialog'] = ln
                c['dialogPath'] = [x for x in pth if x]
                legacy_ctl.append(c)
                found.add(prefix + k)
                if c.get('enableKey') and prefix + c['enableKey'] in unmapped:
                    found.add(prefix + c['enableKey'])
        allc = [c for c, _ in collect(tree, []) if c.get('key')]
        keys_here = {c['key'] for c in allc}
        for c in allc:
            ek = c.get('enableKey')
            if not ek:
                continue
            # The check box can name its member without the struct of the control (NeighborRebarMark: LineType_en for
            # RebarGroupMarkAttributes.LineType); the files saved from the dialog tell which key Tekla writes
            nest = c['key'].rpartition('.')[0]
            if nest and '.' not in ek and prefix + ek not in sample_keys and prefix + nest + '.' + ek in sample_keys:
                c['enableKey'] = ek = nest + '.' + ek
                c['enableKeyNote'] = 'the check box member has no struct prefix in the .aid; the saved files hold this key'
            base = re.sub(r'(_en|_enable|Enable)$', '', ek)
            own_en = prefix + c['key'] + '_en'
            if base != c['key'] and base in keys_here and own_en in sample_keys:
                c['aidNote'] = ('enable check box writes %s, the enable key of %s; the file also has %s_en, which no widget writes '
                                '(probable slip in the .aid)') % (ek, base, c['key'])
        if page == 'Protection':
            for c in allc:
                mm = re.match(r'^(\w+?)_prot_(\w+)$', c['key'])
                if mm:
                    c['label'] = '%s / %s' % (mm.group(2), mm.group(1))
                    c['labelSource'] = 'key'
                    c['labelConfidence'] = 'key-name'
                    c['grid'] = OD([('row', mm.group(2)), ('rowHeader', 'New (picture lb_dprot_new)'),
                                    ('column', mm.group(1)), ('columnHeader', 'Drawn (picture lb_dprot_drawn)')])
        file_menu = [b.key(m) for m in b.members if re.search(r'(^|\.)(in_file|aInFile|InFile)$', m)]
        save_as = [fk(k) for k in unmapped if re.search(r'(^|\.)(out_file|aOutFile|OutFile)$', fk(k)) and '.' not in fk(k)]
        for k in save_as:
            found.add(prefix + k)
        page_enables = []
        if page == 'Attributes':
            for pg, tk in TREE_KEYS.items():
                if tk in unmapped:
                    found.add(tk)
                    page_enables.append(OD([('key', tk), ('page', pg)]))
        file_menu_note = None
        if not file_menu and prefix + 'in_file' in unmapped:
            found.add(prefix + 'in_file')
            file_menu = ['in_file']
            file_menu_note = ('file menu gr_view_get_menu at the top of gratViewAttributes (loads the whole .vi)' if page == 'Attributes'
                              else 'the 2025 panel has no file menu for this page; the key is still written')
        still = [fk(k) for k in unmapped if k not in found]

        out = OD()
        out['page'] = page
        out['viewPrefix'] = prefix
        out['aidFile'] = os.path.basename(path)
        out['aidDialog'] = 'gratViewAttributes'
        out['aidPanel'] = P['pname']
        if page in TREE_KEYS:
            out['treeCheckBoxKey'] = OD([('key', TREE_KEYS[page]), ('in', 'top level of the .vi'),
                                         ('confidence', 'legacy-dialog'),
                                         ('note', 'gratViewAttributes fills its tree (TWOSTATE check boxes) at run time; the key comes from gr_view_attrib, '
                                                  'where this check box precedes the button that opens the page dialog')])
        out['fileMenuKey'] = file_menu[0] if file_menu else None
        if file_menu_note:
            out['fileMenuNote'] = file_menu_note
        out['saveAsKey'] = save_as[0] if save_as else None
        if save_as:
            out['saveAsNote'] = ('not in gratViewAttributes; the older dialogs have it as the "Save as" text field '
                                 '(text gr_*_saveas_file, MEMBER out_file)')
        if page_enables:
            out['pageEnableKeys'] = page_enables
            out['pageEnableKeysEvidence'] = ('gr_view_attrib (older View properties dialog in the same .aid): each key is the MEMBER of the check box '
                                             'placed before the button that opens the page dialog; in gratViewAttributes they are the tree check boxes')
        out['tabs'] = tree.get('tabs', [])
        out['groups'] = tree['groups']
        out['controls'] = tree['controls']
        out['subDialogs'] = P['subdialogs']
        out['siblingPageControls'] = sibling_ctl
        out['legacyControls'] = legacy_ctl
        out['unmappedKeys'] = still
        out['dialogKeysNotInSamples'] = sorted({b.key(m) for m in P['dialog_keys'] if (m.startswith(prefix) if prefix else '.' not in m)
                                               and m not in sample_keys and not re.search(r'(in_file|InFile)$', m)})
        with open(os.path.join(outdir, page + '.json'), 'w', encoding='utf-8', newline='\n') as f:
            json.dump(out, f, indent=2, ensure_ascii=False)
            f.write('\n')
        summary.append((page, prefix, len(P['file_keys']), len(P['file_keys']) - len(unmapped), len(sibling_ctl), len(legacy_ctl), len(still)))
    # sub dialog of the Attributes page (Label tab, "..." buttons): one structure for ViewLabelMarkA1..A5
    for page, prefix, dname, opener in [('ViewLabelMark', 'ViewLabelMarkA1.', 'gr_view_reference_mark_attrib',
                                         'Attributes > Label > Text > "..." next to A1..A5 (callback grViewReferenceMarkContentCB)')]:
        lb = Builder(dialogs[dname], '')
        lt = lb.build(dialogs[dname])
        file_keys = [k for k in sample_keys if k.startswith(prefix)]
        rest = [k[len(prefix):] for k in file_keys]
        done = set()
        for c, _ in collect(lt, []):
            for fld in ('key', 'enableKey'):
                k = c.get(fld)
                if not k:
                    continue
                if k not in rest:
                    hits = [r for r in rest if r.endswith('.' + k)]
                    if len(hits) == 1:
                        c[fld] = hits[0]
                        c[fld + 'ResolvedBy'] = 'member name; nested struct prefix taken from the unique file key ending with it'
                done.add(c[fld])
        for w in lb.allw:
            m = attr(w, 'MEMBER')
            if m and m in rest:
                done.add(m)
            if m and lb.is_enable(w):
                hits = [r for r in rest if r == m or r.endswith('.' + m)]
                if len(hits) == 1:
                    done.add(hits[0])
        out = OD()
        out['page'] = page
        out['viewPrefix'] = prefix
        out['viewPrefixes'] = ['ViewLabelMarkA%d.' % i for i in range(1, 6)]
        out['aidFile'] = os.path.basename(path)
        out['aidDialog'] = dname
        out['openedFrom'] = opener
        out['fileMenuKey'] = 'aInFile'
        out['saveAsKey'] = 'aOutFile'
        out['tabs'] = lt.get('tabs', [])
        out['groups'] = lt['groups']
        out['controls'] = lt['controls']
        out['unmappedKeys'] = [r for r in rest if r not in done and r not in ('aInFile', 'aOutFile')]
        with open(os.path.join(outdir, page + '.json'), 'w', encoding='utf-8', newline='\n') as f:
            json.dump(out, f, indent=2, ensure_ascii=False)
            f.write('\n')
        summary.append((page, prefix, len(file_keys), len(file_keys) - len(out['unmappedKeys']), 0, 0, len(out['unmappedKeys'])))

    # ------------------------------------------------------------- drawing pages
    drawing_keys = OD()
    for sp in drawing_samples:
        for line in open(sp, 'rb').read().split(b'\r\n')[1:]:
            if line:
                drawing_keys.setdefault(line.partition(b' ')[0].decode('latin1'), sp)

    def scope_of(dname, panel):
        if dname not in dialogs:
            return None
        if panel is None:
            return dialogs[dname]
        return next((w for w in walk(dialogs[dname]) if cls(w) == 'row' and name(w) == panel), None)

    def with_enables(builder, nested_prefix):
        keys = {nested_prefix + builder.key(m) for m in builder.members}
        for w in builder.allw:
            if attr(w, 'MEMBER') and builder.is_enable(w):
                keys.add(nested_prefix + builder.key(attr(w, 'MEMBER')))
        return keys

    for page, (ga_prefix, asm_prefix, source, others, tree_key) in DRAWING_PAGES.items():
        prefix = ga_prefix or asm_prefix
        dname, panel, nested = source
        scope = scope_of(dname, panel)
        if scope is None:
            print('WARNING: %s: source %s %s not in this .aid' % (page, dname, panel or ''), file=sys.stderr)
            continue
        all_keys = [k for k in drawing_keys if k.startswith(prefix)]
        marks = OD()
        for k in all_keys:
            mm = LABEL_MARK_RE.match(k[len(prefix):])
            if mm:
                marks[mm.group(1)] = marks.get(mm.group(1), 0) + 1
        file_keys = [k for k in all_keys if not LABEL_MARK_RE.match(k[len(prefix):])]
        b = Builder(scope, prefix, nested, radio_text_labels=True)
        tree = b.build(scope)
        dialog_keys = with_enables(b, prefix)
        subdialogs = []
        for btn in b.buttons:
            sub = btn.get('callbackParameter')
            if sub not in dialogs:
                continue
            mem = [attr(w, 'MEMBER') for w in walk(dialogs[sub]) if attr(w, 'MEMBER')]
            cands = collections.Counter()
            for k in file_keys:
                rest = k[len(prefix):]
                for m_ in mem:
                    if rest.endswith('.' + m_) or rest == m_:
                        cands[rest[:len(rest) - len(m_)]] += 1
            btn['opensDialog'] = sub
            if not cands:
                btn['note'] = 'members of this dialog are not stored under the page prefix (separate settings)'
                continue
            sb = Builder(dialogs[sub], '', cands.most_common(1)[0][0], radio_text_labels=True)
            st_ = sb.build(dialogs[sub])
            subdialogs.append(OD([('dialog', sub), ('openedBy', btn['label']), ('keyPrefix', sb.nested),
                                  ('tabs', st_.get('tabs', [])), ('groups', st_['groups']), ('controls', st_['controls'])]))
            dialog_keys |= with_enables(sb, prefix)
        unmapped = [k for k in file_keys if k not in dialog_keys]
        panel_keys = {c.get(f) for c, _ in collect(tree, []) for f in ('key', 'enableKey') if c.get(f)}
        found = set()
        legacy_ctl = []
        for oname, opanel, onested in others:
            oscope = scope_of(oname, opanel)
            if oscope is None:
                print('WARNING: %s: source %s %s not in this .aid' % (page, oname, opanel or ''), file=sys.stderr)
                continue
            lb = Builder(oscope, prefix, onested, radio_text_labels=True)
            ltree = lb.build(oscope)
            for c, pth in collect(ltree, []):
                k = c.get('key')
                if not k or prefix + k not in unmapped or prefix + k in found:
                    continue
                if any(pk.endswith('.' + k) for pk in panel_keys):
                    continue
                c = OD(c)
                c['confidence'] = 'other-dialog'
                c['aidDialog'] = oname + ('/' + opanel if opanel else '')
                c['dialogPath'] = [x for x in pth if x]
                legacy_ctl.append(c)
                found.add(prefix + k)
                if c.get('enableKey') and prefix + c['enableKey'] in unmapped:
                    found.add(prefix + c['enableKey'])
            for k in with_enables(lb, prefix):
                if k in unmapped and re.search(r'(_en|_enable|Enable)$', k):
                    found.add(k)
        save_as = [k[len(prefix):] for k in unmapped if re.search(r'(^|\.)(out_file|aOutFile|OutFile)$', k[len(prefix):])
                   and '.' not in k[len(prefix):]]
        file_menu = [b.key(m) for m in b.members if re.search(r'(^|\.)(in_file|aInFile|InFile)$', m)]
        for k in save_as + file_menu:
            found.add(prefix + k)
        if not file_menu and prefix + 'in_file' in unmapped:
            found.add(prefix + 'in_file')
            file_menu = ['in_file']
        still = [k[len(prefix):] for k in unmapped if k not in found]

        out = OD()
        out['page'] = page
        out['viewPrefix'] = None
        out['gaDrawingPrefix'] = ga_prefix
        out['assemblyDrawingPrefix'] = asm_prefix
        out['aidFile'] = os.path.basename(path)
        out['aidDialog'] = dname
        if panel:
            out['aidPanel'] = panel
        out['otherSources'] = [o[0] + ('/' + o[1] if o[1] else '') + (' (keys under %s)' % o[2] if o[2] else '') for o in others]
        out['treeCheckBoxKey'] = OD([('key', tree_key), ('in', 'top level of the drawing file'), ('confidence', 'decoded'),
                                     ('note', 'check box in front of the button that opens the page in the drawing properties (gr_gdraw_attrib, gr_adraw_attrib)')])
        out['fileMenuKey'] = file_menu[0] if file_menu else None
        out['saveAsKey'] = save_as[0] if save_as else None
        out['labelMarks'] = [OD([('keyStem', stem), ('rows', 5), ('keys', n),
                                 ('note', 'one row per label text A1..A5, each the structure of the "..." dialog '
                                          'gr_view_reference_mark_attrib (page ViewLabelMark)')]) for stem, n in marks.items()]
        out['tabs'] = tree.get('tabs', [])
        out['groups'] = tree['groups']
        out['controls'] = tree['controls']
        out['subDialogs'] = subdialogs
        out['siblingPageControls'] = []
        out['legacyControls'] = legacy_ctl
        out['unmappedKeys'] = still
        out['dialogKeysNotInSamples'] = sorted(k[len(prefix):] for k in dialog_keys
                                               if k not in drawing_keys and not re.search(r'(in_file|InFile)$', k))
        with open(os.path.join(outdir, page + '.json'), 'w', encoding='utf-8', newline='\n') as f:
            json.dump(out, f, indent=2, ensure_ascii=False)
            f.write('\n')
        summary.append((page, prefix, len(file_keys), len(file_keys) - len(unmapped), 0, len(legacy_ctl), len(still)))

    print('%-28s %-20s %5s %7s %7s %7s %9s' % ('page', 'prefix', 'keys', 'dialog', 'sibling', 'legacy', 'unmapped'))
    for s_ in summary:
        print('%-28s %-20s %5d %7d %7d %7d %9d' % s_)

if __name__ == '__main__':
    main()
