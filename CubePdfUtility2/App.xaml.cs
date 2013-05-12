/* ------------------------------------------------------------------------- */
///
/// App.xaml.cs
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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// App
    /// 
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public partial class App : Application
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Application_Startup
        /// 
        /// <summary>
        /// アプリケーション起動時に実行されるイベントハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var shown = false;
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (System.IO.Path.GetExtension(e.Args[i]) != ".pdf" || !System.IO.File.Exists(e.Args[i])) continue;
                var window = new MainWindow(e.Args[i]);
                window.Show();
                shown = true;
                break;
            }

            if (!shown)
            {
                var window = new MainWindow();
                window.Show();
            }
        }
    }
}
