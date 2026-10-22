"""Dump an .aid file as an indented tree: python dump.py file.aid out.txt"""
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import aid

def fmt(pool, v):
    T, x = v
    if T == 'I' or T == 'D' or T == 'I[]' or T == 'D[]':
        return '%s%s' % (T, x)
    if T.endswith('[]'):
        return T + '[' + ', '.join(repr(aid.S(pool, k)) for k in x) + ']'
    return '%s:%r' % (T, aid.S(pool, x))

def dump(pool, o, out, depth=0):
    attrs = ' '.join('%s=%s' % (aid.S(pool, a), fmt(pool, v)) for a, v in o.attrs if a not in (9, 10)) if o.cls else ''
    if o.cls is not None:
        row = o.get(10)
        out.write('%s%s %s  [%s] %s\n' % ('  ' * depth, aid.S(pool, o.cls), aid.S(pool, o.name), row[1] if row else '', attrs))
    for c in o.children:
        dump(pool, c, out, depth + (1 if o.cls is not None else 0))

if __name__ == '__main__':
    pool, st = aid.load(sys.argv[1])
    root = aid.parse(pool, st)
    with open(sys.argv[2], 'w', encoding='utf-8') as f:
        dump(pool, root, f)
