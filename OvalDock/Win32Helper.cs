using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace OvalDock
{
    class Win32Helper
    {
        //Modifiers:
        public const uint MOD_NONE = 0x0000;
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        public const uint VK_CAPITAL = 0x14;
        public const uint VK_LWIN = 0x5B;


        // Stuff copy pasted from stackoverflow,
        // https://stackoverflow.com/questions/42910628/is-there-a-way-to-get-the-windows-default-folder-icon-using-c
        // modified to get a specific folder's icon rather than the stock one,
        // but also to properly destroy the handle afterwards
        public static Icon GetIcon(string path)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(
                path,
                0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_LARGEICON);
            
            Icon result = (Icon)Icon.FromHandle(shinfo.hIcon).Clone(); // Get a copy that doesn't use the original handle
            DestroyIcon(shinfo.hIcon); // Clean up native icon to prevent resource leak

            return result;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr handle);
                
        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        private const uint SHSIID_FOLDER = 0x3;
        private const uint SHGSI_ICON = 0x100;
        private const uint SHGSI_LARGEICON = 0x0;
        private const uint SHGSI_SMALLICON = 0x1;

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x000000001;
    }
}
