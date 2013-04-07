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

        #region Save

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = _viewmodel.ItemCount > 0;
            e.CanExecute = false;
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
                var index = (e.Parameter != null) ? (int)e.Parameter + 1 : 0;
                if (index > _viewmodel.ItemCount) index = _viewmodel.ItemCount;
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

        #region Add

        private void AddCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool enabled = _viewmodel.ItemCount > 0;
            this.InsertButton.IsEnabled = enabled;
            e.CanExecute = enabled;
        }

        private void AddCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: Parameter に ItemCount - 1 を指定する方法
            // e.Parameter = _viewmodel.ItemCount - 1;
            InsertCommand_Executed(sender, e);
        }

        #endregion

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
                    var index = _viewmodel.Items.IndexOf(item as ImageSource);
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
                    var index = _viewmodel.Items.IndexOf(Thumbnail.SelectedItems[0] as ImageSource);
                    _viewmodel.RotateAt(index, degree);
                    done.Add(_viewmodel.Items[index]);
                }
                foreach (var obj in done) Thumbnail.SelectedItems.Add(obj);
            }
            catch (Exception err) { Debug.WriteLine(err); }
            finally { Refresh(); }
        }

        #endregion

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

        #region Other Methods

        /* ----------------------------------------------------------------- */
        /// Refresh
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

        #region ICommand variables
        public static readonly ICommand Select     = new RoutedCommand("Select",     typeof(MainWindow));
        public static readonly ICommand SelectAll  = new RoutedCommand("SelectAll",  typeof(MainWindow));
        public static readonly ICommand UnSelect   = new RoutedCommand("UnSelect",   typeof(MainWindow));
        public static readonly ICommand Metadata   = new RoutedCommand("Metadata",   typeof(MainWindow));
        public static readonly ICommand Encryption = new RoutedCommand("Encryption", typeof(MainWindow));
        #endregion
    }
}
