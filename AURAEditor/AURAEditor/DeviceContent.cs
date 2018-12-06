using AuraEditor.Common;
using AuraEditor.Models;
using AuraEditor.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404

namespace AuraEditor
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>

    public class LedUI
    {
        public int Index;
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
        public int ZIndex;
        public string PNG_Path;
    }
    public class DeviceContent
    {
        public string DeviceName;
        public int DeviceType;
        public int GridWidth;
        public int GridHeight;
        public List<LedUI> Leds;
        public BitmapImage Image;

        public DeviceContent()
        {
            Leds = new List<LedUI>();
        }

        static public async Task<DeviceContent> GetDeviceContent(SyncDevice syncDevice)
        {
            string modelName = syncDevice.Name;
            string type = syncDevice.Type;
            return await CreateDeviceContent(modelName, type);
        }
        static public async Task<DeviceContent> GetDeviceContent(XmlNode node)
        {
            XmlElement elem = (XmlElement)node;
            string modelName = elem.GetAttribute("name");
            string type = elem.GetAttribute("type");
            return await CreateDeviceContent(modelName, type);
        }
        static private async Task<DeviceContent> CreateDeviceContent(string modelName, string type)
        {
            try
            {
                DeviceContent deviceContent = new DeviceContent();
                string auraCreatorFolderPath = "C:\\ProgramData\\ASUS\\AURA Creator\\Devices\\";

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(auraCreatorFolderPath + modelName);
                StorageFile csvFile = await folder.GetFileAsync(modelName + ".csv");
                StorageFile pngFile = await folder.GetFileAsync(modelName + ".png");

                deviceContent.DeviceName = modelName;

                int exist_Column = -1;
                int leftTopX_Column = -1;
                int leftTopY_Column = -1;
                int rightBottomX_Column = -1;
                int rightBottomY_Column = -1;
                int z_Column = -1;
                int png_Column = -1;

                if (csvFile != null)
                {
                    using (CsvFileReader csvReader = new CsvFileReader(await csvFile.OpenStreamForReadAsync()))
                    {
                        CsvRow row = new CsvRow();
                        while (csvReader.ReadRow(row))
                        {
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

                                LedUI ledui = new LedUI()
                                {
                                    Index = Int32.Parse(row[0].ToLower().Substring("led ".Length)),
                                    Left = Int32.Parse(row[leftTopX_Column]),
                                    Top = Int32.Parse(row[leftTopY_Column]),
                                    Right = Int32.Parse(row[rightBottomX_Column]),
                                    Bottom = Int32.Parse(row[rightBottomY_Column]),
                                    ZIndex = Int32.Parse(row[z_Column]),
                                };

                                if (png_Column != -1 && row[png_Column] != "")
                                    ledui.PNG_Path = auraCreatorFolderPath + modelName + "\\" + row[png_Column];

                                deviceContent.Leds.Add(ledui);
                            }
                        }
                    }
                }

                int gridW, gridH;
                switch (type)
                {
                    case "Notebook": gridW = 27; gridH = 27; break;
                    case "Mouse": gridW = 8; gridH = 10; break;
                    case "Keyboard": gridW = 25; gridH = 14; break;
                    case "MotherBoard": gridW = 36; gridH = 36; break;
                    case "MousePad": gridW = 12; gridH = 16; break;
                    default: gridW = 36; gridH = 36; break;
                }
                deviceContent.DeviceType = GetTypeByTypeName(type);

                if (pngFile != null)
                {
                    using (IRandomAccessStream fileStream = await pngFile.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();

                        bitmapImage.SetSource(fileStream);
                        deviceContent.Image = bitmapImage;
                        deviceContent.GridWidth = gridW;
                        deviceContent.GridHeight = gridH;
                    }
                }

                return deviceContent;
            }
            catch
            {
                return null;
            }
        }

        public async Task<DeviceModel> ToDeviceModel()
        {
            return await ToDeviceModel(new Point(0, 0));
        }
        public async Task<DeviceModel> ToDeviceModel(Point point)
        {
            DeviceModel model = new DeviceModel();
            ObservableCollection<ZoneModel> zones = new ObservableCollection<ZoneModel>();
            ObservableCollection<SpecialZoneModel> specialzones = new ObservableCollection<SpecialZoneModel>();

            model.Name = this.DeviceName;
            model.Type = this.DeviceType;
            model.Image = this.Image;
            model.PixelLeft = point.X;
            model.PixelTop = point.Y;
            model.PixelWidth = GridWidth * GridPixels;
            model.PixelHeight = GridHeight * GridPixels;

            foreach (var led in Leds)
            {
                if (led.PNG_Path == null)
                {
                    ZoneModel zm = new ZoneModel
                    {
                        Index = led.Index,
                        PixelLeft = led.Left,
                        PixelTop = led.Top,
                        PixelWidth = led.Right - led.Left,
                        PixelHeight = led.Bottom - led.Top,
                        Zindex = led.ZIndex,
                    };
                    zones.Add(zm);
                }
                else
                {
                    SoftwareBitmap specialFrameSB;
                    SpecialZoneModel szm = new SpecialZoneModel()
                    {
                        Index = led.Index,
                        PixelLeft = led.Left,
                        PixelTop = led.Top,
                        PixelWidth = led.Right - led.Left,
                        PixelHeight = led.Bottom - led.Top,
                        Zindex = led.ZIndex
                    };

                    StorageFile pngFile = await StorageFile.GetFileFromPathAsync(led.PNG_Path);

                    using (IRandomAccessStream stream = await pngFile.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                        specialFrameSB = await decoder.GetSoftwareBitmapAsync();
                    }

                    specialFrameSB = SoftwareBitmap.Convert(specialFrameSB, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    await szm.SetSoftwareBitmapAsync(specialFrameSB);
                    specialzones.Add(szm);
                }
            }

            model.Zones = zones;
            model.SpecialZones = specialzones;

            return model;
        }
    }
}

