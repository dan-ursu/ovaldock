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

        private bool dragged = false;

        // TODO: Probably more hassle than it's worth, but should this second argument be the inner circle icon somehow?
        private PieFolderItem rootFolder;
        private PieFolderItem currentFolder;

        private List<Button> itemButtons;
        private List<KeyValuePair<Button, Label>> itemLabels;

        // Unfortunately have to rely on Windows Forms for this.
        // .csproj was modified to use Windows Forms as well.
        private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();

            // TODO: Organize this better? Or rename LoadConfig() to LoadProgramConfig()?
            Config.LoadConfig();
            rootFolder = Config.LoadItems();

            currentFolder = rootFolder;

            itemButtons = new List<Button>();
            itemLabels = new List<KeyValuePair<Button, Label>>();

            ResizeWindow();

            CreateOuterDisk();
            CreateInnerDisk();

            mainWindow.LocationChanged += MainWindow_LocationChanged;

            CreateNotifyIcon();

            PreloadIconsRecursive(rootFolder);
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
        private void PreloadIconsRecursive(PieFolderItem folder)
        {
            // Force each icon to load (at least) once.
            // Preloading is handled in the IconAsBitmapSource property.
            //
            // No need to worry about "preloading twice".
            // The second time, it has already been preloaded.
            BitmapSource temp;

            temp = folder.IconAsBitmapSource;

            foreach (PieItem item in folder.Items)
            {
                temp = item.IconAsBitmapSource;
            }

            foreach (PieItem item in folder.Items)
            {
                if (item is PieFolderItem)
                {
                    PreloadIconsRecursive((PieFolderItem)item);
                }
            }
        }

        private void ResizeWindow()
        {
            // Plenty of space, so that labels can go quite a bit out of "bounds"
            mainWindow.Width = 3 * Config.OuterRadius;
            mainWindow.Height = 3 * Config.OuterRadius;

            mainGrid.Width = 3 * Config.OuterRadius;
            mainGrid.Height = 3 * Config.OuterRadius;
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
                    Close();
                };

            System.Windows.Forms.ToolStripMenuItem settingsItem = new System.Windows.Forms.ToolStripMenuItem();
            settingsItem.Text = "Settings";
            settingsItem.Click +=
                (s, e) =>
                {
                    // TODO: Handle previewing of settings somehow.
                    ProgramSettingsWindow settingsWindow = new ProgramSettingsWindow(this);
                    settingsWindow.ShowDialog();
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
            var outerDisk = new System.Windows.Controls.Image();

            //outerDisk.Source = new BitmapImage(new Uri(outerDiskImagePath));
            outerDisk.Source = Util.ToBitmapImage(new Bitmap(Config.OuterDiskImagePath));
            outerDisk.Width = 2 * Config.OuterRadius;
            outerDisk.Height = 2 * Config.OuterRadius;

            //var margin = new Thickness();
            //margin.Left = 0;
            //margin.Top = 0;
            //outerDisk.Margin = margin;

            outerDisk.HorizontalAlignment = HorizontalAlignment.Center;
            outerDisk.VerticalAlignment = VerticalAlignment.Center;

            //outerDisk.MouseRightButtonUp += OuterDisk_MouseRightButtonUp;
            outerDisk.MouseLeftButtonDown += OuterDisk_MouseLeftButtonDown;

            // Create the context menu
            MenuItem addMenuItem = new MenuItem();
            addMenuItem.Header = "Add";
            addMenuItem.Click +=
                (s, e) =>
                {
                    // TODO: This feels inefficient, but PieItemSettingsWindow(null, ...) feels like something might break
                    PieItem newItem = new FileItem(false, null, false, null, null, null, FileItemType.File);
                    PieItemSettingsWindow pieItemSettingsWindow = new PieItemSettingsWindow(newItem, currentFolder);
                    pieItemSettingsWindow.ShowDialog();

                    if (pieItemSettingsWindow.Saved)
                    {
                        currentFolder.Items.Add(pieItemSettingsWindow.NewPieItem);
                        RefreshFolder();
                        Config.SaveItems(rootFolder);
                    }
                };

            outerDisk.ContextMenu = new ContextMenu();
            outerDisk.ContextMenu.Items.Add(addMenuItem);

            mainGrid.Children.Add(outerDisk);
        }

        private void CreateInnerDisk()
        {
            InnerDisk = new System.Windows.Controls.Image();

            // innerDisk.Source = ToBitmapImage(new Bitmap(Config.InnerDiskImagePath));
            InnerDisk.Source = currentFolder.IconAsBitmapSource;

            InnerDisk.Opacity = Config.InnerDiskNormalOpacity;

            InnerDisk.Width = 2 * Config.InnerRadius;
            InnerDisk.Height = 2 * Config.InnerRadius;

            InnerDisk.HorizontalAlignment = HorizontalAlignment.Center;
            InnerDisk.VerticalAlignment = VerticalAlignment.Center;

            InnerDisk.MouseLeftButtonDown += InnerDisk_MouseLeftButtonDown;

            mainGrid.Children.Add(InnerDisk);
        }

        // Add items based on folder's items.
        // Also change center icon to folder's icon.
        public void SwitchToFolder(PieFolderItem folder)
        {
            ClearItems();

            currentFolder = folder;

            // Add items
            for (int i = 0; i < folder.Items.Count; i++)
            {
                AddPieItem(folder.Items[i], i, folder.Items.Count);
            }

            // Change center icon
            InnerDisk.Source = folder.IconAsBitmapSource;
        }

        // To be used if an item was changed in this folder.
        private void RefreshFolder()
        {
            SwitchToFolder(currentFolder);
        }

        private void ClearItems()
        {
            foreach (Button button in itemButtons)
            {
                mainGrid.Children.Remove(button);
            }

            itemButtons.Clear();

            // This might be slightly unnecessary, but might as well remove labels for extra safety.
            // I suspect there will never be any labels anyways though.
            foreach (var kvp in itemLabels)
            {
                mainGrid.Children.Remove(kvp.Value);
            }

            itemLabels.Clear();
        }

        // This is a bit more complicated.
        // We have to account for all the stuff that hovering over/pressing/etc... on an item does.
        private void AddPieItem(PieItem pieItem, int number, int totalItems)
        {
            var itemImage = new System.Windows.Controls.Image();
            itemImage.Source = pieItem.IconAsBitmapSource;


            var itemButton = new Button();

            itemButton.Content = itemImage;
            itemButton.Width = Config.PieItemSize;
            itemButton.Height = Config.PieItemSize;

            // To be spread evenly in a circle.
            var buttonMargin = new Thickness();
            buttonMargin.Left = Config.PieItemRadiusFromCenter * Math.Cos(number * 2 * Math.PI / totalItems);
            buttonMargin.Top = Config.PieItemRadiusFromCenter * Math.Sin(number * 2 * Math.PI / totalItems);
            itemButton.Margin = buttonMargin;

            itemButton.HorizontalAlignment = HorizontalAlignment.Center;
            itemButton.VerticalAlignment = VerticalAlignment.Center;

            itemButton.Background = System.Windows.Media.Brushes.Transparent;
            itemButton.BorderBrush = System.Windows.Media.Brushes.Transparent;


            // All the button behaviour handled here.
            itemButton.Click +=
                (s, e) =>
                {
                    pieItem.LeftClick(this);
                };

            itemButton.MouseEnter +=
                (s, e) =>
                {
                    // Add a label corresponding to the button.
                    var label = new System.Windows.Controls.Label();

                    var labelMargin = new Thickness();
                    labelMargin.Left = itemButton.Margin.Left;
                    labelMargin.Top = itemButton.Margin.Top + Config.PieItemSize + Config.PieItemLabelPadding; // TODO: Figure out proper padding.

                    label.Margin = labelMargin;

                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.VerticalAlignment = VerticalAlignment.Center;

                    label.Content = (pieItem.Name == null) ? "" : pieItem.Name;

                    label.FontSize = Config.PieItemLabelSize;

                    itemLabels.Add(new KeyValuePair<Button, Label>(itemButton, label));
                    mainGrid.Children.Add(label);
                };

            itemButton.MouseLeave +=
                (s, e) =>
                {
                    // Delete the label corresponding to the button.
                    foreach (KeyValuePair<Button, Label> itemLabel in itemLabels)
                    {
                        if (itemLabel.Key == itemButton)
                        {
                            mainGrid.Children.Remove(itemLabel.Value);
                        }
                    }

                    // I suspect this might be a memory leak if we don't include this line?
                    itemLabels.RemoveAll((kvp) => { return kvp.Key == itemButton; });
                };

            // Create the context menu
            MenuItem settingsMenuItem = new MenuItem();
            settingsMenuItem.Header = "Settings";
            settingsMenuItem.Click +=
                (s, e) =>
                {
                    PieItemSettingsWindow pieItemSettingsWindow = new PieItemSettingsWindow(pieItem, currentFolder);
                    pieItemSettingsWindow.ShowDialog();

                    // Replace the old pieItem with the new one
                    if (pieItemSettingsWindow.Saved)
                    {
                        int i = currentFolder.Items.IndexOf(pieItem);

                        // You know, I feel like this should never happen
                        if (i == -1)
                            return;

                        currentFolder.Items[i] = pieItemSettingsWindow.NewPieItem;
                        RefreshFolder();
                        Config.SaveItems(rootFolder);
                    }
                };

            MenuItem removeMenuItem = new MenuItem();
            removeMenuItem.Header = "Remove";
            removeMenuItem.Click +=
                (s, e) =>
                {
                    currentFolder.Items.Remove(pieItem);
                    RefreshFolder();
                    Config.SaveItems(rootFolder);
                };

            itemButton.ContextMenu = new ContextMenu();
            itemButton.ContextMenu.Items.Add(settingsMenuItem);
            itemButton.ContextMenu.Items.Add(removeMenuItem);


            //TODO: Figure out transparency of background/border on hover.

            itemButtons.Add(itemButton);
            mainGrid.Children.Add(itemButton);
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            dragged = true;
        }

        // TODO: Use a button for the inner disk instead of an image? Can't currently figure out buttons with a non-square border.
        private void InnerDisk_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            InnerDisk.Opacity = Config.InnerDiskMouseDownOpacity;

            dragged = false;

            // So apparently DragMove() is blocking.
            // That's how this only needs to be called once, and why the next line works.
            //
            // TODO: DragMove() is preventing the opacity from instantly changing if we're holding the mouse still. Fix.
            DragMove();

            InnerDisk.Opacity = Config.InnerDiskNormalOpacity;

            // Hacky way to handle mouse click on the inner disk.
            if (!dragged)
            {
                if (currentFolder.PrevFolder == null)
                {
                    ToggleVisibility();
                }
                else
                {
                    SwitchToFolder(currentFolder.PrevFolder);
                }
            }

            dragged = false;
        }

        private void OuterDisk_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
