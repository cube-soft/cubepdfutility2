/* ------------------------------------------------------------------------- */
///
/// ByteFormatConverter.cs
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
    /// ByteFormatConverter
    /// 
    /// <summary>
    /// バイト数を読みやすい表記に整形するためのコンバータクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ByteFormatConverter : IValueConverter
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
                var n = (long)value;
                var units = new string[] { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
                var bytes = (double)n;
                var index = 0;

                while (bytes > 1000.0)
                {
                    bytes /= 1024.0;
                    ++index;
                    if (index >= units.Length - 1) break;
                }

                return string.Format("{0:G3} {1}", bytes, units[index]);

            }
            catch (Exception /* err */) { return string.Empty; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertBack
        /// 
        /// <summary>
        /// 不可逆変換なので未実装です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default(long);
        }
    }
}
