/* ------------------------------------------------------------------------- */
///
/// InsertWindow.xaml.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// InsertWindow
    /// 
    /// <summary>
    /// InsertWindow.xaml の相互作用ロジック
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public partial class InsertWindow : Window
    {
        #region Initialization and Termination

        /* ----------------------------------------------------------------- */
        ///
        /// InsertWindow (constructor)
        /// 
        /// <summary>
        /// 既定の値でオブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public InsertWindow()
        {
            InitializeComponent();
            FileListView.ItemsSource = _files;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// InsertWindow (constructor)
        /// 
        /// <summary>
        /// 引数に指定された現在の選択位置と総ページ数の値を用いて
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <remarks>
        /// TODO: index には SelectedIndex を指定するのでどのページも選択
        /// されていない場合は -1 が指定される。その場合は、「現在の選択位置」
        /// のラジオボタンを選択不可能の状態にし、ラジオボタンの選択状況の
        /// 初期値を「先頭」に移す。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public InsertWindow(int index, int total)
            : this()
        {
            _index = index;
            _total = total;
            TotalPageTextBlock.Text = string.Format("/ {0} ページ", _total);
            CurrentRadioButton.IsEnabled = (_index >= 0) ? true : false;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// FileList
        /// 
        /// <summary>
        /// 挿入するファイル一覧を取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IList<System.IO.FileInfo> FileList
        {
            get { return _files; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Index
        /// 
        /// <summary>
        /// 挿入する位置を表すインデックスを取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        #endregion

        #region Commands

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        /// 
        /// <summary>
        /// 挿入するファイルを決定します。
        /// パラメータは常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Save

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_files.Count > 0);
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: ラジオボタンの状態を見て、Index の値を更新する。
            DialogResult = true;
            Close();
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Close
        /// 
        /// <summary>
        /// 挿入処理をキャンセルして終了します。
        /// パラメータは常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Close

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        /// 
        /// <summary>
        /// 新しいファイルを追加します。
        /// パラメータ (e.Parameter) に新しいファイルパスが指定されていれば
        /// そのファイルを追加し、null の場合は OpenFileDialog を表示して
        /// 追加するファイルをユーザに尋ねます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Add

        private void AddCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void AddCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var path = e.Parameter as string;
                if (path == null)
                {
                    var dialog = new System.Windows.Forms.OpenFileDialog();
                    dialog.Filter = Properties.Resources.PdfFilter;
                    dialog.CheckFileExists = true;
                    if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                    path = dialog.FileName;
                }
                if (string.IsNullOrEmpty(path)) return;
                bool double_file = false;
                foreach (var item in _files)
                {
                    if (item.FullName.Equals(new System.IO.FileInfo(path).FullName))
                    {
                        double_file = true;
                    }
                }
                if (!double_file)
                {
                    _files.Add(new System.IO.FileInfo(path));
                }
            }
            catch (Exception err) { Trace.WriteLine(err.ToString()); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Move
        /// 
        /// <summary>
        /// 選択されているファイルを移動します。
        /// パラメータ (e.Parameter) は、移動する量 (1, -1, ...) が指定
        /// されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Move

        private void MoveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (FileListView == null || FileListView.SelectedIndex == -1)
                {
                    e.CanExecute = false;
                    return;
                }
                var index = FileListView.SelectedIndex + (int)e.Parameter;
                e.CanExecute = (0 <= index && index < _files.Count);
            }
            catch (Exception err)
            {
                Trace.WriteLine(err.ToString());
                e.CanExecute = false;
            }
        }

        private void MoveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: implementation
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        /// 
        /// <summary>
        /// 選択されているファイルを一覧から削除します。
        /// パラメータ (e.Parameter) は常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Remove

        private void RemoveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (FileListView != null && FileListView.SelectedIndex != -1);
        }

        private void RemoveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: implementation
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Clear
        /// 
        /// <summary>
        /// ファイル一覧をクリアします。
        /// パラメータ (e.Parameter) は常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Clear

        private void ClearCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_files.Count > 0);
        }

        private void ClearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: implementation
        }

        #endregion

        #endregion

        //#region Events
        ///* ----------------------------------------------------------------- */
        ///
        /// PageNumberTextBox_TextChanged
        /// 
        /// <summary>
        /// テキストが変更されるたび、それが適切なページ番号か確認します
        /// 
        /// </summary>
        /// <remarks> TextChangedイベントを迂闊に用いるとIMEとの絡みでバグが起こる様子。要検証。<remarks>
        ///* ----------------------------------------------------------------- */
        //private void PageNumberTextBox_TextChanged(Object sender, TextChangedEventArgs e)
        //{
        //    int pageNumber;
        //    if (!int.TryParse(PageNumberTextBox.Text, out pageNumber)) {
        //        PageNumberTextBox.Text = "0";
        //        return;
        //    }
        //    else if (pageNumber < 0 | pageNumber > _files.Count)
        //    {
        //        PageNumberTextBox.Text = "0";
        //    }
        //}
        //#endregion

        #region Variables
        private ObservableCollection<System.IO.FileInfo> _files = new ObservableCollection<System.IO.FileInfo>();
        private int _index = 0;
        private int _total = 0;
        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// ICommands
        ///
        /// <summary>
        /// この画面で必要とされるコマンド群を定義します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region ICommand variables
        public static readonly ICommand Add    = new RoutedCommand("Add",    typeof(InsertWindow));
        public static readonly ICommand Move   = new RoutedCommand("Move",   typeof(InsertWindow));
        public static readonly ICommand Remove = new RoutedCommand("Remove", typeof(InsertWindow));
        public static readonly ICommand Clear  = new RoutedCommand("Clear",  typeof(InsertWindow));
        #endregion
    }
}
