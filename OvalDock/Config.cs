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

        // Cache the icon. BitmapSource accesses Bitmap accesses string
        private static Bitmap pieFileNotFoundIconBitmap = null;
        private static BitmapSource pieFileNotFoundIconBitmapSource = null;
        public static Bitmap PieFileNotFoundIconBitmap
        {
            get
            {
                // Check icon cache.
                if(pieFileNotFoundIconBitmap != null)
                {
                    return pieFileNotFoundIconBitmap;
                }

                // Icon not pre-cached. Load it, save it, return it.
                try
                {
                    pieFileNotFoundIconBitmap = new Bitmap(PieFileNotFoundIconPath);
                    return pieFileNotFoundIconBitmap;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
        public static BitmapSource PieFileNotFoundIconBitmapSource
        {
            get
            {
                // Check BitmapSource cache.
                if (pieFileNotFoundIconBitmapSource != null)
                {
                    return pieFileNotFoundIconBitmapSource;
                }

                // Try to load Bitmap (property, NOT directly accessing the cache)
                try
                {
                    pieFileNotFoundIconBitmapSource = Util.ToBitmapImage(PieFileNotFoundIconBitmap);
                    return pieFileNotFoundIconBitmapSource;
                }
                catch(Exception e)
                {
                    return null;
                }
            }
        }
        public static string PieFileNotFoundIconPath { get; set; }

        // TODO: Consider just making a CachedImage class.
        // Exactly the same for the default folder icon
        private static Bitmap pieFolderDefaultIconBitmap = null;
        private static BitmapSource pieFolderDefaultIconBitmapSource = null;
        public static Bitmap PieFolderDefaultIconBitmap
        {
            get
            {
                // Check icon cache.
                if (pieFolderDefaultIconBitmap != null)
                {
                    return pieFolderDefaultIconBitmap;
                }

                // Icon not pre-cached. Load it, save it, return it.
                try
                {
                    pieFolderDefaultIconBitmap = new Bitmap(PieFolderDefaultIconPath);
                    return pieFolderDefaultIconBitmap;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
        public static BitmapSource PieFolderDefaultIconBitmapSource
        {
            get
            {
                // Check BitmapSource cache.
                if (pieFolderDefaultIconBitmapSource != null)
                {
                    return pieFolderDefaultIconBitmapSource;
                }

                // Try to load Bitmap (property, NOT directly accessing the cache)
                try
                {
                    pieFolderDefaultIconBitmapSource = Util.ToBitmapImage(PieFolderDefaultIconBitmap);
                    return pieFolderDefaultIconBitmapSource;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
        public static string PieFolderDefaultIconPath { get; set; }

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

            PieFolderDefaultIconPath = @".\System\Icons\My Documents.png";
            PieFileNotFoundIconPath = @".\system\Icons\firewire.png";
            PieItemSize = 50;
            PieItemRadiusFromCenter = 325;

            PieItemLabelPadding = 50;
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
