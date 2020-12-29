using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

using System.Xml;

namespace OvalDock
{
    class Config
    {
        public static string ProgramName { get; private set; }

        public static string InnerDiskImagePath { get; private set; }

        public static int InnerRadius { get; set; }

        public static double InnerDiskNormalOpacity { get; private set; }

        public static double InnerDiskMouseDownOpacity { get; private set; }

        public static int OuterRadius { get; private set; }

        public static string OuterDiskImagePath { get; private set; }

        public static Bitmap PieFolderDefaultIcon { get; private set; }

        public static int PieItemSize { get; private set; }

        public static int PieItemLabelPadding { get; private set; }

        public static int PieItemLabelSize { get; private set; }

        public static int PieItemRadiusFromCenter { get; private set; }

        public static Bitmap PieFileNotFoundIcon { get; private set; }

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

            OuterRadius = 200;
            PieItemSize = 50;
            PieItemLabelPadding = 50;
            PieItemLabelSize = 20;
            PieItemRadiusFromCenter = 325;

            OuterDiskImagePath = @".\System\Background\circle.png";
            PieFolderDefaultIcon = new Bitmap(@".\System\Icons\My Documents.png");
            PieFileNotFoundIcon = new Bitmap(@".\system\Icons\firewire.png");

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
