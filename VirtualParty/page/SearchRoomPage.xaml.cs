using System.Windows;
using System.Windows.Controls;

namespace VirtualParty
{
    public partial class SearchRoomPage : Page
    {
        public delegate void ConfirmHandler(string roomName);
        public event ConfirmHandler confirmEvent;
        private string roomName = "";

        public SearchRoomPage()
        {
            InitializeComponent();
        }

        private void RoomJoinBtn_Click(object sender, RoutedEventArgs e)
        {
            confirmEvent(roomName);
        }

        public void SetIntroduce(object sender, RoutedEventArgs e)
        {
            TreeViewItem room = sender as TreeViewItem;
            roomName = room.Header.ToString();
            introText.Text = room.Tag.ToString();
            roomJoinPanel.Visibility = Visibility.Visible;
        }
    }
}
