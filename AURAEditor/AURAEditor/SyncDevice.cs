namespace AuraEditor
{
    public class SyncDevice
    {
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
                GetImagePath(type);
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
                sync = value;
                GetImagePath(type);
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
                }
            }
        }

        public SyncDevice()
        {
        }

        public void GetImagePath(string ImgType)
        {
            if (ImgType != null)
            {
                if (ImgType == "Notebook")
                {
                    if (Sync)
                    {
                        DeviceImgPath = "../Assets/ConnectedDevices/icons/asus_ac_laptop_ic_s.png";
                    }
                    else
                    {
                        DeviceImgPath = "../Assets/ConnectedDevices/icons/asus_ac_laptop_ic_n.png";
                    }
                }
                else
                {
                    if (Sync)
                    {
                        DeviceImgPath = "../Assets/ConnectedDevices/icons/asus_ac_" + ImgType.ToLower() + "_ic_s.png";
                    }
                    else
                    {
                        DeviceImgPath = "../Assets/ConnectedDevices/icons/asus_ac_" + ImgType.ToLower() + "_ic_n.png";
                    }
                }
            }
        }
    }
}
