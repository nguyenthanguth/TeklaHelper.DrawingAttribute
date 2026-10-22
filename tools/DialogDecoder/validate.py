"""Check option values of the page JSON files against real .vi files.
python validate.py <dialogs dir> <file or folder> ..."""
import sys, os, json, glob, collections

def controls(n):
    for c in n.get('controls', []):
        yield c
    for g in n.get('groups', []):
        yield from controls(g)
    for t in n.get('tabs', []):
        yield from controls(t)

pages = {}
for f in glob.glob(os.path.join(sys.argv[1], '*.json')):
    d = json.load(open(f, encoding='utf-8'))
    items = list(controls(d))
    for sd in d.get('subDialogs', []):
        items += list(controls(sd))
    items += d.get('siblingPageControls', []) + d.get('legacyControls', [])
    for c in items:
        if c.get('key') and c.get('options') and c['type'] in ('option', 'color', 'linetype'):
            pages[d['viewPrefix'] + c['key']] = (d['page'], c)
files = []
for a in sys.argv[2:]:
    files += glob.glob(os.path.join(a, '**', '*.vi'), recursive=True) if os.path.isdir(a) else [a]
vals = collections.defaultdict(collections.Counter)
for f in files:
    for line in open(f, 'rb').read().split(b'\r\n'):
        k, _, v = line.partition(b' ')
        k = k.decode('latin1')
        if k.startswith('view_dial.'):
            k = k[10:]
        if k in pages:
            vals[k][v.decode('latin1')] += 1
print(len(files), 'files')
bad = 0
for k, (page, c) in sorted(pages.items()):
    allowed = {str(o['value']) for o in c['options']}
    off = {v: n for v, n in vals[k].items() if v not in allowed}
    if off:
        bad += 1
        print('%-22s %-45s %-8s outside menu: %s   (menu %s)' % (page, k, c['type'], dict(off), sorted(allowed, key=lambda x: (len(x), x))[:20]))
print(bad, 'keys with values outside the menu')
