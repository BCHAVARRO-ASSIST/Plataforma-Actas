$body = @{
    Usuario = "admin"
    Password = "password"
} | ConvertTo-Json

try {
    Write-Host "TEST 1: AUTENTICACION CON 'admin'/'password'"
    $login = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $body -ContentType "application/json"
    Write-Host "Token JWT emitido exitosamente: " ($login.Token.Length -gt 0)
    Write-Host "Usuario autenticado: " $login.Usuario.Nombre $login.Usuario.Apellido
    Write-Host "Rol asignado: " $login.Rol.Nombre
    Write-Host "Permisos totales: " $login.Permisos.Count

    $headers = @{
        Authorization = "Bearer " + $login.Token
    }

    Write-Host "`nTEST 2: OBTENER INVENTARIO DE EQUIPOS"
    $equip = Invoke-RestMethod -Uri "http://localhost:5000/api/equipment" -Method Get -Headers $headers
    Write-Host "Equipos disponibles en inventario: " $equip.Count

    Write-Host "`nTEST 3: OBTENER LISTADO DE ACTAS"
    $actas = Invoke-RestMethod -Uri "http://localhost:5000/api/actas" -Method Get -Headers $headers
    Write-Host "Actas registradas en sistema: " $actas.Count

    Write-Host "`n=== PRUEBA DE INTEGRACION COMPLETADA CON EXITO (200 OK) ==="
}
catch {
    Write-Host "Respuesta de autenticacion:" $_.Exception.Message
}
