using AuraEditor.Common;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace AuraEditor.Models
{
    public class SpecialZoneModel : ZoneModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _imgUri;
        public string ImageSource
        {
            get
            {
                return _imgUri;
            }
            set
            {
                _imgUri = value;
                RaisePropertyChanged("ImageSource");
            }
        }

        private SolidColorBrush _myColor;
        public SolidColorBrush MyColor
        {
            get
            {
                return _myColor;
            }
            set
            {
                _myColor = value;
                RaisePropertyChanged("MyColor");
            }
        }

        public SpecialZoneModel() : base()
        {
        }

        override public void ChangeStatus(RegionStatus status)
        {
            Color outer;
            //Color inner;

            if (_myStatus == status)
                return;

            _myStatus = status;

            switch (_myStatus)
            {
                case RegionStatus.Normal:
                    outer = Colors.White;
                    //inner = new Color { A = 2, R = 255, G = 255, B = 255 };
                    Selected = false;
                    break;
                case RegionStatus.NormalHover:
                    outer = Colors.White;
                    //inner = new Color { A = 99, R = 255, G = 0, B = 41 };
                    Selected = false;
                    break;
                case RegionStatus.Selected:
                    outer = new Color { A = 255, R = 255, G = 0, B = 41 };
                    //inner = new Color { A = 2, R = 255, G = 255, B = 255 };
                    Selected = true;
                    break;
                case RegionStatus.SelectedHover:
                    outer = new Color { A = 255, R = 255, G = 0, B = 41 };
                    ////inner = new Color { A = 99, R = 255, G = 0, B = 41 };
                    Selected = true;
                    break;
                case RegionStatus.Watching:
                    outer = new Color { A = 255, R = 4, G = 61, B = 246 };
                    //inner = new Color { A = 2, R = 255, G = 255, B = 255 };
                    Selected = true;
                    break;
                default:
                    outer = Colors.Red;
                    //inner = Colors.Red;
                    Selected = true;
                    break;
            }

            MyColor = new SolidColorBrush(outer);
        }
    }
}
