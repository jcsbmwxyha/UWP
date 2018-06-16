using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;

namespace Ch0904
{
    public class MyColors : INotifyPropertyChanged
    {
        private SolidColorBrush _Brush1;

        // 宣告PropertyChanged事件
        public event PropertyChangedEventHandler PropertyChanged;

        // 建立綁定的資料來源(為一筆刷顏色)
        public SolidColorBrush Brush1
        {
            get { return _Brush1; }
            set
            {
                _Brush1 = value;
                // 當來源屬性(即筆刷顏色)有改變時呼叫NotifyPropertyChanged
                NotifyPropertyChanged("Brush1");
            }
        }

        // NotifyPropertyChanged 方法將導致PropertyChanged事件，
        // 指示來源屬性有改變了
        async public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
                string res = "文字顏色已經改變為藍色 !";
                var messDialog = new MessageDialog(res);
                await messDialog.ShowAsync();
            }
        }
    }
}
