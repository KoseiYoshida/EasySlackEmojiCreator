using SlackEmojiCreator.Delete;
using SlackEmojiCreator.Upload;
using SlackEmojiCreator.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SlackEmojiCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<EmojiData> emojiDatas = new ObservableCollection<EmojiData>();

        private readonly bool HasInitialized;

        private readonly string[] fontFamilies;
        private readonly SolidBrush[] brushes;

        private TextImage textImage;

        public MainWindow()
        {
            InitializeComponent();

            // 複数スレッドで使用されるコレクションの設定
            BindingOperations.EnableCollectionSynchronization(emojiDatas, new object());

            textImage = new TextImage(outputText, outputTextFrame);

            // 参考:https://w3g.jp/sample/css/font-family
            fontFamilies = new string[3]
            {
                "Impact",
                "Sans-serif",
                "Monospace",
            };

            foreach(var f in fontFamilies)
            {
                fontFamilyComboBox.Items.Add(f);
            }

            brushes = new SolidBrush[4]
            {
                new SolidBrush(System.Drawing.Color.Black),
                new SolidBrush(System.Drawing.Color.Red),
                new SolidBrush(System.Drawing.Color.Green),
                new SolidBrush(System.Drawing.Color.Blue),
             };

            foreach (var brush in brushes)
            {
                colorComboBox.Items.Add(brush.Color);
            }

            fontFamilyComboBox.SelectedIndex = 0;
            colorComboBox.SelectedIndex = 0;

            HasInitialized = true;

            textImage.UpdateText(inputText.Text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            emojiListView.DataContext = emojiDatas;
        }

        private void AddButton_Clicked(object sender, RoutedEventArgs e)
        {
            var baseName = textImage.GetText().Replace(" ", "").Replace("　", "");
            baseName = baseName.Replace("\n", "").Replace("\r", "");
            if (baseName.Length < 1)
            {
                // TODO: 失敗情報をユーザーに表示する
                Console.WriteLine($"Failed to add image.");
                return;
            }

            var name = baseName;
            name = name.ToLowerInvariant();

            if(!textImage.TryCaptureAsImage(out var bitmapSource))
            {
                // TODO: 失敗情報をユーザーに表示する
                Console.WriteLine($"Failed to add image. Cannot capture.");
                return;
            }

            var data = new EmojiData() { Name = name, BitmapSource = bitmapSource};
            emojiDatas.Add(data);
        }

        // TODO: アップロード終了したものから消す、失敗したものからその理由を表示する（非同期化）
        private void UploadButton_Clicked(object sender, RoutedEventArgs e)
        {
            var uploader = new EmojiUploader(Properties.Settings.Default.Workspace, Properties.Settings.Default.EmojiAddToken);

            List<EmojiData> succeededEmojis = new List<EmojiData>(emojiDatas.Count);
            foreach (var emoji in emojiDatas)
            {
                var name = emoji.Name;
                byte[] imageArray = ImageUtility.GetByteArray(emoji.BitmapSource);
                var uploadResult = Task.Run(() => uploader.UploadEmojiAsync(imageArray, name)).Result;
                if (uploadResult.IsSucceeded)
                {
                    emoji.Note = $"Upload Suceeded";
                    succeededEmojis.Add(emoji);
                }
                else
                {
                    emoji.Note = $"Failed : {uploadResult.FailureReason}";
                    emoji.NoteColor = new SolidColorBrush(Colors.Red);
                }
            }
           
            foreach (var emoji in succeededEmojis)
            {
                emojiDatas.Remove(emoji);
            }
        }


        private void ClearButton_Clicked(object sender, RoutedEventArgs e)
        {
            emojiDatas.Clear();
        }


        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!HasInitialized)
            {
                return;
            }

            textImage.FontFamily = new System.Windows.Media.FontFamily(fontFamilyComboBox.SelectedItem as string);
            textImage.UpdateText(inputText.Text);
        }

        private void colorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!HasInitialized)
            {
                return;
            }
            
            var c = brushes[colorComboBox.SelectedIndex].Color;
            var color = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
            textImage.Color = color;
            textImage.UpdateText(inputText.Text);
        }

        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AccountWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void inputText_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.InvokeAsync(() => {
                Task.Delay(0);
                ((TextBox)sender).SelectAll();
            });
        }

        private void inputText_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var textbox = (sender as TextBox);
            textImage.UpdateText(textbox.Text);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new DeleteWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void emojiDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var context = (sender as Button).DataContext;
            if(!(context is EmojiData))
            {
                throw new ArgumentException($"This context is not {nameof(EmojiData)}. Context type : {context.GetType()}");
            }

            emojiDatas.Remove(context as EmojiData);
        }


    }
}
