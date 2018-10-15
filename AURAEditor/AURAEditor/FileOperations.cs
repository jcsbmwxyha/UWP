using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using System.Threading.Tasks;
using System.Xml;
using AuraEditor.Dialogs;
using Windows.Foundation;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.Common.StorageHelper;
using static AuraEditor.Common.Definitions;
using Windows.UI;
using Windows.UI.Core.Preview;
using Windows.ApplicationModel.Core;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        private bool needSave;
        private StorageFile m_UserFileListXml;
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
                    new_mfi.Click += FileItem_Click;
                    FileListMenuFlyout.Items.Add(new_mfi);
                    UpdateListXml();
                }
            }
        }
        private async void UpdateListXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement userfilesElement = doc.CreateElement(string.Empty, "userfiles", string.Empty);
            doc.AppendChild(userfilesElement);

            foreach (var item in FileListMenuFlyout.Items)
            {
                MenuFlyoutItem mfi = item as MenuFlyoutItem;
                string filename = mfi.Text;

                XmlElement fileElement = doc.CreateElement(string.Empty, "file", string.Empty);
                fileElement.SetAttribute("name", filename);
                userfilesElement.AppendChild(fileElement);
            }

            await SaveFile(m_UserFileListXml, doc.OuterXml);
        }

        private async Task IntializeFileOperations()
        {
            needSave = true;
            await GetOrCreateListXml();
            await GetOrCreateFolderOfFiles();
            await TestOrCreateScriptFolder();
        }
        private async Task GetOrCreateListXml()
        {
            try
            {
                m_UserFileListXml = await StorageFile.GetFileFromPathAsync(UserFileListXmlPath);
            }
            catch
            {
                // XML doesn't exist
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS");
                folder = await EnterOrCreateFolder(folder, "AURA Creator");
                m_UserFileListXml = await folder.CreateFileAsync("UserFiles.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting);

                XmlDocument doc = new XmlDocument();
                XmlElement recentfilesElement = doc.CreateElement(string.Empty, "userfiles", string.Empty);
                doc.AppendChild(recentfilesElement);

                await Windows.Storage.FileIO.WriteTextAsync(m_UserFileListXml, doc.OuterXml);
            }
        }
        private async Task GetOrCreateFolderOfFiles()
        {
            try
            {
                m_UserFileFolder = await StorageFolder.GetFolderFromPathAsync(UserFilesDefaultFolderPath);
                List<string> filenameList = new List<string>(await GetAllFilenames(m_UserFileListXml));

                foreach (var filename in filenameList)
                {
                    MenuFlyoutItem mfi = new MenuFlyoutItem();
                    mfi.Text = filename;
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
        private async Task<List<string>> GetAllFilenames(StorageFile userFileSF)
        {
            List<string> list = new List<string>();
            XmlDocument xml = new XmlDocument();
            xml.Load(await userFileSF.OpenStreamForReadAsync());
            XmlNode userfilesNode = xml.SelectSingleNode("userfiles");
            XmlNodeList fileNodes = userfilesNode.SelectNodes("file");

            for (int i = 0; i < fileNodes.Count; i++)
            {
                XmlElement element = (XmlElement)fileNodes[i];
                list.Add(element.GetAttribute("name"));
            }

            return list;
        }
        private async Task<bool> SaveCurrentUserFile()
        {
            if (CurrentUserFilename != "")
            {
                await SaveFile(UserFilesDefaultFolderPath + CurrentUserFilename + ".xml", GetUserData());
            }
            else
            {
                NamingDialog dialog = new NamingDialog(GetUserFilenames());
                ContentDialogResult namingResult = await dialog.ShowAsync();

                if (dialog.Result == true)
                {
                    CurrentUserFilename = dialog.TheName;
                    await m_UserFileFolder.CreateFileAsync(CurrentUserFilename + ".xml", CreationCollisionOption.ReplaceExisting);
                    await SaveFile(UserFilesDefaultFolderPath + CurrentUserFilename + ".xml", GetUserData());
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        private async Task LoadUserFile(string filename)
        {
            string filepath = UserFilesDefaultFolderPath + filename + ".xml";
            Reset();
            await LoadContent(await LoadFile(filepath));
            SpaceManager.RefreshSpaceGrid();
        }
        private async void FileItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            string selectedName = item.Text;

            if (selectedName == CurrentUserFilename)
                return;

            ContentDialogResult result = ContentDialogResult.Secondary;

            if (needSave)
            {
                YesNoCancelDialog dialog = new YesNoCancelDialog
                {
                    Title = "Save File",
                    DialogContent = "Do you want to save the changes?"
                };
                result = await dialog.ShowAsync();
            }

            if (result == ContentDialogResult.Primary)
            {
                bool successful = await SaveCurrentUserFile();
                if (successful)
                {
                    await LoadUserFile(selectedName);
                    CurrentUserFilename = selectedName;
                }
            }
            else if (result == ContentDialogResult.Secondary)
            {
                await LoadUserFile(selectedName);
                CurrentUserFilename = selectedName;
            }
        }
        private async void NewButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = ContentDialogResult.Secondary;

            if (needSave)
            {
                YesNoCancelDialog dialog = new YesNoCancelDialog
                {
                    Title = "Save File",
                    DialogContent = "Do you want to save the changes?"
                };
                result = await dialog.ShowAsync();
            }

            if (result == ContentDialogResult.Primary)
            {
                bool successful = await SaveCurrentUserFile();
                if (successful)
                {
                    CurrentUserFilename = "";
                    Reset();
                    SpaceManager.FillWithIngroupDevices();
                }
            }
            else if (result == ContentDialogResult.Secondary)
            {
                CurrentUserFilename = "";
                Reset();
                SpaceManager.FillWithIngroupDevices();
            }
        }
        private async void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var inputFile = await ShowFileOpenPickerAsync();

            if (inputFile == null)
            {
                return;
            }

            ContentDialogResult result = ContentDialogResult.Secondary;

            if (needSave)
            {
                YesNoCancelDialog dialog = new YesNoCancelDialog
                {
                    Title = "Save File",
                    DialogContent = "Do you want to save the changes?"
                };
                result = await dialog.ShowAsync();
            }

            if (result == ContentDialogResult.Primary)
            {
                if (CurrentUserFilename != "")
                {
                    await SaveFile(CurrentUserFilename, GetUserData());
                }
                else
                {
                    StorageFile saveFile = await ShowFileSavePickerAsync();

                    if (saveFile != null)
                    {
                        await SaveFile(saveFile, GetUserData());
                        CurrentUserFilename = saveFile.Path;
                    }
                    else
                    {
                        return;
                    }
                }

                // load file
                StorageFile copyfile = await inputFile.CopyAsync(m_UserFileFolder, inputFile.Name, NameCollisionOption.ReplaceExisting);
                CurrentUserFilename = copyfile.Name.Replace(".xml", "");
                Reset();
                await LoadContent(await LoadFile(copyfile));
                SpaceManager.RefreshSpaceGrid();
            }
            else if (result == ContentDialogResult.Secondary)
            {
                // load file
                StorageFile copyfile = await inputFile.CopyAsync(m_UserFileFolder, inputFile.Name, NameCollisionOption.ReplaceExisting);
                CurrentUserFilename = copyfile.Name.Replace(".xml","");
                Reset();
                await LoadContent(await LoadFile(copyfile));
                SpaceManager.RefreshSpaceGrid();
            }
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveCurrentUserFile();
        }
        private async void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile saveFile = await ShowFileSavePickerAsync();

            if (saveFile != null)
            {
                await SaveFile(saveFile, GetUserData());
            }
        }
        private async void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            e.Handled = true;
            ContentDialogResult result = ContentDialogResult.Secondary;

            if (needSave)
            {
                YesNoCancelDialog dialog = new YesNoCancelDialog
                {
                    Title = "Save File",
                    DialogContent = "Do you want to save the changes?"
                };
                result = await dialog.ShowAsync();
            }

            if (result == ContentDialogResult.Primary)
            {
                await SaveCurrentUserFile();
                CoreApplication.Exit();
            }
            else if (result == ContentDialogResult.Secondary)
            {
                CoreApplication.Exit();
            }
        }
        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
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

            UpdateListXml();
        }
        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUserFilename == "")
                return;

            YesNoCancelDialog dialog = new YesNoCancelDialog
            {
                Title = "Save File",
                DialogContent = "Do you want to save the changes?"
            };
            ContentDialogResult result = await dialog.ShowAsync();
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

            CurrentUserFilename = "";
            Reset();
            SpaceManager.FillWithIngroupDevices();
            UpdateListXml();
        }
        public string GetUserData()
        {
            XmlNode root = CreateXmlNodeOfFile("root");

            root.AppendChild(SpaceManager.ToXmlNodeForUserData());
            root.AppendChild(LayerManager.ToXmlNodeForUserData());

            return root.OuterXml;
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
                string deviceName = element.GetAttribute("name");
                int x = Int32.Parse(element.SelectSingleNode("x").InnerText);
                int y = Int32.Parse(element.SelectSingleNode("y").InnerText);

                DeviceContent deviceContent = await DeviceContent.GetDeviceContent(deviceName);
                Device d = deviceContent.ToDevice(new Point(x, y));

                devices.Add(d);
            }
            SpaceManager.GlobalDevices = devices;
        }
        private void ParsingLayers(XmlNodeList layerNodes)
        {
            List<DeviceLayer> layers = new List<DeviceLayer>();

            foreach (XmlNode node in layerNodes)
            {
                XmlElement element = (XmlElement)node;
                string layerName = element.GetAttribute("name");
                DeviceLayer layer = new DeviceLayer(layerName);

                // parsing effects
                XmlNode effectsNode = element.SelectSingleNode("effects");
                foreach (XmlNode effectNode in effectsNode.ChildNodes)
                {
                    XmlElement element2 = (XmlElement)effectNode;
                    int type = Int32.Parse(element2.SelectSingleNode("type").InnerText);

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
                    string name = element2.GetAttribute("name");
                    int type = GetDeviceTypeByDeviceName(name);

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
            DragDevImgToggleButton.IsChecked = false;
            SetLayerButton.IsEnabled = true;
            SelectedEffectLine = null;
            LayerManager.Reset();
            SpaceManager.Reset();
            TimelineZoomLevel = 2;
        }
    }
}

