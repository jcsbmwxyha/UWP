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

        public string EngName
        {
            get
            {
                return GetEffEngNameByIdx(Model.Type);
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
                RecalculateCurText();
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
            RecalculateCurText();
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

        private string _curText;
        public string CurText
        {
            get { return _curText; }
            set
            {
                if (_curText != value)
                {
                    _curText = value;
                    RaisePropertyChanged("CurText");
                }
            }

        }
        public string FullText;
        public string IconPath_s { set; get; }
        public string IconPath_n { set; get; }
        private double mPixelSizeOfName { get; set; }

        public EffectLineViewModel(TimelineEffect eff)
        {
            Model = eff;
            FullText = GetLanguageNameByIdx(Model.Type);
            mPixelSizeOfName = GetPixelsOfText(FullText);
            RecalculateCurText();
            IconPath_n = "ms-appx:///Assets/EffectLine/asus_gc_aurazone_" + EngName + "_btn_s.png";
            IconPath_s = "ms-appx:///Assets/EffectLine/asus_gc_aurazone_" + EngName + "_btn_n.png";
        }
        public EffectLineViewModel(EffectLineViewModel vm)
        {
            Model = TimelineEffect.Clone(vm.Model);
            FullText = GetLanguageNameByIdx(Model.Type);
            mPixelSizeOfName = GetPixelsOfText(FullText);
            RecalculateCurText();
            Layer = new LayerModel();
            Left = vm.Left;
            StartTime = vm.StartTime;
            DurationTime = vm.DurationTime;
            IconPath_n = "ms-appx:///Assets/EffectLine/asus_gc_aurazone_" + EngName + "_btn_s.png";
            IconPath_s = "ms-appx:///Assets/EffectLine/asus_gc_aurazone_" + EngName + "_btn_n.png";
        }
        public EffectLineViewModel(int type)
        {
            Model = new TimelineEffect(type);
            FullText = GetLanguageNameByIdx(type);
            mPixelSizeOfName = GetPixelsOfText(FullText);
            RecalculateCurText();
            IconPath_n = "ms-appx:///Assets/EffectLine/asus_gc_aurazone_" + EngName + "_btn_s.png";
            IconPath_s = "ms-appx:///Assets/EffectLine/asus_gc_aurazone_" + EngName + "_btn_n.png";
        }

        public void RecalculateCurText()
        {
            double curTextContainerSize = Width - 70;

            if (curTextContainerSize < mPixelSizeOfName)
                AddDot(FullText, curTextContainerSize);
            else
                CurText = FullText;
        }
        private void AddDot(string fullText, double curTextContainerSize)
        {
            double dotLength = GetPixelsOfText("...");
            double remain = curTextContainerSize - dotLength;

            if (remain < 0)
                CurText = "";
            else
            {
                int textCount = fullText.Length - 1;
                string content = fullText.Substring(0, textCount);

                while (remain < GetPixelsOfText(content))
                {
                    textCount--;
                    content = fullText.Substring(0, textCount);
                }

                CurText = content + "...";
            }
        }
    }
}
