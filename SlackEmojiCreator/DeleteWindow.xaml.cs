using SlackAPI.Fetch;
using SlackAPI.Delete;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Math = Utility.Math;

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
            try
            {
                emojiInfoDict = await emojiListFetcher.GetUploadedEmojiInfoAsync();
            }
            catch(SlackAPI.Exception.SlackAPIException ex)
            {
                Console.WriteLine($"Failed to get emoji list. Message:{ex.Message}");
                MessageBox.Show($"Failed to get emoji list.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            emojiDatas.Clear();

            foreach ((var name, var imageUri) in emojiInfoDict)
            {                    
                var data = new SelectableEmojiData() { Name = name, Selected = false, ThumnailUri = imageUri };
                emojiDatas.Add(data);
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

            var deleter = new EmojiDeleter(workspace, token);
            List<string> failed = new List<string>((int)Math.NextPow2(targetNames.Length));
            foreach(var name in targetNames)
            {                
                try
                {
                    // TODO: 一つ一つawaitする意味ある？Parallel? WhenAll?
                    await deleter.DeleteAsync(name);
                }
                catch(System.Net.Http.HttpRequestException ex)
                {
                    Console.WriteLine($"Failed to send request. Emoji : {name}, {ex.Message}");
                    failed.Add(name);
                }
                catch (SlackAPI.Exception.SlackAPIException ex)
                {
                    Console.WriteLine($"Some kind of error occurred by using Slack API.", ex.Message);
                    failed.Add(name);
                }
            }

            // 削除に失敗したものをユーザーに表示
            if(failed.Count > 0)
            {
                var stringBuilder = new StringBuilder();
                foreach(var name in failed)
                {
                    stringBuilder.Append($"\n");
                    stringBuilder.Append(name);
                }
                MessageBox.Show($"Failed to upload below. {stringBuilder.ToString()}", "", MessageBoxButton.OK, MessageBoxImage.Error);
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
