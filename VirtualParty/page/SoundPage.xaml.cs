using System;
using System.Windows;
using System.Windows.Controls;

namespace VirtualParty
{
    public partial class SoundPage : Page
    {
        public delegate void SoundHandler(int id);
        public event SoundHandler SoundEvent;
        public SoundPage()
        {
            InitializeComponent();
        }
        private void send(int id) { SoundEvent(id); }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            soundElement.LoadedBehavior = MediaState.Manual;
        }


        private void SoundBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "qinqin":
                    soundElement.Source = new Uri("../../../sound/0.wav", UriKind.Relative);
                    soundElement.Play();
                    send(0);
                    break;
                case "shangxin":
                    soundElement.Source = new Uri("../../../sound/1.wav", UriKind.Relative);
                    soundElement.Play();
                    send(1);
                    break;
                case "numa":
                    soundElement.Source = new Uri("../../../sound/2.wav", UriKind.Relative);
                    soundElement.Play();
                    send(2);
                    break;
                case "maimeng":
                    soundElement.Source = new Uri("../../../sound/3.wav", UriKind.Relative);
                    soundElement.Play();
                    send(3);
                    break;
                case "chijing":
                    soundElement.Source = new Uri("../../../sound/4.wav", UriKind.Relative);
                    soundElement.Play();
                    send(4);
                    break;
                default:
                    soundElement.Source = new Uri("../../../sound/5.wav", UriKind.Relative);
                    soundElement.Play();
                    send(5);
                    break;
            }
        }
    }
}
