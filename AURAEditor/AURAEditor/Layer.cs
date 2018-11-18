using AuraEditor.Common;
using AuraEditor.UserControls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.Math2;
using static AuraEditor.Common.XmlHelper;

namespace AuraEditor
{
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

        public ObservableCollection<TimelineEffect> TimelineEffects;
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
        public string TriggerAction;
        
        public Layer(string name = "")
        {
            TimelineEffects = new ObservableCollection<TimelineEffect>();
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

        #region Track behavior
        public void AddTimelineEffect(TimelineEffect eff)
        {
            eff.Layer = this;
            double oneSecLength = AuraLayerManager.PixelsPerTimeUnit / AuraLayerManager.SecondsPerTimeUnit;
            eff.TestX = GetFirstRoomPosition(oneSecLength);
            eff.TestW = oneSecLength;
            //UI_Track.AddEffectline(effect.UI);
            //AnimationStart(eff.UI, "Opacity", 300, 0, 1);

            TimelineEffects.Add(eff);
        }
        public async void AddAndInsertTimelineEffect(TimelineEffect effect)
        {
            effect.Layer = this;
            await TryPlaceEffect(effect);
            //UI_Track.AddEffectline(effect.UI);
            //AnimationStart(effect.UI, "Opacity", 300, 0, 1);

            TimelineEffects.Add(effect);
        }
        public void AppendTimelineEffect(TimelineEffect insertedEL)
        {
        }
        public void InsertTimelineEffect(TimelineEffect insertedEL)
        {
        }
        public async Task TryPlaceEffect(TimelineEffect placedEff)
        {
            TimelineEffect crossingEff = GetFirstCrossingEffect(placedEff);
            placedEff.Layer = this;

            if (crossingEff != null)
            {
                if (placedEff.TestX <= crossingEff.TestX)
                {
                    double move = placedEff.TestRight - crossingEff.TestX;
                    PushAllOnRightSide(placedEff, move);
                }
                else if (placedEff.TestX > crossingEff.TestX)
                {
                    double source = placedEff.TestX;
                    double target = source + crossingEff.TestRight - placedEff.TestX;

                    //await AnimationStartAsync(placedUI.RenderTransform, "TranslateX", 200, source, target);
                    await TryPlaceEffect(placedEff);
                }
            }
        }
        public void DeleteEffectLine(TimelineEffect eff)
        {
            TimelineEffect next = GetTheNext(eff);

            if (next == null)
                next = GetThePrevious(eff);

            if (next != null)
                AuraLayerManager.Self.CheckedEffect = next;
            else
                AuraLayerManager.Self.CheckedEffect = null;

            //UI_Track.RemoveEffectline(el);
            TimelineEffects.Remove(eff);
        }
        public TimelineEffect WhichIsOn(double x)
        {
            foreach (TimelineEffect e in TimelineEffects)
            {
                double left = e.TestX;
                double width = e.TestW;

                if ((left <= x) && (x <= left + width))
                    return e;
            }
            return null;
        }
        public TimelineEffect GetFirstOnRightSide(double x)
        {
            TimelineEffect result = null;

            foreach (TimelineEffect e in TimelineEffects)
            {
                if (e.TestX >= x)
                {
                    if (result == null)
                        result = e;

                    if (e.TestX < result.TestX)
                    {
                        result = e;
                    }
                }
            }
            return result;
        }
        public double GetFirstRoomPosition(double needRoomLength)
        {
            double roomX = 0;

            for (int i = 0; i < TimelineEffects.Count; i++)
            {
                TimelineEffect eff = TimelineEffects[i];

                if (roomX <= eff.TestX && eff.TestX < roomX + needRoomLength)
                {
                    roomX = eff.TestRight;
                    i = -1; // rescan every effect line
                }
            }

            return roomX;
        }

        private TimelineEffect GetTheNext(TimelineEffect eff)
        {
            TimelineEffect find = GetFirstOnRightSide(eff.TestRight);

            if (find == null)
                return null;
            else
                return find;
        }
        private TimelineEffect GetThePrevious(TimelineEffect eff)
        {
            double rightmostPosition = 0;
            TimelineEffect previousEffect = null;

            foreach (var e in TimelineEffects)
            {
                if (e.Equals(eff))
                    continue;

                double end = e.EndTime;

                if (end > rightmostPosition)
                {
                    rightmostPosition = end;
                    previousEffect = e;
                }
            }

            if (previousEffect != null)
                return previousEffect;
            else
                return null;
        }
        private void PushAllOnRightSide(TimelineEffect effect, double move)
        {
            foreach (TimelineEffect e in TimelineEffects)
            {
                if (effect.Equals(e))
                    continue;

                if (effect.TestX <= e.TestX)
                {
                    e.TestX += move;
                    //double source = e.UI.X;
                    //double target = source + move;

                    //AnimationStart(e.UI.RenderTransform, "TranslateX", 200, source, target);
                }
            }
        }
        private TimelineEffect GetFirstCrossingEffect(TimelineEffect testEffect)
        {
            TimelineEffect result = null;

            foreach (TimelineEffect e in TimelineEffects)
            {
                if (e.Equals(testEffect))
                    continue;

                if (IsCrossing(testEffect, e))
                {
                    if (result == null)
                        result = e;
                    else if (e.TestX < result.TestX)
                    {
                        result = e;
                    }
                }
            }

            return result;
        }
        static private bool IsCrossing(TimelineEffect effect1, TimelineEffect effect2)
        {
            return ControlHelper.IsCrossing(
                effect1.TestX, effect1.TestW,
                effect2.TestX, effect2.TestW);
        }
        #endregion

        public void AddTriggerEffect(TriggerEffect effect)
        {
            effect.Layer = this;
            TriggerEffects.Add(effect);
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
