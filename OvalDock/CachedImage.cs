using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Media.Imaging;

namespace OvalDock
{
    // Provides an image class that caches both a Bitmap and BitmapSource of the image
    // BitmapSource loads from Bitmap loads from string (path)
    public class CachedImage
    {
        // Changing the image path will clear the cache!
        private string imagePath = null;
        public string ImagePath
        {
            get
            {
                return imagePath;
            }

            set
            {
                imagePath = value;
                ClearCache();
            }
        }

        private Bitmap imageBitmapCache = null;
        public Bitmap ImageBitmap
        {
            get
            {
                // Check cache
                if (imageBitmapCache != null)
                    return imageBitmapCache;

                // No cached image. Try to load from path.
                try
                {
                    imageBitmapCache = new Bitmap(imagePath);
                    return imageBitmapCache;
                }
                catch(Exception e)
                {
                    return null;
                }
            }

            set
            {
                // Clear the rest of the cache if changing the image manually.
                imageBitmapCache = value;
                imageBitmapSourceCache = null;
            }
        }

        private BitmapSource imageBitmapSourceCache = null;
        public BitmapSource ImageBitmapSource
        { 
            get
            {
                // Check cache
                if (imageBitmapSourceCache != null)
                    return imageBitmapSourceCache;

                // No cached image. Try to load from the bitmap version (property, NOT cache!)
                if(ImageBitmap == null)
                {
                    return null;
                }
                else
                {
                    imageBitmapSourceCache = Util.ToBitmapImage(ImageBitmap);
                    return imageBitmapSourceCache;
                }
            }

            set
            {
                // Clear the rest of the cache if changing the image manually.
                imageBitmapSourceCache = value;
                imageBitmapCache = null;
            }
        }
                
        public void ClearCache()
        {
            imageBitmapCache = null;
            imageBitmapSourceCache = null;
        }

        public void CreateCache()
        {
            Bitmap tempBitmap = ImageBitmap;
            BitmapSource tempBitmapSource = ImageBitmapSource;
        }

        // Shallow copy. Used for when you want to reference default icons and change it later.
        public CachedImage Copy()
        {
            return (CachedImage)MemberwiseClone();
        }
    }
}
