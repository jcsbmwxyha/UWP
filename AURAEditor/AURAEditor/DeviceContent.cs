﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using Windows.Storage.Streams;
using AuraEditor.Common;
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

        static public async Task<DeviceContent> GetDeviceContent(XmlNode node)
        {
            try
            {
                DeviceContent deviceContent = new DeviceContent();
                string auraCreatorFolderPath = "C:\\ProgramData\\ASUS\\AURA Creator\\Devices\\";

                XmlElement elem = (XmlElement)node;
                string modelName = elem.GetAttribute("name");
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(auraCreatorFolderPath + modelName);
                StorageFile csvFile = await folder.GetFileAsync(modelName + ".csv");
                StorageFile pngFile = await folder.GetFileAsync(modelName + ".png");

                deviceContent.DeviceName = modelName;
                deviceContent.DeviceType = GetTypeByTypeName(elem.GetAttribute("type"));

                int exist_Column = 0;
                int leftTopX_Column = 0;
                int leftTopY_Column = 0;
                int rightBottomX_Column = 0;
                int rightBottomY_Column = 0;
                int z_Column = 0;
                int png_Column = 0;

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

                                if (row[png_Column] != "")
                                    ledui.PNG_Path = auraCreatorFolderPath + modelName + "\\" + row[png_Column];

                                deviceContent.Leds.Add(ledui);
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
        public async Task<Device> ToDevice()
        {
            return await ToDevice(new Point(0, 0));
        }
        public async Task<Device> ToDevice(Point gridPosition)
        {
            Device device;
            Image img;
            List<LightZone> zones = new List<LightZone>();

            img = new Image
            {
                RenderTransform = new CompositeTransform(),
                Width = Image.PixelWidth,
                Height = Image.PixelHeight,
                Source = this.Image,
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                Stretch = Stretch.Fill,
            };

            for (int idx = 0; idx < this.Leds.Count; idx++)
            {
                LedUI led = this.Leds[idx];
                LightZone lz;

                if (led.PNG_Path == null)
                {
                    lz = new LightZone(led);
                    zones.Add(lz);
                }
                else
                {
                    SpecialLightZone slz = new SpecialLightZone(led);
                    await slz.CreateSpecialFrame(led);
                    zones.Add(slz);
                }
            }

            device = new Device(img, zones.ToArray())
            {
                Name = this.DeviceName,
                Type = this.DeviceType,
                LightZones = zones.ToArray(),
                GridPosition = new Point(gridPosition.X, gridPosition.Y),
                Width = GridWidth,
                Height = GridHeight,
            };

            return device;
        }
    }
}

