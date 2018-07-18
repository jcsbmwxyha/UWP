using System;
using System.Collections.Generic;
using Windows.UI.Popups;

namespace VocabularyTest.Common
{
    class CommonHelper
    {
        public const string yahooURL = @"https://tw.dictionary.search.yahoo.com/search?p=";
        public const string googleURL = @"https://translate.google.com.tw/#en/zh-TW/";

        public static async void ShowMessage(string res)
        {
            var messDialog = new MessageDialog(res);
            await messDialog.ShowAsync();
        }

        public static string ParseString(string content, string startString, string endString, bool includeStartEnd)
        {
            string result = "";
            int startIndex, endIndex;

            startIndex = content.IndexOf(startString);
            if (startIndex == -1)
                return "";
            else if (includeStartEnd)
                content = content.Substring(startIndex);
            else
                content = content.Substring(startIndex + startString.Length);

            endIndex = content.IndexOf(endString);

            if (endIndex == -1)
                return "";
            else if (includeStartEnd)
                result = content.Substring(0, endIndex + endString.Length);
            else
                result = content.Substring(0, endIndex);

            return result;
        }

        public static void SwapListItem<T>(IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
        public static void SwapValue<T>(ref T a, ref T b)
        {
            T temp;
            temp = a;
            a = b;
            b = temp;
        }
    }
}
