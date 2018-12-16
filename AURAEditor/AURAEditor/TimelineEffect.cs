using AuraEditor.UserControls;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Definitions;
using System.ComponentModel;
using AuraEditor.Pages;

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
        private double _left;
        public double Left
        {
            get
            {
                return _left;
            }
            set
            {
                if (_left != value)
                {
                    _left = value;
                    RaisePropertyChanged("Left");

                    double timeUnits = _left / PixelsPerTimeUnit;
                    double seconds = timeUnits * LayerPage.SecondsPerTimeUnit;
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

                    double timeUnits = _width / PixelsPerTimeUnit;
                    double seconds = timeUnits * LayerPage.SecondsPerTimeUnit;
                    _durationTime = seconds * 1000;
                }
            }
        }
        public double Right
        {
            get
            {
                return _left + _width;
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
                double timeUnits = seconds / LayerPage.SecondsPerTimeUnit;

                _left = timeUnits * PixelsPerTimeUnit;
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
                double timeUnits = seconds / LayerPage.SecondsPerTimeUnit;

                _width = timeUnits * PixelsPerTimeUnit;
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
