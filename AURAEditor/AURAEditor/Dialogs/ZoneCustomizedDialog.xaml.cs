using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using AuraEditor.Common;
using Windows.UI.Input;
using Windows.UI.Xaml.Media.Imaging;
using Point = Windows.Foundation.Point;
using Rect = Windows.Foundation.Rect;
using Color = Windows.UI.Color;
using Colors = Windows.UI.Colors;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class ZoneCustomizedDialog : ContentDialog
    {
        AddDeviceDialog addDeviceDialog;
        Array myRectangleArray;
        MouseEventCtrl myMouseEventCtrl;
        DeviceItem _deviceItem;

        public ZoneCustomizedDialog(DeviceItem di)
        {
            this.InitializeComponent();
            addDeviceDialog = AddDeviceDialog.GetInstance();
            _deviceItem = di;
            myMouseEventCtrl = IntializeMouseEventCtrl();

            //myMouseEventCtrl.SetRegions(regionRects.ToArray(), statusChangedCallBack);
            DeviceImage.Source = new BitmapImage(new Uri(di.Image));
        }

        private MouseEventCtrl IntializeMouseEventCtrl()
        {
            myRectangleArray = Array.CreateInstance(typeof(Rectangle), Constants.MAX_KEYS);
            MouseEventCtrl.StatusChangedCallBack statusChangedCallBack =
                new MouseEventCtrl.StatusChangedCallBack(Region_StatusChanged);

            int[,] lightRegions = _deviceItem.LightRegions;

            List<Region> regions = new List<Region>();
            for (int i = 0; i < lightRegions.GetLength(0); i++)
            {
                Point p1 = new Point(lightRegions[i, 0], lightRegions[i, 1]);
                Point p2 = new Point(lightRegions[i, 2], lightRegions[i, 3]);
                Rect rect = new Rect(p1, p2);
                Region r = new Region()
                {
                    Index = i,
                    MyRect = new Rect(p1, p2),
                    Hover = false,
                    Selected = false,
                    Callback = statusChangedCallBack
                };
                regions.Add(r);

                Rectangle rectangle = CreateRectangle(rect);
                DialogGrid.Children.Add(rectangle);
                myRectangleArray.SetValue(rectangle, i);
            }

            int[] selectedIndexes = _deviceItem.SelectedIndexes;
            if (selectedIndexes != null)
            {
                for (int i = 0; i < selectedIndexes.Length; i++)
                {
                    int index = selectedIndexes[i];
                    Rectangle rectangle = (Rectangle)myRectangleArray.GetValue(index);
                    rectangle.Stroke = new SolidColorBrush(Colors.Red);
                    regions[index].Selected = true;
                }
            }

            MouseEventCtrl mec = new MouseEventCtrl
            {
                MonitorMaxRect = new Rect(new Point(0, 0), new Point(942, 471)),
                MyRegions = regions.ToArray()
            };

            return mec;
        }

        private void ZoneCustomizedDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // save selected indexes before show name dialog
            _deviceItem.SelectedIndexes = myMouseEventCtrl.GetSelectedIndexes();
            this.Closed += ShowAddDeviceDialog;
        }

        private void ZoneCustomizedDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Closed += ShowAddDeviceDialog;
        }

        private async void ShowAddDeviceDialog(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            this.Closed -= ShowAddDeviceDialog;
            AddDeviceDialog addDeviceDialog = AddDeviceDialog.GetInstance();
            var result = await addDeviceDialog.ShowAsync();
        }

        private void Image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            Point Position = e.GetCurrentPoint(fe).Position;
            myMouseEventCtrl.OnMousePressed(Position);
            bool _hasCapture = fe.CapturePointer(e.Pointer);
        }

        private void Image_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            PointerPoint ptrPt = e.GetCurrentPoint(fe);
            Point Position = ptrPt.Position;

            if (ptrPt.Properties.IsLeftButtonPressed)
            {
                myMouseEventCtrl.OnMouseMoved(Position, true);
                bool _hasCapture = fe.CapturePointer(e.Pointer);
            }
            else
            {
                myMouseEventCtrl.OnMouseMoved(Position, false);
                //UpdateEventLog("no need to draw mouse rectangle");
                return; // no need to draw mouse rectangle
            }

            // Draw mouse rectangle
            Rect r = myMouseEventCtrl.MouseRect;
            CompositeTransform ct = mouseRectangle.RenderTransform as CompositeTransform;

            ct.TranslateX = r.X;
            ct.TranslateY = r.Y;
            mouseRectangle.Width = r.Width;
            mouseRectangle.Height = r.Height;
        }

        private void Image_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Rect r = myMouseEventCtrl.MouseRect;
            UpdateEventLog(r.X.ToString() + "," + r.Y.ToString() + "," + (r.X + r.Width).ToString() + "," + (r.Y + r.Height).ToString());
            myMouseEventCtrl.OnMouseReleased();
            mouseRectangle.Width = 0;
            mouseRectangle.Height = 0;
        }
        
        private void Image_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            myMouseEventCtrl.OnRightTapped();
        }

        private Rectangle CreateRectangle(Windows.Foundation.Rect Rect)
        {
            CompositeTransform ct = new CompositeTransform
            {
                TranslateX = Rect.X,
                TranslateY = Rect.Y
            };

            Rectangle rectangle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Transparent),
                StrokeThickness = 3,
                RenderTransform = ct,
                Width = Rect.Width,
                Height = Rect.Height,
                HorizontalAlignment = 0,
                VerticalAlignment = 0,
                RadiusX=3,
                RadiusY=4
            };

            rectangle.Stroke = new SolidColorBrush(Colors.Black);

            return rectangle;
        }

        private void Region_StatusChanged(int regionIndex, int status)
        {
            Rectangle rectangle = (Rectangle)myRectangleArray.GetValue(regionIndex);

            if (status == RegionStatus.Normal)
            {
                rectangle.Stroke = new SolidColorBrush(Colors.Black);
                rectangle.Fill = new SolidColorBrush(Colors.Transparent);
            }
            else if (status == RegionStatus.NormalHover)
            {
                rectangle.Stroke = new SolidColorBrush(Colors.Black);
                rectangle.Fill = new SolidColorBrush(new Color { A = 100, R = 0, G = 0, B = 123 });
            }
            else if (status == RegionStatus.Selected)
            {
                rectangle.Stroke = new SolidColorBrush(Colors.Red);
                rectangle.Fill = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                UpdateEventLog("Region_StatusChanged" + regionIndex.ToString());
                rectangle.Stroke = new SolidColorBrush(Colors.Red);
                rectangle.Fill = new SolidColorBrush(new Color { A = 100, R = 0, G = 0, B = 123 });
            }

            //UpdateEventLog("Region_StatusChanged" + regionIndex.ToString() + ", " + status.ToString());
        }

        public void UpdateEventLog(string s)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            DialogLogBlock.TextWrapping = TextWrapping.Wrap;
            run.Text = s;
            paragraph.Inlines.Add(run);
            DialogLogBlock.Blocks.Insert(0, paragraph);
        }
    }
}
