# ChatHub - Versione macOS

## Per il conoscente che compila

### Requisiti
- Python 3.7+ installato (scaricabile da https://www.python.org/downloads/)

### Compilazione con PyInstaller
1. Apri il Terminale
2. Naviga nella cartella dove hai estratto ChatHub_Mac
3. Esegui:
   `
   pip install pyinstaller
   pyinstaller --onefile --name "ChatHub" --noconsole chat_hub_single.py
   `
4. L'eseguibile sarà in dist/ChatHub

### Avvio
Doppio clic su dist/ChatHub o da terminale:
`
./dist/ChatHub
`

### Note
- Funziona in LAN (stessa rete WiFi)
- Si apre automaticamente il browser
- Nessun file da installare separatamente
- L'eseguibile è autocontenuto (~10 MB)
