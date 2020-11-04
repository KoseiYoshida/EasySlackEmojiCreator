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

        public MainWindow()
        {
            InitializeComponent();

            // 複数スレッドで使用されるコレクションへの参加
            BindingOperations.EnableCollectionSynchronization(emojiDatas, new object());
            

            outputTextBlock = outputText1;
            outputTextBoxParent = outputTextParent1;

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

            UpdateOutputTexts(inputText.Text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            emojiListView.DataContext = emojiDatas;
        }

        private readonly TextBlock outputTextBlock;

        // Captureのために、決まった大きさのBoxを定義している
        private readonly TextBlock outputTextBoxParent;        

        private readonly string[] fontFamilies;
        private readonly SolidBrush[] brushes;

        private void UpdateOutputTexts(string sourceText)
        {
            var textBox = outputTextBlock;
            textBox.Text = sourceText;
            var fontfamilyString = fontFamilies[fontFamilyComboBox.SelectedIndex];
            textBox.FontFamily = new System.Windows.Media.FontFamily(fontfamilyString);
            var c = brushes[colorComboBox.SelectedIndex].Color;
            var color = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
            textBox.Foreground = new SolidColorBrush(color);
        }

        private void AddButton_Clicked(object sender, RoutedEventArgs e)
        {
            var baseName = inputText.Text.Replace(" ", "").Replace("　", "");
            baseName = baseName.Replace("\n", "").Replace("\r", "");
            if (baseName.Length < 1)
            {
                return;
            }

            var name = baseName;
            name = name.ToLowerInvariant();
            var bitmapSource = ImageUtility.CaptureFrameworkElement(outputTextBoxParent);

            var data = new EmojiData() { Name = name, BitmapSource = bitmapSource};
            emojiDatas.Add(data);
        }

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

            UpdateOutputTexts(inputText.Text);
        }

        private void colorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!HasInitialized)
            {
                return;
            }

            UpdateOutputTexts(inputText.Text);
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
            UpdateOutputTexts(textbox.Text);
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
