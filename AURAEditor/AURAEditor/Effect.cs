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

namespace AuraEditor
{
    public class Effect
    {
        public DeviceLayer Layer { get; set; }
        public string EffectName { get; set; }
        public string EffectLuaName { get; set; }
        public int EffectType { get; set; }
        public EffectLine UI { get; }
        public double UI_X
        {
            get
            {
                CompositeTransform ct = UI.RenderTransform as CompositeTransform;
                return ct.TranslateX;
            }
            set
            {
                CompositeTransform ct = UI.RenderTransform as CompositeTransform;
                ct.TranslateX = value;
            }
        }
        public double UI_Width
        {
            get { return UI.Width; }
            set
            {
                UI.Width = value;
            }
        }
        public double UI_Right { get { return UI_X + UI_Width; } }
        public double StartTime
        {
            get
            {
                CompositeTransform ct = UI.RenderTransform as CompositeTransform;
                double timeUnits = ct.TranslateX / AuraCreatorManager.pixelsPerTimeUnit;
                double seconds = timeUnits * AuraCreatorManager.secondsPerTimeUnit;

                return seconds * 1000;
            }
            set
            {
                CompositeTransform ct = UI.RenderTransform as CompositeTransform;
                double seconds = value / 1000;
                double timeUnits = seconds / AuraCreatorManager.secondsPerTimeUnit;

                ct.TranslateX = timeUnits * AuraCreatorManager.pixelsPerTimeUnit;
            }
        }
        public double DurationTime
        {
            get
            {
                double timeUnits = UI.Width / AuraCreatorManager.pixelsPerTimeUnit;
                double seconds = timeUnits * AuraCreatorManager.secondsPerTimeUnit;

                return seconds * 1000;
            }
            set
            {
                double seconds = value / 1000;
                double timeUnits = seconds / AuraCreatorManager.secondsPerTimeUnit;

                UI.Width = timeUnits * AuraCreatorManager.pixelsPerTimeUnit;
            }
        }
        private EffectInfo _info;
        public EffectInfo Info
        {
            get { return _info; }
            set
            {
                if ((EffectType != 3) && (EffectType != 6) && (EffectType != 2))
                    UI.Background = new SolidColorBrush(value.Color);
                _info = value;
            }
        }

        public Effect(DeviceLayer layer, int effectType)
        {
            Layer = layer;
            EffectType = effectType;
            EffectName = GetEffectName(effectType);
            UI = CreateEffectUI(effectType);
            UI.DataContext = this;
            UI_X = (int)Layer.GetFirstFreeRoomPosition();
            DurationTime = 1000; // 1s
            Info = new EffectInfo(effectType);
        }
        private EffectLine CreateEffectUI(int effectType)
        {
            EffectLine el = new EffectLine
            {
                Height = 34,
                Width = AuraCreatorManager.GetPixelsPerSecond(),
                HorizontalAlignment = HorizontalAlignment.Center,
                ManipulationMode = ManipulationModes.TranslateX
            };

            CompositeTransform ct = el.RenderTransform as CompositeTransform;
            ct.TranslateY = 5;

            return el;
        }
    }
}
