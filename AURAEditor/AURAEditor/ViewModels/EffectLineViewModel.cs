using AuraEditor.Models;
using AuraEditor.Pages;
using AuraEditor.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.EffectHelper;

namespace AuraEditor.ViewModels
{
    public class EffectLineViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public TimelineEffect Model;
        public LayerModel Layer { get; set; }

        public string Name
        {
            get
            {
                return Model.Name;
            }
        }
        public double Left
        {
            get
            {
                double seconds = StartTime / 1000;
                return seconds * LayerPage.PixelsPerSecond;
            }
            set
            {
                double seconds = value / LayerPage.PixelsPerSecond;
                StartTime = seconds * 1000;
                RaisePropertyChanged("Left");
            }
        }
        public double Width
        {
            get
            {
                double seconds = DurationTime / 1000;
                return seconds * LayerPage.PixelsPerSecond;
            }
            set
            {
                double seconds = value / LayerPage.PixelsPerSecond;
                DurationTime = seconds * 1000;
                RaisePropertyChanged("Width");
                RecalculateStringLength();
            }
        }
        public double Right
        {
            get
            {
                return Left + Width;
            }
        }
        public double StartTime
        {
            get
            {
                return Model.StartTime;
            }
            set
            {
                Model.StartTime = value;
                RaisePropertyChanged("Left");
            }
        }
        public double DurationTime
        {
            get
            {
                return Model.DurationTime;
            }
            set
            {
                Model.DurationTime = value;
                RaisePropertyChanged("Width");
            }
        }
        public virtual double EndTime { get { return StartTime + DurationTime; } }
        public void UpdateTimelineProportion()
        {
            RaisePropertyChanged("Left");
            RaisePropertyChanged("Width");
            RecalculateStringLength();
        }

        public delegate void MoveToEventHandler(double value);
        public event MoveToEventHandler MoveTo;
        public void MovePositionWithAnimation(double value)
        {
            MoveTo?.Invoke(value);
        }
        public void ClearMoveToHandler()
        {
            if (MoveTo == null) return;

            Delegate[] clientList = MoveTo.GetInvocationList();
            foreach (var d in clientList)
                MoveTo -= (d as MoveToEventHandler);
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (_isChecked == value)
                    return;

                if (value == true)
                    LayerPage.Self.CheckedEffect = this;

                _isChecked = value;
                RaisePropertyChanged("IsChecked");
            }
        }

        private string effectBlockContent;
        public string EffectBlockContent
        {
            get { return effectBlockContent; }
            set
            {
                if (effectBlockContent != value)
                {
                    effectBlockContent = value;
                    RaisePropertyChanged("EffectBlockContent");
                }
            }

        }
        public string EffectBlockContentTooltip;
        public string IconPath_s { set; get; }
        public string IconPath_n { set; get; }
        private double mPixelSizeOfName { get; set; }

        public EffectLineViewModel(TimelineEffect eff)
        {
            Model = eff;
            EffectBlockContent = GetLanguageNameByStringName(Name);
            EffectBlockContentTooltip = GetLanguageNameByStringName(Name);
            mPixelSizeOfName = GetPixelsOfText(GetLanguageNameByStringName(Name));
            IconPath_n = "ms-appx:///Assets/EffectLine/asus_gc_aurazone_" + Name + "_btn_s.png";
            IconPath_s = "ms-appx:///Assets/EffectLine/asus_gc_aurazone_" + Name + "_btn_n.png";
            RecalculateStringLength();
        }
        public EffectLineViewModel(EffectLineViewModel vm)
        {
            TimelineEffect eff = TimelineEffect.Clone(vm.Model);
            Model = eff;
            EffectBlockContent = GetLanguageNameByStringName(Name);
            EffectBlockContentTooltip = GetLanguageNameByStringName(Name);
            mPixelSizeOfName = GetPixelsOfText(GetLanguageNameByStringName(Name));
            Layer = new LayerModel();
            Left = vm.Left;
            StartTime = vm.StartTime;
            DurationTime = vm.DurationTime;
            IconPath_n = "ms-appx:///Assets/EffectLine/asus_gc_aurazone_" + Name + "_btn_s.png";
            IconPath_s = "ms-appx:///Assets/EffectLine/asus_gc_aurazone_" + Name + "_btn_n.png";
            RecalculateStringLength();
        }
        public EffectLineViewModel(int type)
        {
            TimelineEffect eff = new TimelineEffect(type);
            Model = eff;
            EffectBlockContent = GetLanguageNameByStringName(Name);
            EffectBlockContentTooltip = GetLanguageNameByStringName(Name);
            mPixelSizeOfName = GetPixelsOfText(GetLanguageNameByStringName(Name));
            IconPath_n = "ms-appx:///Assets/EffectLine/asus_gc_aurazone_" + Name + "_btn_s.png";
            IconPath_s = "ms-appx:///Assets/EffectLine/asus_gc_aurazone_" + Name + "_btn_n.png";
            RecalculateStringLength();
        }

        public void RecalculateStringLength()
        {
            double textContainerSize = Width - 70;

            if (textContainerSize < mPixelSizeOfName)
                AddDot(GetLanguageNameByStringName(Name), textContainerSize);
            else
                EffectBlockContent = GetLanguageNameByStringName(Name);

            RaisePropertyChanged("EffectBlockContent");
        }
        private void AddDot(string textContent, double textContainerSize)
        {
            double dotLength = GetPixelsOfText("...");
            double remain = textContainerSize - dotLength;

            if(remain<0)
                EffectBlockContent = "";
            else
            {
                int textCount = textContent.Length - 1;
                string content = textContent.Substring(0, textCount);

                while (remain < GetPixelsOfText(content))
                {
                    textCount--;
                    content = textContent.Substring(0, textCount);
                }

                EffectBlockContent = content + "...";
            }
        }
    }
}
