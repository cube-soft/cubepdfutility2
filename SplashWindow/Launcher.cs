/* ------------------------------------------------------------------------- */
///
/// Launcher.cs
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
using System.Collections.Generic;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// Launcher
    /// 
    /// <summary>
    /// CubePDF Utility を起動させるためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class Launcher
    {
        #region Initialization and Termination

        /* ----------------------------------------------------------------- */
        ///
        /// Launcher (constructor)
        /// 
        /// <summary>
        /// 既定の値でオブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Launcher()
        {
            try
            {
                var registry = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(_RegRoot, false);
                _install = registry.GetValue(_RegPath, string.Empty) as string;
                _version = registry.GetValue(_RegVersion, string.Empty) as string;
            }
            catch (Exception err) { Trace.WriteLine(err.ToString()); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Launcher (constructor)
        /// 
        /// <summary>
        /// 引数に指定されたコマンド引数の配列を用いて、オブジェクトを
        /// 初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Launcher(string[] args)
            : this()
        {
            foreach (var s in args) _arguments.Add(s);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Arguments
        /// 
        /// <summary>
        /// CubePDF Utility に渡すコマンドライン引数の一覧を取得、または
        /// 設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IList<string> Arguments
        {
            get { return _arguments; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// InstallDirectory
        /// 
        /// <summary>
        /// CubePDF Utility がインストールされているディレクトリへのパスを
        /// 取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string InstallDirectory
        {
            get { return _install; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Version
        /// 
        /// <summary>
        /// CubePDF Utility のバージョンを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Version
        {
            get { return _version; }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Run
        /// 
        /// <summary>
        /// CubePDF Utility を起動させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Run()
        {
            try
            {
                if (string.IsNullOrEmpty(_install)) return false;

                var args = new System.Text.StringBuilder();
                foreach (var s in _arguments) args.AppendFormat("\"{0}\" ", s);

                var process = new Process();
                process.StartInfo.FileName = System.IO.Path.Combine(_install, "CubePdfUtility.exe");
                process.StartInfo.Arguments = args.ToString();
                return process.Start();
            }
            catch (Exception /* err */) { return false; }
        }

        #region Variables
        private List<string> _arguments = new List<string>();
        private string _install = string.Empty;
        private string _version = string.Empty;
        #endregion

        #region Constant variables
        private static readonly string _RegRoot = @"Software\CubeSoft\CubePDF Utility2";
        private static readonly string _RegPath = "InstallDirectory";
        private static readonly string _RegVersion = "Version";
        #endregion
    }
}
