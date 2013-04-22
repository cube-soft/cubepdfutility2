/* ------------------------------------------------------------------------- */
///
/// MainWindow.xaml.cs
///
/// Copyright (c) 2013 CubeSoft, Inc. All rights reserved.
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with this program.  If not, see < http://www.gnu.org/licenses/ >.
///
/* ------------------------------------------------------------------------- */
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Windows.Controls.Ribbon;
using IWshRuntimeLibrary;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// MainWindows
    /// 
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public partial class MainWindow : RibbonWindow
    {
        #region Initialization and Termination

        /* ----------------------------------------------------------------- */
        /// MainWindow (static constructor)
        /* ----------------------------------------------------------------- */
        static MainWindow()
        {
            _ViewSize = new List<KeyValuePair<int, string>>() {
                new KeyValuePair<int, string>(64,   "64px"),
                new KeyValuePair<int, string>(128, "128px"),
                new KeyValuePair<int, string>(150, "150px"),
                new KeyValuePair<int, string>(256, "256px"),
                new KeyValuePair<int, string>(300, "300px"),
                new KeyValuePair<int, string>(512, "512px"),
                new KeyValuePair<int, string>(600, "600px"),
            };
        }

        /* ----------------------------------------------------------------- */
        /// MainWindow (constructor)
        /* ----------------------------------------------------------------- */
        public MainWindow()
        {
            InitializeComponent();
            
            var size = _ViewSize[2];
            _viewmodel.ItemWidth = size.Key;
            ThumbnailImageView.ItemWidth = _viewmodel.ItemWidth;
            ViewSizeGalleryCategory.ItemsSource = _ViewSize;
            ViewSizeGallery.SelectedItem = size;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// ProductName
        /// 
        /// <summary>
        /// このアプリケーションの製品名を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string ProductName
        {
            get
            {
                var asm = Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(),
                    typeof(System.Reflection.AssemblyProductAttribute)) as System.Reflection.AssemblyProductAttribute;
                return (asm != null) ? asm.Product : string.Empty;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ViewModel
        /// 
        /// <summary>
        /// このウィンドウに関連付けられている ViewModel を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public CubePdf.Wpf.IListViewModel ViewModel
        {
            get { return _viewmodel; }
        }

        #endregion

        #region Commands

        /* ----------------------------------------------------------------- */
        ///
        /// Open
        ///
        /// <summary>
        /// PDF ファイルを開きます。
        /// パラメータ (e.Parameter) は、PDF ファイルへのパス、または null
        /// です。パラメータが null の場合、ファイルを開くためのダイアログ
        /// から指定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Open

        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
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
                if (!String.IsNullOrEmpty(_viewmodel.FilePath) && !CloseFile()) return;
                OpenFile(path, "");
            }
            catch (Exception err) { Debug.WriteLine(err.GetType().ToString()); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Close
        /// 
        /// <summary>
        /// 現在、開いている PDF ファイルを閉じます。
        /// パラメータ (e.Parameter) に "Exit" の文字列（を含む文字列）が
        /// 指定された場合はアプリケーション自体も終了します。
        /// 
        /// TODO: 内容が編集されている場合は、保存するかどうかを尋ねる
        /// ダイアログを表示する。
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
            try
            {
                e.Handled = CloseFile();
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally
            {
                Refresh();
                if (e.Handled)
                {
                    var name = (e != null) ? e.Parameter as string : null;
                    if (name != null && name.IndexOf("Exit") >= 0) Application.Current.Shutdown();
                }
            }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        ///
        /// <summary>
        /// 現在の内容で上書き保存します。
        /// パラメータ (e.Parameter) は常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Save

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (SaveButton != null) SaveButton.IsEnabled = _viewmodel.PageCount > 0;
            e.CanExecute = _viewmodel.PageCount > 0;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                _viewmodel.Save();
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// SaveAs
        /// 
        /// <summary>
        /// 現在の内容を別名として保存します。
        /// パラメータ (e.Parameter) は常に null です。新しいファイル名は、
        /// ファイル保存用ダイアログから指定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region SaveAs

        private void SaveAsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _viewmodel.PageCount > 0;
        }

        private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var dialog = new System.Windows.Forms.SaveFileDialog();
                dialog.Filter = Properties.Resources.PdfFilter;
                dialog.OverwritePrompt = true;
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                _viewmodel.Save(dialog.FileName);
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Insert
        /// 
        /// <summary>
        /// 現在、開いている PDF ファイルに新しい PDF ファイルを挿入します。
        /// パラメータ (e.Parameter) は、null、または挿入位置へのインデックス
        /// です。挿入位置へのインデックスが指定された場合はその直後に、
        /// null の場合は先頭に挿入します。パラメータが -1 の場合は、挿入位置
        /// が指定されていない事を表します（ListView.SelectedIndex の挙動）。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Insert

        private void InsertCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            this.InsertButton.IsEnabled = _viewmodel.PageCount > 0;
            e.CanExecute = (e.Parameter == null || (int)e.Parameter >= 0);
        }

        private void InsertCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var index = (e.Parameter != null) ? Math.Min((int)e.Parameter + 1, _viewmodel.PageCount) : 0;
                var obj = (index == 0) ? InsertHead.Header
                    : (index == _viewmodel.PageCount) ? InsertTail.Header
                    : InsertSelect.Header;
                var dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = Properties.Resources.PdfFilter;
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                InsertFile(index, dialog.FileName, "", obj as string);
            }
            catch (Exception err) { Debug.WriteLine(err); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// 現在、開いている PDF ファイルからいくつかのページを削除します。
        /// パラメータ (e.Parameter) は、削除対象となるページのサムネイル
        /// オブジェクトのリスト、または null です。パラメータが null の
        /// 場合は、削除する項目を選択するためのダイアログを表示します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Remove

        private void RemoveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            this.RemoveButton.IsEnabled = _viewmodel.PageCount > 0;
            var items = e.Parameter as IList;
            e.CanExecute = (items == null || items.Count > 0 && items.Count < Thumbnail.Items.Count);
        }

        private void RemoveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var items = new ArrayList();
            var src = e.Parameter as IList;
            if (src == null)
            {
                var dialog = new RemoveWindow(_viewmodel);
                dialog.Owner = this;
                if (dialog.ShowDialog() == false) return;
                foreach (var i in dialog.PageRange) items.Add(_viewmodel.Items[i - 1]);
            }
            else items.AddRange(src);

            try
            {
                _viewmodel.BeginCommand();
                while (items.Count > 0)
                {
                    var index = items.Count - 1;
                    _viewmodel.Remove(items[index]);
                    items.RemoveAt(index);
                }

                var obj = (src == null) ? RemoveRange.Header : RemoveSelect.Header;
                _viewmodel.History[0].Text = obj as string;
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally
            {
                _viewmodel.EndCommand();
                Refresh();
            }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Extract
        ///
        /// <summary>
        /// 一部、または全部のページを新しい PDF ファイルとして抽出します。
        /// パラメータ (e.Parameter) は、抽出対象となるページのサムネイル
        /// オブジェクトのリスト、または null です。パラメータが null の
        /// 場合は、全ページを抽出対象とします。
        /// 
        /// Extract コマンドと Split コマンドの違いは、一つのファイルとして
        /// 保存するか、個別のファイルとして保存するかです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Extract

        private void ExtractCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            this.ExtractButton.IsEnabled = _viewmodel.PageCount > 0;
            var items = e.Parameter as IList;
            if (items == null) items = _viewmodel.Items;
            e.CanExecute = items.Count > 0;
        }

        private void ExtractCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var items = e.Parameter as IList;
                if (items == null) items = _viewmodel.Items;

                var dialog = new System.Windows.Forms.SaveFileDialog();
                dialog.Filter = Properties.Resources.PdfFilter;
                dialog.OverwritePrompt = true;
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                _viewmodel.Extract(items, dialog.FileName);
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Split
        ///
        /// <summary>
        /// 一部、または全部のページを新しい PDF ファイルとして抽出します。
        /// パラメータ (e.Parameter) は、抽出対象となるページのサムネイル
        /// オブジェクトのリスト、または null です。パラメータが null の
        /// 場合は、全ページを抽出対象とします。
        /// 
        /// Extract コマンドと Split コマンドの違いは、一つのファイルとして
        /// 保存するか、個別のファイルとして保存するかです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Split

        private void SplitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var items = e.Parameter as IList;
            if (items == null) items = _viewmodel.Items;
            e.CanExecute = items.Count > 0;
        }

        private void SplitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var items = e.Parameter as IList;
                if (items == null) items = _viewmodel.Items;

                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                _viewmodel.Split(items, dialog.SelectedPath);
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Move
        ///
        /// <summary>
        /// ページ順序を変更します。
        /// 
        /// TODO: パラメータ (e.Parameter) の整備
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Move

        private void MoveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var control = this.Thumbnail;
            e.CanExecute = (control != null && control.SelectedItem != null && control.SelectedItems.Count < control.Items.Count);
        }

        private void MoveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter == null) return;

            try
            {
                var delta = int.Parse(e.Parameter as string);
                if (delta == 0) return;

                var indices = new List<int>();
                foreach (var item in Thumbnail.SelectedItems)
                {
                    var index = _viewmodel.IndexOf(item);
                    indices.Add(index);
                }

                _viewmodel.BeginCommand();
                var sorted = (delta < 0) ? indices.OrderBy(i => i) : indices.OrderByDescending(i => i);
                foreach (var oldindex in sorted)
                {
                    if (oldindex < 0) continue;
                    var newindex = oldindex + delta;
                    if (newindex < 0 || newindex >= _viewmodel.PageCount) continue;
                    _viewmodel.Move(oldindex, newindex);
                }
                _viewmodel.History[0].Text = (delta < 0) ? ForwardButton.Label : BackButton.Label;
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally
            {
                _viewmodel.EndCommand();
                Refresh();
            }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Rotate
        ///
        /// <summary>
        /// 現在、選択されているページを回転します。
        /// 
        /// TODO: パラメータ (e.Parameter) の整備
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Rotate

        private void RotateCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var control = this.Thumbnail;
            e.CanExecute = (control != null && control.SelectedItem != null);
        }

        private void RotateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var degree = int.Parse(e.Parameter as string);
                var done = new System.Collections.ArrayList();
                _viewmodel.BeginCommand();
                while (Thumbnail.SelectedItems.Count > 0)
                {
                    var obj = Thumbnail.SelectedItems[0];
                    _viewmodel.Rotate(obj, degree);
                    done.Add(obj);
                    Thumbnail.SelectedItems.Remove(obj);
                }
                foreach (var obj in done) Thumbnail.SelectedItems.Add(obj);
                _viewmodel.History[0].Text = (degree < 0) ? RotateLeftButton.Label : RotateRightButton.Label;
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally
            {
                _viewmodel.EndCommand();
                Refresh();
            }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Undo
        ///
        /// <summary>
        /// 直前の操作を元に戻します。
        /// パラメータ (e.Parameter) は、常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Undo

        private void UndoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _viewmodel.History.Count > 0;
            UndoButton.IsEnabled = e.CanExecute;
        }

        private void UndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var count = (e.Parameter != null) ? HistoryGallery.Items.IndexOf(e.Parameter) + 1 : 1;
                for (var i = 0; i < count; ++i)
                {
                    var text = _viewmodel.History[0].Text;
                    _viewmodel.Undo();
                    _viewmodel.UndoHistory[0].Text = text;
                }
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Redo
        ///
        /// <summary>
        /// 直前に元に戻した操作を再実行します。
        /// パラメータ (e.Parameter) は、常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Redo

        private void RedoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _viewmodel.UndoHistory.Count > 0;
            RedoButton.IsEnabled = e.CanExecute;
        }

        private void RedoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var count = (e.Parameter != null) ? UndoHistoryGallery.Items.IndexOf(e.Parameter) + 1 : 1;
                for (var i = 0; i < count; ++i)
                {
                    var text = _viewmodel.UndoHistory[0].Text;
                    _viewmodel.Redo();
                    _viewmodel.History[0].Text = text;
                }
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Select
        ///
        /// <summary>
        /// ListView の項目選択に関するコマンドです。
        /// パラメータ (e.Parameter) は、現在、選択されているオブジェクトの
        /// リスト、または null です。選択されているオブジェクトのリストが
        /// 指定された場合は、選択を反転させます。null の場合は、全選択
        /// されている場合は選択の解除、それ以外の場合は全選択を行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Select

        private void SelectCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool enabled = _viewmodel.PageCount > 0;
            SelectButton.IsEnabled = enabled;
            e.CanExecute = enabled;
        }

        private void SelectCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var items = e.Parameter as IList;
                if (items == null)
                {
                    if (Thumbnail.SelectedItems.Count == Thumbnail.Items.Count) Thumbnail.UnselectAll();
                    else Thumbnail.SelectAll();
                    return;
                }

                var selected = new ArrayList();
                foreach (var item in Thumbnail.Items)
                {
                    if (!items.Contains(item)) selected.Add(item);
                }
                Thumbnail.SelectedItems.Clear();
                foreach (var item in selected) Thumbnail.SelectedItems.Add(item);
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// SelectAll
        ///
        /// <summary>
        /// ListView の全項目を選択します。
        /// パラメータ (e.Parameter) は常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region SelectAll

        private void SelectAllCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _viewmodel.PageCount > 0;
        }

        private void SelectAllCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Thumbnail.SelectAll();
            Refresh();
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// UnSelect
        ///
        /// <summary>
        /// ListView で選択されている全ての項目を解除します。
        /// パラメータ (e.Parameter) は常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region UnSelect

        private void UnSelectCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var control = this.Thumbnail;
            e.CanExecute = (control != null && control.SelectedItem != null);
        }

        private void UnSelectCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Thumbnail.UnselectAll();
            Refresh();
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Metadata
        ///
        /// <summary>
        /// PDF のメタデータを編集するためのダイアログを表示します。
        /// パラメータ (e.Parameter) は常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Metadata

        private void MetadataCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _viewmodel.PageCount > 0;
        }

        private void MetadataCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new MetadataWindow(_viewmodel);
            dialog.Owner = this;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            if (dialog.ShowDialog() == true) _viewmodel.Metadata = dialog.Metadata;
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// ZoomIn
        ///
        /// <summary>
        /// サムネイルのサイズを 1 段階拡大します。
        /// パラメータ (e.Parameter) は常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region ZoomIn

        private void ZoomInCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                var item = (KeyValuePair<int, string>)ViewSizeGallery.SelectedItem;
                var index = _ViewSize.IndexOf(item);
                e.CanExecute = index < _ViewSize.Count - 1;
            }
            catch (Exception err) { Debug.WriteLine(err); }
        }

        private void ZoomInCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var item = (KeyValuePair<int, string>)ViewSizeGallery.SelectedItem;
                var index = _ViewSize.IndexOf(item);
                ViewSizeGallery.SelectedItem = _ViewSize[index + 1];
            }
            catch (Exception err) { Debug.WriteLine(err); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// ZoomOut
        ///
        /// <summary>
        /// サムネイルのサイズを 1 段階縮小します。
        /// パラメータ (e.Parameter) は常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region ZoomOut

        private void ZoomOutCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                var item = (KeyValuePair<int, string>)ViewSizeGallery.SelectedItem;
                var index = _ViewSize.IndexOf(item);
                e.CanExecute = index > 0;
            }
            catch (Exception err) { Debug.WriteLine(err); }
        }

        private void ZoomOutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var item = (KeyValuePair<int, string>)ViewSizeGallery.SelectedItem;
                var index = _ViewSize.IndexOf(item);
                ViewSizeGallery.SelectedItem = _ViewSize[index - 1];
            }
            catch (Exception err) { Debug.WriteLine(err); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// ViewSize
        ///
        /// <summary>
        /// サムネイルのサイズを変更します。
        /// パラメータ (e.Parameter) はサムネイルの新しい幅が指定されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region ViewSize

        private void ViewSizeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ViewSizeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var width = (int)e.Parameter;
                if (_viewmodel.ItemWidth != width)
                {
                    _viewmodel.ItemWidth = width;
                    ThumbnailImageView.ItemWidth = _viewmodel.ItemWidth;
                }
            }
            catch (Exception err) { Debug.WriteLine(err); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// ViewMode
        ///
        /// <summary>
        /// 枠線のみ表示するかどうかを変更します。
        /// パラメータ (e.Parameter) は「枠線のみ表示するかどうか」を表す
        /// 真偽値です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region ViewMode

        private void ViewModeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ViewModeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var enable = (bool)e.Parameter;
                _viewmodel.ItemVisibility = enable ?
                    CubePdf.Wpf.ListViewItemVisibility.Minimum :
                    CubePdf.Wpf.ListViewItemVisibility.Normal;
            }
            catch (Exception err) { Debug.WriteLine(err); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Redraw
        ///
        /// <summary>
        /// サムネイルを再描画します。
        /// パラメータ (e.Parameter) は常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Redraw

        private void RedrawCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _viewmodel.PageCount > 0;
        }

        private void RedrawCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                _viewmodel.Reset();
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Encryption
        ///
        /// <summary>
        /// PDF のセキュリティ項目を編集するためのダイアログを表示します。
        /// パラメータ (e.Parameter) は常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Encryption

        private void EncryptionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _viewmodel.PageCount > 0;
        }

        private void EncryptionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new EncryptionWindow(_viewmodel);
            dialog.Owner = this;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            if (dialog.ShowDialog() == true) _viewmodel.Encryption = dialog.Encryption;
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Version
        ///
        /// <summary>
        /// CubePDF Utility のバージョン情報を確認するためのダイアログを
        /// 表示します。
        /// パラメータ (e.Parameter) は常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Version

        private void VersionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void VersionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new VersionWindow();
            dialog.Owner = this;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }

        #endregion

        #endregion

        #region Event handlers

        /* ----------------------------------------------------------------- */
        ///
        /// OnClosing
        /// 
        /// <summary>
        /// アプリケーションの終了時に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var result = CloseFile();
            e.Cancel = !result;
            base.OnClosing(e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnPreviewDragOver
        ///
        /// <summary>
        /// ウィンドウに何らかの項目がドラッグされた時に実行されるイベント
        /// ハンドラです。PDF ファイルの場合は受け入れる（開く）事を通知
        /// します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnPreviewDragOver(DragEventArgs e)
        {
            base.OnPreviewDragOver(e);

            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null) return;

            e.Handled = true;
            foreach (var file in files)
            {
                if (System.IO.Path.GetExtension(file) == Properties.Resources.PdfExtension)
                {
                    e.Effects = DragDropEffects.All;
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnDrop
        ///
        /// <summary>
        /// ウィンドウにドロップされた時に実行されるイベントハンドラです。
        /// PDF ファイルの場合は、該当ファイルを開きます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);

            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null) return;

            foreach (var file in files)
            {
                if (System.IO.Path.GetExtension(file) == Properties.Resources.PdfExtension)
                {
                    OpenFile(file, "");
                    e.Handled = true;
                    return;
                }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnPreview
        /// 
        /// <summary>
        /// プレビュー画面を開きます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void OnPreview(object sender, EventArgs e)
        {
            if (Thumbnail == null || Thumbnail.SelectedIndex == -1) return;
            var dialog = new PreviewWindow(_viewmodel, Thumbnail.SelectedIndex);
            dialog.ShowDialog();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ApplicationMenu_Loaded
        /// 
        /// <summary>
        /// リボンアプリケーションが読み込まれた際に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void ApplicationMenu_Loaded(object sender, RoutedEventArgs e)
        {   
            var recents = CubePdf.Data.SystemEnvironment.GetRecentFiles("*.pdf");
            for (int i = 0; i < recents.Count; ++i)
            {
                var gallery = new RibbonGalleryItem();
                gallery.Content = String.Format("{0} {1}", i + 1, System.IO.Path.GetFileName(recents[i]));
                gallery.Tag = recents[i];
                RecentFilesGallery.Items.Add(gallery);
            }
        }

        #endregion

        #region Other Methods

        /* ----------------------------------------------------------------- */
        ///
        /// OpenFile
        /// 
        /// <summary>
        /// 指定されたパスの PDF ファイルを開きます。パスワードが設定されて
        /// いる場合は、パスワードを入力するためのダイアログを表示して
        /// ユーザに入力してもらいます。入力されたパスワードが間違っていた
        /// 場合は、正しいパスワードが入力されるか、またはキャンセルボタンが
        /// 押下されるまでダイアログを表示し続けます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void OpenFile(string path, string password)
        {
            Cursor = Cursors.Wait;
            var filename = System.IO.Path.GetFileName(path);
            var message = String.Format(Properties.Resources.OpenFile, filename);
            InfoStatusBarItem.Content = message;

            ThreadPool.QueueUserWorkItem(new WaitCallback((Object parameter) => {
                try
                {
                    var reader = new CubePdf.Editing.DocumentReader(path, password);
                    Dispatcher.BeginInvoke(new Action(() => {
                        _viewmodel.Open(reader);
                        Cursor = Cursors.Arrow;
                        Refresh();
                        reader.Dispose();
                    }));
                }
                catch (CubePdf.Data.EncryptionException /* err */)
                {
                    Dispatcher.BeginInvoke(new Action(() => {
                        var dialog = new PasswordWindow(path);
                        dialog.Owner = this;
                        if (dialog.ShowDialog() == true) OpenFile(path, dialog.Password);
                        Cursor = Cursors.Arrow;
                        Refresh();
                    }));
                }
                catch (Exception err) { Debug.WriteLine(err); }
            }), null);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CloseFile
        /// 
        /// <summary>
        /// PDF ファイルを閉じます。編集されていた場合は、ファイルを上書き
        /// 保存するかどうかを尋ねるダイアログを表示します。キャンセル
        /// ボタンが押下された場合は false を、それ以外のボタンが押下された
        /// 場合は true が返ります。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private bool CloseFile()
        {
            if (_viewmodel.IsModified)
            {
                var result = MessageBox.Show(Properties.Resources.IsOverwrite, ProductName,
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                if (result == MessageBoxResult.Cancel) return false;
                if (result == MessageBoxResult.Yes) SaveCommand_Executed(this, null);
            }
            _viewmodel.Close();
            return true;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// InsertFile
        /// 
        /// <summary>
        /// index の位置に指定された PDF ファイルを挿入します。パスワードが
        /// 設定されている場合は、パスワードを入力するためのダイアログを表示
        /// してユーザに入力してもらいます。入力されたパスワードが間違って
        /// いた場合は、正しいパスワードが入力されるか、またはキャンセル
        /// ボタンが押下されるまでダイアログを表示し続けます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void InsertFile(int index, string path, string password, string history)
        {
            Cursor = Cursors.Wait;
            var filename = System.IO.Path.GetFileName(path);
            var message = String.Format(Properties.Resources.InsertFile, filename);
            InfoStatusBarItem.Content = message;

            ThreadPool.QueueUserWorkItem(new WaitCallback((Object parameter) => {
                try
                {
                    var reader = new CubePdf.Editing.DocumentReader(path, password);
                    Dispatcher.BeginInvoke(new Action(() => {
                        _viewmodel.Insert(index, reader);
                        _viewmodel.History[0].Text = history;
                        Cursor = Cursors.Arrow;
                        Refresh();
                        reader.Dispose();
                    }));
                }
                catch (CubePdf.Data.EncryptionException /* err */)
                {
                    Dispatcher.BeginInvoke(new Action(() => {
                        var dialog = new PasswordWindow(path);
                        dialog.Owner = this;
                        if (dialog.ShowDialog() == true) InsertFile(index, path, dialog.Password, history);
                        Cursor = Cursors.Arrow;
                        Refresh();
                    }));
                }
                catch (Exception err) { Debug.WriteLine(err); }
            }), null);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Refresh
        ///
        /// <summary>
        /// 各種コマンドを実行した後に、各種コントロールで表示されている
        /// 情報を更新します。データ・バインディング可能なものに関しては、
        /// できるだけバインディングで対応します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Refresh()
        {
            if (_viewmodel != null && _viewmodel.PageCount > 0)
            {
                var restricted = (_viewmodel.EncryptionStatus == CubePdf.Data.EncryptionStatus.RestrictedAccess);

                var filename = System.IO.Path.GetFileName(_viewmodel.FilePath);
                var rstr = restricted ? "（保護）" : "";
                var mstr = _viewmodel.IsModified ? "*" : "";
                Title = String.Format("{0}{1}{2} - {3}", filename, mstr, rstr, ProductName);

                InfoStatusBarItem.Content = String.Format("{0} ページ", _viewmodel.PageCount);
                LockStatusBarItem.Visibility = restricted ? Visibility.Visible : Visibility.Collapsed;
                Thumbnail.Items.Refresh();
                Thumbnail.Focus();
            }
            else
            {
                Title = ProductName;
                if (InfoStatusBarItem != null) InfoStatusBarItem.Content = string.Empty;
                if (LockStatusBarItem != null) LockStatusBarItem.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Variables
        private CubePdf.Wpf.IListViewModel _viewmodel = new CubePdf.Wpf.ListViewModel();
        #endregion

        #region Static variables
        private static readonly IList<KeyValuePair<int, string>> _ViewSize;
        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// ICommands
        ///
        /// <summary>
        /// このアプリケーション専用で必要とされるコマンド群を定義します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region ICommand variables
        public static readonly ICommand Select   = new RoutedCommand("Select",   typeof(MainWindow));
        public static readonly ICommand UnSelect = new RoutedCommand("UnSelect", typeof(MainWindow));
        public static readonly ICommand Version  = new RoutedCommand("Version",  typeof(MainWindow));
        public static readonly ICommand ZoomIn   = new RoutedCommand("ZoomIn",   typeof(MainWindow));
        public static readonly ICommand ZoomOut  = new RoutedCommand("ZoomOut",  typeof(MainWindow));
        public static readonly ICommand ViewSize = new RoutedCommand("ViewSize", typeof(MainWindow));
        public static readonly ICommand ViewMode = new RoutedCommand("ViewMode", typeof(MainWindow));
        public static readonly ICommand Redraw   = new RoutedCommand("Redraw",   typeof(MainWindow));
        #endregion
    }
}
