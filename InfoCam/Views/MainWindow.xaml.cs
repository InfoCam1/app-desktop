using System.Windows;
using System.Windows.Controls;
using InfoCam.Models;

namespace InfoCam.Views
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Default view
            MainContent.Content = new CamerasView();
            SetActiveButton(BtnCamaras);
        }

        private void SetActiveButton(Button activeButton)
        {
            // Reset all buttons
            BtnCamaras.Tag = null;
            BtnIncidencias.Tag = null;
            BtnMapa.Tag = null;
            BtnUsuarios.Tag = null;

            // Set the active button
            activeButton.Tag = "Active";
        }

        private void ShowCameras_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new CamerasView();
            SetActiveButton(BtnCamaras);
        }

        private void ShowIncidencias_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new IncidenciasView();
            SetActiveButton(BtnIncidencias);
        }

        private void ShowMap_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new MapView();
            SetActiveButton(BtnMapa);
        }

        private void ShowUsuarios_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UsuariosView();
            SetActiveButton(BtnUsuarios);
        }

        private void GlobalSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (MainContent.Content is IActionableView view)
            {
                view.Filter(GlobalSearchBox.Text);
            }
        }

        private void GlobalGeneratePdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainContent.Content is IActionableView view)
            {
                view.GenerateReport();
            }
            else
            {
                MessageBox.Show("Esta vista no soporta la generación de informes.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
