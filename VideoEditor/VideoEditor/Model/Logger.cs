using System;
using System.ComponentModel;
using System.Text;

namespace VideoEditor.Model
{
    public sealed class Logger : INotifyPropertyChanged
    {
        private static Logger mInstance;
        /// <summary>
        /// Első naplóbejegyzés létrehozása.
        /// </summary>
        private Logger()
        {
            var longtimestr = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            logText = new StringBuilder();
            logText.Append($"{longtimestr} - Logger initiated");
        }
        /// <summary>
        /// Singleton Logger naplózó osztály példányosítása.
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new Logger();
                return mInstance;
            }
        }

        private StringBuilder logText;
        private void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
        /// <summary>
        /// Publikus OnPropertyChanged esemény meghívása, mivel stringbuilder mutable nem unmutable.
        /// </summary>
        public void OnPropertyChanged(string propertyName) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Napló tartalma
        /// </summary>
        public StringBuilder LogText
        {
            get { return logText; }
            set { logText = value; }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
