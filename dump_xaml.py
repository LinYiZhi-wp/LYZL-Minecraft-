path = r'C:\Users\Linyizhi\.gemini\GeminiLauncher\Views\VersionSelectorPage.xaml'

with open(path, 'rb') as f:
    raw = f.read()

# Try to detect encoding from BOM or XML declaration
if raw.startswith(b'\xff\xfe'):
    enc = 'utf-16-le'
    raw = raw[2:]
elif raw.startswith(b'\xfe\xff'):
    enc = 'utf-16-be'
    raw = raw[2:]
elif raw.startswith(b'\xef\xbb\xbf'):
    enc = 'utf-8-sig'
else:
    enc = 'utf-8'

print(f"Detected encoding: {enc}")
text = raw.decode(enc, errors='replace')

# Write clean ASCII-safe version
out_path = r'C:\Users\Linyizhi\.gemini\xaml_ascii.txt'
with open(out_path, 'w', encoding='utf-8') as f:
    for i, line in enumerate(text.splitlines(), 1):
        # Mark non-ASCII chars
        safe = ''.join(c if ord(c) < 128 else f'[U+{ord(c):04X}]' for c in line)
        f.write(f"{i:4d}: {safe}\n")

print("Written to xaml_ascii.txt")
