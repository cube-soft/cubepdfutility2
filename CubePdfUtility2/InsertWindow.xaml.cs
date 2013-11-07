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
using System.Runtime.InteropServices;

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
            this.SourceInitialized += (x, y) => this.HideMinimizeButtons();
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
            if (HeadRadioButton.IsChecked ?? false) {
                Index = 0;
            }
            else if (TailRadioButton.IsChecked ?? false)
            {
                Index = _total;
            }
            else if (UserInputRadioButton.IsChecked ?? false)
            {
                Index = int.Parse(PageNumberTextBox.Text) - 1;
            }
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
                if (isOpenable(path))
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

        #region Events
        ///* ----------------------------------------------------------------- */
        ///
        /// PageNumberTextBox_TextChanged
        /// 
        /// <summary>
        /// テキストが変更されるたび、それが適切なページ番号か確認します。
        /// </summary>
        ///* ----------------------------------------------------------------- */
        private void PageNumberTextBox_TextChanged(Object sender, TextChangedEventArgs e)
        {
            int pageNumber;
            if (!int.TryParse(PageNumberTextBox.Text, out pageNumber))
            {
                PageNumberTextBox.Text = "";
            }
            else if (pageNumber > _total)
            {
                PageNumberTextBox.Text = "";
            }
        }

        ///* ----------------------------------------------------------------- */
        ///
        /// FileListView_PreviewDragOver
        /// 
        /// <summary>
        /// DragDropEffectsを設定します。
        /// </summary>
        /// 
        ///* ----------------------------------------------------------------- */
        private void FileListView_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        ///* ----------------------------------------------------------------- */
        ///
        /// FileListView_Drop
        /// 
        /// <summary>
        /// 外部Drag＆Drop：OpenできるファイルならOpenしてFileListViewに追加
        /// 内部Drag＆Drop：Drop位置にDrag元のItemを移動する。
        /// </summary>
        /// 
        ///* ----------------------------------------------------------------- */
        private void FileListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                //ファイルをpdfで開くことができるかどうか確かめる必要がある？
                string[] draggedFiles = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (draggedFiles != null)
                {
                    ListView listview = (ListView)this.FindName("FileListView");

                    foreach (var fileName in draggedFiles)
                    {
                        try
                        {
                            if (!fileName.Substring(fileName.Length - 4).ToLower().Equals(".pdf")) continue;
                            if (isOpenable(fileName))
                            {
                                _files.Add(new System.IO.FileInfo(fileName));
                            }
                        }
                        catch (Exception err) { Trace.WriteLine(err.ToString()); continue; }
                    }
                }
            }
            else if (sender is ListView)
            {
                var drop = e.GetPosition(FileListView);
                var item = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;
                var index = _files.IndexOf(item.DataContext as System.IO.FileInfo);
                for (int i = 0; i < _files.Count; i++)
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

        ///* ----------------------------------------------------------------- */
        ///
        /// FileListView_PreviewMouseLeftButtonDown
        /// 
        /// <summary>
        /// 内部Drag＆Drop：Drag元のItemとその座標を取得する。
        /// </summary>
        /// 
        ///* ----------------------------------------------------------------- */
        private void FileListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem)
            {
                _dragTarget = sender as ListViewItem;
                _dragStart = e.GetPosition(_dragTarget);
            }
        }

        ///* ----------------------------------------------------------------- */
        ///
        /// FileListView_PreviewMouseLeftButtonDown
        /// 
        /// <summary>
        /// 内部Drag＆Drop：Drag先のItemとその座標を取得し、その差が一定以上なら
        /// DoDragDropを呼び出してItemを移動させる。
        /// </summary>
        /// 
        ///* ----------------------------------------------------------------- */
        private void FileListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is ListViewItem)
            {
                var item = sender as ListViewItem;
                if (e.LeftButton == MouseButtonState.Pressed && _dragTarget == item)
                {
                    var now = e.GetPosition(item);
                    if (Math.Abs(now.X - _dragStart.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(now.Y - _dragStart.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        DragDrop.DoDragDrop(item, item, DragDropEffects.Move);
                        _dragTarget = null;
                    }
                }
            }
        }
       
        #endregion

        #region isOpenable
        ///* ----------------------------------------------------------------- */
        ///
        /// FileListView_Drop
        /// 
        /// <summary>
        /// pathで指定されたファイルがOpen可能か判定します。
        /// </summary>
        /// 
        ///* ----------------------------------------------------------------- */
        private bool isOpenable(String path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            foreach (var item in _files)
            {
                if (item.FullName.Equals(new System.IO.FileInfo(path).FullName))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Variables
        private ObservableCollection<System.IO.FileInfo> _files = new ObservableCollection<System.IO.FileInfo>();
        private int _index = 0;
        private int _total = 0;
        private ListViewItem _dragTarget;
        private Point _dragStart;
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

    /* --------------------------------------------------------------------- */
    ///
    /// WindowExtensions
    /// 
    /// <summary>
    /// Windowクラスの拡張メソッド（最小化・最大化ボタン無効）用クラス
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class WindowExtensions
    {
        private const int GWL_STYLE = -16,
                          WS_MAXIMIZEBOX = 0x10000,
                          WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        extern private static int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        extern private static int SetWindowLong(IntPtr hwnd, int index, int value);

        public static void HideMinimizeButtons(this Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MINIMIZEBOX));
        }

        public static void HideMaximizeButtons(this Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MAXIMIZEBOX));
        }
    }
}
