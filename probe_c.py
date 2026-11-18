import sys, os, traceback
OUT = [
 r"C:\Users\masci\Desktop\ASTRAL_PROBE_C.txt",
 r"C:\Users\masci\Documents\Electronic Arts\The Sims 4\ASTRAL_PROBE_C.txt",
 r"C:\Users\masci\Astral\ASTRAL_PROBE_C.txt",
]
def w(msg):
    for p in OUT:
        try:
            f = open(p, "w"); f.write(msg); f.close()
        except Exception:
            pass
try:
    info = []
    info.append("OK python=" + sys.version.replace("\n"," "))
    info.append("executable=" + str(sys.executable))
    try: info.append("cwd=" + os.getcwd())
    except Exception as e: info.append("cwd ERR " + str(e))
    info.append("sys.path=" + " ; ".join(sys.path))
    w("\n".join(info))
except Exception:
    w("ERR " + traceback.format_exc())
