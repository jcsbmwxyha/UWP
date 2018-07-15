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
    public class MyViewModel : INotifyPropertyChanged
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

        public MyModel Model { get; set; }
        public string TheText
        {
            get {
                return Model.MyText;
            }
            set
            {
                if (Model.MyText != value)
                {
                    Model.MyText = value;
                    RaisePropertyChanged("TheText");
                    _sendEffectCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public MyViewModel()
        {
            Model = new MyModel { MyText = "" };
        }
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        void UpdateTextExecute()
        {
            TheText = "SkyMVVM";
        }

        //定義是否可以更新Title
        bool CanUpdateTextExecute()
        {
            return true;
        }
    }
}
