using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using InfoCam.Models;
using InfoCam.Services;

namespace InfoCam.Views
{
    public partial class UsuariosView : UserControl, IActionableView
    {
        private readonly ApiService _apiService;
        private List<Usuario> _allUsuarios;

        public UsuariosView()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _allUsuarios = new List<Usuario>();
            Loaded += UsuariosView_Loaded;
        }

        private async void UsuariosView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsuariosAsync();
        }

        private async Task LoadUsuariosAsync()
        {
            try
            {
                _allUsuarios = await _apiService.GetUsuariosAsync();
                UsuariosGrid.ItemsSource = _allUsuarios;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Filter(string query)
        {
            if (_allUsuarios == null) return;

            string searchText = query.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                UsuariosGrid.ItemsSource = _allUsuarios;
            }
            else
            {
                var filtered = _allUsuarios.Where(u => 
                    (u.Username != null && u.Username.ToLower().Contains(searchText)) ||
                    (u.Nombre != null && u.Nombre.ToLower().Contains(searchText)) ||
                    (u.Apellido != null && u.Apellido.ToLower().Contains(searchText)) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchText))
                ).ToList();
                UsuariosGrid.ItemsSource = filtered;
            }
        }

        public void GenerateReport()
        {
            // Report generation for users not implemented yet
            MessageBox.Show("Funcionalidad de informe de usuarios no implementada.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UserFormDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    bool success = await _apiService.CreateUsuarioAsync(dialog.Usuario);
                    if (success)
                    {
                        MessageBox.Show("Usuario creado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadUsuariosAsync();
                    }
                    else
                    {
                        MessageBox.Show("Error al crear el usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsuariosGrid.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione un usuario para editar.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectedUsuario = (Usuario)UsuariosGrid.SelectedItem;
            var dialog = new UserFormDialog(selectedUsuario);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    bool success = await _apiService.UpdateUsuarioAsync(dialog.Usuario);
                    if (success)
                    {
                        MessageBox.Show("Usuario actualizado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadUsuariosAsync();
                    }
                    else
                    {
                        MessageBox.Show("Error al actualizar el usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsuariosGrid.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione un usuario para eliminar.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectedUsuario = (Usuario)UsuariosGrid.SelectedItem;
            
            var result = MessageBox.Show(
                $"¿Está seguro de que desea eliminar el usuario '{selectedUsuario.Username}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = await _apiService.DeleteUsuarioAsync(selectedUsuario.Id);
                    if (success)
                    {
                        MessageBox.Show("Usuario eliminado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadUsuariosAsync();
                    }
                    else
                    {
                        MessageBox.Show("Error al eliminar el usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
