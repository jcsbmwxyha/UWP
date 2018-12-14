﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using Windows.UI.Xaml.Media.Imaging;
using static FrameCoordinatesGenerator.Common.Math2;

namespace FrameCoordinatesGenerator.Models
{
    public enum DeviceStatus
    {
        OnStage = 0,
        Temp,
    }

    public class DeviceModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region -- Property--
        private double _pixelLeft;
        public double PixelLeft
        {
            get
            {
                return _pixelLeft;
            }
            set
            {
                _pixelLeft = value;
                RaisePropertyChanged("PixelLeft");
            }
        }

        private double _pixelTop;
        public double PixelTop
        {
            get
            {
                return _pixelTop;
            }
            set
            {
                _pixelTop = value;
                RaisePropertyChanged("PixelTop");
            }
        }

        private double _pixelWidth;
        public double PixelWidth
        {
            get
            {
                return _pixelWidth;
            }
            set
            {
                _pixelWidth = value;
                RaisePropertyChanged("PixelWidth");
            }
        }

        private double _pixelHeight;
        public double PixelHeight
        {
            get
            {
                return _pixelHeight;
            }
            set
            {
                _pixelHeight = value;
                RaisePropertyChanged("PixelHeight");
            }
        }

        public double PixelRight { get { return PixelLeft + PixelWidth; } }
        public double PixelBottom { get { return PixelTop + PixelHeight; } }

        private bool _operationenabled;
        public bool OperationEnabled
        {
            get
            {
                return _operationenabled;
            }
            set
            {
                _operationenabled = value;
                RaisePropertyChanged("OperationEnabled");
            }
        }

        private string _visualstate;
        public string VisualState
        {
            get
            {
                return _visualstate;
            }
            set
            {
                _visualstate = value;
                RaisePropertyChanged("VisualState");
            }
        }

        public BitmapImage Image;

        private ObservableCollection<ZoneModel> _zones;
        public ObservableCollection<ZoneModel> Zones
        {
            get
            {
                return _zones;
            }
            set
            {
                _zones = value;
                RaisePropertyChanged("Zones");
            }
        }

        private ObservableCollection<SpecialZoneModel> _specialzones;
        public ObservableCollection<SpecialZoneModel> SpecialZones
        {
            get
            {
                return _specialzones;
            }
            set
            {
                _specialzones = value;
                RaisePropertyChanged("Zones");
            }
        }

        public List<ZoneModel> AllZones
        {
            get
            {
                List<ZoneModel> list = Zones.ToList();
                list.AddRange(SpecialZones.ToList());

                return list;
            }
        }

        public string Name { get; set; }
        public int Type { get; set; }
        public DeviceStatus Status { get; set; }
        #endregion

        public DeviceModel()
        {
        }
    }
}