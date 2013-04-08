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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// MainWindow (constructor)
        /* ----------------------------------------------------------------- */
        public MainWindow()
        {
            InitializeComponent();

            // Insert code required on object creation below this point.
            _viewmodel.ItemWidth = (int)ThumbnailImageView.ItemWidth;
            Thumbnail.DataContext = _viewmodel.Items;
        }

        #endregion

        #region Commands

        /* ----------------------------------------------------------------- */
        ///
        /// Open
        ///
        /// <summary>
        /// PDF ファイルを開きます。
        /// パラメータ (e.Parameter) は常に null です。ファイル名は、ファイル
        /// を開くためのダイアログから指定します。
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
                var dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = Properties.Resources.PdfFilter;
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                _viewmodel.Open(dialog.FileName);
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
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
                _viewmodel.Close();
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally
            {
                Refresh();
                var name = e.Parameter as string;
                if (name != null && name.IndexOf("Exit") >= 0) Application.Current.Shutdown();
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
            if (SaveButton != null) SaveButton.IsEnabled = _viewmodel.ItemCount > 0;
            e.CanExecute = _viewmodel.ItemCount > 0;
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
            e.CanExecute = _viewmodel.ItemCount > 0;
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
            this.InsertButton.IsEnabled = _viewmodel.ItemCount > 0;
            e.CanExecute = (e.Parameter == null || (int)e.Parameter >= 0);
        }

        private void InsertCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var index = (e.Parameter != null) ? Math.Min((int)e.Parameter + 1, _viewmodel.ItemCount) : 0;
                var dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = Properties.Resources.PdfFilter;
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                _viewmodel.Insert(index, dialog.FileName);
            }
            catch (CubePdf.Wpf.MultipleLoadException err) { MessageBox.Show(err.Message); }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
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
            this.RemoveButton.IsEnabled = _viewmodel.ItemCount > 0;
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
                while (items.Count > 0)
                {
                    var index = items.Count - 1;
                    _viewmodel.Remove(items[index]);
                    items.RemoveAt(index);
                }
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
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
            this.ExtractButton.IsEnabled = _viewmodel.ItemCount > 0;
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
                    var index = _viewmodel.Items.IndexOf(item as System.Drawing.Image);
                    if (delta < 0) indices.Add(index);
                    else indices.Insert(0, index);
                }

                foreach (var oldindex in indices)
                {
                    if (oldindex < 0) continue;
                    var newindex = oldindex + delta;
                    if (newindex < 0 || newindex >= _viewmodel.ItemCount) continue;
                    _viewmodel.Move(oldindex, newindex);
                }
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
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
                while (this.Thumbnail.SelectedItems.Count > 0)
                {
                    var index = _viewmodel.Items.IndexOf(Thumbnail.SelectedItems[0] as System.Drawing.Image);
                    _viewmodel.RotateAt(index, degree);
                    done.Add(_viewmodel.Items[index]);
                }
                foreach (var obj in done) Thumbnail.SelectedItems.Add(obj);
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Undo
        ///
        /// <summary>
        /// 未実装
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Undo

        private void UndoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // TODO: implementation
            e.CanExecute = false;
        }

        private void UndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: implementation
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Redo
        ///
        /// <summary>
        /// 未実装
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Redo

        private void RedoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // TODO: implementation
            e.CanExecute = false;
        }

        private void RedoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: implementation
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
            bool enabled = _viewmodel.ItemCount > 0;
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
            e.CanExecute = _viewmodel.ItemCount > 0;
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
            e.CanExecute = _viewmodel.ItemCount > 0;
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
            e.CanExecute = _viewmodel.ItemCount > 0;
        }

        private void EncryptionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new EncryptionWindow(_viewmodel);
            dialog.Owner = this;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            if (dialog.ShowDialog() == true) _viewmodel.Encryption = dialog.Encryption;
        }

        #endregion

        #endregion

        #region Event handlers

        /* ----------------------------------------------------------------- */
        ///
        /// Previewing
        /// 
        /// <summary>
        /// プレビュー画面を開きます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Previewing(object sender, EventArgs e)
        {
            if (Thumbnail == null || Thumbnail.SelectedIndex == -1) return;
            var dialog = new PreviewWindow(_viewmodel, Thumbnail.SelectedIndex);
            dialog.ShowDialog();
        }

        #endregion

        #region Other Methods

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
            if (_viewmodel != null && _viewmodel.ItemCount > 0)
            {
                this.Title = String.Format("{0} - CubePDF Utility", System.IO.Path.GetFileName(_viewmodel.FilePath));
                this.PageCountStatusBarItem.Content = String.Format("{0} ページ", _viewmodel.ItemCount);
                this.Thumbnail.Items.Refresh();
            }
            else
            {
                this.Title = "CubePDF Utility";
                this.PageCountStatusBarItem.Content = string.Empty;
            }
        }

        #endregion

        #region Variables
        private CubePdf.Wpf.IListViewModel _viewmodel = new CubePdf.Wpf.ListViewModel();
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
        #endregion
    }
}
