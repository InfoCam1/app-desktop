using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using InfoCam.Models;
using InfoCam.Services;
using Microsoft.Web.WebView2.Core;

namespace InfoCam.Views
{
    public partial class MapView : UserControl, IActionableView
    {
        private readonly ApiService _apiService;
        private List<Incidencia> _allIncidencias;
        private List<Camera> _allCameras;
        private string _currentQuery = string.Empty;

        public MapView()
        {
            InitializeComponent();
            _apiService = new ApiService();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await MapBrowser.EnsureCoreWebView2Async();
            MapBrowser.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            await LoadMapDataAsync();
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string message = e.TryGetWebMessageAsString();
                if (message.StartsWith("DBLCLICK:"))
                {
                    string[] parts = message.Substring(9).Split(',');
                    if (parts.Length == 2 && 
                        double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double lat) &&
                        double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double lon))
                    {
                        // Open incident form with pre-filled coordinates
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var incidenciaForm = new IncidenciaFormWindow(lat, lon);
                            if (incidenciaForm.ShowDialog() == true)
                            {
                                // Refresh map after adding incident
                                LoadMapDataAsync();
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing map click: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadMapDataAsync()
        {
            try
            {
                string today = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm");
                var incidenciasTask = _apiService.GetActiveIncidenciasAsync(today);
                var camerasTask = _apiService.GetActiveCamerasAsync();
                
                await Task.WhenAll(incidenciasTask, camerasTask);

                _allIncidencias = incidenciasTask.Result;
                _allCameras = camerasTask.Result;

                string html = GenerateMapHtml(_allIncidencias, _allCameras);
                MapBrowser.NavigateToString(html);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading map data: {ex.Message}", "Error - Map", MessageBoxButton.OK, MessageBoxImage.Error);
                // Volver a un mapa vacío en caso de error
                MapBrowser.NavigateToString(GenerateMapHtml(new List<Incidencia>(), new List<Camera>()));
            }
        }

        private string GenerateMapHtml(List<Incidencia> incidencias, List<Camera> cameras)
        {
            var scriptBuilder = new StringBuilder();

            // Definición del icono verde
            scriptBuilder.AppendLine(@"
                var greenIcon = new L.Icon({
                  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png',
                  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
                  iconSize: [25, 41],
                  iconAnchor: [12, 41],
                  popupAnchor: [1, -34],
                  shadowSize: [41, 41]
                });
            ");

            // Definición del icono rojo
            scriptBuilder.AppendLine(@"
                var redIcon = new L.Icon({
                  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
                  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
                  iconSize: [25, 41],
                  iconAnchor: [12, 41],
                  popupAnchor: [1, -34],
                  shadowSize: [41, 41]
                });
            ");

            // Incidencias
            foreach (var inc in incidencias)
            {
                if (inc.Latitud != 0 && inc.Longitud != 0)
                {
                    string inicio = inc.Fecha_inicio.HasValue ? inc.Fecha_inicio.Value.ToString("dd/MM/yyyy HH:mm") : "N/A";
                    string fin = inc.Fecha_fin.HasValue ? inc.Fecha_fin.Value.ToString("dd/MM/yyyy HH:mm") : "N/A";
                    
                    string popupContent = $"<b>[Incidencia] {inc.TipoIncidencia}</b><br> <b>Nombre:</b>{inc.Nombre}<br><b>Inicio:</b> {inicio}<br><b>Fin:</b> {fin}";
                    
                    if (string.IsNullOrEmpty(inc.External_id))
                    {
                         // ExternalId nulo/vacío (App móvil) -> Rojo
                         scriptBuilder.AppendLine($"L.marker([{inc.Latitud.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {inc.Longitud.ToString(System.Globalization.CultureInfo.InvariantCulture)}], {{icon: redIcon}}).addTo(map).bindPopup('{popupContent} (Mobile)');");
                    }
                    else
                    {
                        // Tiene ExternalId (Fuente externa) -> Azul por defecto
                        scriptBuilder.AppendLine($"L.marker([{inc.Latitud.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {inc.Longitud.ToString(System.Globalization.CultureInfo.InvariantCulture)}]).addTo(map).bindPopup('{popupContent}');");
                    }
                }
            }

            // Cámaras (Verde)
            foreach (var cam in cameras)
            {
                if (cam.Latitud != 0 && cam.Longitud != 0)
                {
                    string popupContent = $"<b>[Cámara] {cam.Nombre}</b>";
                    if (!string.IsNullOrEmpty(cam.Imagen))
                    {
                        popupContent += $"<br><img src='{cam.Imagen}' width='150' style='cursor:pointer;' onclick='openModal(this.src)'/>";
                    }
                    scriptBuilder.AppendLine($"L.marker([{cam.Latitud.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {cam.Longitud.ToString(System.Globalization.CultureInfo.InvariantCulture)}], {{icon: greenIcon}}).addTo(map).bindPopup(\"{popupContent}\");");
                }
            }

            // Centro por defecto
            double centerLat = 43.0;
            double centerLon = -2.5;

            // Intentar centrar en el primer elemento encontrado
            if (incidencias.Any() && incidencias.First().Latitud != 0)
            {
                centerLat = incidencias.First().Latitud;
                centerLon = incidencias.First().Longitud;
            }
            else if (cameras.Any() && cameras.First().Latitud != 0)
            {
                centerLat = cameras.First().Latitud;
                centerLon = cameras.First().Longitud;
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.7.1/dist/leaflet.css' />
    <script src='https://unpkg.com/leaflet@1.7.1/dist/leaflet.js'></script>
    <style>
        body, html {{ height: 100%; margin: 0; padding: 0; }}
        #map {{ height: 100%; background: #333; }}
        
        /* Modal Styles */
        #imgModal {{
            display: none; 
            position: fixed; 
            z-index: 9999; 
            padding-top: 50px; 
            left: 0; 
            top: 0; 
            width: 100%; 
            height: 100%; 
            overflow: auto; 
            background-color: rgba(0,0,0,0.9); 
        }}
        #imgModalContent {{
            margin: auto;
            display: block;
            max-width: 90%;
            max-height: 90%;
        }}
        #closeBtn {{
            position: absolute;
            top: 15px;
            right: 35px;
            color: #f1f1f1;
            font-size: 40px;
            font-weight: bold;
            transition: 0.3s;
            cursor: pointer;
        }}
        #closeBtn:hover,
        #closeBtn:focus {{
            color: #bbb;
            text-decoration: none;
            cursor: pointer;
        }}
    </style>
</head>
<body>
    <div id='map'></div>

    <!-- The Modal -->
    <div id='imgModal' onclick='closeModal()'>
      <span id='closeBtn'>&times;</span>
      <img id='imgModalContent'>
    </div>

    <script>
        var map = L.map('map').setView([{centerLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {centerLon.ToString(System.Globalization.CultureInfo.InvariantCulture)}], 9);

        L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
            attribution: '&copy; <a href=""https://www.openstreetmap.org/copyright"">OpenStreetMap</a> contributors'
        }}).addTo(map);

        {scriptBuilder}

        // Evento de doble clic para añadir incidencia
        map.on('dblclick', function(e) {{
            var lat = e.latlng.lat;
            var lng = e.latlng.lng;
            window.chrome.webview.postMessage('DBLCLICK:' + lat + ',' + lng);
        }});

        // Lógica del modal
        function openModal(src) {{
            var modal = document.getElementById('imgModal');
            var modalImg = document.getElementById('imgModalContent');
            modal.style.display = 'block';
            modalImg.src = src;
        }}

        function closeModal() {{
            var modal = document.getElementById('imgModal');
            modal.style.display = 'none';
        }}
    </script>
</body>
</html>";
        }
        public void Filter(string query)
        {
            _currentQuery = query;
            RefreshMap();
        }

        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            RefreshMap();
        }

        private void Filter_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshMap();
        }

        private void RefreshMap()
        {
            if (_allIncidencias == null || _allCameras == null) return;

            // Filtrar Cámaras
            List<Camera> filteredCameras = new List<Camera>();
            if (FilterCameras.IsChecked == true)
            {
                filteredCameras = string.IsNullOrWhiteSpace(_currentQuery)
                    ? _allCameras
                    : _allCameras.Where(c => c.Nombre != null && c.Nombre.IndexOf(_currentQuery, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            // Filtrar Incidencias
            List<Incidencia> filteredIncidencias = new List<Incidencia>();
            foreach (var inc in _allIncidencias)
            {
                bool isMobile = string.IsNullOrEmpty(inc.External_id);
                bool showMobile = FilterIncidenciasMovil.IsChecked == true;
                bool showTrafico = FilterIncidenciasTrafico.IsChecked == true;

                if (isMobile && showMobile)
                {
                    filteredIncidencias.Add(inc);
                }
                else if (!isMobile && showTrafico)
                {
                    filteredIncidencias.Add(inc);
                }
            }
            
            string html = GenerateMapHtml(filteredIncidencias, filteredCameras);
            MapBrowser.NavigateToString(html);
        }

        public void GenerateReport()
        {
            MessageBox.Show("La generación de informes no está disponible en la vista de mapa.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
