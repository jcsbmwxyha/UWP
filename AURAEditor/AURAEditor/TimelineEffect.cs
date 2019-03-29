using AuraEditor.Models;
using AuraEditor.Pages;
using AuraEditor.UserControls;
using System.ComponentModel;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Definitions;

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
        
        public override double StartTime { get; set; }
        public override double DurationTime { get; set; }

        public TimelineEffect(int effectType) : base(effectType)
        {
            DurationTime = 3000; // 3s
        }

        static public TimelineEffect Clone(TimelineEffect copy)
        {
            TimelineEffect target = new TimelineEffect(copy.Type);

            target.Info = copy.Info.Clone() as EffectInfoModel;
            target.DurationTime = copy.DurationTime;

            return target;
        }
    }
}
