using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using AuraEditor.Common;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
using AuraEditor.Pages;

namespace AuraEditor.Models
{
    public class ZoneModel : INotifyPropertyChanged
    {
        #region -- Property --
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private double _pixelleft;
        public double PixelLeft
        {
            get
            {
                return _pixelleft;
            }
            set
            {
                _pixelleft = value;
                RaisePropertyChanged("PixelLeft");
            }
        }

        private double _pixeltop;
        public double PixelTop
        {
            get
            {
                return _pixeltop;
            }
            set
            {
                _pixeltop = value;
                RaisePropertyChanged("PixelTop");
            }
        }

        private double _pixelwidth;
        public double PixelWidth
        {
            get
            {
                return _pixelwidth;
            }
            set
            {
                _pixelwidth = value;
                RaisePropertyChanged("PixelWidth");
            }
        }

        private double _pixelheight;
        public double PixelHeight
        {
            get
            {
                return _pixelheight;
            }
            set
            {
                _pixelheight = value;
                RaisePropertyChanged("PixelHeight");
            }
        }

        private string _zoneState;
        public string ZoneState
        {
            get
            {
                return _zoneState;
            }
            set
            {
                _zoneState = value;
                RaisePropertyChanged("ZoneState");
            }
        }

        private SolidColorBrush _stroke;
        public SolidColorBrush Stroke
        {
            get
            {
                return _stroke;
            }
            set
            {
                _stroke = value;
                RaisePropertyChanged("Stroke");
            }
        }

        private SolidColorBrush _fill;
        public SolidColorBrush Fill
        {
            get
            {
                return _fill;
            }
            set
            {
                _fill = value;
                RaisePropertyChanged("Fill");
            }
        }

        public int Index { get; internal set; }
        public bool Hover;
        public bool Selected;
        public int Zindex;
        protected RegionStatus _myStatus;
        #endregion

        public ZoneModel()
        {
            ZoneState = "Normal";
            Stroke = new SolidColorBrush(Colors.White);
            Fill = new SolidColorBrush(Colors.Transparent);
        }
        public Rect GetRect()
        {
            return new Rect(
                new Point(PixelLeft, PixelTop),
                new Point(PixelLeft + PixelWidth, PixelTop + PixelHeight)
            );
        }

        public void OnReceiveMouseEvent(MouseEvent mouseEvent)
        {
            bool shift = SpacePage.Self.PressShift;
            bool ctrl = SpacePage.Self.PressCtrl;

            if (mouseEvent == MouseEvent.Click)
            {
                if (ctrl)
                {
                    if (Selected == true) ChangeStatus(RegionStatus.NormalHover);
                    else ChangeStatus(RegionStatus.SelectedHover);
                }
                else
                {
                    ChangeStatus(RegionStatus.SelectedHover);
                }
            }
            else if (mouseEvent == MouseEvent.InRegion)
            {
                if (ctrl)
                {
                    if (Selected == true) ChangeStatus(RegionStatus.Normal);
                    else ChangeStatus(RegionStatus.Selected);
                }
                else
                {
                    ChangeStatus(RegionStatus.Selected);
                }
            }
            else if (mouseEvent == MouseEvent.OutRegion)
            {
                if (ctrl)
                {
                    if (Selected == true) ChangeStatus(RegionStatus.Normal);
                    else ChangeStatus(RegionStatus.Selected);
                }
                else
                {
                    ChangeStatus(RegionStatus.Normal);
                }
            }
            else if (mouseEvent == MouseEvent.Hover)
            {
                if (Selected == true)
                {
                    ChangeStatus(RegionStatus.SelectedHover);
                }
                else
                {
                    ChangeStatus(RegionStatus.NormalHover);
                }
            }
            else // Unhover
            {
                if (Selected == true)
                {
                    ChangeStatus(RegionStatus.Selected);
                }
                else
                {
                    ChangeStatus(RegionStatus.Normal);
                }
            }
        }
        virtual public void ChangeStatus(RegionStatus status)
        {
            if (_myStatus == status)
                return;

            _myStatus = status;

            switch (_myStatus)
            {
                case RegionStatus.Normal:
                    Stroke = new SolidColorBrush(Colors.White);
                    Fill = new SolidColorBrush(Colors.Transparent);
                    Selected = false;
                    Hover = false;
                    break;
                case RegionStatus.NormalHover:
                    Stroke = new SolidColorBrush(Colors.White);
                    Fill = new SolidColorBrush(new Color { A = 100, R = 255, G = 0, B = 41 });
                    Selected = false;
                    Hover = true;
                    break;
                case RegionStatus.Selected:
                    Stroke = new SolidColorBrush(new Color { A = 255, R = 255, G = 0, B = 41 });
                    Fill = new SolidColorBrush(Colors.Transparent);
                    Selected = true;
                    Hover = false;
                    break;
                case RegionStatus.SelectedHover:
                    Stroke = new SolidColorBrush(new Color { A = 255, R = 255, G = 0, B = 41 });
                    Fill = new SolidColorBrush(new Color { A = 100, R = 255, G = 0, B = 41 });
                    Selected = true;
                    Hover = true;
                    break;
                case RegionStatus.Watching:
                    Stroke = new SolidColorBrush(new Color { A = 255, R = 4, G = 61, B = 246 });
                    Fill = new SolidColorBrush(Colors.Transparent);
                    Selected = true;
                    Hover = false;
                    break;
                default:
                    break;
            }
        }
        //virtual public void Preview(Color c)
        //{
        //    Stroke = new SolidColorBrush(c);
        //    //Fill = new SolidColorBrush(Colors.Transparent);
        //}
    }
}
