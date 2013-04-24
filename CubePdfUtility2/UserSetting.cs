/* ------------------------------------------------------------------------- */
///
/// UserSetting.cs
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
using Microsoft.Win32;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// UserSetting
    /// 
    /// <summary>
    /// CubePdfUtility2 のユーザ設定を保持するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class UserSetting
    {
        #region Initialization and Termination

        /* ----------------------------------------------------------------- */
        ///
        /// UserSetting (constructor)
        /// 
        /// <summary>
        /// 既定の値でオブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public UserSetting()
        {
            try
            {
                var subkey = Registry.LocalMachine.OpenSubKey(_RegRoot, false);
                if (subkey == null) return;
                _path = subkey.GetValue(_RegPath, string.Empty) as string;
                _version = subkey.GetValue(_RegVersion, "1.0.0") as string;
            }
            catch (Exception /* err */) { }
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Position
        /// 
        /// <summary>
        /// メイン画面の位置（左上の座標）を取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Point Position
        {
            get { return _position; }
            set { _position = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Size
        /// 
        /// <summary>
        /// メイン画面のサイズを取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Size Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public bool IsMaximized
        {
            get { return _maximize; }
            set { _maximize = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ItemWidth
        /// 
        /// <summary>
        /// サムネイルの幅を取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int ItemWidth
        {
            get { return _itemwidth; }
            set { _itemwidth = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ItemWidth
        /// 
        /// <summary>
        /// サムネイルの表示方法を取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public CubePdf.Wpf.ListViewItemVisibility ItemVisibility
        {
            get { return _visibility; }
            set { _visibility = value; }
        }

        #endregion

        #region Public methods

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        /// 
        /// <summary>
        /// レジストリから設定を読み込みます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Load()
        {
            try
            {
                var root = Registry.CurrentUser.OpenSubKey(_RegRoot, false);
                if (root == null) return;

                var setting = new CubePdf.Settings.Document();
                setting.Read(root);

                var x = setting.Root.Find(_RegX);
                var y = setting.Root.Find(_RegY);
                if (x != null && y != null) _position = new Point(x.GetValue((int)_position.X), y.GetValue((int)_position.Y));

                var width  = setting.Root.Find(_RegWidth);
                var height = setting.Root.Find(_RegHeight);
                if (width != null && height != null) _size = new Size(width.GetValue((int)_size.Width), height.GetValue((int)_size.Height));

                var maximize = setting.Root.Find(_RegMaximize);
                if (maximize != null) _maximize = maximize.GetValue(_maximize);

                var item = setting.Root.Find(_RegItemWidth);
                if (item != null) _itemwidth = item.GetValue(_itemwidth);

                var visibility = setting.Root.Find(_RegVisibility);
                if (visibility != null) _visibility = (CubePdf.Wpf.ListViewItemVisibility)visibility.Value;
            }
            catch (Exception /* err */) { }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        /// 
        /// <summary>
        /// レジストリへ設定を保存します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Save()
        {
            try
            {
                var root = Registry.CurrentUser.CreateSubKey(_RegRoot);
                if (root == null) return;

                var setting = new CubePdf.Settings.Document();
                setting.Root.Add(new CubePdf.Settings.Node(_RegX, (int)_position.X));
                setting.Root.Add(new CubePdf.Settings.Node(_RegY, (int)_position.Y));
                setting.Root.Add(new CubePdf.Settings.Node(_RegWidth, (int)_size.Width));
                setting.Root.Add(new CubePdf.Settings.Node(_RegHeight, (int)_size.Height));
                setting.Root.Add(new CubePdf.Settings.Node(_RegMaximize, _maximize));
                setting.Root.Add(new CubePdf.Settings.Node(_RegItemWidth, _itemwidth));
                setting.Root.Add(new CubePdf.Settings.Node(_RegVisibility, (int)_visibility));

                setting.Write(root);
            }
            catch (Exception /* err */) { }
        }

        #endregion

        #region Variables
        private string _path = string.Empty;
        private string _version = "1.0.0";
        private Point _position = new Point(20, 20);
        private Size _size = new Size(800, 600);
        private bool _maximize = false;
        private CubePdf.Wpf.ListViewItemVisibility _visibility = CubePdf.Wpf.ListViewItemVisibility.Normal;
        private int _itemwidth = 150;
        #endregion

        #region Constant variables
        private static readonly string _RegRoot       = @"Software\CubeSoft\CubePDF Utility2";
        private static readonly string _RegPath       = "InstallDirectory";
        private static readonly string _RegVersion    = "Version";
        private static readonly string _RegX          = "X";
        private static readonly string _RegY          = "Y";
        private static readonly string _RegWidth      = "Width";
        private static readonly string _RegHeight     = "Height";
        private static readonly string _RegMaximize   = "IsMaximized";
        private static readonly string _RegVisibility = "ItemVisibility";
        private static readonly string _RegItemWidth  = "ItemWidth";
        #endregion
    }
}
