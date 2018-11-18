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

        //public EffectLine UI { get; }
        
        private double _testX;
        public double TestX {
            get
            {
                return _testX;
            }
            set
            {
                if (_testX != value)
                {
                    _testX = value;
                    RaisePropertyChanged("TestX");
                }
            }
        }
        private double _testW;
        public double TestW
        {
            get
            {
                return _testW;
            }
            set
            {
                if (_testW != value)
                {
                    _testW = value;
                    RaisePropertyChanged("TestW");
                }
            }
        }
        public double TestRight
        {
            get
            {
                return _testX + _testW;
            }
        }
        private bool _testIsChecked;
        public bool TestIsChecked
        {
            get
            {
                return _testIsChecked;
            }
            set
            {
                if (_testIsChecked != value)
                {
                    _testIsChecked = value;
                    RaisePropertyChanged("TestIsChecked");
                }
            }
        }
        public override double StartTime
        {
            get
            {
                double timeUnits = _testX / AuraLayerManager.PixelsPerTimeUnit;
                double seconds = timeUnits * AuraLayerManager.SecondsPerTimeUnit;

                return seconds * 1000;
            }
            set
            {
                double seconds = value / 1000;
                double timeUnits = seconds / AuraLayerManager.SecondsPerTimeUnit;

                _testX = timeUnits * AuraLayerManager.PixelsPerTimeUnit;
                RaisePropertyChanged("TestX");
            }
        }
        public override double DurationTime
        {
            get
            {
                double timeUnits = _testW / AuraLayerManager.PixelsPerTimeUnit;
                double seconds = timeUnits * AuraLayerManager.SecondsPerTimeUnit;

                return seconds * 1000;
            }
            set
            {
                double seconds = value / 1000;
                double timeUnits = seconds / AuraLayerManager.SecondsPerTimeUnit;

                _testW = timeUnits * AuraLayerManager.PixelsPerTimeUnit;
                RaisePropertyChanged("TestW");
            }
        }

        public TimelineEffect(int effectType) : base(effectType)
        {
            //UI = CreateEffectUI(effectType);
            //UI.DataContext = this;
            DurationTime = 1000; // 1s
        }
        private EffectLine CreateEffectUI(int effectType)
        {
            EffectLine el = new EffectLine
            {
                Height = 34,
                Width = AuraLayerManager.GetPixelsPerSecond(),
                ManipulationMode = ManipulationModes.TranslateX
            };

            //CompositeTransform ct = el.RenderTransform as CompositeTransform;
            //ct.TranslateY = 8;

            return el;
        }

        public void MoveTo(double x)
        {

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
