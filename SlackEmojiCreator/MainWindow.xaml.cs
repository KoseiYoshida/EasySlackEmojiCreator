using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Drawing.Brush;
using Brushes = System.Drawing.Brushes;

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

            textColors = new SolidColorBrush[4]
            {
                new SolidColorBrush(Colors.Black),
                new SolidColorBrush(Colors.Red),
                new SolidColorBrush(Colors.Green),
                new SolidColorBrush(Colors.Blue),
            };

            outputTextBoxes = new TextBlock[4]
            {
                outputText1,
                outputText2,
                outputText3,
                outputText4,
            };

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

            fontFamilyComboBox.SelectedIndex = 0;
            
        }


        private readonly TextBlock[] outputTextBoxes;

        private readonly SolidColorBrush[] textColors;

        private readonly string[] fontFamilies;


        private void UpdateOutputTexts(string sourceText)
        {
            for(int i = 0; i < outputTextBoxes.Length; i++)
            {
                var textBox = outputTextBoxes[i];
                textBox.Text = sourceText;
                var fontfamilyString = fontFamilies[fontFamilyComboBox.SelectedIndex];
                textBox.FontFamily = new System.Windows.Media.FontFamily(fontfamilyString);
                textBox.Foreground = textColors[i];
            }
        }


        private void CaptureControl(Visual targetControl)
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

            using (var fStream = new FileStream("result.png", FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(fStream);
            }
        }

        // TODO: keydownだとタイミングが合わない
        private void EmojiText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var textbox = (sender as TextBox);
            UpdateOutputTexts(textbox.Text);
        }


        private Dictionary<string, System.Windows.Controls.Image> candidates = new Dictionary<string, System.Windows.Controls.Image>();

        private void AddButton_Clicked(object sender, RoutedEventArgs e)
        {
            var baseName = inputText;
            if (baseName.Text.Length < 1)
            {
                return;
            }

            candidates.Clear();
            candidatesText.Text = "";

            //for (int i = 0; i < images.Length; i++)
            //{
            //    var name = baseName.Text + "-" + textColors[i].Color.Name.ToString();
            //    candidates.Add(name, images[i]);
            //    candidatesText.Text += name;
            //    candidatesText.Text += "\n";
            //}
        }

        private void UploadButton_Clicked(object sender, RoutedEventArgs e)
        {
            // TODO: LocalFileUploaderから、MemoryStreamのアップロード機能を分離する。
            var uploader = new LocalFileEmojiUploader(Properties.Settings.Default.Workspace, Properties.Settings.Default.EmojiAddToken);
            foreach((string name, System.Windows.Controls.Image image) in candidates)
            {
                byte[] imageArray = GetByteArray(image);
                Task.Run(() => uploader.UploadEmojiAsync(imageArray, name));
            }
        }

        private byte[] GetByteArray(System.Windows.Controls.Image image)
        {
            // TODO: 
            byte[] imageArray;
            var encoder = new PngBitmapEncoder();

            var bitmapSource = image.Source.Clone() as BitmapSource;
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
            candidates.Clear();
            candidatesText.Text = "files";
        }


        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateOutputTexts(inputText.Text);
        }


        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AccountWindow();
            window.Owner = this;
            window.ShowDialog();
        }


    }
}
