using AuraEditor.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AuraEditor.Common
{
    public enum MaskType
    {
        None,
        NoSupportDevice,
        NoSyncDevice,
        Editing,
        Playing,
        LiveServiceUpdate,
    }

    public class MaskManager
    {
        static private MaskManager self;

        static public MaskManager GetInstance()
        {
            if (self == null)
            {
                self = new MaskManager();
            }
            return self;
        }

        MaskManager()
        {
        }

        public void ShowMask(MaskType type)
        {
            LayerPage layerPage = LayerPage.Self;
            SpacePage spacePage = SpacePage.Self;
            MainPage mainPage = MainPage.Self;

            switch (type)
            {
                case MaskType.None:
                    mainPage.ActionBarMask.Visibility = Visibility.Collapsed;
                    mainPage.SettingRelativePanel.Visibility = Visibility.Visible;
                    mainPage.LiveServiceUpdatingMask.Visibility = Visibility.Collapsed;
                    mainPage.NoSupportedDeviceGrid.Visibility = Visibility.Collapsed;
                    spacePage.NoSyncDeviceMask.Visibility = Visibility.Collapsed;
                    layerPage.PlayingMask.Visibility = Visibility.Collapsed;
                    mainPage.EditBarRelativePanel.Visibility = Visibility.Collapsed;
                    mainPage.ActionBarRelativePanel.Visibility = Visibility.Visible;
                    mainPage.AvailableButtonMask.Visibility = Visibility.Collapsed;
                    mainPage.FileComboboxMask.Visibility = Visibility.Collapsed;
                    mainPage.EffectBlockMask.Visibility = Visibility.Collapsed;
                    mainPage.EffectInfoMask.Visibility = Visibility.Collapsed;
                    break;
                case MaskType.NoSupportDevice:
                    mainPage.NoSupportedDeviceGrid.Visibility = Visibility.Visible;
                    break;
                case MaskType.NoSyncDevice:
                    spacePage.NoSyncDeviceMask.Visibility = Visibility.Visible;
                    break;
                case MaskType.Editing:
                    mainPage.SettingRelativePanel.Visibility = Visibility.Collapsed;
                    layerPage.PlayingMask.Visibility = Visibility.Visible;
                    Canvas.SetZIndex(layerPage.PlayingMask, 5);
                    mainPage.EditBarRelativePanel.Visibility = Visibility.Visible;
                    mainPage.ActionBarRelativePanel.Visibility = Visibility.Collapsed;
                    mainPage.AvailableButtonMask.Visibility = Visibility.Visible;
                    mainPage.FileComboboxMask.Visibility = Visibility.Visible;
                    mainPage.EffectBlockMask.Visibility = Visibility.Visible;
                    mainPage.EffectInfoMask.Visibility = Visibility.Visible;
                    break;
                case MaskType.Playing:
                    mainPage.ActionBarMask.Visibility = Visibility.Visible;
                    layerPage.PlayingMask.Visibility = Visibility.Visible;
                    Canvas.SetZIndex(layerPage.PlayingMask, 3);
                    mainPage.EditBarRelativePanel.Visibility = Visibility.Collapsed;
                    mainPage.ActionBarRelativePanel.Visibility = Visibility.Visible;
                    mainPage.AvailableButtonMask.Visibility = Visibility.Visible;
                    mainPage.FileComboboxMask.Visibility = Visibility.Visible;
                    mainPage.EffectBlockMask.Visibility = Visibility.Visible;
                    mainPage.EffectInfoMask.Visibility = Visibility.Visible;
                    break;
                case MaskType.LiveServiceUpdate:
                    mainPage.LiveServiceUpdatingMask.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
