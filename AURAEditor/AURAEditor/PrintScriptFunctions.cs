﻿using System.Text;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.Math2;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Xml;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        public string PrintScriptXml(bool repeat)
        {
            XmlNode root = CreateXmlNode("root");

            XmlNode headerNode = CreateXmlNode("header");
            headerNode.InnerText = "AURA_Creator";
            root.AppendChild(headerNode);

            XmlNode versionNode = CreateXmlNode("version");
            versionNode.InnerText = "1.0";
            root.AppendChild(versionNode);

            root.AppendChild(GetEffecttProviderXmlNode(repeat));
            root.AppendChild(GetViewportXmlNode());
            root.AppendChild(GetEffectListXmlNode());

            return root.OuterXml;
        }
        private XmlNode GetEffecttProviderXmlNode(bool repeat)
        {
            XmlNode effectProviderNode = CreateXmlNode("effectProvider");

            XmlNode periodNode = CreateXmlNode("period");
            periodNode.InnerText = LayerManager.PlayTime.ToString();
            XmlAttribute attribute = CreateXmlAttributeOfFile("key");
            if(repeat)
                attribute.Value = "true";
            else
                attribute.Value = "false";
            periodNode.Attributes.Append(attribute);
            
            effectProviderNode.AppendChild(periodNode);
            effectProviderNode.AppendChild(GetQueueXmlNode());

            return effectProviderNode;
        }
        private XmlNode GetQueueXmlNode()
        {
            XmlNode queueNode = CreateXmlNode("queue");
            double maxDistance = MaxOperatingLength(SpaceManager.MaxOperatingGridWidth, SpaceManager.MaxOperatingGridHeight, 0);
            int effectCount = 0;
            int layerCount = 0;

            foreach (Layer layer in LayerManager.Layers)
            {
                if (layer.Eye == false)
                    continue;

                List<Effect> effCollection = new List<Effect>();
                effCollection.AddRange(layer.TimelineEffects);
                layer.ComputeTriggerEffStartAndDuration();
                effCollection.AddRange(layer.TriggerEffects);

                foreach (Effect eff in effCollection)
                {
                    XmlNode effectNode = CreateXmlNode("effect");

                    // Give uniqle index for all effects
                    eff.ScriptName = GetEffectName(eff.Type) + effectCount.ToString();
                    effectCount++;

                    XmlAttribute attribute = CreateXmlAttributeOfFile("key");
                    attribute.Value = eff.ScriptName;
                    effectNode.Attributes.Append(attribute);

                    XmlNode viewportNode = CreateXmlNode("viewport");
                    viewportNode.InnerText = layer.Name;
                    effectNode.AppendChild(viewportNode);

                    if (IsTriggerEffect(eff.Type))
                    {
                        XmlNode triggerNode = CreateXmlNode("trigger");
                        triggerNode.InnerText = layer.TriggerAction.Replace(" ", "");
                        effectNode.AppendChild(triggerNode);
                    }
                    else
                    {
                        XmlNode triggerNode = CreateXmlNode("trigger");
                        triggerNode.InnerText = "Period";
                        effectNode.AppendChild(triggerNode);
                    }

                    XmlNode delayNode = CreateXmlNode("delay");
                    delayNode.InnerText = eff.StartTime.ToString();
                    effectNode.AppendChild(delayNode);

                    XmlNode durationNode = CreateXmlNode("duration");
                    durationNode.InnerText = eff.DurationTime.ToString();
                    effectNode.AppendChild(durationNode);

                    XmlNode layerNode = CreateXmlNode("layer");
                    layerNode.InnerText = layerCount.ToString();
                    effectNode.AppendChild(layerNode);

                    queueNode.AppendChild(effectNode);
                }

                layerCount++;
            }

            return queueNode;
        }
        private XmlNode GetViewportXmlNode()
        {
            XmlNode viewportNode = CreateXmlNode("viewport");

            foreach (Layer layer in LayerManager.Layers)
            {
                viewportNode.AppendChild(layer.ToXmlNodeForScript());
            }

            return viewportNode;
        }
        private XmlNode GetEffectListXmlNode()
        {
            XmlNode effectListNode = CreateXmlNode("effectList");

            foreach (Layer layer in LayerManager.Layers)
            {
                if (layer.Eye == false)
                    continue;

                List<Effect> effCollection = new List<Effect>();
                effCollection.AddRange(layer.TimelineEffects);
                effCollection.AddRange(layer.TriggerEffects);

                foreach (Effect eff in effCollection)
                {
                    effectListNode.AppendChild(eff.ToXmlNodeForScript(layer.GetCountOfZones()));
                }
            }

            return effectListNode;
        }
    }
}