using System;
using Windows.UI.Popups;

namespace VocabularyTest.Common
{
    class CommonHelper
    {
        public static async void ShowMessage(string res)
        {
            var messDialog = new MessageDialog(res);
            await messDialog.ShowAsync();
        }
    }
}
