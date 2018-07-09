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
using System.Collections.Generic;
using Windows.Graphics.Display;
using Microsoft.Graphics.Canvas.Effects;
using System.Threading.Tasks;

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

    public sealed partial class MainPage : Page
    {
        SoftwareBitmap g704_softwareBitmap;

        public MainPage()
        {
            InitializeComponent();
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            g704_softwareBitmap = await InitialG704Bitmap();
            g704_softwareBitmap = SoftwareBitmap.Convert(g704_softwareBitmap,
                BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(g704_softwareBitmap);
            G704.Source = source;
        }
        private async Task<SoftwareBitmap> InitialG704Bitmap()
        {
            string fname = @"Assets\g704.png";
            SoftwareBitmap softwareBitmap;

            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile printingFile = await InstallationFolder.GetFileAsync(fname);

            using (IRandomAccessStream printingFileStream = await printingFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder printingDecoder = await BitmapDecoder.CreateAsync(printingFileStream);
                softwareBitmap = await printingDecoder.GetSoftwareBitmapAsync();
            }
            return softwareBitmap;
        }

        private void ChangeBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            string content = (string)b.Content;

            if (content == "White")
                GridRow1.Background = new SolidColorBrush(Colors.White);
            else if (content == "Black")
                GridRow1.Background = new SolidColorBrush(Colors.Black);
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

            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".png" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "MyLuaScript";

            StorageFile saveFile = await savePicker.PickSaveFileAsync();

            if (saveFile == null)
                return;

            SaveSoftwareBitmapToFile(g704_softwareBitmap, saveFile);
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

        private async void ImageTurnRed_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            if (b.Name == "ConvertToStraightBefore")
                g704_softwareBitmap = SoftwareBitmap.Convert(g704_softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);

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
            int width = g704_softwareBitmap.PixelWidth;
            int height = g704_softwareBitmap.PixelHeight;

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            ColorRectangle.Fill = MyColorHelper.GetRainbowBrush();
            await renderTargetBitmap.RenderAsync(ColorRectangle, width, height);

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

        private async void ScaleRainbowButton_Click(object sender, RoutedEventArgs e)
        {
            SoftwareBitmap tempSB;
            double scale = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;

            int renderTB_Width = 200;
            int renderTB_Height = 100;
            int newWidth = g704_softwareBitmap.PixelWidth;
            int newHeight = g704_softwareBitmap.PixelHeight;

            // Step 1 :
            // Create RenderTargetBitmap by ColorRectangle.
            // Don't worry about both width and height of ColorRectangle.
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            ColorRectangle.Fill = MyColorHelper.GetRainbowBrush();
            await renderTargetBitmap.RenderAsync(ColorRectangle, renderTB_Width, renderTB_Height);

            // Step 2 :
            // Get IBuffer from RenderTargetBitmap,
            IBuffer ib = await renderTargetBitmap.GetPixelsAsync();

            // Step 3 :
            // Create a temporary SoftwareBitmap which is based on current scale
            tempSB = SoftwareBitmap.CreateCopyFromBuffer(ib, BitmapPixelFormat.Bgra8,
                (int)(renderTB_Width * scale), (int)(renderTB_Height * scale));
            tempSB = SoftwareBitmap.Convert(tempSB, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            // Step 4 :
            // Adjust to the same size as g704 image
            tempSB = Resize(tempSB, newWidth, newHeight);

            // Step 5 :
            // Assign this temp image pixels to g704 image
            g704_softwareBitmap = SoftwareBitmap.Convert(g704_softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            AssignPixelValues(g704_softwareBitmap, tempSB);
            g704_softwareBitmap = SoftwareBitmap.Convert(g704_softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(g704_softwareBitmap);
            G704.Source = source;
        }
        public static SoftwareBitmap Resize(SoftwareBitmap softwareBitmap, float newWidth, float newHeight)
        {
            using (var resourceCreator = CanvasDevice.GetSharedDevice())
            using (var canvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(resourceCreator, softwareBitmap))
            using (var canvasRenderTarget = new CanvasRenderTarget(resourceCreator, newWidth, newHeight, canvasBitmap.Dpi))
            using (var drawingSession = canvasRenderTarget.CreateDrawingSession())
            using (var scaleEffect = new ScaleEffect())
            {
                scaleEffect.Source = canvasBitmap;
                scaleEffect.Scale = new System.Numerics.Vector2(newWidth / softwareBitmap.PixelWidth, newHeight / softwareBitmap.PixelHeight);
                drawingSession.DrawImage(scaleEffect);
                drawingSession.Flush();
                return SoftwareBitmap.CreateCopyFromBuffer(canvasRenderTarget.GetPixelBytes().AsBuffer(), BitmapPixelFormat.Bgra8, (int)newWidth, (int)newHeight, BitmapAlphaMode.Premultiplied);
            }
        }
        private unsafe void AssignPixelValues(SoftwareBitmap newSoftwareBitmap, SoftwareBitmap softwareBitmap)
        {
            using (BitmapBuffer buffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Read))
            using (BitmapBuffer newBuffer = newSoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
            using (var reference = buffer.CreateReference())
            using (var newReference = newBuffer.CreateReference())
            {
                byte* dataInBytes;
                byte* newDataInBytes;
                uint capacity;
                ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacity);
                ((IMemoryBufferByteAccess)newReference).GetBuffer(out newDataInBytes, out capacity);

                // Fill-in the BGRA plane
                BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);

                for (int i = 0; i < bufferLayout.Height; i++)
                {
                    for (int j = 0; j < bufferLayout.Width; j++)
                    {
                        int pixelIndex = bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j;
                        if (newDataInBytes[pixelIndex + 3] != 0)
                        {
                            newDataInBytes[pixelIndex + 0] = dataInBytes[pixelIndex + 0];
                            newDataInBytes[pixelIndex + 1] = dataInBytes[pixelIndex + 1];
                            newDataInBytes[pixelIndex + 2] = dataInBytes[pixelIndex + 2];
                        }
                    }
                }
            }
        }
    }
}
