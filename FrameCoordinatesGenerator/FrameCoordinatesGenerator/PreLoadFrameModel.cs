using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameCoordinatesGenerator
{
    class PreLoadFrameModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _ledindex;
        public string LedIndex
        {
            get
            {
                return _ledindex;
            }
            set
            {
                _ledindex = value;
                RaisePropertyChanged("LedIndex");
            }
        }
        public int IntIndex
        {
            get
            {
                string s = _ledindex.ToLower().Replace("led", "").Replace(" ", "");
                return Int32.Parse(s);
            }
        }
    
        public bool Exist;

        private double _left;
        public double Left
        {
            get
            {
                return _left;
            }
            set
            {
                _left = value;
                RaisePropertyChanged("Left");
            }
        }

        private double _right;
        public double Right
        {
            get
            {
                return _right;
            }
            set
            {
                _right = value;
                RaisePropertyChanged("Right");
            }
        }

        private double _top;
        public double Top
        {
            get
            {
                return _top;
            }
            set
            {
                _top = value;
                RaisePropertyChanged("Top");
            }
        }

        private double _bottom;
        public double Bottom
        {
            get
            {
                return _bottom;
            }
            set
            {
                _bottom = value;
                RaisePropertyChanged("Bottom");
            }
        }

        public string PNG;
        public int Z_Index;

        public double Width { get { return Right - Left; } }
        public double Height { get { return Bottom - Top; } }

        private bool _editing;
        public bool Editing
        {
            get
            {
                return _editing;
            }
            set
            {
                _editing = value;
                RaisePropertyChanged("Editing");
            }
        }

        private bool _conflict;
        public bool Conflict
        {
            get
            {
                return _conflict;
            }
            set
            {
                _conflict = value;
                RaisePropertyChanged("Conflict");
            }
        }
    }
}
