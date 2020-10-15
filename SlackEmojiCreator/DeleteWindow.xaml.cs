using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SlackEmojiCreator
{
    /// <summary>
    /// DeleteWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DeleteWindow : Window
    {
        private EmojiListFetcher emojiListFetcher;
        private ObservableCollection<SelectableEmojiData> emojiDatas = new ObservableCollection<SelectableEmojiData>();
        private Dictionary<string, Uri> emojiInfoDict = new Dictionary<string, Uri>();


        public DeleteWindow()
        {
            InitializeComponent();

            var workspace = Properties.Settings.Default.Workspace;
            var emojiListToken = Properties.Settings.Default.EmojiListToken;
            emojiListFetcher = new EmojiListFetcher(workspace, emojiListToken);

            // 複数スレッドで使用されるコレクションへの参加
            BindingOperations.EnableCollectionSynchronization(emojiDatas, new object());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            selectableEmojiListView.DataContext = emojiDatas;

            Task.Run(() => UpdateList());
        }


        private async Task UpdateList()
        {
            // TODO: プレビュー用の画像とってくる
            emojiInfoDict = await emojiListFetcher.GetUploadedEmojiInfoAsync();

            emojiDatas.Clear();

            foreach ((var name, var _) in emojiInfoDict)
            {
                var data = new SelectableEmojiData() { Name = name, Selected = false };
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
            Task.Run(() => UpdateList());            
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
                await deleter.DeleteAsync(name);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var deleteTargets = emojiDatas.Where(emoji => emoji.Selected == true)
                                                    .Select(emoji => emoji.Name)
                                                    .ToArray();

            Task.Run(() => Delete(deleteTargets)).ContinueWith(_ => UpdateList());
        }

    }
}
