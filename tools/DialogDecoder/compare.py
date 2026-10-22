"""Compare two folders of page JSON files, e.g. the dialogs of two Tekla versions, or a new decoding with the files the
generator reads:

python compare.py <old dialogs folder> <new dialogs folder> [--samples]

Without --samples the value statistics (samples, samplesSource) are ignored: they change whenever the sample folders do.
Prints, per page, the controls added and removed (by key) and every other difference, path by path.
"""
import sys, os, json, glob

IGNORED = {'samples', 'samplesSource'}


def strip(node, ignored):
    if isinstance(node, dict):
        return {k: strip(v, ignored) for k, v in node.items() if k not in ignored}
    if isinstance(node, list):
        return [strip(v, ignored) for v in node]
    return node


def keys(node, found):
    if isinstance(node, dict):
        if isinstance(node.get('key'), str):
            found.add(node['key'])
        for v in node.values():
            keys(v, found)
    elif isinstance(node, list):
        for v in node:
            keys(v, found)
    return found


def diff(a, b, path, out):
    if type(a) is not type(b):
        out.append('%s: %s -> %s' % (path, short(a), short(b)))
    elif isinstance(a, dict):
        for k in sorted(set(a) | set(b)):
            if k not in a:
                out.append('%s.%s: added %s' % (path, k, short(b[k])))
            elif k not in b:
                out.append('%s.%s: removed %s' % (path, k, short(a[k])))
            else:
                diff(a[k], b[k], '%s.%s' % (path, k), out)
    elif isinstance(a, list):
        if len(a) != len(b):
            out.append('%s: %d items -> %d items' % (path, len(a), len(b)))
        for i, (x, y) in enumerate(zip(a, b)):
            diff(x, y, '%s[%d]' % (path, i), out)
    elif a != b:
        out.append('%s: %s -> %s' % (path, short(a), short(b)))


def short(value):
    text = json.dumps(value, ensure_ascii=False)
    return text if len(text) <= 100 else text[:97] + '...'


def main():
    args = [a for a in sys.argv[1:] if not a.startswith('--')]
    if len(args) != 2:
        print(__doc__)
        return 2

    ignored = set() if '--samples' in sys.argv else IGNORED
    old_dir, new_dir = args
    names = sorted({os.path.basename(f) for d in (old_dir, new_dir) for f in glob.glob(os.path.join(d, '*.json'))})
    changed = 0
    for name in names:
        old_path, new_path = os.path.join(old_dir, name), os.path.join(new_dir, name)
        if not os.path.exists(old_path) or not os.path.exists(new_path):
            print('%s: only in %s' % (name, new_dir if os.path.exists(new_path) else old_dir))
            changed += 1
            continue

        old = strip(json.load(open(old_path, encoding='utf-8')), ignored)
        new = strip(json.load(open(new_path, encoding='utf-8')), ignored)
        if old == new:
            continue

        changed += 1
        old_keys, new_keys = keys(old, set()), keys(new, set())
        print('== %s' % name)
        for k in sorted(new_keys - old_keys):
            print('   + key %s' % k)
        for k in sorted(old_keys - new_keys):
            print('   - key %s' % k)
        lines = []
        diff(old, new, '', lines)
        for line in lines:
            print('   ' + line)

    print('%d of %d pages differ' % (changed, len(names)))
    return 1 if changed else 0


if __name__ == '__main__':
    sys.exit(main())
