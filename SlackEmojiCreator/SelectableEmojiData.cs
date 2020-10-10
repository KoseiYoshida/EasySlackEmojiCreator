using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SlackEmojiCreator
{
    class SelectableEmojiData : INotifyPropertyChanged
    {
        public string Name { get; set; } = "";

        // The name "IsSelected" is used for checkbox property.
        private bool selected = false;
        public bool Selected { 
            get 
            {
                return selected;
            }
            set
            {
                selected = value;
                OnPropertyChanged($"{nameof(Selected)}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
