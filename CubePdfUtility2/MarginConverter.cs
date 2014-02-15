/* ------------------------------------------------------------------------- */
///
/// MarginConverter.cs
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
    public class MarginConverter : IMultiValueConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        /// 
        /// <summary>
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                _backs = new object[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    _backs[i] = values[i];
                }
                var width = (double)values[0];
                //var margin = (System.Windows.Thickness)values[1];
                //var padding = (System.Windows.Thickness)values[2];
                //return width - margin.Top * 2 - padding.Top * 2;
                return width - 10 * 2 - 3 * 2;
            }
            catch (Exception /* err */) { return new System.Windows.Thickness(1.0); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertBack
        /// 
        /// <summary>
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            try
            {
                object[] vals = new object[_backs.Length];
                for (int i = 0; i < _backs.Length; i++)
                {
                    vals[i] = _backs[i];
                }
                return vals;
            }
            catch (Exception /* err */) { return new object[] { }; }
        }

        #region Static variables
        private static object[] _backs;
        #endregion
    }
}
