using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SlackEmojiCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string[] currentInputFilesPath;

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {

            if (currentInputFilesPath == null || currentInputFilesPath.Length == 0)
            {
                outputTextBox.Text = "No input.";
                return;
            }

            var defaultSetting = Properties.Settings.Default;
            var workspaceName = defaultSetting.Workspace;
            var emojiListToken  = defaultSetting.EmojiListToken;
            var emojiAddToken = defaultSetting.EmojiAddToken;

            // TODO: Workspace, Tokenのチェックをする

            var emojiFetcher = new EmojiListFetcher(workspaceName, emojiListToken);
            var task = Task.Run(() =>
            {
                return emojiFetcher.GetEmojiNamesAsync();
            });

            string[] emojiNames = task.Result;
            Array.Sort(emojiNames);

            // Upload
            List<string> addedFiles = new List<string>(emojiNames.Length);
            List<string> erroredFiles = new List<string>(emojiNames.Length);
            var emojiUploader = new LocalFileEmojiUploader(workspaceName, emojiAddToken);
            foreach (var filePath in currentInputFilesPath)
            {
                // TODO: すでにある絵文字の名前と被っている場合、スキップする。

                try
                {
                    Task.Run(() => emojiUploader.UploadEmojiAsync(filePath));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{filePath} not added. {ex}");
                    erroredFiles.Add(filePath);
                    continue;
                }

                addedFiles.Add(filePath);
            }

            // change output text
            var sb = new StringBuilder();
            sb.Append("Added:\n");
            foreach (var file in addedFiles)
            {
                sb.Append(file).Append("\n");
            }

            if (erroredFiles.Count > 0)
            {
                sb.Append("Not added:\n");
                foreach (var file in erroredFiles)
                {
                    sb.Append(file);
                }
            }

            outputTextBox.Text = sb.ToString();
        }


        private void DropFiles(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if(files == null)
            {
                return;
            }

            currentInputFilesPath = files;

            var sb = new StringBuilder();
            foreach(string file in files)
            {
                sb.Append(file).Append("\n");
            }

            var textBox = sender as TextBox;
            textBox.Text = sb.ToString();
        }

        private void Input_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }

        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AccountWindow();
            window.Owner = this;
            window.ShowDialog();
            
        }
    }
}
