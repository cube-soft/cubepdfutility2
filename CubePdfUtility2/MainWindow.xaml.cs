/* ------------------------------------------------------------------------- */
///
/// MainWindow.xaml.cs
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
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
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
        ///
        /// MainWindow (constructor)
        ///
        /// <summary>
        /// 既定の値でオブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MainWindow()
        {
            InitializeComponent();
            ReplaceFont();
            SourceInitialized += new EventHandler(LoadSetting);

            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _viewmodel.View = Thumbnail;
            _viewmodel.BackupFolder = System.IO.Path.Combine(appdata, @"CubeSoft\CubePdfUtility2");
            _viewmodel.BackupDays = 30;
            _viewmodel.RunCompleted += new EventHandler(ViewModel_RunCompleted);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// MainWindow (constructor)
        ///
        /// <summary>
        /// 引数に指定されたパスを利用して、既定の値でオブジェクトを初期化
        /// します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MainWindow(string path)
            : this()
        {
            Loaded += (sender, e) => {
                try { if (!String.IsNullOrEmpty(path))  OpenFileAsync(path, ""); }
                catch (Exception err) { Trace.TraceError(err.ToString()); }
            };
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
        public CubePdf.Wpf.ListViewModel ViewModel
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
                OpenFileAsync(path, "");
            }
            catch (Exception err) { Trace.TraceError(err.ToString()); }
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
            catch (Exception err) { Trace.TraceError(err.ToString()); }
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
            try { _viewmodel.Save(); }
            catch (Exception err)
            {
                MessageBox.Show(Properties.Resources.SaveError, Properties.Resources.ErrorTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Trace.TraceError(err.ToString());
            }
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
            catch (Exception err)
            {
                MessageBox.Show(Properties.Resources.SaveError, Properties.Resources.ErrorTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Trace.TraceError(err.ToString());
            }
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
                InsertFileAsync(index, dialog.FileName, "", obj as string);
            }
            catch (Exception err) { Trace.TraceError(err.ToString()); }
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
                var dialog = new RemoveWindow(_viewmodel, _font);
                dialog.Owner = this;
                if (dialog.ShowDialog() == false) return;
                foreach (var i in dialog.PageRange) items.Add(_viewmodel.Items[i - 1]);
            }
            else items.AddRange(src);

            try
            {
                Cursor = Cursors.Wait;
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
            catch (Exception err) { Trace.TraceError(err.ToString()); }
            finally { _viewmodel.EndCommand(); }
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
                var items = GetSortedItems(e.Parameter as IList);
                var dialog = new System.Windows.Forms.SaveFileDialog();
                dialog.Filter = Properties.Resources.PdfFilter;
                dialog.OverwritePrompt = true;
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                _viewmodel.Extract(items, dialog.FileName);
            }
            catch (Exception err)
            {
                MessageBox.Show(Properties.Resources.SaveError, Properties.Resources.ErrorTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Trace.TraceError(err.ToString());
            }
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
            catch (Exception err)
            {
                MessageBox.Show(Properties.Resources.SaveError, Properties.Resources.ErrorTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Trace.TraceError(err.ToString());
            }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Move
        ///
        /// <summary>
        /// ページ順序を変更します。
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
            catch (Exception err) { Trace.TraceError(err.ToString()); }
            finally { _viewmodel.EndCommand(); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Rotate
        ///
        /// <summary>
        /// 現在、選択されているページを回転します。
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
                Cursor = Cursors.Wait;
                var degree = int.Parse(e.Parameter as string);
                var done = new List<int>();
                _viewmodel.BeginCommand();
                while (Thumbnail.SelectedItems.Count > 0)
                {
                    var obj = Thumbnail.SelectedItems[0];
                    var index = _viewmodel.IndexOf(obj);
                    _viewmodel.RotateAt(index, degree);
                    done.Add(index);
                    Thumbnail.SelectedItems.Remove(obj);
                }
                foreach (var index in done) Thumbnail.SelectedItems.Add(_viewmodel.Items[index]);
                _viewmodel.History[0].Text = (degree < 0) ? RotateLeftButton.Label : RotateRightButton.Label;
            }
            catch (Exception err) { Trace.TraceError(err.ToString()); }
            finally { _viewmodel.EndCommand(); }
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
                Cursor = Cursors.Wait;
                var count = (e.Parameter != null) ? HistoryGallery.Items.IndexOf(e.Parameter) + 1 : 1;
                for (var i = 0; i < count; ++i)
                {
                    var text = _viewmodel.History[0].Text;
                    _viewmodel.Undo();
                    _viewmodel.UndoHistory[0].Text = text;
                }
            }
            catch (Exception err) { Trace.TraceError(err.ToString()); }
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
                Cursor = Cursors.Wait;
                var count = (e.Parameter != null) ? UndoHistoryGallery.Items.IndexOf(e.Parameter) + 1 : 1;
                for (var i = 0; i < count; ++i)
                {
                    var text = _viewmodel.UndoHistory[0].Text;
                    _viewmodel.Redo();
                    _viewmodel.History[0].Text = text;
                }
            }
            catch (Exception err) { Trace.TraceError(err.ToString()); }
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
            catch (Exception err) { Trace.TraceError(err.ToString()); }
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
            var dialog = new MetadataWindow(_viewmodel, _font);
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
                if (ViewSizeGallery == null || ViewSizeGallery.SelectedItem == null) return;
                var item = (KeyValuePair<int, string>)ViewSizeGallery.SelectedItem;
                var index = _ViewSize.IndexOf(item);
                e.CanExecute = index < _ViewSize.Count - 1;
            }
            catch (Exception err) { Trace.TraceError(err.ToString()); }
        }

        private void ZoomInCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var item = (KeyValuePair<int, string>)ViewSizeGallery.SelectedItem;
                var index = _ViewSize.IndexOf(item);
                ViewSizeGallery.SelectedItem = _ViewSize[index + 1];
            }
            catch (Exception err) { Trace.TraceError(err.ToString()); }
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
                if (ViewSizeGallery == null || ViewSizeGallery.SelectedItem == null) return;
                var item = (KeyValuePair<int, string>)ViewSizeGallery.SelectedItem;
                var index = _ViewSize.IndexOf(item);
                e.CanExecute = index > 0;
            }
            catch (Exception err) { Trace.TraceError(err.ToString()); }
        }

        private void ZoomOutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var item = (KeyValuePair<int, string>)ViewSizeGallery.SelectedItem;
                var index = _ViewSize.IndexOf(item);
                ViewSizeGallery.SelectedItem = _ViewSize[index - 1];
            }
            catch (Exception err) { Trace.TraceError(err.ToString()); }
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
                if (_viewmodel.ItemWidth != width) _viewmodel.ItemWidth = width;
            }
            catch (Exception err) { Trace.TraceError(err.ToString()); }
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
            catch (Exception err) { Trace.TraceError(err.ToString()); }
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
            try { _viewmodel.Reset(); }
            catch (Exception err) { Trace.TraceError(err.ToString()); }
            finally
            {
                Cursor = Cursors.Wait;
                Thumbnail.Items.Refresh();
                Cursor = Cursors.Arrow;
            }
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
            var dialog = new EncryptionWindow(_viewmodel, _font);
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
            var dialog = new VersionWindow(_setting.Version);
            dialog.Owner = this;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Help
        ///
        /// <summary>
        /// ヘルプ（マニュアル）を表示します。
        /// パラメータ (e.Parameter) は常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Help

        private void HelpCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // TODO: 実装
            e.CanExecute = false;
        }

        private void HelpCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: 実装
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Web
        ///
        /// <summary>
        /// Web ページを表示します。
        /// パラメータ (e.Parameter) は表示する Web ページの URL です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Web

        private void WebCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void WebCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var url = e.Parameter as string;
            if (url == null) return;
            Process.Start(url);
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Password
        ///
        /// <summary>
        /// 暗号化を解除するためのパスワードダイアログを表示します。
        /// パラメータ (e.Parameter) は常に null です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Password

        private void PasswordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _viewmodel.EncryptionStatus == CubePdf.Data.EncryptionStatus.RestrictedAccess;
        }

        private void PasswordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var path = _viewmodel.FilePath;
            if (String.IsNullOrEmpty(path)) return;

            var dialog = new PasswordWindow(path, _font);
            dialog.Owner = this;
            if (dialog.ShowDialog() == true && CloseFile()) OpenFileAsync(path, dialog.Password);
        }

        #endregion

        #endregion

        #region Event handlers

        /* ----------------------------------------------------------------- */
        ///
        /// OnClosing
        /// 
        /// <summary>
        /// アプリケーションの終了直前に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var result = CloseFile();
            e.Cancel = !result;
            if (!e.Cancel) SaveSetting(this, e);
            base.OnClosing(e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnClosed
        /// 
        /// <summary>
        /// アプリケーションの終了時に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _viewmodel.Dispose();
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
                    OpenFileAsync(file, "");
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
        /// OnContentRendered
        /// 
        /// <summary>
        /// メイン画面が表示された後に実行されるイベントハンドラです。
        /// スプラッシュ画面の終了、およびアップデートの確認が行われます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (_shown) return;
            _shown = true;

            try
            {
                foreach (var ps in Process.GetProcessesByName("CubePdfUtilitySplash")) ps.Kill();
                
                if (string.IsNullOrEmpty(_setting.InstallDirectory) ||
                    DateTime.Now <= _setting.LastCheckUpdate.AddDays(1)) return;
                var path = System.IO.Path.Combine(_setting.InstallDirectory, "UpdateChecker.exe");
                Process.Start(path);
            }
            catch (Exception err) { Trace.TraceError(err.ToString()); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ViewModel_RunCompleted
        /// 
        /// <summary>
        /// ViewModel の各種処理が終了した際に実行されるイベントハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void ViewModel_RunCompleted(object sender, EventArgs e)
        {
            Refresh();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Thumbnail_ScrollChanged
        /// 
        /// <summary>
        /// サムネイルを表示しているコントロールがスクロールされた時に
        /// 実行されるイベントハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Thumbnail_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _viewmodel.Refresh();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// NavigationCanvas_MenuItemClick
        /// 
        /// <summary>
        /// ナビゲーション画面に表示されている各メニュー項目がクリックされた
        /// 時に実行されるイベントハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void NavigationCanvas_MenuItemClick(object sender, RoutedEventArgs e)
        {
            var control = sender as MenuItem;
            if (control != null && control.Tag != null)
            {
                OpenFileAsync(control.Tag as string, "");
            }
        }

        #endregion

        #region Private methods for UserSetting

        /* ----------------------------------------------------------------- */
        ///
        /// LoadSetting
        /// 
        /// <summary>
        /// ユーザ設定をメイン画面に適用します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void LoadSetting(object sender, EventArgs e)
        {
            _setting.Load();

            if (_setting.IsMaximized) WindowState = WindowState.Maximized;
            else
            {
                Width  = Math.Max(_setting.Size.Width, _MinSize);
                Height = Math.Max(_setting.Size.Height, _MinSize);
                Left   = Math.Max(Math.Min(_setting.Position.X, SystemParameters.WorkArea.Right - Width), 8);
                Top    = Math.Max(Math.Min(_setting.Position.Y, SystemParameters.WorkArea.Bottom - Height), 0);
            }

            // NOTE: ItemWidth は、既に用意されている選択肢 (_ViewSize) のうち、
            // ユーザ設定に保存されている値を超えない最大値を使用する。
            ViewSizeGalleryCategory.ItemsSource = _ViewSize;
            var size = _ViewSize[0];
            foreach (var item in _ViewSize)
            {
                if (item.Key > _setting.ItemWidth) break;
                size = item;
            }
            ViewSizeGallery.SelectedItem = size;

            _viewmodel.ItemVisibility = _setting.ItemVisibility;
            ViewModeCheckBox.IsChecked = (_setting.ItemVisibility == CubePdf.Wpf.ListViewItemVisibility.Minimum);

            UpdateRecentFiles();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SaveSetting
        /// 
        /// <summary>
        /// メイン画面の現在の状態をユーザ設定に保存します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SaveSetting(object sender, EventArgs e)
        {
            _setting.Position = new Point(Left, Top);
            _setting.Size = new Size((int)Width, (int)Height);
            _setting.IsMaximized = (WindowState == WindowState.Maximized);
            _setting.ItemWidth = _viewmodel.ItemWidth;
            _setting.ItemVisibility = _viewmodel.ItemVisibility;
            _setting.Save();
        }

        #endregion

        #region Private methods for open, insert, and close operations

        /* ----------------------------------------------------------------- */
        ///
        /// OpenFile
        /// 
        /// <summary>
        /// 指定された IDocumentReader オブジェクトを用いて GUI 上に該当
        /// ファイルの内容を表示します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void OpenFile(CubePdf.Data.IDocumentReader reader)
        {
            try {
                _viewmodel.Open(reader);
                RecentFile.Add(reader.FilePath);
                UpdateRecentFiles();
            }
            catch (Exception err)
            {
                MessageBox.Show(Properties.Resources.OpenError, Properties.Resources.ErrorTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Trace.TraceError(err.ToString());
                Refresh();
            }
            finally { reader.Dispose(); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OpenFileAsync
        /// 
        /// <summary>
        /// 指定されたパスの PDF ファイルを非同期で開きます。
        /// </summary>
        /// 
        /// <remarks>
        /// パスワードが設定されている場合は、パスワードを入力するための
        /// ダイアログを表示してユーザに入力してもらいます。入力された
        /// パスワードが間違っていた場合は、正しいパスワードが入力されるか、
        /// またはキャンセルボタンが押下されるまでダイアログを表示し続けます。
        /// 
        /// ナビゲーション用の画面に関しては、非同期処理中にメニュー項目が
        /// クリックされたと誤判断される事があるため、非同期処理に移る前に
        /// 非表示に設定しています。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private void OpenFileAsync(string path, string password)
        {
            Cursor = Cursors.Wait;
            var filename = System.IO.Path.GetFileName(path);
            var message = String.Format(Properties.Resources.OpenFile, filename);
            InfoStatusBarItem.Content = message;

            NavigationCanvas.Visibility = Visibility.Collapsed;
            ThreadPool.QueueUserWorkItem(new WaitCallback((Object parameter) => {
                var reader = new CubePdf.Editing.DocumentReader();
                try
                {
                    reader.Open(path, password);
                    if (NeedPassword(reader)) throw new CubePdf.Data.EncryptionException();
                    else Dispatcher.BeginInvoke(new Action(() => {
                        OpenFile(reader);
                        if (reader.IsTaggedDocument)
                        {
                            MessageBox.Show(Properties.Resources.TaggedPdf, Properties.Resources.WarningTitle,
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }));
                }
                catch (CubePdf.Data.EncryptionException /* err */)
                {
                    Dispatcher.BeginInvoke(new Action(() => {
                        var dialog = new PasswordWindow(path, _font);
                        dialog.Owner = this;
                        if (dialog.ShowDialog() == true) OpenFileAsync(path, dialog.Password);
                        else Refresh();
                    }));
                }
            }), null);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// InsertFile
        /// 
        /// <summary>
        /// 指定された IDocumentReader オブジェクトを用いて GUI 上に該当
        /// ファイルの内容を追加表示します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void InsertFile(int index, CubePdf.Data.IDocumentReader reader, string history)
        {
            try
            {
                _viewmodel.Insert(index, reader);
                _viewmodel.History[0].Text = history;
            }
            catch (ArgumentException err)
            {
                MessageBox.Show(err.Message, Properties.Resources.ErrorTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Trace.TraceError(err.ToString());
                Refresh();
            }
            catch (Exception err)
            {
                MessageBox.Show(Properties.Resources.InsertError, Properties.Resources.ErrorTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Trace.TraceError(err.ToString());
                Refresh();
            }
            finally { reader.Dispose(); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// InsertFileAsync
        /// 
        /// <summary>
        /// index の位置に指定された PDF ファイルを挿入します。
        /// </summary>
        /// 
        /// <remarks>
        /// パスワードが設定されている場合は、パスワードを入力するための
        /// ダイアログを表示してユーザに入力してもらいます。入力された
        /// パスワードが間違っていた場合は、正しいパスワードが入力されるか、
        /// またはキャンセルボタンが押下されるまでダイアログを表示し続けます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private void InsertFileAsync(int index, string path, string password, string history)
        {
            Cursor = Cursors.Wait;
            var filename = System.IO.Path.GetFileName(path);
            var message = String.Format(Properties.Resources.InsertFile, filename);
            InfoStatusBarItem.Content = message;

            ThreadPool.QueueUserWorkItem(new WaitCallback((Object parameter) => {
                var reader = new CubePdf.Editing.DocumentReader();
                try
                {
                    reader.Open(path, password);
                    if (NeedPassword(reader)) throw new CubePdf.Data.EncryptionException();
                    Dispatcher.BeginInvoke(new Action(() => {
                        InsertFile(index, reader, history);
                    }));
                }
                catch (CubePdf.Data.EncryptionException /* err */)
                {
                    Dispatcher.BeginInvoke(new Action(() => {
                        var dialog = new PasswordWindow(path, _font);
                        dialog.Owner = this;
                        if (dialog.ShowDialog() == true) InsertFileAsync(index, path, dialog.Password, history);
                    }));
                }
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
                if (result == MessageBoxResult.Yes)
                {
                    try { _viewmodel.SaveOnClose(); }
                    catch (Exception err)
                    {
                        MessageBox.Show(Properties.Resources.SaveError, Properties.Resources.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        Trace.TraceError(err.ToString());
                        return false;
                    }
                }
            }
            _viewmodel.Close();

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Win32Api.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
            return true;
        }

        #endregion

        #region Private methods for others

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

                NavigationCanvas.Visibility = Visibility.Collapsed;
                InfoStatusBarItem.Content = String.Format("{0} ページ", _viewmodel.PageCount);
                LockStatusBarItem.Visibility = restricted ? Visibility.Visible : Visibility.Collapsed;
                Thumbnail.Focus();
            }
            else
            {
                Title = ProductName;
                NavigationCanvas.Visibility = Visibility.Visible;
                if (InfoStatusBarItem != null) InfoStatusBarItem.Content = string.Empty;
                if (LockStatusBarItem != null) LockStatusBarItem.Visibility = Visibility.Collapsed;
            }
            Cursor = Cursors.Arrow;
            RecentFilesGallery.SelectedItem = null;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// UpdateRecentFiles
        /// 
        /// <summary>
        /// システムの「最近開いたファイル」から情報を取得して、最新の状態に
        /// 更新します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void UpdateRecentFiles()
        {
            var recents = RecentFile.Find("*.pdf");
            
            RecentFilesGalleryCategory.Items.Clear();
            for (int i = 0; i < recents.Length; ++i)
            {
                var gallery = new RibbonGalleryItem();
                gallery.Content = String.Format("{0} {1}", i + 1, System.IO.Path.GetFileName(recents[i]));
                gallery.Tag = recents[i];
                RecentFilesGalleryCategory.Items.Add(gallery);
            }

            NavigationCanvas.Clear();
            NavigationCanvas.AddFiles(recents);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// NeedPassword
        /// 
        /// <summary>
        /// パスワードが必要かどうかを判断します。
        /// </summary>
        /// 
        /// <remarks>
        /// AES256 の場合は、オーナーパスワード無しでは表示できないため
        /// （PDFLibNet の関係）パスワードを要求するようにしています。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private bool NeedPassword(CubePdf.Editing.DocumentReader reader)
        {
            return reader.EncryptionStatus == CubePdf.Data.EncryptionStatus.RestrictedAccess &&
                   reader.Encryption.Method == CubePdf.Data.EncryptionMethod.Aes256;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ReplaceFont
        ///
        /// <summary>
        /// コンストラクタ実行時に、画面のフォントを差し替えます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void ReplaceFont()
        {
            if (FontFamily.Source == "メイリオ" || FontFamily.Source.Contains("Meiryo")) return;

            var fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (var ff in fonts.Families)
            {
                if (ff.Name == "メイリオ" || ff.Name.Contains("Meiryo"))
                {
                    TextElement.FontFamilyProperty.OverrideMetadata(typeof(TextElement), new FrameworkPropertyMetadata(new FontFamily(ff.Name)));
                    TextBlock.FontFamilyProperty.OverrideMetadata(typeof(TextBlock), new FrameworkPropertyMetadata(new FontFamily(ff.Name)));
                    MainRibbon.FontFamily = new FontFamily(ff.Name);
                    Thumbnail.ContextMenu.FontFamily = new FontFamily(ff.Name);
                    FooterStatusBar.FontFamily = new FontFamily(ff.Name);
                    NavigationCanvas.FontFamily = new FontFamily(ff.Name);
                    _font = ff.Name;
                    break;
                }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetSortedItems
        /// 
        /// <summary>
        /// 引数に指定されたリストを ListViewModel.Items で格納されている
        /// 順番にソートして返します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private IList GetSortedItems(IList src)
        {
            if (src == null) return _viewmodel.Items;

            var dest = new List<CubePdf.Drawing.ImageContainer>();
            foreach (var item in _viewmodel.Items)
            {
                if (src.Contains(item)) dest.Add(item);
            }
            return dest;
        }


        /* ----------------------------------------------------------------- */
        ///
        /// ChangeLogoVisibility
        ///
        /// <summary>
        /// メイン画面の幅によって、ロゴを表示するかどうかを切り替えます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void ChangeLogoVisibility(object sender, EventArgs e)
        {
            // NOTE: これらの値は、該当タブの項目を増やす（減らす）際に調整する必要がある。
            var edit_tab_width = 900;
            var view_tab_width = 410;
            var help_tab_width = 310;

            var limit = EditTab.IsSelected ? edit_tab_width : (ViewTab.IsSelected ? view_tab_width : help_tab_width);
            LogoImage.Visibility = (Width < limit) ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #region Variables
        private UserSetting _setting = new UserSetting();
        private string _font = string.Empty;
        private CubePdf.Wpf.ListViewModel _viewmodel = new CubePdf.Wpf.ListViewModel();
        private bool _shown = false;
        #endregion

        #region Static variables
        private static readonly IList<KeyValuePair<int, string>> _ViewSize;
        private static readonly int _MinSize = 400;
        #endregion

        #region Win32 APIs

        internal class Win32Api
        {
            [DllImport("kernel32.dll")]
            public static extern bool SetProcessWorkingSetSize(IntPtr procHandle, int min, int max);
        }

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
        public static readonly ICommand ZoomIn   = new RoutedCommand("ZoomIn",   typeof(MainWindow));
        public static readonly ICommand ZoomOut  = new RoutedCommand("ZoomOut",  typeof(MainWindow));
        public static readonly ICommand ViewSize = new RoutedCommand("ViewSize", typeof(MainWindow));
        public static readonly ICommand ViewMode = new RoutedCommand("ViewMode", typeof(MainWindow));
        public static readonly ICommand Redraw   = new RoutedCommand("Redraw",   typeof(MainWindow));
        public static readonly ICommand Version  = new RoutedCommand("Version",  typeof(MainWindow));
        public static readonly ICommand Help     = new RoutedCommand("Help",     typeof(MainWindow));
        public static readonly ICommand Web      = new RoutedCommand("Web",      typeof(MainWindow));
        public static readonly ICommand Password = new RoutedCommand("Password", typeof(MainWindow));
        #endregion
    }
}
