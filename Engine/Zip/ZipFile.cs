﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ZipAccess
{
    class ZipFile : IDisposable
    {
        const int ZZIP_ERROR = -4096;

        enum ZZipError
        {
            ZZIP_NO_ERROR = 0,	                /* no error, may be used if user sets it. */
            ZZIP_OUTOFMEM =      ZZIP_ERROR-20, /* out of memory */
            ZZIP_DIR_OPEN =      ZZIP_ERROR-21, /* failed to open zipfile, see errno for details */
            ZZIP_DIR_STAT =      ZZIP_ERROR-22, /* failed to fstat zipfile, see errno for details */
            ZZIP_DIR_SEEK =      ZZIP_ERROR-23, /* failed to lseek zipfile, see errno for details */
            ZZIP_DIR_READ =      ZZIP_ERROR-24, /* failed to read zipfile, see errno for details */
            ZZIP_DIR_TOO_SHORT = ZZIP_ERROR-25,
            ZZIP_DIR_EDH_MISSING = ZZIP_ERROR-26,
            ZZIP_DIRSIZE =       ZZIP_ERROR-27,
            ZZIP_ENOENT =        ZZIP_ERROR-28,
            ZZIP_UNSUPP_COMPR =  ZZIP_ERROR-29,
            ZZIP_CORRUPTED =     ZZIP_ERROR-31,
            ZZIP_UNDEF =         ZZIP_ERROR-32,
            ZZIP_DIR_LARGEFILE = ZZIP_ERROR-33
        }

        const int ZZIP_CASELESS =   (1<<12); /* ignore filename case inside zips */
        const int ZZIP_NOPATHS =    (1<<13); /* ignore subdir paths, just filename*/
        const int ZZIP_PREFERZIP =  (1<<14); /* try first zipped file, then real*/
        const int ZZIP_ONLYZIP =    (1<<16); /* try _only_ zipped file, skip real*/
        const int ZZIP_FACTORY =    (1<<17); /* old file handle is not closed */
        const int ZZIP_ALLOWREAL =  (1<<18); /* real files use default_io (magic) */
        const int ZZIP_THREADED = (1 << 19); /* try to be safe for multithreading */

        static char[] SEPS = { '/', '\\' };

        IntPtr zzipDir = IntPtr.Zero;
	    String file;
	    String fileFilter;
	    List<ZipFileInfo> files = new List<ZipFileInfo>();
	    List<ZipFileInfo> directories = new List<ZipFileInfo>();

        public ZipFile(String filename)
        {
            this.file = filename;
            this.fileFilter = null;
            commonLoad();
        }

        public ZipFile(String filename, String fileFilter)
        {
            this.file = filename;
            this.fileFilter = fileFilter;
            commonLoad();
        }

	    public void Dispose()
        {
            if(zzipDir != IntPtr.Zero)
            {
                ZipFile_Close(zzipDir);
                zzipDir = IntPtr.Zero;
            }
        }

        public unsafe ZipStream openFile(String filename)
        {
            String cFile = fixPathFile(filename);
	        //Get uncompressed size
	        ZZipStat zstat = new ZZipStat();
	        ZipFile_DirStat(zzipDir, cFile, &zstat, ZZIP_CASELESS);

	        //Open file
	        IntPtr zzipFile = ZipFile_OpenFile(zzipDir, cFile, ZZIP_ONLYZIP | ZZIP_CASELESS);
	        if(zzipFile != IntPtr.Zero)
	        {
		        return new ZipStream(zzipFile, zstat.UncompressedSize);
	        }
	        else
	        {
		        return null;
	        }
        }

	    public List<ZipFileInfo> listFiles(String path, bool recursive)
        {
            return findMatches(files, path, "*", recursive);
        }

	    public List<ZipFileInfo> listFiles(String path, String searchPattern, bool recursive)
        {
            return findMatches(files, path, searchPattern, recursive);
        }

	    public List<ZipFileInfo> listDirectories(String path, bool recursive)
        {
            return findMatches(directories, path, "*", recursive);
        }

	    public List<ZipFileInfo> listDirectories(String path, String searchPattern, bool recursive)
        {
            return findMatches(directories, path, searchPattern, recursive);
        }

	    public unsafe bool exists(String filename)
        {
            String cFile = fixPathFile(filename);
	        ZZipStat zstat = new ZZipStat();
            ZZipError res = ZipFile_DirStat(zzipDir, cFile, &zstat, ZZIP_CASELESS);
            return (res == ZZipError.ZZIP_NO_ERROR);
        }

        public ZipFileInfo getFileInfo(String filename)
        {
            String fixedFileName = fixPathFile(filename);
	        foreach(ZipFileInfo file in files)
	        {
		        if(file.FullName == fixedFileName)
		        {
			        return file;
		        }
	        }
	        fixedFileName = fixPathDir(filename);
	        foreach(ZipFileInfo file in directories)
	        {
		        if(file.FullName == fixedFileName)
		        {
			        return file;
		        }
	        }
	        return null;
        }

        private List<ZipFileInfo> findMatches(List<ZipFileInfo> sourceList, String path, String searchPattern, bool recursive)
        {
            bool matchAll = searchPattern == "*";
	        searchPattern = wildcardToRegex(searchPattern);
	        Regex r = new Regex(searchPattern);
	        bool matchBaseDir = (path == "" || path == "/");
	        if(!matchBaseDir)
	        {
		        path = fixPathDir(path);
	        }

	        List<ZipFileInfo> files = new List<ZipFileInfo>();
	        //recursive
	        if(recursive)
	        {
		        //looking in root folder
		        if(matchBaseDir)
		        {
			        foreach(ZipFileInfo file in sourceList)
			        {
				        if(matchAll || r.Match(file.FullName).Success)
				        {
					        files.Add(file);
				        }
			        }
		        }
		        //looking in a specific folder
		        else
		        {
			        foreach(ZipFileInfo file in sourceList)
			        {
				        Match match = r.Match(file.FullName);
				        if(file.FullName.Contains(path) && (matchAll || match.Success))
				        {
					        files.Add(file);
				        }
			        }
		        }
	        }
	        //Non recursive
	        else
	        {
		        //looking in root folder
		        if(matchBaseDir)
		        {
			        foreach(ZipFileInfo file in sourceList)
			        {
				        if(!file.FullName.Contains("/") && (matchAll || r.Match(file.FullName).Success))
				        {
					        files.Add(file);
				        }
			        }
		        }
		        //looking in specific folder
		        else
		        {
			        foreach(ZipFileInfo file in sourceList)
			        {
				        if(file.FullName.Contains(path) && (matchAll || r.Match(file.FullName).Success))
				        {
					        String cut = file.FullName.Replace(path, "");
					        if(cut.EndsWith("/"))
					        {
						        cut = cut.Substring(0, cut.Length - 1);
					        }
					        if(cut != String.Empty && !cut.Contains("/"))
					        {
						        files.Add(file);
					        }
				        }
			        }
		        }
	        }
	        return files;
        }

        private String wildcardToRegex(String wildcard)
        {
            String expression = "^";
            for (int i = 0; i < wildcard.Length; i++)
            {
                switch (wildcard[i])
                {
                    case '?':
                        expression += '.'; // regular expression notation.
                        //mIsWild = true;
                        break;
                    case '*':
                        expression += ".*";
                        //mIsWild = true;
                        break;
                    case '.':
                        expression += "\\";
                        expression += wildcard[i];
                        break;
                    case ';':
                        expression += "|";
                        //mIsWild = true;
                        break;
                    default:
                        expression += wildcard[i];
                        break;
                }
            }
            return expression;
        }

        private String fixPathDir(String path)
        {
            path = fixPathFile(path);
            path += "/";
            return path;
        }

        private String fixPathFile(String path)
        {
            //Fix up any ../ sections to point to the upper directory.
	        String[] splitPath = path.Split(SEPS, StringSplitOptions.RemoveEmptyEntries);
	        int lenMinusOne = splitPath.Length - 1;
	        StringBuilder pathString = new StringBuilder(path.Length);
	        for(int i = 0; i < lenMinusOne; ++i)
	        {
		        if(splitPath[i+1] != "..")
		        {
			        pathString.Append(splitPath[i]);
			        pathString.Append("/");
		        }
		        else
		        {
			        ++i;
		        }
	        }
	        if(splitPath[lenMinusOne] != "..")
	        {
		        pathString.Append(splitPath[lenMinusOne]);
	        }
	        return pathString.ToString();
        }

        private unsafe void commonLoad()
        {
            ZZipError zzipError = ZZipError.ZZIP_NO_ERROR;
            //zzipDir = zzip_dir_open(mName.c_str(), &zzipError);
            zzipDir = ZipFile_OpenDir(file, ref zzipError);
            if (zzipError != ZZipError.ZZIP_NO_ERROR)
	        {
		        String errorMessage;
		        switch (zzipError)
		        {
			        case ZZipError.ZZIP_OUTOFMEM:
				        errorMessage = "Out of memory";
				        break;            
			        case ZZipError.ZZIP_DIR_OPEN:
			        case ZZipError.ZZIP_DIR_STAT: 
			        case ZZipError.ZZIP_DIR_SEEK:
			        case ZZipError.ZZIP_DIR_READ:
				        errorMessage = "Unable to read zip file";
				        break;           
			        case ZZipError.ZZIP_UNSUPP_COMPR:
				        errorMessage = "Unsupported compression format";
				        break;            
			        case ZZipError.ZZIP_CORRUPTED:
				        errorMessage = "Archive corrupted";
				        break;            
			        default:
				        errorMessage = "Unknown ZZIP error number";
				        break;            
		        };
		        throw new ZipIOException("Could not open zip file {0} because of {1}", file, errorMessage);
	        }

            //Read the directories and files out of the zip file
            ZZipStat zzipEntry = new ZZipStat();
            while (ZipFile_Read(zzipDir, &zzipEntry))
            {
                String entryName = zzipEntry.Name;
                if (fileFilter == null || entryName.StartsWith(fileFilter))
                {
                    ZipFileInfo fileInfo = new ZipFileInfo(entryName, zzipEntry.CompressedSize, zzipEntry.UncompressedSize);
                    //Make sure we don't end with a /
                    if (fileInfo.IsDirectory)
                    {
                        directories.Add(fileInfo);
                    }
                    else
                    {
                        files.Add(fileInfo);
                    }
                }
            }
        }

        [DllImport("Zip")]
        private static extern IntPtr ZipFile_OpenDir(String file, ref ZZipError error);

        [DllImport("Zip")]
        private static extern void ZipFile_Close(IntPtr zzipFile);

        [DllImport("Zip")]
        private static unsafe extern bool ZipFile_Read(IntPtr zzipDir, ZZipStat* zstat);

        [DllImport("Zip")]
        private static unsafe extern ZZipError ZipFile_DirStat(IntPtr zzipDir, String filename, ZZipStat* zstat, int mode);

        [DllImport("Zip")]
        private static extern IntPtr ZipFile_OpenFile(IntPtr zzipDir, String filename, int mode);
    }
}
