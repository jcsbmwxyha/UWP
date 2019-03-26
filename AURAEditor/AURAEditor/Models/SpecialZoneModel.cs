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

        public string ImageSource { get; set; }
        public string ImageSourceSolid { get; set; }

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

        private SolidColorBrush _myColorSolid;
        public SolidColorBrush MyColorSolid
        {
            get
            {
                return _myColorSolid;
            }
            set
            {
                _myColorSolid = value;
                RaisePropertyChanged("MyColorSolid");
            }
        }

        public SpecialZoneModel() : base()
        {
        }

        override public void ChangeStatus(RegionStatus status)
        {
            if (_myStatus == status)
                return;

            _myStatus = status;

            switch (_myStatus)
            {
                case RegionStatus.Normal:
                    MyColor = new SolidColorBrush(Colors.White);
                    MyColorSolid = new SolidColorBrush(Colors.Transparent);
                    Selected = false;
                    break;
                case RegionStatus.NormalHover:
                    MyColor = new SolidColorBrush(Colors.White);
                    MyColorSolid = new SolidColorBrush(new Color { A = 100, R = 255, G = 0, B = 41 });
                    Selected = false;
                    break;
                case RegionStatus.Selected:
                    MyColor = new SolidColorBrush(new Color { A = 255, R = 255, G = 0, B = 41 });
                    MyColorSolid = new SolidColorBrush(Colors.Transparent);
                    Selected = true;
                    break;
                case RegionStatus.SelectedHover:
                    MyColor = new SolidColorBrush(new Color { A = 255, R = 255, G = 0, B = 41 });
                    MyColorSolid = new SolidColorBrush(new Color { A = 100, R = 255, G = 0, B = 41 });
                    Selected = true;
                    break;
                case RegionStatus.Watching:
                    MyColor = new SolidColorBrush(new Color { A = 255, R = 4, G = 61, B = 246 });
                    MyColorSolid = new SolidColorBrush(Colors.Transparent);
                    Selected = true;
                    break;
                default:
                    MyColor = new SolidColorBrush(Colors.Red);
                    MyColorSolid = new SolidColorBrush(Colors.Red);
                    Selected = true;
                    break;
            }
        }
        //override public void Preview(Color c)
        //{
        //    MyColorSolid = new SolidColorBrush(c);
        //    MyColor = new SolidColorBrush(c);
        //}
    }
}
