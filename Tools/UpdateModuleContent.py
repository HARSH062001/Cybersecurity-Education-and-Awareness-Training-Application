# Previews and applies a selected module-text change with a backup.
"""Review and update existing module text without changing section structure.

Run with --help for examples. Standard library only. Preview is the default;
--apply saves the validated change and creates a timestamped backup.
"""
import argparse
import difflib
import json
import os
from pathlib import Path
import tempfile
from datetime import datetime

ROOT = Path(__file__).resolve().parents[1]
DEFAULT_SOURCE = ROOT / 'CybersecurityTrainingWeb' / 'Data' / 'module-content.json'
MARKERS = {'Learning outcomes', 'Key terms', 'Main topics', 'Workplace examples',
           'Business impact examples', 'Assessment coverage'}

def load(path):
    with path.open(encoding='utf-8-sig') as f:
        data = json.load(f)
    if not isinstance(data, list):
        raise ValueError('Expected a list of modules')
    return data

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', type=Path, default=DEFAULT_SOURCE,
                        help='Module JSON file (default: application content file)')
    parser.add_argument('--module', type=int, required=True, choices=range(1, 9))
    parser.add_argument('--list', action='store_true', help='List blocks and their numbers')
    parser.add_argument('--block', type=int, help='Zero-based block number from --list')
    parser.add_argument('--text-file', type=Path, help='UTF-8 file containing replacement text')
    parser.add_argument('--apply', action='store_true', help='Save after preview and validation')
    args = parser.parse_args()
    data = load(args.source)
    matches = [m for m in data if m.get('id') == args.module]
    if len(matches) != 1:
        parser.error('Expected exactly one matching module')
    module = matches[0]
    blocks = module.get('blocks', [])
    if args.list:
        if args.apply or args.text_file or args.block is not None:
            parser.error('--list cannot be combined with editing options')
        print(module.get('title', ''))
        for i, block in enumerate(blocks):
            print(f"{i}: {block.get('kind')} - {block.get('text', '')[:120]}")
        return
    if args.block is None or args.text_file is None:
        parser.error('Use --list or supply --block and --text-file')
    if not 0 <= args.block < len(blocks):
        parser.error('Block number is out of range')
    block = blocks[args.block]
    if block.get('kind') not in ('paragraph', 'bullet'):
        parser.error('Only paragraph and bullet text can be changed; headings and tables define the page structure')
    original = block.get('text', '')
    replacement = args.text_file.read_text(encoding='utf-8-sig').strip()
    if not replacement:
        parser.error('Replacement text must not be empty')
    if original in MARKERS or replacement in MARKERS:
        parser.error('Section marker text cannot be changed or introduced')
    print(f"Module {args.module}, block {args.block} ({block['kind']})")
    for line in difflib.unified_diff(original.splitlines(), replacement.splitlines(),
                                     fromfile='current', tofile='proposed', lineterm=''):
        print(line)
    if original == replacement:
        print('No change needed.')
        return
    if not args.apply:
        print('Preview only. Add --apply to save this change and create a backup.')
        return
    block['text'] = replacement
    serialized = json.dumps(data, ensure_ascii=False, indent=2) + '\n'
    json.loads(serialized)
    backup = args.source.with_name(args.source.name + '.' + datetime.now().strftime('%Y%m%d-%H%M%S-%f') + '.bak')
    backup.write_bytes(args.source.read_bytes())
    temp_name = None
    try:
        with tempfile.NamedTemporaryFile(mode='w', encoding='utf-8', dir=args.source.parent,
                                         delete=False, newline='\n') as f:
            temp_name = f.name
            f.write(serialized)
        os.replace(temp_name, args.source)
    finally:
        if temp_name and os.path.exists(temp_name):
            os.unlink(temp_name)
    print(f'Saved {args.source}. Backup: {backup}')
    print('Restart or rebuild the application if it is using an older copied content file.')

if __name__ == '__main__':
    try:
        main()
    except (OSError, ValueError, json.JSONDecodeError) as exc:
        raise SystemExit(f'Error: {exc}')
