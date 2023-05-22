using System;
using System.Globalization;
using Xamarin.Forms;

namespace VideoEditor.View.Converter
{
    public sealed class DragColorConverter : IValueConverter
    {
        public static DragColorConverter dragColorConverter = new DragColorConverter();

        /// <summary>
        /// A drag and drop mozgásnál megvalósított átalakítás bool? értéket szín értékké.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? isBeingDragged = (bool?)value;
            Color result = (isBeingDragged ?? false) ? Color.LightGray : Color.Azure;
            return result;
        }

        /// <summary>
        /// Színt konvertál bool értékké.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color isBeingDragged = (Color)value;
            bool? result = (isBeingDragged == Color.LightGray);
            return result;
        }
    }
}
