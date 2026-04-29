import subprocess

result = subprocess.run(
    ['dotnet', 'build', r'C:\Users\Linyizhi\.gemini\GeminiLauncher\GeminiLauncher.csproj'],
    capture_output=True
)
for enc in ['gbk', 'utf-8']:
    try:
        out = result.stdout.decode(enc)
        break
    except Exception:
        out = result.stdout.decode('utf-8', errors='replace')

with open(r'C:\Users\Linyizhi\.gemini\build_full.txt', 'w', encoding='utf-8') as f:
    f.write(out)
    f.write('\n---STDERR---\n')
    f.write(result.stderr.decode('utf-8', errors='replace'))
