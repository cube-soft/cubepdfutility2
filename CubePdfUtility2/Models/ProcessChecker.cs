/* ------------------------------------------------------------------------- */
///
/// ProcessChecker.cs
///
/// Copyright (c) 2014 CubeSoft, Inc. All rights reserved.
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
using System.Collections.Generic;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// ProcessChecker
    /// 
    /// <summary>
    /// プロセスとファイルの管理を行うためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ProcessChecker
    {
        #region Initialization and Termination

        /* ----------------------------------------------------------------- */
        ///
        /// ProcessChecker (constructor)
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="root">
        /// 各種情報をプロセス間で共有するためのファイルを保存するフォルダ
        /// へのパス。
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        public ProcessChecker(string root)
        {
            _root = System.IO.Path.Combine(root, _DataFolder);
            if (!System.IO.Directory.Exists(_root)) System.IO.Directory.CreateDirectory(_root);
        }

        #endregion

        #region Public methods

        /* ----------------------------------------------------------------- */
        ///
        /// Register
        /// 
        /// <summary>
        /// PDF ファイルを開かれているプロセスを登録します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Register(string path, Process process)
        {
            var datapath = CreateDataPath(path);
            using (var writer = new System.IO.StreamWriter(datapath)) writer.WriteLine(process.Id);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        /// 
        /// <summary>
        /// PDF ファイルを一覧から削除します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Remove(string path)
        {
            var datapath = CreateDataPath(path);
            if (System.IO.File.Exists(datapath)) System.IO.File.Delete(datapath);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetProcess
        /// 
        /// <summary>
        /// 引数にしていされた PDF ファイルを開いているプロセスを取得します。
        /// 開いているプロセスが存在しない場合は、null が返ります。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Process GetProcess(string path)
        {
            var datapath = CreateDataPath(path);
            if (!System.IO.File.Exists(datapath)) return null;

            var pid = -1;
            using (var reader = new System.IO.StreamReader(datapath)) pid = int.Parse(reader.ReadLine());

            try { return Process.GetProcessById(pid); }
            catch (ArgumentException /* err */)
            {
                System.IO.File.Delete(datapath);
                return null;
            }
        }

        #endregion

        #region Other methods

        /* ----------------------------------------------------------------- */
        ///
        /// CreateDataPath
        /// 
        /// <summary>
        /// ファイル名からデータを保存するパスを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private string CreateDataPath(string path)
        {
            var hash = BitConverter.ToString(CreateHash(path)).ToLower().Replace("-", "");
            return System.IO.Path.Combine(_root, hash);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CreateHash
        /// 
        /// <summary>
        /// ファイル名からハッシュ値を生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private byte[] CreateHash(string path)
        {
            var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            var dest = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(path));
            sha1.Clear();
            return dest;
        }

        #endregion

        #region Variables
        private string _root = string.Empty;
        #endregion

        #region Static variables
        private static readonly string _DataFolder = "Processes";
        #endregion
    }
}
