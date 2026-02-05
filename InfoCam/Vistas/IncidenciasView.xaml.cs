using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using InfoCam.Models;
using InfoCam.Services;

namespace InfoCam.Views
{
    public partial class IncidenciasView : UserControl, IActionableView
    {
        private readonly ApiService _apiService;
        private readonly ReportService _reportService;
        private List<Incidencia> _allIncidencias;

        public IncidenciasView()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _reportService = new ReportService();
            _allIncidencias = new List<Incidencia>();
            Loaded += IncidenciasView_Loaded;
        }

        private async void IncidenciasView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadIncidenciasAsync();
        }

        private async Task LoadIncidenciasAsync()
        {
            try
            {
                _allIncidencias = await _apiService.GetIncidenciasAsync();
                IncidenciasGrid.ItemsSource = _allIncidencias;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading incidencias: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Filter(string query)
        {
            if (_allIncidencias == null) return;

            string searchText = query.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                IncidenciasGrid.ItemsSource = _allIncidencias;
            }
            else
            {
                var filtered = _allIncidencias.Where(i => 
                    (i.Nombre != null && i.Nombre.ToLower().Contains(searchText)) ||
                    (i.TipoIncidencia != null && i.TipoIncidencia.ToLower().Contains(searchText)) ||
                    (i.Causa != null && i.Causa.ToLower().Contains(searchText))
                ).ToList();
                IncidenciasGrid.ItemsSource = filtered;
            }
        }

        public void GenerateReport()
        {
            try
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "IncidenciasReport";
                dlg.DefaultExt = ".pdf";
                dlg.Filter = "PDF documents (.pdf)|*.pdf";

                if (dlg.ShowDialog() == true)
                {
                    _reportService.GenerateIncidenciasReport(_allIncidencias, dlg.FileName);
                    MessageBox.Show("Informe generado correctamente.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                 MessageBox.Show($"Error generando informe: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Para crear una nueva incidencia acceda al mapa y haga doble click en el punto deseado", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (IncidenciasGrid.SelectedItem is Incidencia selected)
            {
                var form = new IncidenciaFormWindow(selected);
                if (form.ShowDialog() == true)
                {
                    await LoadIncidenciasAsync();
                }
            }
            else
            {
                MessageBox.Show("Selecciona una incidencia para editar.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = IncidenciasGrid.SelectedItems.Cast<Incidencia>().ToList();
            
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Selecciona al menos una incidencia para eliminar.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string confirmMessage = selectedItems.Count == 1
                ? $"¿Eliminar la incidencia '{selectedItems[0].Nombre}'?"
                : $"¿Eliminar {selectedItems.Count} incidencias seleccionadas?";

            if (MessageBox.Show(confirmMessage, "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    int successCount = 0;
                    int failCount = 0;

                    foreach (var incidencia in selectedItems)
                    {
                        bool success = await _apiService.DeleteIncidenciaAsync(incidencia.Id);
                        if (success)
                            successCount++;
                        else
                            failCount++;
                    }

                    await LoadIncidenciasAsync();

                    if (failCount == 0)
                    {
                        MessageBox.Show($"Se eliminaron {successCount} incidencia(s) correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}
