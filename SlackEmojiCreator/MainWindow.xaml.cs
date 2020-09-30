using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
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

            textColors = new SolidBrush[4]
            {
                new SolidBrush(System.Drawing.Color.Black),
                new SolidBrush(System.Drawing.Color.Red),
                new SolidBrush(System.Drawing.Color.Green),
                new SolidBrush(System.Drawing.Color.Blue),
            };

            outputTextBoxes = new TextBox[4]
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

        //private void Window_ContentRendered(object sender, EventArgs e)
        //{
        //    // Windowが表示されるまで待機したいので、タイマーで1秒後に処理を実行
        //    var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        //    timer.Start();
        //    timer.Tick += (s, args) =>
        //    {
        //        // タイマーの停止
        //        timer.Stop();

        //        // キャプチャ開始
        //        CaptureControl(block);
        //    };
        //}

        private void CaptureControl(Visual targetControl)
        {
            System.Windows.Point leftTopCorner = targetControl.PointToScreen(new System.Windows.Point(0f, 0f));
            var width = (targetControl as FrameworkElement).ActualWidth;
            var height = (targetControl as FrameworkElement).ActualHeight;
            Rect targetRect = new Rect(leftTopCorner.X, leftTopCorner.Y, width, height);
            BitmapSource bitmapSource;
            using(var screenBitmap = new Bitmap((int)targetRect.Width, (int)targetRect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using(var bitmapGraphics = Graphics.FromImage(screenBitmap))
                {
                    bitmapGraphics.CopyFromScreen((int)targetRect.X, (int)targetRect.Y, 0, 0, screenBitmap.Size);
                    bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(screenBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }

            using(var fStream = new FileStream("result.png", FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(fStream);
                Console.WriteLine($"saved");
            }
        }

        private readonly TextBox[] outputTextBoxes;

        private readonly SolidBrush[] textColors;

        private readonly string[] fontFamilies;

        //private void SetTextAsImage(string text, System.Windows.Controls.Image targetImage, Brush brush, string fontFamily)
        //{
        //    // TODO: ソースがなく、widthやheightがautoのときの取得方法を調べる
        //    //var width = (int)targetImage.ActualWidth
        //    //var height = (int)targetImage.ActualHeight;
        //    var width = 400;
        //    var height = 400;

        //    string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
        //    int maxCount = lines.Length;
        //    for (int i = 0; i < lines.Length; i++)
        //    {
        //        var len = lines[i].Length;
        //        if (len > maxCount)
        //            maxCount = len;
        //    }

        //    Bitmap canvas = new Bitmap(width, height);
        //    Graphics g = Graphics.FromImage(canvas);
        //    float size = width / maxCount;
        //    Font font = new Font(fontFamily, size);
        //    // TODO: StringFormatの工夫で対処できるかも？
        //    var sf = new StringFormat(StringFormatFlags.NoClip);
        //    g.DrawString(text, font, brush, 0, 0, sf);

        //    font.Dispose();
        //    g.Dispose();

        //    IntPtr hbitmap = canvas.GetHbitmap();
        //    targetImage.Source = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

        //}

        //private void UpdateTextImages(string text)
        //{
        //    for (int i = 0; i < images.Length; i++)
        //    {
        //        SetTextAsImage(text, images[i], textColors[i], fontFamilies[fontFamilyComboBox.SelectedIndex]);
        //    }
        //}

        // TODO: keydownだとタイミングが合わない
        
        
        private void UpdateTexts()
        {

        }

        private void EmojiText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var textbox = (sender as TextBox);
            // TODO: 行数に合わせて、空白文字で列を調整する？
            //UpdateTextImages(textbox.Text);
            
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
            // UpdateTextImages(inputText.Text);
        }



        //private void DropFiles(object sender, DragEventArgs e)
        //{
        //    var files = e.Data.GetData(DataFormats.FileDrop) as string[];
        //    if(files == null)
        //    {
        //        return;
        //    }

        //    currentInputFilesPath = files;

        //    var sb = new StringBuilder();
        //    foreach(string file in files)
        //    {
        //        sb.Append(file).Append("\n");
        //    }

        //    var textBox = sender as TextBox;
        //    textBox.Text = sb.ToString();
        //}

        //private void Input_PreviewDragOver(object sender, DragEventArgs e)
        //{
        //    e.Effects = DragDropEffects.Copy;
        //    e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        //}

        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AccountWindow();
            window.Owner = this;
            window.ShowDialog();
        }


    }
}
