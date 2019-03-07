using AuraEditor.Common;
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
        private ObservableCollection<TriggerEffect> _oldTriggerEffectList;

        private string _oldActionSelected;
        private string _currentActionSelected = "One Click";

        static public TriggerDialog Self;

        public TriggerDialog(LayerModel layer)
        {
            this.InitializeComponent();
            Self = this;

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

            if(_currentActionSelected != selectedAction)
            {
                _oldActionSelected = _currentActionSelected;
                _currentActionSelected = selectedAction;
                ReUndoManager.Store(new ActionSelectedCommand(_oldActionSelected, _currentActionSelected));
            }
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
            ReUndoManager.Store(new AddEffectCommand(effect));
        }
        private void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            _oldTriggerEffectList = new ObservableCollection<TriggerEffect>(m_EffectList);
            ReUndoManager.Store(new RemoveAllCommand(_oldTriggerEffectList));
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

        #region ReUndo
        public class ActionSelectedCommand : IReUndoCommand
        {
            private string _oldActionSelectedValue;
            private string _currentActionSelectedValue;

            public ActionSelectedCommand(string oldActionSelectedValue, string currentActionSelectedValue)
            {
                _oldActionSelectedValue = oldActionSelectedValue;
                _currentActionSelectedValue = currentActionSelectedValue;
            }

            public void ExecuteRedo()
            {
                Self.TriggerActionButton.Content = _currentActionSelectedValue;
                Self.m_Layer.TriggerAction = _currentActionSelectedValue;
            }

            public void ExecuteUndo()
            {
                Self.TriggerActionButton.Content = _oldActionSelectedValue;
                Self.m_Layer.TriggerAction = _oldActionSelectedValue;
            }
        }
        public class AddEffectCommand : IReUndoCommand
        {
            private TriggerEffect _oldEffectValue;

            public AddEffectCommand(TriggerEffect oldEffectValue)
            {
                _oldEffectValue = oldEffectValue;
            }

            public void ExecuteRedo()
            {
                Self.m_EffectList.Add(_oldEffectValue);
                Self.m_Layer.TriggerEffects = Self.m_EffectList.ToList();
                if (Self.m_EffectList.Count != 0)
                {
                    Self.TriggerEffectTextBlock.Visibility = Visibility.Collapsed;
                    Self.m_Layer.IsTriggering = true;
                }
                Self.TriggerEffectListView.SelectedIndex = Self.m_EffectList.Count - 1;
            }

            public void ExecuteUndo()
            {
                Self.DeleteTriggerEffect(_oldEffectValue);
                Self.m_Layer.TriggerEffects = Self.m_EffectList.ToList();
                if (Self.m_EffectList.Count != 0)
                {
                    Self.TriggerEffectTextBlock.Visibility = Visibility.Collapsed;
                    Self.m_Layer.IsTriggering = true;
                }
                else
                {
                    Self.TriggerEffectTextBlock.Visibility = Visibility.Visible;
                    Self.m_Layer.IsTriggering = false;
                }
                Self.TriggerEffectListView.SelectedIndex = Self.m_EffectList.Count - 1;
            }
        }

        public class RemoveAllCommand : IReUndoCommand
        {
            private ObservableCollection<TriggerEffect> _oldEffectList;

            public RemoveAllCommand(ObservableCollection<TriggerEffect> oldEffectList)
            {
                _oldEffectList = oldEffectList;
            }

            public void ExecuteRedo()
            {
                Self.m_EffectList.Clear();
                Self.m_Layer.TriggerEffects = Self.m_EffectList.ToList();
                if (Self.m_EffectList.Count != 0)
                {
                    Self.TriggerEffectTextBlock.Visibility = Visibility.Collapsed;
                    Self.m_Layer.IsTriggering = true;
                }
                else
                {
                    Self.TriggerEffectTextBlock.Visibility = Visibility.Visible;
                    Self.m_Layer.IsTriggering = false;
                }
                Self.TriggerEffectListView.SelectedIndex = Self.m_EffectList.Count - 1;
            }

            public void ExecuteUndo()
            {
                Self.m_EffectList = new ObservableCollection<TriggerEffect>(_oldEffectList);
                Self.m_Layer.TriggerEffects = Self.m_EffectList.ToList();
                if (Self.m_EffectList.Count != 0)
                {
                    Self.TriggerEffectTextBlock.Visibility = Visibility.Collapsed;
                    Self.m_Layer.IsTriggering = true;
                }
                else
                {
                    Self.TriggerEffectTextBlock.Visibility = Visibility.Visible;
                    Self.m_Layer.IsTriggering = false;
                }
                Self.TriggerEffectListView.SelectedIndex = 0;
            }
        }
        #endregion
    }
}
