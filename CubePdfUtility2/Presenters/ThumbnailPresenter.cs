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
using System.Linq;
using System.Text;

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

            Model.Images.CollectionChanged += Model_CollectionChanged;
        }

        #endregion

        #region Event handlers

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
                default:
                    break;
            }
        }

        #endregion
    }
}
