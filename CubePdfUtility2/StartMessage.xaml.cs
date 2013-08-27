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
using Microsoft.Windows.Controls.Ribbon;

namespace CubePdfUtility
{
    /// <summary>
    /// Interaction logic for StartMessage.xaml
    /// </summary>
    public partial class StartMessage : Canvas
    {
        public StartMessage()
        {
            InitializeComponent();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AddRecentFiles
        ///
        /// <summary>
        /// StartMessageに最近開いたファイルを追加します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void AddRecentFiles(IList<string> recents)
        {
            for (int i = 0; i < recents.Count; ++i)
            {
                var gallery = new RibbonGalleryItem();
                gallery.Content = String.Format("{0} {1}", i + 1, System.IO.Path.GetFileName(recents[i]));
                gallery.Tag = recents[i];
                RecentFilesGallery.Items.Add(gallery);
            }
        }
    }
}
