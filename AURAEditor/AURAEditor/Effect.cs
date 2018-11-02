﻿using AuraEditor.Common;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.XmlHelper;

namespace AuraEditor
{
    public class Effect
    {
        public Layer Layer { get; set; }
        public string Name { get; private set; }
        public string ScriptName { get; set; }
        public int Type { get; private set; }
        public virtual double StartTime { get; set; }
        public virtual double DurationTime { get; set; }
        public EffectInfo Info { get; set; }

        public Effect(Layer layer, int effectType)
        {
            Layer = layer;
            Type = effectType;
            Name = GetEffectName(effectType);
            Info = new EffectInfo(effectType);
        }
        public Effect(Layer layer, string effectName)
        {
            Layer = layer;
            Type = GetEffectIndex(effectName);
            Name = effectName;
            Info = new EffectInfo(Type);
        }
        public void ChangeType(int effectType)
        {
            Type = effectType;
            Name = GetEffectName(effectType);
            Info = new EffectInfo(effectType);
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
            startNode.InnerText = StartTime.ToString();
            effNode.AppendChild(startNode);

            XmlNode durationNode = CreateXmlNode("duration");
            durationNode.InnerText = DurationTime.ToString();
            effNode.AppendChild(durationNode);

            return effNode;
        }
    }
}
