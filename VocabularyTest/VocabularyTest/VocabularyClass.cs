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
        public Vocabulary(string e, string c, string t)
        {
            English = e;
            Chinese = c;
            Translater = t;
            IsSelect = Visibility.Collapsed;
        }
        public string English { get; set; }
        public string Chinese { get; set; }
        public string Translater { get; set; }
        public Windows.UI.Xaml.Visibility IsSelect { get; set; }
    }
}
