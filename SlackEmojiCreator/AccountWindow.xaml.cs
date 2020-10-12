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
            //emojiAddToken.Text = defaultSetting.EmojiAddToken;
            //emojiRemoveToken.Text = defaultSetting.EmojiRemoveToken;
        }

        private void UpdateResultMessage(string msg)
        {
            resultTextBlock.Text = msg;
        }

        // TODO: Tokenが有効かどうかのチェックいれる。
        private void SaveSetting()
        {

            if (string.IsNullOrEmpty(workspaceName.Text))
            {
                UpdateResultMessage("Please enter workspace name.");
                return;
            }

            var defaultSetting = Properties.Settings.Default;
            defaultSetting.Workspace = workspaceName.Text;
            defaultSetting.EmojiListToken = emojiListToken.Text;
            //defaultSetting.EmojiAddToken = emojiAddToken.Text;
            //defaultSetting.EmojiRemoveToken = emojiRemoveToken.Text;

            // EmojiListTokenでAddもRemoveもできる。
            defaultSetting.EmojiAddToken = emojiListToken.Text;
            defaultSetting.EmojiRemoveToken = emojiListToken.Text;

            try
            {
                defaultSetting.Save();
            }
            catch(Exception e)
            {
                UpdateResultMessage($"Failed to save. {e}");
                return;
            }

            UpdateResultMessage($"Succeeded to save.");
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
