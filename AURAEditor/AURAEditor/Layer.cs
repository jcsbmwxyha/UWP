using AuraEditor.Common;
using AuraEditor.Models;
using AuraEditor.Pages;
using AuraEditor.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        private bool eye;
        public bool Eye
        {
            get
            {
                return eye;
            }
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
            get
            {
                return isTriggering;
            }
            set
            {
                if (isTriggering != value)
                {
                    isTriggering = value;
                    RaisePropertyChanged("IsTriggering");
                }
            }
        }
        public LayerTrack UI_Track;
        public LayerBackground UI_Background;
        public string TriggerAction;

        private string _visualstate;
        public string VisualState
        {
            get
            {
                return _visualstate;
            }
            set
            {
                if (_visualstate != value)
                {
                    _visualstate = value;
                    RaisePropertyChanged("VisualState");
                }
            }
        }

        public Layer(string name = "")
        {
            TimelineEffects = new ObservableCollection<TimelineEffect>();
            TimelineEffects.CollectionChanged += TimelineEffectsChanged;
            TriggerEffects = new List<TriggerEffect>();

            Name = name;
            Eye = true;

            UI_Track = new LayerTrack
            {
                DataContext = this,
            };
            UI_Track.Height = 52;

            m_ZoneDictionary = new Dictionary<int, int[]>();
            TriggerAction = "One Click";
        }

        private void TimelineEffectsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TimelineEffect model;
            EffectLine view;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    model = e.OldItems[0] as TimelineEffect;
                    UI_Track.Track.Children.Remove(model.View);
                    break;
                case NotifyCollectionChangedAction.Add:
                    model = e.NewItems[0] as TimelineEffect;
                    view = new EffectLine();
                    view.DataContext = model;
                    view.Height = 36;
                    Windows.UI.Xaml.Controls.Canvas.SetTop(view, 8);
                    model.View = view;
                    UI_Track.Track.Children.Add(model.View);
                    break;
            }
        }

        #region -- Zones --
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
        #endregion

        #region Track behavior
        public void AddTimelineEffect(TimelineEffect eff)
        {
            eff.Layer = this;

            TimelineEffects.Add(eff);
        }
        public double InsertTimelineEffectFitly(TimelineEffect eff)
        {
            eff.Layer = this;
            TimelineEffects.Add(eff);
            double result = MoveToFitPosition(eff);

            return result;
        }
        public void AppendTimelineEffect(TimelineEffect eff)
        {
            TimelineEffect rightmost = GetRightmostEffect();

            if (rightmost == null)
            {
                eff.StartTime = 0;
            }
            else
            {
                eff.StartTime = rightmost.EndTime;
            }

            TimelineEffects.Add(eff);
        }

        public double MoveToFitPosition(TimelineEffect placedEff)
        {
            placedEff.Layer = this;
            TimelineEffect pilingEff = GetFirstPilingEffect(placedEff);

            if (pilingEff != null)
            {
                if (placedEff.Left <= pilingEff.Left)
                {
                    double move = placedEff.Right - pilingEff.Left;
                    PushAllOnRightSide(placedEff, move);

                    return placedEff.Left;
                }
                else
                {
                    double target = pilingEff.Right;
                    placedEff.MoveTo(target);

                    TimelineEffect nextEff = GetTheNext(placedEff);
                    if (nextEff != null)
                    {
                        if (ControlHelper.IsPiling(target, placedEff.Width,
                                                   nextEff.Left, nextEff.Width))
                        {
                            double move = target + placedEff.Width - nextEff.Left;
                            PushAllOnRightSide(placedEff, move);
                        }
                    }

                    return target;
                }
            }

            return placedEff.Left;
        }
        public void DeleteEffectLine(TimelineEffect eff)
        {
            TimelineEffect next = GetTheNext(eff);

            if (next == null)
                next = GetThePrevious(eff);

            if (next != null)
                LayerPage.Self.CheckedEffect = next;
            else
                LayerPage.Self.CheckedEffect = null;

            TimelineEffects.Remove(eff);
        }
        public TimelineEffect WhichIsOn(double x)
        {
            foreach (TimelineEffect e in TimelineEffects)
            {
                double left = e.Left;
                double width = e.Width;

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
                if (e.Left >= x)
                {
                    if (result == null)
                        result = e;

                    if (e.Left < result.Left)
                    {
                        result = e;
                    }
                }
            }
            return result;
        }
        public double GetFirstRoomPosition(double needRoomOfDuration)
        {
            double roomOfDuration = 0;

            for (int i = 0; i < TimelineEffects.Count; i++)
            {
                TimelineEffect eff = TimelineEffects[i];

                if (roomOfDuration <= eff.StartTime && eff.StartTime < roomOfDuration + needRoomOfDuration)
                {
                    roomOfDuration = eff.EndTime;
                    i = -1; // rescan every effect line
                }
            }

            return roomOfDuration;
        }
        public List<double> GetAllEffHeadAndTailPositions(TimelineEffect ExceptionalEff)
        {
            List<double> result = new List<double>();
            foreach (var eff in TimelineEffects)
            {
                if (eff == ExceptionalEff)
                    continue;
                result.Add(eff.Left);
                result.Add(eff.Right);
            }

            return result;
        }

        private TimelineEffect GetRightmostEffect()
        {
            TimelineEffect rightmost = null;
            double rightmostPosition = 0;

            foreach (TimelineEffect eff in TimelineEffects)
            {
                if (eff.Right > rightmostPosition)
                {
                    rightmost = eff;
                    rightmostPosition = eff.Right;
                }
            }

            return rightmost;
        }
        private TimelineEffect GetTheNext(TimelineEffect eff)
        {
            TimelineEffect find = GetFirstOnRightSide(eff.Left + 1);

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

                if (effect.Left <= e.Left)
                {
                    double target = e.Left + move;
                    e.MoveTo(target);
                }
            }
        }
        private TimelineEffect GetFirstPilingEffect(TimelineEffect testEffect)
        {
            TimelineEffect result = null;

            foreach (TimelineEffect e in TimelineEffects)
            {
                if (e.Equals(testEffect))
                    continue;

                if (IsPiling(testEffect, e))
                {
                    if (result == null)
                        result = e;
                    else if (e.Left < result.Left)
                    {
                        result = e;
                    }
                }
            }

            return result;
        }
        private List<TimelineEffect> GetAllPilingEffect(double left, double right)
        {
            List<TimelineEffect> result = new List<TimelineEffect>();

            foreach (TimelineEffect eff in TimelineEffects)
            {
                if (ControlHelper.IsPiling(left, right, eff.Left, eff.Width))
                    result.Add(eff);
            }

            return result;
        }
        static private bool IsPiling(TimelineEffect effect1, TimelineEffect effect2)
        {
            return ControlHelper.IsPiling(
                effect1.Left, effect1.Width,
                effect2.Left, effect2.Width);
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
            int width = SpacePage.Self.GetCurrentOperatingGridWidth;
            int height = SpacePage.Self.GetCurrentOperatingGridHeight;
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
            List<DeviceModel> deviceModels = SpacePage.Self.DeviceModelCollection;
            XmlNode groupoNode = CreateXmlNode("group");
            XmlAttribute attribute = CreateXmlAttributeOfFile("key");
            attribute.Value = Name;
            groupoNode.Attributes.Append(attribute);

            foreach (var dm in deviceModels)
            {
                XmlNode deviceNode = dm.ToXmlNodeForScript();
                XmlNode usageNode = GetUsageXmlNode(dm.Type);

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
            List<DeviceModel> deviceModels = SpacePage.Self.DeviceModelCollection;
            foreach (var d in deviceModels)
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
        private XmlNode GetDeviceUsageXmlNode(DeviceModel device)
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
