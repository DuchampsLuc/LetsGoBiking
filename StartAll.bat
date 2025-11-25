@echo off
title Smart Startup - Proxy + Backend + Front
setlocal ENABLEDELAYEDEXPANSION

:: 🔧 Se place dans le dossier du .bat (ÉLÉMENT ESSENTIEL)
cd /d "%~dp0"

echo ============================================
echo 🚀 STARTUP INTELLIGENT : Proxy + Backend + Front
echo ============================================

:: --- Vérif si le script est lancé en admin ---
net session >nul 2>&1
if %errorlevel% NEQ 0 (
    echo ❗ Le script doit être lance en administrateur.
    echo 👉 Relancement automatique avec privilèges admin...
    powershell -Command "Start-Process '%~f0' -Verb runAs"
    exit /b
)

echo ✔ Démarrage en mode administrateur.

:: -------------------------------------------------
:: 0) Lancement d'ActiveMQ
:: -------------------------------------------------
echo.
echo 🚀 Lancement d'ActiveMQ...
start "" "activemq.bat" start

:: Attendre quelques secondes pour que le broker soit opérationnel
timeout /t 5 >nul



:: -------------------------------------------------
:: 1) Vérification du port 8080 (Front)
:: -------------------------------------------------
echo.
echo 🔍 Vérification du port 8080...

for /f "tokens=5" %%a in ('netstat -ano ^| findstr :8080 ^| findstr LISTENING') do set PORTPID=%%a

if defined PORTPID (
    echo ❌ Le port 8080 est occupé par le PID : %PORTPID%
    echo.
    echo 👉 Processus utilisant le port :
    tasklist /FI "PID eq %PORTPID%"
    echo.
    echo ❗ Le front ne pourra pas démarrer tant que ce process utilise 8080.
    pause
) else (
    echo ✔ Port 8080 disponible.
)


:: -------------------------------------------------
:: 2) Lancement du Proxy (Self-host)
:: -------------------------------------------------
echo.
echo 🚀 Lancement du PROXY SOAP...
start "" "%CD%\ServiceProxyBike\ServiceProxyBike\bin\Debug\ServiceProxyBike.exe"
timeout /t 2 >nul


:: -------------------------------------------------
:: 3) Lancement du Backend (Self-host)
:: -------------------------------------------------
echo.
echo 🚀 Lancement du BACKEND...
start "" "%CD%\BackendBiking\BackendBiking\bin\Debug\BackendBiking.exe"
timeout /t 2 >nul


:: -------------------------------------------------
:: 4) Lancement du Front (HTTP Server)
:: -------------------------------------------------
echo.
echo 🚀 Lancement du FRONT-END sur http://localhost:8080 ...
start cmd /k "cd /d %CD%\HarmoWeb && py -m http.server 8080"

echo.
echo ============================================
echo ✔ Tous les services sont lances.
echo ============================================
echo.
pause
exit /b
