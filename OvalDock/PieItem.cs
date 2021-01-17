using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using System.Xml;

namespace OvalDock
{
    public abstract class PieItem
    {
        // TODO: I feel like modifying this directly should clear the icon cache
        public bool IsCustomIcon { get; private set; }
        public bool IsCustomName { get; private set; }

        public abstract string TypeName { get; }

        public virtual CachedImage Icon { get; protected set; }

        /*
         * The custom name. Returns null if CustomName is false.
         * 
         * TODO: Check if returning null will somehow cause problems somewhere.
         *       Better to set label to "" and have Label return ""?
         */
        private string name = null;
        public virtual string Name
        {
            get
            {
                if (IsCustomName)
                    return name;
                else
                    return null;
            }

            private set
            {
                name = value;
            }
        }

        public PieItem(bool customName, string label, bool customIcon, string iconPath)
        {
            IsCustomName = customName;

            if(IsCustomName)
            {
                Name = label;
            }

            IsCustomIcon = customIcon;
            Icon = new CachedImage();

            if(IsCustomIcon)
            {
                Icon.ImagePath = iconPath;
            }
        }

        // This is again specific on type of item.
        public abstract void LeftClick(MainWindow mainWindow);

        public virtual void SaveConfig(XmlElement element)
        {
            element.SetAttribute("IsCustomIcon", IsCustomIcon.ToString());
            element.SetAttribute("IconPath", Icon.ImagePath == null ? "" : Icon.ImagePath);

            element.SetAttribute("IsCustomName", IsCustomName.ToString());
            element.SetAttribute("Name", Name == null ? "" : Name);
        }

        public virtual void LoadConfig(XmlElement element)
        {
            // TODO: You know, there should probably be some try/catch here for safety, for all of these individually?
            IsCustomIcon = bool.Parse(element.GetAttribute("IsCustomIcon"));
            Icon.ImagePath = element.GetAttribute("IconPath");

            IsCustomName = bool.Parse(element.GetAttribute("IsCustomName"));
            Name = element.GetAttribute("Name");
        }
    }
}
