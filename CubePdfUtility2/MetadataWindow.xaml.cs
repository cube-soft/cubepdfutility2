using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CubePdfUtility
{
    /// <summary>
    /// MetadataWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MetadataWindow : Window
    {
        #region Initializing and Terminating

        public MetadataWindow()
        {
            InitializeComponent();
        }

        public MetadataWindow(CubePdf.Wpf.IListViewModel viewmodel)
            : this()
        {
            _metadata = new CubePdf.Data.Metadata(viewmodel.Metadata);
            DataContext = _metadata;
            var sizestr = CubePdf.Data.StringConverter.FormatByteSize(viewmodel.FileSize);

            // 読み取り専用の情報
            FileName.Text        = System.IO.Path.GetFileName(viewmodel.FilePath);
            FileSize.Content     = String.Format("{0} ({1:N0} バイト)", sizestr, viewmodel.FileSize);
            CreationTime.Content = viewmodel.CreationTime.ToString();
            UpdateTime.Content   = viewmodel.UpdateTime.ToString();

            // TODO: Version.Minor の値を SelectedIndex にバインディングしたい
            PdfVersion.SelectedIndex = _metadata.Version.Minor;
        }

        #endregion

        #region Properties

        public CubePdf.Data.Metadata Metadata
        {
            get { return _metadata; }
        }

        #endregion

        #region Commands

        #region Save

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_metadata.Version.Minor != PdfVersion.SelectedIndex)
            {
                _metadata.Version = new Version(1, PdfVersion.SelectedIndex, 0, 0);
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
        #endregion
    }
}
