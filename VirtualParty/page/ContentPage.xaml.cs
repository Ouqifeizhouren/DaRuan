using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VirtualParty
{
    public partial class ContentPage : Page
    {
        private bool isEditable = false;
        public delegate void ConfirmHandler(string pictureSource);
        public event ConfirmHandler pictureChange;
        public ContentPage()
        {
            InitializeComponent();
        }

        private void PictureBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "图片文件(*.jpg,*.png,*.jpeg)|*.jpg;*.png;*.jpeg";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                picture.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                pictureChange(openFileDialog.FileName);
            }
        }

        public void PictureGet(string pictureSource)
        {
            picture.Source = new BitmapImage(new Uri(pictureSource));
        }

        public void PageEdit()
        {
            if (!isEditable)
            {
                picture.Visibility = Visibility.Hidden;
                pictureBtn.Visibility = Visibility.Visible;
                isEditable = true;

                topic.BorderThickness = new Thickness(1);
                topic.Background = Brushes.White;
                topic.IsReadOnly = false;
                content.IsReadOnly = false;
            }
            else
            {
                picture.Visibility = Visibility.Visible;
                pictureBtn.Visibility = Visibility.Hidden;
                isEditable = false;

                topic.BorderThickness = new Thickness(0);
                topic.Background = Brushes.Transparent;
                topic.IsReadOnly = true;
                content.IsReadOnly = true;
            }
        }
    }
}
