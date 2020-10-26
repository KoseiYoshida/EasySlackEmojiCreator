using SlackEmojiCreator.Delete;
using SlackEmojiCreator.Upload;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
                "Sans-serif",
                "Impact",
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


        // TODO: Visualじゃなくて、Controlでいい？
        private BitmapSource CaptureControl(Visual targetControl)
        {
            System.Windows.Point leftTopCorner = targetControl.PointToScreen(new System.Windows.Point(0f, 0f));
            var width = (targetControl as FrameworkElement).ActualWidth;
            var height = (targetControl as FrameworkElement).ActualHeight;
            Rect targetRect = new Rect(leftTopCorner.X, leftTopCorner.Y, width, height);
            BitmapSource bitmapSource;
            using (var screenBitmap = new Bitmap((int)targetRect.Width, (int)targetRect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bitmapGraphics = Graphics.FromImage(screenBitmap))
                {
                    bitmapGraphics.CopyFromScreen((int)targetRect.X, (int)targetRect.Y, 0, 0, screenBitmap.Size);
                    bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(screenBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }

            return bitmapSource;
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
            var bitmapSource = CaptureControl(outputTextBoxParent);

            var data = new EmojiData() { Name = name, BitmapSource = bitmapSource };
            emojiDatas.Add(data);
        }

        private void UploadButton_Clicked(object sender, RoutedEventArgs e)
        {
            // TODO: LocalFileUploaderから、MemoryStreamのアップロード機能を分離する。
            var uploader = new EmojiUploader(Properties.Settings.Default.Workspace, Properties.Settings.Default.EmojiAddToken);
            foreach (var emoji in emojiDatas)
            {
                var name = emoji.Name;
                byte[] imageArray = GetByteArray(emoji.BitmapSource);
                Task.Run(() => uploader.UploadEmojiAsync(imageArray, name));
            }
        }

        private byte[] GetByteArray(BitmapSource bitmapSource)
        {
            // TODO: 
            byte[] imageArray;
            var encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                // TODO: 非同期にする
                imageArray = ms.ToArray();
                //await ms.ReadAsync(imageArray, 0, (int)ms.Length);
            }

            return imageArray;
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
