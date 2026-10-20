Add-Type -AssemblyName PresentationFramework
[System.Windows.Forms.MessageBox]::Show("REMINDER ASTRAL: controlla gli scatti della guardia anti-degenerazione su deepseek 4.1 flash`n`nDove: logs\loop_diagnostics.jsonl (evento guard_triggered, per-modello)`n`nSe il contatore ha dati, verifica quali cause scattano piu'' spesso e magari rivedi temperature/frequency_penalty in llm_core.py","Astral - Reminder", "OK", "Information")
Start-Process "C:\Users\Nicolo\Astral Assistant\logs"
