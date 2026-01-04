# Crear carpetas si no existen
New-Item -ItemType Directory -Force -Path "Data"
New-Item -ItemType Directory -Force -Path "Models"
New-Item -ItemType Directory -Force -Path "Services"
New-Item -ItemType Directory -Force -Path "Forms"

# Mover archivos a sus lugares correctos
Move-Item -Path "DatabaseHelper.cs" -Destination "Data\" -ErrorAction SilentlyContinue
Move-Item -Path "Models.cs" -Destination "Models\" -ErrorAction SilentlyContinue
Move-Item -Path "*Service.cs" -Destination "Services\" -ErrorAction SilentlyContinue
Move-Item -Path "*Form.cs" -Destination "Forms\" -ErrorAction SilentlyContinue

Write-Host "Archivos organizados correctamente."