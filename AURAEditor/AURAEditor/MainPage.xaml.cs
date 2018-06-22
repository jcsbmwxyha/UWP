using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Input;
using Windows.UI.Xaml.Documents;
using MoonSharp.Interpreter;
using Windows.Storage.Pickers;
using AuraEditor.Common;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404

namespace AuraEditor
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    
    public class DeviceItem
    {
        public string DeviceName { get; set; }
        public string Image { get; set; }
        public int[,] LightRegions { get; set; }
        public int[] SelectedIndexes { get; set; }
        public int Mode { get; set; }

        public DeviceItem(string deviceName)
        {
            Mode = 1; // All
            DeviceName = deviceName;
        }
    }

    public sealed partial class MainPage : Page
    {
        DeviceGroupManager _deviceGroupManager;
        int timeLineSliderValue = 25;
        public int TimeLineZoomSliderValue
        {
            get { return timeLineSliderValue; }
            set
            {
                if (value >= 50 && timeLineSliderValue == 25)
                {
                    timeLineSliderValue = 75;
                }
                else if ((value < 50 && timeLineSliderValue == 75))
                {
                    timeLineSliderValue = 25;
                }
                else
                    return;
            }
        }
        ObservableCollection<DeviceItem> _devicelist;

        public MainPage()
        {
            this.InitializeComponent();
            EffectListView.ItemsSource = EffectHelper.GetCommonEffectList();
            TriggerEventListView.ItemsSource = EffectHelper.GetTriggerEffectList();
            OtherTriggerEventListView.ItemsSource = EffectHelper.GetOtherTriggerEffectList();
            SpaceLine.Visibility = Visibility.Collapsed;
            _deviceGroupManager = new DeviceGroupManager(TimeLineStackPanel);
            _devicelist = GetCurrentDevices();
            UpdateSpaceGrid();
        }

        private ObservableCollection<DeviceItem> GetCurrentDevices()
        {
            DeviceItem d;
            Device device;
            ObservableCollection<DeviceItem> devicelist = new ObservableCollection<DeviceItem>();

            // NB Example
            d = new DeviceItem("Notebook")
            {
                Image = "ms-appx:///Assets/asus_gc_aura_customize_keyboard_g703_mask.png",
                LightRegions = new int[,] {
                    // key 0 ~ 9
                    { 23, 94, 67, 123} ,
                    { 116, 94, 160, 123} ,
                    { 162, 94, 206, 123} ,
                    { 209, 94, 253, 123} ,
                    { 255, 94, 299, 123} ,
                    { 325, 94, 369, 123} ,
                    { 372, 94, 416, 123} ,
                    { 418, 94, 462, 123} ,
                    { 465, 94, 509, 123} ,
                    { 534, 94, 578, 123} ,
                    // key 10 ~ 19
                    { 581, 94, 625, 123} ,
                    { 627, 94, 671, 123} ,
                    { 674, 94, 718, 123} ,
                    { 735, 94, 779, 123} ,
                    { 781, 94, 825, 123} ,
                    { 828, 94, 872, 123} ,
                    { 874, 94, 918, 123} ,
                    { 23, 125, 67, 168} ,
                    { 69, 125, 113, 168} ,
                    { 116, 125, 160, 168} ,
                    // key 20 ~ 29
                    { 162, 125, 206, 168} ,
                    { 209, 125, 253, 168} ,
                    { 255, 125, 299, 168} ,
                    { 302, 125, 346, 168} ,
                    { 348, 125, 392, 168} ,
                    { 395, 125, 439, 168} ,
                    { 441, 125, 485, 168} ,
                    { 488, 125, 532, 168} ,
                    { 534, 125, 578, 168} ,
                    { 581, 125, 625, 168} ,
                    // key 30 ~ 39
                    { 627, 125, 718, 168} ,
                    { 735, 125, 779, 168} ,
                    { 781, 125, 825, 168} ,
                    { 828, 125, 872, 168} ,
                    { 874, 125, 918, 168} ,
                    { 23, 171, 90, 214} ,
                    { 92, 171, 136, 214} ,
                    { 139, 171, 183, 214} ,
                    { 185, 171, 229, 214} ,
                    { 232, 171, 276, 214} ,
                    // key 40 ~ 49
                    { 278, 171, 322, 214} ,
                    { 325, 171, 369, 214} ,
                    { 371, 171, 416, 214} ,
                    { 418, 171, 462, 214} ,
                    { 464, 171, 509, 214} ,
                    { 511, 171, 555, 214} ,
                    { 557, 171, 602, 214} ,
                    { 604, 171, 648, 214} ,
                    { 651, 171, 718, 214} ,
                    { 735, 171, 779, 214} ,
                    // key 50 ~ 59
                    { 781, 171, 825, 214} ,
                    { 828, 171, 872, 214} ,
                    { 874, 171, 918, 259} ,
                    { 23, 216, 102, 259} ,
                    { 104, 216, 148, 259} ,
                    { 150, 216, 195, 259} ,
                    { 197, 216, 241, 259} ,
                    { 243, 216, 288, 259} ,
                    { 290, 216, 334, 259} ,
                    { 336, 216, 381, 259} ,
                    // key 60 ~ 69
                    { 383, 216, 427, 259} ,
                    { 429, 216, 474, 259} ,
                    { 476, 216, 520, 259} ,
                    { 522, 216, 567, 259} ,
                    { 569, 216, 613, 259} ,
                    { 616, 216, 718, 259} ,
                    { 735, 216, 779, 259} ,
                    { 781, 216, 825, 259} ,
                    { 828, 216, 872, 259} ,
                    { 23, 262, 125, 305} ,
                    // key 70 ~ 79
                    { 127, 262, 171, 305} ,
                    { 174, 262, 218, 305} ,
                    { 220, 262, 264, 305} ,
                    { 267, 262, 311, 305} ,
                    { 313, 262, 357, 305} ,
                    { 360, 262, 404, 305} ,
                    { 406, 262, 450, 305} ,
                    { 453, 262, 497, 305} ,
                    { 499, 262, 543, 305} ,
                    { 546, 262, 590, 305} ,
                    // key 80 ~ 89
                    { 592, 262, 718, 305} ,
                    { 735, 262, 779, 305} ,
                    { 781, 262, 825, 305} ,
                    { 828, 262, 872, 305} ,
                    { 874, 262, 918, 350} ,
                    { 23, 307, 78, 350} ,
                    { 81, 307, 125, 350} ,
                    { 127, 307, 171, 350} ,
                    { 174, 307, 218, 350} ,
                    { 220, 307, 451, 360} ,
                    // key 90 ~ 99
                    { 453, 307, 497, 350} ,
                    { 499, 307, 543, 350} ,
                    { 546, 307, 590, 350} ,
                    { 592, 307, 718, 350} ,
                    { 735, 307, 779, 350} ,
                    { 781, 307, 825, 350} ,
                    { 828, 307, 872, 350} ,
                    { 688, 352, 732, 395} ,
                    { 735, 352, 779, 395} ,
                    { 781, 352, 825, 395} ,
                    // key 100 ~ 109
                    { 834, 8, 918, 62} ,
                }
            };
            devicelist.Add(d);

            // Mouse Example
            d = new DeviceItem("Mouse")
            {
                Image = "ms-appx:///Assets/201701050624411245.png",
                LightRegions = new int[,] {
                    {286, 229, 433, 462},
                    {450, 300, 496, 421},
                    {516, 229, 663, 462},
                }
            };
            devicelist.Add(d);

            // Keyboard Example
            d = new DeviceItem("Keyboard")
            {
                Image = "ms-appx:///Assets/Flaire.png",
                LightRegions = new int[,] {
                    // key 0 ~ 9
                    { 18 , 89 , 58, 118} ,
                    { 97 , 89 , 138, 118} ,
                    { 136, 89 , 177, 118} ,
                    { 176, 89 , 217, 118} ,
                    { 215, 89 , 256, 118} ,
                    { 275, 89 , 315, 118} ,
                    { 315, 89 , 355, 118} ,
                    { 354, 89 , 394, 118} ,
                    { 394, 89 , 434, 118} ,
                    { 453, 89 , 493, 118} ,
                    { 492, 89 , 532, 118} ,
                    { 532, 89 , 571, 118} ,
                    { 572, 89 , 611, 118} ,
                    { 625, 87 , 665, 116} ,
                    { 664, 87 , 704, 116} ,
                    { 704, 87 , 744, 116} ,

                    { 18 , 119, 58, 161} ,
                    { 57 , 119, 98, 161} ,
                    { 97 , 119, 138, 161} ,
                    { 136, 119, 177, 161} ,
                    { 176, 119, 217, 161} ,
                    { 215, 119, 256, 161} ,
                    { 255, 119, 296, 161} ,
                    { 294, 119, 335, 161} ,
                    { 334, 119, 375, 161} ,
                    { 373, 119, 414, 161} ,
                    { 413, 119, 453, 161} ,
                    { 453, 119, 493, 161} ,
                    { 492, 119, 532, 161} ,
                    { 532, 119, 611, 161} ,
                    { 625, 119, 665, 161} ,
                    { 664, 119, 704, 161} ,
                    { 704, 119, 744, 161} ,
                    { 749, 117, 789, 159} ,
                    { 788, 117, 828, 159} ,
                    { 828, 117, 868, 159} ,
                    { 868, 117, 907, 159} ,

                    { 18 , 163, 78, 205} ,
                    { 77 , 163, 117, 205} ,
                    { 117, 163, 157, 205} ,
                    { 156, 163, 196, 205} ,
                    { 196, 163, 236, 205} ,
                    { 235, 163, 275, 205} ,
                    { 275, 163, 315, 205} ,
                    { 314, 163, 354, 205} ,
                    { 354, 163, 394, 205} ,
                    { 393, 163, 433, 205} ,
                    { 433, 163, 473, 205} ,
                    { 472, 163, 513, 205} ,
                    { 512, 163, 552, 205} ,
                    { 552, 163, 611, 205} ,
                    { 625, 162, 665, 204} ,
                    { 664, 162, 704, 204} ,
                    { 704, 162, 744, 204} ,
                    { 749, 161, 789, 203} ,
                    { 788, 161, 828, 203} ,
                    { 828, 161, 868, 203} ,
                    { 868, 161, 907, 246} ,

                    { 18 , 206, 88, 248} ,
                    { 87 , 206, 127, 248} ,
                    { 126, 206, 167, 248} ,
                    { 166, 206, 206, 248} ,
                    { 205, 206, 246, 248} ,
                    { 245, 206, 285, 248} ,
                    { 285, 206, 325, 248} ,
                    { 325, 206, 364, 248} ,
                    { 364, 206, 404, 248} ,
                    { 404, 206, 443, 248} ,
                    { 443, 206, 483, 248} ,
                    { 483, 206, 522, 248} ,
                    { 523, 206, 611, 248} ,
                    { 749, 204, 789, 246} ,
                    { 788, 204, 828, 246} ,
                    { 828, 204, 868, 246} ,

                    { 18 , 250, 107, 292} ,
                    { 107, 250, 146, 292} ,
                    { 147, 250, 186, 292} ,
                    { 186, 250, 226, 292} ,
                    { 226, 250, 266, 292} ,
                    { 265, 250, 305, 292} ,
                    { 305, 250, 345, 292} ,
                    { 344, 250, 384, 292} ,
                    { 384, 250, 424, 292} ,
                    { 423, 250, 463, 292} ,
                    { 463, 250, 503, 292} ,
                    { 502, 250, 611, 292} ,
                    { 663, 250, 703, 292} ,
                    { 749, 248, 789, 290} ,
                    { 788, 248, 828, 290} ,
                    { 828, 248, 868, 290} ,
                    { 868, 248, 907, 333} ,

                    { 18 , 293, 67, 335} ,
                    { 68 , 293, 121, 335} ,
                    { 119, 293, 177, 335} ,
                    { 175, 293, 385, 345} ,
                    { 384, 293, 424, 335} ,
                    { 423, 293, 463, 335} ,
                    { 463, 293, 503, 335} ,
                    { 528, 293, 612, 335} ,
                    { 623, 291, 663, 333} ,
                    { 663, 291, 703, 333} ,
                    { 702, 291, 742, 333} ,
                    { 748, 291, 829, 333} ,
                    { 828, 291, 868, 333} ,
                }
            };

            for (int i = 0; i < d.LightRegions.GetLength(0); i++)
            {
                d.LightRegions[i, 0] += 8;
                d.LightRegions[i, 1] += 42;
                d.LightRegions[i, 2] += 8;
                d.LightRegions[i, 3] += 42;
            }
            devicelist.Add(d);

            // HeadSet Example
            d = new DeviceItem("Headset")
            {
                Image = "ms-appx:///Assets/ROG-Strix-Wireless-front.png",
                LightRegions = new int[,] {
                    {234, 198, 448, 428},
                    {478, 198, 692, 428},
                }
            };
            devicelist.Add(d);

            // TODO : combine DeviceItem class and Device class
            device = new Device("Notebook", 0, 0, 0);
            _deviceGroupManager.GlobalDevices.Add(device);
            device = new Device("Mouse", 1, 0, 1);
            _deviceGroupManager.GlobalDevices.Add(device);
            device = new Device("Keyboard", 2, 1, 0);
            _deviceGroupManager.GlobalDevices.Add(device);
            device = new Device("Headset", 3, 1, 1);
            _deviceGroupManager.GlobalDevices.Add(device);

            return devicelist;
        }

        private void EffectRadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            FrameworkElement fe;

            if (rb.Name == "EffectRadioButton")
                fe = EffectListView;
            else if (rb.Name == "EventRadioButton")
                fe = EventStackPanel;
            else if (rb.Name == "TriggerEventRadioButton")
                fe = TriggerEventListView;
            else // OtherTriggerEventToggleButton
                fe = OtherTriggerEventListView;

            if (fe == null)
                return;
            else if (fe.Visibility == Visibility.Visible)
                fe.Visibility = Visibility.Collapsed;
            else
                fe.Visibility = Visibility.Visible;
        }
        private void EffectListView_DragStarting(object sender, DragItemsStartingEventArgs e)
        {
            var item = e.Items[0] as string;
            e.Data.SetText(item);
            e.Data.RequestedOperation = DataPackageOperation.Copy;
        }

        async private void ShowMess(string res)
        {
            var messDialog = new MessageDialog(res);
            await messDialog.ShowAsync();
        }
        public void UpdateEventLog(string s)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            eventLog.TextWrapping = TextWrapping.Wrap;
            run.Text = s;
            paragraph.Inlines.Add(run);
            eventLog.Blocks.Insert(0, paragraph);
        }

        private void DeleteItem_DragEnter(object sender, DragEventArgs e)
        {
            // Trash only accepts text
            e.AcceptedOperation = DataPackageOperation.Move;
            // We don't want to show the Move icon
            e.DragUIOverride.IsGlyphVisible = false;
            e.DragUIOverride.Caption = "Drop item here to remove it from selection";
        }
        private void DeleteItem_Drop(object sender, DragEventArgs e)
        {
            //if (e.DataView.Contains(StandardDataFormats.Text))
            //{
            //    canDelete = true;
            //}
        }

        private async void AddDeviceButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            AddDeviceDialog dialog = new AddDeviceDialog(_devicelist);
            var result = await dialog.ShowAsync();
        }

        public async void AddDeviceFinished(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            DeviceGroup dg = new DeviceGroup();
            AddDeviceDialog d = AddDeviceDialog.GetInstance();
            bool none = true;

            foreach (DeviceItem di in d.DeviceList)
            {
                if (di.Mode == 1 || di.Mode == 2)
                {
                    none = false;
                    break;
                }
            }

            if (none == true)
                return;

            NamedDialog namedDialog = new NamedDialog(null);
            var result = await namedDialog.ShowAsync();
            if (result == ContentDialogResult.None)
                return;

            dg.GroupName = namedDialog.CustomizeName;
            if (dg.GroupName == "")
                return;

            foreach (DeviceItem di in d.DeviceList)
            {
                int type = 0;
                string devicename = di.DeviceName;

                if (di.Mode == 0)
                    continue;

                if (devicename == "Notebook")
                    type = 0;
                else if (devicename == "Mouse")
                    type = 1;
                else if (devicename == "Keyboard")
                    type = 2;
                else if (devicename == "Headset")
                    type = 3;
                else
                    continue;

                if (di.Mode == 1)
                {
                    dg.AddDeviceZones(type, new int[] { -1 }); // -1 means all
                }
                else if (di.Mode == 2)
                {
                    dg.AddDeviceZones(type, di.SelectedIndexes);
                }
            }

            _deviceGroupManager.AddDeviceGroup(dg);
            TimeLineDeviceNameListView.SelectedIndex = 0;
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            LeftScrollViewer.ChangeView(null, sv.VerticalOffset, null, true);
        }

        private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".lua");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            var inputFile = await fileOpenPicker.PickSingleFileAsync();

            if (inputFile == null)
            {
                // The user cancelled the picking operation
                return;
            }

            string luaScript = await Windows.Storage.FileIO.ReadTextAsync(inputFile);
            luaScript = luaScript.Replace("require(\"script//global\")", "");

            Script script = new Script();

            DynValue script_dv;
            Table eventprovider_table;
            Table viewport_table;
            Table event_table;
            
            script_dv = script.DoString(luaScript + "\nreturn EventProvider");
            eventprovider_table = script_dv.Table;
            script_dv = script.DoString(luaScript + "\nreturn Viewport");
            viewport_table = script_dv.Table;
            script_dv = script.DoString(luaScript + "\nreturn Event");
            event_table = script_dv.Table;

            _deviceGroupManager.ClearAllGroup();
            List<DeviceGroup> devicegroups = ParsingEventProviderTable(eventprovider_table);

            if (devicegroups.Count == 0)
                return;

            foreach(var dg in devicegroups)
            {
                List<Device> devices = GetDevicesFromViewportTable(viewport_table, dg.GroupName);
                //dg.SetDevices(devices);

                foreach (var effect in dg.Effects)
                {
                    EffectInfo ei = GetEffectInfoFromEventTable(event_table, effect.EffectLuaName);
                    effect.Info = ei;
                }
                _deviceGroupManager.AddDeviceGroup(dg);
            }

            TimeLineDeviceNameListView.SelectedIndex = 0;
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\ProgramData\\ASUS");
            folder = await CheckOrCreateFolder(folder, "RogAuraEditor");
            StorageFile sf =
                await folder.CreateFileAsync("script.lua", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            await Windows.Storage.FileIO.WriteTextAsync(sf, _deviceGroupManager.PrintLuaScript());
        }
        private async void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".lua" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "MyLuaScript";

            StorageFile saveFile = await savePicker.PickSaveFileAsync();

            if (saveFile != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(saveFile);
                // write to file
                await Windows.Storage.FileIO.WriteTextAsync(saveFile, _deviceGroupManager.PrintLuaScript());
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(saveFile);
            }
        }
        private async Task<StorageFolder> CheckOrCreateFolder(StorageFolder sf, string folderName)
        {
            IReadOnlyList<StorageFolder> folderList = await sf.GetFoldersAsync();

            foreach (StorageFolder folder in folderList)
            {
                if (folder.Name == folderName)
                    return folder;
            }

            return await sf.CreateFolderAsync(folderName);
        }
        private List<Device> GetDevicesFromViewportTable(Table viewport_table, string groupName)
        {
            List<Device> devices = new List<Device>();
            Table groupTable = viewport_table.Get(groupName).Table;

            foreach (var deviceKey in groupTable.Keys)
            {
                Table deviceTable = groupTable.Get(deviceKey.String).Table;
                string deviceName = deviceTable.Get("name").String;

                int type = 0;
                switch (deviceTable.Get("DeviceType").String)
                {
                    case "Notebook": type = 0; break;
                    case "Mouse": type = 1; break;
                    case "Keyboard": type = 2; break;
                    case "Headset": type = 3; break;
                }

                //Table layoutTable = deviceTable.Get("layout").Table;
                Table locationTable = deviceTable.Get("location").Table;

                int x = (int)locationTable.Get("x").Number;
                int y = (int)locationTable.Get("y").Number;

                //Device d = new Device(deviceName, type, x, y)
                //{
                //    W = layoutTable.Get("weight").Number,
                //    H = layoutTable.Get("height").Number,
                //};
                Device d = new Device(deviceName, type, x, y);
                devices.Add(d);
            }

            return devices;
        }
        private List<DeviceGroup> ParsingEventProviderTable(Table eventProviderTable)
        {
            List<DeviceGroup> groups = new List<DeviceGroup>();
            Table queueTable = eventProviderTable.Get("queue").Table;

            for (int queueIndex = 1; queueIndex <= queueTable.Length; queueIndex++)
            {
                Table t = queueTable.Get(queueIndex).Table;
                string groupName = t.Get("Viewport").String;

                DeviceGroup dg = groups.Find(x => x.GroupName == groupName);

                if (dg == null)
                {
                    dg = new DeviceGroup(groupName);
                    groups.Add(dg);
                }

                string effectLuaName = t.Get("Effect").String;
                double start = t.Get("Delay").Number;
                double duration = t.Get("Duration").Number;
                int type = EffectHelper.GetEffectIndex(effectLuaName);

                Effect effect = new Effect(dg, type)
                {
                    EffectLuaName = effectLuaName,
                    Start = (int)start / 10,
                    Duration = (int)duration / 10
                };
                dg.AddEffect(effect);
            }

            return groups;
        }
        private EffectInfo GetEffectInfoFromEventTable(Table eventTable, string effectLuaName)
        {
            Table effectTable = eventTable.Get(effectLuaName).Table;
            Table colorTable = effectTable.Get("initColor").Table;
            Table waveTable = effectTable.Get("wave").Table;

            EffectInfo ei = new EffectInfo();

            if (!effectLuaName.Contains("Rainbow") && !effectLuaName.Contains("Smart") && !effectLuaName.Contains("ColorCycle"))
            {
                ei.Color = AuraEditorColorHelper.HslToRgb(
                    colorTable.Get("alpha").Number,
                    colorTable.Get("hue").Number,
                    colorTable.Get("saturation").Number,
                    colorTable.Get("lightness").Number
                    );
            }

            switch (waveTable.Get("waveType").String)
            {
                case "SineWave": ei.WaveType = 0; break;
                case "HalfSineWave": ei.WaveType = 1; break;
                case "QuarterSineWave": ei.WaveType = 2; break;
                case "SquareWave": ei.WaveType = 3; break;
                case "TriangleWave": ei.WaveType = 4; break;
                case "SawToothleWave": ei.WaveType = 5; break;
            }

            ei.Min = waveTable.Get("min").Number;
            ei.Max = waveTable.Get("max").Number;
            ei.WaveLength = waveTable.Get("waveLength").Number;
            ei.Freq = waveTable.Get("freq").Number;
            ei.Phase = waveTable.Get("phase").Number;
            ei.Start = waveTable.Get("start").Number;
            ei.Velocity = waveTable.Get("velocity").Number;

            return ei;
        }

        private void TimeLineZoomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            TimeLineZoomSliderValue = (int)TimeLineZoomSlider.Value;
        }

        private void DeviceList_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //ListView lv = sender as ListView;
            //int index = lv.SelectedIndex;

            //if (index >= 0)
            //    UpdateSpaceGrid(_deviceGroupManager.DeviceGroupCollection[index]);
        }
        private void UpdateSpaceGrid()
        {
            SpacePanel.Children.Clear();
            SpacePanel.Children.Add(GridImage);
            List<Device> devices = _deviceGroupManager.GlobalDevices;

            foreach (Device d in devices)
            {
                SpacePanel.Children.Add(d.DeviceImg);
            }
        }
        //private void UpdateSpaceGrid(DeviceGroup dg)
        //{
        //    SpacePanel.Children.Clear();
        //    SpacePanel.Children.Add(GridImage);
        //    List<Device> devices = dg.GetDevices();

        //    foreach (Device d in devices)
        //    {
        //        SpacePanel.Children.Add(d.DeviceImg);
        //    }
        //}

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile sf = await StorageFile.GetFileFromPathAsync("C:\\ProgramData\\ASUS\\RogAuraEditor\\Flaire.jpg");
            Image img = new Image();

            if (sf != null)
            {
                // Open a stream for the selected file.
                // The 'using' block ensures the stream is disposed
                // after the image is loaded.
                using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                    await sf.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    // Set the image source to the selected bitmap.
                    Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage =
                        new Windows.UI.Xaml.Media.Imaging.BitmapImage();

                    bitmapImage.SetSource(fileStream);
                    img.Source = bitmapImage;
                }

                SpacePanel.Children.Add(img);
            }
        }

        private void TrashCanButton_Click(object sender, RoutedEventArgs e)
        {
            //_deviceGroupManager.CurrentEffect.MyDeviceGroup.de
        }
    }
}

