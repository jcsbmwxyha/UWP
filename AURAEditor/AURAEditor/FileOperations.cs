using AuraEditor.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.StorageHelper;
using static AuraEditor.Common.XmlHelper;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        public bool NeedSave;
        private StorageFolder m_UserFileFolder;
        private List<string> GetUserFilenames()
        {
            List<string> filenames = new List<string>();
            foreach (var item in FileListMenuFlyout.Items)
            {
                MenuFlyoutItem mfi = item as MenuFlyoutItem;
                filenames.Add(mfi.Text);
            }
            return filenames;
        }
        public string CurrentUserFilename
        {
            get
            {
                return FileListButton.Content as string;
            }
            set
            {
                if (value != FileListButton.Content as string)
                {
                    FileListButton.Content = value;

                    if (value == "")
                    {
                        return;
                    }

                    foreach (var filename in GetUserFilenames())
                    {
                        if (filename == value)
                        {
                            return;
                        }
                    }

                    MenuFlyoutItem new_mfi = new MenuFlyoutItem();
                    new_mfi.Text = value;
                    new_mfi.Style = (Style)Application.Current.Resources["RogMenuFlyoutItemStyle1"];
                    new_mfi.Click += FileItem_Click;
                    FileListMenuFlyout.Items.Add(new_mfi);
                }
            }
        }

        #region Intialize
        private async Task IntializeFileOperations()
        {
            NeedSave = false;
            await GetOrCreateUserFilesFolder();
            await TestOrCreateScriptFolder();
        }
        private async Task GetOrCreateUserFilesFolder()
        {
            try
            {
                m_UserFileFolder = await StorageFolder.GetFolderFromPathAsync(UserFilesDefaultFolderPath);
                var fileList = await m_UserFileFolder.GetFilesAsync();
                var filenameList = from file in fileList
                                   orderby file.DateCreated.ToFileTime()
                                   select file.DisplayName;

                foreach (var filename in filenameList)
                {
                    MenuFlyoutItem mfi = new MenuFlyoutItem();
                    mfi.Text = filename;
                    mfi.Style = (Style)Application.Current.Resources["RogMenuFlyoutItemStyle1"];
                    mfi.Click += FileItem_Click;
                    FileListMenuFlyout.Items.Add(mfi);
                }
            }
            catch
            {
                // Folder doesn't exist
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS");
                folder = await EnterOrCreateFolder(folder, "AURA Creator");
                folder = await EnterOrCreateFolder(folder, "UserFiles");
                m_UserFileFolder = folder;
            }
        }
        private async Task TestOrCreateScriptFolder()
        {
            try
            {
                await StorageFolder.GetFolderFromPathAsync(DefaultScriptFolder);
            }
            catch
            {
                // Folder doesn't exist
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS");
                folder = await EnterOrCreateFolder(folder, "AURA Creator");
                folder = await EnterOrCreateFolder(folder, "script");
            }
        }
        #endregion

        #region Framework element
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveCurrentUserFile();
            NeedSave = false;

            // Apply after saving file
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS\\AURA Creator\\script");
            StorageFile sf = await folder.CreateFileAsync("LastScript.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(sf, PrintScriptXml(true));

            await (new ServiceViewModel()).AuraEditorTrigger();
        }
        private async void NewFileButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = ContentDialogResult.Secondary;

            if (NeedSave)
            {
                YesNoCancelDialog dialog = new YesNoCancelDialog
                {
                    DialogTitle = "Save File",
                    DialogContent = "Do you want to save the changes?"
                };
                await dialog.ShowAsync();
                result = dialog.Result;
            }

            if (result != ContentDialogResult.None)
            {
                if (result == ContentDialogResult.Primary)
                {
                    bool successful = await SaveCurrentUserFile();

                    if (!successful)
                        return;
                }

                Reset();
            }
        }
        private async void RenameItem_Click(object sender, RoutedEventArgs e)
        {
            NamingDialog dialog = new NamingDialog(CurrentUserFilename, GetUserFilenames());
            ContentDialogResult namingResult = await dialog.ShowAsync();

            if (CurrentUserFilename == dialog.TheName)
                return;

            StorageFile file = await m_UserFileFolder.GetFileAsync(CurrentUserFilename + ".xml");
            await file.RenameAsync(dialog.TheName + ".xml");

            foreach (var item in FileListMenuFlyout.Items)
            {
                MenuFlyoutItem mfi = item as MenuFlyoutItem;
                if (mfi.Text == CurrentUserFilename)
                {
                    mfi.Text = dialog.TheName;
                    CurrentUserFilename = mfi.Text;
                };
            }
        }
        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUserFilename == "")
                return;

            YesNoCancelDialog dialog = new YesNoCancelDialog
            {
                DialogTitle = "Delete File",
                DialogContent = "Delete this file ?"
            };

            await dialog.ShowAsync();
            ContentDialogResult result = dialog.Result;

            if (result != ContentDialogResult.Primary)
                return;

            StorageFile file = await m_UserFileFolder.GetFileAsync(CurrentUserFilename + ".xml");
            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);

            foreach (var item in FileListMenuFlyout.Items)
            {
                MenuFlyoutItem mfi = item as MenuFlyoutItem;
                if (mfi.Text == CurrentUserFilename)
                {
                    FileListMenuFlyout.Items.Remove(mfi);
                    break;
                };
            }

            Reset();
        }
        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var inputFile = await ShowFileOpenPickerAsync();

            if (inputFile == null)
            {
                return;
            }

            ContentDialogResult result = ContentDialogResult.Secondary;

            if (NeedSave)
            {
                YesNoCancelDialog dialog = new YesNoCancelDialog
                {
                    DialogTitle = "Save File",
                    DialogContent = "Do you want to save the changes?"
                };
                await dialog.ShowAsync();
                result = dialog.Result;
            }

            if (result != ContentDialogResult.None)
            {
                if (result == ContentDialogResult.Primary)
                {
                    bool successful = await SaveCurrentUserFile();

                    if (!successful)
                        return;
                }

                StorageFile copyfile = await inputFile.CopyAsync(m_UserFileFolder, inputFile.Name, NameCollisionOption.ReplaceExisting);
                CurrentUserFilename = copyfile.Name.Replace(".xml", "");
                await LoadUserFile(CurrentUserFilename);
            }
        }
        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile saveFile = await ShowFileSavePickerAsync();

            if (saveFile != null)
            {
                SpaceManager.ClearTempDeviceData();
                await SaveFile(saveFile, GetUserData());
            }
        }
        private async void FileItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            string selectedName = item.Text;

            if (selectedName == CurrentUserFilename)
                return;

            ContentDialogResult result = ContentDialogResult.Secondary;

            if (NeedSave)
            {
                YesNoCancelDialog dialog = new YesNoCancelDialog
                {
                    DialogTitle = "Save File",
                    DialogContent = "Do you want to save the changes?"
                };
                await dialog.ShowAsync();
                result = dialog.Result;
            }

            if (result != ContentDialogResult.None)
            {
                if (result == ContentDialogResult.Primary)
                {
                    bool successful = await SaveCurrentUserFile();

                    if (!successful)
                        return;
                }

                await LoadUserFile(selectedName);
                CurrentUserFilename = selectedName;
            }
        }
        private async void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            e.Handled = true;
            ContentDialogResult result = ContentDialogResult.Secondary;

            if (NeedSave)
            {
                YesNoCancelDialog dialog = new YesNoCancelDialog
                {
                    DialogTitle = "Save File",
                    DialogContent = "Do you want to save the changes?"
                };
                await dialog.ShowAsync();
                result = dialog.Result;
            }

            if (result != ContentDialogResult.None)
            {
                if (result == ContentDialogResult.Primary)
                {
                    bool successful = await SaveCurrentUserFile();

                    if (!successful)
                        return;
                }

                CoreApplication.Exit();
            }
        }
        #endregion

        private async Task<bool> SaveCurrentUserFile()
        {
            if (CurrentUserFilename == "")
            {
                NamingDialog dialog = new NamingDialog(GetUserFilenames());
                ContentDialogResult namingResult = await dialog.ShowAsync();

                if (dialog.Result == true)
                {
                    CurrentUserFilename = dialog.TheName;
                    await m_UserFileFolder.CreateFileAsync(CurrentUserFilename + ".xml", CreationCollisionOption.ReplaceExisting);
                }
                else
                {
                    return false;
                }
            }

            SpaceManager.ClearTempDeviceData();
            await SaveFile(UserFilesDefaultFolderPath + CurrentUserFilename + ".xml", GetUserData());
            return true;
        }
        private string GetUserData()
        {
            XmlNode root = CreateXmlNode("root");

            root.AppendChild(SpaceManager.ToXmlNodeForUserData());
            root.AppendChild(LayerManager.ToXmlNodeForUserData());

            return root.OuterXml;
        }

        private async Task LoadUserFile(string filename)
        {
            string filepath = UserFilesDefaultFolderPath + filename + ".xml";
            Clean();
            await LoadContent(await LoadFile(filepath));
            SpaceManager.RefreshSpaceGrid();
        }
        private async Task LoadContent(string xmlContent)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlContent);

            XmlNode spaceNode = xml.SelectSingleNode("/root/space");
            XmlNode layersNode = xml.SelectSingleNode("/root/layers");

            XmlNodeList deviceNodes = spaceNode.SelectNodes("device");
            XmlNodeList layerNodes = layersNode.SelectNodes("layer");

            await ParsingGlobalDevices(deviceNodes);
            ParsingLayers(layerNodes);
        }
        private async Task ParsingGlobalDevices(XmlNodeList deviceNodes)
        {
            List<Device> devices = new List<Device>();

            foreach (XmlNode node in deviceNodes)
            {
                XmlElement element = (XmlElement)node;
                int x = Int32.Parse(element.SelectSingleNode("x").InnerText);
                int y = Int32.Parse(element.SelectSingleNode("y").InnerText);

                DeviceContent deviceContent = await DeviceContent.GetDeviceContent(node);
                Device d = await deviceContent.ToDevice(new Point(x, y));

                devices.Add(d);
            }

            // Check device sync or not
            var syncDevices = ConnectedDevicesDialog.Self.GetIngroupDevices();
            foreach (var d in devices)
            {
                if (syncDevices.Find(sd => sd.Name == d.Name && sd.Sync == true) != null)
                    d.Status = DeviceStatus.OnStage;
                else
                    d.Status = DeviceStatus.Temp;
            }

            SpaceManager.GlobalDevices = devices;
        }
        private void ParsingLayers(XmlNodeList layerNodes)
        {
            List<Layer> layers = new List<Layer>();

            foreach (XmlNode node in layerNodes)
            {
                XmlElement element = (XmlElement)node;
                string layerName = element.GetAttribute("name");
                Layer layer = new Layer(layerName);

                layer.TriggerAction = element.GetAttribute("trigger");

                // parsing effects
                XmlNode effectsNode = element.SelectSingleNode("effects");
                foreach (XmlNode effectNode in effectsNode.ChildNodes)
                {
                    XmlElement element2 = (XmlElement)effectNode;
                    int type = Int32.Parse(element2.SelectSingleNode("type").InnerText);

                    List<ColorPoint> colorPoints = new List<ColorPoint>();
                    XmlNode colorPointListNode = element2.SelectSingleNode("colorPointList");
                    foreach (XmlNode colorpoint in colorPointListNode.ChildNodes)
                    {
                        ColorPoint cp = new ColorPoint();
                        XmlElement element3 = (XmlElement)colorpoint;
                        cp.Color = new Color
                        {
                            A = Byte.Parse(element3.SelectSingleNode("a").InnerText),
                            R = Byte.Parse(element3.SelectSingleNode("r").InnerText),
                            G = Byte.Parse(element3.SelectSingleNode("g").InnerText),
                            B = Byte.Parse(element3.SelectSingleNode("b").InnerText),
                        };
                        cp.Offset = double.Parse(element3.SelectSingleNode("offset").InnerText);
                        colorPoints.Add(cp);
                    }

                    EffectInfo info = new EffectInfo()
                    {
                        InitColor = new Color
                        {
                            A = Byte.Parse(element2.SelectSingleNode("a").InnerText),
                            R = Byte.Parse(element2.SelectSingleNode("r").InnerText),
                            G = Byte.Parse(element2.SelectSingleNode("g").InnerText),
                            B = Byte.Parse(element2.SelectSingleNode("b").InnerText),
                        },
                        Type = type,
                        Direction = Int32.Parse(element2.SelectSingleNode("direction").InnerText),
                        Speed = Int32.Parse(element2.SelectSingleNode("speed").InnerText),
                        Angle = Int32.Parse(element2.SelectSingleNode("angle").InnerText),
                        Random = bool.Parse(element2.SelectSingleNode("random").InnerText),
                        High = Int32.Parse(element2.SelectSingleNode("high").InnerText),
                        Low = Int32.Parse(element2.SelectSingleNode("low").InnerText),
                        ColorPointList = new List<ColorPoint>(colorPoints),
                    };

                    if (!IsTriggerEffect(type))
                    {
                        TimelineEffect eff = new TimelineEffect(layer, type);
                        eff.StartTime = Int32.Parse(element2.SelectSingleNode("start").InnerText);
                        eff.DurationTime = Int32.Parse(element2.SelectSingleNode("duration").InnerText);
                        eff.Info = info;
                        layer.AddTimelineEffect(eff);
                    }
                    else
                    {
                        TriggerEffect eff = new TriggerEffect(layer, type);
                        eff.StartTime = Int32.Parse(element2.SelectSingleNode("start").InnerText);
                        eff.DurationTime = Int32.Parse(element2.SelectSingleNode("duration").InnerText);
                        eff.Info = info;
                        layer.AddTriggerEffect(eff);
                    }
                }

                // parsing zones
                XmlNode devicesNode = element.SelectSingleNode("devices");
                foreach (XmlNode deviceNode in devicesNode.ChildNodes)
                {
                    Dictionary<int, int[]> zoneDictionary = new Dictionary<int, int[]>();
                    List<int> zones = new List<int>();
                    XmlElement element2 = (XmlElement)deviceNode;
                    string typeName = element2.GetAttribute("type");
                    int type = GetTypeByTypeName(typeName);

                    XmlNodeList indexNodes = element2.ChildNodes;
                    foreach (XmlNode index in indexNodes)
                    {
                        zones.Add(Int32.Parse(index.InnerText));
                    }

                    zoneDictionary.Add(type, zones.ToArray());
                    layer.AddDeviceZones(zoneDictionary);
                }

                layers.Add(layer);
                LayerManager.AddDeviceLayer(layer);
            }
        }

        private void Reset()
        {
            Clean();
            SpaceManager.FillStageWithDevices();
        }
        private void Clean()
        {
            SelectedEffectLine = null;
            SetLayerButton.IsEnabled = true;
            TimelineZoomLevel = 2;
            NeedSave = false;
            CurrentUserFilename = "";
            LayerManager.Clean();
            SpaceManager.Clean();
        }
    }
}

