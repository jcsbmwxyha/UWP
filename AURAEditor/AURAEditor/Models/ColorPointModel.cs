using AuraEditor.Dialogs;
using AuraEditor.UserControls;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.ControlHelper;

namespace AuraEditor.Models
{
    public class ColorPointModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Color _color;
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    RaisePropertyChanged("Color");
                }
            }
        }

        private double _pixelX;
        public double PixelX
        {
            get
            {
                return _pixelX;
            }
            set
            {
                if (_pixelX != value)
                {
                    _pixelX = value;
                    RaisePropertyChanged("PixelX");
                }
            }
        }
        public double Offset
        {
            get
            {
                return _pixelX / 196;
            }
            set
            {
                _pixelX = value * 196;
                RaisePropertyChanged("PixelX");
            }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    RaisePropertyChanged("IsChecked");
                }
            }
        }

        public double LeftBorder { get; set; }
        public double RightBorder { get; set; }
        public ColorPointView View { get; internal set; }

        public ColorPatternModel ParentPattern;

        public ColorPointModel()
        {
            Offset = 0;
        }
        public ColorPointModel(TriggerDialog td)
        {
            Offset = 0;
        }
        public ColorPointModel(ColorPointModel cp, TriggerDialog td)
        {
            Offset = cp.Offset;
            Color = cp.Color;
        }

        static public ColorPointModel Copy(ColorPointModel cpm)
        {
            return new ColorPointModel
            {
                ParentPattern = cpm.ParentPattern,
                Offset = cpm.Offset,
                Color = cpm.Color,
                LeftBorder = cpm.LeftBorder,
                RightBorder = cpm.RightBorder
            };
        }
    }
}
