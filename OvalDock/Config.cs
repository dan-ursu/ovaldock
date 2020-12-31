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

        // TODO: Migrate InnerDiskImagePath and OuterDiskImagePath to CachedImage ?
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

        public static void LoadDefaultProgramSettings()
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

        // Hopefully no typos in here.
        public static void SaveProgramSettings()
        {
            XmlDocument config = new XmlDocument();
            XmlElement rootNode = config.CreateElement("Root");
            config.AppendChild(rootNode);

            // TODO: There is a lot of code repetition in rounding doubles. Fix.
            rootNode.SetAttribute("InnerDiskImagePath", InnerDiskImagePath == null ? "" : InnerDiskImagePath);
            rootNode.SetAttribute("InnerRadius", InnerRadius.ToString("0"));
            rootNode.SetAttribute("InnerDiskNormalOpacity", InnerDiskNormalOpacity.ToString("0.##"));
            rootNode.SetAttribute("InnerDiskMouseDownOpacity", InnerDiskMouseDownOpacity.ToString("0.##"));

            rootNode.SetAttribute("OuterDiskImagePath", OuterDiskImagePath == null ? "" : OuterDiskImagePath);
            rootNode.SetAttribute("OuterRadius", OuterRadius.ToString("0"));
            rootNode.SetAttribute("OuterDiskNormalOpacity", OuterDiskNormalOpacity.ToString("0.##"));
            rootNode.SetAttribute("OuterDiskMouseDownOpacity", OuterDiskMouseDownOpacity.ToString("0.##"));

            rootNode.SetAttribute("FileNotFoundIcon", FileNotFoundIcon.ImagePath == null ? "" : FileNotFoundIcon.ImagePath);
            rootNode.SetAttribute("FolderDefaultIcon", FolderDefaultIcon.ImagePath == null ? "" : FolderDefaultIcon.ImagePath);
            rootNode.SetAttribute("PieItemSize", PieItemSize.ToString("0"));
            rootNode.SetAttribute("PieItemNormalOpacity", PieItemNormalOpacity.ToString("0.##"));
            rootNode.SetAttribute("PieItemMouseDownOpacity", PieItemMouseDownOpacity.ToString("0.##"));
            rootNode.SetAttribute("PieItemRadiusFromCenter", PieItemRadiusFromCenter.ToString("0"));

            config.Save(ProgramSaveLocation);
        }

        // I really hope there's no typos in here either.
        public static void LoadProgramSettings()
        {
            // Settings will get overwritten on a per setting basis.
            // Any setting not overwritten will hence be the deafult.
            LoadDefaultProgramSettings();

            XmlDocument config = new XmlDocument();

            try
            {
                config.Load(ProgramSaveLocation);
            }
            catch(Exception e)
            {
                return;
            }

            XmlElement rootNode = config.DocumentElement;

            // TODO: You know, I feel like some of this could be just a little automated.
            //       Probably keep an array of a custom Property class?
            InnerDiskImagePath        = rootNode.GetAttribute("InnerDiskImagePath")        == "" ? InnerDiskImagePath        : rootNode.GetAttribute("InnerDiskImagePath");
            InnerRadius               = rootNode.GetAttribute("InnerRadius")               == "" ? InnerRadius               : double.Parse(rootNode.GetAttribute("InnerRadius"));
            InnerDiskNormalOpacity    = rootNode.GetAttribute("InnerDiskNormalOpacity")    == "" ? InnerDiskNormalOpacity    : double.Parse(rootNode.GetAttribute("InnerDiskNormalOpacity"));
            InnerDiskMouseDownOpacity = rootNode.GetAttribute("InnerDiskMouseDownOpacity") == "" ? InnerDiskMouseDownOpacity : double.Parse(rootNode.GetAttribute("InnerDiskMouseDownOpacity"));

            OuterDiskImagePath        = rootNode.GetAttribute("OuterDiskImagePath")        == "" ? OuterDiskImagePath        : rootNode.GetAttribute("OuterDiskImagePath");
            OuterRadius               = rootNode.GetAttribute("OuterRadius")               == "" ? OuterRadius               : double.Parse(rootNode.GetAttribute("OuterRadius"));
            OuterDiskNormalOpacity    = rootNode.GetAttribute("OuterDiskNormalOpacity")    == "" ? OuterDiskNormalOpacity    : double.Parse(rootNode.GetAttribute("OuterDiskNormalOpacity"));
            OuterDiskMouseDownOpacity = rootNode.GetAttribute("OuterDiskMouseDownOpacity") == "" ? OuterDiskMouseDownOpacity : double.Parse(rootNode.GetAttribute("OuterDiskMouseDownOpacity"));

            FileNotFoundIcon.ImagePath = rootNode.GetAttribute("FileNotFoundIcon") == "" ? FileNotFoundIcon.ImagePath : rootNode.GetAttribute("FileNotFoundIcon");
            FileNotFoundIcon.ClearCache();

            FolderDefaultIcon.ImagePath = rootNode.GetAttribute("FolderDefaultIcon") == "" ? FolderDefaultIcon.ImagePath : rootNode.GetAttribute("FolderDefaultIcon");
            FolderDefaultIcon.ClearCache();

            PieItemSize             = rootNode.GetAttribute("PieItemSize")             == "" ? PieItemSize             : double.Parse(rootNode.GetAttribute("PieItemSize"));
            PieItemNormalOpacity    = rootNode.GetAttribute("PieItemNormalOpacity")    == "" ? PieItemNormalOpacity    : double.Parse(rootNode.GetAttribute("PieItemNormalOpacity"));
            PieItemMouseDownOpacity = rootNode.GetAttribute("PieItemMouseDownOpacity") == "" ? PieItemMouseDownOpacity : double.Parse(rootNode.GetAttribute("PieItemMouseDownOpacity"));
            PieItemRadiusFromCenter = rootNode.GetAttribute("PieItemRadiusFromCenter") == "" ? PieItemRadiusFromCenter : double.Parse(rootNode.GetAttribute("PieItemRadiusFromCenter"));
        }
    }
}
