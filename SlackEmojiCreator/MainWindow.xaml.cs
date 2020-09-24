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

            images = new System.Windows.Controls.Image[4]
            {
                image1,
                image2,
                image3,
                image4
            };

            textColors = new SolidBrush[4]
            {
                new SolidBrush(System.Drawing.Color.Black),
                new SolidBrush(System.Drawing.Color.Red),
                new SolidBrush(System.Drawing.Color.Green),
                new SolidBrush(System.Drawing.Color.Blue),
            };

            // 参考:https://w3g.jp/sample/css/font-family
            fontFamilies = new string[3]
            {
                "Sans-serif",
                "Impact",
                "メイリオ",
            };

            foreach(var f in fontFamilies)
            {
                fontFamilyComboBox.Items.Add(f);
            }

            fontFamilyComboBox.SelectedIndex = 0;
            
            //InstalledFontCollection installedFontCollection = new InstalledFontCollection();

            //// Get the array of FontFamily objects.
            //var fam = installedFontCollection.Families;

            //// The loop below creates a large string that is a comma-separated
            //// list of all font family names.

            //foreach(var f in fam)
            //{
            //    Console.WriteLine(f);
            //}
        }

        private System.Windows.Controls.Image[] images;

        private readonly SolidBrush[] textColors;

        private readonly string[] fontFamilies;

        private void SetTextAsImage(string text, System.Windows.Controls.Image targetImage, Brush brush, string fontFamily)
        {
            // TODO: ソースがなく、widthやheightがautoのときの取得方法を調べる
            //var width = (int)targetImage.ActualWidth
            //var height = (int)targetImage.ActualHeight;
            var width = 100;
            var height = 100;

            Bitmap canvas = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(canvas);
            Font font = new Font(fontFamily, 40);
            Rectangle rect = new Rectangle(0, 0, width, height);
            g.FillRectangle(System.Drawing.Brushes.White, rect);
            g.DrawString(text, font, brush, rect);
            font.Dispose();
            g.Dispose();
            IntPtr hbitmap = canvas.GetHbitmap();
            targetImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

        }

        private void UpdateTextImages(string text)
        {
            for (int i = 0; i < images.Length; i++)
            {
                SetTextAsImage(text, images[i], textColors[i], fontFamilies[fontFamilyComboBox.SelectedIndex]);
            }
        }

        // TODO: keydownだとタイミングが合わない
        private void EmojiText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var textbox = (sender as TextBox);
            UpdateTextImages(textbox.Text);
        }


        private Dictionary<string, System.Windows.Controls.Image> candidates = new Dictionary<string, System.Windows.Controls.Image>();

        private void AddButton_Clicked(object sender, RoutedEventArgs e)
        {
            var baseName = inputText;
            if(baseName.Text.Length < 1)
            {
                return;
            }

            candidates.Clear();
            candidatesText.Text = "";

            for(int i = 0; i < images.Length; i++)
            {
                var name = baseName.Text + "-" + textColors[i].Color.Name.ToString();
                candidates.Add(name, images[i]);
                candidatesText.Text += name;
                candidatesText.Text += "\n";
            }
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
            UpdateTextImages(inputText.Text);
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
