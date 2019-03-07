using AuraEditor.Models;
using System;
using System.Xml;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.XmlHelper;

namespace AuraEditor
{
    public class Effect
    {
        public LayerModel Layer { get; set; }
        public string Name { get; set; }
        public string ScriptName { get; set; }
        public int Type { get; private set; }
        public virtual double StartTime { get; set; }
        public virtual double DurationTime { get; set; }
        public virtual double EndTime { get { return StartTime + DurationTime; } }
        public EffectInfoModel Info { get; set; }



        public Effect(int effectType)
        {
            Type = effectType;
            Name = GetEffectName(effectType);
            Info = new EffectInfoModel(effectType);
        }
        public Effect(string effectName)
        {
            Type = GetEffectIndex(effectName);
            Name = effectName;
            Info = new EffectInfoModel(Type);
        }
        public void ChangeType(int effectType)
        {
            Type = effectType;
            Name = GetEffectName(effectType);
            Info.ChangeType(effectType);
        }

        public virtual XmlNode ToXmlNodeForScript(int zoneCount)
        {
            XmlNode effectNode = Info.ToXmlNodeForScript(zoneCount);
            XmlAttribute attribute = CreateXmlAttributeOfFile("key");
            attribute.Value = ScriptName;
            effectNode.Attributes.Append(attribute);

            return effectNode;
        }

        public virtual XmlNode ToXmlNodeForUserData()
        {
            XmlNode effNode = Info.ToXmlNodeForUserData();

            XmlNode startNode = CreateXmlNode("start");
            startNode.InnerText = ((int)StartTime).ToString();
            effNode.AppendChild(startNode);

            XmlNode durationNode = CreateXmlNode("duration");
            durationNode.InnerText = ((int)DurationTime).ToString();
            effNode.AppendChild(durationNode);

            return effNode;
        }

      
    }
}
