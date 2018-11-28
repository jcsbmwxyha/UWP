using AuraEditor.Common;
using AuraEditor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static AuraEditor.MainPage;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.ControlHelper;
using MoonSharp.Interpreter;
using System.ComponentModel;

namespace AuraEditor
{
    public class TimelineEffect : Effect, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

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
        private double _x;
        public double X
        {
            get
            {
                return _x;
            }
            set
            {
                if (_x != value)
                {
                    _x = value;
                    RaisePropertyChanged("X");

                    double timeUnits = _x / AuraLayerManager.PixelsPerTimeUnit;
                    double seconds = timeUnits * AuraLayerManager.SecondsPerTimeUnit;
                    _startTime = seconds * 1000;
                }
            }
        }
        private double _width;
        public double Width
        {
            get
            {
                return _width;
            }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    RaisePropertyChanged("Width");

                    double timeUnits = _width / AuraLayerManager.PixelsPerTimeUnit;
                    double seconds = timeUnits * AuraLayerManager.SecondsPerTimeUnit;
                    _durationTime = seconds * 1000;
                }
            }
        }
        public double Right
        {
            get
            {
                return _x + _width;
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
                _isChecked = value;
                RaisePropertyChanged("IsChecked");
            }
        }
        private double _startTime;
        public override double StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                double seconds = value / 1000;
                double timeUnits = seconds / AuraLayerManager.SecondsPerTimeUnit;

                _x = timeUnits * AuraLayerManager.PixelsPerTimeUnit;
                _startTime = value;
                RaisePropertyChanged("X");
            }
        }
        private double _durationTime;
        public override double DurationTime
        {
            get
            {
                return _durationTime;
            }
            set
            {
                double seconds = value / 1000;
                double timeUnits = seconds / AuraLayerManager.SecondsPerTimeUnit;

                _width = timeUnits * AuraLayerManager.PixelsPerTimeUnit;
                _durationTime = value;
                RaisePropertyChanged("Width");
            }
        }

        public TimelineEffect(int effectType) : base(effectType)
        {
            DurationTime = 1000; // 1s
        }

        public void MoveTo(double target)
        {
            CompositeTransform ct = View.RenderTransform as CompositeTransform;
            AnimationStart(View.RenderTransform, "TranslateX", 300, ct.TranslateX, target);
        }
        static public TimelineEffect CloneEffect(TimelineEffect copy)
        {
            TimelineEffect target = new TimelineEffect(copy.Type);

            target.Info = copy.Info.Clone() as EffectInfo;
            target.DurationTime = copy.DurationTime;

            return target;
        }
    }
}
