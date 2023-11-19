using System.Windows;

namespace VirtualParty
{
    public partial class ContentSetWindow : Fluent.RibbonWindow
    {
        public delegate void ConfirmHandler(string content, int index);
        public event ConfirmHandler confirmEvent;

        public ContentSetWindow()
        {
            InitializeComponent();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            confirmEvent(content.Text, templateBox.SelectedIndex);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
