using System;
using System.Windows;
using System.Windows.Input;

namespace WMSUpdater
{
    public partial class MainWindow : Window
    {
        private bool _isUpdating = false;

        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                MessageBox.Show(
                    e.ExceptionObject.ToString(),
                    "Error no controlado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            };
        }

        // =============================
        // Progreso de barra
        // =============================
        private void SetProgress(int value)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = value;
            });
        }

        // =============================
        // Log / Estado
        // =============================
        private void SetStatus(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText(
                    $"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
                LogTextBox.ScrollToEnd();
            });
        }

        // =============================
        // Botón Actualizar
        // =============================
        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdating)
                return;

            try
            {
                _isUpdating = true;

                UpdateButton.IsEnabled = false;
                CloseButton.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;

                SetStatus("Iniciando actualización...");

                var updater = new WmsUpdateService();
                await updater.UpdateAsync(SetStatus, SetProgress);

                SetStatus("✔ Actualización completada correctamente.");
            }
            catch (Exception ex)
            {
                SetStatus("❌ ERROR: " + ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Error de actualización",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                _isUpdating = false;

                UpdateButton.IsEnabled = true;
                CloseButton.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        // =============================
        // Botón Cerrar
        // =============================
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdating)
            {
                MessageBox.Show(
                    "Hay una actualización en curso.\nEspere a que finalice.",
                    "Actualización en progreso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Close();
        }

        // =============================
        // Evitar cierre forzado
        // =============================
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_isUpdating)
            {
                MessageBox.Show(
                    "No puede cerrar la aplicación durante una actualización.",
                    "Actualización en progreso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }
    }
}
