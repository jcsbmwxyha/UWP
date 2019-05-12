using AuraEditor.Common;
using AuraEditor.Models;
using AuraEditor.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.ControlHelper;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.UserControls.ColorPatternView;

namespace AuraEditor.ViewModels
{
    class ColorPatternViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public EffectInfoModel mInfoModel;

        static private ColorPatternViewModel _self;
        static private ColorPatternViewModel _triggerself;
        static public ColorPatternViewModel Self
        {
            get
            {
                if (GetCurrentContentDialog() == null)
                    return _self;
                else
                    return _triggerself;
            }
        }

        #region -- Property --

        public ObservableCollection<ColorPointModel> CurrentColorPoints;
        public LinearGradientBrush CurrentColorForground
        {
            get
            {
                return ColorPointsToForeground(CurrentColorPoints.ToList());
            }
        }

        internal List<ColorPointLightData> GetCustomizedLightData()
        {
            List<ColorPointLightData> res = new List<ColorPointLightData>();
            foreach (var cp in CustomizeColorPoints)
            {
                res.Add(new ColorPointLightData(cp.Color, cp.PixelX));
            }
            return res;
        }

        public List<ColorPointModel> CustomizeColorPoints
        {
            get { return mInfoModel.CustomizedPattern; }
            set { mInfoModel.CustomizedPattern = value; }
        }
        public LinearGradientBrush CustomizeColorForground
        {
            get
            {
                return ColorPointsToForeground(CustomizeColorPoints.ToList());
            }
        }

        public int Select
        {
            get
            {
                return mInfoModel.PatternSelect;
            }
            set
            {
                if (mInfoModel.PatternSelect != value)
                {
                    mInfoModel.PatternSelect = value;
                    RefreshCurrentCPs();
                }
            }
        }
        #endregion

        public ColorPatternViewModel(EffectInfoModel info)
        {
            CurrentColorPoints = new ObservableCollection<ColorPointModel>();
            mInfoModel = info;
            RefreshCurrentCPs();

            if (!IsTriggerEffect(info.Type))
                _self = this;
            else
                _triggerself = this;
        }

        public void OnManipulationDelta()
        {
            RaisePropertyChanged("CurrentColorForground");
        }
        public void OnCustomizeChanged()
        {
            var oldCPs = GetCustomizedLightData();

            SetColorPointBorders(CurrentColorPoints.ToList());

            CustomizeColorPoints.Clear();
            foreach (var cp in CurrentColorPoints)
                CustomizeColorPoints.Add(ColorPointModel.Copy(cp));

            var newCPs = GetCustomizedLightData();

            ReUndoManager.Store(new ColorPatternModifyCommand(mInfoModel, oldCPs, newCPs, Select, -1));

            Select = -1;
            RaisePropertyChanged("CurrentColorForground");
            RaisePropertyChanged("CustomizeColorForground");
        }

        public void RefreshCurrentCPs()
        {
            CurrentColorPoints.Clear();
            List<ColorPointModel> d_cps;

            if (Select == -1)
            {
                SetColorPointBorders(CustomizeColorPoints);
                d_cps = CustomizeColorPoints;
            }
            else if (Select < DefaultColorPointListCollection.Count)
                d_cps = DefaultColorPointListCollection[Select];
            else
                d_cps = DefaultColorPointListCollection[DefaultColorPointListCollection.Count - 1];

            foreach (var d_cp in d_cps)
            {
                var cp = ColorPointModel.Copy(d_cp);
                CurrentColorPoints.Add(cp);
            }

            CurrentColorPoints[0].IsChecked = true;
            RaisePropertyChanged("CurrentColorForground");
            RaisePropertyChanged("CustomizeColorForground");
        }
    }
}
