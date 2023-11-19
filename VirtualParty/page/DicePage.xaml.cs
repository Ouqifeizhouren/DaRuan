using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace VirtualParty
{
    public partial class DicePage : Page
    {

        private List<string> diceImgPath = new List<string> { "../img/touzi1.png", "../img/touzi2.png", "../img/touzi3.png", "../img/touzi4.png", "../img/touzi5.png", "../img/touzi6.png" };
        private int diceImgNum = 0;
        private int interval = 0;
        private DispatcherTimer time;
        private Random random = null;
        private bool isEditable = false;
        private bool isClickable = true;
        public List<string> questions = new List<string> { "       请您先点击左上角图标编辑问题的内容！", "", "", "", "", "", "", ""};
        private int index = -1;
        public delegate void BarrageHandler(string str);
        public event BarrageHandler barrageEvent;

        public DicePage()
        {
            InitializeComponent();
        }

        private void ShookBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isClickable) return;
            isClickable = false;
            random = new Random();
            diceImgNum = random.Next(0, 5);
            interval = 80;
            time = new DispatcherTimer(DispatcherPriority.Render);
            time.Interval = TimeSpan.FromMilliseconds(interval);
            time.Tick += Tick;
            time.Start();
        }

        private void Tick(object sender, EventArgs e)
        {
            if (time.IsEnabled == true)
            {
                diceImgNum = (diceImgNum + 1) % 6;
                diceImg.Source = new BitmapImage(new Uri(diceImgPath[diceImgNum], UriKind.Relative));
                interval++;
            }
            if (interval == 100)
            {
                time.Stop();
                isClickable = true;
                barrageEvent("丢出了一个" + (diceImgNum + 1).ToString() + "！" + ((diceImgNum == 2 || diceImgNum == 3) ? "马马虎虎~" : (diceImgNum < 2 ? "准备接受惩罚吧！" : "大你~")));
            }
        }

        private void QuestionBtn_Click(object sender, RoutedEventArgs e)
        {
            QuestionChange();
        }

        private void QuestionChange()
        {
            index = (index + 1) % 8;
            for (int i = 0; i < questions.Count; i++)
            {
                if (questions[index] != "") break;
                index = (index + 1) % 8;
            }
            content.Text = questions[index];
        }

        public void PageEdit()
        {
            if (!isEditable)
            {
                questionBtn.Visibility = Visibility.Hidden;
                content.Visibility = Visibility.Hidden;
                isEditable = true;

                topic.BorderThickness = new Thickness(1);
                topic.Background = Brushes.White;
                topic.IsReadOnly = false;
                questionPanel.Visibility = Visibility.Visible;
            }
            else
            {
                questionBtn.Visibility = Visibility.Visible;
                content.Visibility = Visibility.Visible;
                isEditable = false;

                topic.BorderThickness = new Thickness(0);
                topic.Background = Brushes.Transparent;
                topic.IsReadOnly = true;
                questionPanel.Visibility = Visibility.Hidden;
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < questions.Count; i++) questions[i] = "";
            questions[0] = text1.Text;
            questions[1] = text2.Text;
            questions[2] = text3.Text;
            questions[3] = text4.Text;
            questions[4] = text5.Text;
            questions[5] = text6.Text;
            questions[6] = text7.Text;
            questions[7] = text8.Text;
            PageEdit();
            index = -1;
            QuestionChange();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            PageEdit();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            QuestionChange();
        }
    }
}
