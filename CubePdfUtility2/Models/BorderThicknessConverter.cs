/* ------------------------------------------------------------------------- */
///
/// BorderThicknessConverter.cs
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
using System.Collections.Generic;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// BorderThicknessConverter
    /// 
    /// <summary>
    /// サムネイルの枠線の太さを調節するためのコンバータクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    class BorderThicknessConverter : IMultiValueConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        /// 
        /// <summary>
        /// 項目の幅/高さ、および表示サイズとして指定されている幅/高さ
        /// の値を元に枠線の太さを決定します。
        /// </summary>
        /// 
        /// <remarks>
        /// 引数 values には 4 つの値を指定する必要がある。順に項目の幅、
        /// 表示サイズとして指定されている幅、項目の高さ、表示サイズとして
        /// 指定されている高さとなる。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var width      = (double)values[0];
                var widthview  = (double)values[1];
                var height     = (double)values[2];
                var heightview = (double)values[3];
                var horizontal = width / widthview;
                var vertical   = height / heightview;
                return new System.Windows.Thickness(horizontal > vertical ? horizontal : vertical);
            }
            catch (Exception /* err */) { return new System.Windows.Thickness(1.0); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertBack
        /// 
        /// <summary>
        /// 未実装です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
