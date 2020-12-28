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
        // This is to be used if we want to set a custom path.
        // Set to null to use a default icon. This will vary depending on the type of object.
        public string IconPath { get; private set; }

        public bool IsCustomIcon { get; private set; }
        public bool IsCustomName { get; private set; }

        public abstract string TypeName { get; }

        // To be used for only needing to load the icon from disk once.
        protected Bitmap icon = null;
        protected BitmapSource iconAsBitmapSource = null;

        // If an icon (NOT necessarily a custom icon) has been already loaded, return that. Otherwise, try to load the custom icon and then return. 
        // Code for getting a non-default icon should be overridden in stuff that inherits this.
        //
        // It is worth noting that this CAN return null. Perhaps this is worth changing at some point?
        protected virtual Bitmap Icon
        {
            get
            {
                if (icon != null)
                    return icon;

                if (!IsCustomIcon)
                    return null;

                try
                {
                    icon = new Bitmap(IconPath);
                    return icon;
                }
                catch (Exception e) // TODO: Maybe handle exceptions here a little more properly?
                {
                    return null;
                }
            }
        }

        /*
         * Because, of course, WPF couldn't just be normal and work with Bitmap.
         * It has to be hipster and work with something called BitmapImage and BitmapSource instead.
         * NOTE: BitmapImage extends BitmapSource.
         * 
         * Again, this CAN return null. Should possibly rethink this at some point.
         * 
         * TODO: Should we be using two properties like this? Let's just use a BitmapSource and that's it?
         */
        public virtual BitmapSource IconAsBitmapSource
        {
            get
            {
                // Check if we've preloaded stuff
                if (iconAsBitmapSource != null)
                    return iconAsBitmapSource;

                // Checking for custom icon is done with the Icon property. No need to check again.
                if (Icon != null)
                {
                    iconAsBitmapSource = Util.ToBitmapImage(Icon);
                    return iconAsBitmapSource;
                }

                return null;
            }
        }

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
            Name = label;

            IsCustomIcon = customIcon;
            IconPath = iconPath;
        }

        // This is again specific on type of item.
        public abstract void LeftClick();

        public void ClearIconCache()
        {
            icon = null;
            iconAsBitmapSource = null;
        }

        public virtual void SaveConfig(XmlElement element)
        {
            element.SetAttribute("IsCustomIcon", IsCustomIcon.ToString());
            element.SetAttribute("IconPath", IconPath == null ? "" : IconPath);

            element.SetAttribute("IsCustomName", IsCustomName.ToString());
            element.SetAttribute("Name", Name == null ? "" : Name);
        }

        public virtual void LoadConfig(XmlElement element)
        {
            // TODO: You know, there should probably be some try/catch here for safety, for all of these individually?
            IsCustomIcon = bool.Parse(element.GetAttribute("IsCustomIcon"));
            IconPath = element.GetAttribute("IconPath");

            IsCustomName = bool.Parse(element.GetAttribute("IsCustomName"));
            Name = element.GetAttribute("Name");
        }
    }
}
