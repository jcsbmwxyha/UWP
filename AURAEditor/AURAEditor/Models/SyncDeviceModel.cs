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
        public string Name { get; set; }

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
                ConnectedDevicesDialog.Self.UpdateSelectedText();
                RaisePropertyChanged("Sync");
            }
        }

        private string deviceImgPath;
        public string DeviceImgPath {
            get
            {
                return deviceImgPath;
            }
            set
            {
                if (deviceImgPath != value)
                {
                    deviceImgPath = value;
                    RaisePropertyChanged("DeviceImgPath");
                }
            }
        }
        #endregion

        public SyncDeviceModel()
        {
        }

        public void UpdateDeviceImage()
        {
            if (type != null)
            {
                if (Sync)
                {
                    DeviceImgPath = "../Assets/ConnectedDevices/icons/asus_ac_" + type.ToLower() + "_ic_s.png";
                }
                else
                {
                    DeviceImgPath = "../Assets/ConnectedDevices/icons/asus_ac_" + type.ToLower() + "_ic_n.png";
                }
            }
        }
    }
}
