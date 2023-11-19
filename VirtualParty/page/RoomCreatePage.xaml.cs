using System.Windows;
using System.Windows.Controls;

namespace VirtualParty
{
    public partial class RoomCreatePage : Page
    {
        public delegate void ConfirmHandler(string name, string introduction);
        public event ConfirmHandler confirmEvent;
        public delegate void CancelHandler();
        public event CancelHandler cancelEvent;

        public RoomCreatePage()
        {
            InitializeComponent();
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            confirmEvent(nameText.Text, introductionText.Text);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            cancelEvent();
        }
    }
}
