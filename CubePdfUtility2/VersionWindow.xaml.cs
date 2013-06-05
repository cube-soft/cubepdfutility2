/* ------------------------------------------------------------------------- */
///
/// VersionWindow.xaml.cs
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
using System.Windows;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// VersionWindow
    /// 
    /// <summary>
    /// VersionWindow.xaml の相互作用ロジック
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public partial class VersionWindow : Window
    {
        #region Initialization and Termination

        /* ----------------------------------------------------------------- */
        ///
        /// VersionWindow (constructor)
        /// 
        /// <summary>
        /// 指定されたバージョンの値を使用して、オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public VersionWindow(string version)
        {
            InitializeComponent();
            SetVersion(version);
            SourceInitialized += (sender, e) => {
                if (Top < 0 || Top > SystemParameters.WorkArea.Bottom - Height) Top = 0;
                if (Left < 0 || Left > SystemParameters.WorkArea.Right - Width) Left = 0;
            };
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VersionWindow (constructor)
        /// 
        /// <summary>
        /// 既定の値でオブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public VersionWindow()
            : this(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()) { }

        #endregion

        #region Event handlers

        /* ----------------------------------------------------------------- */
        ///
        /// Button_Click
        /// 
        /// <summary>
        /// OK ボタンがクリックされた時に実行されるイベントハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Button_Click(object sender, EventArgs e)
        {
            Close();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// HyperLink_Click
        /// 
        /// <summary>
        /// ハイパーリンクテキストが実行された時に実行されるイベントハンドラ
        /// です。該当 URL へ移動します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void HyperLink_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.cube-soft.jp/cubepdfutility/");
            }
            catch { }
        }

        #endregion

        #region Other methods

        /* ----------------------------------------------------------------- */
        ///
        /// SetVersion
        /// 
        /// <summary>
        /// バージョン情報を設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetVersion(string version)
        {
            VersionLabel.Content = String.Format("Version {0} ({1})", version, ((IntPtr.Size == 4) ? "x86" : "x64"));
        }

        #endregion
    }
}
