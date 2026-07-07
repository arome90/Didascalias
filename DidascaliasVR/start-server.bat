@echo off
setlocal enabledelayedexpansion
title Servidor de Streaming

:: ============================================================
:: 1. Comprobar Node.js
:: ============================================================
echo Comprobando Node.js...
node -v >nul 2>&1
if !errorlevel! neq 0 (
    echo Node.js no se encontro en tu dispositivo. Descargando instalador...
    curl -o node_installer.msi https://nodejs.org/dist/v20.11.0/node-v20.11.0-x64.msi
    if !errorlevel! neq 0 (
        echo No se pudo descargar Node.js. Revisa tu conexion.
        pause
        exit /b 1
    )
    echo Instalando Node.js...
    msiexec /i node_installer.msi /quiet /norestart
    del node_installer.msi

    :: Recargar PATH para que node sea reconocido
    set "PATH=%PATH%;C:\Program Files\nodejs"
    node -v >nul 2>&1
    if !errorlevel! neq 0 (
        echo Error: reinicia el bat manualmente.
        pause
        exit /b 1
    )
)
echo Node.js encontrado.

:: ============================================================
:: 2. Comprobar e instalar modulos necesarios
:: ============================================================
echo Comprobando modulos de Node...

if not exist node_modules\ws (
    echo Instalando ws...
    call npm install ws
)

if not exist node_modules\express (
    echo Instalando express...
    call npm install express
)

:: serve se instala globalmente
where serve >nul 2>&1
if !errorlevel! neq 0 (
    echo Instalando serve...
    call npm install -g serve
)

echo Modulos listos.

:: ============================================================
:: 3. Lanzar ambos servidores EN SEGUNDO PLANO en esta misma ventana
::    /B evita abrir ventanas nuevas para cada proceso.
:: ============================================================
echo Iniciando server.js...
start /B "" node server.js

echo Iniciando servidor HTML...
start /B "" npx serve . -l 3000

:: ============================================================
:: 4. Informar al usuario
:: ============================================================
echo.
echo =============================================
echo  Servidores iniciados correctamente
echo  WebSocket : ws://localhost:8080
echo  HTML      : http://localhost:3000
echo  (Esta ventana puede minimizarse, no cerrarse)
echo =============================================
echo.

:: Esta ventana se queda abierta esperando. Cerrarla NO mata los
:: procesos /B (quedan huerfanos), por eso Unity los cierra por
:: puerto desde C# usando netstat + taskkill.
:loop
timeout /t 3600 /nobreak >nul
goto loop