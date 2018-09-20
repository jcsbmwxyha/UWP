using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AuraEditor.Common
{
    public class ObservableRecentList : ObservableCollection<string>
    {
        public int MaxCount;

        public ObservableRecentList()
        {
            MaxCount = 5;
        }
        public ObservableRecentList(List<string> list) : base(list)
        {
            MaxCount = 5;
        }
        public void InsertHead(string item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Items[i] == item)
                {
                    RemoveAt(i);
                    break;
                }
            }

            Insert(0, item);

            if (Count > MaxCount)
            {
                RemoveAt(MaxCount);
            }
        }
        public void InsertTail(string item)
        {
            if (Count == MaxCount)
            {
                return;
            }

            Add(item);
        }
    }
}