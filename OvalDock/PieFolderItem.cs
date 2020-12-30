using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;

namespace OvalDock
{
    /*
     * This class is meant to be a pie folder.
     * NOT to be confused with an actual folder on your storage drive.
     */

    public class PieFolderItem : PieItem
    {
        public PieFolderItem PrevFolder { get; }

        public List<PieItem> Items { get; }

        public override CachedImage Icon
        {
            get
            {
                // Check to make sure we have a Bitmap cached.
                if (base.Icon.ImageBitmap != null)
                    return base.Icon;

                // Use the default icon otherwise
                // Clone because we CAN modify the icon directly later.
                // Load cache on main copy beforehand to do as little work on the rest of the copies as possible.
                Config.FolderDefaultIcon.CreateCache();
                base.Icon = Config.FolderDefaultIcon.Copy();
                return base.Icon;
            }
        }

        public const string TYPE_NAME = "PieFolderItem";
        public override string TypeName { get { return TYPE_NAME; } }

        public PieFolderItem(bool customName, string label, bool customIcon, string iconPath, PieFolderItem prevFolder) : base(customName, label, customIcon, iconPath)
        {
            Items = new List<PieItem>();
            PrevFolder = prevFolder;
        }

        public override void LeftClick(MainWindow mainWindow)
        {
            mainWindow.SwitchToFolder(this);
        }

        public override void SaveConfig(XmlElement element)
        {
            base.SaveConfig(element);

            foreach (PieItem item in Items)
            {
                XmlElement itemElement = element.OwnerDocument.CreateElement(item.TypeName);
                item.SaveConfig(itemElement);
                element.AppendChild(itemElement);
            }
        }

        public override void LoadConfig(XmlElement element)
        {
            base.LoadConfig(element);

            foreach (var itemElementAsXmlNode in element.ChildNodes)
            {
                // TODO: I hope this never fails. Maybe check ahead of time?
                XmlElement itemElement = (XmlElement)itemElementAsXmlNode;

                PieItem subItem = null;

                // Fill with default values
                // The rest of the values will be filled in after calling LoadConfig() on this.
                switch (itemElement.Name)
                {
                    case PieFolderItem.TYPE_NAME:
                        // (except we need to fill with prevFolder now)
                        subItem = new PieFolderItem(false, null, false, null, this);
                        break;

                    case FileItem.TYPE_NAME:
                        subItem = new FileItem(false, null, false, null, null, null, FileItemType.File);
                        break;

                    default: // This should NEVER happen.
                             // TODO: Add error message if it does?
                        break;
                }

                // This should NEVER be null afterwards.
                subItem.LoadConfig(itemElement);
                Items.Add(subItem);
            }
        }
    }
}
