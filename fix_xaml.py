import re

path = r'C:\Users\Linyizhi\.gemini\GeminiLauncher\Views\VersionSelectorPage.xaml'

with open(path, 'rb') as f:
    raw = f.read()

# Show line 47 bytes for diagnosis
lines = raw.split(b'\n')
print("Line 47 bytes:", lines[46])
print()

# Decode with replacement chars marked
text = raw.decode('utf-8', errors='replace')
lines_text = text.splitlines()

print("Line 47 decoded:", repr(lines_text[46]))
print()

# Replace the corrupted character (replacement char \ufffd) in Content="..."
# The broken part looks like Content="\xe2\x86?" which is an incomplete UTF-8 sequence
# \xe2\x86\x90 would be ← (U+2190). Let's try to fix by replacing replacement chars.
fixed_line = lines_text[46].replace('\ufffd', '&#x2190;')
print("Fixed line:", repr(fixed_line))
lines_text[46] = fixed_line

new_content = '\r\n'.join(lines_text)
# Ensure proper XML declaration encoding
with open(path, 'w', encoding='utf-8') as f:
    f.write(new_content)

print("Done!")
