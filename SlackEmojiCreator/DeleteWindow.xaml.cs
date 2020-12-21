﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SlackEmojiCreator.Delete
{
    /// <summary>
    /// DeleteWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DeleteWindow : Window
    {
        private readonly EmojiListFetcher emojiListFetcher;
        private readonly ObservableCollection<SelectableEmojiData> emojiDatas = new ObservableCollection<SelectableEmojiData>();
        private Dictionary<string, Uri> emojiInfoDict = new Dictionary<string, Uri>();


        public DeleteWindow()
        {
            InitializeComponent();

            // 複数スレッドで使用されるコレクションへの参加
            BindingOperations.EnableCollectionSynchronization(emojiDatas, new object());

            var workspace = Properties.Settings.Default.Workspace;
            var emojiListToken = Properties.Settings.Default.EmojiListToken;
            emojiListFetcher = new EmojiListFetcher(workspace, emojiListToken);            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            selectableEmojiListView.DataContext = emojiDatas;

            _ = UpdateList();
        }

        private async Task UpdateList()
        {
            emojiInfoDict = await emojiListFetcher.GetUploadedEmojiInfoAsync();

            emojiDatas.Clear();

            foreach ((var name, var imageUri) in emojiInfoDict)
            {                    
                var data = new SelectableEmojiData() { Name = name, Selected = false, ThumnailUri = imageUri };
                try
                {
                    emojiDatas.Add(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex}");
                }
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Updateが完了するまではUIを触れなくする
            _ = UpdateList();
        }

        private async Task Delete(string[] targetNames)
        {
            var workspace = Properties.Settings.Default.Workspace;
            var token = Properties.Settings.Default.EmojiRemoveToken;

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine($"No token");
                throw new ArgumentNullException($"");
            }

            var deleter = new EmojiDeleter(workspace, token);
            foreach(var name in targetNames)
            {
                // TODO: 一つ一つawaitする意味ある？
                await deleter.DeleteAsync(name);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var deleteTargets = emojiDatas.Where(emoji => emoji.Selected == true)
                                                    .Select(emoji => emoji.Name)
                                                    .ToArray();


            await Delete(deleteTargets).ContinueWith(_ => UpdateList());
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(var emoji in emojiDatas)
            {
                emoji.Selected = true;
            }
        }
    }
}
