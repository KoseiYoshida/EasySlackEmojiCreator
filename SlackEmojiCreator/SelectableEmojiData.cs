﻿using System;
using System.ComponentModel;
using System.Windows;

namespace SlackEmojiCreator.Delete
{
    /// <summary>
    /// Selectable emoji data.
    /// </summary>
    public sealed class SelectableEmojiData : INotifyPropertyChanged
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

        private Uri thumnailUri = null;
        public Uri ThumnailUri
        {
            get
            {
                return thumnailUri;
            }
            set
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    thumnailUri = value;
                    OnPropertyChanged($"{nameof(ThumnailUri)}");
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
