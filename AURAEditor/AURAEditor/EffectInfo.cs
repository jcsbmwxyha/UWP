using AuraEditor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using static AuraEditor.Common.EffectHelper;

namespace AuraEditor
{
    public class EffectInfo
    {
        public Color Color { get; set; }
        public int WaveType { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double WaveLength { get; set; }
        public double Freq { get; set; }
        public double Phase { get; set; }
        public double Start { get; set; }
        public double Velocity { get; set; }

        public EffectInfo()
        {
            Color = Colors.Red;
            WaveType = 0;
            Min = 0;
            Max = 0;
            WaveLength = 0;
            Freq = 0;
            Phase = 0;
            Start = 0;
            Velocity = 0;
        }
        public EffectInfo(int type)
        {
            Color = Colors.Red;
            WaveType = GetSuggestedWaveTypeValue(type);
            Min = GetSuggestedMinValue(type);
            Max = GetSuggestedMaxValue(type);
            WaveLength = GetSuggestedWaveLenValue(type);
            Freq = GetSuggestedFreqValue(type);
            Phase = GetSuggestedPhaseValue(type);
            Start = 0;
            Velocity = GetSuggestedVelocityValue(type);
        }
    }
}
