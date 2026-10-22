"""Parser for Tekla RES 1.1 (.aid) compiled dialog resources.

Layout: 'RES 1.1\n' <count> ' ' then <count> NUL-terminated strings (1-based pool),
then a text token stream:
  O<class> <name>      open object (class and name are pool indices)
  o                    close object (may be glued to the next token)
  a<attr> <T><v>       scalar attribute; T in I (int), K/N/S/P/... (pool index)
  A<attr> <n> <T><v1> v2 .. vn   array attribute of n values of type T
"""
import sys, re, pickle, os

class Obj:
    __slots__ = ('cls', 'name', 'attrs', 'children', 'parent', 'pos')
    def __init__(self, cls, name, parent, pos):
        self.cls = cls; self.name = name; self.attrs = []; self.children = []; self.parent = parent; self.pos = pos
    def get(self, a, default=None):
        for k, v in self.attrs:
            if k == a:
                return v
        return default

def load(path):
    d = open(path, 'rb').read()
    nl = d.index(b'\n') + 1
    sp = d.index(b' ', nl)
    count = int(d[nl:sp])
    pos = sp + 1
    pool = [None]
    for _ in range(count):
        j = d.index(b'\x00', pos)
        pool.append(d[pos:j])
        pos = j + 1
    stream = d[pos:].decode('latin1')
    return pool, stream

TOK = re.compile(r'o|[OaA][0-9]+|[A-Z]-?[0-9.eE+-]+|-?[0-9][0-9.eE+-]*')

def parse(pool, stream):
    toks = TOK.findall(stream)
    root = Obj(None, None, None, -1)
    cur = root
    i = 0
    n = len(toks)
    def num(x):
        try:
            return int(x)
        except ValueError:
            return float(x)
    def val(t):
        return (t[0], num(t[1:]))
    while i < n:
        t = toks[i]
        if t == 'o':
            cur = cur.parent
            i += 1
        elif t[0] == 'O':
            o = Obj(int(t[1:]), int(toks[i + 1]), cur, i)
            cur.children.append(o)
            cur = o
            i += 2
        elif t[0] == 'a':
            cur.attrs.append((int(t[1:]), val(toks[i + 1])))
            i += 2
        elif t[0] == 'A':
            cnt = int(toks[i + 1])
            first = toks[i + 2]
            T = first[0]
            vals = [num(first[1:])] + [num(x) for x in toks[i + 3:i + 2 + cnt]]
            cur.attrs.append((int(t[1:]), (T + '[]', vals)))
            i += 2 + cnt
        else:
            raise ValueError('unexpected token %r at %d: %r' % (t, i, toks[i - 5:i + 5]))
    return root

def S(pool, k):
    return pool[k].decode('latin1') if 0 < k < len(pool) else '#%d' % k

if __name__ == '__main__':
    pool, stream = load(sys.argv[1])
    root = parse(pool, stream)
    print(len(pool) - 1, 'strings', len(root.children), 'top objects')
