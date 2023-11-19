using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Forms;
using TagLib;
using NAudio.Wave;
using System.Windows.Media;
using System.Windows.Media.Animation;
using EasyNetQ;
using CCMessages;
using System.Threading;
using System.Text;
using VirtualParty.control;

namespace VirtualParty
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private string ServiceIP = "127.0.0.1";
        private DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
        private RecordController record = new RecordController();
        private List<FileInfo> songs = new List<FileInfo>();
        private int position = 0;
        private string cacheDictionary = @"C:\PartyCache";
        private bool isDragging = false;
        private bool startRecord = false;
        private int barrageRow = 0;
        private Color color = (Color)ColorConverter.ConvertFromString("Black");
        private SoundPage soundPage = new SoundPage();
        private SynchronizationContext mainThreadSynContext;
        private bool isRoomed = false;
        private DispatcherTimer Searchtimer = new DispatcherTimer();
        private bool isAdmin = true;
        private Admin admin = new Admin();
        private Guest guest = new Guest();
        private IBus bus;
        private bool isPlaying = false;
        private List<Step> steps = new List<Step>();
        private string userID = "新用户" + new Random().Next(1001, 9999).ToString();
        private int stepIndex = 0;
        private int cacheNum = 0;
        private RectangleGeometry geometry = new RectangleGeometry();

        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += Tick;
            mainThreadSynContext = SynchronizationContext.Current;
            bus = RabbitHutch.CreateBus("host=" + ServiceIP + ";username=lc_test;password=lc_test");
            admin.AirTimer.Tick += Air;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mediaElement.LoadedBehavior = MediaState.Manual;
            recordElement.LoadedBehavior = MediaState.Manual;
            mediaElement.MediaOpened += MediaOpened;
            mediaElement.MediaEnded += MediaEnded;
            soundPanel.Content = new Frame() { Content = soundPage };
            geometry.Rect = new Rect(300, 0, 1200, 584.76);
            barragePanel.Clip = geometry;
            soundPage.SoundEvent += SendSound;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            bus.Dispose();
            mediaElement.Close();
        }

        private void MusicPlay(string exten)
        {
            if (songs.Count == 0 && isRoomed == false) return;
            mediaElement.Source = null;
            stopBtnImg.Source = new BitmapImage(new Uri("../img/pauseCircle.png", UriKind.Relative));
            stopBtn.Tag = "false";
            FileInfo toPlay = isAdmin ? songs[position] : new FileInfo(cacheDictionary + "\\cache" + cacheNum + exten);
            if (isAdmin)
            {
                toPlay.CopyTo(cacheDictionary + "\\cache" + cacheNum + toPlay.Extension, true);
                MusicPlayStart(cacheDictionary + "\\cache" + cacheNum + toPlay.Extension);
                if (!admin.roomIn.Equals("")) bus.PubSub.Publish(new RoomCommunicate(4, userID, (byte[])System.IO.File.ReadAllBytes(cacheDictionary + "\\cache" + cacheNum + toPlay.Extension), toPlay.Extension), admin.roomIn);
                cacheNum = (cacheNum + 1) % 3;
            }
            else MusicPlayStart(cacheDictionary + "\\cache" + cacheNum + toPlay.Extension);
        }

        private void MusicPlayStart(string fileInfo)
        {
            FileInfo song = new FileInfo(fileInfo);
            Uri uri = new Uri(song.FullName, UriKind.Relative);
            isPlaying = true;
            timer.Start();

            if (mediaElement.Source != null && mediaElement.Source == uri)
            {
                mediaElement.Position = TimeSpan.FromSeconds(0);
                mediaElement.Play();
                return;
            }
            mediaElement.Source = uri;
            if (isVideo(song))
            {
                musicName.Content = "视频播放";
                musicCoverImg.Source = new BitmapImage(new Uri("../img/yinyue.png", UriKind.Relative));
                mediaElement.Play();
                return;
            }
            using (var fileStream = song.Open(FileMode.Open))
            {
                var tagFile = TagLib.File.Create(new StreamFileAbstraction(song.Name, fileStream, fileStream));
                var tags = tagFile.GetTag(TagTypes.Id3v2);
                musicName.Content = tags.Title;
                if (tags.Pictures.Length > 0)
                {
                    var picture = tags.Pictures[0];
                    var memoryStream = new MemoryStream(picture.Data.Data);
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.StreamSource = memoryStream;
                    img.EndInit();
                    musicCoverImg.Source = img;
                }
            }
            mediaElement.Play();
        }

        private bool isVideo(FileInfo info)
        {
            if (info.Extension == ".avi" || info.Extension == ".mp4" || info.Extension == ".mkv") return true;
            else return false;
        }

        private void MediaOpened(object sender, RoutedEventArgs e)
        {
            TimeSpan time = mediaElement.NaturalDuration.TimeSpan;
            slider.Maximum = time.TotalSeconds;
            endTime.Content = time.ToString("mm':'ss");
        }

        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            if (isAdmin)
            {
                position = (position + 1) % songs.Count;
                MusicPlay("");
            }
        }

        private void RoomCreate_Click(object sender, RoutedEventArgs e)
        {
            editPanel.Visibility = Visibility.Hidden;
            RoomCreatePage roomCreatePage = new RoomCreatePage();
            roomCreatePage.confirmEvent += RoomCreate;
            roomCreatePage.cancelEvent += () => { displayPanel.Content = null; };
            displayPanel.Content = new Frame() { Content = roomCreatePage };
            editPanel.Visibility = Visibility.Hidden;

        }

        public void RoomCreate(string name, string introduction)
        {
            string RoomName = name;

            bus.PubSub.Subscribe<RoomCommunicate>(userID, a => RoomOperation(a), x => x.WithTopic(name).WithTopic("Request"));

            isAdmin = true;
            UserIgnore();
            admin.SomeBodyComeIn += ShowNewMemerInTreeview;
            admin.SomeBodyComeIn += AirAllMembers;
            admin.roomIn = name; admin.roomIntroduction = introduction;
            admin.members.Add(userID);
            admin.AirTimer.Start();
            ShowNewMemerInTreeview(userID);
            WelcomePage page = new WelcomePage();
            displayPanel.Content = new Frame() { Content = page };
        }

        public void DisplayPanelReset()
        {
            displayPanel.Content = null;
        }
        public void SendSound(int id)
        {
            bus.PubSub.Publish(new RoomCommunicate(7, userID, id), (isAdmin ? admin.roomIn : guest.roomIn));
        }

        public void RoomOperation(RoomCommunicate a)
        {
            new Thread(() =>
            {
                mainThreadSynContext.Post(new SendOrPostCallback(s =>
                {
                    if (a.myName != userID)
                    {
                        if (a.type == 0 && isAdmin)
                        {
                            admin.ComeIn(a.myName);
                        }
                        if (a.type == 1)
                        {
                            BarrageShow(a.barrage);
                        }
                        if (a.type == 2)
                        {
                            Spread_Room();
                        }
                        if (a.type == 3)
                        {
                            RefreshMembers(a.members);
                            if (mediaElement.Source != null)
                            {
                                if (mediaElement.Position.Subtract(a.MediaPosition).Duration() > new TimeSpan(0, 0, 0, 0, 100) && mediaElement.Visibility != Visibility.Visible) { mediaElement.Position = a.MediaPosition; }
                                if (mediaElement.Position.Subtract(a.MediaPosition).Duration() > new TimeSpan(0, 0, 0, 0, 500) && mediaElement.Visibility == Visibility.Visible) { mediaElement.Position = a.MediaPosition; }
                                if (a.playing == true && isPlaying == false) { timer.Start(); mediaElement.Play(); isPlaying = true; stopBtnImg.Source = new BitmapImage(new Uri("../img/pauseCircle.png", UriKind.Relative)); }
                                else if (a.playing == false && isPlaying == true) { timer.Stop(); mediaElement.Pause(); isPlaying = false; stopBtnImg.Source = new BitmapImage(new Uri("../img/playCircle.png", UriKind.Relative)); }
                            }
                            if (a.steps != null)
                            {
                                for (int i = 0; i < steps.Count; i++)
                                {
                                    Step temp = steps[i];
                                    if (temp.index == 0)
                                    {
                                        ContentPage contentPage = temp.page as ContentPage;
                                        if ((!contentPage.topic.Text.Equals(a.steps[i].topicName)) || (!contentPage.content.Text.Equals(a.steps[i].contentName)))
                                        {
                                            contentPage.topic.Text = a.steps[i].topicName;
                                            contentPage.content.Text = a.steps[i].contentName;
                                        }
                                    }
                                    else if (temp.index == 1)
                                    {
                                        DicePage dicePage = temp.page as DicePage;
                                        if ((!dicePage.topic.Text.Equals(a.steps[i].topicName)) || (!dicePage.content.Text.Equals(a.steps[i].contentName)))
                                        {
                                            dicePage.topic.Text = a.steps[i].topicName;
                                            dicePage.content.Text = a.steps[i].contentName;
                                        }
                                    }
                                }
                                int tempCount = steps.Count;
                                for (int i = tempCount; i < a.steps.Count; i++)
                                {
                                    StepInfo stepInfo = a.steps[i];
                                    switch (a.steps[i].index)
                                    {
                                        case 1:
                                            DicePage dicePage = new DicePage();
                                            dicePage.barrageEvent += BarrageSend;
                                            dicePage.topic.Text = stepInfo.topicName;
                                            dicePage.content.Text = stepInfo.contentName;
                                            steps.Add(new Step(dicePage, 1));
                                            break;
                                        case 2:
                                            VideoPage videoPage = new VideoPage();
                                            videoPage.chooseEvent += (FileInfo info) =>
                                            {
                                                steps[stepIndex].songs.Add(info);
                                                if (musicListPanel.Visibility == Visibility.Visible) MusicListShow();
                                            };
                                            videoPage.mediaShowEvent += () => { mediaPanel.Visibility = Visibility.Visible; };
                                            videoPage.mediaHideEvent += () => { mediaPanel.Visibility = Visibility.Hidden; };
                                            steps.Add(new Step(videoPage, 2));
                                            break;
                                        default:
                                            ContentPage contentPage = new ContentPage();
                                            steps.Add(new Step(contentPage, 0));
                                            contentPage.topic.Text = stepInfo.topicName;
                                            contentPage.content.Text = stepInfo.contentName;

                                            break;
                                    }
                                }
                                if (a.stepIndex != stepIndex || (tempCount == 0 && steps.Count == 1))
                                {
                                    stepIndex = a.stepIndex;
                                    displayPanel.Content = new Frame() { Content = steps[stepIndex].page };
                                    if (steps[stepIndex].index != 2) mediaPanel.Visibility = Visibility.Hidden;
                                    else mediaPanel.Visibility = Visibility.Visible;
                                    mediaElement.Stop();
                                    mediaElement.Source = null;
                                    mediaElement.Close();
                                }
                            }
                        }
                        if (a.type == 4)
                        {
                            mediaElement.Stop();
                            mediaElement.Source = null;
                            mediaElement.Close();
                            using (FileStream fsW = new FileStream(cacheDictionary + "\\cache" + cacheNum + a.exten, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                fsW.Write(a.music, 0, a.music.Length);
                            }
                            if (a.exten == ".mp3" || a.exten == ".flac" || a.exten == ".wav")
                            {
                                MusicPlay(a.exten);
                                cacheNum = (cacheNum + 1) % 3;
                            }
                            if (a.exten == ".jpg" || a.exten == ".png" || a.exten == ".jpeg")
                            {
                                ContentPage b = steps[a.stepIndex].page as ContentPage;
                                b.PictureGet(cacheDictionary + "\\cache" + cacheNum + a.exten);
                                cacheNum = (cacheNum + 1) % 3;
                            }
                            if (a.exten == ".avi" || a.exten == ".mp4" || a.exten == ".mkv")
                            {
                                mediaPanel.Visibility = Visibility.Visible;
                                MusicPlay(a.exten);
                                cacheNum = (cacheNum + 1) % 3;
                            }
                        }
                        if (a.type == 6)
                        {
                            using (FileStream fsW = new FileStream(cacheDictionary + "\\record.wav", FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                fsW.Write(a.music, 0, a.music.Length);
                            }
                            RecordPlay();
                        }
                        if (a.type == 7)
                        {
                            soundPage.soundElement.Source = new Uri("../../../sound/" + a.soundID +".wav", UriKind.Relative);
                            soundPage.soundElement.Play();
                        }
                    }
                }), null);
            }).Start();
        }

        public void ShowNewMemerInTreeview(string a)
        {
            admin.members.Add(a);
            guest.members.Add(a);
            new Thread(() =>
            {
                mainThreadSynContext.Post(new SendOrPostCallback(s =>
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = a;
                    item.Margin = new Thickness(0, 6, 0, 0);
                    MemberTreeView.Items.Add(item);
                }), null);
            }).Start();
        }

        public void RefreshMembers(List<string> members)
        {
            if (members == null) return;
            foreach (string member in members)
            {
                if (!guest.members.Contains(member))
                {
                    guest.members.Add(member);
                    ShowNewMemerInTreeview(member);
                }
            }

        }

        public void AirAllMembers(string a)
        {
            new Thread(() =>
            {
                mainThreadSynContext.Post(new SendOrPostCallback(s =>
                {
                    List<StepInfo> Temp = new List<StepInfo> { };
                    for (int i = 0; i < steps.Count; i++)
                    {
                        Step temp = steps[i];
                        if (temp.index == 0)
                        {
                            ContentPage contentPage = temp.page as ContentPage;
                            Temp.Add(new StepInfo(0, contentPage.topic.Text, contentPage.content.Text));
                        }
                        else if (temp.index == 1)
                        {
                            DicePage dicePage = temp.page as DicePage;
                            Temp.Add(new StepInfo(0, dicePage.topic.Text, dicePage.content.Text));
                        }
                        else
                        {
                            Temp.Add(new StepInfo(3));
                        }
                    }
                    bus.PubSub.Publish(new RoomCommunicate(3, userID, (List<string>)admin.members, mediaElement.Position, isPlaying, Temp, stepIndex), admin.roomIn);
                }), null);
            }).Start();
        }

        public void Air(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                mainThreadSynContext.Post(new SendOrPostCallback(s =>
                {
                    List<StepInfo> Temp = new List<StepInfo> { };
                    for (int i = 0; i < steps.Count; i++)
                    {
                        Step temp = steps[i];
                        if (temp.index == 0)
                        {
                            ContentPage contentPage = temp.page as ContentPage;
                            Temp.Add(new StepInfo(0, contentPage.topic.Text, contentPage.content.Text));
                        }
                        else if (temp.index == 1)
                        {
                            DicePage dicePage = temp.page as DicePage;
                            Temp.Add(new StepInfo(1, dicePage.topic.Text, dicePage.content.Text));
                        }
                        else
                        {
                            Temp.Add(new StepInfo(2));
                        }
                    }
                    bus.PubSub.Publish(new RoomCommunicate(3, userID, (List<string>)admin.members, mediaElement.Position, isPlaying, Temp, stepIndex), admin.roomIn);
                }), null);
            }).Start();
        }

        private void Spread_Room()
        {
            new Thread(() =>
            {
                mainThreadSynContext.Post(new SendOrPostCallback(s =>
                {
                    bus.PubSub.Publish(new MyResponse(admin.roomIn, admin.roomIntroduction), "RoomExists");
                }), null);
            }).Start();
        }

        private void RoomJoin_Click(object sender, RoutedEventArgs e)
        {
            editPanel.Visibility = Visibility.Hidden;
            SearchRoomPage searchPage = guest.searchRoomPage;
            displayPanel.Content = new Frame() { Content = searchPage };
            guest.foundedRooms.Clear();
            guest.roomIntros.Clear();
            Search_Room();
        }

        private void Search_Room()
        {
            bus.PubSub.Publish(new RoomCommunicate(2, userID, (Barrage)null), "Request");
            bus.PubSub.Subscribe<MyResponse>(userID, d => FoundRoom(d));
        }

        private void FoundRoom(MyResponse gotMessage)
        {
            string name = gotMessage.roomName, intro = gotMessage.roomIntroduction;
            guest.foundedRooms.Add(name); guest.roomIntros.Add(name, intro);
            new Thread(() =>
            {
                mainThreadSynContext.Post(new SendOrPostCallback(s =>
                {
                    SearchRoomPage searchPage = guest.searchRoomPage;
                    searchPage.roomJoinPanel.Visibility = Visibility.Hidden;
                    TreeViewItem item = new TreeViewItem();
                    Boolean exist = false;
                    foreach (TreeViewItem i in searchPage.roomList.Items)
                    {
                        if (i.Header.ToString().Equals(name)) { exist = true; break; }
                    }
                    if (!exist)
                    {
                        item.Header = name;
                        item.Tag = "详情：" + intro;
                        item.MouseDoubleClick += searchPage.SetIntroduce;
                        searchPage.confirmEvent += JoinRoom;
                        searchPage.roomList.Items.Add(item);
                        displayPanel.Content = new Frame() { Content = searchPage };
                    }
                }), null);
            }).Start();
        }

        private void JoinRoom(string roomName)
        {
            WelcomePage page = new WelcomePage();
            page.topic.Content = "加入房间成功，\r\n等待派对开始吧！";
            displayPanel.Content = new Frame() { Content = page };
            isRoomed = true; isAdmin = false;
            GuestIgnore();
            guest.roomIn = roomName;
            Searchtimer.Stop();
            string R = guest.roomIn;
            guest.SomeBodyComeIn += ShowNewMemerInTreeview;
            bus.PubSub.Publish(new RoomCommunicate(0, userID, (Barrage)null), R);
            bus.PubSub.Subscribe<RoomCommunicate>(userID, a => RoomOperation(a), x => x.WithTopic(R));
        }

        private void Comment_Click(object sender, RoutedEventArgs e)
        {
            barrageControlPanel.Visibility = barrageControlPanel.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (!startRecord)
            {
                recordElement.Stop();
                recordElement.Source = null;
                System.Windows.MessageBox.Show($"即将开始录音！", "消息提示");
                record.StartRecord(cacheDictionary + "\\record.wav");
                startRecord = true;
            }
            else
            {
                record.StopRecord();
                startRecord = false;
                bus.PubSub.Publish(new RoomCommunicate(6, userID, (byte[])System.IO.File.ReadAllBytes(cacheDictionary + "\\record.wav"), ".wav"), (isAdmin ? admin.roomIn : guest.roomIn));
                //RecordPlay();

            }
        }

        private void RecordPlay()
        {
            recordElement.Source = new Uri(cacheDictionary + "\\record.wav", UriKind.Relative);
            recordElement.Play();
        }

        private void MusicListBtn_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewPanel.Visibility == Visibility.Hidden)
            {
                treeViewPanel.Visibility = Visibility.Visible;
                settingPanel.Visibility = Visibility.Visible;
                hiddenBtn.Visibility = Visibility.Visible;
                visibleBtn.Visibility = Visibility.Hidden;

                rightPanel.SetValue(Grid.ColumnProperty, 1);
                rightPanel.SetValue(Grid.ColumnSpanProperty, 1);
                editPanel.SetValue(Grid.ColumnProperty, 1);
                editPanel.SetValue(Grid.ColumnSpanProperty, 1);
            }
            if (musicListPanel.Visibility == Visibility.Hidden)
            {
                musicListPanel.Visibility = Visibility.Visible;
                MusicListShow();
            }
            else
            {
                musicListPanel.Visibility = Visibility.Hidden;
            }
        }

        private void MusicListShow()
        {
            while (musicListView.Items.Count > 0) musicListView.Items.RemoveAt(0);
            songs = steps[stepIndex].songs;
            for (int i = 0; i < songs.Count; i++)
            {
                FileInfo song = songs[i];
                TreeViewItem item = new TreeViewItem();
                string name = song.Name.Substring(0, song.Name.LastIndexOf('.'));
                item.Header = name;
                item.Margin = new Thickness(0, 6, 0, 0);
                item.FontSize = 10;
                item.MouseDoubleClick += MusicListItem_MouseDoubleClick;
                item.Tag = i;
                musicListView.Items.Add(item);
            }
        }

        private void MusicListItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            position = Convert.ToInt32(item.Tag.ToString());
            MusicPlay("");
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mediaElement.Position = TimeSpan.FromSeconds(slider.Value);
            isDragging = false;
        }

        private void Slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var value = e.GetPosition(slider).X / slider.ActualWidth * slider.Maximum;
            mediaElement.Position = TimeSpan.FromSeconds(value);
        }

        private void Tick(object sender, EventArgs e)
        {
            nowTime.Content = mediaElement.Position.ToString("mm':'ss");
            if (!isDragging) slider.Value = mediaElement.Position.TotalSeconds;
        }

        private void MusicPlayBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            if (isRoomed == false)
            {
                if (btn.Name == "lastBtn")
                {
                    if (songs.Count == 0) return;
                    position = (position - 1 + songs.Count) % songs.Count;
                    MusicPlay("");
                }
                else if (btn.Name == "nextBtn")
                {
                    if (songs.Count == 0) return;
                    position = (position + 1) % songs.Count;
                    MusicPlay("");
                }
                else
                {
                    if (btn.Tag.ToString() == "true")
                    {
                        btn.Tag = "false";
                        stopBtnImg.Source = new BitmapImage(new Uri("../img/pauseCircle.png", UriKind.Relative));
                        timer.Start();
                        mediaElement.Play();
                        isPlaying = true;
                    }
                    else
                    {
                        btn.Tag = "true";
                        stopBtnImg.Source = new BitmapImage(new Uri("../img/playCircle.png", UriKind.Relative));
                        timer.Stop();
                        mediaElement.Pause();
                        isPlaying = false;
                    }
                }
            }
        }

        private void MusicPlayReset()
        {
            timer.Stop();
            mediaElement.Stop();
            mediaElement.Source = null;
            stopBtnImg.Source = new BitmapImage(new Uri("../img/playCircle.png", UriKind.Relative));
            endTime.Content = "00:00";
            musicCoverImg.Source = new BitmapImage(new Uri("../img/yinyue.png", UriKind.Relative));
            musicName.Content = "MusicTitle";
        }

        private void UserIgnore()
        {
            roomCreateBtn.IsEnabled = false;
            roomJoinBtn.IsEnabled = false;
            settingBtn.IsEnabled = false;
        }

        private void GuestIgnore()
        {
            UserIgnore();
            stepAddBtn.Visibility = Visibility.Hidden;
            stepRemoveBtn.Visibility = Visibility.Hidden;
        }

        private void BarrageSend_Click(object sender, RoutedEventArgs e)
        {
            BarrageSend(barrageContent.Text);
        }

        private void BarrageSend(string str)
        {
            Barrage barrage = new Barrage(color, userID + "：" + str);
            BarrageShow(barrage);
            string RoomName = isAdmin ? admin.roomIn : guest.roomIn;
            bus.PubSub.Publish(new RoomCommunicate(1, userID, (Barrage)barrage), RoomName);
        }

        private void BarrageShow(Barrage bar)
        {
            new Thread(() =>
            {
                mainThreadSynContext.Post(new SendOrPostCallback(s =>
                {
                    System.Windows.Controls.Label barrage = new System.Windows.Controls.Label();
                    barrage.Content = bar.content;
                    barrage.Foreground = new SolidColorBrush(bar.color);
                    barrage.IsHitTestVisible = false;
                    barrage.FontSize = 30;
                    barrage.SetValue(Grid.RowProperty, barrageRow);
                    barrageRow = (barrageRow + 1) % 5;
                    barragePanel.Children.Add(barrage);
                    Storyboard board = new Storyboard();
                    barrage.RenderTransform = new TranslateTransform(0, 0);
                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 1200;
                    animation.To = -1000;
                    animation.Duration = TimeSpan.FromSeconds(5);
                    animation.Completed += (object sender, EventArgs e) => { barragePanel.Children.Remove(barrage); };
                    Storyboard.SetTarget(animation, barrage);
                    Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                    board.Children.Add(animation);
                    board.Begin();
                }), null);
            }).Start();
        }

        private void ColorChange_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            color = ((SolidColorBrush)btn.Background).Color;
        }

        private void SendPicture(string route)
        {
            bus.PubSub.Publish(new RoomCommunicate(4, userID, (byte[])System.IO.File.ReadAllBytes(route), new FileInfo(route).Extension), admin.roomIn);
        }

        private void PartClick(object sender, MouseButtonEventArgs e)
        {
            if (isRoomed == true && isAdmin == false) return;
            editPanel.Visibility = Visibility.Visible;
            int i = 0;
            for (; i < stepTreeView.Items.Count; i++)
            {
                TreeViewItem item = stepTreeView.Items[i] as TreeViewItem;
                if (item.IsSelected == true) break;
            }
            stepIndex = i;
            displayPanel.Content = new Frame() { Content = steps[stepIndex].page };
            if (steps[stepIndex].index != 2) mediaPanel.Visibility = Visibility.Hidden;
            else steps[stepIndex].mediaLastState();
        }

        private void StepAddBtn_Click(object sender, MouseButtonEventArgs e)
        {
            ContentSetWindow contentSetWindow = new ContentSetWindow();
            contentSetWindow.confirmEvent += StepAdd;
            WindowHelper.Show(contentSetWindow);
        }

        private void StepAdd(string content, int index)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = 0;
            stack.Margin = new Thickness(0, 6, 0, 0);
            System.Windows.Controls.Label label = new System.Windows.Controls.Label();
            label.Content = content;
            stack.Children.Add(label);
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = stack;
            treeViewItem.MouseDoubleClick += PartClick;
            stepTreeView.Items.Add(treeViewItem);
            switch (index)
            {
                case 1:
                    DicePage dicePage = new DicePage();
                    dicePage.barrageEvent += BarrageSend;
                    steps.Add(new Step(dicePage, 1));
                    break;
                case 2:
                    VideoPage videoPage = new VideoPage();
                    videoPage.chooseEvent += (FileInfo info) =>
                    {
                        steps[stepIndex].songs.Add(info);
                        if (musicListPanel.Visibility == Visibility.Visible) MusicListShow();
                    };
                    videoPage.mediaShowEvent += () => { mediaPanel.Visibility = Visibility.Visible; };
                    videoPage.mediaHideEvent += () => { mediaPanel.Visibility = Visibility.Hidden; };
                    steps.Add(new Step(videoPage, 2));
                    break;
                default:
                    ContentPage i = new ContentPage();
                    i.pictureChange += SendPicture;
                    steps.Add(new Step(i, 0));

                    break;
            }
        }

        private void StepRemoveBtn_Click(object sender, MouseButtonEventArgs e)
        {
            int position = 0;
            for (int i = 0; i < stepTreeView.Items.Count; i++)
            {
                TreeViewItem item = stepTreeView.Items[i] as TreeViewItem;
                if (item.IsSelected == true)
                {
                    position = i;
                    break;
                }
            }
            stepTreeView.Items.RemoveAt(position);
            steps.RemoveAt(position);
            if (steps.Count == 0)
            {
                editPanel.Visibility = Visibility.Hidden;
                DisplayPanelReset();
                return;
            }
            if (position == stepIndex)
            {
                stepIndex = 0;
                displayPanel.Content = new Frame() { Content = steps[stepIndex].page };
                if (steps[stepIndex].index != 2) mediaPanel.Visibility = Visibility.Hidden;
                else steps[stepIndex].mediaLastState();
            }
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWindow = new SettingWindow();
            settingWindow.confirmEvent += SettingChange;
            settingWindow.userName.Text = userID;
            settingWindow.route.Text = cacheDictionary;
            WindowHelper.Show(settingWindow);
        }

        private void SettingChange(string name, string route)
        {
            userID = name;
            try
            {
                if (!Directory.Exists(route))
                {
                    Directory.CreateDirectory(route);
                }
                cacheDictionary = route;
            }
            catch
            {
                System.Windows.MessageBox.Show($"缓存地址无法创建！", "消息提示");
            }
        }

        private void VisibleBtn_Click(object sender, RoutedEventArgs e)
        {
            treeViewPanel.Visibility = Visibility.Visible;
            settingPanel.Visibility = Visibility.Visible;
            hiddenBtn.Visibility = Visibility.Visible;
            visibleBtn.Visibility = Visibility.Hidden;

            rightPanel.SetValue(Grid.ColumnProperty, 1);
            rightPanel.SetValue(Grid.ColumnSpanProperty, 1);
            editPanel.SetValue(Grid.ColumnProperty, 1);
            editPanel.SetValue(Grid.ColumnSpanProperty, 1);

            geometry.Rect = new Rect(300, 0, 1200, 584.76);
            barragePanel.Clip = geometry;
        }

        private void HiddenBtn_Click(object sender, RoutedEventArgs e)
        {
            treeViewPanel.Visibility = Visibility.Hidden;
            settingPanel.Visibility = Visibility.Hidden;
            musicListPanel.Visibility = Visibility.Hidden;
            hiddenBtn.Visibility = Visibility.Hidden;
            visibleBtn.Visibility = Visibility.Visible;

            rightPanel.SetValue(Grid.ColumnProperty, 0);
            rightPanel.SetValue(Grid.ColumnSpanProperty, 2);
            editPanel.SetValue(Grid.ColumnProperty, 0);
            editPanel.SetValue(Grid.ColumnSpanProperty, 2);

            geometry.Rect = new Rect(0, 0, 1500, 584.76);
            barragePanel.Clip = geometry;
        }

        private void Sound_Click(object sender, RoutedEventArgs e)
        {
            if (soundPanel.Visibility == Visibility.Hidden)
            {
                soundPanel.Visibility = Visibility.Visible;
            }
            else
            {
                soundPanel.Visibility = Visibility.Hidden;
            }
        }

        private void MusicAddBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "音乐文件(*.mp3,*.flac,*.wav)|*.mp3;*.flac;*.wav";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string item in openFileDialog.FileNames)
                {
                    FileInfo info = new FileInfo(item);
                    steps[stepIndex].songs.Add(info);
                }
                if (musicListPanel.Visibility == Visibility.Visible) MusicListShow();
            }
        }

        private void MusicRemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            MusicRemoveWindow musicRemoveWindow = new MusicRemoveWindow();
            foreach (FileInfo song in steps[stepIndex].songs)
            {
                musicRemoveWindow.content.Items.Add(song.Name);
            }
            musicRemoveWindow.confirmEvent += (int position) =>
            {
                steps[stepIndex].songs.RemoveAt(position);
                musicRemoveWindow.content.Items.RemoveAt(position);
                if (musicListPanel.Visibility == Visibility.Visible) MusicListShow();
            };
            WindowHelper.Show(musicRemoveWindow);
        }

        private void listChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (steps[stepIndex].index == 2) steps[stepIndex].mediaStateChange();
            position = 0;
            songs = steps[stepIndex].songs;
            MusicPlay("");
        }

        private void PageEditBtn_Click(object sender, RoutedEventArgs e)
        {
            steps[stepIndex].PageEdit();
        }
    }
}
