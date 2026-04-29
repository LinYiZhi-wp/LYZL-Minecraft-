import re

path = r'C:\Users\Linyizhi\.gemini\GeminiLauncher\Views\VersionSelectorPage.xaml'

with open(path, 'rb') as f:
    raw = f.read()

text = raw.decode('utf-8', errors='replace')

print("Has replacement chars:", '\ufffd' in text)
print("Has &#x2190;:", '&#x2190;' in text)

# Find any Button with Grid.Column="0"
m = re.search(r'<Button[^>]*Grid\.Column="0"[^>]*>', text)
if m:
    print("Button found:", repr(m.group()))
else:
    print("No such button found")

# Show raw bytes around the problem area
# Find "Grid.Column" in raw
idx = raw.find(b'Grid.Column="0"')
if idx != -1:
    print("Raw bytes around Grid.Column=0:", raw[idx-80:idx+120])
