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

        public override BitmapSource IconAsBitmapSource
        {
            get
            {
                // Preloaded or custom icon handled in base class.
                if (base.IconAsBitmapSource != null)
                    return base.IconAsBitmapSource;

                // File icon handled here.
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
                    iconAsBitmapSource = bmpSrc;
                    return bmpSrc;
                }
                catch (Exception e)
                {
                    // Icon could not be extracted. Save the default for later and return.
                    // TODO: The default CAN be null? What happens then?
                    iconAsBitmapSource = Config.PieFileNotFoundIconBitmapSource;
                    return iconAsBitmapSource;
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

            try
            {
                // TODO: Are there other executable file types? .msi? .com?
                if (File.Exists(FilePath) && FilePath.EndsWith(".exe"))
                {
                    Process.Start(FilePath);
                }
                else
                {
                    // ProcessStartInfo.UseShellExecute Property
                    // true if the shell should be used when starting the process;
                    // false if the process should be created directly from the executable file.
                    // The default is true on .NET Framework apps and false on .NET Core apps.

                    //TODO: IMPLEMENT ARGUMENTS!
                    ProcessStartInfo processStartInfo = new ProcessStartInfo();

                    processStartInfo.FileName = FilePath;
                    processStartInfo.UseShellExecute = true;
                    processStartInfo.Arguments = Arguments;

                    Process.Start(processStartInfo);
                }

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
