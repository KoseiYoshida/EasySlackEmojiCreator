using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SlackEmojiCreator
{
    public sealed class EmojiData : INotifyPropertyChanged
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

        private string note = "";
        public string Note
        {
            get
            {
                return note;
            }
            set
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    note = value;
                    OnPropertyChanged($"{nameof(Note)}");
                });
            }
        }

        private Brush noteColor = new SolidColorBrush(Colors.Black);
        public Brush NoteColor
        {
            get
            {
                return noteColor;
            }
            set
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    noteColor = value;
                    OnPropertyChanged($"{nameof(NoteColor)}");
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
