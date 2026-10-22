"""Compact text view of a page JSON: python show.py dialogs/PartObject.json"""
import sys, json

def opts(c):
    o = c.get('options')
    if not o:
        return ''
    if c['type'] in ('color',):
        return ' {%d colours %s..}' % (len(o), o[0]['value'])
    return ' {' + '; '.join('%s=%s' % (x['value'], x['label']) for x in o) + '}'

def ctl(c, ind):
    if c.get('type') == 'table':
        return '%s[table %s: %s]' % (ind, c.get('aidName'), ', '.join(str(x['member']) for x in c['columns']))
    if c.get('type') == 'button':
        return '%s[button "%s" -> %s]' % (ind, c.get('label'), c.get('opensDialog') or c.get('callback'))
    if 'key' not in c:
        return '%s[%s]' % (ind, c)
    s = '%s- "%s" (%s) %s %s' % (ind, c.get('label'), c.get('labelSource'), c['key'], c['type'])
    if c.get('enableKey'):
        s += ' en=' + c['enableKey']
    if c.get('enableLabel'):
        s += ' enLabel="%s"' % c['enableLabel']
    s += opts(c)
    if c.get('confidence') != 'decoded':
        s += ' <' + str(c.get('confidence')) + '>'
    if c.get('aidNote'):
        s += ' !NOTE'
    return s

def node(n, ind, out):
    for c in n.get('controls', []):
        out.append(ctl(c, ind))
    for g in n.get('groups', []):
        out.append('%s[group "%s"%s]' % (ind, g['label'], (' ' + g['groupSource']) if g.get('groupSource') else ''))
        node(g, ind + '    ', out)
    for t in n.get('tabs', []):
        out.append('%s<tab "%s">' % (ind, t['label']))
        node(t, ind + '    ', out)

d = json.load(open(sys.argv[1], encoding='utf-8'))
out = ['PAGE %s prefix=%s' % (d['page'], d['viewPrefix'])]
node(d, '  ', out)
for sd in d.get('subDialogs', []):
    out.append('  SUBDIALOG %s prefix=%s' % (sd['dialog'], sd['keyPrefix']))
    node(sd, '    ', out)
for c in d.get('siblingPageControls', []):
    out.append(ctl(c, '  SIB(%s) ' % c['shownOnPage']))
for c in d.get('legacyControls', []):
    out.append(ctl(c, '  LEG(%s) ' % c['aidDialog']))
out.append('  UNMAPPED: ' + ' '.join(d['unmappedKeys']))
print('\n'.join(out))
