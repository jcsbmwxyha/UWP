using AuraEditor.Common;
using AuraEditor.Models;
using AuraEditor.Pages;
using AuraEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
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
            this.DataContextChanged += (s, e) => Bindings.Update();
            SetDefaultPatterns();
        }
        private void ColorPatternView_Unloaded(object sender, RoutedEventArgs e)
        {
            Bindings.StopTracking();
        }

        public void SetDefaultPatterns()
        {
            for (int i = 0; i < Definitions.DefaultColorPointListCollection.Count; i++)
            {
                LinearGradientBrush patternBursh = new LinearGradientBrush();
                patternBursh.StartPoint = new Point(0, 0.5);
                patternBursh.EndPoint = new Point(1, 0.5);

                for (int num = 0; num < Definitions.DefaultColorPointListCollection[i].Count; num++)
                {
                    patternBursh.GradientStops.Add(
                        new GradientStop
                        {
                            Color = DefaultColorPointListCollection[i][num].Color,
                            Offset = DefaultColorPointListCollection[i][num].Offset
                        }
                    );
                }

                DefaultPatternMenuFlyout.Items[i].Foreground = patternBursh;
            }
        }

        private void DefaultRainbow_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem mf = sender as MenuFlyoutItem;

            var pattern = ColorPatternModel.Self;
            var oldSelect = pattern.Selected;
            var newSelect = (int)Char.GetNumericValue(mf.Name[mf.Name.Length - 1]) - 1;

            m_ColorPatternModel.Selected = newSelect;

            ReUndoManager.Store(new ColorPatternModifyCommand(m_ColorPatternModel.Info, null, null, oldSelect, newSelect));
        }
        private void CustomizeRainbow_Click(object sender, RoutedEventArgs e)
        {
            var pattern = ColorPatternModel.Self;
            var oldSelect = pattern.Selected;

            m_ColorPatternModel.Selected = -1;

            ReUndoManager.Store(new ColorPatternModifyCommand(m_ColorPatternModel.Info, null, null, oldSelect, -1));
        }

        private void AddColorPointButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPointModel checkedCp = CurrentColorPoints.FirstOrDefault(p => p.IsChecked == true);

            if (checkedCp != null && CurrentColorPoints.Count < 7)
            {
                int checkedIndex = CurrentColorPoints.IndexOf(checkedCp);
                int insertIndex;
                double insertX;

                if (checkedIndex == CurrentColorPoints.Count - 1) // last
                {
                    insertIndex = checkedIndex;
                    insertX = (checkedCp.PixelX + CurrentColorPoints[checkedIndex - 1].PixelX) / 2;
                }
                else
                {
                    insertIndex = checkedIndex + 1;
                    insertX = (checkedCp.PixelX + CurrentColorPoints[checkedIndex + 1].PixelX) / 2;
                }

                if (Math.Abs(insertX - checkedCp.PixelX) > 12)
                {
                    ColorPointModel newCp = new ColorPointModel
                    {
                        PixelX = insertX,
                        Color = checkedCp.Color,
                    };
                    CurrentColorPoints.Insert(insertIndex, newCp);
                    m_ColorPatternModel.OnCustomizeChanged();
                    newCp.IsChecked = true;
                }
            }
        }

        private void RemoveColorPointButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPointModel checkedCp = CurrentColorPoints.FirstOrDefault(p => p.IsChecked == true);

            if (checkedCp != null && CurrentColorPoints.Count > 2)
            {
                int curIndex = CurrentColorPoints.IndexOf(checkedCp);
                int needCheckedIndex;

                if (curIndex == CurrentColorPoints.Count - 1) // last
                {
                    needCheckedIndex = curIndex - 1;
                }
                else
                {
                    needCheckedIndex = curIndex;
                }

                CurrentColorPoints.Remove(checkedCp);
                m_ColorPatternModel.OnCustomizeChanged();
                CurrentColorPoints[needCheckedIndex].IsChecked = true;
            }
        }

        public class ColorPatternModifyCommand : IReUndoCommand
        {
            private EffectInfoModel _info;
            List<ColorPointLightData> _newlist;
            List<ColorPointLightData> _oldlist;
            private int _oldSelect;
            private int _newSelect;
            private EffectLineViewModel _commonEff = null;

            public ColorPatternModifyCommand(EffectInfoModel info, List<ColorPointLightData> oldlist, List<ColorPointLightData> newlist, int oldSelect, int newSelect)
            {
                _info = info;
                _oldlist = oldlist;
                _newlist = newlist;
                _oldSelect = oldSelect;
                _newSelect = newSelect;

                if (IsCommonEffect(info.Name))
                    _commonEff = LayerPage.Self.CheckedEffect;
            }

            public void ExecuteRedo()
            {
                if (_newlist != null)
                {
                    var cusList = _info.CustomizedPattern;
                    cusList.Clear();

                    foreach (var data in _newlist)
                    {
                        cusList.Add(data.ToModel());
                    }
                    SetColorPointBorders(cusList);
                }

                _info.PatternSelect = _newSelect;

                if (_commonEff != null)
                {
                    LayerPage.Self.CheckedEffect = _commonEff;
                    var pm = ColorPatternModel.Self;
                    pm.Selected = _newSelect;
                    pm.RefreshCPs();
                }
            }

            public void ExecuteUndo()
            {
                if (_oldlist != null)
                {
                    var cusList = _info.CustomizedPattern;
                    cusList.Clear();

                    foreach (var data in _oldlist)
                    {
                        cusList.Add(data.ToModel());
                    }
                    SetColorPointBorders(cusList);
                }

                _info.PatternSelect = _oldSelect;

                if (_commonEff != null)
                {
                    LayerPage.Self.CheckedEffect = _commonEff;
                    var pm = ColorPatternModel.Self;
                    pm.Selected = _newSelect;
                    pm.RefreshCPs();
                }
            }
        }
    }
}
