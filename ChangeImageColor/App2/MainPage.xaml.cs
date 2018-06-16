using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Microsoft.Graphics.Canvas;
using Windows.Graphics.DirectX.Direct3D11;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404

namespace App2
{
    [ComImport]
    [Guid("5b0d3235-4dba-4d44-865e-8f1d0e4fd04d")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SoftwareBitmap g704_softwareBitmap;
        
        public MainPage()
        {
            this.InitializeComponent();
            InitialG704Bitmap();
        }
        private async void InitialG704Bitmap()
        {
            string fname = @"Assets\g704.png";
            Image resultImg = new Image();
            SoftwareBitmap softwareBitmap;

            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile printingFile = await InstallationFolder.GetFileAsync(fname);

            using (IRandomAccessStream printingFileStream = await printingFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder printingDecoder = await BitmapDecoder.CreateAsync(printingFileStream);
                softwareBitmap = await printingDecoder.GetSoftwareBitmapAsync();
            }
            g704_softwareBitmap = softwareBitmap;
        }

        private void UpdateEventLog(string s)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            eventLog.TextWrapping = TextWrapping.Wrap;
            run.Text = s;
            paragraph.Inlines.Add(run);
            eventLog.Blocks.Insert(0, paragraph);
        }
        private void BackgroundTurnWhite_Click(object sender, RoutedEventArgs e)
        {
            GridRow1.Background = new SolidColorBrush(Colors.White);
        }
        private void BackgroundTurnBlack_Click(object sender, RoutedEventArgs e)
        {
            GridRow1.Background = new SolidColorBrush(Colors.Black);
        }

        private async void TurnRed_Click(object sender, RoutedEventArgs e)
        {
            g704_softwareBitmap = SoftwareBitmap.Convert(g704_softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            SoftwareBitmapChangeColorRed(g704_softwareBitmap);
            g704_softwareBitmap = SoftwareBitmap.Convert(g704_softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(g704_softwareBitmap);
            G704.Source = source;
        }
        private async void TurnRedNoConvert_Click(object sender, RoutedEventArgs e)
        {
            SoftwareBitmapChangeColorRed(g704_softwareBitmap);
            g704_softwareBitmap = SoftwareBitmap.Convert(g704_softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(g704_softwareBitmap);
            G704.Source = source;
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
        private async void RainbowButton_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            ColorRectangle.Fill = GetRainbowBrush();
            await renderTargetBitmap.RenderAsync(ColorRectangle, 944, 412);

            g704_softwareBitmap = SoftwareBitmap.Convert(g704_softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            IBuffer ib = await renderTargetBitmap.GetPixelsAsync();
            byte[] pixel_array = ib.ToArray();
            ApplyRainbow(g704_softwareBitmap, pixel_array);
            g704_softwareBitmap = SoftwareBitmap.Convert(g704_softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(g704_softwareBitmap);
            G704.Source = source;
        }
        private unsafe void ApplyRainbow(SoftwareBitmap softwareBitmap, byte[] pixel_array)
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
                                dataInBytes[pixelIndex + 0] = (byte)pixel_array[pixelIndex + 0];
                                dataInBytes[pixelIndex + 1] = (byte)pixel_array[pixelIndex + 1];
                                dataInBytes[pixelIndex + 2] = (byte)pixel_array[pixelIndex + 2];
                            }
                        }
                    }
                }
            }
        }
        public static Brush GetRainbowBrush()
        {
            LinearGradientBrush lgb = new LinearGradientBrush();
            GradientStopCollection gradientStops = new GradientStopCollection();
            GradientStop stop1 = new GradientStop();
            GradientStop stop2 = new GradientStop();
            GradientStop stop3 = new GradientStop();
            GradientStop stop4 = new GradientStop();
            GradientStop stop5 = new GradientStop();
            GradientStop stop6 = new GradientStop();

            stop1.Color = Windows.UI.Colors.Red;
            stop2.Color = Windows.UI.Colors.Yellow;
            stop3.Color = Windows.UI.Colors.LightGreen;
            stop4.Color = Windows.UI.Colors.Aqua;
            stop5.Color = Windows.UI.Colors.Blue;
            stop6.Color = Windows.UI.Colors.Purple;
            stop1.Offset = 0.1;
            stop2.Offset = 0.25;
            stop3.Offset = 0.4;
            stop4.Offset = 0.6;
            stop5.Offset = 0.75;
            stop6.Offset = 0.9;
            gradientStops.Add(stop1);
            gradientStops.Add(stop2);
            gradientStops.Add(stop3);
            gradientStops.Add(stop4);
            gradientStops.Add(stop5);
            gradientStops.Add(stop6);
            lgb.GradientStops = gradientStops;
            lgb.StartPoint = new Point(0, 0);
            lgb.EndPoint = new Point(1, 0);
            return lgb;
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

            g704_softwareBitmap = softwareBitmap;
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(softwareBitmap);
            G704.Source = source;
        }
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (g704_softwareBitmap == null)
                return;

            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            var outputFile = await fileOpenPicker.PickSingleFileAsync();

            if (outputFile == null)
                return;
            
            SaveSoftwareBitmapToFile(g704_softwareBitmap, outputFile);
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
                encoder.BitmapTransform.ScaledWidth = 944;
                encoder.BitmapTransform.ScaledHeight = 412;
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
