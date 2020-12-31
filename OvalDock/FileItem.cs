using System;
using System.Collections.Generic;
using System.Text;

using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml;

namespace OvalDock
{
    // Used for loading the settings window.
    // TODO: Could probably use for other things too.
    public enum FileItemType
    {
        File,
        Folder,
        Other
    }

    class FileItem : PieItem
    {
        public FileItemType Type { get; private set; }

        public string FilePath { get; private set; }

        public string Arguments { get; private set; }

        public override CachedImage Icon
        {
            get
            {
                // Check to make sure we have a BitmapSource cached.
                if (base.Icon.ImageBitmapSource != null)
                    return base.Icon;

                // Extract the file icon as a BitmapSource otherwise.
                // TODO: Just copy pasted this code from somewhere.
                try
                {
                    var sysicon = System.Drawing.Icon.ExtractAssociatedIcon(FilePath);
                    var bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                sysicon.Handle,
                                System.Windows.Int32Rect.Empty,
                                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    sysicon.Dispose();

                    // Icon extracted. Save for later and return.
                    base.Icon.ImageBitmapSource = bmpSrc;
                    return base.Icon;
                }
                catch (Exception e)
                {
                    // Icon could not be extracted. Use the "file not found" icon then.
                    // TODO: The default CAN be null? What happens then?
                    //
                    // Clone because we CAN modify the icon directly later.
                    // Load cache on main copy beforehand to do as little work on the rest of the copies as possible.
                    //
                    // TODO: This whole cloning technique is not great.
                    //       There is no way to conveniently check if we are using a "file not found" icon.
                    Config.FileNotFoundIcon.CreateCache();
                    base.Icon = Config.FileNotFoundIcon.Copy();
                    return base.Icon;
                }
            }
        }

        public override string Name
        {
            get
            {
                if (IsCustomName)
                    return base.Name;
                else
                {
                    try
                    {
                        return Path.GetFileName(FilePath);
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }
            }
        }

        public const string TYPE_NAME = "FileItem";
        public override string TypeName { get { return TYPE_NAME; } }

        public FileItem(bool customName, string label, bool customIcon, string iconPath, string filePath, string arguments, FileItemType type) : base(customName, label, customIcon, iconPath)
        {
            FilePath = filePath;
            Arguments = arguments;
            Type = type;

            //TODO: Handle the file not existing somehow?
        }

        public override void LeftClick(MainWindow mainWindow)
        {
            if (Type == FileItemType.File && !File.Exists(FilePath))
            {
                MessageBox.Show("File does not exist.");
                return;
            }

            if(Type == FileItemType.Folder && !Directory.Exists(FilePath))
            {
                MessageBox.Show("Folder does not exist.");
                return;
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo();

            processStartInfo.FileName = FilePath;
            processStartInfo.Arguments = Arguments;

            // TODO: Are there other executable file types? .msi? .com?
            if (File.Exists(FilePath) && FilePath.EndsWith(".exe"))
            {
                string workingDirectory = Path.GetDirectoryName(FilePath);

                if(workingDirectory != null && workingDirectory != "")
                {
                    processStartInfo.WorkingDirectory = workingDirectory;
                }
            }
            else
            {
                // ProcessStartInfo.UseShellExecute Property
                // true if the shell should be used when starting the process;
                // false if the process should be created directly from the executable file.
                // The default is true on .NET Framework apps and false on .NET Core apps.
                processStartInfo.UseShellExecute = true;
            }

            try
            {
                Process.Start(processStartInfo);
                mainWindow.ToggleVisibility();
            }
            catch (Exception e)
            {
                MessageBox.Show("Something went wrong - tell a programmer.");
            }           
        }

        public override void SaveConfig(XmlElement element)
        {
            base.SaveConfig(element);

            element.SetAttribute("FilePath", FilePath == null ? "" : FilePath);
            element.SetAttribute("Arguments", Arguments == null ? "" : Arguments);

            switch(Type)
            {
                case FileItemType.File:
                    element.SetAttribute("Type", "File");
                    break;

                case FileItemType.Folder:
                    element.SetAttribute("Type", "Folder");
                    break;

                case FileItemType.Other:
                    element.SetAttribute("Type", "Other");
                    break;
            }
        }

        public override void LoadConfig(XmlElement element)
        {
            base.LoadConfig(element);

            FilePath = element.GetAttribute("FilePath");
            Arguments = element.GetAttribute("Arguments");

            switch(element.GetAttribute("Type"))
            {
                case "File":
                    Type = FileItemType.File;
                    break;

                case "Folder":
                    Type = FileItemType.Folder;
                    break;

                case "Other":
                    Type = FileItemType.Other;
                    break;
            }
        }
    }
}
