using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualParty;

namespace CCMessages
{
    public class MyResponse
    {
        public string roomName { get; set; }
        public string roomIntroduction { get; set; }
        public MyResponse(string a, string b) { roomName = a;roomIntroduction = b; }
    }
    public class MyRequest
    {
        public string myName { get; set; }
        public MyRequest(string a) { myName = a; }
    }
    public class RoomCommunicate
    {
        public int type { get; set; } = 0;                                //0=进房间通知，1=弹幕，2=查找房间，3=广播成员名单+音乐进度+页信息，4=音乐文件、图片、视频，5=需要同步，6=语音文件
        public string myName { get; set; }
        public Barrage barrage { get; set; } = null;
        public TimeSpan MediaPosition { get; set; } = new TimeSpan();
        public bool playing { get; set; }
        public string exten { get; set; }
        public int stepIndex { get; set; }
        public int soundID { get; set; }
        public List<string> members { get; set; } = new List<string> { };
        public List<StepInfo> steps { get; set; } = new List<StepInfo> { };
        public byte[] music { get; set; }
        public RoomCommunicate(int t, string m, Barrage b) { type = t; myName = m; barrage = b; }
        public RoomCommunicate(int t, string m, int id) { type = t; soundID = id; myName = m; }
        public RoomCommunicate(int t, string m, List<string> a, TimeSpan ts, bool playing) { type = t; myName = m; members = a; MediaPosition = ts; this.playing = playing;}
        public RoomCommunicate(int t, string m, List<string> a, TimeSpan ts, bool playing, List<StepInfo> b, int stepIndex) { type = t; myName = m; members = a; MediaPosition = ts; this.playing = playing; steps = b; this.stepIndex = stepIndex; }
        public RoomCommunicate(int t, string m, byte[] mu, string ex) { type = t; myName = m; music = mu; exten = ex; }
        public RoomCommunicate(int t, string m) { type = t; myName = m;}
        public RoomCommunicate() { type = -1; myName = ""; }
    }
}
