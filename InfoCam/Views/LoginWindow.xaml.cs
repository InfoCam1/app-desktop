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
                StatusText.Text = "Please enter both username and password.";
                return;
            }

            StatusText.Text = "Logging in...";
            LoginButton.IsEnabled = false;

            try
            {
                Usuario user = await _apiService.LoginAsync(username, password);
                if (user != null)
                {
                    // Check if user is admin
                    if (user.IsAdmin)
                    {
                        // Store logged-in user globally
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
