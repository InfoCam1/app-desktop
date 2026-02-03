using System;
using System.Text.RegularExpressions;
using System.Windows;
using InfoCam.Models;

namespace InfoCam.Views
{
    public partial class UserFormDialog : Window
    {
        public Usuario Usuario { get; private set; }
        private bool _isEditMode;

        public UserFormDialog(Usuario usuario = null)
        {
            InitializeComponent();
            _isEditMode = usuario != null;

            if (_isEditMode)
            {
                // Edit mode - populate fields
                Title = "Editar Usuario";
                Usuario = usuario;
                UsernameBox.Text = usuario.Username;
                PasswordBox.Password = usuario.Password;
                NombreBox.Text = usuario.Nombre;
                ApellidoBox.Text = usuario.Apellido;
                EmailBox.Text = usuario.Email;
                TelefonoBox.Text = usuario.Telefono.ToString();
                IsAdminCheckBox.IsChecked = usuario.IsAdmin;
            }
            else
            {
                // Create mode
                Title = "Nuevo Usuario";
                Usuario = new Usuario();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            // Validation
            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                ErrorText.Text = "El usuario es obligatorio.";
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ErrorText.Text = "La contraseña es obligatoria.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NombreBox.Text))
            {
                ErrorText.Text = "El nombre es obligatorio.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                ErrorText.Text = "El email es obligatorio.";
                return;
            }

            // Email validation
            if (!IsValidEmail(EmailBox.Text))
            {
                ErrorText.Text = "El formato del email no es válido.";
                return;
            }

            // Telefono validation
            if (!int.TryParse(TelefonoBox.Text, out int telefono))
            {
                ErrorText.Text = "El teléfono debe ser un número válido.";
                return;
            }

            // Update Usuario object
            Usuario.Username = UsernameBox.Text;
            Usuario.Password = PasswordBox.Password;
            Usuario.Nombre = NombreBox.Text;
            Usuario.Apellido = ApellidoBox.Text ?? "";
            Usuario.Email = EmailBox.Text;
            Usuario.Telefono = telefono;
            Usuario.IsAdmin = IsAdminCheckBox.IsChecked ?? false;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }
}
