/* ------------------------------------------------------------------------- */
///
/// ThumbnailForm.cs
///
/// Copyright (c) 2013 CubeSoft, Inc.
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
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using Cube;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// ThumbnailForm
    /// 
    /// <summary>
    /// PDF に含まれる画像の一覧を表示するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public partial class ThumbnailForm : Form
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ThumbnailForm
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ThumbnailForm()
        {
            InitializeComponent();
            InitializeLayout();

            ImagesListView.ContextMenuStrip = CreateContextMenu();
            ImagesListView.SelectedIndexChanged += (s, e) => { SaveButton.Enabled = AnyItemsSelected; };
            ImagesListView.DoubleClick += (s, e) => RaisePreviewEvent();

            ExitButton.Click += (s, e) => Close();
            SaveButton.Click += (s, e) => RaiseSavedEvent(SelectedIndices);
            SaveAllButton.Click += (s, e) => RaiseSavedEvent();
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// ImageSize
        /// 
        /// <summary>
        /// サムネイルのサイズを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Size ImageSize
        {
            get { return ImagesListView.LargeImageList.ImageSize; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SelectedIndices
        /// 
        /// <summary>
        /// 選択されている画像のインデックスを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IList<int> SelectedIndices
        {
            get
            {
                var dest = new List<int>();
                foreach (int index in ImagesListView.SelectedIndices) dest.Add(index);
                dest.Sort();
                return dest;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AnyItemsSelected
        /// 
        /// <summary>
        /// 画像が一つでも選択されているかどうかを示す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool AnyItemsSelected
        {
            get
            {
                var items = ImagesListView.SelectedItems;
                return items != null && items.Count > 0;
            }
        }

        #endregion

        #region Events

        /* ----------------------------------------------------------------- */
        ///
        /// Saved
        /// 
        /// <summary>
        /// 画像の保存時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public EventHandler<DataEventArgs<IList<int>>> Saved;

        /* ----------------------------------------------------------------- */
        ///
        /// Removed
        /// 
        /// <summary>
        /// 項目の削除時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public EventHandler<DataEventArgs<IList<int>>> Removed;

        /* ----------------------------------------------------------------- */
        ///
        /// Previewed
        /// 
        /// <summary>
        /// 画像がプレビューされる時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public EventHandler<DataEventArgs<int>> Previewed;

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        /// 
        /// <summary>
        /// 新しいサムネイルを追加します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Add(Image image)
        {
            ImagesListView.LargeImageList.Images.Add(image);
            ImagesListView.Items.Add(new ListViewItem(
                string.Empty,
                ImagesListView.LargeImageList.Images.Count - 1
            ));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RemoveAt
        /// 
        /// <summary>
        /// 指定されたインデックスに対応するサムネイルを削除します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= ImagesListView.Items.Count) return;
            ImagesListView.Items.RemoveAt(index);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SelectAll
        /// 
        /// <summary>
        /// 全てのサムネイルを選択します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void SelectAll()
        {
            foreach (ListViewItem item in ImagesListView.Items) item.Selected = true;
        }

        #endregion

        #region Virtual methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnSaved
        /// 
        /// <summary>
        /// 画像の保存時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnSaved(DataEventArgs<IList<int>> e)
        {
            if (Saved != null) Saved(this, e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnRemoved
        /// 
        /// <summary>
        /// 項目の削除時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnRemoved(DataEventArgs<IList<int>> e)
        {
            if (Removed != null) Removed(this, e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnPreviewed
        /// 
        /// <summary>
        /// 画像がプレビューされる時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnPreviewed(DataEventArgs<int> e)
        {
            if (Previewed != null) Previewed(this, e);
        }

        #endregion

        #region Override methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnLoad
        /// 
        /// <summary>
        /// オブジェクトがロードされた時に実行されるハンドラです。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SaveButton.Enabled = false;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnFormClosed
        /// 
        /// <summary>
        /// フォームが閉じた時に実行されるハンドラです。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            ImagesListView.Items.Clear();
            ImagesListView.LargeImageList.Images.Clear();
            ImagesListView.LargeImageList.Dispose();
            ImagesListView.LargeImageList = null;
        }

        #endregion

        #region Other private methods

        /* ----------------------------------------------------------------- */
        ///
        /// InitializeLayout
        /// 
        /// <summary>
        /// レイアウトを初期化します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void InitializeLayout()
        {
            ImagesListView.LargeImageList = new ImageList();
            ImagesListView.LargeImageList.ImageSize = new Size(128, 128);
            ImagesListView.LargeImageList.ColorDepth = ColorDepth.Depth32Bit;

            SetWindowTheme(ImagesListView.Handle, "Explorer", null);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CreateContextMenu
        /// 
        /// <summary>
        /// コンテキストメニューを生成します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private ContextMenuStrip CreateContextMenu()
        {
            var dest = new ContextMenuStrip();

            var preview = dest.Items.Add("プレビュー", null, (s, e) => RaisePreviewEvent());
            var hr0 = dest.Items.Add("-");
            var save = dest.Items.Add("保存", null, (s, e) => RaiseSavedEvent(SelectedIndices));
            var remove = dest.Items.Add("一覧から削除", null, (s, e) => OnRemoved(new DataEventArgs<IList<int>>(SelectedIndices)));
            var hr1 = dest.Items.Add("-");
            var select = dest.Items.Add("全て選択", null, (s, e) => SelectAll());

            Action action = () =>
            {
                preview.Enabled = AnyItemsSelected;
                save.Enabled = AnyItemsSelected;
                remove.Enabled = AnyItemsSelected;
            };

            action();
            ImagesListView.SelectedIndexChanged += (s, e) => action();

            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RaiseSavedEvent
        /// 
        /// <summary>
        /// Saved イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void RaiseSavedEvent()
        {
            var indices = new List<int>();
            for (int i = 0; i < ImagesListView.Items.Count; ++i) indices.Add(i);
            RaiseSavedEvent(indices);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RaiseSavedEvent
        /// 
        /// <summary>
        /// Saved イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void RaiseSavedEvent(IList<int> indices)
        {
            OnSaved(new DataEventArgs<IList<int>>(indices));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RaisePreviewEvent
        /// 
        /// <summary>
        /// Preview イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void RaisePreviewEvent()
        {
            var indices = SelectedIndices;
            if (indices.Count <= 0) return;
            OnPreviewed(new DataEventArgs<int>(indices[0]));
        }

        #endregion

        #region Win32 APIs
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);
        #endregion
    }
}
