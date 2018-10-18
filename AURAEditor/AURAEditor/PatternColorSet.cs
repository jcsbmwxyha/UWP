using AuraEditor.UserControls;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.AuraEditorColorHelper;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        public List<ColorPoint> DefaultColorPoints1 = new List<ColorPoint>();
        public List<ColorPoint> DefaultColorPoints2 = new List<ColorPoint>();
        public List<ColorPoint> DefaultColorPoints3 = new List<ColorPoint>();
        public List<ColorPoint> DefaultColorPoints4 = new List<ColorPoint>();
        public List<ColorPoint> DefaultColorPoints5 = new List<ColorPoint>();
        public List<ColorPoint> DefaultColorPoints6 = new List<ColorPoint>();
        public List<List<ColorPoint>> DefaultColorList = new List<List<ColorPoint>>();
        public List<List<ColorPoint>> DefaultColorList1 = new List<List<ColorPoint>>();
        public List<ColorPoint> CustomizeColorPoints = new List<ColorPoint>();

        int width = 13;

        public void SetDefaultPatternList()
        {
            ColorPoint cp11 = new ColorPoint();
            ColorPoint cp12 = new ColorPoint();
            ColorPoint cp21 = new ColorPoint();
            ColorPoint cp22 = new ColorPoint();
            ColorPoint cp23 = new ColorPoint();
            ColorPoint cp31 = new ColorPoint();
            ColorPoint cp32 = new ColorPoint();
            ColorPoint cp33 = new ColorPoint();
            ColorPoint cp34 = new ColorPoint();
            ColorPoint cp41 = new ColorPoint();
            ColorPoint cp42 = new ColorPoint();
            ColorPoint cp43 = new ColorPoint();
            ColorPoint cp44 = new ColorPoint();
            ColorPoint cp45 = new ColorPoint();
            ColorPoint cp51 = new ColorPoint();
            ColorPoint cp52 = new ColorPoint();
            ColorPoint cp53 = new ColorPoint();
            ColorPoint cp54 = new ColorPoint();
            ColorPoint cp55 = new ColorPoint();
            ColorPoint cp56 = new ColorPoint();
            ColorPoint cp61 = new ColorPoint();
            ColorPoint cp62 = new ColorPoint();
            ColorPoint cp63 = new ColorPoint();
            ColorPoint cp64 = new ColorPoint();
            ColorPoint cp65 = new ColorPoint();
            ColorPoint cp66 = new ColorPoint();
            ColorPoint cp67 = new ColorPoint();
            
            cp11.Color = HexToColor("#FFFFFFFF");
            cp11.Offset = 0.0;
            cp12.Color = HexToColor("#FF4E4E4E");
            cp12.Offset = 1.0;
            DefaultColorPoints1.Add(cp11);
            DefaultColorPoints1.Add(cp12);
            DefaultColorList.Add(DefaultColorPoints1);

            cp21.Color = HexToColor("#FFFEBE3F");
            cp21.Offset = 0.0;
            cp22.Color = HexToColor("#FFFE3F7D");
            cp22.Offset = 0.5;
            cp23.Color = HexToColor("#FFF91D1D");
            cp23.Offset = 1.0;
            DefaultColorPoints2.Add(cp21);
            DefaultColorPoints2.Add(cp22);
            DefaultColorPoints2.Add(cp23);
            DefaultColorList.Add(DefaultColorPoints2);

            cp31.Color = HexToColor("#FFD1FE3F");
            cp31.Offset = 0.0;
            cp32.Color = HexToColor("#FF00DCFF");
            cp32.Offset = 0.33;
            cp33.Color = HexToColor("#FF00DCFF");
            cp33.Offset = 0.66;
            cp34.Color = HexToColor("#FFD1FE3F");
            cp34.Offset = 1.0;
            DefaultColorPoints3.Add(cp31);
            DefaultColorPoints3.Add(cp32);
            DefaultColorPoints3.Add(cp33);
            DefaultColorPoints3.Add(cp34);
            DefaultColorList.Add(DefaultColorPoints3);
            
            cp41.Color = HexToColor("#FFF1FF00");
            cp41.Offset = 0.0;
            cp42.Color = HexToColor("#FFFFB500");
            cp42.Offset = 0.25;
            cp43.Color = HexToColor("#FFF1FF00");
            cp43.Offset = 0.5;
            cp44.Color = HexToColor("#FFFFB500");
            cp44.Offset = 0.75;
            cp45.Color = HexToColor("#FFF1FF00");
            cp45.Offset = 1.0;
            DefaultColorPoints4.Add(cp41);
            DefaultColorPoints4.Add(cp42);
            DefaultColorPoints4.Add(cp43);
            DefaultColorPoints4.Add(cp44);
            DefaultColorPoints4.Add(cp45);
            DefaultColorList.Add(DefaultColorPoints4);
            
            cp51.Color = HexToColor("#FFFF0091");
            cp51.Offset = 0.0;
            cp52.Color = HexToColor("#FF8C00FF");
            cp52.Offset = 0.2;
            cp53.Color = HexToColor("#FF4B00D9");
            cp53.Offset = 0.4;
            cp54.Color = HexToColor("#FF4B00D9");
            cp54.Offset = 0.6;
            cp55.Color = HexToColor("#FF8C00FF");
            cp55.Offset = 0.8;
            cp56.Color = HexToColor("#FFFF0091");
            cp56.Offset = 1.0;
            DefaultColorPoints5.Add(cp51);
            DefaultColorPoints5.Add(cp52);
            DefaultColorPoints5.Add(cp53);
            DefaultColorPoints5.Add(cp54);
            DefaultColorPoints5.Add(cp55);
            DefaultColorPoints5.Add(cp56);
            DefaultColorList.Add(DefaultColorPoints5);

            cp61.Color = HexToColor("#FFFF000D");
            cp61.Offset = 0.0;
            cp62.Color = HexToColor("#FFF500FF");
            cp62.Offset = 0.16;
            cp63.Color = HexToColor("#FF0006FF");
            cp63.Offset = 0.32;
            cp64.Color = HexToColor("#FF00FAFF");
            cp64.Offset = 0.48;
            cp65.Color = HexToColor("#FF01FF00");
            cp65.Offset = 0.64;
            cp66.Color = HexToColor("#FFFFF600");
            cp66.Offset = 0.8;
            cp67.Color = HexToColor("#FFFF000D");
            cp67.Offset = 1.0;
            DefaultColorPoints6.Add(cp61);
            DefaultColorPoints6.Add(cp62);
            DefaultColorPoints6.Add(cp63);
            DefaultColorPoints6.Add(cp64);
            DefaultColorPoints6.Add(cp65);
            DefaultColorPoints6.Add(cp66);
            DefaultColorPoints6.Add(cp67);
            DefaultColorList.Add(DefaultColorPoints6);

            foreach(var item in DefaultColorList)
            {
                SetListBorder(item);
            }
        }

        public List<List<ColorPoint>> CallDefaultList()
        {
            return DefaultColorList;
        }
        public void SetDefaultPattern()
        {
            DefaultColorList1 = CallDefaultList();
            CustomizeRainbow.Foreground = new SolidColorBrush(Colors.White);

            //Button Color
            LinearGradientBrush Pattern1 = new LinearGradientBrush();
            Pattern1.StartPoint = new Point(0, 0.5);
            Pattern1.EndPoint = new Point(1, 0.5);
            for(int i = 0; i < DefaultColorList[0].Count; i++)
            {
                Pattern1.GradientStops.Add(new GradientStop { Color = DefaultColorList1[0][i].Color, Offset = DefaultColorList1[0][i].Offset});
            }

            // Use the brush to paint the rectangle.
            PatternButton.Background = MultiPointRectangle.Fill = DefaultRainbow1.Foreground = Pattern1;
            foreach (var item in DefaultColorList[0])
            {
                ColorPoints.Add(new ColorPoint(item));
            }

            MainPage.Self.ShowColorPointUI(ColorPoints);

            // Button Color  
            LinearGradientBrush Pattern2 = new LinearGradientBrush();
            Pattern2.StartPoint = new Point(0, 0.5);
            Pattern2.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < DefaultColorList[1].Count; i++)
            {
                Pattern2.GradientStops.Add(new GradientStop { Color = DefaultColorList1[1][i].Color, Offset = DefaultColorList1[1][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow2.Foreground = Pattern2;

            // Button Color  
            LinearGradientBrush Pattern3 = new LinearGradientBrush();
            Pattern3.StartPoint = new Point(0, 0.5);
            Pattern3.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < DefaultColorList[2].Count; i++)
            {
                Pattern3.GradientStops.Add(new GradientStop { Color = DefaultColorList1[2][i].Color, Offset = DefaultColorList1[2][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow3.Foreground = Pattern3;

            // Button Color  
            LinearGradientBrush Pattern4 = new LinearGradientBrush();
            Pattern4.StartPoint = new Point(0, 0.5);
            Pattern4.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < DefaultColorList[3].Count; i++)
            {
                Pattern4.GradientStops.Add(new GradientStop { Color = DefaultColorList1[3][i].Color, Offset = DefaultColorList1[3][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow4.Foreground = Pattern4;

            // Button Color  
            LinearGradientBrush Pattern5 = new LinearGradientBrush();
            Pattern5.StartPoint = new Point(0, 0.5);
            Pattern5.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < DefaultColorList[4].Count; i++)
            {
                Pattern5.GradientStops.Add(new GradientStop { Color = DefaultColorList1[4][i].Color, Offset = DefaultColorList1[4][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow5.Foreground = Pattern5;

            // Button Color  
            LinearGradientBrush Pattern6 = new LinearGradientBrush();
            Pattern6.StartPoint = new Point(0, 0.5);
            Pattern6.EndPoint = new Point(1, 0.5);
            for (int i = 0; i < DefaultColorList[5].Count; i++)
            {
                Pattern6.GradientStops.Add(new GradientStop { Color = DefaultColorList1[5][i].Color, Offset = DefaultColorList1[5][i].Offset });
            }

            // Use the brush to paint the rectangle.
            DefaultRainbow6.Foreground = Pattern6;
        }

        public void ReDrawMultiPointRectangle()
        {
            EffectInfo ui = SelectedEffectLine.Info;
            LinearGradientBrush Pattern = new LinearGradientBrush();
            Pattern.StartPoint = new Point(0, 0.5);
            Pattern.EndPoint = new Point(1, 0.5);

            for (int i = 0; i < ColorPoints.Count; i++)
            {
                Pattern.GradientStops.Add(new GradientStop { Color = ColorPoints[i].Color, Offset = ColorPoints[i].Offset });
            }

            PatternButton.Background = CustomizeRainbow.Foreground = MultiPointRectangle.Fill = Pattern;
            CustomizeColorPoints = new List<ColorPoint>(ColorPoints);
            ui.ColorPointList = new List<ColorPoint>(ColorPoints);
            SetListBorder(ColorPoints);
        }

        public void SetListBorder(List<ColorPoint> cps)
        {
            for (int i = 0; i < cps.Count; i++)
            {
                if (i == 0)
                {
                    cps[i].UI.LeftBorder = 0;
                    cps[i].UI.RightBorder = cps[i + 1].UI.X - width;
                }
                else if (i == (cps.Count - 1))
                {
                    cps[i].UI.LeftBorder = cps[i - 1].UI.X + width;
                    cps[i].UI.RightBorder = 180;
                }
                else
                {
                    cps[i].UI.LeftBorder = cps[i - 1].UI.X + width;
                    cps[i].UI.RightBorder = cps[i + 1].UI.X - width;
                }
            }
        }
    }
}