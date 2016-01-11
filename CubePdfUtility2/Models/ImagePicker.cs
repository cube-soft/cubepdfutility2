/* ------------------------------------------------------------------------- */
///
/// ImagePicker.cs
///
/// Copyright (c) 2014 CubeSoft, Inc. All rights reserved.
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
using System.Drawing;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Cube;
using CubePdf.Data;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// ImagePicker
    ///
    /// <summary>
    /// 画像を抽出するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ImagePicker
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ImagePicker
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ImagePicker()
        {
            _worker.ProgressChanged += Worker_ProgressChanged;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Pages
        /// 
        /// <summary>
        /// 抽出対象となるページ一覧を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IList<PageBase> Pages
        {
            get { return _worker.Pages; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Images
        /// 
        /// <summary>
        /// 抽出した画像の一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ObservableCollection<Image> Images { get; } = new ObservableCollection<Image>();

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Images
        /// 
        /// <summary>
        /// 抽出した画像の一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Run()
        {
            _worker.RunAsync();
        }

        /* --------------------------------------------------------------------- */
        ///
        /// GetImage
        /// 
        /// <summary>
        /// 指定されたインデックスに対応する画像を upper に応じてリサイズして
        /// 返します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public Image GetImage(int index, Size upper)
        {
            if (index < 0 || index >= Images.Count) return null;

            var original = Images[index];
            var ratio = original.Width > original.Height ?
                Math.Min(upper.Width / (double)original.Width, 1.0) :
                Math.Min(upper.Height / (double)original.Height, 1.0);
            var width = (int)(original.Width * ratio);
            var height = (int)(original.Height * ratio);
            var x = (upper.Width - width) / 2;
            var y = (upper.Height - height) / 2;

            var dest = new Bitmap(upper.Width, upper.Height);
            using (var gs = Graphics.FromImage(dest))
            {
                gs.DrawImage(original, x, y, width, height);
            }
            return dest;
        }

        #endregion

        #region Event handlers

        /* ----------------------------------------------------------------- */
        ///
        /// Worker_ProgressChanged
        /// 
        /// <summary>
        /// 進捗状況が変化した時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Worker_ProgressChanged(object sender, ProgressEventArgs<CubePdf.Wpf.ImageList> e)
        {
            var key = _raw.Count;
            _raw.Add(e.Value);

            for (var i = 0; i < e.Value.Images.Count; ++i)
            {
                _map.Add(new KeyValuePair<int, int>(key, i));
                _backup.Add(e.Value.Images[i]);
                Images.Add(e.Value.Images[i]);
            }
        }

        #endregion

        #region Fields
        private CubePdf.Wpf.BackgroundImageExtractor _worker = new CubePdf.Wpf.BackgroundImageExtractor();
        private IList<CubePdf.Wpf.ImageList> _raw = new List<CubePdf.Wpf.ImageList>();
        private IList<KeyValuePair<int, int>> _map = new List<KeyValuePair<int, int>>();
        private IList<Image> _backup = new List<Image>();
        #endregion
    }
}
