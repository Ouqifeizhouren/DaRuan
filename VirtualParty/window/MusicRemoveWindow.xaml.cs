using System.Windows;

namespace VirtualParty
{
    public partial class MusicRemoveWindow : Fluent.RibbonWindow
    {
        public delegate void ConfirmHandler(int position);
        public event ConfirmHandler confirmEvent;

        public MusicRemoveWindow()
        {
            InitializeComponent();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            confirmEvent(content.SelectedIndex == -1 ? 0 : content.SelectedIndex);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
