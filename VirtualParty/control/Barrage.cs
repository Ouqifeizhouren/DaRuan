using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace VirtualParty
{
    public class Barrage
    {
        public Color color { get; set; }
        public string content { get; set; }
        public Barrage(Color a, string b) { color = a; content = b; }
    }
}
