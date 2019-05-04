using AuraEditor.Common;
using AuraEditor.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.MetroEventSource;
using static AuraEditor.Common.XmlHelper;

namespace AuraEditor.Models
{
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

        public string ModelName { get; set; }
        public int Type { get; set; }
        public string FolderName { get; set; }
        public string CsvName { get; set; }
        public string PngName { get; set; }
        public bool Plugged { get; set; }
        public bool Sync { get; internal set; }
        #endregion

        public DeviceModel()
        {
        }
        static public async Task<DeviceModel> ToDeviceModelAsync(string model, string folder, string csv, string png, string type, string sync)
        {
            return await GetDeviceModel(model, folder, csv, png, type, sync);
        }
        static public async Task<DeviceModel> ToDeviceModelAsync(XmlNode node)
        {
            XmlElement elem = (XmlElement)node;
            string modelName = elem.GetAttribute("name");
            string folderName = elem.GetAttribute("folder");
            string csvName = elem.GetAttribute("csv");
            string pngName = elem.GetAttribute("png");
            string type = elem.GetAttribute("type");
            return await GetDeviceModel(modelName, folderName, csvName, pngName, type, "true");
        }
        static private async Task<DeviceModel> GetDeviceModel(string modelName, string folderName, string csvName,
                                                                string pngName, string type, string sync)
        {
            try
            {
                DeviceModel dm = new DeviceModel
                {
                    ModelName = modelName,
                    FolderName = folderName,
                    CsvName=csvName,
                    PngName = pngName,
                    Type = GetTypeByTypeName(type),
                    Sync = bool.Parse(sync),
                };

                Log.Debug("[GetDeviceModel] Model name : " + modelName +
                          ", Folder name : " + folderName +
                          ", CSV name : " + csvName +
                          ", PNG name : " + pngName +
                          ", Type name : " + type +
                          ", Sync name : " + sync);

                ObservableCollection<ZoneModel> zones = new ObservableCollection<ZoneModel>();
                ObservableCollection<SpecialZoneModel> specialzones = new ObservableCollection<SpecialZoneModel>();

                string auraCreatorFolderPath = ApplicationData.Current.LocalFolder.Path + "\\Devices\\";
                double rateW = 0;
                double rateH = 0;

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(auraCreatorFolderPath + folderName);
                StorageFile csvFile = await folder.GetFileAsync(csvName + ".csv");
                StorageFile pngFile = await folder.GetFileAsync(pngName + ".png");
                
                int gridW, gridH;

                switch (type)
                {
                    case "Notebook": gridW = 27; gridH = 27; break;
                    case "Mouse": gridW = 8; gridH = 10; break;
                    case "Keyboard": gridW = 25; gridH = 14; break;
                    case "MotherBoard": gridW = 36; gridH = 36; break;
                    case "MousePad": gridW = 12; gridH = 16; break;
                    case "Headset": gridW = 12; gridH = 15; break;
                    case "Microphone": gridW = 10; gridH = 12; break;
                    default: gridW = 36; gridH = 36; break;
                }

                if (pngFile != null)
                {
                    Log.Debug("[GetDeviceModel] Parse PNG info ...");
                    using (IRandomAccessStream fileStream = await pngFile.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(fileStream);
                        dm.Image = bitmapImage;
                    }
                }

                int exist_Column = -1;
                int leftTopX_Column = -1;
                int leftTopY_Column = -1;
                int rightBottomX_Column = -1;
                int rightBottomY_Column = -1;
                int z_Column = -1;
                int png_Column = -1;

                if (csvFile != null)
                {
                    Log.Debug("[GetDeviceModel] Parse CSV info ...");

                    // If csv not define, we use default gridW & gridH.
                    rateW = (double)(gridW * GridPixels) / dm.Image.PixelWidth;
                    rateH = (double)(gridH * GridPixels) / dm.Image.PixelHeight;

                    using (CsvFileReader csvReader = new CsvFileReader(await csvFile.OpenStreamForReadAsync()))
                    {
                        CsvRow row = new CsvRow();
                        while (csvReader.ReadRow(row))
                        {
                            if (row[0].ToLower() == "gridwidth")
                            {
                                gridW = Int32.Parse(row[1]);
                                rateW = (double)(gridW * GridPixels) / dm.Image.PixelWidth;
                            }
                            else if (row[0].ToLower() == "gridheight")
                            {
                                gridH = Int32.Parse(row[1]);
                                rateH = (double)(gridH * GridPixels) / dm.Image.PixelHeight;
                            }
                            if (row[0].ToLower() == "parameters")
                            {
                                for (int i = 0; i < row.Count; i++)
                                {
                                    if (row[i].ToLower() == "exist") { exist_Column = i; }
                                    else if (row[i].ToLower() == "lefttop_x") { leftTopX_Column = i; }
                                    else if (row[i].ToLower() == "lefttop_y") { leftTopY_Column = i; }
                                    else if (row[i].ToLower() == "rightbottom_x") { rightBottomX_Column = i; }
                                    else if (row[i].ToLower() == "rightbottom_y") { rightBottomY_Column = i; }
                                    else if (row[i].ToLower() == "z_index") { z_Column = i; }
                                    else if (row[i].ToLower() == "png") { png_Column = i; }
                                }
                            }
                            else if (row[0].ToLower().Contains("led "))
                            {
                                if (row[exist_Column] != "1")
                                    continue;

                                if (png_Column != -1 && png_Column < row.Count && row[png_Column] != "")
                                {
                                    SpecialZoneModel szm = new SpecialZoneModel()
                                    {
                                        Index = Int32.Parse(row[0].ToLower().Substring("led ".Length)),
                                        PixelLeft = (int)Math.Round(Double.Parse(row[leftTopX_Column]) * rateW, 0),
                                        PixelTop = (int)Math.Round(Double.Parse(row[leftTopY_Column]) * rateH, 0),
                                        PixelWidth = (int)Math.Round(Double.Parse(row[rightBottomX_Column]) * rateW, 0)
                                                   - (int)Math.Round(Double.Parse(row[leftTopX_Column]) * rateW, 0),
                                        PixelHeight = (int)Math.Round(Double.Parse(row[rightBottomY_Column]) * rateH, 0)
                                                    - (int)Math.Round(Double.Parse(row[leftTopY_Column]) * rateH, 0),
                                    };

                                    if (z_Column != -1 && z_Column < row.Count && row[z_Column] != "")
                                        szm.Zindex = Int32.Parse(row[z_Column]);
                                    else
                                        szm.Zindex = 1;

                                    StorageFile ledPngFile = await folder.GetFileAsync(row[png_Column]);
                                    Log.Debug("[GetDeviceModel] Parse led png : " + ledPngFile.Name);

                                    string uri = @"ms-appdata:///local/Devices/" + folder.Name + "/" + row[png_Column];
                                    szm.ImageSource = uri;
                                    szm.ImageSourceSolid = uri.Insert(uri.Length - 4, "_solid");
                                    specialzones.Add(szm);
                                }
                                else
                                {
                                    ZoneModel zm = new ZoneModel
                                    {
                                        Index = Int32.Parse(row[0].ToLower().Substring("led ".Length)),
                                        PixelLeft = (int)Math.Round(Double.Parse(row[leftTopX_Column]) * rateW, 0),
                                        PixelTop = (int)Math.Round(Double.Parse(row[leftTopY_Column]) * rateH, 0),
                                        PixelWidth = (int)Math.Round(Double.Parse(row[rightBottomX_Column]) * rateW, 0)
                                                   - (int)Math.Round(Double.Parse(row[leftTopX_Column]) * rateW, 0),
                                        PixelHeight = (int)Math.Round(Double.Parse(row[rightBottomY_Column]) * rateH, 0)
                                                    - (int)Math.Round(Double.Parse(row[leftTopY_Column]) * rateH, 0),
                                    };

                                    if (z_Column != -1 && z_Column < row.Count && row[z_Column] != "")
                                        zm.Zindex = Int32.Parse(row[z_Column]);
                                    else
                                        zm.Zindex = 1;

                                    zones.Add(zm);
                                }

                                dm.Zones = zones;
                                dm.SpecialZones = specialzones;
                            }
                        }

                        dm.PixelWidth = gridW * GridPixels;
                        dm.PixelHeight = gridH * GridPixels;
                    }
                }

                return dm;
            }
            catch (Exception ex)
            {
                Log.Debug("[DeviceContent] Model load failed : " + modelName + ", " + ex.ToString());
                return null;
            }
        }

        public XmlNode ToXmlNodeForUserData()
        {
            XmlNode deviceNode = CreateXmlNode("device");

            XmlAttribute attributeName = CreateXmlAttributeOfFile("name");
            attributeName.Value = ModelName;
            deviceNode.Attributes.Append(attributeName);

            XmlAttribute attributeFolder = CreateXmlAttributeOfFile("folder");
            attributeFolder.Value = FolderName;
            deviceNode.Attributes.Append(attributeFolder);

            XmlAttribute attributeCSV = CreateXmlAttributeOfFile("csv");
            attributeCSV.Value = CsvName;
            deviceNode.Attributes.Append(attributeCSV);

            XmlAttribute attributePNG = CreateXmlAttributeOfFile("png");
            attributePNG.Value = PngName;
            deviceNode.Attributes.Append(attributePNG);

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
            modelNode.InnerText = ModelName.ToString();
            deviceNode.AppendChild(modelNode);

            XmlNode csvNode = CreateXmlNode("csv");
            csvNode.InnerText = CsvName.ToString();
            deviceNode.AppendChild(csvNode);

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
                case 8: type = "Microphone"; break;
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
