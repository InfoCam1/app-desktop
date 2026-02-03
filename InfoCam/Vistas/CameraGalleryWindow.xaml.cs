using System.Collections.Generic;
using System.Windows;
using InfoCam.Models;

namespace InfoCam.Views
{
    public partial class CameraGalleryWindow : Window
    {
        public CameraGalleryWindow(List<Camera> cameras)
        {
            InitializeComponent();
            CamerasItemsControl.ItemsSource = cameras;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
