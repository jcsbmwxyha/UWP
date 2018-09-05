using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MVVMTest
{
    public class ViewModel : INotifyPropertyChanged
    {
        private RelayCommand _sendEffectCommand;
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand UpdateTitleName
        {
            get
            {
                return (_sendEffectCommand ?? (_sendEffectCommand = new RelayCommand(UpdateTextExecute, CanUpdateTextExecute)));
            }
        }
        public Model MyModel { get; set; }
        
        public bool BoolForCanExecute {
            get { return MyModel.BoolForCanExecute; }
            set
            {
                MyModel.BoolForCanExecute = value;
                _sendEffectCommand.RaiseCanExecuteChanged();
            }
        }

        public string ViewModelText
        {
            get { return MyModel.ModelText; }
            set
            {
                if (MyModel.ModelText != value)
                {
                    MyModel.ModelText = value;
                    RaisePropertyChanged("ViewModelText");
                }
            }
        }

        public ViewModel()
        {
            MyModel = new Model
            {
                ModelText = "",
                BoolForCanExecute = true
            };
        }

        void UpdateTextExecute()
        {
            ViewModelText = "SkyMVVM";
        }
        bool CanUpdateTextExecute()
        {
            return BoolForCanExecute;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
