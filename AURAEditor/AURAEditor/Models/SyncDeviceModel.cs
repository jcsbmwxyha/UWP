using AuraEditor.Dialogs;
using System.ComponentModel;

namespace AuraEditor.Models
{
    public class SyncDeviceModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region -- Property --
        public string ModelName { get; set; }

        private string type;
        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                UpdateDeviceImage();
            }
        }

        private bool sync;
        public bool Sync
        {
            get
            {
                return sync;
            }
            set
            {
                if (value == true)
                    sync = value;
                else
                    sync = false;

                UpdateDeviceImage();
                ConnectedDevicesDialog.Self.UpdateSelectedState();
                RaisePropertyChanged("Sync");
            }
        }

        private string deviceIconPath;
        public string DeviceIconPath {
            get
            {
                return deviceIconPath;
            }
            set
            {
                if (deviceIconPath != value)
                {
                    deviceIconPath = value;
                    RaisePropertyChanged("DeviceIconPath");
                }
            }
        }
        #endregion

        public void UpdateDeviceImage()
        {
            if (type != null)
            {
                if (Sync)
                {
                    DeviceIconPath = "../Assets/ConnectedDevices/icons/asus_ac_" + type.ToLower() + "_ic_s.png";
                }
                else
                {
                    DeviceIconPath = "../Assets/ConnectedDevices/icons/asus_ac_" + type.ToLower() + "_ic_n.png";
                }
            }
        }
    }
}
