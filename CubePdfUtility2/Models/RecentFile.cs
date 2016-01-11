/* ------------------------------------------------------------------------- */
///
/// RecentFile.cs
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
using System.Linq;
using IWshRuntimeLibrary;

namespace CubePdfUtility
{
    /* --------------------------------------------------------------------- */
    ///
    /// RecentFile
    /// 
    /// <summary>
    /// 「最近使用したファイル」に関する処理を行うためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public abstract class RecentFile
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Add
        /// 
        /// <summary>
        /// 引数に指定したパスを「最近使用したファイル」の一覧に追加します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static void Add(string path)
        {
            IWshShortcut shortcut = null;

            try
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
                var link = System.IO.Path.Combine(dir, System.IO.Path.GetFileName(path) + ".lnk");
                var shell = new WshShell();
                shortcut = shell.CreateShortcut(link) as IWshShortcut;
                if (shortcut == null) return;

                shortcut.TargetPath = path;
                shortcut.WindowStyle = 1;
                shortcut.Save();
            }
            catch (Exception err) { System.Diagnostics.Trace.TraceError(err.ToString()); }
            finally
            {
                if (shortcut != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(shortcut);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        /// 
        /// <summary>
        /// パターンにマッチするファイルを「最近使用したファイル」の一覧
        /// から削除します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static void Remove(string pattern)
        {
            foreach (var link in FindLink(pattern)) System.IO.File.Delete(link);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Exists
        /// 
        /// <summary>
        /// パターンにマッチするファイルが「最近使用したファイル」の一覧に
        /// 存在するかどうかを判別します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static bool Exists(string pattern)
        {
            var links = FindLink(pattern);
            return links != null && links.Length > 0;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Find
        /// 
        /// <summary>
        /// 「最近使用したファイル」の一覧からパターンにマッチする全ての
        /// ファイルを取得します。
        /// </summary>
        /// 
        /// <remarks>
        /// 取得されるパスは、リンク先の最終的なファイルへのパスです。
        /// 「最近開いたファイル」のうち、既に存在しないファイルは結果に
        /// 含まれません。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string[] Find(string pattern)
        {
            var dest  = new System.Collections.Generic.List<string>();
            var shell = new IWshShell_Class();

            foreach (var link in FindLink(pattern))
            {
                var shortcut = shell.CreateShortcut(link) as IWshShortcut_Class;
                if (shortcut == null || !System.IO.File.Exists(shortcut.TargetPath)) continue;
                dest.Add(shortcut.TargetPath);
            }

            return dest.ToArray();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// FindLink
        /// 
        /// <summary>
        /// 「最近使用したファイル」の一覧からパターンにマッチする全ての
        /// ファイルへのリンクを最終アクセス時刻順に取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static string[] FindLink(string pattern)
        {
            var dir = Environment.GetFolderPath(System.Environment.SpecialFolder.Recent);
            var files = System.IO.Directory.GetFiles(dir + "\\", pattern + ".lnk");

            var list = new List<KeyValuePair<DateTime, string>>();
            foreach (var path in files) list.Add(new KeyValuePair<DateTime, string>(System.IO.File.GetLastAccessTime(path), path));
            list.Sort((a, b) => { return DateTime.Compare(a.Key, b.Key); });
            var dest = new List<string>();
            foreach (var element in list) dest.Add(element.Value);
            dest.Reverse();
            return dest.ToArray();
        }
    }
}
