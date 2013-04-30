/* ------------------------------------------------------------------------- */
///
/// PasswordWindow.xaml.cs
///
/// Copyright (c) 2013 CubeSoft, Inc. All rights reserved.
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with this program.  If not, see < http://www.gnu.org/licenses/ >.
///
/* ------------------------------------------------------------------------- */
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// PasswordWindow
    /// 
    /// <summary>
    /// PasswordWindow.xaml の相互作用ロジック
    /// </summary>
    /* --------------------------------------------------------------------- */
    public partial class PasswordWindow : Window
    {
        /* ----------------------------------------------------------------- */
        ///
        /// PasswordWindow (constructor)
        ///
        /// <summary>
        /// 既定の値でオブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public PasswordWindow()
        {
            InitializeComponent();
            LoadIcon();
            ReplaceFont();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// PasswordWindow (constructor)
        ///
        /// <summary>
        /// 引数に指定されたファイルパスを利用して、オブジェクトを初期化
        /// します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public PasswordWindow(string path)
            : this()
        {
            MessageLabel.Text = String.Format(Properties.Resources.PasswordPrompt, System.IO.Path.GetFileName(path));
        }

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Password
        ///
        /// <summary>
        /// パスワードを取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
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
            var fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (var ff in fonts.Families)
            {
                if (ff.Name.Contains("Meiryo"))
                {
                    PasswordTextBox.FontFamily = new System.Windows.Media.FontFamily(ff.Name);
                    break;
                }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LoadIcon
        ///
        /// <summary>
        /// システムのアイコンを読み込みます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void LoadIcon()
        {
            var icon = System.Drawing.SystemIcons.Warning;
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            IconImage.Source = source;
        }

        #endregion

        #region Variables
        private string _password = string.Empty;
        #endregion
    }
}
