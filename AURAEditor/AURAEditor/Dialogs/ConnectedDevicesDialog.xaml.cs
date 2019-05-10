using AuraEditor.Models;
using AuraEditor.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.MetroEventSource;

// 內容對話方塊項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace AuraEditor.Dialogs
{
    public sealed partial class ConnectedDevicesDialog : ContentDialog
    {
        static public ConnectedDevicesDialog Self;
        private ObservableCollection<SyncDeviceModel> m_SyncDeviceList;

        public ConnectedDevicesDialog()
        {
            Self = this;
            this.InitializeComponent();

            UpdateDeviceList();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            List<DeviceModel> deviceModels = SpacePage.Self.DeviceModelCollection;

            foreach (SyncDeviceModel sd in m_SyncDeviceList)
            {
                var get = deviceModels.Find(find => find.Plugged == true && find.ModelName == sd.ModelName);

                if (get == null) continue;

                // F -> T, if piling, move to free room.
                if (get.Sync == false && sd.Sync == true && SpacePage.Self.IsPiling(get))
                    SpacePage.Self.MoveDeviceToFreeRoom(get);

                get.Sync = sd.Sync;
            }

            SpacePage.Self.SendSyncStateToService();
            SpacePage.Self.RefreshStage();

            this.Hide();
            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainPage.Self.CanShowDeviceUpdateDialog = true;
            MainPage.Self.ShowDeviceUpdateDialogOrNot();
        }
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_SyncDeviceList.Count == 0)
                return;

            if (SelectAllButton.IsChecked == true)
            {
                foreach (var sd in m_SyncDeviceList)
                    sd.Sync = true;
            }
            else
            {
                foreach (var sd in m_SyncDeviceList)
                    sd.Sync = false;
            }
        }

        public void UpdateDeviceList()
        {
            m_SyncDeviceList = new ObservableCollection<SyncDeviceModel>();
            var getList = SpacePage.Self.DeviceModelCollection.FindAll(find => find.Plugged == true);

            foreach (var dm in getList)
            {
                SyncDeviceModel sd = new SyncDeviceModel
                {
                    ModelName = dm.ModelName,
                    Type = GetTypeNameByType(dm.Type),
                    Sync = dm.Sync
                };

                m_SyncDeviceList.Add(sd);
            }

            ConnectedDevicesListView.ItemsSource = m_SyncDeviceList;
            UpdateSelectedState();
        }
        public void UpdateSelectedState()
        {
            int selectedcount = 0;

            foreach (var sd in m_SyncDeviceList)
            {
                if (sd.Sync == true)
                    selectedcount++;
            }

            if (selectedcount == m_SyncDeviceList.Count)
                SelectAllButton.IsChecked = true;
            else
                SelectAllButton.IsChecked = false;

            SelectedNumberText_M.Text = selectedcount.ToString();
        }
    }
}
