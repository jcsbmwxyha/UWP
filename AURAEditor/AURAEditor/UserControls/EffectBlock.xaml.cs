using System;
using AuraEditor.Common;
using System.ComponentModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static AuraEditor.Common.EffectHelper;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AuraEditor.UserControls
{
    public sealed partial class EffectBlock : UserControl, INotifyPropertyChanged
    {
        public int Idx
        {
            get
            {
                if (DataContext == null)
                    return -1;

                return (int)DataContext;
            }
        }
        public string EffName
        {
            get
            {
                return GetLanguageNameByIdx(Idx);
            }
        }
        public bool Dragging;
        static public Point LastDraggingPoint;
        public event PropertyChangedEventHandler PropertyChanged;
        private ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

        public EffectBlock()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => RaisePropertyChanged("EffName");
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerOver", false);
        }
        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (Dragging != true)
                VisualStateManager.GoToState(this, "Normal", false);
        }
        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint ptrPt = e.GetCurrentPoint(sender as UIElement);
            LastDraggingPoint = ptrPt.Position;
            VisualStateManager.GoToState(this, "Pressed", false);
        }
        private void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerOver", false);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //MyText get value in load step
            EffectBlockBackground_Normal = "ms-appx:///Assets/EffectBlock/asus_ac_" + GetEffEngNameByIdx(Idx) + "_btn_n.png";
            EffectBlockBackground_Pressed = "ms-appx:///Assets/EffectBlock/asus_ac_" + GetEffEngNameByIdx(Idx) + "_btn_s.png";
        }

        private string _effectBlockBackground_Normal = "ms-appx:///Assets/EffectBlock/";
        private string _effectBlockBackground_Pressed = "ms-appx:///Assets/EffectBlock/";
        public string EffectBlockBackground_Normal
        {
            get { return _effectBlockBackground_Normal; }
            set
            {
                if (_effectBlockBackground_Normal != value)
                {
                    _effectBlockBackground_Normal = value;
                    RaisePropertyChanged("EffectBlockBackground_Normal");
                }
            }
        }
        public string EffectBlockBackground_Pressed
        {
            get { return _effectBlockBackground_Pressed; }
            set
            {
                if (_effectBlockBackground_Pressed != value)
                {
                    _effectBlockBackground_Pressed = value;
                    RaisePropertyChanged("EffectBlockBackground_Pressed");
                }
            }
        }
        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
