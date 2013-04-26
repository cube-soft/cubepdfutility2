/* ------------------------------------------------------------------------- */
///
/// ItemHeightConverter.cs
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
using System.Windows.Data;
using System.Globalization;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// ItemHeightConverter
    /// 
    /// <summary>
    /// サムネイルの高さの値を調節するためのコンバータクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ItemHeightConverter : IValueConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        /// 
        /// <summary>
        /// ListViewModel から渡された値をもとに、サムネイルの高さを調節
        /// します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var height = (int)value;
                return height + _TextHeight;
            }
            catch (Exception /* err */) { return default(int); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertBack
        /// 
        /// <summary>
        /// 調節した高さを元の値に戻します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var height = (int)value;
                return height - _TextHeight;
            }
            catch (Exception /* err */) { return default(int); }
        }

        #region Static variables
        private static readonly int _TextHeight = 15;
        #endregion
    }
}
