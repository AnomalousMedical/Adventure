﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZipAccess;
using System.IO;

namespace Engine.Resources
{
    class ZipArchive : Archive
    {
        private ZipFile zipFile = null;
        private static String[] splitPattern = { ".zip", ".dat" };
        private String fullZipPath;

        internal static bool CanOpenURL(String url)
        {
            return url.Contains(".zip") || url.Contains(".dat");
        }

        public ZipArchive(String filename)
        {
            String zipName = parseZipName(filename);
            fullZipPath = Path.GetFullPath(zipName);
            String subDir = parseURLInZip(filename);
            if (subDir == "")
            {
                zipFile = new ZipFile(zipName);
            }
            else
            {
                zipFile = new ZipFile(zipName, subDir);
            }
        }

        public override void Dispose()
        {
            if (zipFile != null)
            {
                zipFile.Dispose();
                zipFile = null;
            }
        }

        public override bool containsRealAbsolutePath(String path)
        {
            return FileSystem.fixPathFile(path).StartsWith(fullZipPath, StringComparison.OrdinalIgnoreCase);
        }

        public override IEnumerable<String> listFiles(bool recursive)
        {
            return enumerateNames(zipFile.listFiles("", recursive));
        }

        public override IEnumerable<String> listFiles(string url, bool recursive)
        {
            return enumerateNames(zipFile.listFiles(parseURLInZip(url), recursive));
        }

        public override IEnumerable<String> listFiles(String url, String searchPattern, bool recursive)
        {
            return enumerateNames(zipFile.listFiles(parseURLInZip(url), searchPattern, recursive));
        }

        public override IEnumerable<String> listDirectories(bool recursive)
        {
            return enumerateNames(zipFile.listDirectories("", recursive));
        }

        public override IEnumerable<String> listDirectories(String url, bool recursive)
        {
            return enumerateNames(zipFile.listDirectories(parseURLInZip(url), recursive));
        }

        public override IEnumerable<String> listDirectories(string url, bool recursive, bool includeHidden)
        {
            return listDirectories(url, recursive);
        }

        public override IEnumerable<String> listDirectories(String url, String searchPattern, bool recursive)
        {
            return enumerateNames(zipFile.listDirectories(parseURLInZip(url), searchPattern, recursive));
        }

        public override IEnumerable<String> listDirectories(string url, string searchPattern, bool recursive, bool includeHidden)
        {
            return listDirectories(url, searchPattern, recursive);
        }

        public override System.IO.Stream openStream(string url, FileMode mode)
        {
            return zipFile.openFile(parseURLInZip(url));
        }

        public override System.IO.Stream openStream(string url, FileMode mode, FileAccess access)
        {
            return zipFile.openFile(parseURLInZip(url));
        }

        public override bool isDirectory(string url)
        {
            ZipFileInfo info = zipFile.getFileInfo(parseURLInZip(url));
            return info != null && info.IsDirectory;
        }

        public override bool exists(String filename)
        {
            return zipFile.exists(parseURLInZip(filename));
        }

        public override VirtualFileInfo getFileInfo(String filename)
        {
            ZipFileInfo info = zipFile.getFileInfo(parseURLInZip(filename));
            return new VirtualFileInfo(info.Name, info.DirectoryName, info.FullName, getFullPath(info.FullName), info.CompressedSize, info.UncompressedSize);
        }

        private String parseURLInZip(String url)
        {
            String searchDirectory = url;
            if (url.Contains(".zip") || url.Contains(".dat"))
            {
                if (url.EndsWith(".zip") || url.EndsWith(".dat"))
                {
                    searchDirectory = "";
                }
                else
                {
                    String[] split = url.Split(splitPattern, StringSplitOptions.None);
                    if (split.Length == 1)
                    {
                        searchDirectory = "";
                    }
                    else
                    {
                        searchDirectory = split[1];
                    }
                }
            }
            return searchDirectory;
        }

        public static String parseZipName(String url)
        {
            String searchDirectory = url;
            //zip file
            if (url.Contains(".zip"))
            {
                if (url.EndsWith(".zip"))
                {
                    searchDirectory = url;
                }
                else
                {
                    String[] split = url.Split(splitPattern, StringSplitOptions.None);
                    if (split.Length > 0)
                    {
                        searchDirectory = split[0] + ".zip";
                    }
                }
            }
            //dat file
            else if (url.Contains(".dat"))
            {
                if (url.EndsWith(".dat"))
                {
                    searchDirectory = url;
                }
                else
                {
                    String[] split = url.Split(splitPattern, StringSplitOptions.None);
                    if (split.Length > 0)
                    {
                        searchDirectory = split[0] + ".dat";
                    }
                }
            }
            return searchDirectory;
        }

        private IEnumerable<String> enumerateNames(IEnumerable<ZipFileInfo> zipFiles)
        {
            foreach (ZipFileInfo info in zipFiles)
            {
                yield return info.FullName;
            }
        }

        private String getFullPath(String filename)
        {
            if (filename.Contains(".zip") || filename.Contains(".dat"))
            {
                return filename;
            }
            else
            {
                return fullZipPath + "/" + filename;
            }
        }
    }
}
