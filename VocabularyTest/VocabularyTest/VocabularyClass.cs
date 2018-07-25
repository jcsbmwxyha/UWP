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
        static public string CreateSimpleContent(List<Vocabulary> voclist)
        {
            string result = "";

            foreach (Vocabulary vd in voclist)
            {
                result += vd.English + "\r\n";

                if (vd.KK != "")
                    result += vd.KK + "\r\n";

                result += vd.Chinese + "\r\n\r\n";
            }

            return result;
        }
        static public string CreateFormatContent(List<Vocabulary> voclist)
        {
            string result = "";

            foreach (Vocabulary vd in voclist)
            {
                result +=
                    "<eg>" + vd.English + "<eg/>\r\n" +
                    "<kk>" + vd.KK + "<kk/>\r\n" +
                    "<ch>" + vd.Chinese + "<ch/>\r\n";

                if (vd.Star == true)
                    result += "<star>" + "t" + "<star/>\r\n";
                else
                    result += "<star>" + "f" + "<star/>\r\n";

                if (vd.Ear == true)
                    result += "<ear>" + "t" + "<ear/>\r\n";
                else
                    result += "<ear>" + "f" + "<ear/>\r\n";

                result += "<note>" + vd.Note + "<note/>\r\n";
            }

            return result;
        }
    }
}
