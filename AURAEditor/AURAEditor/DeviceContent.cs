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
        public int UI_Width;
        public int UI_Height;
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

                deviceContent.DeviceName = modelName;
                if (modelName == "GLADIUS II")
                    deviceContent.DeviceType = 1;
                else
                    deviceContent.DeviceType = 0;


                if (csvFile != null)
                {
                    using (CsvFileReader csvReader = new CsvFileReader(await csvFile.OpenStreamForReadAsync()))
                    {
                        CsvRow row = new CsvRow();
                        while (csvReader.ReadRow(row))
                        {
                            if (row[0] == "UI_width")
                            {
                                deviceContent.UI_Width = Int32.Parse(row[1]);
                            }
                            else if (row[0] == "UI_height")
                            {
                                deviceContent.UI_Height = Int32.Parse(row[1]);
                            }
                            else if (row[0].Contains("LED "))
                            {
                                deviceContent.Leds.Add(
                                    new LedUI()
                                    {
                                        Index = Int32.Parse(row[0].Substring("LED ".Length)),
                                        Left = Int32.Parse(row[3]),
                                        Top = Int32.Parse(row[4]),
                                        Right = Int32.Parse(row[5]),
                                        Bottom = Int32.Parse(row[6]),
                                        ZIndex = Int32.Parse(row[7]),
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
                TranslateX = GridWidthPixels * gridPosition.X,
                TranslateY = GridWidthPixels * gridPosition.Y
            };

            img = new Image
            {
                RenderTransform = ct,
                Width = GridWidthPixels * this.UI_Width,
                Height = GridWidthPixels * this.UI_Height,
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
                Width = this.UI_Width,
                Height = this.UI_Height,
                Image = img,
            };

            return device;
        }
    }
}

