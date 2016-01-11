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
using System.ComponentModel;
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
        /// Run
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
        /// SaveAsync
        /// 
        /// <summary>
        /// 非同期で保存処理を実行します。
        /// </summary>
        /// 
        /// <remarks>
        /// 引数に指定される index は現在の状態の Images に対応するものです。
        /// Images は外部から一部または全部が削除される可能性があるので、
        /// いったんバックアップデータを用いて元々のインデックスに変換します。
        /// </remarks>
        ///
        /* --------------------------------------------------------------------- */
        public void SaveAsync(IList<int> indices, string folder, Action done)
        {
            var worker = new BackgroundWorker();
            worker.RunWorkerCompleted += (s, e) => { if (done != null) done(); };
            worker.DoWork += (s, e) =>
            {
                foreach (var index in indices)
                {
                    if (index < 0 || index >= Images.Count) continue;

                    var original = _backup.IndexOf(Images[index]);
                    if (original == -1) continue;

                    var map = _map[original];
                    if (map.Key   < 0 || map.Key   >= _raw.Count ||
                        map.Value < 0 || map.Value >= _raw[map.Key].Images.Count) continue;

                    var page = _raw[map.Key].Page;
                    var count = _raw[map.Key].Images.Count;
                    var basename = System.IO.Path.GetFileNameWithoutExtension(page.FilePath);
                    var dest = Unique(folder, basename, page.PageNumber, map.Value, count);

                    var image = _raw[map.Key].Images[map.Value];
                    if (image == null) continue;
                    image.Save(dest, System.Drawing.Imaging.ImageFormat.Png);
                }
            };
            worker.RunWorkerAsync();
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
            var power = original.Width > original.Height ?
                Math.Min(upper.Width / (double)original.Width, 1.0) :
                Math.Min(upper.Height / (double)original.Height, 1.0);
            var width = (int)(original.Width * power);
            var height = (int)(original.Height * power);
            var x = (upper.Width - width) / 2;
            var y = (upper.Height - height) / 2;

            var dest = new Bitmap(upper.Width, upper.Height);
            using (var gs = Graphics.FromImage(dest))
            {
                gs.DrawImage(original, x, y, width, height);
            }
            return dest;
        }

        /* --------------------------------------------------------------------- */
        ///
        /// GetImage
        /// 
        /// <summary>
        /// 指定されたインデックスに対応する画像を指定倍率にリサイズして
        /// 返します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public Image GetImage(int index, double power)
        {
            if (index < 0 || index >= Images.Count) return null;

            var original = Images[index];
            var width = (int)(original.Width * power);
            var height = (int)(original.Height * power);

            var dest = new Bitmap(width, height);
            using (var gs = Graphics.FromImage(dest))
            {
                gs.DrawImage(original, 0, 0, width, height);
            }
            return dest;
        }

        /* --------------------------------------------------------------------- */
        ///
        /// GetPage
        /// 
        /// <summary>
        /// 指定されたインデックスに対応する画像が含まれるページを返します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public PageBase GetPage(int index)
        {
            if (index < 0 || index >= Images.Count) return null;

            var original = _backup.IndexOf(Images[index]);
            if (original == -1) return null;

            var map = _map[original];
            if (map.Key < 0 || map.Key >= _raw.Count) return null;

            return _raw[map.Key].Page;
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

        #region Other private methods

        /* ----------------------------------------------------------------- */
        ///
        /// Unique
        /// 
        /// <summary>
        /// 一意のパス名を取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private string Unique(string folder, string basename, int pagenum, int index, int count)
        {
            var digit = string.Format("D{0}", count.ToString("D").Length);
            for (var i = 1; i < 1000; ++i)
            {
                var filename = (i == 1) ?
                               string.Format("{0}-{1}-{2}.png", basename, pagenum, index.ToString(digit)) :
                               string.Format("{0}-{1}-{2} ({3}).png", basename, pagenum, index.ToString(digit), i);
                var dest = System.IO.Path.Combine(folder, filename);
                if (!System.IO.File.Exists(dest)) return dest;
            }

            return System.IO.Path.Combine(folder, System.IO.Path.GetRandomFileName());
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
