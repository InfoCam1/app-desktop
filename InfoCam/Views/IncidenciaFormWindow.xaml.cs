using InfoCam.Models;
using InfoCam.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace InfoCam.Views
{
    public partial class IncidenciaFormWindow : Window
    {
        public Incidencia Incidencia { get; private set; }
        private readonly ApiService _apiService;
        private ObservableCollection<string> tiposIncidencias;

        public IncidenciaFormWindow(Incidencia incidencia = null)
        {
            InitializeComponent();

            _apiService = new ApiService(); // Inicializa tu servicio API
            Incidencia = incidencia ?? new Incidencia();

            // Ejecuta FillForm cuando la ventana esté cargada
            Loaded += async (_, __) => await FillForm();
        }

        private async Task FillForm()
        {
            try
            {
                var tipos = await _apiService.GetTiposIncidenciasAsync();

                if (tipos == null || tipos.Count == 0)
                {
                    MessageBox.Show("No se pudieron cargar los tipos de incidencias.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ObservableCollection para refresco inmediato
                tiposIncidencias = new ObservableCollection<string>(tipos);
                TipoCombo.ItemsSource = tiposIncidencias;

                // Selecciona el tipo actual si existe, sino el primero
                if (!string.IsNullOrEmpty(Incidencia.TipoIncidencia) && tiposIncidencias.Contains(Incidencia.TipoIncidencia))
                    TipoCombo.SelectedItem = Incidencia.TipoIncidencia;
                else
                    TipoCombo.SelectedIndex = 0;

                // Resto del formulario
                NombreBox.Text = Incidencia.Nombre;
                CausaBox.Text = Incidencia.Causa;
                
                // Inicializar sliders con valores de la incidencia
                if (Incidencia.Latitud >= LatSlider.Minimum && Incidencia.Latitud <= LatSlider.Maximum)
                {
                    LatSlider.Value = Incidencia.Latitud;
                }
                
                if (Incidencia.Longitud >= LonSlider.Minimum && Incidencia.Longitud <= LonSlider.Maximum)
                {
                    LonSlider.Value = Incidencia.Longitud;
                }
                
                FechaInicioPicker.SelectedDate = Incidencia.Fecha_inicio;
                FechaFinPicker.SelectedDate = Incidencia.Fecha_fin;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando tipos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LatSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LatValueText != null)
            {
                LatValueText.Text = LatSlider.Value.ToString("F2") + "°";
            }
        }

        private void LonSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LonValueText != null)
            {
                LonValueText.Text = LonSlider.Value.ToString("F2") + "°";
            }
        }

        private void UseLocationButton_Click(object sender, RoutedEventArgs e)
        {
            // Coordenadas de ubicaciones típicas del País Vasco
            var locations = new[]
            {
                new { Name = "Bilbao", Lat = 43.263, Lon = -2.935 },
                new { Name = "Vitoria-Gasteiz", Lat = 42.847, Lon = -2.672 },
                new { Name = "San Sebastián", Lat = 43.318, Lon = -1.981 }
            };

            // Seleccionar una ubicación aleatoria
            var random = new Random();
            var location = locations[random.Next(locations.Length)];

            LatSlider.Value = location.Lat;
            LonSlider.Value = location.Lon;

            MessageBox.Show($"Ubicación establecida: {location.Name}", "Ubicación", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorText.Text = "";

                // Validaciones
                if (string.IsNullOrWhiteSpace(NombreBox.Text))
                {
                    ErrorText.Text = "El nombre de la incidencia es obligatorio.";
                    return;
                }

                if (TipoCombo.SelectedItem == null)
                {
                    ErrorText.Text = "Debe seleccionar un tipo de incidencia.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(CausaBox.Text))
                {
                    ErrorText.Text = "La causa de la incidencia es obligatoria.";
                    return;
                }

                if (FechaInicioPicker.SelectedDate == null)
                {
                    ErrorText.Text = "La fecha de inicio es obligatoria.";
                    return;
                }

                if (FechaFinPicker.SelectedDate != null && FechaFinPicker.SelectedDate < FechaInicioPicker.SelectedDate)
                {
                    ErrorText.Text = "La fecha de fin no puede ser anterior a la de inicio.";
                    return;
                }

                // Asignar los datos del formulario a la incidencia
                Incidencia.TipoIncidencia = TipoCombo.SelectedItem?.ToString();
                Incidencia.Nombre = NombreBox.Text;
                Incidencia.Causa = CausaBox.Text;
                Incidencia.Fecha_inicio = FechaInicioPicker.SelectedDate;
                Incidencia.Fecha_fin = FechaFinPicker.SelectedDate;

                // Usar valores de los sliders para coordenadas
                Incidencia.Latitud = LatSlider.Value;
                Incidencia.Longitud = LonSlider.Value;

                // Vincular con el usuario logueado
                if (App.CurrentUser != null)
                {
                    Incidencia.Usuario = new Usuario { Id = App.CurrentUser.Id };
                }

                bool success;



                // Decide si es Create o Update según Id
                if (Incidencia.Id > 0)
                {
                    success = await _apiService.UpdateIncidenciaAsync(Incidencia);
                }
                else
                {
                    success = await _apiService.CreateIncidenciaAsync(Incidencia);
                }

                if (!success)
                {
                    MessageBox.Show("No se pudo guardar la incidencia.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error guardando incidencia: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
