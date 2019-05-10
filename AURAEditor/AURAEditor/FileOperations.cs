using AuraEditor.Common;
using AuraEditor.Dialogs;
using AuraEditor.Models;
using AuraEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static AuraEditor.Common.MetroEventSource;
using static AuraEditor.Common.StorageHelper;
using static AuraEditor.Common.XmlHelper;
using static AuraEditor.Common.ControlHelper;
using Windows.ApplicationModel.Resources;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        private StorageFolder m_LocalUserFileFolder;
        private StorageFolder m_LocalUserScriptFolder;

        private IReUndoCommand savedUndoCommand;
        private bool NeedSave
        {
            get
            {
                if (CurrentUserFilename == "" && (ReUndoManager.CanRedo() || ReUndoManager.CanUndo()))
                    return true;
                if (savedUndoCommand == ReUndoManager.CurUndoCommand)
                    return false;
                return true;
            }
        }

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
                return FileListButtonContent.Text as string;
            }
            set
            {
                if (value != FileListButtonContent.Text as string)
                {
                    FileListButtonContent.Text = value;

                    if (value == "")
                    {
                        RenameItem.IsEnabled = false;
                        DeleteItem.IsEnabled = false;
                        return;
                    }

                    RenameItem.IsEnabled = true;
                    DeleteItem.IsEnabled = true;

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
            m_LocalUserScriptFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(LocalUserScriptsFolderName, CreationCollisionOption.OpenIfExists);
            m_LocalUserFileFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(LocalUserFilesFolderName, CreationCollisionOption.OpenIfExists);

            var fileList = await m_LocalUserFileFolder.GetFilesAsync();
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
        #endregion

        #region Framework element
        private async void SaveAndApplyButton_Click(object sender, RoutedEventArgs e)
        {
            bool successful = await SaveCurrentUserFile();

            if (!successful)
                return;

            savedUndoCommand = ReUndoManager.CurUndoCommand;
            long StartTime = 0;

            // Apply after saving file
            Log.Debug("[SaveAndApplyButton] Save LastScript");
            StorageFolder localfolder = ApplicationData.Current.LocalFolder;
            StorageFile localsf = await localfolder.CreateFileAsync("LastScript.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(localsf, GetLastScript());
            Log.Debug("[SaveAndApplyButton] Save LastScript successfully : " + localsf.Path);

            Log.Debug("[SaveAndApplyButton] Bef AuraEditorTrigger");
            await (new ServiceViewModel()).AuraEditorTrigger(StartTime);
            Log.Debug("[SaveAndApplyButton] Aft AuraEditorTrigger");

            SendMessageToServer("[XML] Change");
        }
        private async void NewFileButton_Click(object sender, RoutedEventArgs e)
        {
            CanShowDeviceUpdateDialog = false;

            Log.Debug("[NewFile] New File");
            ContentDialogResult result = ContentDialogResult.Secondary;

            if (NeedSave)
            {
                YesNoCancelDialog dialog = new YesNoCancelDialog
                {
                    DialogTitle = resourceLoader.GetString("YesNoCancelDialog_SaveFile"),
                    DialogContent = resourceLoader.GetString("YesNoCancelDialog_SaveHint"),
                    DialogYesButtonContent = resourceLoader.GetString("YesNoCancelDialog_Save"),
                    DialogCancelButtonContent = resourceLoader.GetString("YesNoCancelDialog_Discard")
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

                    savedUndoCommand = ReUndoManager.CurUndoCommand;
                }

                ResetToDefault();
            }
            ForHotkeyFocus.Focus(FocusState.Programmatic);
        }
        private async void RenameItem_Click(object sender, RoutedEventArgs e)
        {
            NamingDialog dialog = new NamingDialog(CurrentUserFilename, GetUserFilenames());
            ContentDialogResult namingResult = await dialog.ShowAsync();

            if (dialog.NamingCancel)
                return;

            if (CurrentUserFilename == dialog.TheName)
                return;

            Log.Debug("[Rename] Old : " + dialog.TheName + " , New : " + CurrentUserFilename);

            StorageFile localscript = await m_LocalUserScriptFolder.GetFileAsync(CurrentUserFilename + ".xml");
            await localscript.RenameAsync(dialog.TheName + ".xml");
            StorageFile localfile = await m_LocalUserFileFolder.GetFileAsync(CurrentUserFilename + ".xml");
            await localfile.RenameAsync(dialog.TheName + ".xml");

            Log.Debug("[Rename] Rename successfully !");

            foreach (var item in FileListMenuFlyout.Items)
            {
                MenuFlyoutItem mfi = item as MenuFlyoutItem;
                if (mfi.Text == CurrentUserFilename)
                {
                    mfi.Text = dialog.TheName;
                    CurrentUserFilename = mfi.Text;
                };
            }

            SendMessageToServer("[XML] Change");
        }
        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUserFilename == "")
                return;

            YesNoCancelDialog dialog = new YesNoCancelDialog
            {
                DialogTitle = resourceLoader.GetString("YesNoCancelDialog_DeleteNow"),
                DialogContent = resourceLoader.GetString("YesNoCancelDialog_DeleteHint"),
                DialogYesButtonContent = resourceLoader.GetString("YesNoCancelDialog_Delete"),
                DialogCancelButtonContent = resourceLoader.GetString("YesNoCancelDialog_Cancel")
            };

            await dialog.ShowAsync();
            ContentDialogResult result = dialog.Result;

            if (result != ContentDialogResult.Primary)
                return;

            Log.Debug("[DeleteFile] Delete file : " + CurrentUserFilename);
            StorageFile localscript = await m_LocalUserScriptFolder.GetFileAsync(CurrentUserFilename + ".xml");
            await localscript.DeleteAsync(StorageDeleteOption.PermanentDelete);
            StorageFile localfile = await m_LocalUserFileFolder.GetFileAsync(CurrentUserFilename + ".xml");
            await localfile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            Log.Debug("[DeleteFile] Delete successfully !");

            foreach (var item in FileListMenuFlyout.Items)
            {
                MenuFlyoutItem mfi = item as MenuFlyoutItem;
                if (mfi.Text == CurrentUserFilename)
                {
                    FileListMenuFlyout.Items.Remove(mfi);
                    break;
                };
            }

            ResetToDefault();
            SendMessageToServer("[XML] Change");
        }
        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            CanShowDeviceUpdateDialog = false;

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
                    DialogTitle = resourceLoader.GetString("YesNoCancelDialog_SaveFile"),
                    DialogContent = resourceLoader.GetString("YesNoCancelDialog_SaveHint"),
                    DialogYesButtonContent = resourceLoader.GetString("YesNoCancelDialog_Save"),
                    DialogCancelButtonContent = resourceLoader.GetString("YesNoCancelDialog_Discard")
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

                    savedUndoCommand = ReUndoManager.CurUndoCommand;
                }

                StorageFile copyfile = await inputFile.CopyAsync(m_LocalUserFileFolder, inputFile.Name, NameCollisionOption.ReplaceExisting);
                await inputFile.CopyAsync(m_LocalUserFileFolder, inputFile.Name, NameCollisionOption.ReplaceExisting);
                Log.Debug("[ImportButton] CopyAsync " + inputFile.Path + " to " + m_LocalUserFileFolder + "\\" + inputFile.Name);

                CurrentUserFilename = copyfile.Name.Replace(".xml", "");
                await LoadUserFile(CurrentUserFilename);
                await SaveCurrentUserFile();

                ReUndoManager.Clear();
                SetLayerCounter = 1;
            }
        }
        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile saveFile = await ShowFileSavePickerAsync();

            if (saveFile != null)
            {
                SpacePage.ClearTempDeviceData();
                await SaveFile(saveFile, GetUserData());
                Log.Debug("[ExportButton] SaveFile : " + saveFile.Path);
            }
        }
        private async void FileItem_Click(object sender, RoutedEventArgs e)
        {
            CanShowDeviceUpdateDialog = false;

            var item = sender as MenuFlyoutItem;
            string selectedName = item.Text;

            if (selectedName == CurrentUserFilename)
                return;

            ContentDialogResult result = ContentDialogResult.Secondary;

            if (NeedSave)
            {
                YesNoCancelDialog dialog = new YesNoCancelDialog
                {
                    DialogTitle = resourceLoader.GetString("YesNoCancelDialog_SaveFile"),
                    DialogContent = resourceLoader.GetString("YesNoCancelDialog_SaveHint"),
                    DialogYesButtonContent = resourceLoader.GetString("YesNoCancelDialog_Save"),
                    DialogCancelButtonContent = resourceLoader.GetString("YesNoCancelDialog_Discard")
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

                    savedUndoCommand = ReUndoManager.CurUndoCommand;
                }

                Log.Debug("[FileItem_Click] Selected file name : " + selectedName);
                await LoadUserFile(selectedName);
                CurrentUserFilename = selectedName;

                ReUndoManager.Clear();
                SetLayerCounter = 1;
            }
        }

        private async void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            ContentDialog cDialog = GetCurrentContentDialog();
            e.Handled = true;

            if (cDialog != null)
            {
                cDialog.Hide();
                return;
            }

            CanShowDeviceUpdateDialog = false;

            e.Handled = true;
            ContentDialogResult result = ContentDialogResult.Secondary;

            if (NeedSave)
            {
                YesNoCancelDialog dialog = new YesNoCancelDialog
                {
                    DialogTitle = resourceLoader.GetString("YesNoCancelDialog_SaveFile"),
                    DialogContent = resourceLoader.GetString("YesNoCancelDialog_SaveHint"),
                    DialogYesButtonContent = resourceLoader.GetString("YesNoCancelDialog_Save"),
                    DialogCancelButtonContent = resourceLoader.GetString("YesNoCancelDialog_Discard")
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
                Log.Debug("[OnCloseRequest] Exit ...");
                CoreApplication.Exit();
            }
        }
        private void SaveSettings()
        {
            g_LocalSettings.Values["SpaceZooming"] = SpacePage.GetSpaceZoomPercent().ToString();
            g_LocalSettings.Values["LayerLevel"] = LayerPage.LayerZoomSlider.Value.ToString();
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
            StorageFile localfile;
            StorageFile localscript;

            if (CurrentUserFilename == "")
            {
                NamingDialog dialog = new NamingDialog(GetUserFilenames());
                await dialog.ShowAsync();

                if (dialog.Result != true)
                {
                    return false;
                }

                CurrentUserFilename = dialog.TheName;
            }
            else
            {
                CanShowDeviceUpdateDialog = true;
                ShowDeviceUpdateDialogOrNot();
            }

            Log.Debug("[SaveCurrentUserFile] Save file name : " + CurrentUserFilename);
            SpacePage.ClearTempDeviceData();
            localfile = await m_LocalUserFileFolder.CreateFileAsync(CurrentUserFilename + ".xml", CreationCollisionOption.OpenIfExists);
            localscript = await m_LocalUserScriptFolder.CreateFileAsync(CurrentUserFilename + ".xml", CreationCollisionOption.OpenIfExists);
            await SaveFile(localfile, GetUserData());
            await SaveFile(localscript, GetLastScript());
            Log.Debug("[SaveCurrentUserFile] Save successfully");

            return true;
        }
        private string GetUserData()
        {
            XmlNode root = CreateXmlNode("root");

            XmlNode versionNode = CreateXmlNode("version");
            versionNode.InnerText = "1.0";
            root.AppendChild(versionNode);

            root.AppendChild(SpacePage.ToXmlNodeForUserData());
            root.AppendChild(LayerPage.ToXmlNodeForUserData());

            return root.OuterXml;
        }

        private async Task LoadUserFile(string filename)
        {
            StorageFile localfile = await m_LocalUserFileFolder.CreateFileAsync(filename + ".xml", CreationCollisionOption.OpenIfExists);

            Clean();
            LoadContent(await LoadFile(localfile));

            SpacePage.SendSyncStateToService();
            SpacePage.RefreshStage();

            ReUndoManager.Clear();
        }
        private void LoadContent(string xmlContent)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlContent);

            XmlNode spaceNode = xml.SelectSingleNode("/root/space");
            XmlNode layersNode = xml.SelectSingleNode("/root/layers");

            XmlNodeList deviceNodes = spaceNode.SelectNodes("device");
            XmlNodeList layerNodes = layersNode.SelectNodes("layer");

            ParsingDevices(deviceNodes);
            ParsingLayers(layerNodes);
        }
        private void ParsingDevices(XmlNodeList deviceNodes)
        {
            List<DeviceModel> remains = new List<DeviceModel>(SpacePage.DeviceModelCollection);

            foreach (XmlNode node in deviceNodes)
            {
                XmlElement element = (XmlElement)node;
                string folderName = element.GetAttribute("folder");
                DeviceModel get = remains.Find(d => d.FolderName == folderName);
                int x = Int32.Parse(element.SelectSingleNode("x").InnerText);
                int y = Int32.Parse(element.SelectSingleNode("y").InnerText);

                if (get != null)
                {
                    // Because notebook csv file maybe perkey or 4zone or single, even they have the same model name.
                    // If different, ignore it.
                    if (CompareCsv(element.GetAttribute("csv"), get.CsvName))
                    {
                        get.Sync = true;
                        get.Plugged = true;
                        get.PixelLeft = x * GridPixels;
                        get.PixelTop = y * GridPixels;
                    }

                    remains.Remove(get);
                }
                else
                {
                    //DeviceModel dm = await DeviceModel.ToDeviceModelAsync(node);
                    //dm.PixelLeft = x * GridPixels;
                    //dm.PixelTop = y * GridPixels;
                    //dm.Sync = false;
                    //dm.Status = DeviceStatus.Temp;

                    //SpacePage.DeviceModelCollection.Add(dm);
                }
            }

            foreach (var dm in remains)
            {
                dm.Sync = false;
            }
        }

        private bool CompareCsv(string csv1, string csv2)
        {
            int type1 = 0, type2 = 0;

            if (csv1.ToLower().Contains("perkey")) type1 = 1;
            if (csv1.ToLower().Contains("zone")) type1 = 2;
            if (csv1.ToLower().Contains("single")) type1 = 3;

            if (csv2.ToLower().Contains("perkey")) type2 = 1;
            if (csv2.ToLower().Contains("zone")) type2 = 2;
            if (csv2.ToLower().Contains("single")) type2 = 3;

            return (type1 == type2);
        }

        private void ParsingLayers(XmlNodeList layerNodes)
        {
            foreach (XmlNode node in layerNodes)
            {
                XmlElement element = (XmlElement)node;
                string layerName = element.GetAttribute("name");
                string eye = element.GetAttribute("Eye");
                LayerModel layer = new LayerModel(layerName);
                layer.Eye = bool.Parse(eye);

                layer.TriggerAction = element.GetAttribute("trigger");

                // parsing effects
                XmlNode effectsNode = element.SelectSingleNode("effects");
                foreach (XmlNode effectNode in effectsNode.ChildNodes)
                {
                    XmlElement element2 = (XmlElement)effectNode;
                    int type = Int32.Parse(element2.SelectSingleNode("type").InnerText);

                    List<ColorPointModel> colorPoints = new List<ColorPointModel>();
                    XmlNode colorPointListNode = element2.SelectSingleNode("colorPointList");
                    foreach (XmlNode colorpoint in colorPointListNode.ChildNodes)
                    {
                        ColorPointModel cp = new ColorPointModel();
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

                    EffectInfoModel info = new EffectInfoModel(type)
                    {
                        InitColor = new Color
                        {
                            A = Byte.Parse(element2.SelectSingleNode("a").InnerText),
                            R = Byte.Parse(element2.SelectSingleNode("r").InnerText),
                            G = Byte.Parse(element2.SelectSingleNode("g").InnerText),
                            B = Byte.Parse(element2.SelectSingleNode("b").InnerText),
                        },
                        DoubleColor1 = new Color
                        {
                            A = Byte.Parse(element2.SelectSingleNode("d1a").InnerText),
                            R = Byte.Parse(element2.SelectSingleNode("d1r").InnerText),
                            G = Byte.Parse(element2.SelectSingleNode("d1g").InnerText),
                            B = Byte.Parse(element2.SelectSingleNode("d1b").InnerText),
                        },
                        DoubleColor2 = new Color
                        {
                            A = Byte.Parse(element2.SelectSingleNode("d2a").InnerText),
                            R = Byte.Parse(element2.SelectSingleNode("d2r").InnerText),
                            G = Byte.Parse(element2.SelectSingleNode("d2g").InnerText),
                            B = Byte.Parse(element2.SelectSingleNode("d2b").InnerText),
                        },
                        Type = type,
                        Speed = Int32.Parse(element2.SelectSingleNode("speed").InnerText),
                        Angle = Int32.Parse(element2.SelectSingleNode("angle").InnerText),
                        RandomRangeMax = Int32.Parse(element2.SelectSingleNode("randomRangeMax").InnerText),
                        RandomRangeMin = Int32.Parse(element2.SelectSingleNode("randomRangeMin").InnerText),
                        ColorModeSelection = Int32.Parse(element2.SelectSingleNode("colormodeselection").InnerText),
                        High = Int32.Parse(element2.SelectSingleNode("high").InnerText),
                        Low = Int32.Parse(element2.SelectSingleNode("low").InnerText),
                        PatternSelect = Int32.Parse(element2.SelectSingleNode("patternSelect").InnerText),
                        CustomizedPattern = new List<ColorPointModel>(colorPoints),
                        ColorSegmentation = bool.Parse(element2.SelectSingleNode("colorSegmentation").InnerText),
                        RainbowSpecialEffects = bool.Parse(element2.SelectSingleNode("rainbowRotation").InnerText),
                        RainbowSpecialMode = Int32.Parse(element2.SelectSingleNode("rotationMode").InnerText),
                    };

                    if (!IsTriggerEffect(type))
                    {
                        TimelineEffect eff = new TimelineEffect(type);
                        eff.StartTime = Int32.Parse(element2.SelectSingleNode("start").InnerText);
                        eff.DurationTime = Int32.Parse(element2.SelectSingleNode("duration").InnerText);
                        eff.Info = info;
                        layer.AddTimelineEffect(new EffectLineViewModel(eff));
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

                LayerPage.AddLayer(layer);
            }
        }

        private void ResetToDefault()
        {
            SetLayerCounter = 1;
            Clean();
            CurrentUserFilename = "";
            SpacePage.FillCurrentIngroupDevices();
            ReUndoManager.Clear();
        }
        private void Clean()
        {
            savedUndoCommand = null;
            SetLayerButton.IsEnabled = true;
            SetLayerRectangle.Visibility = Visibility.Collapsed;
            LayerPage.Clean();
            SpacePage.Clean();
        }
    }
}

