/* ------------------------------------------------------------------------- */
///
/// MetadataWindow.xaml.cs
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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// MetadataWindow
    /// 
    /// <summary>
    /// MetadataWindow.xaml の相互作用ロジック
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public partial class MetadataWindow : Window
    {
        #region Initializing and Terminating

        /* ----------------------------------------------------------------- */
        /// MetadataWindow (constructor)
        /* ----------------------------------------------------------------- */
        public MetadataWindow()
        {
            InitializeComponent();
            SourceInitialized += (sender, e) => {
                if (Top < 0 || Top > SystemParameters.WorkArea.Bottom - Height) Top = 0;
                if (Left < 0 || Left > SystemParameters.WorkArea.Right - Width) Left = 0;
            };
        }

        /* ----------------------------------------------------------------- */
        /// MetadataWindow (constructor)
        /* ----------------------------------------------------------------- */
        public MetadataWindow(CubePdf.Wpf.ListViewModel viewmodel)
            : this()
        {
            _metadata = new CubePdf.Data.Metadata(viewmodel.Metadata);
            _restricted = viewmodel.EncryptionStatus == CubePdf.Data.EncryptionStatus.RestrictedAccess;
            DataContext = _metadata;

            var info = new System.IO.FileInfo(viewmodel.FilePath);
            var sizestr = CubePdf.Misc.StringConverter.FormatByteSize(info.Length);

            // 読み取り専用の情報
            FileName.Text        = System.IO.Path.GetFileName(viewmodel.FilePath);
            FileSize.Content     = String.Format("{0} ({1:N0} バイト)", sizestr, info.Length);
            CreationTime.Content = info.CreationTime.ToString();
            UpdateTime.Content   = info.LastWriteTime.ToString();

            // Version.Minor は読み取り専用なので Binding ではなくコード側で対応
            PdfVersion.SelectedIndex = _metadata.Version.Minor;

            // ViewerPreferences の SelectedIndex は単純には捌けなさそうなのでコード側で対応
            ViewerPreferences.SelectedIndex = -1;
            for (int i = 0; i <= 6; ++i)
            {
                if ((_metadata.ViewerPreferences & (int)Math.Pow(2, i)) != 0)
                {
                    ViewerPreferences.SelectedIndex = i;
                }
            }
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Metadata
        /// 
        /// <summary>
        /// 設定した文書プロパティの情報を取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public CubePdf.Data.Metadata Metadata
        {
            get { return _metadata; }
        }

        #endregion

        #region Commands

        #region Save

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !_restricted;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_metadata.Version.Minor != PdfVersion.SelectedIndex)
            {
                _metadata.Version = new Version(1, PdfVersion.SelectedIndex, 0, 0);
            }

            if ((_metadata.ViewerPreferences & (int)Math.Pow(2, ViewerPreferences.SelectedIndex)) == 0)
            {
                _metadata.ViewerPreferences &= ~((int)Math.Pow(2, 6) - 1);
                _metadata.ViewerPreferences |= (int)Math.Pow(2, ViewerPreferences.SelectedIndex);
            }

            DialogResult = true;
            Close();
        }

        #endregion

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

        #endregion

        #region Variables
        private CubePdf.Data.Metadata _metadata = null;
        private bool _restricted = false;
        #endregion
    }
}
