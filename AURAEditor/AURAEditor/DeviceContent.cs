using System;
using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MoonSharp.Interpreter;
using Windows.Storage.Pickers;
using AuraEditor.Common;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.ApplicationModel.Core;
using Windows.Storage.Streams;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.EffectHelper;
using System.Xml;
using System.ComponentModel;
using System.Linq;

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

        static public async Task<DeviceContent> GetDeviceContent(string modelName)
        {
            try
            {
                DeviceContent deviceContent = new DeviceContent();
                string auraCreatorFolderPath = "C:\\ProgramData\\ASUS\\AURA Creator\\Devices\\";

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(auraCreatorFolderPath + modelName);
                StorageFile csvFile = await folder.GetFileAsync(modelName + ".csv");
                StorageFile pngFile = await folder.GetFileAsync(modelName + ".png");

                int exist_Column = 0;
                int leftTopX_Column = 0;
                int leftTopY_Column = 0;
                int rightBottomX_Column = 0;
                int rightBottomY_Column = 0;
                int z_Column = 0;

                deviceContent.DeviceName = modelName;

                if (modelName == "GLADIUS II")
                    deviceContent.DeviceType = 1;
                else if (modelName == "Flare")
                    deviceContent.DeviceType = 2;
                else
                    deviceContent.DeviceType = 0;

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
                                }
                            }
                            else if (row[0].ToLower().Contains("led "))
                            {
                                if (row[exist_Column] != "1")
                                    continue;

                                deviceContent.Leds.Add(
                                    new LedUI()
                                    {
                                        Index = Int32.Parse(row[0].ToLower().Substring("led ".Length)),
                                        Left = Int32.Parse(row[leftTopX_Column]),
                                        Top = Int32.Parse(row[leftTopY_Column]),
                                        Right = Int32.Parse(row[rightBottomX_Column]),
                                        Bottom = Int32.Parse(row[rightBottomY_Column]),
                                        ZIndex = Int32.Parse(row[z_Column]),
                                    });
                            }
                        }
                    }
                }

                if (pngFile != null)
                {
                    using (IRandomAccessStream fileStream = await pngFile.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();

                        bitmapImage.SetSource(fileStream);
                        deviceContent.Image = bitmapImage;
                        deviceContent.GridWidth = bitmapImage.PixelWidth / GridPixels;
                        deviceContent.GridHeight = bitmapImage.PixelHeight / GridPixels;
                    }
                }

                return deviceContent;
            }
            catch
            {
                return null;
            }
        }
        public Device ToDevice()
        {
            return ToDevice(new Point(0, 0));
        }
        public Device ToDevice(Point gridPosition)
        {
            Device device;
            CompositeTransform ct;
            Image img;
            List<LightZone> zones = new List<LightZone>();

            ct = new CompositeTransform
            {
                TranslateX = GridPixels * gridPosition.X,
                TranslateY = GridPixels * gridPosition.Y
            };

            img = new Image
            {
                RenderTransform = ct,
                Width = Image.PixelWidth,
                Height = Image.PixelHeight,
                Source = this.Image,
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                Stretch = Stretch.Fill,
            };

            for (int idx = 0; idx < this.Leds.Count; idx++)
            {
                LedUI led = this.Leds[idx];
                zones.Add(new LightZone(gridPosition, led));
            }

            device = new Device(img)
            {
                Name = this.DeviceName,
                Type = this.DeviceType,
                LightZones = zones.ToArray(),
                GridPosition = new Point(gridPosition.X, gridPosition.Y),
                Width = GridWidth,
                Height = GridHeight,
                Image = img,
            };

            return device;
        }
    }
}

