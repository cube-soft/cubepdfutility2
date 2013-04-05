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
    /// RemoveWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class RemoveWindow : Window
    {
        public RemoveWindow()
        {
            InitializeComponent();
        }

        #region Commands

        #region Close

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #endregion
    }
}
