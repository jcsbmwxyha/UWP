using AuraEditor.Common;
using AuraEditor.Models;
using AuraEditor.Pages;
using AuraEditor.UserControls;
using AuraEditor.ViewModels;
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
using static AuraEditor.UserControls.EffectLine;
using Windows.ApplicationModel.Resources;
using System.Linq;

namespace AuraEditor.Models
{
    public class LayerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ObservableCollection<EffectLineViewModel> EffectLineViewModels;
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
        public string ScriptName;

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

        public string TriggerAction;
        public double RemainingSpaceOnLast
        {
            get
            {
                var eff = GetRightmostEffect();
                return (eff != null) ? LayerPage.MaxRightPixel - eff.Right : LayerPage.MaxRightPixel;
            }
        }

        public int copy_count = 0;
        public string nameOfOriginalLayer = "";
        public LayerModel originalLayer;

        public LayerModel(string name = "")
        {
            EffectLineViewModels = new ObservableCollection<EffectLineViewModel>();
            EffectLineViewModels.CollectionChanged += TimelineEffectsChanged;
            TriggerEffects = new List<TriggerEffect>();

            Name = name;
            Eye = true;

            m_ZoneDictionary = new Dictionary<int, int[]>();
            TriggerAction = "Click";
        }
        public LayerModel(LayerModel layerModel)
        {
            if (layerModel.nameOfOriginalLayer != "") //be copyed layer then copy again
            {
                originalLayer = layerModel.originalLayer; //set original layer
                nameOfOriginalLayer = layerModel.nameOfOriginalLayer;
            }
            else
            {
                originalLayer = layerModel; //set original layer
                nameOfOriginalLayer = layerModel.Name;
            }
            originalLayer.copy_count++;

            if (originalLayer.copy_count == 1)
                Name = nameOfOriginalLayer + "_copy";
            else
                Name = nameOfOriginalLayer + "_copy" + originalLayer.copy_count;

            Eye = layerModel.Eye;
            isTriggering = layerModel.isTriggering;
            m_ZoneDictionary = new Dictionary<int, int[]>(layerModel.m_ZoneDictionary);
            TriggerAction = layerModel.TriggerAction;

            EffectLineViewModels = new ObservableCollection<EffectLineViewModel>();
            EffectLineViewModels.CollectionChanged += TimelineEffectsChanged;
            TriggerEffects = new List<TriggerEffect>();

            foreach (TriggerEffect each_TriggerEffect in layerModel.TriggerEffects)
            {
                TriggerEffects.Add(TriggerEffect.Clone(each_TriggerEffect));
            }

            foreach (EffectLineViewModel eff in layerModel.EffectLineViewModels)
            {
                AddTimelineEffect(new EffectLineViewModel(eff));
            }
        }
        static public LayerModel Clone(LayerModel vm)
        {
            return new LayerModel(vm);
        }
        private void TimelineEffectsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            EffectLineViewModel model;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    model = e.OldItems[0] as EffectLineViewModel;
                    ReUndoManager.Store(new RemoveEffectCommand(model));
                    break;
                case NotifyCollectionChangedAction.Add:
                    model = e.NewItems[0] as EffectLineViewModel;
                    ReUndoManager.Store(new AddEffectCommand(model));
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
        public int[] GetDeviceZones(int type)
        {
            if (m_ZoneDictionary.ContainsKey(type))
                return m_ZoneDictionary[type];

            return null;
        }
        public void SetDeviceZones(int type, int[] indexes)
        {
            m_ZoneDictionary.Remove(type);
            m_ZoneDictionary.Add(type, indexes);
        }
        #endregion

        #region -- Track behavior --
        public void AddTimelineEffect(EffectLineViewModel eff)
        {
            eff.Layer = this;
            EffectLineViewModels.Add(eff);
        }
        public bool TryInsertToTimelineFitly(EffectLineViewModel eff)
        {
            eff.Layer = this;

            if (ExceedIfApplyingEff(eff))
                return false;

            EffectLineViewModels.Add(eff);
            double moveTo = ApplyEffect(eff);

            // Because EffectLine create after MoveTo invoke,
            // assign again to make sure position is correct.
            eff.Left = moveTo;

            return true;
        }
        public bool ExceedIfApplyingEff(EffectLineViewModel eff)
        {
            EffectLineViewModel first = GetFirstIntersectingEffect(eff);

            if (first != null)
            {
                double needSpace;

                if (eff.Left <= first.Left)
                {
                    needSpace = eff.Right - first.Left;
                }
                else
                {
                    eff.Left = first.Right;
                    EffectLineViewModel second = GetFirstIntersectingEffect(eff);

                    if (second == null)
                    {
                        return eff.Right > LayerPage.MaxRightPixel;
                    }
                    else
                    {
                        needSpace = eff.Right - second.Left;
                    }
                }

                return (RemainingSpaceOnLast < needSpace);
            }

            return (LayerPage.MaxRightPixel < eff.Right);
        }
        public double ApplyEffect(EffectLineViewModel placedEff)
        {
            EffectLineViewModel firstIntersecting = GetFirstIntersectingEffect(placedEff);

            if (firstIntersecting != null)
            {
                if (placedEff.Left <= firstIntersecting.Left)
                {
                    double move = placedEff.Right - firstIntersecting.Left;
                    PushAllBehindEffect(placedEff, move);

                    return placedEff.Left;
                }
                else
                {
                    double target = firstIntersecting.Right;
                    placedEff.MovePositionWithAnimation(target);

                    EffectLineViewModel nextEff = GetTheNext(placedEff);
                    if (nextEff != null)
                    {
                        if (ControlHelper.IsPiling(target, placedEff.Width, nextEff.Left, nextEff.Width))
                        {
                            double push = target + placedEff.Width - nextEff.Left;
                            PushAllBehindEffect(placedEff, push);
                        }
                    }

                    return target;
                }
            }

            return placedEff.Left;
        }
        private void PushAllBehindEffect(EffectLineViewModel effect, double move)
        {
            foreach (EffectLineViewModel e in EffectLineViewModels)
            {
                if (effect.Equals(e))
                    continue;

                if (effect.Left <= e.Left)
                {
                    double target = e.Left + move;
                    e.MovePositionWithAnimation(target);
                    ReUndoManager.Store(new MoveEffectCommand(e, e.Left, target));
                }
            }
        }
        public void DeleteEffectLine(EffectLineViewModel eff)
        {
            EffectLineViewModel next = GetTheNext(eff);

            if (next == null)
                next = GetThePrevious(eff);

            if (next != null)
                LayerPage.Self.CheckedEffect = next;
            else
                LayerPage.Self.CheckedEffect = null;

            EffectLineViewModels.Remove(eff);
        }

        // Functional
        public EffectLineViewModel WhichIsOn(double x)
        {
            foreach (EffectLineViewModel e in EffectLineViewModels)
            {
                double left = e.Left;
                double width = e.Width;

                if ((left <= x) && (x <= left + width))
                    return e;
            }
            return null;
        }
        public double GetFirstRoomPosition(double needRoomOfDuration)
        {
            double roomOfDuration = 0;

            for (int i = 0; i < EffectLineViewModels.Count; i++)
            {
                EffectLineViewModel eff = EffectLineViewModels[i];

                if (roomOfDuration <= eff.StartTime && eff.StartTime < roomOfDuration + needRoomOfDuration)
                {
                    roomOfDuration = eff.EndTime;
                    i = -1; // rescan every effect line
                }
            }

            return roomOfDuration;
        }
        public List<double> GetAllEffHeadAndTailPositions(EffectLineViewModel ExceptionalEff)
        {
            List<double> result = new List<double>();
            foreach (var eff in EffectLineViewModels)
            {
                if (eff == ExceptionalEff)
                    continue;
                result.Add(eff.Left);
                result.Add(eff.Right);
            }

            return result;
        }
        public EffectLineViewModel GetFirstBehindPosition(double x)
        {
            EffectLineViewModel result = null;

            foreach (EffectLineViewModel e in EffectLineViewModels)
            {
                if (e.Left >= x)
                {
                    if (result == null)
                        result = e;

                    if (e.Left < result.Left)
                        result = e;
                }
            }
            return result;
        }
        public EffectLineViewModel GetRightmostEffect()
        {
            EffectLineViewModel rightmost = null;
            double rightmostPosition = 0;

            foreach (EffectLineViewModel eff in EffectLineViewModels)
            {
                if (eff.Right > rightmostPosition)
                {
                    rightmost = eff;
                    rightmostPosition = eff.Right;
                }
            }

            return rightmost;
        }
        private EffectLineViewModel GetTheNext(EffectLineViewModel eff)
        {
            EffectLineViewModel find = GetFirstBehindPosition(eff.Left + 1);

            if (find == null)
                return null;
            else
                return find;
        }
        private EffectLineViewModel GetThePrevious(EffectLineViewModel eff)
        {
            double rightmostPosition = 0;
            EffectLineViewModel previousEffect = null;

            foreach (var e in EffectLineViewModels)
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
        public EffectLineViewModel GetFirstIntersectingEffect(EffectLineViewModel eff)
        {
            EffectLineViewModel result = null;

            foreach (EffectLineViewModel e in EffectLineViewModels)
            {
                if (e.Equals(eff))
                    continue;

                if (IsIntersecting(eff, e))
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
        private bool IsIntersecting(EffectLineViewModel effect1, EffectLineViewModel effect2)
        {
            return ControlHelper.IsPiling(
                effect1.Left, effect1.Width,
                effect2.Left, effect2.Width);
        }
        private EffectLineViewModel GetFirstIntersectingEffect(double left, double width)
        {
            EffectLineViewModel result = null;

            foreach (var e in EffectLineViewModels)
            {
                if (ControlHelper.IsPiling(left, width, e.Left, e.Width))
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

        public void UpdateTimelineProportion()
        {
            foreach (var eff in EffectLineViewModels)
                eff.UpdateTimelineProportion();
        }
        #endregion

        #region -- Trigger Effect --
        public void AddTriggerEffect(TriggerEffect effect)
        {
            effect.Layer = this;
            TriggerEffects.Add(effect);
        }
        public void ComputeTriggerEffStartAndDuration()
        {
            double delay_accu = 0;
            double duration = 0;
            int width = SpacePage.Self.OperatingGridWidth;
            int height = SpacePage.Self.OperatingGridHeight;
            double angle = 0;
            double length = MaxOperatingLength(width, height, angle);
            double ledSpeed = 0;

            foreach (var eff in TriggerEffects)
            {
                if (GetEffEngNameByIdx(eff.Type) == "Reactive") // No speed problem, give 1 second
                    duration = 1000;
                else
                {
                    ledSpeed = EffectInfoModel.GetLedSpeed(eff.Type, eff.Info.Speed);
                    duration = (length / ledSpeed) * 1000 + 100; // buffer : 100ms
                }

                eff.StartTime = delay_accu;
                eff.DurationTime = duration;
                delay_accu += duration;
            }
        }
        #endregion

        public void ClearAllEffect()
        {
            ReUndoManager.Store(new RemoveAllEffectCommand(this));
            EffectLineViewModels.Clear();
            TriggerEffects.Clear();
            IsTriggering = false;
            LayerPage.Self.CheckedLayer = this;
        }

        public XmlNode ToXmlNodeForScript()
        {
            List<DeviceModel> deviceModels = SpacePage.Self.DeviceModelCollection;
            XmlNode groupoNode = CreateXmlNode("group");
            XmlAttribute attribute = CreateXmlAttributeOfFile("key");
            attribute.Value = ScriptName;
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
            if (TriggerAction == "Click")
                triggerAttribute.Value = "OneClick";
            else if (TriggerAction == "Double_Click")
                triggerAttribute.Value = "DoubleClick";
            else
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
            foreach (var vm in EffectLineViewModels)
                effects.Add(vm.Model);
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
            attribute.Value = device.ModelName;
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

        public class AddEffectCommand : IReUndoCommand
        {
            EffectLineViewModel _elvm;

            public AddEffectCommand(EffectLineViewModel elvm)
            {
                _elvm = elvm;
            }

            public void ExecuteRedo()
            {
                var layer = _elvm.Layer;
                layer.TryInsertToTimelineFitly(_elvm);
            }
            public void ExecuteUndo()
            {
                var layer = _elvm.Layer;
                layer.DeleteEffectLine(_elvm);
            }
        }
        public class RemoveEffectCommand : IReUndoCommand
        {
            EffectLineViewModel _elvm;

            public RemoveEffectCommand(EffectLineViewModel elvm)
            {
                _elvm = elvm;
            }

            public void ExecuteRedo()
            {
                var layer = _elvm.Layer;
                layer.DeleteEffectLine(_elvm);
            }
            public void ExecuteUndo()
            {
                var layer = _elvm.Layer;
                layer.AddTimelineEffect(_elvm);
            }
        }
        public class RemoveAllEffectCommand : IReUndoCommand
        {
            LayerModel _layerModel;
            List<EffectLineViewModel> _elvms;
            List<TriggerEffect> _triggerEffs;

            public RemoveAllEffectCommand(LayerModel layermodel)
            {
                _layerModel = layermodel;
                _elvms = layermodel.EffectLineViewModels.ToList();
                _triggerEffs = new List<TriggerEffect>(layermodel.TriggerEffects);
            }

            public void ExecuteRedo()
            {
                _layerModel.EffectLineViewModels.Clear();
                _layerModel.TriggerEffects.Clear();
                _layerModel.IsTriggering = false;
            }
            public void ExecuteUndo()
            {
                foreach (var elvm in _elvms)
                    _layerModel.AddTimelineEffect(elvm);

                foreach (var tEff in _triggerEffs)
                    _layerModel.TriggerEffects.Add(tEff);

                _layerModel.IsTriggering = _triggerEffs.Count > 0 ? true : false;
            }
        }
    }
}
