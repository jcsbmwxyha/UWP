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

        static public int GetTypeByTypeName(string typeName)
        {
            if (typeName == "Notebook") return 0;
            else if (typeName == "Mouse") return 1;
            else if (typeName == "Keyboard") return 2;
            else if (typeName == "ASUS HeadSet") return 3;
            else if (typeName == "Display") return 4;
            else if (typeName == "Desktop") return 5;
            else if (typeName == "MotherBoard") return 6;
            return 0;
        }
        static public string GetTypeNameByType(int type)
        {
            if (type == 0) return "Notebook";
            else if (type == 1) return "Mouse";
            else if (type == 2) return "Keyboard";
            else if (type == 3) return "ASUS HeadSet";
            else if (type == 4) return "Display";
            else if (type == 5) return "Desktop";
            else if (type == 6) return "MotherBoard";
            return "Notebook";
        }
    }
}
