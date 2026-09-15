import time, json
from tools_exec import execute_tool
from exec_logger import read_executions

latenze = []
for i in range(5):
    t0 = time.perf_counter()
    r = execute_tool('scan_storage', {'folder_path': 'C:\\Users\\masci\\Astral', 'min_size_mb': 100})
    dt = (time.perf_counter() - t0) * 1000
    latenze.append(dt)
    print(f'run {i+1}: {dt:.1f} ms | ok={isinstance(r, (dict, list, str))}')
print(f'--- media {sum(latenze)/len(latenze):.1f} ms | min {min(latenze):.1f} | max {max(latenze):.1f}')
entries = read_executions(tool='scan_storage', limit=5)
print(f'--- entries JSONL: {len(entries)}')
if entries:
    print('--- ultimo:', json.dumps(entries[-1], ensure_ascii=False)[:300])

