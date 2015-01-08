﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Resources;
using System.IO;
using Engine;
using System.Diagnostics;
using Logging;
using Ionic.Zip;

namespace Anomaly
{
    class PublishController
    {
        private static PublishControllerEventArgs eventArgs = new PublishControllerEventArgs();
        public event EventHandler<PublishControllerEventArgs> DirectoryIgnored;

        //Use a dictionary to ensure that the files are unique the key is a lowercase mangled name and the value is the filename as returned.
        private List<VirtualFileInfo> files = new List<VirtualFileInfo>();
        HashSet<String> recursiveDirectories = new HashSet<string>();
        private Solution solution;
        private List<String> ignoreFiles = new List<String>();
        private List<String> ignoreDirectories = new List<string>();

        public PublishController(Solution solution)
        {
            this.solution = solution;
        }

        /// <summary>
        /// Scan for resources using the profile, can be null to use no profile.
        /// </summary>
        /// <param name="profileName">The name of the profile of ignore files.</param>
        public void scanResources(String profileName)
        {
            ignoreDirectories.Clear();
            ignoreFiles.Clear();
            if (profileName != null)
            {
                ConfigFile configFile = new ConfigFile(Path.Combine(solution.WorkingDirectory, profileName + ".rpr"));
                configFile.loadConfigFile();
                ConfigSection ignoredDirectories = configFile.createOrRetrieveConfigSection("IgnoredDirectories");
                ConfigIterator dirIter = new ConfigIterator(ignoredDirectories, "Dir");
                while (dirIter.hasNext())
                {
                    addIgnoreDirectory(dirIter.next());
                }
                ConfigSection ignoredFiles = configFile.createOrRetrieveConfigSection("IgnoredFiles");
                ConfigIterator fileIter = new ConfigIterator(ignoredFiles, "File");
                while (fileIter.hasNext())
                {
                    addIgnoreFile(fileIter.next());
                }
            }

            ResourceManager resourceManager = solution.getAllResources();

            files.Clear();
            recursiveDirectories.Clear();
            foreach (SubsystemResources subsystem in resourceManager.getSubsystemEnumerator())
            {
                foreach (ResourceGroup group in subsystem.getResourceGroupEnumerator())
                {
                    foreach (Resource resource in group.getResourceEnumerator())
                    {
                        processFile(resource.LocName, resource.Recursive);
                    }
                }
            }
            foreach (ExternalResource additionalResource in solution.AdditionalResources)
            {
                processFile(additionalResource.Path, true);
            }
            foreach (String directory in recursiveDirectories)
            {
                processRecursiveDirectory(directory);
            }
        }

        public void saveResourceProfile(String profileName)
        {
            int i;
            ConfigFile configFile = new ConfigFile(Path.Combine(solution.WorkingDirectory, profileName + ".rpr"));
            ConfigSection ignoredDirectories = configFile.createOrRetrieveConfigSection("IgnoredDirectories");
            i = 0;
            foreach(String dir in ignoreDirectories)
            {
                ignoredDirectories.setValue("Dir" + i++, dir);
            }
            ConfigSection ignoredFiles = configFile.createOrRetrieveConfigSection("IgnoredFiles");
            i = 0;
            foreach(String file in ignoreFiles)
            {
                ignoredFiles.setValue("File" + i++, file);
            }
            configFile.writeConfigFile();
        }

        public void copyResources(String targetDirectory, String archiveName, bool compress, bool obfuscate)
        {
            String originalDirectory = targetDirectory;
            if (compress)
            {
                targetDirectory += "/zipTemp";
            }
            Log.Info("Starting file copy to {0}", targetDirectory);
            foreach (VirtualFileInfo sourceFile in files)
            {
                if (!ignoreFiles.Contains(sourceFile.FullName) && !ignoreDirectories.Contains(sourceFile.DirectoryName))
                {
                    String destination = targetDirectory + Path.DirectorySeparatorChar + sourceFile.FullName;
                    String directory = Path.GetDirectoryName(destination);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    Log.Info("Copying {0} to {1}.", sourceFile.RealLocation, destination);
                    File.Copy(sourceFile.RealLocation, destination, true);
                }
            }
            if (compress)
            {
                String zipFileName = originalDirectory + "\\" + archiveName + ".zip";
                Log.Info("Starting compression to {0}", zipFileName);
                if (File.Exists(zipFileName))
                {
                    File.Delete(zipFileName);
                }

                using (ZipFile zipFile = new ZipFile(zipFileName, new ZipStatusTextWriter()))
                {
                    zipFile.AddDirectory(targetDirectory, "");
                    zipFile.Save();
                }

                Log.Info("Finished compression to {0}", zipFileName);
                Directory.Delete(targetDirectory, true);

                if (obfuscate)
                {
                    String obfuscateFileName = originalDirectory + "\\" + archiveName + ".dat";
                    obfuscateZipFile(zipFileName, obfuscateFileName);
                    File.Delete(zipFileName);
                }
            }
        }

        public static void obfuscateZipFile(String zipFileName, String obfuscateFileName)
        {
            Log.Info("Starting obfuscation to {0}", obfuscateFileName);
            if (File.Exists(obfuscateFileName))
            {
                File.Delete(obfuscateFileName);
            }

            byte[] buffer = new byte[4096];
            using (Stream source = File.Open(zipFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (Stream dest = File.Open(obfuscateFileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                {
                    int numRead;
                    while ((numRead = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        for (int i = 0; i < numRead; ++i)
                        {
                            buffer[i] ^= 73;
                        }
                        dest.Write(buffer, 0, numRead);
                    }
                }
            }
            Log.Info("Finished obfuscation to {0}", obfuscateFileName);
        }

        public static void deobfuscateZipFile(String obfuscatedFileName, String deobfuscatedFileName)
        {
            Log.Info("Starting deobfuscation to {0}", deobfuscatedFileName);
            if (File.Exists(deobfuscatedFileName))
            {
                File.Delete(deobfuscatedFileName);
            }

            byte[] buffer = new byte[4096];
            using (Stream source = File.Open(obfuscatedFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (Stream dest = File.Open(deobfuscatedFileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                {
                    int numRead;
                    while ((numRead = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        for (int i = 0; i < numRead; ++i)
                        {
                            buffer[i] ^= 73;
                        }
                        dest.Write(buffer, 0, numRead);
                    }
                }
            }
            Log.Info("Finished deobfuscation to {0}", deobfuscatedFileName);
        }

        /// <summary>
        /// Get the list of files as pretty names, aka not mangled to all lower
        /// case for comparison. This is suitable for display.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<VirtualFileInfo> getPrettyFileList()
        {
            return files;
        }

        public void addIgnoreFile(VirtualFileInfo fileInfo)
        {
            addIgnoreFile(fileInfo.FullName);
        }

        private void addIgnoreFile(String fileName)
        {
            ignoreFiles.Add(fileName);
        }

        public void removeIgnoreFile(VirtualFileInfo fileInfo)
        {
            ignoreFiles.Remove(fileInfo.FullName);
        }

        public void addIgnoreDirectory(String directory)
        {
            ignoreDirectories.Add(directory);
            if (DirectoryIgnored != null)
            {
                eventArgs.FileInfo = VirtualFileSystem.Instance.getFileInfo(directory);
                DirectoryIgnored.Invoke(this, eventArgs);
            }
        }

        public void removeIgnoreDirectory(String directory)
        {
            ignoreDirectories.Remove(directory);
        }

        private void processFile(String url, bool recursive)
        {
            VirtualFileSystem vfs = VirtualFileSystem.Instance;
            if (vfs.isDirectory(url))
            {
                if (!ignoreDirectories.Contains(url))
                {
                    if (recursive)
                    {
                        recursiveDirectories.Add(url);
                    }
                    else
                    {
                        foreach (String file in vfs.listFiles(url, false))
                        {
                            addFile(vfs.getFileInfo(file));
                        }
                    }
                }
            }
            else
            {
                try
                {
                    addFile(vfs.getFileInfo(url));
                }
                catch (FileNotFoundException)
                {
                    Log.Warning("Could not find file {0}", url);
                }
            }
        }

        private void addFile(VirtualFileInfo fileInfo)
        {
            if (!fileInfo.Name.EndsWith("~") && !fileInfo.DirectoryName.Contains(".svn") && !ignoreFiles.Contains(fileInfo.FullName) && !ignoreDirectories.Contains(fileInfo.DirectoryName))
            {
                //Make sure the file does not already exist in the list.
                foreach (VirtualFileInfo existingInfo in files)
                {
                    if (existingInfo.RealLocation == fileInfo.RealLocation)
                    {
                        return;
                    }
                }
                files.Add(fileInfo);
            }
        }

        private void processRecursiveDirectory(String directory)
        {
            foreach (String file in VirtualFileSystem.Instance.listFiles(directory, true))
            {
                addFile(VirtualFileSystem.Instance.getFileInfo(file));
            }
        }
    }

    class PublishControllerEventArgs : EventArgs
    {
        public VirtualFileInfo FileInfo { get; set; }
    }

    class PublishDirectoryInfo
    {
        static char[] SEPS = { ':' };

        public static PublishDirectoryInfo FromString(String sourceString)
        {
            PublishDirectoryInfo newDirInfo = null;
            String[] split = sourceString.Split(SEPS);
            if (split.Length == 2)
            {
                bool recursive;
                if (bool.TryParse(split[1], out recursive))
                {
                    newDirInfo = new PublishDirectoryInfo(split[0], recursive);
                }
            }
            return newDirInfo;
        }

        public PublishDirectoryInfo(String directory, bool recursive)
        {
            Directory = directory;
            Recursive = recursive;
        }

        public String Directory { get; set; }

        public bool Recursive { get; set; }

        public override string ToString()
        {
            return String.Format("{0}:{1}", Directory, Recursive);
        }
    }
}
