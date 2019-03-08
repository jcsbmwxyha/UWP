using AuraEditor.Common;
using AuraEditor.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;
using static AuraEditor.UserControls.ColorPatternView;

namespace AuraEditor.Models
{
    public class ColorPatternModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private EffectInfoModel Info;
        static public ColorPatternModel Self;

        #region -- Property --
        public ObservableCollection<ColorPointModel> CurrentColorPoints;
        public LinearGradientBrush CurrentColorForground
        {
            get
            {
                return ColorPointsToForeground(CurrentColorPoints.ToList());
            }
        }

        internal List<ColorPointLightData> GetCustomizedCpData()
        {
            List<ColorPointLightData> res = new List<ColorPointLightData>();
            foreach (var cp in CustomizeColorPoints)
            {
                res.Add(new ColorPointLightData(cp.Color, cp.PixelX));
            }
            return res;
        }

        public List<ColorPointModel> CustomizeColorPoints;
        public LinearGradientBrush CustomizeColorForground
        {
            get
            {
                return ColorPointsToForeground(CustomizeColorPoints.ToList());
            }
        }

        public Canvas PatternCPsCanvas;

        private int _selected = -2;
        public int Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    Info.PatternSelect = value;
                    _selected = value;
                    RefreshCPs();
                }
            }
        }
        #endregion

        public ColorPatternModel(EffectInfoModel info, Canvas canvas)
        {
            CurrentColorPoints = new ObservableCollection<ColorPointModel>();
            CurrentColorPoints.CollectionChanged += CurrentCPsChanged;
            PatternCPsCanvas = canvas;
            this.Info = info;
            
            CustomizeColorPoints = info.CustomizedPattern;
            Selected = info.PatternSelect;
            Self = this;
        }

        private void CurrentCPsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ColorPointModel model;
            ColorPointView view;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    PatternCPsCanvas.Children.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    model = e.OldItems[0] as ColorPointModel;
                    PatternCPsCanvas.Children.Remove(model.View);
                    break;
                case NotifyCollectionChangedAction.Add:
                    model = e.NewItems[0] as ColorPointModel;
                    view = new ColorPointView();
                    view.DataContext = model;
                    model.View = view;
                    PatternCPsCanvas.Children.Add(model.View);
                    break;
            }
        }

        public void OnManipulationDelta()
        {
            RaisePropertyChanged("CurrentColorForground");
        }

        public void OnCustomizeChanged()
        {
            var oldCPs = GetCustomizedCpData();

            SetColorPointBorders(CurrentColorPoints.ToList());

            CustomizeColorPoints.Clear();
            foreach (var cp in CurrentColorPoints)
                CustomizeColorPoints.Add(ColorPointModel.Copy(cp));
            
            var newCPs = GetCustomizedCpData();

            ReUndoManager.Store(new ColorPatternModifyCommand(oldCPs, newCPs, Selected, -1));

            Selected = -1;
            RaisePropertyChanged("CurrentColorForground");
            RaisePropertyChanged("CustomizeColorForground");
        }

        public void RefreshCPs()
        {
            CurrentColorPoints.Clear();

            if (Selected == -1)
            {
                foreach (var cp in CustomizeColorPoints)
                    CurrentColorPoints.Add(ColorPointModel.Copy(cp));
            }
            else
            {
                List<ColorPointModel> d_cps;
                if (Selected < DefaultColorPointListCollection.Count)
                    d_cps = DefaultColorPointListCollection[Selected];
                else
                    d_cps = DefaultColorPointListCollection[5];

                foreach (var d_cp in d_cps)
                {
                    var cp = ColorPointModel.Copy(d_cp);
                    CurrentColorPoints.Add(cp);
                }
            }
            
            RaisePropertyChanged("CurrentColorForground");
            RaisePropertyChanged("CustomizeColorForground");
        }
    }
}
