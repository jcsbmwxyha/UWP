using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AuraEditor.Common
{
    static class EffectHelper
    {
        static string[] _commonEffects =
        {
            "Static",
            "Breath",
            "ColorCycle",
            "Rainbow",
            "Strobing",
            "Comet",
            "Star",
        };
        static string[] _triggerEffects =
        {
            "Raidus",
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
        static public int GetDeviceTypeByDeviceName(string deviceName)
        {
            deviceName = deviceName.ToUpper();
            if (deviceName == "G703GI") return 0;
            if (deviceName == "GL504GM") return 0;
            if (deviceName == "GL504GS") return 0;
            if (deviceName == "G703VI") return 0;
            if (deviceName == "GM501GS") return 0;

            if (deviceName == "GLADIUS II") return 1;
            if (deviceName == "PUGIO") return 1;

            if (deviceName == "FLARE") return 2;
            if (deviceName == "GL12CM") return 4;
            return 0;
        }
        static public int GetDeviceTypeByTypeName(string deviceName)
        {
            deviceName = deviceName.ToLower();
            if (deviceName == "notebook") return 0;
            if (deviceName == "mouse") return 1;
            if (deviceName == "keyboard") return 2;
            if (deviceName == "headset") return 3;
            if (deviceName == "desktop") return 4;
            return 0;
        }
        static public string GetTypeNameByType(int deviceType)
        {
            if (deviceType == 0) return "notebook";
            if (deviceType == 1) return "mouse";
            if (deviceType == 2) return "keyboard";
            if (deviceType == 3) return "headset";
            if (deviceType == 4) return "desktop";
            return "notebook";
        }
    }
}
