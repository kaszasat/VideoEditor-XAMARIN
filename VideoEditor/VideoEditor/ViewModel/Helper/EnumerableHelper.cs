using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.Internals;

namespace VideoEditor.ViewModel.Helper
{
    public static class EnumerableHelpers
    {
        /// <summary>
        /// Generikus IEnumerable<T>-t átalakít ObservableCollection<T>-vé.
        /// </summary>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return null;

            var result = new ObservableCollection<T>();
            source.ForEach(result.Add);
            return result;
        }
    }
}
