using AuraEditor.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using Windows.UI.Xaml.Media.Imaging;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.XmlHelper;

namespace AuraEditor.Models
{
    public enum DeviceStatus
    {
        OnStage = 0,
        Temp,
    }

    public class DeviceModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region -- Property--
        private double _pixelLeft;
        public double PixelLeft
        {
            get
            {
                return _pixelLeft;
            }
            set
            {
                _pixelLeft = value;
                SpacePage.Self.OnDeviceMoved(this);
                RaisePropertyChanged("PixelLeft");
            }
        }

        private double _pixelTop;
        public double PixelTop
        {
            get
            {
                return _pixelTop;
            }
            set
            {
                _pixelTop = value;
                SpacePage.Self.OnDeviceMoved(this);
                RaisePropertyChanged("PixelTop");
            }
        }

        private double _pixelWidth;
        public double PixelWidth
        {
            get
            {
                return _pixelWidth;
            }
            set
            {
                _pixelWidth = value;
                RaisePropertyChanged("PixelWidth");
            }
        }

        private double _pixelHeight;
        public double PixelHeight
        {
            get
            {
                return _pixelHeight;
            }
            set
            {
                _pixelHeight = value;
                RaisePropertyChanged("PixelHeight");
            }
        }

        public double PixelRight { get { return PixelLeft + PixelWidth; } }
        public double PixelBottom { get { return PixelTop + PixelHeight; } }

        private bool _operationenabled;
        public bool OperationEnabled
        {
            get
            {
                return _operationenabled;
            }
            set
            {
                _operationenabled = value;
                RaisePropertyChanged("OperationEnabled");
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
                _visualstate = value;
                RaisePropertyChanged("VisualState");
            }
        }

        public BitmapImage Image;

        private ObservableCollection<ZoneModel> _zones;
        public ObservableCollection<ZoneModel> Zones
        {
            get
            {
                return _zones;
            }
            set
            {
                _zones = value;
                RaisePropertyChanged("Zones");
            }
        }

        private ObservableCollection<SpecialZoneModel> _specialzones;
        public ObservableCollection<SpecialZoneModel> SpecialZones
        {
            get
            {
                return _specialzones;
            }
            set
            {
                _specialzones = value;
                RaisePropertyChanged("Zones");
            }
        }

        public List<ZoneModel> AllZones
        {
            get
            {
                List<ZoneModel> list = Zones.ToList();
                list.AddRange(SpecialZones.ToList());

                return list;
            }
        }

        public string Name { get; set; }
        public int Type { get; set; }
        public DeviceStatus Status { get; set; }
        #endregion

        public DeviceModel()
        {
        }

        public XmlNode ToXmlNodeForUserData()
        {
            XmlNode deviceNode = CreateXmlNode("device");

            XmlAttribute attributeName = CreateXmlAttributeOfFile("name");
            attributeName.Value = Name;
            deviceNode.Attributes.Append(attributeName);

            XmlAttribute attributeType = CreateXmlAttributeOfFile("type");
            attributeType.Value = GetTypeNameByType(Type);
            deviceNode.Attributes.Append(attributeType);

            XmlNode xNode = CreateXmlNode("x");
            xNode.InnerText = (PixelLeft / GridPixels).ToString();
            deviceNode.AppendChild(xNode);

            XmlNode yNode = CreateXmlNode("y");
            yNode.InnerText = (PixelTop / GridPixels).ToString();
            deviceNode.AppendChild(yNode);

            return deviceNode;
        }
        public XmlNode ToXmlNodeForScript()
        {
            XmlNode deviceNode = CreateXmlNode("device");

            XmlNode modelNode = CreateXmlNode("model");
            modelNode.InnerText = Name.ToString();
            deviceNode.AppendChild(modelNode);

            string type = "";
            switch (Type)
            {
                case 0: type = "Notebook"; break;
                case 1: type = "Mouse"; break;
                case 2: type = "Keyboard"; break;
                case 3: type = "Headset"; break;
                case 5: type = "Desktop"; break;
                case 6: type = "MotherBoard"; break;
                case 7: type = "MousePad"; break;
            }
            XmlNode typeNode = CreateXmlNode("type");
            typeNode.InnerText = type.ToString();
            deviceNode.AppendChild(typeNode);

            XmlNode locationNode = GetLocationXmlNode();
            deviceNode.AppendChild(locationNode);

            return deviceNode;
        }
        private XmlNode GetLocationXmlNode()
        {
            XmlNode locationNode = CreateXmlNode("location");

            XmlNode xNode = CreateXmlNode("x");
            xNode.InnerText = (PixelLeft / GridPixels).ToString();
            locationNode.AppendChild(xNode);

            XmlNode yNode = CreateXmlNode("y");
            yNode.InnerText = (PixelTop / GridPixels).ToString();
            locationNode.AppendChild(yNode);

            return locationNode;
        }
    }
}
