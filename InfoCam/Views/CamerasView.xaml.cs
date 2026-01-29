using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Controls;
using InfoCam.Models;
using InfoCam.Services;

namespace InfoCam.Views
{
    public partial class CamerasView : UserControl, IActionableView
    {
        private readonly ApiService _apiService;
        private readonly ReportService _reportService;
        private List<Camera> _allCameras;

        public CamerasView()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _reportService = new ReportService();
            _allCameras = new List<Camera>();
            Loaded += CamerasView_Loaded;
        }

        private async void CamerasView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCamerasAsync();
        }

        private async Task LoadCamerasAsync()
        {
            try
            {
                _allCameras = await _apiService.GetCamerasAsync();
                CamerasGrid.ItemsSource = _allCameras;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading cameras: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Filter(string query)
        {
            if (_allCameras == null) return;

            string searchText = query.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                CamerasGrid.ItemsSource = _allCameras;
            }
            else
            {
                var filtered = _allCameras.Where(c => 
                    (c.Nombre != null && c.Nombre.ToLower().Contains(searchText))
                ).ToList();
                CamerasGrid.ItemsSource = filtered;
            }
        }

        public void GenerateReport()
        {
            try
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "CamarasReport";
                dlg.DefaultExt = ".pdf";
                dlg.Filter = "PDF documents (.pdf)|*.pdf";

                if (dlg.ShowDialog() == true)
                {
                    _reportService.GenerateCamerasReport(_allCameras, dlg.FileName);
                    MessageBox.Show("Informe generado correctamente.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                 MessageBox.Show($"Error generando informe: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = CamerasGrid.SelectedItems.Cast<Camera>().ToList();
            
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Selecciona al menos una cámara para eliminar.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string confirmMessage = selectedItems.Count == 1
                ? $"¿Eliminar la cámara '{selectedItems[0].Nombre}'?"
                : $"¿Eliminar {selectedItems.Count} cámaras seleccionadas?";

            if (MessageBox.Show(confirmMessage, "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    int successCount = 0;
                    int failCount = 0;

                    foreach (var camera in selectedItems)
                    {
                        bool success = await _apiService.DeleteCameraAsync(camera.Id);
                        if (success)
                            successCount++;
                        else
                            failCount++;
                    }

                    await LoadCamerasAsync();

                    if (failCount == 0)
                    {
                        MessageBox.Show($"Se eliminaron {successCount} cámara(s) correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Eliminadas: {successCount}\nFallidas: {failCount}", "Resultado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ViewGallery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var activeCameras = await _apiService.GetActiveCamerasAsync();
                
                if (activeCameras == null || activeCameras.Count == 0)
                {
                    MessageBox.Show("No hay cámaras activas para visualizar.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                CameraGalleryWindow gallery = new CameraGalleryWindow(activeCameras);
                gallery.Owner = Window.GetWindow(this);
                gallery.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar cámaras activas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport();
        }
    }
}
