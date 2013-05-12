/* ------------------------------------------------------------------------- */
///
/// ImageView.cs
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
using System.Windows.Controls;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// ImageView
    /// 
    /// <summary>
    /// ListView の表示スタイルについて定義されたクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ImageView : ViewBase
    {
        /* ----------------------------------------------------------------- */
        ///
        /// ItemWidth
        /// 
        /// <summary>
        /// 表示される各項目の幅を取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ItemHeight
        /// 
        /// <summary>
        /// 表示される各項目の幅を取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }
        
        /* ----------------------------------------------------------------- */
        /// DefaultStyleKey
        /* ----------------------------------------------------------------- */
        protected override object DefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "ImageView"); }
        }

        /* ----------------------------------------------------------------- */
        /// ItemContainerDefaultStyleKey
        /* ----------------------------------------------------------------- */
        protected override object ItemContainerDefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "ImageViewItem"); }
        }

        #region Dependency Properties
        public static readonly DependencyProperty ItemWidthProperty = WrapPanel.ItemWidthProperty.AddOwner(typeof(ImageView));
        public static readonly DependencyProperty ItemHeightProperty = WrapPanel.ItemHeightProperty.AddOwner(typeof(ImageView));
        #endregion
    }
}
