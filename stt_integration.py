# -*- coding: utf-8 -*-
"""Astral STT - Riconoscimento vocale locale con whisper.cpp (nessuna API cloud)."""
import os
import re
import wave
import traceback
import subprocess
from datetime import datetime

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
WHISPER_DIR = os.path.join(BASE_DIR, "whisper_bin", "Release")
WHISPER_EXE = os.path.join(WHISPER_DIR, "whisper-cli.exe")
WHISPER_MODEL = os.path.join(WHISPER_DIR, "ggml-base.bin")
SAMPLE_RATE = 16000
SILENCE_RMS = 300
MAX_SECONDS = 60
LANG = "it"
WHISPER_TEMP = "0.2"
LOG_FILE = os.path.join(BASE_DIR, "error_log.txt")

def log_error(context, exc):
    """Scrive su error_log.txt con data/ora, contesto e traceback."""
    try:
        with open(LOG_FILE, "a", encoding="utf-8") as lf:
            lf.write("[%s] STT ERROR (%s): %s\n%s%s\n" % (
                datetime.now().strftime("%Y-%m-%d %H:%M:%S"), context, exc,
                traceback.format_exc(), "-" * 50))
    except Exception:
        pass

WHISPER_PROMPT = ("Sessione di chat in italiano con termini tecnici inglesi: computer, software, "
    "hardware, file, folder, download, upload, browser, email, password, login, account, screen, "
    "mouse, keyboard, code, script, Python, Windows, PowerShell, API, server, database, cloud, "
    "bug, feature, update, settings, config, prompt, token, chat, AI, input, output, command.")

try:
    import numpy as np
    import sounddevice as sd
    SD_OK = True
except Exception as e:
    SD_OK = False
    log_error("import sounddevice/numpy", e)

def is_available():
    return SD_OK and os.path.exists(WHISPER_EXE) and os.path.exists(WHISPER_MODEL)

def _record(timeout_silence=2.0, max_seconds=MAX_SECONDS):
    block = int(SAMPLE_RATE * 0.1)
    frames, started, silence = [], False, 0.0
    with sd.RawInputStream(samplerate=SAMPLE_RATE, channels=1, dtype="int16", blocksize=block) as stream:
        elapsed = 0.0
        while elapsed < max_seconds:
            buf, _ = stream.read(block)
            chunk = np.frombuffer(buf, dtype=np.int16)
            rms = float(np.sqrt(np.mean(chunk.astype(np.float32) ** 2)))
            frames.append(chunk.copy())
            elapsed += 0.1
            if rms > SILENCE_RMS:
                started, silence = True, 0.0
            elif started:
                silence += 0.1
                if silence >= timeout_silence:
                    break
    if not started or len(frames) < 2:
        return None
    return np.concatenate(frames)

def transcribe(max_seconds=MAX_SECONDS, timeout_silence=2.5):
    """Registra dal mic e trascrive con whisper-cli. Ritorna testo o None."""
    if not is_available():
        return None
    tmp = os.path.join(WHISPER_DIR, "_tmp_mic.wav")
    try:
        audio = _record(timeout_silence, max_seconds)
        if audio is None:
            log_error("registrazione", "Nessun parlato rilevato (soglia RMS non superata o solo silenzio)")
            return None
        with wave.open(tmp, "wb") as wf:
            wf.setnchannels(1)
            wf.setsampwidth(2)
            wf.setframerate(SAMPLE_RATE)
            wf.writeframes(audio.tobytes())
        res = subprocess.run(
            [WHISPER_EXE, "-m", WHISPER_MODEL, "-f", tmp, "-nt", "--no-timestamps", "-np", "-l", LANG, "--temperature", WHISPER_TEMP, "--prompt", WHISPER_PROMPT, "--no-speech-thold", "0.60"],
            capture_output=True, text=True, encoding="utf-8", errors="replace", timeout=90, cwd=WHISPER_DIR,
        )
        text = (res.stdout or "").strip()
        for marker in ("[BLANK_AUDIO]", "[ Silence ]", "(silence)"):
            text = text.replace(marker, " ")
        # Filtra allucinazioni whisper: tag interi tra parentesi quadre/tonde
        cleaned = " ".join(
            w for w in text.split()
            if not re.fullmatch(r"[\[(][^\])]{0,30}[\])]", w, re.IGNORECASE)
        )
        return cleaned or None
    except Exception as e:
        log_error("transcribe", e)
        return None
    finally:
        try:
            if os.path.exists(tmp):
                os.remove(tmp)
        except Exception:
            pass

