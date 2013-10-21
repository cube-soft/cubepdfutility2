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
            e.CanExecute = true;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
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
            // TODO: implementation
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
                e.CanExecute = (0 <= index && index < FileListView.Items.Count);
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
            e.CanExecute = (FileListView != null && FileListView.Items.Count > 0);
        }

        private void ClearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: implementation
        }

        #endregion

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
