using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SlackEmojiCreator
{
    public sealed class EmojiData
    {
        private string name = "";
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    name = value;
                    OnPropertyChanged($"{nameof(Name)}");
                });
            }
        }

        private BitmapSource bitmapSource = null;
        public BitmapSource BitmapSource
        {
            get
            {
                return bitmapSource;
            }
            set
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    bitmapSource = value;
                    OnPropertyChanged($"{nameof(BitmapSource)}");
                });
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
