using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using System.Xml;

namespace OvalDock
{
    class Config
    {
        public static string ProgramName { get; private set; }

        public static string InnerDiskImagePath { get; set; }
        public static double InnerRadius { get; set; }
        public static double InnerDiskNormalOpacity { get; set; }
        public static double InnerDiskMouseDownOpacity { get; set; }

        public static string OuterDiskImagePath { get; set; }
        public static double OuterRadius { get; set; }
        public static double OuterDiskNormalOpacity { get; set; }
        public static double OuterDiskMouseDownOpacity { get; set; }

        public static CachedImage FileNotFoundIcon { get; private set; }
        public static CachedImage FolderDefaultIcon { get; private set; }

        public static double PieItemSize { get; set; }
        public static double PieItemNormalOpacity { get; set; }
        public static double PieItemMouseDownOpacity { get; set; }
        public static double PieItemRadiusFromCenter { get; set; }

        public static int PieItemLabelPadding { get; private set; }

        public static int PieItemLabelSize { get; private set; }

        
               

        private static string ItemSaveLocation { get; set; }

        private static string ProgramSaveLocation { get; set; }

        public static uint HotkeyModifiers { get; private set; }
        public static uint Hotkey { get; private set; }

        public static void LoadConfig()
        {
            ProgramName = "OvalDock";

            ItemSaveLocation = "items.xml";
            ProgramSaveLocation = "config.xml";

            InnerDiskImagePath = @".\System\Icons\Windows Logo.png";
            InnerRadius = 50;
            InnerDiskNormalOpacity = 1.0;
            InnerDiskMouseDownOpacity = 0.5;

            OuterDiskImagePath = @".\System\Background\circle.png";
            OuterRadius = 200;
            OuterDiskNormalOpacity = 1.0;
            OuterDiskMouseDownOpacity = 0.5;

            FileNotFoundIcon = new CachedImage();
            FileNotFoundIcon.ImagePath = @".\system\Icons\firewire.png";
            FolderDefaultIcon = new CachedImage();
            FolderDefaultIcon.ImagePath = @".\System\Icons\My Documents.png";
            PieItemSize = 50;
            PieItemNormalOpacity = 1.0;
            PieItemMouseDownOpacity = 0.5;
            PieItemRadiusFromCenter = 325;

            PieItemLabelPadding = 20;
            PieItemLabelSize = 20;

            
            

            // TODO: SO APPARENTLY, MOD_WIN NEEDS TO BE A MODIFIER IF YOU WANT TO USE IT AS theHotKey AS WELL
            //       Double check!
            HotkeyModifiers = Win32Helper.MOD_CONTROL + Win32Helper.MOD_WIN;
            Hotkey = Win32Helper.VK_LWIN;
        }

        public static void SaveItems(PieFolderItem root)
        {
            XmlDocument config = new XmlDocument();
            XmlElement rootNode = config.CreateElement("Root");
            config.AppendChild(rootNode);

            root.SaveConfig(config.DocumentElement);

            config.Save(ItemSaveLocation);
        }

        public static PieFolderItem LoadItems()
        {
            XmlDocument config = new XmlDocument();
            PieFolderItem rootFolder = new PieFolderItem(false, null, true, InnerDiskImagePath, null);

            try
            {
                config.Load(ItemSaveLocation);
                rootFolder.LoadConfig(config.DocumentElement);
            }
            catch (Exception e) // TODO: You know, I should really handle this stuff better.
            {

            }

            return rootFolder;
        }
    }
}
