using AuraEditor.Common;
using AuraEditor.UserControls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.Math2;
using static AuraEditor.Common.XmlHelper;

namespace AuraEditor
{
    public enum LayerVisualStatus
    {
        Normal = 0,
        Hover,
        Selected,
    }
    public class Layer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public List<TimelineEffect> TimelineEffects;
        public List<TriggerEffect> TriggerEffects;

        public LayerTitle UI_Title;
        private bool eye;
        public bool Eye
        {
            get { return eye; }
            set
            {
                if (eye != value)
                {
                    eye = value;
                    RaisePropertyChanged("Eye");
                }
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
        private bool isTriggering;
        public bool IsTriggering
        {
            get { return isTriggering; }
            set
            {
                if (isTriggering != value)
                {
                    isTriggering = value;
                    RaisePropertyChanged("IsTriggering");

                    if (value == true)
                        UI_Background.GoToState("Trigger");
                    else
                        UI_Background.GoToState("NoTrigger");
                }
            }
        }
        public LayerTrack UI_Track;
        public LayerBackground UI_Background;

        private Dictionary<int, int[]> m_ZoneDictionary;
        public Dictionary<int, int[]> GetZoneDictionary()
        {
            return m_ZoneDictionary;
        }
        public int GetCountOfZones()
        {
            int count = 0;
            foreach (KeyValuePair<int, int[]> pair in m_ZoneDictionary)
                count += pair.Value.Length;
            return count;
        }
        public string TriggerAction;

        public Layer(string name = "")
        {
            TimelineEffects = new List<TimelineEffect>();
            TriggerEffects = new List<TriggerEffect>();

            Name = name;
            Eye = true;
            UI_Title = new LayerTitle
            {
                DataContext = this
            };
            UI_Track = new LayerTrack
            {
                DataContext = this,
            };
            UI_Background = new LayerBackground
            {
                DataContext = this,
            };

            m_ZoneDictionary = new Dictionary<int, int[]>();
            TriggerAction = "One Click";
        }

        public void AddTimelineEffect(TimelineEffect effect)
        {
            effect.Layer = this;
            effect.UI.X = GetFirstFreeRoomPosition(effect.UI.Width);
            UI_Track.AddEffectline(effect.UI);
            AnimationStart(effect.UI, "Opacity", 300, 0, 1);

            TimelineEffects.Add(effect);
        }
        public async void AddAndInsertTimelineEffect(TimelineEffect effect)
        {
            effect.Layer = this;
            await InsertTimelineEffect(effect);
            UI_Track.AddEffectline(effect.UI);
            AnimationStart(effect.UI, "Opacity", 300, 0, 1);

            TimelineEffects.Add(effect);
        }
        public void AddTriggerEffect(TriggerEffect effect)
        {
            effect.Layer = this;
            TriggerEffects.Add(effect);
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
        public void SetDeviceZones(int type, int[] indexes)
        {
            m_ZoneDictionary.Remove(type);
            m_ZoneDictionary.Add(type, indexes);
        }

        public async Task InsertTimelineEffect(TimelineEffect insertedEL)
        {
            TimelineEffect overlappedEL = TestAndGetFirstOverlappingEffect(insertedEL);
            insertedEL.Layer = this;

            if (overlappedEL != null)
            {
                EffectLine overUI = overlappedEL.UI;
                EffectLine inUI = insertedEL.UI;

                if (inUI.X <= overUI.X)
                {
                    double move = inUI.Right - overUI.X;
                    PushAllRightsideEffectsToRight(insertedEL, move);
                }
                else if (overUI.X < inUI.X)
                {
                    double source = inUI.X;
                    double target = source + overUI.Right - inUI.X;

                    await AnimationStartAsync(inUI.RenderTransform, "TranslateX", 200, source, target);
                    await InsertTimelineEffect(insertedEL);
                }
            }
        }
        public void DeleteEffectLine(EffectLine el)
        {
            EffectLine next = GetNextEffectLine(el);

            if (next == null)
                next = GetPreviousEffectLine(el);

            if (next != null)
                next.IsChecked = true;
            else
                MainPage.Self.SelectedEffectLine = null;

            UI_Track.RemoveEffectline(el);
            TimelineEffects.Remove(el.DataContext as TimelineEffect);

        }
        private EffectLine GetNextEffectLine(EffectLine el)
        {
            TimelineEffect find = FindFirstEffectOnTheRight(el.Right);

            if (find == null)
                return null;
            else
                return find.UI;
        }
        private EffectLine GetPreviousEffectLine(EffectLine el)
        {
            double rightmostPosition = 0;
            TimelineEffect previousEffect = null;

            foreach (var effect in TimelineEffects)
            {
                if (effect.UI.Equals(el))
                    continue;

                double end = effect.EndTime;

                if (end > rightmostPosition)
                {
                    rightmostPosition = end;
                    previousEffect = effect;
                }
            }

            if (previousEffect != null)
                return previousEffect.UI;
            else
                return null;
        }

        public void PushAllRightsideEffectsToRight(TimelineEffect effect, double move)
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
                if (e.UI.X >= x)
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
        public double GetFirstFreeRoomPosition(double needRoomLength)
        {
            double roomX = 0;

            for (int i = 0; i < TimelineEffects.Count; i++)
            {
                TimelineEffect effect = TimelineEffects[i];
                EffectLine UI = effect.UI;

                if (roomX <= UI.X && UI.X < roomX + needRoomLength)
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

        public void ComputeTriggerEffStartAndDuration()
        {
            double delay_accu = 0;
            double duration = 0;
            int width = AuraSpaceManager.Self.MaxOperatingGridWidth;
            int height = AuraSpaceManager.Self.MaxOperatingGridHeight;
            double angle = 0;
            double length = MaxOperatingLength(width, height, angle);
            double ledSpeed = 0;

            foreach (var eff in TriggerEffects)
            {
                if (GetEffectName(eff.Type) == "Reactive") // No speed problem, give 1 second
                    duration = 1000;
                else
                {
                    ledSpeed = EffectInfo.GetLedSpeed(eff.Type, eff.Info.Speed);
                    duration = (length / ledSpeed) * 1000 + 100; // buffer : 100ms
                }

                eff.StartTime = delay_accu;
                eff.DurationTime = duration;
                delay_accu += duration;
            }
        }
        public XmlNode ToXmlNodeForScript()
        {
            List<Device> globalDevices = AuraSpaceManager.Self.GlobalDevices;
            XmlNode groupoNode = CreateXmlNode("group");
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
            XmlNode usageNode = CreateXmlNode("usage");

            if (!m_ZoneDictionary.ContainsKey(deviceType))
                return usageNode;

            int[] zoneIndexes = m_ZoneDictionary[deviceType];

            foreach (int index in zoneIndexes)
            {
                XmlNode ledNode = CreateXmlNode("led");
                XmlAttribute attribute = CreateXmlAttributeOfFile("key");
                attribute.Value = index.ToString();
                ledNode.Attributes.Append(attribute);
                usageNode.AppendChild(ledNode);
            };

            return usageNode;
        }

        public XmlNode ToXmlNodeForUserData()
        {
            XmlNode layerNode = CreateXmlNode("layer");

            XmlAttribute attribute = CreateXmlAttributeOfFile("name");
            attribute.Value = Name;
            layerNode.Attributes.Append(attribute);

            XmlAttribute triggerAttribute = CreateXmlAttributeOfFile("trigger");
            triggerAttribute.Value = TriggerAction;
            layerNode.Attributes.Append(triggerAttribute);
            
            XmlAttribute attributeEye = CreateXmlAttributeOfFile("Eye");
            attributeEye.Value = Eye.ToString();
            layerNode.Attributes.Append(attributeEye);

            // devices
            XmlNode devicesNode = CreateXmlNode("devices");
            List<Device> globalDevices = AuraSpaceManager.Self.GlobalDevices;
            foreach (var d in globalDevices)
            {
                XmlNode deviceNode = GetDeviceUsageXmlNode(d);
                devicesNode.AppendChild(deviceNode);
            }
            layerNode.AppendChild(devicesNode);

            // effects
            XmlNode effectsNode = CreateXmlNode("effects");
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
            XmlNode deviceNode = CreateXmlNode("device");

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
                    XmlNode indexNode = CreateXmlNode("index");
                    indexNode.InnerText = index.ToString();
                    deviceNode.AppendChild(indexNode);
                };
            }

            return deviceNode;
        }
    }
}
