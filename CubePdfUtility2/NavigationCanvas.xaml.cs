/* ------------------------------------------------------------------------- */
///
/// NavigationCanvas.xaml.cs
/// 
/// Copyright (c) 2013 CubeSoft, Inc. All rights reserved.
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License as published
/// by the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Affero General Public License for more details.
///
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.
///
/* ------------------------------------------------------------------------- */
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// NavigationCanvas
    /// 
    /// <summary>
    /// Interaction logic for NavigationCanvas.xaml
    /// </summary>
    /* --------------------------------------------------------------------- */
    public partial class NavigationCanvas : Canvas
    {
        #region Initializations and Terminations

        /* ----------------------------------------------------------------- */
        ///
        /// NavigationCanvas (constructor)
        /// 
        /// <summary>
        /// 既定の値でオブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public NavigationCanvas()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// AddFiles
        ///
        /// <summary>
        /// ナビゲーション用画面に表示するファイルを追加します。
        /// </summary>
        /// 
        /// <remarks>
        /// ナビゲーション画面は左右に 2 つの表示領域を持っています。
        /// AddFiles メソッドでは、左詰めでそれぞれの領域の最大表示可能数
        /// までの項目を追加します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public void AddFiles(IList<string> paths)
        {
            var index = 0;
            var limit = Math.Min(paths.Count, index + 6 - PrimaryFields.Children.Count);
            for (; index < limit; ++index)
            {
                PrimaryFields.Children.Add(CreatePdfMenuItem(paths[index]));
            }
            if (index >= paths.Count) return;

            limit = Math.Min(paths.Count, index + 6 - SecondaryFields.Children.Count);
            for (; index < limit; ++index)
            {
                SecondaryFields.Children.Add(CreatePdfMenuItem(paths[index]));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Clear
        /// 
        /// <summary>
        /// ナビゲーション画面に表示されている各メニュー項目をクリアします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Clear()
        {
            foreach (var obj in PrimaryFields.Children)
            {
                var control = obj as MenuItem;
                if (control == null) continue;
                if (MenuItemClick != null) control.Click -= MenuItemClick;
            }
            PrimaryFields.Children.Clear();

            foreach (var obj in SecondaryFields.Children)
            {
                var control = obj as MenuItem;
                if (control == null) continue;
                if (MenuItemClick != null) control.Click -= MenuItemClick;
            }
            SecondaryFields.Children.Clear();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// MenuItemClick
        /// 
        /// <summary>
        /// ナビゲーション画面に表示されているメニュー項目がクリックされた
        /// 時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event RoutedEventHandler MenuItemClick;

        /* ----------------------------------------------------------------- */
        ///
        /// CreatePdfMenuItem
        /// 
        /// <summary>
        /// PDF ファイルのナビゲーション用のメニュー項目を作成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private MenuItem CreatePdfMenuItem(string path)
        {
            var dest = new MenuItem();
            dest.Header = System.IO.Path.GetFileName(path);
            dest.Tag = path;
            dest.Margin = new Thickness(0, 2, 0, 3);
            dest.Icon = new System.Windows.Controls.Image {
                Source = new BitmapImage(new Uri("Images/PdfFile.png", UriKind.Relative))
            };
            if (MenuItemClick != null) dest.Click += MenuItemClick;
            return dest;
        }

        #endregion
    }
}
