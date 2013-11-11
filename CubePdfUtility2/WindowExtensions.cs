/* ------------------------------------------------------------------------- */
///
/// WindowExtensions.cs
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
using System.Runtime.InteropServices;
using System.Windows;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// WindowExtensions
    /// 
    /// <summary>
    /// Windowクラスの拡張メソッド（最小化・最大化ボタン無効）用クラス
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class WindowExtensions
    {
        /* ----------------------------------------------------------------- */
        ///
        /// HideMinimizeButton
        /// 
        /// <summary>
        /// 最小化ボタンを非表示にします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static void HideMinimizeButton(this Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MINIMIZEBOX));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// HideMinimizeButton
        /// 
        /// <summary>
        /// 最大化ボタンを非表示にします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static void HideMaximizeButton(this Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MAXIMIZEBOX));
        }

        #region Win32 APIs
        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        extern private static int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        extern private static int SetWindowLong(IntPtr hwnd, int index, int value);
        #endregion
    }
}
