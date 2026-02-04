using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WMSUpdater
{
    public class WmsUpdateService
    {
        public string SourcePath =>
            @"\\192.168.10.45\Adjuntos\ULTIMA VERSION SIS STYBA\WMS_v2";

        public string TargetPath =>
            @"C:\Users\Public\WMS_v2";

        public string ExeName => "WMS_v2.exe";

        public string WmsExePath =>
            Path.Combine(TargetPath, ExeName);

        public async Task UpdateAsync(
            Action<string> log,
            Action<int> progress)
        {
            log("Verificando si WMS está en ejecución...");
            CloseIfRunning(log);

            log("Preparando carpeta destino...");
            Directory.CreateDirectory(TargetPath);

            log("Analizando archivos...");
            var files = Directory.GetFiles(SourcePath, "*", SearchOption.AllDirectories);

            int totalFiles = files.Length;
            int copiedFiles = 0;

            log($"Archivos a copiar: {totalFiles}");

            await Task.Run(() =>
            {
                foreach (var file in files)
                {
                    var relativePath = file.Substring(SourcePath.Length + 1);
                    var destinationFile = Path.Combine(TargetPath, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
                    File.Copy(file, destinationFile, true);

                    copiedFiles++;
                    int percent = (int)((copiedFiles / (double)totalFiles) * 100);

                    progress(percent);
                }
            });

            log("Actualización finalizada.");

            LaunchWms(log);
        }

        private void CloseIfRunning(Action<string> log)
        {
            var processes = Process.GetProcessesByName(
                Path.GetFileNameWithoutExtension(ExeName));

            if (processes.Length == 0)
            {
                log("WMS no está en ejecución.");
                return;
            }

            foreach (var process in processes)
            {
                try
                {
                    log($"Solicitando cierre de {process.ProcessName}...");
                    process.CloseMainWindow();

                    if (!process.WaitForExit(5000))
                    {
                        log("No respondió, forzando cierre...");
                        process.Kill(true);
                        process.WaitForExit();
                    }

                    log("Proceso cerrado correctamente.");
                }
                catch (Exception ex)
                {
                    log("Error al cerrar proceso: " + ex.Message);
                }
            }
        }

        private void LaunchWms(Action<string> log)
        {
            if (!File.Exists(WmsExePath))
            {
                log("⚠ No se encontró WMS_v2.exe para ejecutar.");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = WmsExePath,
                    UseShellExecute = true
                });

                log("🚀 WMS iniciado correctamente.");
            }
            catch (Exception ex)
            {
                log("❌ Error al iniciar WMS:" + ex.Message);
            }
        }
    }
}
