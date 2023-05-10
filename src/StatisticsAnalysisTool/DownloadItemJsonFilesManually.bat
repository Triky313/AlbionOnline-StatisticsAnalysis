powershell -Command "if(!(Test-Path -Path 'Temp')) { New-Item -ItemType Directory -Path 'Temp' } ; Invoke-WebRequest https://raw.githubusercontent.com/Triky313/ao-bin-dumps/main/formatted/items.json -OutFile ItemList.json"
powershell -Command "Invoke-WebRequest https://raw.githubusercontent.com/Triky313/ao-bin-dumps/main/items.json -OutFile Items.json"
powershell -Command "Invoke-WebRequest https://raw.githubusercontent.com/Triky313/ao-bin-dumps/main/mobs.json -OutFile Temp\mobs.json"
