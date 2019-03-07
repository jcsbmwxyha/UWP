using AuraEditor.Models;
using AuraEditor.Pages;
using AuraEditor.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.Definitions;

namespace AuraEditor.ViewModels
{
    public class EffectLineViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public TimelineEffect Model;
        public LayerModel Layer { get; set; }
        private EffectLine _view;
        public EffectLine View
        {
            get
            {
                return _view;
            }
            set
            {
                _view = value;
            }
        }
        public string Name
        {
            get
            {
                return Model.Name;
            }
        }
        public double Left
        {
            get
            {
                double seconds = StartTime / 1000;
                return seconds * LayerPage.PixelsPerSecond;
            }
            set
            {
                double seconds = value / LayerPage.PixelsPerSecond;
                StartTime = seconds * 1000;
                RaisePropertyChanged("Left");
            }
        }
        public double Width
        {
            get
            {
                double seconds = DurationTime / 1000;
                return seconds * LayerPage.PixelsPerSecond;
            }
            set
            {
                double seconds = value / LayerPage.PixelsPerSecond;
                DurationTime = seconds * 1000;
                RaisePropertyChanged("Width");
            }
        }
        public double Right
        {
            get
            {
                return Left + Width;
            }
        }
        public double StartTime
        {
            get
            {
                return Model.StartTime;
            }
            set
            {
                Model.StartTime = value;
                RaisePropertyChanged("StartTime");
            }
        }
        public double DurationTime
        {
            get
            {
                return Model.DurationTime;
            }
            set
            {
                Model.DurationTime = value;
                RaisePropertyChanged("DurationTime");
            }
        }
        public virtual double EndTime { get { return StartTime + DurationTime; } }
        
        public delegate void MoveToEventHandler(double value);
        public event MoveToEventHandler MoveTo;
        public void MovePosition(double value)
        {
            MoveTo?.Invoke(value);
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
                if (_isChecked == value)
                    return;

                if (value == true)
                    LayerPage.Self.CheckedEffect = this;

                _isChecked = value;
                RaisePropertyChanged("IsChecked");
            }
        }

        private string effectBlockContent;
        public string EffectBlockContent
        {
            get { return effectBlockContent; }
            set
            {
                if (effectBlockContent != value)
                {
                    effectBlockContent = value;
                    RaisePropertyChanged("EffectBlockContent");
                }
            }

        }
        public double PixelSizeOfName { get; set; }



        public EffectLineViewModel(TimelineEffect eff)
        {
            Model = eff;
            EffectBlockContent = Name;
            PixelSizeOfName = getPixelSizeOfName(Name);

        }
        private EffectLineViewModel(EffectLineViewModel vm)
        {
            TimelineEffect eff = TimelineEffect.CloneEffect(vm.Model);
            Model = eff;
            EffectBlockContent = Name;
            PixelSizeOfName = getPixelSizeOfName(Name);
        }
        public EffectLineViewModel(int type)
        {
            TimelineEffect eff = new TimelineEffect(type);
            Model = eff;
            EffectBlockContent = Name;
            PixelSizeOfName = getPixelSizeOfName(Name);
        }

        static public EffectLineViewModel Clone(EffectLineViewModel vm)
        {
            return new EffectLineViewModel(vm);
        }

        private double getPixelSizeOfName(string text)
        {
            var tmp = new TextBlock { Text = text, FontSize = 20, FontFamily = new FontFamily("Segoe UI") };
            tmp.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Size NameSize = tmp.DesiredSize;
            return NameSize.Width;
        }
    }
}
