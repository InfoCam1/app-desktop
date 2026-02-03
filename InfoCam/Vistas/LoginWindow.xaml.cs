using System;
using System.Threading.Tasks;
using System.Windows;
using InfoCam.Models;
using InfoCam.Services;

namespace InfoCam.Views
{
    public partial class LoginWindow : Window
    {
        private readonly ApiService _apiService;

        public LoginWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                StatusText.Text = "Por favor introduce nombre de usuario y contraseña.";
                return;
            }

            StatusText.Text = "Iniciando sesión...";
            LoginButton.IsEnabled = false;

            try
            {
                Usuario user = await _apiService.LoginAsync(username, password);
                if (user != null)
                {
                    // Comprobar si el usuario es administrador
                    if (user.IsAdmin)
                    {
                        // Guardar el usuario en la aplicación para uso posterior
                        App.CurrentUser = user;
                        
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        StatusText.Text = "Acceso denegado.";
                    }
                }
                else
                {
                    StatusText.Text = "Algo ha salido mal. Revisa tus credenciales.";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }
    }
}
