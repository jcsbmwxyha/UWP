using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace VocabularyTest
{
    public class Vocabulary : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        bool star;
        public bool Star {
            get
            {
                return star;
            }
            set
            {
                star = value;
                NotifyPropertyChanged("Star");
            }
        }
        
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
