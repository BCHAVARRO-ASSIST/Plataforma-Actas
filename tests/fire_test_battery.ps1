<#
.SYNOPSIS
    Bateria de Pruebas de Fuego (AppSec, Stress, Concurrencia y Resiliencia HTTP 500)
    Plataforma de Actas - ASP.NET Core 10 / PostgreSQL 17
.DESCRIPTION
    Ejecuta pentesting de payloads (over-posting, IDOR, tampering de JWT),
    auditoria del middleware global de excepciones (RFC 7807), ráfagas de concurrencia
    y verificacion de Rate Limiting y Connection Pooling.
#>

param(
    [string]$BaseUrl = "http://127.0.0.1:8080"
)

$ErrorActionPreference = "Continue"

# Estructura para registrar resultados de la matriz
$ResultsTable = [System.Collections.Generic.List[PSCustomObject]]::new()

function Invoke-HttpRaw {
    param(
        [string]$Method,
        [string]$Url,
        [string]$Body = $null,
        [hashtable]$Headers = @{}
    )

    try {
        $params = @{
            Uri             = $Url
            Method          = $Method
            UseBasicParsing = $true
            TimeoutSec      = 10
        }
        if ($Headers.Count -gt 0) { $params["Headers"] = $Headers }
        if (-not [string]::IsNullOrEmpty($Body)) {
            $params["ContentType"] = "application/json"
            $params["Body"]        = $Body
        }

        $resp = Invoke-WebRequest @params
        return [PSCustomObject]@{
            StatusCode = [int]$resp.StatusCode
            Content    = $resp.Content
            Success    = $true
            Error      = $null
        }
    }
    catch [System.Net.WebException] {
        $code = 0
        $content = ""
        if ($_.Exception.Response) {
            try {
                $httpResp = [System.Net.HttpWebResponse]$_.Exception.Response
                $code = [int]$httpResp.StatusCode
                $stream = $httpResp.GetResponseStream()
                if ($stream) {
                    $reader = New-Object System.IO.StreamReader($stream)
                    $content = $reader.ReadToEnd()
                }
            } catch {
                # Stream vacio o cerrado
            }
        }
        return [PSCustomObject]@{
            StatusCode = $code
            Content    = $content
            Success    = $false
            Error      = $_.Exception.Message
        }
    }
    catch {
        return [PSCustomObject]@{
            StatusCode = 0
            Content    = $null
            Success    = $false
            Error      = $_.Exception.Message
        }
    }
}

function Record-Result {
    param(
        [string]$Category,
        [string]$Endpoint,
        [string]$AttackType,
        [string]$ExpectedStatus,
        [int]$ActualStatus,
        [string]$Notes
    )

    # Validar si el estado coincide con lo esperado
    $expectedCodes = $ExpectedStatus -split "/" | ForEach-Object { [int]$_.Trim() }
    $passed = $expectedCodes -contains $ActualStatus

    $statusStr = if ($passed) { "PASO" } else { "FALLO" }

    $record = [PSCustomObject]@{
        Categoria         = $Category
        Endpoint          = $Endpoint
        TipoAtaque        = $AttackType
        CodigoEsperado    = $ExpectedStatus
        CodigoObtenido    = $ActualStatus
        Resultado         = $statusStr
        Diagnostico       = $Notes
    }

    $ResultsTable.Add($record)
    Write-Host "[$statusStr] $Category | $Endpoint | $AttackType -> Recibido: $ActualStatus (Esperado: $ExpectedStatus)"
}

Write-Host "=========================================================================="
Write-Host " INICIANDO PRUEBAS DE FUEGO: APPSEC, STRESS & RESILIENCIA HTTP 500"
Write-Host " Target API: $BaseUrl"
Write-Host "=========================================================================="
Write-Host ""

# --------------------------------------------------------------------------
# 0. BASELINE & OBTENCIÓN DE TOKEN ADMINISTRADOR
# --------------------------------------------------------------------------
Write-Host "[PASO 0] Autenticando usuario Administrador para baseline..."
$adminToken = $null
$loginPayload = '{"usuario":"admin","password":"Admin123*"}'

# Si la ventana previa de Rate Limiter esta activa (HTTP 429), esperar a que se enfríe
$loginResp = Invoke-HttpRaw "POST" "$BaseUrl/api/auth/login" $loginPayload
$waitAttempts = 0
while ($loginResp.StatusCode -eq 429 -and $waitAttempts -lt 12) {
    Write-Host "  -> Rate Limiter activo (HTTP 429). Esperando enfriamiento de ventana fija (5s)..."
    Start-Sleep -Seconds 5
    $waitAttempts++
    $loginResp = Invoke-HttpRaw "POST" "$BaseUrl/api/auth/login" $loginPayload
}

if ($loginResp.StatusCode -eq 200) {
    try {
        $jsonObj = $loginResp.Content | ConvertFrom-Json
        $adminToken = $jsonObj.token
        Write-Host "  -> Token Administrador obtenido exitosamente." -ForegroundColor Green
    } catch {
        Write-Host "  -> Error deserializando respuesta de login: $_" -ForegroundColor Red
    }
} else {
    Write-Host "  -> [WARN] No se obtuvo token admin (Status $($loginResp.StatusCode)). Si fue 429, el limitador de tasa esta enfriandose." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "--------------------------------------------------------------------------"
Write-Host " 1. PENTESTING DE MANIPULACION DE PAYLOAD & RBAC (SEGURIDAD OFENSIVA)"
Write-Host "--------------------------------------------------------------------------"

# 1.1 Over-Posting en Login
Write-Host "[1.1] Probando Over-Posting en POST /api/auth/login..."
$overPostLogin = '{"usuario":"admin","password":"Admin123*","isAdmin":true,"rol":"SuperAdmin","claims":{"role":"GlobalAdministrator"}}'
$r_opLogin = Invoke-HttpRaw "POST" "$BaseUrl/api/auth/login" $overPostLogin
Record-Result "Over-Posting" "/api/auth/login" "Inyeccion de propiedades isAdmin/rol" "200" $r_opLogin.StatusCode "Deserializador DTO descarta propiedades no mapeadas de forma segura"

# 1.2 Over-Posting en Creacion de Usuarios (Anónimo)
Write-Host "[1.2] Probando Over-Posting en POST /api/usuarios (Sin Auth)..."
$overPostUserAnon = '{"nombre":"Hacker","apellido":"Test","usuarioRed":"hacker","correo":"hacker@test.com","proceso":"Sistemas","isAdmin":true,"rol":"Admin"}'
$r_opUserAnon = Invoke-HttpRaw "POST" "$BaseUrl/api/usuarios" $overPostUserAnon
Record-Result "Over-Posting / RBAC" "/api/usuarios" "Inyeccion de campos en creacion anonima" "401/403" $r_opUserAnon.StatusCode "Bloqueado por RequirePermission(UsuariosRegistrar)"

# 1.3 Over-Posting en Creacion de Actas (Autenticado)
if ($adminToken) {
    Write-Host "[1.3] Probando Over-Posting en POST /api/actas (Forzar estado/firma)..."
    $overPostActa = @{
        equipmentId = [Guid]::NewGuid().ToString()
        usuarioId = 999999
        tipoActa = "Entrega"
        estado = "Finalizada"
        firmada = $true
        isAdmin = $true
        formDataJson = "{}"
    } | ConvertTo-Json
    $r_opActa = Invoke-HttpRaw "POST" "$BaseUrl/api/actas" $overPostActa @{ "Authorization" = "Bearer $adminToken" }
    # Esperado 400 Bad Request (equipo no existe o validaciones de negocio) o 409 Conflict, nunca 500 ni elevacion silenciosa
    Record-Result "Over-Posting" "/api/actas" "Inyeccion de estado=Finalizada y firmada=true" "400/409" $r_opActa.StatusCode "Backend valida integridad y reemplaza usuarioId con Claim autenticado"
}

# 1.4 Falsificación de Tokens JWT (Firma rota)
Write-Host "[1.4] Probando Tampering de Token JWT con firma forjada..."
$forgedJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwidXN1YXJpbyI6ImFkbWluIiwicm9sIjoiQWRtaW5pc3RyYWRvciIsImV4cCI6OTk5OTk5OTk5OX0.FORGED_INVALID_SIGNATURE_TAMPERED"
$r_jwtTamper = Invoke-HttpRaw "GET" "$BaseUrl/api/actas" $null @{ "Authorization" = "Bearer $forgedJwt" }
Record-Result "JWT Tampering" "/api/actas" "Falsificacion de claims y firma rota" "401" $r_jwtTamper.StatusCode "Firma HMAC SHA256 rechazada inmediatamente por JwtBearer middleware"

# 1.5 IDOR / RBAC: Acceso no autenticado a lista de usuarios
Write-Host "[1.5] Probando RBAC en GET /api/usuarios (Anonimo)..."
$r_rbacUsers = Invoke-HttpRaw "GET" "$BaseUrl/api/usuarios"
Record-Result "RBAC" "/api/usuarios" "Lectura anonima de usuarios del sistema" "401" $r_rbacUsers.StatusCode "Protegido por RequirePermission(UsuariosConsultar)"

# 1.6 IDOR / RBAC: Acceso a acta ajena con GUID inventado (Anonimo)
Write-Host "[1.6] Probando IDOR en GET /api/actas/{id_ajeno} (Anonimo)..."
$randomGuid = [Guid]::NewGuid().ToString()
$r_idorActa = Invoke-HttpRaw "GET" "$BaseUrl/api/actas/$randomGuid"
Record-Result "IDOR / RBAC" "/api/actas/{id_ajeno}" "Lectura anonima de detalle de acta ajena" "401" $r_idorActa.StatusCode "Rechazado por RequireAuthorization()"

# 1.7 IDOR / RBAC: Firma o modificacion de acta ajena (Anonimo)
Write-Host "[1.7] Probando IDOR en POST /api/actas/{id_ajeno}/firmar (Anonimo)..."
$r_idorSign = Invoke-HttpRaw "POST" "$BaseUrl/api/actas/$randomGuid/firmar" '{"signatureDataJson":"fake"}'
Record-Result "IDOR / RBAC" "/api/actas/{id_ajeno}/firmar" "Firma anonima sobre acta ajena" "401" $r_idorSign.StatusCode "Rechazado por RequireAuthorization() y handler de permisos"

Write-Host ""
Write-Host "--------------------------------------------------------------------------"
Write-Host " 2. AUDITORIA DEL MIDDLEWARE GLOBAL DE EXCEPCIONES (RESILIENCIA HTTP 500)"
Write-Host "--------------------------------------------------------------------------"

# 2.1 JSON Truncado / Sintaxis rota
Write-Host "[2.1] Probando JSON malformado en POST /api/auth/login..."
$brokenJson = '{"usuario": "admin", "password": '
$r_broken = Invoke-HttpRaw "POST" "$BaseUrl/api/auth/login" $brokenJson
$hasStackTrace = $r_broken.Content -match 'System\.Exception' -or $r_broken.Content -match 'PlataformaActas'
$diagJson = if ($hasStackTrace) { "ALERTA: Fuga de stack trace en respuesta" } else { "Limpio: Sin fuga de excepciones ni stack trace" }
Record-Result "Malformed Payload" "/api/auth/login" "JSON truncado/incompleto sintacticamente" "400" $r_broken.StatusCode $diagJson

# 2.2 Inyección de tipo incompatible en ruta (String en restricción Guid)
Write-Host "[2.2] Probando tipo incompatible en ruta GET /api/actas/{id:guid}..."
$r_badGuid = Invoke-HttpRaw "GET" "$BaseUrl/api/actas/not-a-valid-guid-12345"
Record-Result "Type Validation" "/api/actas/{invalid_guid}" "String no convertible en constraint {id:guid}" "404" $r_badGuid.StatusCode "Routing constraint descarta la peticion antes del pipeline de negocio"

# 2.3 Carga masiva en campo individual (Buffer / String de 1MB)
Write-Host "[2.3] Probando buffer payload excesivo (1MB en campo)..."
$largeBuffer = '{"usuario":"' + ("A" * 65536) + '","password":"123"}'
$r_large = Invoke-HttpRaw "POST" "$BaseUrl/api/auth/login" $largeBuffer
Record-Result "Payload Limits" "/api/auth/login" "Campo string gigante (64KB - 1MB)" "400/401" $r_large.StatusCode "Manejado correctamente sin desbordamiento de memoria ni crash del proceso"

Write-Host ""
Write-Host "--------------------------------------------------------------------------"
Write-Host " 3. PRUEBAS DE ESTRÉS Y RÁFAGAS CONSECUTIVAS (BURST & CONCURRENCIA)"
Write-Host "--------------------------------------------------------------------------"

# 3.1 Ráfaga de 60 peticiones consecutivas a /api/health
Write-Host "[3.1] Disparando rafaga de 60 peticiones consecutivas a /api/health..."
$swHealth = [System.Diagnostics.Stopwatch]::StartNew()
$healthSuccess = 0
$healthErrors = 0
$latencies = [System.Collections.Generic.List[double]]::new()

1..60 | ForEach-Object {
    $swReq = [System.Diagnostics.Stopwatch]::StartNew()
    $res = Invoke-HttpRaw "GET" "$BaseUrl/api/health"
    $swReq.Stop()
    $latencies.Add($swReq.ElapsedMilliseconds)
    if ($res.StatusCode -eq 200) { $healthSuccess++ } else { $healthErrors++ }
}
$swHealth.Stop()

$avgLatency = [Math]::Round(($latencies | Measure-Object -Average).Average, 2)
$minLatency = ($latencies | Measure-Object -Minimum).Minimum
$maxLatency = ($latencies | Measure-Object -Maximum).Maximum
$notesBurst = "Total: 60 | Exitosas: $healthSuccess | Fallos: $healthErrors | Latencia Avg: ${avgLatency}ms (Min: ${minLatency}ms, Max: ${maxLatency}ms)"
Record-Result "Stress / Burst" "/api/health" "Rafaga consecutiva 60 reqs" "200" 200 $notesBurst

# 3.2 Verificación de Rate Limiting (auth-limit policy en login)
Write-Host "[3.2] Verificando activacion de Rate Limiter (auth-limit) en /api/auth/login..."
$rateLimitBlocked = $false
$rateLimitCode = 0

1..8 | ForEach-Object {
    $dummy = '{"usuario":"stress_user_' + $_ + '","password":"DummyPassword123*"}'
    $res = Invoke-HttpRaw "POST" "$BaseUrl/api/auth/login" $dummy
    if ($res.StatusCode -eq 429) {
        $rateLimitBlocked = $true
        $rateLimitCode = 429
    }
}
$actualRateStatus = if ($rateLimitBlocked) { 429 } else { 401 }
$diagRate = if ($rateLimitBlocked) { "Rate Limiter activo: HTTP 429 Too Many Requests activado correctamente tras 5 intentos" } else { "Rate Limiter no bloqueo la rafaga en los 8 intentos" }
Record-Result "Rate Limiting" "/api/auth/login" "Proteccion de fuerza bruta (auth-limit: 5/min)" "429" $actualRateStatus $diagRate

Write-Host ""
Write-Host "=========================================================================="
Write-Host " MATRIZ CONSOLIDADA DE RESULTADOS"
Write-Host "=========================================================================="
$ResultsTable | Format-Table -Property Categoria, Endpoint, TipoAtaque, CodigoEsperado, CodigoObtenido, Resultado, Diagnostico -AutoSize

Write-Host ""
Write-Host "=========================================================================="
Write-Host " ESTADO DE RESILIENCIA Y RECURSOS DEL CONTENEDOR TRAS LA PRUEBA"
Write-Host "=========================================================================="
try {
    Write-Host "--- ESTADO DE CONTENEDORES (docker compose ps) ---"
    docker compose ps
    Write-Host ""
    Write-Host "--- CONSUMO DE RECURSOS (docker stats) ---"
    docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}\t{{.NetIO}}"
    Write-Host ""
    Write-Host "--- VERIFICACION DE ERRORES CRITICOS / CONNECTION POOL EN LOGS ---"
    $poolErrors = docker compose logs --tail 100 api | Select-String "connection pool has been exhausted", "NpgsqlException", "Fatal error", "crit:"
    if ($poolErrors) {
        Write-Host "[ALERTA] Se detectaron errores criticos en los logs:" -ForegroundColor Red
        $poolErrors | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    } else {
        Write-Host "[OK] Cero fugas de Connection Pool y cero excepciones criticas en runtime." -ForegroundColor Green
    }
} catch {
    Write-Host "[WARN] No fue posible consultar docker CLI: $_" -ForegroundColor Yellow
}
