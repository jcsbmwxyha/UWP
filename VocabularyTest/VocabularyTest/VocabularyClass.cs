using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace VocabularyTest
{
    public class Vocabulary
    {
        public Vocabulary(string e, string kk, string c, string note)
        {
            English = e;
            KK = kk;
            Chinese = c;
            Note = note;
            Star = false;
        }
        public string English { get; set; }
        public string KK { get; set; }
        public string Chinese { get; set; }
        public string Note { get; set; }
        public bool Star { get; set; }
    }
}
