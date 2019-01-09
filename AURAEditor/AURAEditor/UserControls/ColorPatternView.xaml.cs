using AuraEditor.Common;
using AuraEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class ColorPatternView : UserControl
    {
        private ColorPatternModel m_ColorPatternModel { get { return this.DataContext as ColorPatternModel; } }
        private ObservableCollection<ColorPointModel> CurrentColorPoints
        {
            get
            {
                return m_ColorPatternModel.CurrentColorPoints;
            }
        }

        public ColorPatternView()
        {
            this.InitializeComponent();
            SetDefaultPatterns();
        }
        public void SetDefaultPatterns()
        {
            for (int i = 0; i < Definitions.DefaultColorPointListCollection.Count; i++)
            {
                LinearGradientBrush pattern = new LinearGradientBrush();
                pattern.StartPoint = new Point(0, 0.5);
                pattern.EndPoint = new Point(1, 0.5);

                for (int num = 0; num < Definitions.DefaultColorPointListCollection[i].Count; num++)
                {
                    pattern.GradientStops.Add(
                        new GradientStop {
                            Color = DefaultColorPointListCollection[i][num].Color,
                            Offset = DefaultColorPointListCollection[i][num].Offset
                        }
                    );
                }

                DefaultPatternMenuFlyout.Items[i].Foreground = pattern;
                SetColorPointBorders(DefaultColorPointListCollection[i]);
            }
        }
        private void DefaultRainbow_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem mf = sender as MenuFlyoutItem;
            m_ColorPatternModel.Selected = (int)Char.GetNumericValue(mf.Name[mf.Name.Length - 1]) - 1;
        }
        private void CustomizeRainbow_Click(object sender, RoutedEventArgs e)
        {
            m_ColorPatternModel.Selected = -1;
        }

        private void PlusItemBt(object sender, RoutedEventArgs e)
        {
            ColorPointModel newColorPointBt = new ColorPointModel();
            AddColorPoint(newColorPointBt);
            //newColorPointBt.UI.OnRedraw += ReDrawMultiPointRectangle;
            ReDrawMultiPointRectangle();
        }
        private void AddColorPoint(ColorPointModel colorPoint)
        {
            int curIndex = 0;
            if (CurrentColorPoints.Count < 7)
            {
                //foreach (var item in CurrentColorPoints)
                //{
                //    List<RadioButton> items = FindAllControl<RadioButton>(item.UI, typeof(RadioButton));

                //    if (items[0].IsChecked == true)
                //    {
                //        curIndex = CurrentColorPoints.IndexOf(item);
                //        if ((curIndex + 1) == CurrentColorPoints.Count)
                //        {
                //            if ((CurrentColorPoints[curIndex].Offset - CurrentColorPoints[curIndex - 1].Offset) < 25)
                //            {
                //                CurrentColorPoints.Add(colorPoint);
                //                CurrentColorPoints.Remove(CurrentColorPoints[CurrentColorPoints.Count - 1]);
                //                return;
                //            }
                //            else
                //            {
                //                colorPoint.Offset = (CurrentColorPoints[curIndex - 1].Offset + CurrentColorPoints[curIndex].Offset) / 2;
                //                CurrentColorPoints.Insert(curIndex, colorPoint);
                //            }
                //        }
                //        else
                //        {
                //            if ((CurrentColorPoints[curIndex + 1].Offset - CurrentColorPoints[curIndex].Offset) < 25)
                //            {
                //                CurrentColorPoints.Add(colorPoint);
                //                CurrentColorPoints.Remove(CurrentColorPoints[CurrentColorPoints.Count - 1]);
                //                return;
                //            }
                //            else
                //            {
                //                colorPoint.Offset = (CurrentColorPoints[curIndex].Offset + CurrentColorPoints[curIndex + 1].Offset) / 2;
                //                CurrentColorPoints.Insert(curIndex + 1, colorPoint);
                //            }
                //        }
                //        colorPoint.Color = item.Color;
                //        //PatternCanvas.Children.Add(colorPoint.UI);
                //        break;
                //    }
                //}
            }
        }
        private void MinusItemBt(object sender, RoutedEventArgs e)
        {
            RemoveColorPoint();
            ReDrawMultiPointRectangle();
        }
        private void RemoveColorPoint()
        {
            if (CurrentColorPoints.Count > 2)
            {
                //foreach (var item in CurrentColorPoints)
                //{
                //    List<RadioButton> items = FindAllControl<RadioButton>(item.UI, typeof(RadioButton));

                //    if (items[0].IsChecked == true)
                //    {
                //        for (int i = 0; i < CurrentColorPoints.Count; i++)
                //        {
                //            if (item == CurrentColorPoints[i])
                //            {
                //                if (i != CurrentColorPoints.Count - 1)
                //                {
                //                    List<RadioButton> items1 = FindAllControl<RadioButton>(CurrentColorPoints[i + 1].UI, typeof(RadioButton));
                //                    items1[0].IsChecked = true;
                //                }
                //                else
                //                {
                //                    List<RadioButton> items1 = FindAllControl<RadioButton>(CurrentColorPoints[i - 1].UI, typeof(RadioButton));
                //                    items1[0].IsChecked = true;
                //                }
                //            }
                //        }
                //        item.UI.OnRedraw -= ReDrawMultiPointRectangle;
                //        CurrentColorPoints.Remove(item);
                //        //PatternCanvas.Children.Remove(item.UI);
                //        break;
                //    }
                //}
            }
        }
        public void ReDrawMultiPointRectangle()
        {
            LinearGradientBrush Pattern = new LinearGradientBrush();
            Pattern.StartPoint = new Point(0, 0.5);
            Pattern.EndPoint = new Point(1, 0.5);

            for (int i = 0; i < CurrentColorPoints.Count; i++)
            {
                Pattern.GradientStops.Add(
                    new GradientStop
                    {
                        Color = CurrentColorPoints[i].Color,
                        Offset = CurrentColorPoints[i].Offset
                    }
                );
            }

            ButtonPolygon.Fill = CustomizePattern.Foreground = PatternRectangle.Fill = Pattern;
            //CustomizeColorPoints = new List<ColorPointModel>(CurrentColorPoints);
            //m_Info.ColorPointList = new List<ColorPointModel>(m_ColorPoints);
            SetColorPointBorders(CurrentColorPoints.ToList());
        }
    }
}
