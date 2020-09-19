using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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


        // TODO: 別クラス、非同期できるようにする
        private void Upload_TextEmoji()
        {

            var text = emojiTextBox.Text;


            // TODO: 
            Byte[] imageArray;
            var encoder = new PngBitmapEncoder();

            var bitmapSource = textImage.Source.Clone() as BitmapSource;
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                // TODO: 非同期にする
                imageArray = ms.ToArray();
                //await ms.ReadAsync(imageArray, 0, (int)ms.Length);
            }

            var uploader = new LocalFileEmojiUploader(Properties.Settings.Default.Workspace, Properties.Settings.Default.EmojiAddToken);
            Task.Run(() => uploader.UploadEmojiAsync(imageArray, text));

            // TODO: 失敗判定をする
            outputTextBox.Text = $"Add {text} succeeded.";
            return;
        }

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

        private void SetTextAsImage(string text, System.Windows.Controls.Image targetImage)
        {
            Bitmap canvas = new Bitmap((int)targetImage.Width, (int)targetImage.Height);
            Graphics g = Graphics.FromImage(canvas);
            Font font = new Font("MS UI Gothic", 80);
            Rectangle rect = new Rectangle(0, 0, (int)targetImage.Width, (int)targetImage.Height);
            g.FillRectangle(System.Drawing.Brushes.White, rect);
            g.DrawString(text, font, System.Drawing.Brushes.Blue, rect);
            font.Dispose();
            g.Dispose();
            IntPtr hbitmap = canvas.GetHbitmap();
            targetImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

        }

        private void EmojText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var textbox = (sender as TextBox);
            SetTextAsImage(textbox.Text, textImage);
        }
    }
}
