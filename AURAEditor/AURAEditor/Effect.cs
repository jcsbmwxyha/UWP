using AuraEditor.Common;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.LuaHelper;

namespace AuraEditor
{
    public class Effect
    {
        public DeviceLayer Layer { get; set; }
        public string Name { get; private set; }
        public string LuaName { get; set; }
        public int Type { get; private set; }
        public EffectInfo Info { get; set; }
        public virtual double StartTime { get; set; }
        public virtual double DurationTime { get; set; }
        public UIInfo UInfo { get; set; }

        public Effect(DeviceLayer layer, int effectType)
        {
            Layer = layer;
            Type = effectType;
            Name = GetEffectName(effectType);
            Info = new EffectInfo(effectType);
            UInfo = new UIInfo(effectType);
        }
        public Effect(DeviceLayer layer, string effectName)
        {
            Layer = layer;
            Type = GetEffectIndex(effectName);
            Name = effectName;
            Info = new EffectInfo(Type);
            UInfo = new UIInfo(Type);
        }
        public void ChangeType(int effectType)
        {
            Type = effectType;
            Name = GetEffectName(effectType);
            Info = new EffectInfo(effectType);
            UInfo = new UIInfo(effectType);
        }

        public virtual Table ToTable()
        {
            return Info.ToTable();
        }
        public virtual XmlNode ToXmlNode()
        {
            XmlNode effNode = UInfo.ToXmlNode();

            XmlNode startNode = CreateXmlNodeOfFile("start");
            startNode.InnerText = StartTime.ToString();
            effNode.AppendChild(startNode);

            XmlNode durationNode = CreateXmlNodeOfFile("duration");
            durationNode.InnerText = DurationTime.ToString();
            effNode.AppendChild(durationNode);

            return effNode;
        }
    }
}
