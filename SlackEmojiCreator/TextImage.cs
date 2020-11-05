using SlackEmojiCreator.Utility;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SlackEmojiCreator
{
    /// <summary>
    /// Text to be able to capture as image.
    /// </summary>
    public sealed class TextImage
    {
        private readonly TextBlock targetTextBlock;
        private readonly FrameworkElement captureElement;
        public FontFamily FontFamily = new FontFamily("impact");
        public Color Color = Color.FromRgb(0, 0, 0);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="targetTextBlock">Text</param>
        /// <param name="captureElement">Capture frame</param>
        /// <exception cref="ArgumentNullException">Throw if <paramref name="targetTextBlock"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Throw if <paramref name="captureElement"/> is null.</exception>
        public TextImage(TextBlock targetTextBlock, FrameworkElement captureElement)
        {
            this.targetTextBlock = targetTextBlock ?? throw new ArgumentNullException(nameof(targetTextBlock));
            this.captureElement = captureElement ?? throw new ArgumentNullException(nameof(captureElement));
        }

        /// <summary>
        /// Get current text.
        /// </summary>
        /// <returns>Text</returns>
        public string GetText()
        {
            return targetTextBlock.Text;
        }

        /// <summary>
        /// Update text.
        /// </summary>
        /// <param name="newText">New text</param>
        public void UpdateText(string newText)
        {
            targetTextBlock.Text = newText;
            targetTextBlock.FontFamily = FontFamily;
            targetTextBlock.Foreground = new SolidColorBrush(Color);
        }

        /// <summary>
        /// Capture text as image.
        /// </summary>
        /// <returns>BitmapSource of captured image</returns>
        public BitmapSource CaptureAsImage()
        {
              return ImageUtility.CaptureFrameworkElement(captureElement);
        }
    }
}
