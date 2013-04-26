using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CubePdfUtility
{
    /// <summary>
    /// PasswordWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PasswordWindow : Window
    {
        public PasswordWindow()
        {
            InitializeComponent();
            LoadIcon();
        }

        public PasswordWindow(string path)
            : this()
        {
            MessageLabel.Text = String.Format(Properties.Resources.PasswordPrompt, System.IO.Path.GetFileName(path));

            ReplaceFont();
        }

        #region Properties

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        #endregion

        #region Commands

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        /// 
        /// <summary>
        /// OK ボタンをに該当するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Save

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrEmpty(PasswordTextBox.Password);
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Password = PasswordTextBox.Password;
            DialogResult = true;
            Close();
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Close
        /// 
        /// <summary>
        /// キャンセルボタンをに該当するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Close

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion

        #endregion

        # region Other Methods
        /* ----------------------------------------------------------------- */
        ///
        /// ReplaceFont
        ///
        /// <summary>
        /// コンストラクタ実行時に、画面のフォントを差し替えます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void ReplaceFont()
        {
            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            System.Drawing.FontFamily[] ffArray = fonts.Families;

            foreach (System.Drawing.FontFamily ff in ffArray)
            {
                if (ff.Name.Contains("Meiryo"))
                {
                    PasswordTextBox.FontFamily = new System.Windows.Media.FontFamily(ff.Name);
                    break;
                }
            }
        }
        #endregion

        private void LoadIcon()
        {
            var icon = System.Drawing.SystemIcons.Warning;
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            IconImage.Source = source;
        }

        #region Variables
        private string _password = string.Empty;
        #endregion
    }
}
