/* ------------------------------------------------------------------------- */
///
/// KiloByteConverter.cs
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
using System.Windows.Data;
using System.Globalization;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// KiloByteConverter
    /// 
    /// <summary>
    /// KB 単位に変換するためのコンバータクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class KiloByteConverter : IValueConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        /// 
        /// <summary>
        /// 引数に指定されたバイトサイズを KB 単位に変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var kbytes = (long)value / 1024.0;
                return (long)Math.Ceiling(kbytes);
            }
            catch (Exception /* err */) { return default(long); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertBack
        /// 
        /// <summary>
        /// KB 単位に変換された値を元に戻します。切り捨てられている関係で、
        /// 完全に元の値に戻るとは限りません。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try { return (long)value * 1024; }
            catch (Exception /* err */) { return default(int); }
        }
    }
}
