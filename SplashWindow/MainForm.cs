/* ------------------------------------------------------------------------- */
///
/// MainForm.cs
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
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// MainForm
    /// 
    /// <summary>
    /// スプラッシュ画面用のフォームクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public partial class MainForm : Form
    {
        #region Initialization and Termination

        /* ----------------------------------------------------------------- */
        ///
        /// MainForm
        /// 
        /// <summary>
        /// 既定の値でオブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MainForm()
        {
            InitializeComponent();

            Width  = Properties.Resources.Splash.Width;
            Height = Properties.Resources.Splash.Height;
            InfoLabel.ForeColor = Color.FromArgb(0x333333);

            var edition = (IntPtr.Size == 4) ? "x86" : "x64";
            var dotnet = Environment.Version.ToString();
            VersionLabel.Text = string.Format(Properties.Resources.Version, _launcher.Version, edition, dotnet);
            VersionLabel.ForeColor = Color.FromArgb(0x333333);
            
            _modules = new List<string>() {
                "System",
                "System.Core",
                "System.Data",
                "System.Drawing",
                "System.Windows.Forms",
                "System.Windows.Interactivity",
                "System.Web",
                "System.Xml",
                "Interop.IWshRuntimeLibrary",
                "WindowBase",
                "PresentationCore",
                "PresentationFramework",
                "Microsoft.Windows.Shell",
                "RibbonControlsLibrary",
                "PDFLibNet",
                "itextsharp",
                "CubePdf.Data",
                "CubePdf.Misc",
                "CubePdf.Settings",
                "CubePdf.Drawing",
                "CubePdf.Editing",
                "CubePdf.Wpf",
            };

        }

        /* ----------------------------------------------------------------- */
        ///
        /// MainForm
        /// 
        /// <summary>
        /// 引数に指定された引数を用いて、オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MainForm(string[] args)
            : this()
        {
            foreach (var s in args) _launcher.Arguments.Add(s);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// CreateParams
        /// 
        /// <summary>
        /// コントロールの作成時に必要な情報をカプセル化します。
        /// MainForm クラスでは、フォームに陰影を付与するためのパラメータを
        /// ベースの値に追加しています。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000;
                return cp;
            }
        }

        #endregion

        #region Event handlers

        /* ----------------------------------------------------------------- */
        ///
        /// OnShown
        /// 
        /// <summary>
        /// フォームが表示された時に実行されるイベントハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_launcher.Run())
            {
                _limit = DateTime.Now.AddSeconds(60);
                RefreshTimer.Start();
            }
            else Close();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TimerTicked
        /// 
        /// <summary>
        /// タイマークラスによって定期的に実行されるイベントハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void TimerTicked(object sender, EventArgs e)
        {
            if (DateTime.Now > _limit)
            {
                RefreshTimer.Stop();
                Close();
            }
            else
            {
                var message = _current < _modules.Count ?
                    string.Format(Properties.Resources.LoadingMessage, _modules[_current++]) :
                    Properties.Resources.DefaultMessage;
                InfoLabel.Text = message;
            }
        }

        #endregion

        #region Variagles
        private Launcher _launcher = new Launcher();
        private List<string> _modules = new List<string>();
        private int _current = 0;
        private DateTime _limit;
        #endregion
    }
}
