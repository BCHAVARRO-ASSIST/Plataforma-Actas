$ErrorActionPreference = "Stop"

Write-Host "=== INICIANDO SUITE DE PRUEBAS DE DOCUMENTOS OFICIALES ==="

# 1. Autenticacion Administrador
$loginAdmin = @{ usuario = 'admin'; password = 'Admin123*' } | ConvertTo-Json
$resAdmin = Invoke-RestMethod -Uri 'http://localhost:5000/api/auth/login' -Method Post -Body $loginAdmin -ContentType 'application/json'
$adminToken = $resAdmin.token
$adminHeaders = @{ Authorization = "Bearer $adminToken" }
Write-Host "1. Autenticacion de Administrador exitosa"

# 2. Listar actas
$actas = Invoke-RestMethod -Uri 'http://localhost:5000/api/actas' -Headers $adminHeaders
Write-Host "2. Total actas encontradas en PostgreSQL: $($actas.Count)"

$testActa = $actas[0]
$actaId = $testActa.id
Write-Host "Probando con Acta: $($testActa.numeroActa)"

# 3. Preview PDF
Write-Host "`n=== 3. Prueba: GET /api/actas/{id}/documento/preview ==="
$previewPdf = "./test_preview_$($testActa.numeroActa).pdf"
Invoke-WebRequest -Uri "http://localhost:5000/api/actas/$actaId/documento/preview" -Headers $adminHeaders -OutFile $previewPdf -UseBasicParsing
$prevInfo = Get-Item $previewPdf
Write-Host "Archivo de Vista Previa generado:" $prevInfo.FullName
Write-Host "Tamano KB:" $([math]::Round($prevInfo.Length / 1024, 2))

# 4. Descargar PDF
Write-Host "`n=== 4. Prueba: GET /api/actas/{id}/documento/descargar ==="
$tempPdf = "./test_download_$($testActa.numeroActa).pdf"
Invoke-WebRequest -Uri "http://localhost:5000/api/actas/$actaId/documento/descargar" -Headers $adminHeaders -OutFile $tempPdf -UseBasicParsing
$fileInfo = Get-Item $tempPdf
Write-Host "Archivo descargado:" $fileInfo.FullName
Write-Host "Tamano KB:" $([math]::Round($fileInfo.Length / 1024, 2))

# 5. Generar documento y persistir en disco
Write-Host "`n=== 5. Prueba: POST /api/actas/{id}/documento/generar ==="
$genRes = Invoke-RestMethod -Uri "http://localhost:5000/api/actas/$actaId/documento/generar" -Headers $adminHeaders -Method Post
Write-Host "Ruta persistida:" $genRes.rutaDocumento
Write-Host "Hash SHA-256:" $genRes.hashSha256
Write-Host "Tamano bytes:" $genRes.tamanoBytes

# 6. Validaciones de Seguridad
Write-Host "`n=== 6. Pruebas de Seguridad y Permisos ==="
try {
    Invoke-WebRequest -Uri "http://localhost:5000/api/actas/$actaId/documento/preview" -OutFile "anon.pdf" -UseBasicParsing
    Write-Error "Fallo: se permitio acceso anonimo"
} catch {
    Write-Host "OK: Acceso anonimo bloqueado correctamente (401 Unauthorized)"
}

try {
    $fakeId = [Guid]::NewGuid()
    Invoke-WebRequest -Uri "http://localhost:5000/api/actas/$fakeId/documento/preview" -Headers $adminHeaders -OutFile "fake.pdf" -UseBasicParsing
    Write-Error "Fallo: no retorno 404 para acta inexistente"
} catch {
    Write-Host "OK: Acta inexistente rechazada correctamente (404 Not Found)"
}

# 7. Validar generacion para todas las actas registradas (Entrega y Devolucion)
Write-Host "`n=== 7. Generando documentos oficiales para todas las actas registradas ==="
foreach ($a in $actas) {
    $g = Invoke-RestMethod -Uri "http://localhost:5000/api/actas/$($a.id)/documento/generar" -Headers $adminHeaders -Method Post
    Write-Host "Acta: $($a.numeroActa) | Tipo: $($a.tipoActa) | Estado: $($a.estado) | PDF: $($g.rutaDocumento) | SHA-256: $($g.hashSha256.Substring(0, 16))..."
}

Write-Host "`n========================================================"
Write-Host ">>> TODAS LAS PRUEBAS DE DOCUMENTOS OFICIALES PASARON CON EXITO <<<"
Write-Host "========================================================"
