using AuraEditor.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraEditor.Models
{
    public class PlayerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public PlayerModel()
        {
            Position = 0;
            IsPlaying = false;
        }

        private string _timerText;
        public string TimerText
        {
            get
            {
                double seconds = Position / LayerPage.PixelsPerSecond;
                return TimeSpan.FromSeconds(seconds).ToString("mm\\:ss\\.ff");
            }
            set
            {
                _timerText = value;
                RaisePropertyChanged("TimerText");
            }
        }

        private double _position;
        public double Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                RaisePropertyChanged("Position");
                RaisePropertyChanged("TimerText");
            }
        }

        private bool _isplaying;
        public bool IsPlaying
        {
            get
            {
                return _isplaying;
            }
            set
            {
                _isplaying = value;
                RaisePropertyChanged("IsPlaying");
            }
        }

        private double _maxEditWidth;
        public double MaxEditWidth
        {
            get
            {
                return _maxEditWidth;
            }
            set
            {
                _maxEditWidth = value;
                RaisePropertyChanged("MaxEditWidth");
            }
        }
    }
}
