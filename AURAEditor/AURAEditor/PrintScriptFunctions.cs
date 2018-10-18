using System.Text;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.Common.EffectHelper;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Xml;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        public string PrintScriptXml()
        {
            XmlNode root = CreateXmlNodeOfFile("root");

            XmlNode headerNode = CreateXmlNodeOfFile("header");
            headerNode.InnerText = "AURA_Creator";
            root.AppendChild(headerNode);

            XmlNode versionNode = CreateXmlNodeOfFile("version");
            versionNode.InnerText = "1.0";
            root.AppendChild(versionNode);

            root.AppendChild(GetEffecttProviderXmlNode());
            root.AppendChild(GetViewportXmlNode());
            root.AppendChild(GetEffectListXmlNode());

            return root.OuterXml;
        }
        private XmlNode GetEffecttProviderXmlNode()
        {
            XmlNode effectProviderNode = CreateXmlNodeOfFile("effectProvider");

            XmlNode periodNode = CreateXmlNodeOfFile("period");
            periodNode.InnerText = LayerManager.PlayTime.ToString();
            effectProviderNode.AppendChild(periodNode);
            effectProviderNode.AppendChild(GetQueueXmlNode());

            return effectProviderNode;
        }
        private XmlNode GetQueueXmlNode()
        {
            XmlNode queueNode = CreateXmlNodeOfFile("queue");
            int effectCount = 0;

            foreach (DeviceLayer layer in LayerManager.DeviceLayers)
            {
                if (layer.Eye == false)
                    continue;

                List<Effect> effCollection = new List<Effect>();
                effCollection.AddRange(layer.TimelineEffects);
                effCollection.AddRange(layer.TriggerEffects);

                foreach (Effect eff in effCollection)
                {
                    XmlNode effectNode = CreateXmlNodeOfFile("effect");

                    // Give uniqle index for all effects
                    eff.ScriptName = GetEffectName(eff.Type) + effectCount.ToString();
                    effectCount++;

                    XmlAttribute attribute = CreateXmlAttributeOfFile("key");
                    attribute.Value = eff.ScriptName;
                    effectNode.Attributes.Append(attribute);

                    XmlNode viewportNode = CreateXmlNodeOfFile("viewport");
                    viewportNode.InnerText = layer.Name;
                    effectNode.AppendChild(viewportNode);

                    if (IsTriggerEffect(eff.Type))
                    {
                        XmlNode triggerNode = CreateXmlNodeOfFile("trigger");
                        triggerNode.InnerText = layer.TriggerAction.Replace(" ","");
                        effectNode.AppendChild(triggerNode);
                    }
                    else
                    {
                        XmlNode triggerNode = CreateXmlNodeOfFile("trigger");
                        triggerNode.InnerText = "Period";
                        effectNode.AppendChild(triggerNode);
                    }

                    XmlNode delayNode = CreateXmlNodeOfFile("delay");
                    delayNode.InnerText = eff.StartTime.ToString();
                    effectNode.AppendChild(delayNode);

                    XmlNode durationNode = CreateXmlNodeOfFile("duration");
                    durationNode.InnerText = eff.DurationTime.ToString();
                    effectNode.AppendChild(durationNode);

                    queueNode.AppendChild(effectNode);
                }
            }

            return queueNode;
        }
        private XmlNode GetViewportXmlNode()
        {
            XmlNode viewportNode = CreateXmlNodeOfFile("viewport");

            foreach (DeviceLayer layer in LayerManager.DeviceLayers)
            {
                viewportNode.AppendChild(layer.ToXmlNodeForScript());
            }

            return viewportNode;
        }
        private XmlNode GetEffectListXmlNode()
        {
            XmlNode effectListNode = CreateXmlNodeOfFile("effectList");

            foreach (DeviceLayer layer in LayerManager.DeviceLayers)
            {
                if (layer.Eye == false)
                    continue;

                List<Effect> effCollection = new List<Effect>();
                effCollection.AddRange(layer.TimelineEffects);
                effCollection.AddRange(layer.TriggerEffects);

                foreach (Effect eff in effCollection)
                {
                    effectListNode.AppendChild(eff.ToXmlNodeForScript());
                }
            }

            return effectListNode;
        }
    }
}