using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace VirtualParty
{
    public class StepInfo
    {
        public int index { get; set; }
        public string topicName { get; set; }
        public string contentName { get; set; }
        public List<string> questions { get; set; }
        public StepInfo(int index, string topicName, string contentName) { this.index = index; this.topicName = topicName; this.contentName = contentName; }
        public StepInfo(int index, string topicName, List<string> questions) { this.index = index; this.topicName = topicName; this.questions = questions; }
        public StepInfo(int index) { this.index = index; }
        public StepInfo() { }
    }
    public class Step
    {
        public List<FileInfo> songs { get; set; } = new List<FileInfo>();
        public Page page { get; set; }
        public int index { get; set; }

        public Step(Page page, int index)
        {
            this.page = page;
            this.index = index;
        }

        public void PageEdit()
        {
            switch (index)
            {
                case 1:
                    DicePage dicePage = this.page as DicePage;
                    dicePage.PageEdit();
                    break;
                case 2:
                    VideoPage videoPage = this.page as VideoPage;
                    videoPage.PageEdit();
                    break;
                default:
                    ContentPage contentPage = this.page as ContentPage;
                    contentPage.PageEdit();
                    break;
            }
        }

        public void mediaLastState()
        {
            VideoPage videoPage = this.page as VideoPage;
            videoPage.mediaLastState();
        }

        public void mediaStateChange()
        {
            VideoPage videoPage = this.page as VideoPage;
            videoPage.isEditable = false;
            videoPage.mediaLastState();
        }
    }
}
