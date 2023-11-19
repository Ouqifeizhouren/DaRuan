using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;

namespace VirtualParty
{
    public interface User
    {
        public void ComeIn(string name);
    }
    public class Admin : User
    {
        public delegate void MembersHandler(string userid);
        public event MembersHandler SomeBodyComeIn;
        public List<string> members { get; set; } = new List<string>();
        public string roomIn { get; set; } = "";
        public string roomIntroduction { get; set; }
        public DispatcherTimer AirTimer { get; set; } = new DispatcherTimer();
        public void ComeIn(string name)
        {
            SomeBodyComeIn(name);
        }
        public Admin()
        {
            AirTimer.Interval = new TimeSpan(0, 0, 0, 0, 500); roomIn = "";
        }
    }
    public class Guest : User
    {
        public SearchRoomPage searchRoomPage { get; set; } = new SearchRoomPage();
        public ConcurrentBag<string> foundedRooms { get; set; } = new ConcurrentBag<string>();
        public Dictionary<string, string> roomIntros { get; set; } = new Dictionary<string, string>();
        public string roomIn { get; set; }
        public delegate void MembersHandler(string userid);
        public event MembersHandler SomeBodyComeIn;
        public List<string> members { get; set; } = new List<string>();
        public void ComeIn(string name)
        {
            SomeBodyComeIn(name);
        }
    }
}
