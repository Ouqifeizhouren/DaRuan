using System.Windows;

namespace VirtualParty
{
    public partial class SettingWindow : Fluent.RibbonWindow
    {
        public delegate void ConfirmHandler(string name, string route);
        public event ConfirmHandler confirmEvent;

        public SettingWindow()
        {
            InitializeComponent();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            confirmEvent(userName.Text, route.Text);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
