using AuraEditor.Common;
using AuraEditor.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using Windows.UI;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.Math2;
using static AuraEditor.Common.XmlHelper;

namespace AuraEditor.Models
{
    public class EffectInfoModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        public string Name
        {
            get
            {
                return GetLanguageNameByIdx(Type);
            }
        }

        private int _type;
        public int Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                RaisePropertyChanged("Name");
            }
        }

        public bool ColorGroupVisible;
        public bool DoubleColorGroupVisible;

        private bool _patternGroupVisible;
        public bool PatternGroupVisible
        {
            get
            {
                return _patternGroupVisible;
            }
            set
            {
                if (value != _patternGroupVisible)
                {
                    _patternGroupVisible = value;
                    RaisePropertyChanged("PatternGroupVisible");
                }
            }
        }

        public bool BrightnessGroupVisible;
        public bool SpeedGroupVisible;
        public bool AngleGroupVisible;
        public bool RainbowRotationVisible;
        public bool TemperatureGroupVisible;
        public bool RandomVisible;

        private Color _initColor;
        public Color InitColor
        {
            get
            {
                return _initColor;
            }
            set
            {
                if (value == _initColor)
                    return;

                _initColor = value;
                RaisePropertyChanged("InitColor");
            }
        }

        private Color _doubleColor1;
        public Color DoubleColor1
        {
            get
            {
                return _doubleColor1;
            }
            set
            {
                if (value == _doubleColor1)
                    return;

                _doubleColor1 = value;
                RaisePropertyChanged("DoubleColor1");
            }
        }

        private Color _doubleColor2;
        public Color DoubleColor2
        {
            get
            {
                return _doubleColor2;
            }
            set
            {
                if (value == _doubleColor2)
                    return;

                _doubleColor2 = value;
                RaisePropertyChanged("DoubleColor2");
            }
        }

        private int _brightness;
        public int Brightness
        {
            get
            {
                return _brightness;
            }
            set
            {
                _brightness = value;
                RaisePropertyChanged("Brightness");
            }
        }

        private int _speed;
        public int Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                _speed = value;
                RaisePropertyChanged("Speed");
            }
        }

        private double _randomRangeMax;
        public double RandomRangeMax
        {
            get
            {
                return _randomRangeMax;
            }
            set
            {
                _randomRangeMax = value;
                RaisePropertyChanged("RandomRangeMax");
            }
        }

        private double _randomRangeMin;
        public double RandomRangeMin
        {
            get
            {
                return _randomRangeMin;
            }
            set
            {
                _randomRangeMin = value;
                RaisePropertyChanged("RandomRangeMin");
            }
        }

        private double _angle;
        public double Angle
        {
            get
            {
                return _angle;
            }
            set
            {
                if (_angle != value)
                {
                    _angle = value;
                    RaisePropertyChanged("Angle");
                }
            }
        }

        private int _high;
        public int High
        {
            get
            {
                return _high;
            }
            set
            {
                _high = value;
                RaisePropertyChanged("High");
            }
        }

        private int _low;
        public int Low
        {
            get
            {
                return _low;
            }
            set
            {
                _low = value;
                RaisePropertyChanged("Low");
            }
        }

        //ColorModeSelection : 1 = single color, 2 = random color, 3 = pattern color, 4 = double color
        private int _colorModeSelection; // For Trigger Dialog Ripple
        public int ColorModeSelection
        {
            get
            {
                return _colorModeSelection;
            }
            set
            {
                if (value == 0)
                    return;

                _colorModeSelection = value;
                RaisePropertyChanged("ColorModeSelection");
            }
        }

        public List<ColorPointModel> CustomizedPattern;
        public int PatternSelect;

        private bool _colorSegmentation;
        public bool ColorSegmentation
        {
            get
            {
                return _colorSegmentation;
            }
            set
            {
                _colorSegmentation = value;
                RaisePropertyChanged("ColorSegmentation");
            }
        }

        private bool _rainbowSpecialEffects;
        public bool RainbowSpecialEffects
        {
            get
            {
                return _rainbowSpecialEffects;
            }
            set
            {
                _rainbowSpecialEffects = value;
                RaisePropertyChanged("RainbowSpecialEffects");
            }
        }
        //RainbowSpecialMode : 1 = clockwise, 2 = countclockwise, 3 = outward, 4 = inward
        public int RainbowSpecialMode;

        public EffectInfoModel(EffectInfoModel eim)
        {
            Type = eim.Type;
            SetGroupVisibility(Type);
            InitColor = eim.InitColor;
            DoubleColor1 = eim.DoubleColor1;
            DoubleColor2 = eim.DoubleColor2;
            Brightness = eim.Brightness;
            Speed = eim.Speed;
            ColorModeSelection = eim.ColorModeSelection;
            Angle = eim.Angle;
            RandomRangeMax = eim.RandomRangeMax;
            RandomRangeMin = eim.RandomRangeMin;
            High = eim.High;
            Low = eim.Low;
            CustomizedPattern = new List<ColorPointModel>(eim.CustomizedPattern); // TODO
            ColorSegmentation = eim.ColorSegmentation;
            RainbowSpecialEffects = eim.RainbowSpecialEffects;
            RainbowSpecialMode = eim.RainbowSpecialMode;
            PatternSelect = eim.PatternSelect;
        }
        public EffectInfoModel(int type)
        {
            Type = type;
            SetGroupVisibility(type);
            InitColor = Colors.Red;
            DoubleColor1 = Colors.Red;
            DoubleColor2 = Colors.Blue;
            Brightness = 3;
            Speed = 1;
            ColorModeSelection = 1;
            Angle = 90;
            RandomRangeMax = 12;
            RandomRangeMin = 0;
            High = 60;
            Low = 30;
            CustomizedPattern = new List<ColorPointModel>(DefaultColorPointListCollection[DefaultColorPointListCollection.Count - 1]); // TODO
            ColorSegmentation = true;
            RainbowSpecialEffects = false;
            RainbowSpecialMode = 1;
            PatternSelect = DefaultColorPointListCollection.Count - 1;
        }

        private void SetGroupVisibility(int type)
        {
            switch (GetEffEngNameByIdx(type))
            {
                case "Static":
                    ColorGroupVisible = true;
                    DoubleColorGroupVisible = false;
                    RandomVisible = false;
                    PatternGroupVisible = false;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = false;
                    AngleGroupVisible = false;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = false;
                    break;
                case "Breathing":
                    ColorGroupVisible = true;
                    DoubleColorGroupVisible = true;
                    RandomVisible = true;
                    PatternGroupVisible = false;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = true;
                    AngleGroupVisible = false;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = false;
                    break;
                case "Color cycle":
                    ColorGroupVisible = false;
                    DoubleColorGroupVisible = false;
                    RandomVisible = false;
                    PatternGroupVisible = false;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = true;
                    AngleGroupVisible = false;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = false;
                    break;
                case "Rainbow":
                    ColorGroupVisible = false;
                    DoubleColorGroupVisible = false;
                    RandomVisible = false;
                    PatternGroupVisible = true;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = true;
                    AngleGroupVisible = true;
                    RainbowRotationVisible = true;
                    TemperatureGroupVisible = false;
                    break;
                case "Strobing":
                    ColorGroupVisible = true;
                    DoubleColorGroupVisible = true;
                    RandomVisible = true;
                    PatternGroupVisible = false;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = true;
                    AngleGroupVisible = false;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = false;
                    break;
                case "Comet":
                    ColorGroupVisible = true;
                    DoubleColorGroupVisible = false;
                    RandomVisible = true;
                    PatternGroupVisible = false;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = true;
                    AngleGroupVisible = true;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = false;
                    break;
                case "Star":
                    ColorGroupVisible = true;
                    DoubleColorGroupVisible = false;
                    RandomVisible = true;
                    PatternGroupVisible = false;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = true;
                    AngleGroupVisible = false;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = false;
                    break;
                case "Tide":
                    ColorGroupVisible = true;
                    DoubleColorGroupVisible = false;
                    RandomVisible = false;
                    PatternGroupVisible = false;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = true;
                    AngleGroupVisible = true;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = false;
                    break;
                case "Raidus":
                    ColorGroupVisible = false;
                    DoubleColorGroupVisible = false;
                    RandomVisible = false;
                    PatternGroupVisible = false;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = false;
                    AngleGroupVisible = false;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = false;
                    break;
                case "Reactive":
                    ColorGroupVisible = true;
                    DoubleColorGroupVisible = false;
                    RandomVisible = true;
                    PatternGroupVisible = false;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = true;
                    AngleGroupVisible = false;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = false;
                    break;
                case "Laser":
                    ColorGroupVisible = true;
                    DoubleColorGroupVisible = false;
                    RandomVisible = true;
                    PatternGroupVisible = false;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = true;
                    AngleGroupVisible = false;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = false;
                    break;
                case "Ripple":
                    ColorGroupVisible = true;
                    DoubleColorGroupVisible = false;
                    RandomVisible = true;
                    PatternGroupVisible = true;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = true;
                    AngleGroupVisible = false;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = false;
                    break;
                case "Music":
                    ColorGroupVisible = false;
                    DoubleColorGroupVisible = false;
                    RandomVisible = false;
                    PatternGroupVisible = false;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = false;
                    AngleGroupVisible = false;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = false;
                    break;
                case "Smart":
                    ColorGroupVisible = false;
                    DoubleColorGroupVisible = false;
                    RandomVisible = false;
                    PatternGroupVisible = false;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = false;
                    AngleGroupVisible = false;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = true;
                    break;
                default:
                    ColorGroupVisible = false;
                    DoubleColorGroupVisible = false;
                    RandomVisible = false;
                    PatternGroupVisible = false;
                    BrightnessGroupVisible = false;
                    SpeedGroupVisible = false;
                    AngleGroupVisible = false;
                    RainbowRotationVisible = false;
                    TemperatureGroupVisible = false;
                    break;
            }
        }

        public void ChangeType(int type)
        {
            Type = type;
            SetGroupVisibility(type);
            InitColor = Colors.Red;
            DoubleColor1 = Colors.Red;
            DoubleColor2 = Colors.Blue;
            Brightness = 3;
            Speed = 1;
            ColorModeSelection = 1;
            Angle = 90;
            RandomRangeMax = 12;
            RandomRangeMin = 0;
            High = 60;
            Low = 30;
            CustomizedPattern = new List<ColorPointModel>(DefaultColorPointListCollection[DefaultColorPointListCollection.Count - 1]); // TODO
            ColorSegmentation = true;
            RainbowSpecialEffects = false;
            RainbowSpecialMode = 1;
            PatternSelect = DefaultColorPointListCollection.Count - 1;
        }

        #region Create script for lighting service
        static public double GetLedSpeed(int effType, int speed)
        {
            if (speed == 0)
            {
                if (GetEffEngNameByIdx(effType) == "Comet") return 10;
                else if (GetEffEngNameByIdx(effType) == "Laser") return 10;
                else if (GetEffEngNameByIdx(effType) == "Ripple") return 10;
            }
            else if (speed == 1)
            {
                if (GetEffEngNameByIdx(effType) == "Comet") return 15;
                else if (GetEffEngNameByIdx(effType) == "Laser") return 15;
                else if (GetEffEngNameByIdx(effType) == "Ripple") return 15;
            }
            else if (speed == 2)
            {
                if (GetEffEngNameByIdx(effType) == "Comet") return 20;
                else if (GetEffEngNameByIdx(effType) == "Laser") return 20;
                else if (GetEffEngNameByIdx(effType) == "Ripple") return 20;
            }

            return 0;
        }

        public XmlNode ToXmlNodeForScript(int zoneCount)
        {
            XmlNode effectNode = CreateXmlNode("effect");

            effectNode.AppendChild(GetViewportTransformXmlNode());
            effectNode.AppendChild(GetWaveListXmlNode(zoneCount));
            effectNode.AppendChild(GetInitColorXmlNode(InitColor));

            return effectNode;
        }

        private XmlNode GetViewportTransformXmlNode()
        {
            XmlNode viewportTransformNode = CreateXmlNode("viewportTransform");
            double[] hsl = AuraEditorColorHelper.RgbTOHsl(InitColor);

            XmlNode rotateNode = CreateXmlNode("rotate");

            XmlNode xNode = CreateXmlNode("x");
            xNode.InnerText = "0";
            rotateNode.AppendChild(xNode);

            XmlNode yNode = CreateXmlNode("y");
            yNode.InnerText = "0";
            rotateNode.AppendChild(yNode);

            XmlNode angleNode = CreateXmlNode("angle");
            angleNode.InnerText = Angle_CreatorToLService(Angle).ToString();
            rotateNode.AppendChild(angleNode);

            XmlNode methodNode = GetMethodXmlNode(Type);
            viewportTransformNode.AppendChild(rotateNode);
            viewportTransformNode.AppendChild(methodNode);

            return viewportTransformNode;
        }
        private XmlNode GetMethodXmlNode(int effType)
        {
            string methodString = "point";
            XmlNode methodNode = CreateXmlNode("method");


            if (GetEffEngNameByIdx(effType) == "Static" ||
                GetEffEngNameByIdx(effType) == "Breathing" ||
                GetEffEngNameByIdx(effType) == "Color cycle" ||
                GetEffEngNameByIdx(effType) == "Strobing")
            {
                methodString = "point";
            }
            else if (GetEffEngNameByIdx(effType) == "Comet" ||
                     GetEffEngNameByIdx(effType) == "Tide")
            {
                XmlNode inputNode = CreateXmlNode("input");
                inputNode.InnerText = "0";
                methodNode.AppendChild(inputNode);

                methodString = "OrthogonaProject";
            }
            else if (GetEffEngNameByIdx(effType) == "Rainbow")
            {
                if (RainbowSpecialEffects)
                {
                    XmlNode inputNode = CreateXmlNode("input");
                    inputNode.InnerText = "Centor_X";
                    methodNode.AppendChild(inputNode);

                    XmlNode inputNode2 = CreateXmlNode("input");
                    inputNode2.InnerText = "Centor_Y";
                    methodNode.AppendChild(inputNode2);

                    if (RainbowSpecialMode == 1 || RainbowSpecialMode == 2)
                        methodString = "radian";
                    else if (RainbowSpecialMode == 3 || RainbowSpecialMode == 4)
                        methodString = "radius";
                }
                else
                {
                    XmlNode inputNode = CreateXmlNode("input");
                    inputNode.InnerText = "0";
                    methodNode.AppendChild(inputNode);

                    methodString = "OrthogonaProject";
                }
            }
            else if (GetEffEngNameByIdx(effType) == "Reactive")
            {
                XmlNode inputNode = CreateXmlNode("input");
                inputNode.InnerText = "keyPressX";
                methodNode.AppendChild(inputNode);

                XmlNode inputNode2 = CreateXmlNode("input");
                inputNode2.InnerText = "keyPressY";
                methodNode.AppendChild(inputNode2);

                XmlNode inputNode3 = CreateXmlNode("input");
                inputNode3.InnerText = "0.5";
                methodNode.AppendChild(inputNode3);

                methodString = "limitRadius";
            }
            else if (GetEffEngNameByIdx(effType) == "Laser")
            {
                XmlNode inputNode = CreateXmlNode("input");
                inputNode.InnerText = "keyPressX";
                methodNode.AppendChild(inputNode);

                XmlNode inputNode2 = CreateXmlNode("input");
                inputNode2.InnerText = "keyPressY";
                methodNode.AppendChild(inputNode2);

                methodString = "distance";
            }
            else if (GetEffEngNameByIdx(effType) == "Ripple")
            {
                XmlNode inputNode = CreateXmlNode("input");
                inputNode.InnerText = "keyPressX";
                methodNode.AppendChild(inputNode);

                XmlNode inputNode2 = CreateXmlNode("input");
                inputNode2.InnerText = "keyPressY";
                methodNode.AppendChild(inputNode2);

                methodString = "radius";
            }
            else if (GetEffEngNameByIdx(effType) == "Star")
            {
                methodString = "shuffle";
            }

            XmlAttribute attribute = CreateXmlAttributeOfFile("key");
            attribute.Value = methodString;
            methodNode.Attributes.Append(attribute);

            return methodNode;
        }


        private XmlNode GetWaveListXmlNode(int zoneCount)
        {
            XmlNode waveListNode = CreateXmlNode("waveList");
            int maxOperatingGridWidth = SpacePage.Self.OperatingGridWidth;
            int maxOperatingGridHeight = SpacePage.Self.OperatingGridHeight;
            double temp = Angle_CreatorToLService(Angle);
            int maxOperatingGridLength = MaxOperatingLength(maxOperatingGridWidth, maxOperatingGridHeight, Angle_CreatorToLService(Angle));

            if (GetEffEngNameByIdx(Type) == "Static")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "ConstantWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "10"));
                waveNode.AppendChild(CreateXmlNodeByValue("freq", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);
            }
            else if (GetEffEngNameByIdx(Type) == "Breathing")
            {
                double[] hsl = AuraEditorColorHelper.RgbTOHsl(InitColor);
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "23"));
                if (Speed == 0)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                else if (Speed == 1)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                else // 2
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "0.4"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);

                // Breath bug workaround
                // Except double color
                if (ColorModeSelection != 4)
                {
                    XmlNode waveNode2 = CreateXmlNode("wave");
                    waveNode2.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("max", hsl[2].ToString()));
                    waveNode2.AppendChild(CreateXmlNodeByValue("min", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("length", "23"));
                    if (Speed == 0)
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                    else if (Speed == 1)
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                    else // 2
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.4"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode2.AppendChild(GetBindToSlotXmlNode(new List<string> { "LIGHTNESS" }));
                    waveListNode.AppendChild(waveNode2);
                }

                if (ColorModeSelection == 2)
                {
                    XmlNode waveNode3 = CreateXmlNode("wave");
                    waveNode3.AppendChild(CreateXmlNodeByValue("type", "RandomWave"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("max", ((double)RandomRangeMax / 12).ToString()));
                    waveNode3.AppendChild(CreateXmlNodeByValue("min", ((double)RandomRangeMin / 12).ToString()));
                    waveNode3.AppendChild(CreateXmlNodeByValue("length", "23"));
                    if (Speed == 0)
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                    else if (Speed == 1)
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                    else // 2
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.4"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode3.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                    waveListNode.AppendChild(waveNode3);
                }
                else if(ColorModeSelection == 4)
                {
                    XmlNode waveNode3 = CreateXmlNode("wave");
                    waveNode3.AppendChild(CreateXmlNodeByValue("type", "CustomStepWave"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("max", "1"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("min", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("length", "23"));
                    if (Speed == 0)
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                    else if (Speed == 1)
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                    else // 2
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.4"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode3.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                    waveNode3.AppendChild(GetCustomizedNodeFromDoubleColor(0));
                    waveListNode.AppendChild(waveNode3);

                    XmlNode waveNode4 = CreateXmlNode("wave");
                    waveNode4.AppendChild(CreateXmlNodeByValue("type", "CustomStepWave"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("max", "1"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("min", "0"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("length", "23"));
                    if (Speed == 0)
                        waveNode4.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                    else if (Speed == 1)
                        waveNode4.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                    else // 2
                        waveNode4.AppendChild(CreateXmlNodeByValue("freq", "0.4"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode4.AppendChild(GetBindToSlotXmlNode(new List<string> { "SATURATION" }));
                    waveNode4.AppendChild(GetCustomizedNodeFromDoubleColor(1));
                    waveListNode.AppendChild(waveNode4);

                    XmlNode waveNode5 = CreateXmlNode("wave");
                    waveNode5.AppendChild(CreateXmlNodeByValue("type", "CustomLinearWave"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("max", "1"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("min", "0"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("length", "23"));
                    if (Speed == 0)
                        waveNode5.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                    else if (Speed == 1)
                        waveNode5.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                    else // 2
                        waveNode5.AppendChild(CreateXmlNodeByValue("freq", "0.4"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode5.AppendChild(GetBindToSlotXmlNode(new List<string> { "LIGHTNESS" }));
                    waveNode5.AppendChild(GetCustomizedNodeFromDoubleColor(2));
                    waveListNode.AppendChild(waveNode5);
                }
            }
            else if (GetEffEngNameByIdx(Type) == "Color cycle")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "QuarterSineWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "23"));
                if (Speed == 0)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "-0.02"));
                else if (Speed == 1)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "-0.04"));
                else // 2
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "-0.08"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                waveListNode.AppendChild(waveNode);
            }
            else if (GetEffEngNameByIdx(Type) == "Rainbow")
            {
                // wave 1 for HUE
                XmlNode waveNode1 = CreateXmlNode("wave");
                if (ColorSegmentation == true)
                    waveNode1.AppendChild(CreateXmlNodeByValue("type", "CustomLinearWave"));
                else // false
                    waveNode1.AppendChild(CreateXmlNodeByValue("type", "CustomStepWave"));
                waveNode1.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode1.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode1.AppendChild(CreateXmlNodeByValue("length", maxOperatingGridLength.ToString()));
                if (Speed == 0)
                {
                    if (RainbowSpecialEffects && (RainbowSpecialMode == 2 || RainbowSpecialMode == 4))
                        waveNode1.AppendChild(CreateXmlNodeByValue("freq", "-0.05"));
                    else
                        waveNode1.AppendChild(CreateXmlNodeByValue("freq", "0.05"));
                }
                else if (Speed == 1)
                {
                    if (RainbowSpecialEffects && (RainbowSpecialMode == 2 || RainbowSpecialMode == 4))
                        waveNode1.AppendChild(CreateXmlNodeByValue("freq", "-0.1"));
                    else
                        waveNode1.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                }
                else// 2
                {
                    if (RainbowSpecialEffects && (RainbowSpecialMode == 2 || RainbowSpecialMode == 4))
                        waveNode1.AppendChild(CreateXmlNodeByValue("freq", "-0.2"));
                    else
                        waveNode1.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                }
                waveNode1.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode1.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode1.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode1.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode1.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                waveNode1.AppendChild(GetCustomizedXmlNode(0));
                waveListNode.AppendChild(waveNode1);

                // wave 2 for SATURATION
                XmlNode waveNode2 = CreateXmlNode("wave");
                if (ColorSegmentation == true)
                    waveNode2.AppendChild(CreateXmlNodeByValue("type", "CustomLinearWave"));
                else // false
                    waveNode2.AppendChild(CreateXmlNodeByValue("type", "CustomStepWave"));
                waveNode2.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode2.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("length", maxOperatingGridLength.ToString()));
                if (Speed == 0)
                {
                    if (RainbowSpecialEffects && (RainbowSpecialMode == 2 || RainbowSpecialMode == 4))
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "-0.05"));
                    else
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.05"));
                }
                else if (Speed == 1)
                {
                    if (RainbowSpecialEffects && (RainbowSpecialMode == 2 || RainbowSpecialMode == 4))
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "-0.1"));
                    else
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                }
                else// 2
                {
                    if (RainbowSpecialEffects && (RainbowSpecialMode == 2 || RainbowSpecialMode == 4))
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "-0.2"));
                    else
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                }
                waveNode2.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode2.AppendChild(GetBindToSlotXmlNode(new List<string> { "SATURATION" }));
                waveNode2.AppendChild(GetCustomizedXmlNode(1));
                waveListNode.AppendChild(waveNode2);

                // wave 3 for LIGHTNESS
                XmlNode waveNode3 = CreateXmlNode("wave");
                if (ColorSegmentation == true)
                    waveNode3.AppendChild(CreateXmlNodeByValue("type", "CustomLinearWave"));
                else // false
                    waveNode3.AppendChild(CreateXmlNodeByValue("type", "CustomStepWave"));
                waveNode3.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode3.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode3.AppendChild(CreateXmlNodeByValue("length", maxOperatingGridLength.ToString()));
                if (Speed == 0)
                {
                    if (RainbowSpecialEffects && (RainbowSpecialMode == 2 || RainbowSpecialMode == 4))
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "-0.05"));
                    else
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.05"));
                }
                else if (Speed == 1)
                {
                    if (RainbowSpecialEffects && (RainbowSpecialMode == 2 || RainbowSpecialMode == 4))
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "-0.1"));
                    else
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                }
                else// 2
                {
                    if (RainbowSpecialEffects && (RainbowSpecialMode == 2 || RainbowSpecialMode == 4))
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "-0.2"));
                    else
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                }
                waveNode3.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode3.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode3.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode3.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode3.AppendChild(GetBindToSlotXmlNode(new List<string> { "LIGHTNESS" }));
                waveNode3.AppendChild(GetCustomizedXmlNode(2));
                waveListNode.AppendChild(waveNode3);
            }
            else if (GetEffEngNameByIdx(Type) == "Strobing")
            {
                if (ColorModeSelection != 4)
                {
                    double[] hsl = AuraEditorColorHelper.RgbTOHsl(InitColor);
                    XmlNode waveNode = CreateXmlNode("wave");
                    waveNode.AppendChild(CreateXmlNodeByValue("type", "SawtoothWave"));
                    waveNode.AppendChild(CreateXmlNodeByValue("max", hsl[2].ToString()));
                    waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                    waveNode.AppendChild(CreateXmlNodeByValue("length", "23"));
                    if (Speed == 0)
                        waveNode.AppendChild(CreateXmlNodeByValue("freq", "0.8"));
                    else if (Speed == 1)
                        waveNode.AppendChild(CreateXmlNodeByValue("freq", "1.2"));
                    else // 2
                        waveNode.AppendChild(CreateXmlNodeByValue("freq", "1.6"));
                    waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "LIGHTNESS" }));
                    waveListNode.AppendChild(waveNode);
                }

                XmlNode waveNode2 = CreateXmlNode("wave");
                waveNode2.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                waveNode2.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode2.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("length", "23"));
                if (Speed == 0)
                    waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.8"));
                else if (Speed == 1)
                    waveNode2.AppendChild(CreateXmlNodeByValue("freq", "1.2"));
                else // 2
                    waveNode2.AppendChild(CreateXmlNodeByValue("freq", "1.6"));
                waveNode2.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode2.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode2);

                if (ColorModeSelection == 2)
                {
                    XmlNode waveNode3 = CreateXmlNode("wave");
                    waveNode3.AppendChild(CreateXmlNodeByValue("type", "RandomWave"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("max", ((double)RandomRangeMax / 12).ToString()));
                    waveNode3.AppendChild(CreateXmlNodeByValue("min", ((double)RandomRangeMin / 12).ToString()));
                    waveNode3.AppendChild(CreateXmlNodeByValue("length", "23"));
                    if (Speed == 0)
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.8"));
                    else if (Speed == 1)
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "1.2"));
                    else // 2
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "1.6"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode3.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                    waveListNode.AppendChild(waveNode3);
                }
                else if (ColorModeSelection == 4)
                {
                    XmlNode waveNode3 = CreateXmlNode("wave");
                    waveNode3.AppendChild(CreateXmlNodeByValue("type", "CustomStepWave"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("max", "1"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("min", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("length", "23"));
                    if (Speed == 0)
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.4"));
                    else if (Speed == 1)
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.6"));
                    else // 2
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.8"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode3.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                    waveNode3.AppendChild(GetCustomizedNodeFromDoubleColor(0));
                    waveListNode.AppendChild(waveNode3);

                    XmlNode waveNode4 = CreateXmlNode("wave");
                    waveNode4.AppendChild(CreateXmlNodeByValue("type", "CustomStepWave"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("max", "1"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("min", "0"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("length", "23"));
                    if (Speed == 0)
                        waveNode4.AppendChild(CreateXmlNodeByValue("freq", "0.4"));
                    else if (Speed == 1)
                        waveNode4.AppendChild(CreateXmlNodeByValue("freq", "0.6"));
                    else // 2
                        waveNode4.AppendChild(CreateXmlNodeByValue("freq", "0.8"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode4.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode4.AppendChild(GetBindToSlotXmlNode(new List<string> { "SATURATION" }));
                    waveNode4.AppendChild(GetCustomizedNodeFromDoubleColor(1));
                    waveListNode.AppendChild(waveNode4);

                    XmlNode waveNode5 = CreateXmlNode("wave");
                    waveNode5.AppendChild(CreateXmlNodeByValue("type", "CustomLinearWave"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("max", "1"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("min", "0"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("length", "23"));
                    if (Speed == 0)
                        waveNode5.AppendChild(CreateXmlNodeByValue("freq", "0.4"));
                    else if (Speed == 1)
                        waveNode5.AppendChild(CreateXmlNodeByValue("freq", "0.6"));
                    else // 2
                        waveNode5.AppendChild(CreateXmlNodeByValue("freq", "0.8"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode5.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode5.AppendChild(GetBindToSlotXmlNode(new List<string> { "LIGHTNESS" }));
                    waveNode5.AppendChild(GetCustomizedNodeFromDoubleColor(2));
                    waveListNode.AppendChild(waveNode5);
                }
            }
            else if (GetEffEngNameByIdx(Type) == "Comet")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "ConstantWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "3"));
                if (Speed == 0)
                {
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "0"));
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "10"));
                }
                else if (Speed == 1)
                {
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "0"));
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "15"));
                }
                else // 2
                {
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "0"));
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "20"));
                }
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "1"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);

                if (ColorModeSelection == 2)
                {
                    // wave 2
                    XmlNode waveNode2 = CreateXmlNode("wave");
                    waveNode2.AppendChild(CreateXmlNodeByValue("type", "RandomWave"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("max", ((double)RandomRangeMax / 12).ToString()));
                    waveNode2.AppendChild(CreateXmlNodeByValue("min", ((double)RandomRangeMin / 12).ToString()));
                    waveNode2.AppendChild(CreateXmlNodeByValue("length", "24"));
                    if (Speed == 0)
                    {
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "1"));
                        waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "10"));
                    }
                    else if (Speed == 1)
                    {
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "1"));
                        waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "15"));
                    }
                    else // 2
                    {
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "1"));
                        waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "20"));
                    }
                    waveNode2.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("isCycle", "1"));
                    waveNode2.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                    waveListNode.AppendChild(waveNode2);
                }
            }
            else if (GetEffEngNameByIdx(Type) == "Tide")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "0.4"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0.2"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "23"));
                waveNode.AppendChild(CreateXmlNodeByValue("freq", "0.25"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "-3"));
                waveNode.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));

                // bindToSignalNode ++
                XmlNode bindToSignalNode = CreateXmlNode("bindToSignal");
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("source", "Formula"));
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("target", "length"));
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("amplify", "1"));
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("offset", "0"));
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("max", (maxOperatingGridLength + 3).ToString())); // because start is -3
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                bindToSignalNode.AppendChild(CreateXmlNodeByValue("length", "25"));
                if (Speed == 0)
                {
                    bindToSignalNode.AppendChild(CreateXmlNodeByValue("freq", "0.05"));
                }
                else if (Speed == 1)
                {
                    bindToSignalNode.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                }
                else // 2
                {
                    bindToSignalNode.AppendChild(CreateXmlNodeByValue("freq", "0.3"));
                }
                waveNode.AppendChild(bindToSignalNode);
                // bindToSignalNode --

                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);
            }
            else if (GetEffEngNameByIdx(Type) == "Star")
            {
                double[] hsl = AuraEditorColorHelper.RgbTOHsl(InitColor);
                // wave 1
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", hsl[2].ToString()));
                if (hsl[2] <= 0.2)
                    waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                else
                    waveNode.AppendChild(CreateXmlNodeByValue("min", (hsl[2] - 0.2).ToString()));
                waveNode.AppendChild(CreateXmlNodeByValue("length", (zoneCount / 7 + 1).ToString()));
                waveNode.AppendChild(CreateXmlNodeByValue("freq", "Random"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "Random"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("velocity", "4"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "1"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "LIGHTNESS" }));
                waveListNode.AppendChild(waveNode);

                XmlNode waveNode2 = CreateXmlNode("wave");
                waveNode2.AppendChild(CreateXmlNodeByValue("type", "ConstantWave"));
                waveNode2.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode2.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("length", (zoneCount / 7 + 1).ToString()));
                waveNode2.AppendChild(CreateXmlNodeByValue("freq", "Random"));
                waveNode2.AppendChild(CreateXmlNodeByValue("phase", "Random"));
                waveNode2.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "4"));
                waveNode2.AppendChild(CreateXmlNodeByValue("isCycle", "1"));
                waveNode2.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode2);

                if (ColorModeSelection == 2)
                {
                    XmlNode waveNode3 = CreateXmlNode("wave");
                    waveNode3.AppendChild(CreateXmlNodeByValue("type", "RandomWave"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("max", ((double)RandomRangeMax / 12).ToString()));
                    waveNode3.AppendChild(CreateXmlNodeByValue("min", ((double)RandomRangeMin / 12).ToString()));
                    waveNode3.AppendChild(CreateXmlNodeByValue("length", (zoneCount / 7 + 1).ToString()));
                    waveNode3.AppendChild(CreateXmlNodeByValue("freq", "Random"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("phase", "Random"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("velocity", "4"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("isCycle", "1"));
                    waveNode3.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                    waveListNode.AppendChild(waveNode3);
                }
            }
            else if (GetEffEngNameByIdx(Type) == "Reactive")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "1"));
                if (Speed == 0)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "1"));
                else if (Speed == 1)
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "2"));
                else // 2
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "4"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);

                if (ColorModeSelection == 2)
                {
                    XmlNode waveNode2 = CreateXmlNode("wave");
                    waveNode2.AppendChild(CreateXmlNodeByValue("type", "RandomWave"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("max", ((double)RandomRangeMax / 12).ToString()));
                    waveNode2.AppendChild(CreateXmlNodeByValue("min", ((double)RandomRangeMin / 12).ToString()));
                    waveNode2.AppendChild(CreateXmlNodeByValue("length", "1"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode2.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                    waveListNode.AppendChild(waveNode2);
                }
            }
            else if (GetEffEngNameByIdx(Type) == "Laser")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "SineWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "10"));
                waveNode.AppendChild(CreateXmlNodeByValue("freq", "Random"));
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                if (Speed == 0)
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "10"));
                else if (Speed == 1)
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "15"));
                else // 2
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "20"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);

                if (ColorModeSelection == 2)
                {
                    XmlNode waveNode2 = CreateXmlNode("wave");
                    waveNode2.AppendChild(CreateXmlNodeByValue("type", "RandomWave"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("max", ((double)RandomRangeMax / 12).ToString()));
                    waveNode2.AppendChild(CreateXmlNodeByValue("min", ((double)RandomRangeMin / 12).ToString()));
                    waveNode2.AppendChild(CreateXmlNodeByValue("length", "10"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "10"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode2.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                    waveListNode.AppendChild(waveNode2);
                }
            }
            else if (GetEffEngNameByIdx(Type) == "Ripple")
            {
                XmlNode waveNode = CreateXmlNode("wave");
                waveNode.AppendChild(CreateXmlNodeByValue("type", "ConstantWave"));
                waveNode.AppendChild(CreateXmlNodeByValue("max", "1"));
                waveNode.AppendChild(CreateXmlNodeByValue("min", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("length", "2"));
                if (Speed == 0)
                {
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "1"));
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "10"));
                }
                else if (Speed == 1)
                {
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "2"));
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "15"));
                }
                else // 2
                {
                    waveNode.AppendChild(CreateXmlNodeByValue("freq", "4"));
                    waveNode.AppendChild(CreateXmlNodeByValue("velocity", "20"));
                }
                waveNode.AppendChild(CreateXmlNodeByValue("phase", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("start", "0"));
                waveNode.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                waveNode.AppendChild(GetBindToSlotXmlNode(new List<string> { "ALPHA" }));
                waveListNode.AppendChild(waveNode);

                if (ColorModeSelection == 2)
                {
                    XmlNode waveNode2 = CreateXmlNode("wave");
                    waveNode2.AppendChild(CreateXmlNodeByValue("type", "RandomWave"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("max", ((double)RandomRangeMax / 12).ToString()));
                    waveNode2.AppendChild(CreateXmlNodeByValue("min", ((double)RandomRangeMin / 12).ToString()));
                    waveNode2.AppendChild(CreateXmlNodeByValue("length", "2"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "15"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode2.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                    waveListNode.AppendChild(waveNode2);
                }

                if (ColorModeSelection == 3)
                {
                    // wave 1 for HUE
                    XmlNode waveNode1 = CreateXmlNode("wave");
                    if (ColorSegmentation == true)
                        waveNode1.AppendChild(CreateXmlNodeByValue("type", "CustomLinearWave"));
                    else // false
                        waveNode1.AppendChild(CreateXmlNodeByValue("type", "CustomStepWave"));
                    waveNode1.AppendChild(CreateXmlNodeByValue("max", "1"));
                    waveNode1.AppendChild(CreateXmlNodeByValue("min", "0"));
                    waveNode1.AppendChild(CreateXmlNodeByValue("length", maxOperatingGridLength.ToString()));
                    if (Speed == 0)
                        waveNode1.AppendChild(CreateXmlNodeByValue("freq", "0.05"));
                    else if (Speed == 1)
                        waveNode1.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                    else // 2
                        waveNode1.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                    waveNode1.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode1.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode1.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode1.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode1.AppendChild(GetBindToSlotXmlNode(new List<string> { "HUE" }));
                    waveNode1.AppendChild(GetCustomizedXmlNode(0));
                    waveListNode.AppendChild(waveNode1);

                    // wave 2 for SATURATION
                    XmlNode waveNode2 = CreateXmlNode("wave");
                    if (ColorSegmentation == true)
                        waveNode2.AppendChild(CreateXmlNodeByValue("type", "CustomLinearWave"));
                    else // false
                        waveNode2.AppendChild(CreateXmlNodeByValue("type", "CustomStepWave"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("max", "1"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("min", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("length", maxOperatingGridLength.ToString()));
                    if (Speed == 0)
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.05"));
                    else if (Speed == 1)
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                    else // 2
                        waveNode2.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode2.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode2.AppendChild(GetBindToSlotXmlNode(new List<string> { "SATURATION" }));
                    waveNode2.AppendChild(GetCustomizedXmlNode(1));
                    waveListNode.AppendChild(waveNode2);

                    // wave 3 for LIGHTNESS
                    XmlNode waveNode3 = CreateXmlNode("wave");
                    if (ColorSegmentation == true)
                        waveNode3.AppendChild(CreateXmlNodeByValue("type", "CustomLinearWave"));
                    else // false
                        waveNode3.AppendChild(CreateXmlNodeByValue("type", "CustomStepWave"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("max", "1"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("min", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("length", maxOperatingGridLength.ToString()));
                    if (Speed == 0)
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.05"));
                    else if (Speed == 1)
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.1"));
                    else // 2
                        waveNode3.AppendChild(CreateXmlNodeByValue("freq", "0.2"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("phase", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("start", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("velocity", "0"));
                    waveNode3.AppendChild(CreateXmlNodeByValue("isCycle", "0"));
                    waveNode3.AppendChild(GetBindToSlotXmlNode(new List<string> { "LIGHTNESS" }));
                    waveNode3.AppendChild(GetCustomizedXmlNode(2));
                    waveListNode.AppendChild(waveNode3);
                }
            }

            return waveListNode;
        }
        static private XmlNode GetBindToSlotXmlNode(List<string> slotkeys)
        {
            XmlNode bindToSlotXmlNode = CreateXmlNode("bindToSlot");

            foreach (var key in slotkeys)
            {
                XmlNode slotNode = CreateXmlNode("slot");
                XmlAttribute attribute = CreateXmlAttributeOfFile("key");
                attribute.Value = key;
                slotNode.Attributes.Append(attribute);
                bindToSlotXmlNode.AppendChild(slotNode);
            }

            return bindToSlotXmlNode;
        }
        private XmlNode GetCustomizedXmlNode(int hsv)
        {
            List<ColorPointModel> list;

            if (PatternSelect == -1)
                list = CustomizedPattern;
            else
                list = DefaultColorPointListCollection[PatternSelect];

            return GetCustomizedNodeFromColorPointList(list, hsv);
        }
        static private XmlNode GetCustomizedNodeFromColorPointList(List<ColorPointModel> ColorPointList, int hsv)
        {
            XmlNode customizedNode = CreateXmlNode("customized");
            double Scaledown = (double)(ColorPointList.Count - 1) / (double)ColorPointList.Count;

            for (int i = 0; i < ColorPointList.Count; i++)
            {
                double ScaledownOffset = ColorPointList[i].Offset * Scaledown * ColorPointList[ColorPointList.Count - 1].Offset;
                if (i == 0)
                {
                    customizedNode.AppendChild(SetNodeInfoXmlNode(i, 0, ColorPointList[i].Color, hsv));
                }
                customizedNode.AppendChild(SetNodeInfoXmlNode(i, ScaledownOffset, ColorPointList[i].Color, hsv));
                if (i == (ColorPointList.Count - 1))
                {
                    customizedNode.AppendChild(SetNodeInfoXmlNode(i, 1, ColorPointList[0].Color, hsv));
                }
            }
            return customizedNode;
        }

        private XmlNode GetCustomizedNodeFromDoubleColor(int hsv)
        {
            XmlNode customizedNode = CreateXmlNode("customized");
            if(hsv != 2)
            {
                //Main Double Color
                customizedNode.AppendChild(SetNodeInfoXmlNode(0, 0, DoubleColor1, hsv));
                customizedNode.AppendChild(SetNodeInfoXmlNode(1, 0.5, DoubleColor2, hsv));

                //The last point is not used, just use the color to execute the script.
                customizedNode.AppendChild(SetNodeInfoXmlNode(2, 1, DoubleColor1, hsv));
            }
            else
            {
                customizedNode.AppendChild(SetNodeInfoXmlNode(0, 0, Colors.Black, hsv));
                customizedNode.AppendChild(SetNodeInfoXmlNode(1, 0.25, DoubleColor1, hsv));
                customizedNode.AppendChild(SetNodeInfoXmlNode(2, 0.5, Colors.Black, hsv));
                customizedNode.AppendChild(SetNodeInfoXmlNode(3, 0.75, DoubleColor2, hsv));
                customizedNode.AppendChild(SetNodeInfoXmlNode(4, 1, Colors.Black, hsv));
            }
            
            return customizedNode;
        }

        static private XmlNode SetNodeInfoXmlNode(int key, double offset, Color c, int hsv)
        {
            XmlNode nodeXmlNode = CreateXmlNode("node");
            XmlAttribute attribute = CreateXmlAttributeOfFile("key");
            XmlNode phaseXmlNode = CreateXmlNode("phase");
            XmlNode fxXmlNode = CreateXmlNode("fx");
            double[] hsl = AuraEditorColorHelper.RgbTOHsl(c);

            attribute.Value = key.ToString();
            nodeXmlNode.Attributes.Append(attribute);
            phaseXmlNode.InnerText = offset.ToString();
            nodeXmlNode.AppendChild(phaseXmlNode);
            fxXmlNode.InnerText = hsl[hsv].ToString();
            nodeXmlNode.AppendChild(fxXmlNode);

            return nodeXmlNode;
        }

        static private XmlNode GetInitColorXmlNode(Color c)
        {
            XmlNode initColorNode = CreateXmlNode("initColor");
            double[] hsl = AuraEditorColorHelper.RgbTOHsl(c);

            XmlNode hueNode = CreateXmlNode("hue");
            hueNode.InnerText = hsl[0].ToString();
            initColorNode.AppendChild(hueNode);

            XmlNode saturationNode = CreateXmlNode("saturation");
            saturationNode.InnerText = hsl[1].ToString();
            initColorNode.AppendChild(saturationNode);

            XmlNode lightnessNode = CreateXmlNode("lightness");
            lightnessNode.InnerText = hsl[2].ToString();
            initColorNode.AppendChild(lightnessNode);

            XmlNode alphaNode = CreateXmlNode("alpha");
            alphaNode.InnerText = (c.A / 255).ToString();
            initColorNode.AppendChild(alphaNode);

            return initColorNode;
        }
        #endregion

        public XmlNode ToXmlNodeForUserData()
        {
            XmlNode effectNode = CreateXmlNode("effect");

            XmlNode typeNode = CreateXmlNode("type");
            typeNode.InnerText = Type.ToString();
            effectNode.AppendChild(typeNode);

            XmlNode aNode = CreateXmlNode("a");
            aNode.InnerText = InitColor.A.ToString();
            effectNode.AppendChild(aNode);

            XmlNode rNode = CreateXmlNode("r");
            rNode.InnerText = InitColor.R.ToString();
            effectNode.AppendChild(rNode);

            XmlNode gNode = CreateXmlNode("g");
            gNode.InnerText = InitColor.G.ToString();
            effectNode.AppendChild(gNode);

            XmlNode bNode = CreateXmlNode("b");
            bNode.InnerText = InitColor.B.ToString();
            effectNode.AppendChild(bNode);

            XmlNode d1aNode = CreateXmlNode("d1a");
            d1aNode.InnerText = DoubleColor1.A.ToString();
            effectNode.AppendChild(d1aNode);

            XmlNode d1rNode = CreateXmlNode("d1r");
            d1rNode.InnerText = DoubleColor1.R.ToString();
            effectNode.AppendChild(d1rNode);

            XmlNode d1gNode = CreateXmlNode("d1g");
            d1gNode.InnerText = DoubleColor1.G.ToString();
            effectNode.AppendChild(d1gNode);

            XmlNode d1bNode = CreateXmlNode("d1b");
            d1bNode.InnerText = DoubleColor1.B.ToString();
            effectNode.AppendChild(d1bNode);

            XmlNode d2aNode = CreateXmlNode("d2a");
            d2aNode.InnerText = DoubleColor2.A.ToString();
            effectNode.AppendChild(d2aNode);

            XmlNode d2rNode = CreateXmlNode("d2r");
            d2rNode.InnerText = DoubleColor2.R.ToString();
            effectNode.AppendChild(d2rNode);

            XmlNode d2gNode = CreateXmlNode("d2g");
            d2gNode.InnerText = DoubleColor2.G.ToString();
            effectNode.AppendChild(d2gNode);

            XmlNode d2bNode = CreateXmlNode("d2b");
            d2bNode.InnerText = DoubleColor2.B.ToString();
            effectNode.AppendChild(d2bNode);

            XmlNode brightnessNode = CreateXmlNode("brightness");
            brightnessNode.InnerText = Brightness.ToString();
            effectNode.AppendChild(brightnessNode);

            XmlNode speedNode = CreateXmlNode("speed");
            speedNode.InnerText = Speed.ToString();
            effectNode.AppendChild(speedNode);

            XmlNode angleNode = CreateXmlNode("angle");
            angleNode.InnerText = ((int)Angle).ToString();
            effectNode.AppendChild(angleNode);

            XmlNode randomRangeMaxNode = CreateXmlNode("randomRangeMax");
            randomRangeMaxNode.InnerText = RandomRangeMax.ToString();
            effectNode.AppendChild(randomRangeMaxNode);

            XmlNode randomRangeMinNode = CreateXmlNode("randomRangeMin");
            randomRangeMinNode.InnerText = RandomRangeMin.ToString();
            effectNode.AppendChild(randomRangeMinNode);

            XmlNode colormodeselectionNode = CreateXmlNode("colormodeselection");
            colormodeselectionNode.InnerText = ColorModeSelection.ToString();
            effectNode.AppendChild(colormodeselectionNode);

            XmlNode highNode = CreateXmlNode("high");
            highNode.InnerText = High.ToString();
            effectNode.AppendChild(highNode);

            XmlNode lowNode = CreateXmlNode("low");
            lowNode.InnerText = Low.ToString();
            effectNode.AppendChild(lowNode);

            XmlNode patternSelection = CreateXmlNode("patternSelect");
            patternSelection.InnerText = PatternSelect.ToString();
            effectNode.AppendChild(patternSelection);

            XmlNode colorPointListNode = CreateXmlNode("colorPointList");
            foreach (var item in CustomizedPattern)
            {
                XmlNode colorPointNode = CreateXmlNode("colorPoint");

                XmlNode colorANode = CreateXmlNode("a");
                colorANode.InnerText = item.Color.A.ToString();
                colorPointNode.AppendChild(colorANode);

                XmlNode colorRNode = CreateXmlNode("r");
                colorRNode.InnerText = item.Color.R.ToString();
                colorPointNode.AppendChild(colorRNode);

                XmlNode colorGNode = CreateXmlNode("g");
                colorGNode.InnerText = item.Color.G.ToString();
                colorPointNode.AppendChild(colorGNode);

                XmlNode colorBNode = CreateXmlNode("b");
                colorBNode.InnerText = item.Color.B.ToString();
                colorPointNode.AppendChild(colorBNode);

                XmlNode offsetNode = CreateXmlNode("offset");
                offsetNode.InnerText = item.Offset.ToString();
                colorPointNode.AppendChild(offsetNode);

                colorPointListNode.AppendChild(colorPointNode);
            }
            effectNode.AppendChild(colorPointListNode);

            XmlNode colorSegmentationNode = CreateXmlNode("colorSegmentation");
            colorSegmentationNode.InnerText = ColorSegmentation.ToString();
            effectNode.AppendChild(colorSegmentationNode);

            XmlNode rainbowRotationNode = CreateXmlNode("rainbowRotation");
            rainbowRotationNode.InnerText = RainbowSpecialEffects.ToString();
            effectNode.AppendChild(rainbowRotationNode);

            XmlNode rotationModeNode = CreateXmlNode("rotationMode");
            rotationModeNode.InnerText = RainbowSpecialMode.ToString();
            effectNode.AppendChild(rotationModeNode);

            return effectNode;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
