using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace OvalDock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Code for global hotkeys
        // https://social.technet.microsoft.com/wiki/contents/articles/30568.wpf-implementing-global-hot-keys.aspx
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private HwndSource source;
        private IntPtr handle;
        private const int HOTKEY_ID = 9000;
        // End of hotkey code


        public System.Windows.Controls.Image InnerDisk { get; private set; }
        public System.Windows.Controls.Image OuterDisk { get; private set; }

        private bool dragged = false;

        // TODO: Probably more hassle than it's worth, but should this second argument be the inner circle icon somehow?
        public PieFolderItem RootFolder { get; private set; }
        public PieFolderItem CurrentFolder { get; private set; }

        //public List<Button> ItemButtons { get; }
        //public List<KeyValuePair<Button, Label>> ItemLabels { get; }

        List<ItemDisplayInfo> ItemDisplayInfos { get; }



        // Unfortunately have to rely on Windows Forms for this.
        // .csproj was modified to use Windows Forms as well.
        private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();

            // TODO: Organize this better? Or rename LoadConfig() to LoadProgramConfig()?
            Config.LoadProgramSettings();
            RootFolder = Config.LoadItems();

            CurrentFolder = RootFolder;

            ItemDisplayInfos = new List<ItemDisplayInfo>();

            //ItemButtons = new List<Button>();
            //ItemLabels = new List<KeyValuePair<Button, Label>>();

            ResizeWindow();

            CreateOuterDisk();
            CreateInnerDisk();

            mainWindow.LocationChanged += MainWindow_LocationChanged;

            CreateNotifyIcon();

            PreloadIconsRecursive(RootFolder);
            RefreshFolder();
        }


        // Handle hotkey registration in here.
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            handle = new WindowInteropHelper(this).Handle;
            source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);

            var result = RegisterHotKey(handle, HOTKEY_ID, Config.HotkeyModifiers, Config.Hotkey);

            // TODO: Handle result = false? Would this ever happen?
        }

        // Also for handling hotkey
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == Config.Hotkey) //TODO: THIS WILL NEED TO CHANGE DEPENDING ON USER-DEFINED HOTKEY
                            {
                                //handle global hot key here...
                                ToggleVisibility();
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }










        // Preload the icons for speed
        public void PreloadIconsRecursive(PieFolderItem folder)
        {
            // Force each icon to load (at least) once.
            // Preloading is handled in the IconAsBitmapSource property.
            //
            // No need to worry about "preloading twice".
            // The second time, it has already been preloaded.
            BitmapSource temp;

            folder.Icon.CreateCache();

            foreach (PieItem item in folder.Items)
            {
                item.Icon.CreateCache();
            }

            foreach (PieItem item in folder.Items)
            {
                if (item is PieFolderItem)
                {
                    PreloadIconsRecursive((PieFolderItem)item);
                }
            }
        }

        public void ResizeWindow()
        {
            // Plenty of space, so that labels can go quite a bit out of "bounds"
            double maxRadius = Math.Max(Config.InnerRadius, Config.OuterRadius);

            mainWindow.Width = 3 * maxRadius;
            mainWindow.Height = 3 * maxRadius;

            mainGrid.Width = 3 * maxRadius;
            mainGrid.Height = 3 * maxRadius;
        }

        private void CreateNotifyIcon()
        {
            // TODO: Make a proper icon for this.
            notifyIcon.Icon = SystemIcons.Application;
            notifyIcon.Text = Config.ProgramName;
            notifyIcon.Visible = true;

            System.Windows.Forms.ToolStripMenuItem quitItem = new System.Windows.Forms.ToolStripMenuItem();
            quitItem.Text = "Quit";
            quitItem.Click +=
                (s, e) =>
                {
                    //Close(); // Close() only shuts down the current window. This doesn't work if there are other windows open.
                    Application.Current.Shutdown();
                };

            System.Windows.Forms.ToolStripMenuItem settingsItem = new System.Windows.Forms.ToolStripMenuItem();
            settingsItem.Text = "Settings";
            settingsItem.Click +=
                (s, e) =>
                {
                    if(ProgramSettingsWindow.IsWindowActive)
                    {
                        MessageBox.Show("Settings window already active");
                        return;
                    }
                    ProgramSettingsWindow settingsWindow = new ProgramSettingsWindow(this);
                    settingsWindow.Show();
                };

            System.Windows.Forms.ContextMenuStrip contextMenu = new System.Windows.Forms.ContextMenuStrip();

            contextMenu.Items.Add(settingsItem);
            contextMenu.Items.Add(quitItem);

            notifyIcon.ContextMenuStrip = contextMenu;
        }

        public void ToggleVisibility()
        {
            switch (Visibility)
            {
                case Visibility.Visible:
                    Visibility = Visibility.Hidden;

                    // TODO: Normally, I would include a reset to root folder command here.
                    //       HOWEVER, it seems that the window will only get redrawn when it is visibile,
                    //       causing the old version to flash on screen for a split second.
                    //       Should figure out a way to force it to refresh while hidden.

                    // SwitchToFolder(rootFolder);

                    break;

                case Visibility.Hidden:
                    //
                    // This is a bit of a nightmare, roughly copy pasted from Stackoverflow.
                    //
                    // MY UNDERSTANDING:
                    //
                    // Only Windows Forms returns the mouse position relative to the SCREEN.
                    // (The WPF answers could only be used if the mouse was on the window?)
                    //
                    // BUT Windows Forms uses actual pixels
                    // and WPF uses units dependent on DPI, scaling, etc...
                    // so you need to convert between the two.
                    //
                    System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
                    var temp = new System.Windows.Point(point.X, point.Y);

                    var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                    var mouse = transform.Transform(temp);

                    // TODO: Width/Height or ActualWidth/ActualHeight here?
                    Left = mouse.X - ActualWidth / 2;
                    Top = mouse.Y - ActualHeight / 2;

                    // NOTE: This assumes the inner disk is dead center. Probably the case.
                    //Left = mouseCoords.X - (this.Width / 2);
                    //Top = mouseCoords.Y - (this.Height / 2);

                    Visibility = Visibility.Visible;
                    break;
            }
        }

        private void CreateOuterDisk()
        {
            OuterDisk = new System.Windows.Controls.Image();

            //outerDisk.Source = new BitmapImage(new Uri(outerDiskImagePath));
            OuterDisk.Source = Util.ToBitmapImage(new Bitmap(Config.OuterDiskImagePath));
            OuterDisk.Width = 2 * Config.OuterRadius;
            OuterDisk.Height = 2 * Config.OuterRadius;

            //var margin = new Thickness();
            //margin.Left = 0;
            //margin.Top = 0;
            //outerDisk.Margin = margin;

            OuterDisk.Opacity = Config.OuterDiskNormalOpacity;

            OuterDisk.HorizontalAlignment = HorizontalAlignment.Center;
            OuterDisk.VerticalAlignment = VerticalAlignment.Center;

            //outerDisk.MouseRightButtonUp += OuterDisk_MouseRightButtonUp;
            OuterDisk.MouseLeftButtonDown += OuterDisk_MouseLeftButtonDown;

            // Create the context menu
            MenuItem addMenuItem = new MenuItem();
            addMenuItem.Header = "Add";
            addMenuItem.Click +=
                (s, e) =>
                {
                    // TODO: This feels inefficient, but PieItemSettingsWindow(null, ...) feels like something might break
                    PieItem newItem = new FileItem(false, null, false, null, null, null, FileItemType.File);
                    PieItemSettingsWindow pieItemSettingsWindow = new PieItemSettingsWindow(newItem, CurrentFolder);
                    pieItemSettingsWindow.ShowDialog();

                    if (pieItemSettingsWindow.Saved)
                    {
                        CurrentFolder.Items.Add(pieItemSettingsWindow.NewPieItem);
                        RefreshFolder();
                        Config.SaveItems(RootFolder);
                    }
                };

            OuterDisk.ContextMenu = new ContextMenu();
            OuterDisk.ContextMenu.Items.Add(addMenuItem);

            OuterDisk.AllowDrop = true;
            OuterDisk.Drop += Disk_Drop;

            mainGrid.Children.Add(OuterDisk);
        }

        private void CreateInnerDisk()
        {
            InnerDisk = new System.Windows.Controls.Image();

            // innerDisk.Source = ToBitmapImage(new Bitmap(Config.InnerDiskImagePath));
            InnerDisk.Source = CurrentFolder.Icon.ImageBitmapSource;

            InnerDisk.Opacity = Config.InnerDiskNormalOpacity;

            InnerDisk.Width = 2 * Config.InnerRadius;
            InnerDisk.Height = 2 * Config.InnerRadius;

            InnerDisk.HorizontalAlignment = HorizontalAlignment.Center;
            InnerDisk.VerticalAlignment = VerticalAlignment.Center;

            InnerDisk.MouseLeftButtonDown += InnerDisk_MouseLeftButtonDown;

            InnerDisk.AllowDrop = true;
            InnerDisk.Drop += Disk_Drop;

            mainGrid.Children.Add(InnerDisk);
        }

        // Drag and drop capability, automagically add file/folder.
        // Used for both the inner and outer disks.
        private void Disk_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null && files.Length != 0)
            {
                // TODO: This feels inefficient, but PieItemSettingsWindow(null, ...) feels like something might break
                FileItemType type;

                if (File.Exists(files[0]))
                {
                    type = FileItemType.File;
                }
                else if(Directory.Exists(files[0]))
                {
                    type = FileItemType.Folder;
                }
                else
                {
                    return;
                }

                PieItem newItem = new FileItem(false, null, false, null, files[0], null, type);
                CurrentFolder.Items.Add(newItem);
                RefreshFolder();
                Config.SaveItems(RootFolder);
            }
        }

        // Add items based on folder's items.
        // Also change center icon to folder's icon.
        public void SwitchToFolder(PieFolderItem folder)
        {
            ClearItems();

            CurrentFolder = folder;

            // Add items
            for (int i = 0; i < folder.Items.Count; i++)
            {
                AddPieItem(folder.Items[i], i, folder.Items.Count);
            }

            // Change center icon
            InnerDisk.Source = folder.Icon.ImageBitmapSource;
        }

        // To be used if an item was changed in this folder.
        public void RefreshFolder()
        {
            SwitchToFolder(CurrentFolder);
        }

        // Refresh the "file not found" icons.
        // TODO: This is inefficient. Will refresh if not using a custom icon
        //       but successfully loaded an icon from the file.
        // TODO: This is also awful. Much better to be rid of the Copy() portion used somewhere
        //       and just directly pointed to Config.FileNotFoundIcon.
        public void ClearCachedImagesFileNotFound(PieFolderItem folder, string oldPath, string newPath)
        {
            foreach(PieItem item in folder.Items)
            {
                if(item is FileItem && !item.IsCustomIcon && item.Icon.ImagePath == oldPath)
                {
                    item.Icon.ClearCache();
                    item.Icon.ImagePath = newPath;
                }
                else if(item is PieFolderItem)
                {
                    ClearCachedImagesFileNotFound((PieFolderItem)item, oldPath, newPath);
                }
            }
        }

        // TODO: Similar here.
        public void ClearCachedImagesDefaultFolder(PieFolderItem folder, string oldPath, string newPath)
        {
            if (!folder.IsCustomIcon && folder.Icon.ImagePath == oldPath)
            {
                folder.Icon.ClearCache();
                folder.Icon.ImagePath = newPath;
            }                

            foreach(PieItem item in folder.Items)
            {
                if(item is PieFolderItem)
                {
                    ClearCachedImagesDefaultFolder((PieFolderItem)item, oldPath, newPath);
                }
            }
        }

        // This is used to update the variable appearance of the items.
        // Ex: name, icon, user-configurable sizes, etc...
        private void UpdateItemAppearance(ItemDisplayInfo itemInfo, int number, int totalItems)
        {
            itemInfo.ItemButton.Width = Config.PieItemSize;
            itemInfo.ItemButton.Height = Config.PieItemSize;
            itemInfo.ItemButton.Opacity = Config.PieItemNormalOpacity;

            // To be spread evenly in a circle.
            var buttonMargin = new Thickness();
            buttonMargin.Left = Config.PieItemRadiusFromCenter * Math.Cos(number * 2 * Math.PI / totalItems);
            buttonMargin.Top = Config.PieItemRadiusFromCenter * Math.Sin(number * 2 * Math.PI / totalItems);
            itemInfo.ItemButton.Margin = buttonMargin;

            // This was initialized as an Image() during AddPieItem().
            ((System.Windows.Controls.Image)itemInfo.ItemButton.Content).Source = itemInfo.Item.Icon.ImageBitmapSource;



            var labelMargin = new Thickness();
            labelMargin.Left = itemInfo.ItemButton.Margin.Left;
            labelMargin.Top = itemInfo.ItemButton.Margin.Top + Config.PieItemSize + Config.PieItemLabelPadding; // TODO: Figure out proper padding.
            itemInfo.ItemLabel.Margin = labelMargin;

            itemInfo.ItemLabel.FontSize = Config.PieItemLabelSize;

            itemInfo.ItemLabel.Content = (itemInfo.Item.Name == null) ? "" : itemInfo.Item.Name;
        }

        public void UpdateItemAppearance()
        {
            for (int i = 0; i < ItemDisplayInfos.Count; i++)
            {
                UpdateItemAppearance(ItemDisplayInfos[i], i, ItemDisplayInfos.Count);
            }
        }

        private void ClearItems()
        {
            foreach (ItemDisplayInfo info in ItemDisplayInfos)
            {
                mainGrid.Children.Remove(info.ItemButton);
                mainGrid.Children.Remove(info.ItemLabel);
            }

            ItemDisplayInfos.Clear();
        }

        // This is a bit more complicated.
        // We have to account for all the stuff that hovering over/pressing/etc... on an item does.
        private void AddPieItem(PieItem pieItem, int number, int totalItems)
        {
            ItemDisplayInfo itemInfo = new ItemDisplayInfo();

            // The PieItem
            itemInfo.Item = pieItem;


            // The Button control to be used in the main window.
            // Here, we set the non-user-configurable options.
            itemInfo.ItemButton = new Button();

            itemInfo.ItemButton.HorizontalAlignment = HorizontalAlignment.Center;
            itemInfo.ItemButton.VerticalAlignment = VerticalAlignment.Center;

            itemInfo.ItemButton.Background = System.Windows.Media.Brushes.Transparent;
            itemInfo.ItemButton.BorderBrush = System.Windows.Media.Brushes.Transparent;

            // Currently just a blank image, will actually be set to something later on.
            var itemImage = new System.Windows.Controls.Image();
            itemInfo.ItemButton.Content = itemImage;


            // The Label control to be used in the main window.
            itemInfo.ItemLabel = new Label();

            itemInfo.ItemLabel.HorizontalAlignment = HorizontalAlignment.Center;
            itemInfo.ItemLabel.VerticalAlignment = VerticalAlignment.Center;

            // This fixes the flashing button issue if the mouse is on top of both the button and the label.
            itemInfo.ItemLabel.IsHitTestVisible = false;

            // Invisible by default. Will activate only on mouse hover on button.
            itemInfo.ItemLabel.Visibility = Visibility.Hidden;


            UpdateItemAppearance(itemInfo, number, totalItems);

            // TODO: Make labels always appear on top of buttons.
            mainGrid.Children.Add(itemInfo.ItemLabel);


            itemInfo.ItemButton.MouseEnter +=
                (s, e) =>
                {
                    itemInfo.ItemLabel.Visibility = Visibility.Visible;
                };

            itemInfo.ItemButton.MouseLeave +=
                (s, e) =>
                {
                    itemInfo.ItemLabel.Visibility = Visibility.Hidden;
                };

            // Handle custom clicking and dragging in these next three functions.
            // MouseDown just means the mouse has been pressed down on the button.
            // Dragging means the mouse has moved past the given threshold while MouseDown and we started visually dragging.
            itemInfo.ItemButton.PreviewMouseLeftButtonDown +=
                (s, e) =>
                {
                    itemInfo.MouseDown = true;

                    itemInfo.InitialMousePosition = Mouse.GetPosition(mainGrid);
                };

            itemInfo.ItemButton.PreviewMouseLeftButtonUp +=
                (s, e) =>
                {
                    if(itemInfo.Dragging)
                    {
                        RefreshFolder();
                        Config.SaveItems(RootFolder);
                    }
                    else
                    {
                        itemInfo.Item.LeftClick(this);
                    }

                    itemInfo.MouseDown = false;
                    itemInfo.Dragging = false;

                    itemInfo.ItemButton.RenderTransform = Transform.Identity;
                    itemInfo.ItemLabel.RenderTransform = Transform.Identity;
                };

            itemInfo.ItemButton.PreviewMouseMove +=
                (s, e) =>
                {
                    if(!itemInfo.MouseDown)
                    {
                        return;
                    }

                    var currentMousePoint = Mouse.GetPosition(mainGrid);

                    if (itemInfo.Dragging)
                    {
                        // TODO: Constantly creating a bunch of new objects here feels inefficient.
                        var offset = new TranslateTransform(currentMousePoint.X - itemInfo.InitialMousePosition.X, currentMousePoint.Y - itemInfo.InitialMousePosition.Y);

                        itemInfo.ItemButton.RenderTransform = offset;
                        itemInfo.ItemLabel.RenderTransform = offset;


                        // TODO: Handle item swapping here.
                        System.Windows.Point centerPoint = new System.Windows.Point(mainGrid.Width / 2, mainGrid.Height / 2);
                        double angle = Math.Atan2(currentMousePoint.Y - centerPoint.Y, currentMousePoint.X - centerPoint.X);

                        // Make between $0$ and $2\pi$
                        if(angle < 0)
                            angle += 2*Math.PI;

                        // itemInfo.ItemLabel.Content = angle.ToString();

                        // You're gonna need to draw a picture for this.
                        // Sorry ahead of time.
                        int oldPosition = CurrentFolder.Items.IndexOf(itemInfo.Item);
                        int halfSpaces = (int)(2 * angle / (2 * Math.PI / CurrentFolder.Items.Count));

                        int newPosition;

                        // Probably will never get strict inequality, but safety for roundoff nonsense.
                        if (halfSpaces <= 0 || halfSpaces >= 2 * CurrentFolder.Items.Count - 1)
                        {
                            newPosition = 0;
                        }
                        else
                        {
                            newPosition = (halfSpaces + 1) / 2;
                        }

                        //itemInfo.ItemLabel.Content = oldPosition.ToString();

                        // Swap oldPosition and newPosition
                        if(newPosition != oldPosition)
                        {
                            if(oldPosition < newPosition)
                            {
                                CurrentFolder.Items.Insert(newPosition + 1, itemInfo.Item);
                                CurrentFolder.Items.RemoveAt(oldPosition);
                            }
                            else // newPosition < oldPosition
                            {
                                CurrentFolder.Items.Insert(newPosition, itemInfo.Item);
                                CurrentFolder.Items.RemoveAt(oldPosition + 1);
                            }

                            // TODO: The behaviour when "swapping" the last and first items might not be
                            //       exactly what the user expects. Investigate making this more intuitive?

                            RefreshFolder();

                            // Start dragging the new Button that was generated during RefreshFolder().
                            // Here's a cheap way to pull this off. Just replace the new one with the old one.
                            int newInfoIndex = ItemDisplayInfos.FindIndex((s) => { return s.Item == itemInfo.Item; });

                            mainGrid.Children.Remove(ItemDisplayInfos[newInfoIndex].ItemButton);
                            mainGrid.Children.Remove(ItemDisplayInfos[newInfoIndex].ItemLabel);

                            ItemDisplayInfos[newInfoIndex] = itemInfo;

                            mainGrid.Children.Add(itemInfo.ItemButton);
                            mainGrid.Children.Add(itemInfo.ItemLabel);
                        }
                    }
                    else
                    {
                        if(System.Windows.Point.Subtract(currentMousePoint, itemInfo.InitialMousePosition).Length >= Config.PieItemDragSensitivity) // TODO: Implement custom tolerances
                        {
                            itemInfo.Dragging = true;
                        }
                    }
                };

            // Create the context menu
            MenuItem settingsMenuItem = new MenuItem();
            settingsMenuItem.Header = "Settings";
            settingsMenuItem.Click +=
                (s, e) =>
                {
                    PieItemSettingsWindow pieItemSettingsWindow = new PieItemSettingsWindow(pieItem, CurrentFolder);
                    pieItemSettingsWindow.ShowDialog();

                    // Replace the old pieItem with the new one
                    if (pieItemSettingsWindow.Saved)
                    {
                        int i = CurrentFolder.Items.IndexOf(pieItem);

                        // You know, I feel like this should never happen
                        if (i == -1)
                            return;

                        CurrentFolder.Items[i] = pieItemSettingsWindow.NewPieItem;
                        RefreshFolder();
                        Config.SaveItems(RootFolder);
                    }
                };

            MenuItem removeMenuItem = new MenuItem();
            removeMenuItem.Header = "Remove";
            removeMenuItem.Click +=
                (s, e) =>
                {
                    CurrentFolder.Items.Remove(pieItem);
                    RefreshFolder();
                    Config.SaveItems(RootFolder);
                };

            itemInfo.ItemButton.ContextMenu = new ContextMenu();
            itemInfo.ItemButton.ContextMenu.Items.Add(settingsMenuItem);
            itemInfo.ItemButton.ContextMenu.Items.Add(removeMenuItem);


            //TODO: Figure out transparency of background/border on hover.

            ItemDisplayInfos.Add(itemInfo);
            mainGrid.Children.Add(itemInfo.ItemButton);
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            dragged = true;
        }

        // TODO: Use a button for the inner disk instead of an image? Can't currently figure out buttons with a non-square border.
        private void InnerDisk_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            InnerDisk.Opacity = Config.InnerDiskMouseDownOpacity;
            OuterDisk.Opacity = Config.OuterDiskMouseDownOpacity;

            foreach(ItemDisplayInfo info in ItemDisplayInfos)
            {
                info.ItemButton.Opacity = Config.PieItemMouseDownOpacity;
            }

            dragged = false;

            // So apparently DragMove() is blocking.
            // That's how this only needs to be called once, and why the next line works.
            //
            // TODO: DragMove() is preventing the opacity from instantly changing if we're holding the mouse still. Fix.
            DragMove();

            InnerDisk.Opacity = Config.InnerDiskNormalOpacity;
            OuterDisk.Opacity = Config.OuterDiskNormalOpacity;

            foreach (ItemDisplayInfo info in ItemDisplayInfos)
            {
                info.ItemButton.Opacity = Config.PieItemNormalOpacity;
            }

            // Hacky way to handle mouse click on the inner disk.
            if (!dragged)
            {
                if (CurrentFolder.PrevFolder == null)
                {
                    ToggleVisibility();
                }
                else
                {
                    SwitchToFolder(CurrentFolder.PrevFolder);
                }
            }

            dragged = false;
        }

        private void OuterDisk_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: There is a lot of code duplication between the inner disk and outer disk methods. Fix.
            InnerDisk.Opacity = Config.InnerDiskMouseDownOpacity;
            OuterDisk.Opacity = Config.OuterDiskMouseDownOpacity;

            foreach (ItemDisplayInfo info in ItemDisplayInfos)
            {
                info.ItemButton.Opacity = Config.PieItemMouseDownOpacity;
            }

            DragMove();

            InnerDisk.Opacity = Config.InnerDiskNormalOpacity;
            OuterDisk.Opacity = Config.OuterDiskNormalOpacity;

            foreach (ItemDisplayInfo info in ItemDisplayInfos)
            {
                info.ItemButton.Opacity = Config.PieItemNormalOpacity;
            }
        }

        private struct ItemDisplayInfo
        {
            public PieItem Item { get; set; }
            public Button ItemButton { get; set; }
            public Label ItemLabel { get; set; }
            public bool MouseDown { get; set; } // If mouse holding onto it, but not necessarily dragging
            public bool Dragging { get; set; } // If the item has explicitly started moving

            public System.Windows.Point InitialMousePosition { get; set; }
        }
    }
}
