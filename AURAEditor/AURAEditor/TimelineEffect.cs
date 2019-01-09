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

                    double timeUnits = _left / PixelsBetweenLongLines;
                    double seconds = timeUnits * LayerPage.SecondsBetweenLongLines;
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

                    double timeUnits = _width / PixelsBetweenLongLines;
                    double seconds = timeUnits * LayerPage.SecondsBetweenLongLines;
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
                double timeUnits = seconds / LayerPage.SecondsBetweenLongLines;

                _left = timeUnits * PixelsBetweenLongLines;
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
                double timeUnits = seconds / LayerPage.SecondsBetweenLongLines;

                _width = timeUnits * PixelsBetweenLongLines;
                _durationTime = value;
                RaisePropertyChanged("Width");
            }
        }

        public TimelineEffect(int effectType) : base(effectType)
        {
            DurationTime = 3000; // 3s
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
