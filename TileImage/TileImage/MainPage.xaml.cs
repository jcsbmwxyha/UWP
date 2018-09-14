using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404

namespace TileImage
{
    [ComImport]
    [Guid("5b0d3235-4dba-4d44-865e-8f1d0e4fd04d")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    public sealed partial class MainPage : Page
    {
        SoftwareBitmap g_BaseSB;
        SoftwareBitmap g_TileSB;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Load_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            var inputFile = await fileOpenPicker.PickSingleFileAsync();

            if (inputFile == null)
                return;

            SoftwareBitmap softwareBitmap;

            using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.PngDecoderId, stream);

                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }

            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            g_BaseSB = softwareBitmap;
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(softwareBitmap);
            BaseImage.Source = source;
        }
        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            if (g_BaseSB == null)
                return;

            int width = g_BaseSB.PixelWidth * Int32.Parse(WidthTextBox.Text);
            int height = g_BaseSB.PixelHeight * Int32.Parse(HeightTextBox.Text);

            g_TileSB = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height);
            g_TileSB = SoftwareBitmap.Convert(g_TileSB, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            Tile();
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(g_TileSB);
            TileImage.Source = source;
        }
        private unsafe void Tile()
        {
            using (BitmapBuffer sourceBuffer = g_BaseSB.LockBuffer(BitmapBufferAccessMode.Read))
            using (BitmapBuffer targetBuffer = g_TileSB.LockBuffer(BitmapBufferAccessMode.Write))
            using (var reference = sourceBuffer.CreateReference())
            using (var newReference = targetBuffer.CreateReference())
            {
                byte* dataInBytes;
                byte* newDataInBytes;
                uint capacity;
                ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacity);
                ((IMemoryBufferByteAccess)newReference).GetBuffer(out newDataInBytes, out capacity);

                // Fill-in the BGRA plane
                BitmapPlaneDescription sourceBufferLayout = sourceBuffer.GetPlaneDescription(0);
                BitmapPlaneDescription targetBufferLayout = targetBuffer.GetPlaneDescription(0);
                int baseWidth = g_BaseSB.PixelWidth;
                int baseHeight = g_BaseSB.PixelHeight;
                int tileWidth = targetBufferLayout.Width;
                int tileHeight = targetBufferLayout.Height;

                for (int i = 0; i < tileHeight; i++)
                {
                    for (int j = 0; j < tileWidth; j++)
                    {
                        int wSourceIndex = i % baseWidth;
                        int hSourceIndex = j % baseHeight;

                        int sourcePixelIndex = sourceBufferLayout.StartIndex + sourceBufferLayout.Stride * wSourceIndex + 4 * hSourceIndex;
                        int targetPixelIndex = targetBufferLayout.StartIndex + targetBufferLayout.Stride * i + 4 * j;

                        newDataInBytes[targetPixelIndex + 0] = dataInBytes[sourcePixelIndex + 0];
                        newDataInBytes[targetPixelIndex + 1] = dataInBytes[sourcePixelIndex + 1];
                        newDataInBytes[targetPixelIndex + 2] = dataInBytes[sourcePixelIndex + 2];
                        newDataInBytes[targetPixelIndex + 3] = dataInBytes[sourcePixelIndex + 3];
                    }
                }
            }
        }
        private unsafe void SoftwareBitmapChangeColorRed(SoftwareBitmap softwareBitmap)
        {
            using (BitmapBuffer buffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
            {
                using (var reference = buffer.CreateReference())
                {
                    byte* dataInBytes;
                    uint capacity;
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacity);

                    // Fill-in the BGRA plane
                    BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);

                    for (int i = 0; i < bufferLayout.Height; i++)
                    {
                        for (int j = 0; j < bufferLayout.Width; j++)
                        {
                            int pixelIndex = bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j;
                            if (dataInBytes[pixelIndex + 3] != 0)
                            {
                                dataInBytes[pixelIndex + 0] = 0;
                                dataInBytes[pixelIndex + 1] = 0;
                                dataInBytes[pixelIndex + 2] = 255;
                            }
                        }
                    }
                }
            }
        }
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (g_TileSB == null)
                return;

            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".png" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "MyTileImage";

            StorageFile saveFile = await savePicker.PickSaveFileAsync();

            if (saveFile == null)
                return;

            SaveSoftwareBitmapToFile(g_TileSB, saveFile);
        }
        private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                // Set the software bitmap
                encoder.SetSoftwareBitmap(softwareBitmap);

                // Set additional encoding parameters, if needed
                encoder.BitmapTransform.ScaledWidth = (uint)softwareBitmap.PixelWidth;
                encoder.BitmapTransform.ScaledHeight = (uint)softwareBitmap.PixelHeight;
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
                encoder.IsThumbnailGenerated = true;

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    switch (err.HResult)
                    {
                        case unchecked((int)0x88982F81): //WINCODEC_ERR_UNSUPPORTEDOPERATION
                                                         // If the encoder does not support writing a thumbnail, then try again
                                                         // but disable thumbnail generation.
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw err;
                    }
                }

                if (encoder.IsThumbnailGenerated == false)
                {
                    await encoder.FlushAsync();
                }
            }
        }
    }
}
