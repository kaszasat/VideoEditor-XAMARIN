using System;
using System.Globalization;
using Xamarin.Forms;

namespace VideoEditor.View.Converter
{
    public sealed class SecondsDoubleToString : IValueConverter
    {
        public static SecondsDoubleToString secondsDoubleToString = new SecondsDoubleToString();

        /// <summary>
        /// Átalakít double értéket és az alsó string értékkel mely a következő behelyettesítéssel
        /// ÓraÓra:PercPerc:MásodpercMásodperc.Törtérték formában megjeleníthető.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? seconds = (double)value;
            if (seconds == null)
            {
                return null;
            }
            string returnString = "";
            int hours = (int)seconds / (60 * 60);
            if (hours < 10)
                returnString += $"0{hours}";
            else
                returnString += $"{hours}";

            int minutes = (int)seconds / (60) - hours * 60;
            if (minutes < 10)
                returnString += $":0{minutes}";
            else
                returnString += $":{minutes}";

            int remainingSeconds = (int)seconds - hours * 60 * 60 - minutes * 60;
            if (remainingSeconds < 10)
                returnString += $":0{remainingSeconds}";
            else
                returnString += $":{remainingSeconds}";

            return $"{returnString}.{Math.Round((seconds.Value - (hours * 60 * 60 + minutes * 60 + remainingSeconds)) * 10000)}";
        }

        /// <summary>
        /// Visszaváltoztat string értéket double értékké.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string seconds = (string)value;
            if (seconds == null)
            {
                return null;
            }
            string[] values = seconds.Split(':');
            int hours = Int32.Parse(values[0]);
            int minutes = Int32.Parse(values[1]);
            double remainingSeconds = double.Parse(values[2].Replace(".", ","), CultureInfo.InvariantCulture);

            return (double)hours * 60.0 * 60.0 + (double)minutes * 60.0 + remainingSeconds/10000;
        }
    }
}
