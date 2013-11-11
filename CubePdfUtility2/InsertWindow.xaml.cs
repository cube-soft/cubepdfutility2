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
            this.SourceInitialized += (x, y) => this.HideMinimizeButton();
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
        /* ----------------------------------------------------------------- */
        public InsertWindow(int index, int total)
            : this()
        {
            _index = index;
            _total = total;
            TotalPageTextBlock.Text = string.Format("/ {0} ページの後ろ", _total);
            if (_index < 0)
            {
                CurrentRadioButton.IsEnabled = false;
                HeadRadioButton.IsChecked = true;
            }
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
            var index = Index + 1;
            if (HeadRadioButton.IsChecked != false) index = 0;
            else if (TailRadioButton.IsChecked != false) index = _total;
            else if (UserInputRadioButton.IsChecked != false) index = int.Parse(PageNumberTextBox.Text);
            Index = Math.Min(Math.Max(index, 0), _total);

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
                if (path != null) AddFile(path);
                else
                {
                    var dialog = new System.Windows.Forms.OpenFileDialog();
                    dialog.Filter = Properties.Resources.PdfFilter;
                    dialog.CheckFileExists = true;
                    dialog.Multiselect = true;
                    if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                    AddFiles(dialog.FileNames);
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
            _files.Move(FileListView.SelectedIndex, FileListView.SelectedIndex + (int)e.Parameter);
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
            _files.RemoveAt(FileListView.SelectedIndex);
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
            _files.Clear();
        }

        #endregion

        #endregion

        #region Events handlers

        /* ----------------------------------------------------------------- */
        ///
        /// PageNumberTextBox_TextChanged
        /// 
        /// <summary>
        /// テキストが変更された時に実行されるイベントハンドラです。数値
        /// 以外の文字の入力をキャンセルします。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void PageNumberTextBox_TextChanged(Object sender, TextChangedEventArgs e)
        {
            var control = sender as TextBox;
            if (control == null) return;

            var page = 0;
            if (control.Text.Length > 0 && !int.TryParse(control.Text, out page))
            {
                var pos = control.SelectionStart;
                var index = Math.Max(PageNumberTextBox.Text.Length - 1, 0);
                control.Text = control.Text.Remove(index);
                control.SelectionStart = Math.Max(pos - 1, 0);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// FileListView_PreviewDragOver
        /// 
        /// <summary>
        /// ファイルリストにオブジェクトがドラッグされた時に実行される
        /// イベントハンドラです。オブジェクトの種類に応じた DragDropEffects
        /// を設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void FileListView_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true)) e.Effects = DragDropEffects.Copy;
            else if (e.Data.GetDataPresent(typeof(ListViewItem))) e.Effects = DragDropEffects.Move;
            else e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// FileListView_Drop
        /// 
        /// <summary>
        /// ドロップ時に実行されるイベントハンドラです。
        /// </summary>
        /// 
        /// <remarks>
        /// 外部Drag＆Drop：OpenできるファイルならOpenしてFileListViewに追加
        /// 内部Drag＆Drop：Drop位置にDrag元のItemを移動する。
        /// </remarks>
        /// 
        /* ----------------------------------------------------------------- */
        private void FileListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                AddFiles(files);
            }
            else if (sender is ListView)
            {
                var drop = e.GetPosition(FileListView);
                var item = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;
                var index = _files.IndexOf(item.DataContext as System.IO.FileInfo);
                for (int i = 0; i < _files.Count; ++i)
                {
                    var t = FileListView.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    var p = FileListView.PointFromScreen(t.PointToScreen(new Point(0, item.ActualHeight / 2)));
                    if (drop.Y < p.Y)
                    {
                        _files.Move(index, (index < i) ? i - 1 : i);
                        return;
                    }
                }
                _files.Move(index, _files.Count - 1);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// FileListView_PreviewMouseLeftButtonDown
        /// 
        /// <summary>
        /// マウス押下時にに実行されるイベントハンドラです。押下された項目と
        /// その座標を取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void FileListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item == null) return;

            _target = item;
            _start = e.GetPosition(_target);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// FileListView_PreviewMouseMove
        /// 
        /// <summary>
        /// マウスが動いた際に実行されるイベントハンドラです。Drag 先の
        /// Item とその座標を取得し、その差が一定以上なら Item を移動
        /// させます。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void FileListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item == null) return;

            if (e.LeftButton == MouseButtonState.Pressed && _target == item)
            {
                var now = e.GetPosition(item);
                if (Math.Abs(now.X - _start.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(now.Y - _start.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DragDrop.DoDragDrop(item, item, DragDropEffects.Move);
                    _target = null;
                }
            }
        }
       
        #endregion

        #region Other methods

        /* ----------------------------------------------------------------- */
        ///
        /// IsValidFile
        /// 
        /// <summary>
        /// 引数に指定されたファイルがリストに追加しても良いものかどうかを
        /// 判定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private bool IsValidFile(System.IO.FileInfo info)
        {
            foreach (var item in _files)
            {
                if (info.FullName == item.FullName) return false;
            }
            return true;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AddFile
        /// 
        /// <summary>
        /// 引数に指定されたファイルを一覧に追加します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void AddFile(string file)
        {
            if (string.IsNullOrEmpty(file)) return;
            if (!System.IO.File.Exists(file) || System.IO.Path.GetExtension(file) != ".pdf") return; 

            try
            {
                var info = new System.IO.FileInfo(file);
                if (IsValidFile(info)) _files.Add(info);
            }
            catch (Exception err) { Trace.WriteLine(err.ToString()); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AddFiles
        /// 
        /// <summary>
        /// 引数に指定されたファイルを一覧に追加します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void AddFiles(string[] files)
        {
            if (files == null) return;
            foreach (var file in files) AddFile(file);
        }

        #endregion

        #region Variables
        private ObservableCollection<System.IO.FileInfo> _files = new ObservableCollection<System.IO.FileInfo>();
        private int _index = 0;
        private int _total = 0;
        private ListViewItem _target = null;
        private Point _start;
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
