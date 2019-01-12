using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.Definitions;
using static AuraEditor.Common.EffectHelper;

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

        private EffectInfoModel info;

        #region -- Property --
        public ObservableCollection<ColorPointModel> CurrentColorPoints;
        public LinearGradientBrush CurrentColorForground
        {
            get
            {
                return ColorPointsToForeground(CurrentColorPoints.ToList());
            }
        }

        public List<ColorPointModel> CustomizeColorPoints;
        public LinearGradientBrush CustomizeColorForground
        {
            get
            {
                return ColorPointsToForeground(CustomizeColorPoints.ToList());
            }
        }

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
                    CurrentColorPoints.Clear();

                    if (value == -1)
                    {
                        foreach (var cp in CustomizeColorPoints)
                            CurrentColorPoints.Add(ColorPointModel.Copy(cp));
                    }
                    else
                    {
                        List<ColorPointModel> d_cps;
                        if (value < DefaultColorPointListCollection.Count)
                            d_cps = DefaultColorPointListCollection[value];
                        else
                            d_cps = DefaultColorPointListCollection[5];

                        foreach (var d_cp in d_cps)
                        {
                            var cp = ColorPointModel.Copy(d_cp);
                            cp.ParentPattern = this;
                            CurrentColorPoints.Add(cp);
                        }
                    }

                    info.PatternSelect = value;
                    _selected = value;
                    RaisePropertyChanged("CurrentColorForground");
                    RaisePropertyChanged("CustomizeColorForground");
                }
            }
        }
        #endregion

        public ColorPatternModel(EffectInfoModel info)
        {
            CurrentColorPoints = new ObservableCollection<ColorPointModel>();
            this.info = info;

            foreach (var cp in info.ColorPointList)
                cp.ParentPattern = this;

            CustomizeColorPoints = info.ColorPointList;
            Selected = info.PatternSelect;
        }

        public void OnManipulationDelta()
        {
            RaisePropertyChanged("CurrentColorForground");
        }

        public void OnManipulationCompleted()
        {
            SetColorPointBorders(CurrentColorPoints.ToList());

            CustomizeColorPoints.Clear();
            foreach (var cp in CurrentColorPoints)
                CustomizeColorPoints.Add(cp);

            Selected = -1;
            RaisePropertyChanged("CurrentColorForground");
            RaisePropertyChanged("CustomizeColorForground");
        }
    }
}
