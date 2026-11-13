from pathlib import Path
from zipfile import ZipFile
import xml.etree.ElementTree as ET
import json, re, shutil
from datetime import datetime

project=Path(__file__).resolve().parents[1]
path=project/'CybersecurityTrainingWeb'/'Data'/'module-content.json'
docs=project/'01_Module Content'
backup_dir=project/'output'/'module-content-backups'
w='{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
data=json.loads(path.read_text(encoding='utf-8-sig'))
assert sorted(m['id'] for m in data)==list(range(1,9))
names=['purpose','outcomes','terms','concepts','workplace','behaviours','review']
markers=['Learning outcomes','Key terms','Main topics','Workplace examples','Business impact examples','Assessment coverage']

def block(kind,text='',rows=None):
 return {'kind':kind,'text':text,'rows':rows or []}

def category(text, style, current):
 if not style.startswith('Heading1'): return current
 t=text.strip().lower()
 if t in ('what learners should be able to do','purpose and learning outcomes'): return 'outcomes'
 if t=='key terms': return 'terms'
 if t in ('topics covered','practical guidance'): return 'concepts'
 if t in ('workplace examples','a realistic sign in moment','follow the information life cycle','online choices in a workday','a day with a work device','four questions before using an ai output','a simple report should answer'): return 'workplace'
 if t.startswith('how a ') or t in ('safer habits to use every day','safer responses to use every day','habits to take back to work','employee checklist','quick self check'): return 'behaviours'
 if t in ('assessment coverage','assessment design','module summary','references'): return 'review'
 return current

for m in data:
 files=list(docs.glob(f"Module {m['id']} *docx"))
 if len(files)!=1: raise ValueError(f'Expected one source document for module {m["id"]}: {files}')
 with ZipFile(files[0]) as z: doc=ET.fromstring(z.read('word/document.xml'))
 groups={n:[] for n in names}; current='purpose'; table_count=0; extracted=[]
 for e in doc.find(w+'body'):
  if e.tag==w+'p':
   text=''.join(t.text or '' for t in e.iter(w+'t')).strip()
   if not text: continue
   style_node=e.find('./'+w+'pPr/'+w+'pStyle')
   style=style_node.get(w+'val','') if style_node is not None else ''
   if re.match(r'^Module\s+\d+\s',text,re.I) and len(text)<100: continue
   if text.startswith('Employee learning module') and 'Cybersecurity Education and Awareness Training Application' in text: continue
   new=category(text,style,current); current=new
   # Structural markers are supplied separately, not duplicated in the learning content.
   if text.strip().lower() in ('key terms','workplace examples','assessment coverage'): continue
   kind='heading' if style.startswith('Heading') or text.lower() == 'module purpose' else 'bullet' if style.lower().startswith('listbullet') else 'paragraph'
   groups[current].append(block(kind,text)); extracted.append(text)
  elif e.tag==w+'tbl':
   rows=[]
   for row in e.findall(w+'tr'):
    cells=[]
    for cell in row.findall(w+'tc'):
     paragraphs=[''.join(t.text or '' for t in p.iter(w+'t')).strip() for p in cell.findall('.//'+w+'p')]
     cells.append(' '.join(t for t in paragraphs if t))
    if any(cells): rows.append(cells)
   if rows:
    groups['terms' if table_count==0 else 'behaviours'].append(block('table',rows=rows)); table_count+=1
 if table_count!=3: raise ValueError(f'Module {m["id"]}: expected 3 tables, found {table_count}')
 for n in names:
  if not groups[n]: raise ValueError(f'Module {m["id"]}: empty {n} section')
 # Scenario-led documents also contain a concise overview under learning outcomes.
 # Show that overview on the catalogue and module header before the scenario.
 if m['id'] >= 4:
  overview=next((b['text'] for b in groups['outcomes'] if b['kind']=='paragraph'),None)
  if not overview: raise ValueError(f'Module {m["id"]}: missing overview paragraph')
  groups['purpose'].insert(0,block('paragraph',overview))
 blocks=[]
 for i,n in enumerate(names):
  if i: blocks.append(block('heading',markers[i-1]))
  blocks.extend(groups[n])
 m['blocks']=blocks
 for text in extracted:
  if not any(b['text']==text for b in blocks): raise ValueError(f'Missing source text in module {m["id"]}')
 print(f'Module {m["id"]}: {len(extracted)} paragraphs/headings, {table_count} source tables')

output=json.dumps(data,ensure_ascii=False,indent=2)+'\n'
json.loads(output)
if json.loads(output) == json.loads(path.read_text(encoding='utf-8-sig')):
 print('Module content is already up to date.')
 raise SystemExit(0)
backup_dir.mkdir(parents=True,exist_ok=True)
backup=backup_dir/('module-content-before-word-sync-'+datetime.now().strftime('%Y%m%d-%H%M%S')+'.json')
shutil.copy2(path,backup)
path.write_text(output,encoding='utf-8',newline='\n')
print('Saved:',path)
print('Previous version:',backup)

