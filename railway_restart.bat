@echo off
echo Backend yeniden baslatiliyor...
cd /d c:\Users\aliye\source\repos\TaxiSignalRBackend
railway redeploy --yes
echo Tamamlandi! Railway deploy edildi.
pause
