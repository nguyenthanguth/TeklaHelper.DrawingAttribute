"""Condensed dump of one dialog with English labels.
python condensed.py TeklaStructures.aid gratViewAttributes out.txt"""
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import aid

SKIP = {'EXPANDX', 'EXPANDY', 'ATTACHX', 'ATTACHY', 'ALIGNX', 'COLUMNS', 'SPANX', 'SPANY', 'BUTTONWIDTH',
        'DEFINEDINFILE', 'DEFINEDINROW', 'BORDER', 'MANAGED', 'WIDTH', 'HEIGHT', 'FONT', 'ROWS'}

def english_table(pool, root):
    t = {}
    for o in root.children:
        if aid.S(pool, o.cls) != 'string_utf8':
            continue
        name = aid.S(pool, o.name)
        e = o.get(dict((aid.S(pool, k), k) for k in range(1, 1))) if False else None
        ent = lang = None
        for a, v in o.attrs:
            an = aid.S(pool, a)
            if an == 'ENTRY':
                ent = v[1]
            elif an == 'LANGUAGE':
                lang = [aid.S(pool, x) for x in v[1]]
        if ent and lang and 'enu' in lang:
            t[name] = pool[ent[lang.index('enu')]].decode('utf-8', 'replace')
    return t

def find(pool, root, cls, name):
    for o in root.children:
        if aid.S(pool, o.cls) == cls and aid.S(pool, o.name) == name:
            return o

def lab(pool, en, k):
    s = aid.S(pool, k)
    if s in en:
        return '%s"%s"' % (s, en[s])
    return s

def dump(pool, en, o, out, depth):
    parts = []
    for a, v in o.attrs:
        an = aid.S(pool, a)
        if an in SKIP:
            continue
        T, x = v
        if T in ('I', 'D') or T in ('I[]', 'D[]'):
            parts.append('%s=%s' % (an, x))
        elif T.endswith('[]'):
            parts.append('%s=[%s]' % (an, ','.join(aid.S(pool, k) for k in x)))
        elif an in ('LABEL', 'TITLE'):
            parts.append('%s=%s' % (an, lab(pool, en, x)))
        else:
            parts.append('%s=%s' % (an, aid.S(pool, x)))
    out.write('%s%s %s %s\n' % ('  ' * depth, aid.S(pool, o.cls), aid.S(pool, o.name), ' '.join(parts)))
    for c in o.children:
        dump(pool, en, c, out, depth + 1)

if __name__ == '__main__':
    pool, st = aid.load(sys.argv[1])
    root = aid.parse(pool, st)
    en = english_table(pool, root)
    d = find(pool, root, 'dialog', sys.argv[2])
    with open(sys.argv[3], 'w', encoding='utf-8') as f:
        dump(pool, en, d, f, 0)
