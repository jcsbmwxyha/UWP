using AuraEditor.Common;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.Common.ControlHelper;
using AuraEditor.UserControls;
using System.ComponentModel;
using System.Xml;

namespace AuraEditor
{
    public class DeviceLayer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        public List<TimelineEffect> TimelineEffects;
        public List<TriggerEffect> TriggerEffects;
        public LayerCanvas UICanvas;
        public bool Eye { get; set; }

        private Dictionary<int, int[]> m_ZoneDictionary;
        public Dictionary<int, int[]> GetZoneDictionary()
        {
            return m_ZoneDictionary;
        }

        public string TriggerAction;

        public DeviceLayer(string name = "")
        {
            Name = name;
            TimelineEffects = new List<TimelineEffect>();
            TriggerEffects = new List<TriggerEffect>();
            UICanvas = new LayerCanvas();
            UICanvas.Width = 5000;
            UICanvas.Height = 50;
            UICanvas.MyCanvas.DragOver += Canvas_DragOver;
            UICanvas.MyCanvas.Drop += Canvas_Drop;
            m_ZoneDictionary = new Dictionary<int, int[]>();
            Eye = true;
            TriggerAction = "One Click";
        }
        private Canvas CreateUICanvas()
        {
            Thickness margin = new Thickness(0, 3, 0, 3);
            Canvas canvas = new Canvas
            {
                Width = 5000,
                Height = 44,
                Margin = margin
            };

            canvas.DragOver += Canvas_DragOver;
            canvas.Drop += Canvas_Drop;

            return canvas;
        }

        public void AddDeviceZones(Dictionary<int, int[]> dictionary)
        {
            if (dictionary == null)
                return;

            foreach (var item in dictionary)
            {
                if (!m_ZoneDictionary.ContainsKey(item.Key))
                {
                    m_ZoneDictionary.Add(item.Key, item.Value);
                }
            }
        }
        public void AddDeviceZones(int type, int[] indexes)
        {
            m_ZoneDictionary.Add(type, indexes);
        }
        public void AddTriggerEffect(TriggerEffect effect)
        {
            TriggerEffects.Add(effect);
        }
        public void AddTimelineEffect(TimelineEffect effect)
        {
            TimelineEffects.Add(effect);
            UICanvas.AddElement(effect.UI);
            AnimationStart(effect.UI, "Opacity", 300, 0, 1);
        }

        public async void InsertEffectLine(TimelineEffect insertedEL)
        {
            TimelineEffect overlappedEL = TestAndGetFirstOverlappingEffect(insertedEL);

            if (overlappedEL != null)
            {
                EffectLine overUI = overlappedEL.UI;
                EffectLine inUI = insertedEL.UI;

                if (inUI.X <= overUI.X)
                {
                    double move = inUI.Right - overUI.X;
                    PushAllEffectsWhichOnTheRight(insertedEL, move);
                }
                else if (overUI.X < inUI.X)
                {
                    double source = inUI.X;
                    double target = source + overUI.Right - inUI.X;

                    await AnimationStartAsync(inUI.RenderTransform, "TranslateX", 200, source, target);
                    InsertEffectLine(insertedEL);
                }
            }
        }
        public void PushAllEffectsWhichOnTheRight(TimelineEffect effect, double move)
        {
            foreach (TimelineEffect e in TimelineEffects)
            {
                if (effect.Equals(e))
                    continue;

                if (effect.UI.X <= e.UI.X)
                {
                    double source = e.UI.X;
                    double target = source + move;

                    AnimationStart(e.UI.RenderTransform, "TranslateX", 200, source, target);
                }
            }
        }
        public TimelineEffect TestAndGetFirstOverlappingEffect(TimelineEffect testEffect)
        {
            TimelineEffect result = null;

            foreach (TimelineEffect e in TimelineEffects)
            {
                if (e.Equals(testEffect))
                    continue;

                if (IsOverlapping(testEffect, e))
                {
                    if (result == null)
                        result = e;
                    else if (e.UI.X < result.UI.X)
                    {
                        result = e;
                    }
                }
            }

            return result;
        }
        public TimelineEffect FindEffectByPosition(double x)
        {
            foreach (TimelineEffect e in TimelineEffects)
            {
                double left = e.UI.X;
                double width = e.UI.Width;

                if ((left <= x) && (x <= left + width))
                    return e;
            }
            return null;
        }
        public TimelineEffect FindFirstEffectOnTheRight(double x)
        {
            TimelineEffect result = null;

            foreach (TimelineEffect e in TimelineEffects)
            {
                if (e.UI.X > x)
                {
                    if (result == null)
                        result = e;

                    if (e.UI.X < result.UI.X)
                    {
                        result = e;
                    }
                }
            }
            return result;
        }
        public double GetFirstFreeRoomPosition()
        {
            double roomX = 0;

            for (int i = 0; i < TimelineEffects.Count; i++)
            {
                TimelineEffect effect = TimelineEffects[i];
                EffectLine UI = effect.UI;

                if (roomX <= UI.X && UI.X < roomX + AuraLayerManager.GetPixelsPerSecond())
                {
                    roomX = UI.X + UI.Width;
                    i = -1; // rescan every effect line
                }
            }

            return roomX;
        }
        static private bool IsOverlapping(TimelineEffect effect1, TimelineEffect effect2)
        {
            EffectLine UI_1 = effect1.UI;
            EffectLine UI_2 = effect2.UI;

            return ControlHelper.IsOverlapping(
                UI_1.X, UI_1.Width,
                UI_2.X, UI_2.Width);
        }

        private async void Canvas_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var effectname = await e.DataView.GetTextAsync();

                if (!IsCommonEffect(effectname))
                    e.AcceptedOperation = DataPackageOperation.None;
                else
                    e.AcceptedOperation = DataPackageOperation.Copy;
            }
        }
        private async void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var effectname = await e.DataView.GetTextAsync();
                int type = GetEffectIndex(effectname);

                TimelineEffect effect = new TimelineEffect(this, type);
                AddTimelineEffect(effect);
                MainPage.Self.NeedSave = true;
            }
        }

        public XmlNode ToXmlNodeForScript()
        {
            List<Device> globalDevices = AuraSpaceManager.Self.GlobalDevices;
            XmlNode groupoNode = CreateXmlNodeOfFile("group");
            XmlAttribute attribute = CreateXmlAttributeOfFile("key");
            attribute.Value = Name;
            groupoNode.Attributes.Append(attribute);

            foreach (var d in globalDevices)
            {
                XmlNode deviceNode = d.ToXmlNodeForScript();
                XmlNode usageNode = GetUsageXmlNode(d.Type);

                deviceNode.AppendChild(usageNode);
                groupoNode.AppendChild(deviceNode);
            }

            return groupoNode;
        }
        private XmlNode GetUsageXmlNode(int deviceType)
        {
            XmlNode usageNode = CreateXmlNodeOfFile("usage");

            if (!m_ZoneDictionary.ContainsKey(deviceType))
                return usageNode;

            int[] zoneIndexes = m_ZoneDictionary[deviceType];

            foreach (int index in zoneIndexes)
            {
                XmlNode ledNode = CreateXmlNodeOfFile("led");
                XmlAttribute attribute = CreateXmlAttributeOfFile("key");
                attribute.Value = index.ToString();
                ledNode.Attributes.Append(attribute);
                usageNode.AppendChild(ledNode);
            };

            return usageNode;
        }

        public XmlNode ToXmlNodeForUserData()
        {
            XmlNode layerNode = CreateXmlNodeOfFile("layer");

            XmlAttribute attribute = CreateXmlAttributeOfFile("name");
            attribute.Value = Name;
            layerNode.Attributes.Append(attribute);

            XmlAttribute triggerAttribute = CreateXmlAttributeOfFile("trigger");
            triggerAttribute.Value = TriggerAction;
            layerNode.Attributes.Append(triggerAttribute);

            // devices
            XmlNode devicesNode = CreateXmlNodeOfFile("devices");
            List<Device> globalDevices = AuraSpaceManager.Self.GlobalDevices;
            foreach (var d in globalDevices)
            {
                XmlNode deviceNode = GetDeviceUsageXmlNode(d);
                devicesNode.AppendChild(deviceNode);
            }
            layerNode.AppendChild(devicesNode);

            // effects
            XmlNode effectsNode = CreateXmlNodeOfFile("effects");
            List<Effect> effects = new List<Effect>();
            effects.AddRange(TimelineEffects);
            effects.AddRange(TriggerEffects);
            foreach (var eff in effects)
            {
                XmlNode effNode = eff.ToXmlNodeForUserData();
                effectsNode.AppendChild(effNode);
            }
            layerNode.AppendChild(effectsNode);

            return layerNode;
        }
        private XmlNode GetDeviceUsageXmlNode(Device device)
        {
            XmlNode deviceNode = CreateXmlNodeOfFile("device");

            XmlAttribute attribute = CreateXmlAttributeOfFile("name");
            attribute.Value = device.Name;
            deviceNode.Attributes.Append(attribute);

            XmlAttribute attributeType = CreateXmlAttributeOfFile("type");
            attributeType.Value = GetTypeNameByType(device.Type);
            deviceNode.Attributes.Append(attributeType);

            if (m_ZoneDictionary.ContainsKey(device.Type))
            {
                int[] zoneIndexes = m_ZoneDictionary[device.Type];
                foreach (int index in zoneIndexes)
                {
                    XmlNode indexNode = CreateXmlNodeOfFile("index");
                    indexNode.InnerText = index.ToString();
                    deviceNode.AppendChild(indexNode);
                };
            }

            return deviceNode;
        }
    }
}
