using AuraEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace AuraEditor.Common
{
    static class EffectHelper
    {
        static string[] _commonEffects =
        {
            "Static",
            "Breathing",
            "Color cycle",
            "Rainbow",
            "Strobing",
            "Comet",
            "Star",
            "Tide",
        };
        static string[] _triggerEffects =
        {
            "Reactive",
            "Laser",
            "Ripple",
        };
        static string[] _otherTriggerEffects =
        {
            "Music",
            "Smart",
        };

        static public ObservableCollection<string> GetCommonEffectBlocks()
        {
            ObservableCollection<string> collection = new ObservableCollection<string>();

            foreach (string name in _commonEffects)
            {
                collection.Add(name);
            }

            return collection;
        }
        static public ObservableCollection<string> GetTriggerEffectBlocks()
        {
            ObservableCollection<string> collection = new ObservableCollection<string>();

            foreach (string name in _triggerEffects)
            {
                collection.Add(name);
            }

            return collection;
        }
        static public ObservableCollection<string> GetOtherTriggerEffectBlocks()
        {
            ObservableCollection<string> collection = new ObservableCollection<string>();

            foreach (string name in _otherTriggerEffects)
            {
                collection.Add(name);
            }

            return collection;
        }

        static public int GetEffectIndex(string effectName)
        {
            List<string> effectBlocks = new List<string>();
            effectBlocks.AddRange(_commonEffects);
            effectBlocks.AddRange(_triggerEffects);
            effectBlocks.AddRange(_otherTriggerEffects);

            // remove index
            char[] charArray = effectName.ToCharArray();
            foreach (char c in charArray)
            {
                if (Char.IsNumber(c))
                {
                    effectName = effectName.Replace(c.ToString(), "");
                    break;
                }
            }

            for (int idx = 0; idx < effectBlocks.Count; idx++)
            {
                if (effectName.Equals(effectBlocks[idx]))
                    return idx;
            }

            return -1;
        }
        static public string GetEffectName(int effectIdx)
        {
            List<string> effectBlocks = new List<string>();
            effectBlocks.AddRange(_commonEffects);
            effectBlocks.AddRange(_triggerEffects);
            effectBlocks.AddRange(_otherTriggerEffects);

            if (effectIdx < effectBlocks.Count)
                return effectBlocks[effectIdx];
            else
                return "";
        }
        static public bool IsCommonEffect(string effectName)
        {
            foreach (string s in _commonEffects)
            {
                if (s == effectName)
                    return true;
            }

            return false;
        }
        static public bool IsTriggerEffect(string effectName)
        {
            foreach (string s in _triggerEffects)
            {
                if (s == effectName)
                    return true;
            }

            return false;
        }
        static public string[] GetCommonEffect()
        {
            return _commonEffects;
        }
        static public string[] GetTriggerEffect()
        {
            return _triggerEffects;
        }
        static public bool IsTriggerEffect(int index)
        {
            string effectName = GetEffectName(index);

            foreach (string s in _triggerEffects)
            {
                if (s == effectName)
                    return true;
            }

            return false;
        }


        public enum DeviceType : int
        {
            Notebook = 0,
            Mouse,
            Keyboard,
            Headset,
            Display,
            Desktop,
            MotherBoard,
            MousePad,
            Microphone,
        }
        static public int GetTypeByTypeName(string typeName)
        {
            if (typeName == "Notebook") return 0;
            else if (typeName == "Mouse") return 1;
            else if (typeName == "Keyboard") return 2;
            else if (typeName == "Headset") return 3;
            else if (typeName == "Display") return 4;
            else if (typeName == "Desktop") return 5;
            else if (typeName == "MotherBoard") return 6;
            else if (typeName == "MousePad") return 7;
            else if (typeName == "Microphone") return 8;
            return 0;
        }
        static public string GetTypeNameByType(int type)
        {
            if (type == 0) return "Notebook";
            else if (type == 1) return "Mouse";
            else if (type == 2) return "Keyboard";
            else if (type == 3) return "Headset";
            else if (type == 4) return "Display";
            else if (type == 5) return "Desktop";
            else if (type == 6) return "MotherBoard";
            else if (type == 7) return "MousePad";
            else if (type == 8) return "Microphone";
            return "Notebook";
        }

        static public string GetEffectNameByNumString(string numberName)
        {
            ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
            if (numberName == "0") return resourceLoader.GetString("StaticEffectText");
            else if (numberName == "1") return resourceLoader.GetString("BreathingEffectText");
            else if (numberName == "2") return resourceLoader.GetString("ColorCycleEffectText");
            else if (numberName == "3") return resourceLoader.GetString("RainbowEffectText");
            else if (numberName == "4") return resourceLoader.GetString("StrobingEffectText");
            else if (numberName == "5") return resourceLoader.GetString("CometEffectText");
            else if (numberName == "6") return resourceLoader.GetString("StarEffectText");
            else if (numberName == "7") return resourceLoader.GetString("TideEffectText");
            else if (numberName == "8") return resourceLoader.GetString("ReactiveEffectText");
            else if (numberName == "9") return resourceLoader.GetString("LaserEffectText");
            else if (numberName == "10") return resourceLoader.GetString("RippleEffectText");
            else if (numberName == "11") return resourceLoader.GetString("MusicEffectText");
            else if (numberName == "12") return resourceLoader.GetString("SmartEffectText");
            return null;
        }

        static public string GetLanguageNameByStringName(string numberName)
        {
            ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
            if (numberName == "Static") return resourceLoader.GetString("StaticEffectText");
            else if (numberName == "Breathing") return resourceLoader.GetString("BreathingEffectText");
            else if (numberName == "Color cycle") return resourceLoader.GetString("ColorCycleEffectText");
            else if (numberName == "Rainbow") return resourceLoader.GetString("RainbowEffectText");
            else if (numberName == "Strobing") return resourceLoader.GetString("StrobingEffectText");
            else if (numberName == "Comet") return resourceLoader.GetString("CometEffectText");
            else if (numberName == "Star") return resourceLoader.GetString("StarEffectText");
            else if (numberName == "Tide") return resourceLoader.GetString("TideEffectText");
            else if (numberName == "Reactive") return resourceLoader.GetString("ReactiveEffectText");
            else if (numberName == "Laser") return resourceLoader.GetString("LaserEffectText");
            else if (numberName == "Ripple") return resourceLoader.GetString("RippleEffectText");
            else if (numberName == "Music") return resourceLoader.GetString("MusicEffectText");
            else if (numberName == "Smart") return resourceLoader.GetString("SmartEffectText");
            return null;
        }

        static public void SetColorPointBorders(List<ColorPointModel> cps)
        {
            int width = 13;

            for (int i = 0; i < cps.Count; i++)
            {
                if (i == 0)
                {
                    cps[i].LeftBorder = 0;
                    cps[i].RightBorder = cps[i + 1].PixelX - width;
                }
                else if (i == (cps.Count - 1))
                {
                    cps[i].LeftBorder = cps[i - 1].PixelX + width;
                    cps[i].RightBorder = 196;
                }
                else
                {
                    cps[i].LeftBorder = cps[i - 1].PixelX + width;
                    cps[i].RightBorder = cps[i + 1].PixelX - width;
                }
            }
        }
        static public LinearGradientBrush ColorPointsToForeground(List<ColorPointModel> cps)
        {
            LinearGradientBrush pattern = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };

            foreach(var cp in cps)
            {
                pattern.GradientStops.Add(
                    new GradientStop
                    {
                        Color = cp.Color,
                        Offset = cp.Offset
                    }
                );
            }

            return pattern;
        }
    }
}
