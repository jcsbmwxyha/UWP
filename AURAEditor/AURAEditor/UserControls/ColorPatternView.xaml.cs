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
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;

// 使用者控制項項目範本記載於 https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class ColorPatternView : UserControl
    {
        static public ColorPatternView Self;
        private ColorPatternModel m_ColorPatternModel { get { return this.DataContext as ColorPatternModel; } }

        private ColorPointModel oldColorPointModel;
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
            Self = this;
            this.DataContextChanged += (s, e) => Bindings.Update();
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
                        new GradientStop
                        {
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

        private void AddColorPointButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPointModel checkedCp = CurrentColorPoints.FirstOrDefault(p => p.IsChecked == true);

            if (checkedCp != null && CurrentColorPoints.Count < 7)
            {
                ColorPointModel newCp = new ColorPointModel();
                newCp.ParentPattern = m_ColorPatternModel;
                int curIndex = CurrentColorPoints.IndexOf(checkedCp);

                if (curIndex == CurrentColorPoints.Count - 1) // last
                {
                    if ((CurrentColorPoints[curIndex].PixelX - CurrentColorPoints[curIndex - 1].PixelX) >= 25)
                    {
                        newCp.PixelX = (CurrentColorPoints[curIndex - 1].PixelX + CurrentColorPoints[curIndex].PixelX) / 2;
                        CurrentColorPoints.Insert(curIndex, newCp);
                    }
                }
                else
                {
                    if ((CurrentColorPoints[curIndex + 1].PixelX - CurrentColorPoints[curIndex].PixelX) >= 25)
                    {
                        newCp.PixelX = (CurrentColorPoints[curIndex].PixelX + CurrentColorPoints[curIndex + 1].PixelX) / 2;
                        CurrentColorPoints.Insert(curIndex + 1, newCp);
                    }
                }

                newCp.Color = checkedCp.Color;
            }

            m_ColorPatternModel.OnManipulationCompleted();
        }
        private void RemoveColorPointButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPointModel checkedCp = CurrentColorPoints.FirstOrDefault(p => p.IsChecked == true);

            if (checkedCp != null && CurrentColorPoints.Count > 2)
            {
                ColorPointModel newCp = new ColorPointModel();
                newCp.ParentPattern = m_ColorPatternModel;
                int curIndex = CurrentColorPoints.IndexOf(checkedCp);

                if (curIndex == CurrentColorPoints.Count - 1) // last
                {
                    CurrentColorPoints[curIndex - 1].IsChecked = true;
                }
                else
                {
                    CurrentColorPoints[curIndex + 1].IsChecked = true;
                }
                oldColorPointModel = checkedCp;
                ReUndoManager.GetInstance().Store(new RemoveColorPointCommand(LayerPage.Self.CheckedEffect, oldColorPointModel, curIndex));
                CurrentColorPoints.Remove(checkedCp);
            }

            m_ColorPatternModel.OnManipulationCompleted();
        }

        public class RemoveColorPointCommand : IReUndoCommand
        {
            private ColorPointModel _oldColorPointValue;
            private int _checkIndex;
            private EffectLineViewModel _checkedEffect;

            public RemoveColorPointCommand(EffectLineViewModel checkedEffect, ColorPointModel oldColorPointValue, int checkIndex)
            {
                _checkedEffect = checkedEffect;
                _checkIndex = checkIndex;
                _oldColorPointValue = oldColorPointValue;
            }

            public void ExecuteRedo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                //Self.CurrentColorPoints[_checkIndex + 1].IsChecked = true;
                Self.CurrentColorPoints.Remove(_oldColorPointValue);
                Self.m_ColorPatternModel.OnManipulationCompleted();
            }

            public void ExecuteUndo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                Self.CurrentColorPoints.Insert(_checkIndex, _oldColorPointValue);
                Self.m_ColorPatternModel.OnManipulationCompleted();
            }
        }

        public class AddColorPointCommand : IReUndoCommand
        {
            private double _oldSppedSliderValue;
            private double _currentSppedSliderValue;
            private EffectLineViewModel _checkedEffect;

            public AddColorPointCommand(EffectLineViewModel checkedEffect, double oldSppedSliderValue, double currentSppedSliderValue)
            {
                _checkedEffect = checkedEffect;
                _oldSppedSliderValue = oldSppedSliderValue;
                _currentSppedSliderValue = currentSppedSliderValue;
            }

            public void ExecuteRedo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.Speed = (int)_currentSppedSliderValue;
            }

            public void ExecuteUndo()
            {
                LayerPage.Self.CheckedEffect = _checkedEffect;
                _checkedEffect.Model.Info.Speed = (int)_oldSppedSliderValue;
            }
        }
    }
}
