using System;
using System.Windows;

namespace SlackEmojiCreator
{
    /// <summary>
    /// AccountWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AccountWindow : Window
    {
        public AccountWindow()
        {
            InitializeComponent();
        }

        private void LoadSetting()
        {
            var defaultSetting = Properties.Settings.Default;
            workspaceName.Text = defaultSetting.Workspace;
            emojiListToken.Text = defaultSetting.EmojiListToken;
            emojiAddToken.Text = defaultSetting.EmojiAddToken;
        }

        private void SaveSetting()
        {
            var defaultSetting = Properties.Settings.Default;
            defaultSetting.Workspace = workspaceName.Text;
            defaultSetting.EmojiListToken = emojiListToken.Text;
            defaultSetting.EmojiAddToken = emojiAddToken.Text;
            defaultSetting.Save();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSetting();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            LoadSetting();
        }
    }
}
