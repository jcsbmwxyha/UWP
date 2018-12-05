using AuraEditor.Dialogs;
using AuraEditor.Models;
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
        private StorageFolder m_UserFilesFolder;
        private StorageFolder m_UserScriptsFolder;
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
            await GetOrCreateUserScriptsFolder();
            //await TestOrCreateScriptFolder();
        }
        private async Task GetOrCreateUserFilesFolder()
        {
            try
            {
                m_UserFilesFolder = await StorageFolder.GetFolderFromPathAsync(UserFilesDefaultFolderPath);
                var fileList = await m_UserFilesFolder.GetFilesAsync();
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
                m_UserFilesFolder = folder;
            }
        }
        private async Task GetOrCreateUserScriptsFolder()
        {
            try
            {
                m_UserScriptsFolder = await StorageFolder.GetFolderFromPathAsync(UserScriptsDefaultFolderPath);
            }
            catch
            {
                // Folder doesn't exist
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS");
                folder = await EnterOrCreateFolder(folder, "AURA Creator");
                folder = await EnterOrCreateFolder(folder, "UserScripts");
                m_UserScriptsFolder = folder;
            }
        }
        #endregion

        #region Framework element
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveCurrentUserFile();
            NeedSave = false;
            long StartTime = 0;

            // Apply after saving file
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS\\AURA Creator");
            StorageFile sf = await folder.CreateFileAsync("LastScript.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(sf, GetLastScript(true));

            await (new ServiceViewModel()).AuraEditorTrigger(StartTime);
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

            StorageFile script = await m_UserScriptsFolder.GetFileAsync(CurrentUserFilename + ".xml");
            await script.RenameAsync(dialog.TheName + ".xml");
            StorageFile file = await m_UserFilesFolder.GetFileAsync(CurrentUserFilename + ".xml");
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

            StorageFile script = await m_UserScriptsFolder.GetFileAsync(CurrentUserFilename + ".xml");
            await script.DeleteAsync(StorageDeleteOption.PermanentDelete);
            StorageFile file = await m_UserFilesFolder.GetFileAsync(CurrentUserFilename + ".xml");
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

                StorageFile copyfile = await inputFile.CopyAsync(m_UserFilesFolder, inputFile.Name, NameCollisionOption.ReplaceExisting);
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

                SaveSettings();
                CoreApplication.Exit();
            }
        }
        private void SaveSettings()
        {
            g_LocalSettings.Values["SpaceZooming"] = SpaceManager.GetSpaceZoomPercent().ToString();
            g_LocalSettings.Values["LayerLevel"] = LayerZoomSlider.Value.ToString();
            g_LocalSettings.Values["RecentColor1"] = g_RecentColor[0].HexColor;
            g_LocalSettings.Values["RecentColor2"] = g_RecentColor[1].HexColor;
            g_LocalSettings.Values["RecentColor3"] = g_RecentColor[2].HexColor;
            g_LocalSettings.Values["RecentColor4"] = g_RecentColor[3].HexColor;
            g_LocalSettings.Values["RecentColor5"] = g_RecentColor[4].HexColor;
            g_LocalSettings.Values["RecentColor6"] = g_RecentColor[5].HexColor;
            g_LocalSettings.Values["RecentColor7"] = g_RecentColor[6].HexColor;
            g_LocalSettings.Values["RecentColor8"] = g_RecentColor[7].HexColor;
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
                    await m_UserFilesFolder.CreateFileAsync(CurrentUserFilename + ".xml", CreationCollisionOption.ReplaceExisting);
                    await m_UserScriptsFolder.CreateFileAsync(CurrentUserFilename + ".xml", CreationCollisionOption.ReplaceExisting);
                }
                else
                {
                    return false;
                }
            }

            SpaceManager.ClearTempDeviceData();
            await SaveFile(UserFilesDefaultFolderPath + CurrentUserFilename + ".xml", GetUserData());
            await SaveFile(UserScriptsDefaultFolderPath + CurrentUserFilename + ".xml", GetLastScript(true));
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
            List<DeviceModel> globalDevices = SpaceManager.DeviceModelCollection;
            globalDevices.Clear();

            List<SyncDevice> new_SD = ConnectedDevicesDialog.Self.GetIngroupDevices();
            foreach (XmlNode node in deviceNodes)
            {
                XmlElement element = (XmlElement)node;
                int x = Int32.Parse(element.SelectSingleNode("x").InnerText);
                int y = Int32.Parse(element.SelectSingleNode("y").InnerText);
                DeviceContent dc = await DeviceContent.GetDeviceContent(node);
                DeviceModel d = await dc.ToDeviceModel(new Point(x * GridPixels, y * GridPixels));

                if (new_SD.Find(sd => sd.Name == d.Name && sd.Sync == true) != null)
                {
                    d.Status = DeviceStatus.OnStage;
                    new_SD.RemoveAll(sd => sd.Name == d.Name && sd.Sync == true);
                }
                else
                    d.Status = DeviceStatus.Temp;

                globalDevices.Add(d);
            }

            foreach (var sd in new_SD)
            {
                DeviceContent dc = await DeviceContent.GetDeviceContent(sd);

                if (dc == null)
                    continue;

                Rect r = new Rect(0, 0, dc.GridWidth, dc.GridHeight);
                Point p = SpaceManager.GetFreeRoomPositionForRect(r);
                DeviceModel dm = await dc.ToDeviceModel(p);
                dm.Status = DeviceStatus.OnStage;
                globalDevices.Add(dm);
            }
        }
        private void ParsingLayers(XmlNodeList layerNodes)
        {
            List<Layer> layers = new List<Layer>();

            foreach (XmlNode node in layerNodes)
            {
                XmlElement element = (XmlElement)node;
                string layerName = element.GetAttribute("name");
                string eye = element.GetAttribute("Eye");
                Layer layer = new Layer(layerName);
                layer.Eye = bool.Parse(eye);

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
                        ColorSegmentation = bool.Parse(element2.SelectSingleNode("colorSegmentation").InnerText),
                    };

                    if (!IsTriggerEffect(type))
                    {
                        TimelineEffect eff = new TimelineEffect(type);
                        eff.StartTime = Int32.Parse(element2.SelectSingleNode("start").InnerText);
                        eff.DurationTime = Int32.Parse(element2.SelectSingleNode("duration").InnerText);
                        eff.Info = info;
                        layer.AddTimelineEffect(eff);
                    }
                    else
                    {
                        TriggerEffect eff = new TriggerEffect(type);
                        eff.StartTime = Int32.Parse(element2.SelectSingleNode("start").InnerText);
                        eff.DurationTime = Int32.Parse(element2.SelectSingleNode("duration").InnerText);
                        eff.Info = info;
                        layer.AddTriggerEffect(eff);
                        layer.IsTriggering = true;
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
                LayerManager.AddLayer(layer);
            }
        }

        private void Reset()
        {
            Clean();
            SpaceManager.FillStageWithDevices();
        }
        private void Clean()
        {
            SetLayerButton.IsEnabled = true;
            SetLayerRectangle.Visibility = Visibility.Collapsed;
            NeedSave = false;
            CurrentUserFilename = "";
            LayerManager.Clean();
            SpaceManager.Clean();
            Player.Stop();
        }
    }
}

