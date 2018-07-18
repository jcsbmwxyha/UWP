using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VocabularyTest
{
    public class Vocabulary : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Vocabulary(string e, string kk, string c, string star, string ear, string note)
        {
            English = e;
            KK = kk;
            Chinese = c;

            if (star == "t")
                Star = true;
            else
                Star = false;
            
            if (ear == "t")
                Ear = true;
            else
                Ear = false;

            Note = note;
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

                var frame = (Frame)Window.Current.Content;
                var page = (MainPage)frame.Content;
                page.SaveBtnEnabled = true;
            }
        }

        bool ear;
        public bool Ear
        {
            get
            {
                return ear;
            }
            set
            {
                ear = value;
                NotifyPropertyChanged("Ear");

                var frame = (Frame)Window.Current.Content;
                var page = (MainPage)frame.Content;
                page.SaveBtnEnabled = true;
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
