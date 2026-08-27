$loginBody = @{ usuario = 'admin'; password = 'Admin123*' } | ConvertTo-Json

try {
    $loginRes = Invoke-RestMethod -Uri 'http://localhost:5000/api/auth/login' -Method Post -Body $loginBody -ContentType 'application/json'
    $token = $loginRes.token
    Write-Host "=== 1. Token JWT obtenido con exito ==="
    
    $headers = @{ Authorization = "Bearer $token" }
    
    $kpis = Invoke-RestMethod -Uri 'http://localhost:5000/api/dashboard/kpis' -Headers $headers
    Write-Host "`n=== 2. Indicadores KPI (Datos reales PostgreSQL) ==="
    $kpis | Format-List
    
    $eqStatus = Invoke-RestMethod -Uri 'http://localhost:5000/api/dashboard/equipment-status' -Headers $headers
    Write-Host "`n=== 3. Distribucion de Estados de Equipos ==="
    $eqStatus | Format-Table
    
    $actasStatus = Invoke-RestMethod -Uri 'http://localhost:5000/api/dashboard/actas-status' -Headers $headers
    Write-Host "`n=== 4. Distribucion de Estados de Actas ==="
    $actasStatus | Format-Table
    
    $deliv = Invoke-RestMethod -Uri 'http://localhost:5000/api/dashboard/deliveries-by-messenger' -Headers $headers
    Write-Host "`n=== 5. Entregas por Mensajero ==="
    $deliv | Format-Table
    
    $timeline = Invoke-RestMethod -Uri 'http://localhost:5000/api/dashboard/timeline?range=30d' -Headers $headers
    Write-Host "`n=== 6. Linea Temporal (30 dias) ==="
    $timeline | Format-Table
    
    $maint = Invoke-RestMethod -Uri 'http://localhost:5000/api/dashboard/maintenance' -Headers $headers
    Write-Host "`n=== 7. Mantenimientos ==="
    $maint | Format-List
    
    $activity = Invoke-RestMethod -Uri 'http://localhost:5000/api/dashboard/recent-activity?limit=5' -Headers $headers
    Write-Host "`n=== 8. Actividad Reciente ==="
    $activity | Format-Table
    
    $repEquip = Invoke-RestMethod -Uri 'http://localhost:5000/api/reportes/equipos' -Method Post -Headers $headers -Body '{}' -ContentType 'application/json'
    Write-Host "`n=== 9. Reporte de Equipos ==="
    Write-Host "Total equipos en reporte:" $repEquip.Count
    
    $repActas = Invoke-RestMethod -Uri 'http://localhost:5000/api/reportes/actas' -Method Post -Headers $headers -Body '{}' -ContentType 'application/json'
    Write-Host "`n=== 10. Reporte de Actas ==="
    Write-Host "Total actas en reporte:" $repActas.Count
    
    $repEntregas = Invoke-RestMethod -Uri 'http://localhost:5000/api/reportes/entregas' -Method Post -Headers $headers -Body '{}' -ContentType 'application/json'
    Write-Host "`n=== 11. Reporte de Entregas ==="
    Write-Host "Total entregas en reporte:" $repEntregas.Count

    $repDevoluciones = Invoke-RestMethod -Uri 'http://localhost:5000/api/reportes/devoluciones' -Method Post -Headers $headers -Body '{}' -ContentType 'application/json'
    Write-Host "`n=== 12. Reporte de Devoluciones ==="
    Write-Host "Total devoluciones en reporte:" $repDevoluciones.Count
    
    Write-Host "`n>>> TODAS LAS PRUEBAS DE DASHBOARD Y REPORTES PASARON SATISFACTORIAMENTE <<<"
} catch {
    Write-Error $_.Exception.Message
}
