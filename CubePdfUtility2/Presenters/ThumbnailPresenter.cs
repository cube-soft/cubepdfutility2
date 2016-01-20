/* ------------------------------------------------------------------------- */
///
/// ThumbnailPresenter.cs
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Linq;
using Cube;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// ThumbnailPresenter
    ///
    /// <summary>
    /// ThumbnailForm とデータを対応付けるためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ThumbnailPresenter : PresenterBase<ThumbnailForm, ImagePicker>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ThumbnailPresenter
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ThumbnailPresenter(ThumbnailForm view, ImagePicker model)
            : base(view, model)
        {
            View.Load += (s, e) => Model.Run();
            View.Saved += View_Saved;
            View.Removed += View_Removed;
            View.Previewed += View_Previewed;

            Model.Images.CollectionChanged += Model_CollectionChanged;
        }

        #endregion

        #region Event handlers

        /* ----------------------------------------------------------------- */
        ///
        /// View_Saved
        /// 
        /// <summary>
        /// 抽出画像の保存時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void View_Saved(object sender, DataEventArgs<IList<int>> e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.Cancel) return;

            View.Cursor = Cursors.WaitCursor;
            Model.SaveAsync(e.Value, dialog.SelectedPath, () =>
            {
                View.Cursor = Cursors.Default;
                View.Close();
            });
        }

        /* ----------------------------------------------------------------- */
        ///
        /// View_Removed
        /// 
        /// <summary>
        /// 抽出画像の一覧から削除される時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void View_Removed(object sender, DataEventArgs<IList<int>> e)
        {
            foreach (var index in e.Value.Reverse()) Model.Images.RemoveAt(index);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// View_Previewed
        /// 
        /// <summary>
        /// 画像がプレビューされる時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void View_Previewed(object sender, DataEventArgs<int> e)
        {
            if (e.Value < 0 || e.Value >= Model.Images.Count) return;

            var image = Model.GetImage(e.Value, 1.0);
            var page  = Model.GetPage(e.Value);
            if (image == null || page == null) return;

            var dialog = new PreviewWindow(image, page);
            dialog.ShowDialog();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Model_CollectionChanged
        /// 
        /// <summary>
        /// コレクションの状態が変化した時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Model_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    View.Add(Model.GetImage(e.NewStartingIndex, View.ImageSize));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    View.RemoveAt(e.OldStartingIndex);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
