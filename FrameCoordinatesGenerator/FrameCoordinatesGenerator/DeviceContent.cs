using CsvParse;
using FrameCoordinatesGenerator.Models;
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
using Windows.UI.Xaml.Media.Imaging;
using static FrameCoordinatesGenerator.Common.Math2;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404

namespace FrameCoordinatesGenerator
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

        public async Task<DeviceModel> ToDeviceModel(StorageFolder folder, Point point)
        {
            var zones = new ObservableCollection<ZoneModel>();
            var specialzones = new ObservableCollection<SpecialZoneModel>();
            
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

                    StorageFile pngFile = await folder.GetFileAsync(led.PNG_Path);

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

            DeviceModel model = new DeviceModel
            {
                Name = this.DeviceName,
                Type = this.DeviceType,
                Image = this.Image,
                PixelLeft = point.X,
                PixelTop = point.Y,
                PixelWidth = GridWidth * GridPixels,
                PixelHeight = GridHeight * GridPixels,
                Zones = zones,
                SpecialZones = specialzones,
            };

            return model;
        }
    }
}

