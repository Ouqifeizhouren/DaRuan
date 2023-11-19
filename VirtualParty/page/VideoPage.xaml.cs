using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace VirtualParty
{
    public partial class VideoPage : Page
    {
        public bool isEditable { get; set; } = true;
        public delegate void ChooseHandler(FileInfo video);
        public event ChooseHandler chooseEvent;
        public delegate void MediaHideHandler();
        public event MediaHideHandler mediaHideEvent;
        public delegate void MediaShowHandler();
        public event MediaShowHandler mediaShowEvent;

        public VideoPage()
        {
            InitializeComponent();
        }

        private void VideoBtn_Click(object sender, RoutedEventArgs e)
        {
            FileInfo video = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "视频文件(*.avi,*.mp4,*.mkv)|*.avi;*.mp4;*.mkv";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                video = new FileInfo(openFileDialog.FileName);
                PageEdit();
                chooseEvent(video);
            }
        }

        public void VedioGet(FileInfo video) { chooseEvent(video); }

        public void PageEdit()
        {
            if (!isEditable)
            {
                isEditable = true;
                mediaHideEvent();
            }
            else
            {
                isEditable = false;
                mediaShowEvent();
            }
        }

        public void mediaLastState()
        {
            if (isEditable == false) mediaShowEvent();
        }
    }
}
