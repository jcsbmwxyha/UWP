using AuraEditor.Models;
using AuraEditor.Pages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.EffectHelper;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class TriggerDialog : ContentDialog
    {
        private LayerModel m_Layer;
        private ObservableCollection<TriggerEffect> m_EffectList;

        public TriggerDialog(LayerModel layer)
        {
            this.InitializeComponent();

            m_Layer = layer;
            TriggerActionButton.Content = m_Layer.TriggerAction;

            m_EffectList = new ObservableCollection<TriggerEffect>();

            if (m_Layer.TriggerEffects != null)
            {
                foreach (var e in m_Layer.TriggerEffects)
                {
                    m_EffectList.Add(e);
                }
            }

            TriggerEffectListView.ItemsSource = m_EffectList;
            if (m_EffectList.Count == 0)
            {
                TriggerEffectTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                TriggerEffectTextBlock.Visibility = Visibility.Collapsed;
                TriggerEffectListView.SelectedIndex = 0;
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            string selectedAction = item.Text;
            TriggerActionButton.Content = selectedAction;
            m_Layer.TriggerAction = selectedAction;
        }
        private void AddEffectButton_Click(object sender, RoutedEventArgs e)
        {
            TriggerEffect effect = new TriggerEffect(GetTriggerEffect()[0]);
            effect.Layer = m_Layer;
            m_EffectList.Add(effect);
            if (m_EffectList.Count != 0)
            {
                TriggerEffectTextBlock.Visibility = Visibility.Collapsed;
            }
            TriggerEffectListView.SelectedIndex = m_EffectList.Count - 1;
        }
        private void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            m_EffectList.Clear();
            TriggerEffectListView.SelectedIndex = -1;
            TriggerEffectTextBlock.Visibility = Visibility.Visible;
        }
        private void TriggerEffectListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;

            if (lv.SelectedIndex == -1)
                TriggerEffectInfoFrame.Navigate(typeof(TriggerEffectInfoPage));
            else
                TriggerEffectInfoFrame.Navigate(typeof(TriggerEffectInfoPage), m_EffectList[lv.SelectedIndex]);
        }
        public void DeleteTriggerEffect(TriggerEffect eff)
        {
            if (m_EffectList.IndexOf(eff) == TriggerEffectListView.SelectedIndex)
            {
                if ((TriggerEffectListView.SelectedIndex - 1) != -1)
                {
                    TriggerEffectListView.SelectedIndex = TriggerEffectListView.SelectedIndex - 1;
                }
                else if (((TriggerEffectListView.SelectedIndex - 1) == -1) && (m_EffectList.Count > 1))
                {
                    TriggerEffectListView.SelectedIndex = TriggerEffectListView.SelectedIndex + 1;
                }
                else
                {
                    TriggerEffectListView.SelectedIndex = -1;
                }
            }

            m_EffectList.Remove(eff);

            if (m_EffectList.Count == 0)
            {
                TriggerEffectTextBlock.Visibility = Visibility.Visible;
            }
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            m_Layer.TriggerEffects = m_EffectList.ToList();

            if (m_Layer.TriggerEffects.Count != 0)
                m_Layer.IsTriggering = true;
            else
                m_Layer.IsTriggering = false;

            this.Hide();

            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }
    }
}
