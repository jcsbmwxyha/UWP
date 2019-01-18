using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraEditor.Common
{
    public static class Extensions
    {
        public static void ForEach<T>(this ObservableCollection<T> enumerable, Action<T> action)
        {
            foreach (var cur in enumerable)
            {
                action(cur);
            }
        }
        public static T Find<T>(this ObservableCollection<T> enumerable, Func<T, bool> predicate)
        {
            var result = new List<T>();

            foreach (var item in enumerable)
            {
                if (!predicate(item))
                {
                    return item;
                }
            }

            return default(T);
        }
        public static ObservableCollection<T> FindAll<T>(this ObservableCollection<T> enumerable, Func<T, bool> predicate)
        {
            var result = new ObservableCollection<T>();

            foreach (var item in enumerable)
            {
                if (predicate(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }
        public static ObservableCollection<T> RemoveAll<T>(this ObservableCollection<T> enumerable, Func<T, bool> predicate)
        {
            var result = new ObservableCollection<T>();

            foreach (var item in enumerable)
            {
                if (!predicate(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }
}
