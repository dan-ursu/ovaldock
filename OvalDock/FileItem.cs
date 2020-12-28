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
    class FileItem : PieItem
    {
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
                    iconAsBitmapSource = Util.ToBitmapImage(Config.PieFileNotFoundIcon);
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

        public FileItem(bool customName, string label, bool customIcon, string iconPath, string filePath, string arguments) : base(customName, label, customIcon, iconPath)
        {
            FilePath = filePath;
            Arguments = arguments;

            //TODO: Handle the file not existing somehow?
        }

        public override void LeftClick()
        {
            if (!File.Exists(FilePath))
            {
                MessageBox.Show("File does not exist.");
                return;
            }

            try
            {
                // TODO: Are there other executable file types? .msi? .com?
                if (FilePath.EndsWith(".exe"))
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
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = this.FilePath,
                        UseShellExecute = true
                    };

                    Process.Start(processStartInfo);
                }
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
        }

        public override void LoadConfig(XmlElement element)
        {
            base.LoadConfig(element);

            FilePath = element.GetAttribute("FilePath");
            Arguments = element.GetAttribute("Arguments");
        }
    }
}
