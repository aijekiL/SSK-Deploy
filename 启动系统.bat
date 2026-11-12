@echo off
rem Auto-open the dashboard on boot (calls start-homepage.ps1 in the same folder).
rem Keep this .bat and start-homepage.ps1 together; in Task Scheduler browse to this .bat directly.
powershell -ExecutionPolicy Bypass -WindowStyle Hidden -File "%~dp0start-homepage.ps1"