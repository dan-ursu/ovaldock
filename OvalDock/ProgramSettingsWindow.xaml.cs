using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OvalDock
{
    /// <summary>
    /// Interaction logic for ProgramSettingsWindow.xaml
    /// </summary>
    public partial class ProgramSettingsWindow : Window
    {
        // TODO: At some point, make the stuff in Config.cs not static,
        //       and use properties in Config.cs to update the instance of TheMainWindow we are working with.
        private MainWindow TheMainWindow { get; }

        // Used to make sure we don't have multiple windows open at once.
        public static bool IsWindowActive { get; private set; }

        private ProgramSettingsWindow()
        {
            InitializeComponent();
        }

        // This is what we're always gonna be calling,
        // so that we can have changes show up in real time
        public ProgramSettingsWindow(MainWindow mainWindow) : this()
        {
            TheMainWindow = mainWindow;

            IsWindowActive = true;

            sliderInnerDiskRadius.Value           = Config.InnerRadius;
            sliderInnerDiskNormalOpacity.Value    = Config.InnerDiskNormalOpacity;
            sliderInnerDiskMouseDownOpacity.Value = Config.InnerDiskMouseDownOpacity;

            labelInnerDiskRadiusValue.Content           = Config.InnerRadius.ToString("0");
            labelInnerDiskNormalOpacityValue.Content    = Config.InnerDiskNormalOpacity.ToString("0.##");
            labelInnerDiskMouseDownOpacityValue.Content = Config.InnerDiskMouseDownOpacity.ToString("0.##");

            textBoxInnerDiskIcon.Text = Config.InnerDiskImagePath;

            sliderOuterDiskRadius.Value           = Config.OuterRadius;
            sliderOuterDiskNormalOpacity.Value    = Config.OuterDiskNormalOpacity;
            sliderOuterDiskMouseDownOpacity.Value = Config.OuterDiskMouseDownOpacity;

            labelOuterDiskRadiusValue.Content           = Config.OuterRadius.ToString("0");
            labelOuterDiskNormalOpacityValue.Content    = Config.OuterDiskNormalOpacity.ToString("0.##");
            labelOuterDiskMouseDownOpacityValue.Content = Config.OuterDiskMouseDownOpacity.ToString("0.##");

            textBoxOuterDiskIcon.Text = Config.OuterDiskImagePath;

            textBoxItemFileNotFoundIcon.Text = Config.FileNotFoundIcon.ImagePath;
            textBoxItemFolderDefaultIcon.Text = Config.FolderDefaultIcon.ImagePath;

            sliderItemSize.Value             = Config.PieItemSize;
            sliderItemNormalOpacity.Value    = Config.PieItemNormalOpacity;
            sliderItemMouseDownOpacity.Value = Config.PieItemMouseDownOpacity;
            sliderItemRadiusFromCenter.Value = Config.PieItemRadiusFromCenter;

            labelItemSizeValue.Content             = Config.PieItemSize.ToString("0");
            labelItemNormalOpacityValue.Content    = Config.PieItemNormalOpacity.ToString("0.##");
            labelItemMouseDownOpacityValue.Content = Config.PieItemMouseDownOpacity.ToString("0.##");
            labelItemRadiusFromCenterValue.Content = Config.PieItemRadiusFromCenter.ToString("0");
        }

        private void sliderInnerDiskRadius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // This is VERY NECESSARY because apparently this method CAN and WILL get called before
            // the value of TheMainWindow is assigned in the constructor.
            // Even if we do the obvious and only call InitializeComponent() and everything else after
            // it has been assigned. Spooky.
            if (TheMainWindow == null)
                return;

            Config.InnerRadius = sliderInnerDiskRadius.Value;

            labelInnerDiskRadiusValue.Content = Config.InnerRadius.ToString("0");

            TheMainWindow.InnerDisk.Width = 2 * Config.InnerRadius;
            TheMainWindow.InnerDisk.Height = 2 * Config.InnerRadius;

            // TODO: Fix resizing the window being jittery if the window dimensions actually end up changing.
            TheMainWindow.ResizeWindow();
        }

        private void sliderInnerDiskNormalOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TheMainWindow == null)
                return;

            Config.InnerDiskNormalOpacity = sliderInnerDiskNormalOpacity.Value;

            labelInnerDiskNormalOpacityValue.Content = Config.InnerDiskNormalOpacity.ToString("0.##");

            TheMainWindow.InnerDisk.Opacity = Config.InnerDiskNormalOpacity;
        }

        private void sliderInnerDiskMouseDownOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TheMainWindow == null)
                return;

            Config.InnerDiskMouseDownOpacity = sliderInnerDiskMouseDownOpacity.Value;

            labelInnerDiskMouseDownOpacityValue.Content = Config.InnerDiskMouseDownOpacity.ToString("0.##");
        }

        private void buttonBrowseInnerDiskIcon_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Check for valid extension
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Bitmap rootFolderBitmap;

                try
                {
                    rootFolderBitmap = new Bitmap(openFileDialog.FileName);
                }
                catch(Exception exception)
                {
                    MessageBox.Show("Could not load icon.");
                    return;
                }

                // Valid image from here on.
                Config.InnerDiskImagePath = openFileDialog.FileName;

                TheMainWindow.RootFolder.Icon.ImagePath = Config.InnerDiskImagePath;
                TheMainWindow.RootFolder.Icon.ImageBitmap = rootFolderBitmap; // We've already preloaded. Why not?
                
                textBoxInnerDiskIcon.Text = Config.InnerDiskImagePath;

                if(TheMainWindow.CurrentFolder == TheMainWindow.RootFolder)
                {
                    TheMainWindow.RefreshFolder();
                }
            }
        }

        private void sliderOuterDiskRadius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TheMainWindow == null)
                return;

            Config.OuterRadius = sliderOuterDiskRadius.Value;

            labelOuterDiskRadiusValue.Content = Config.OuterRadius.ToString("0");

            TheMainWindow.OuterDisk.Width = 2 * Config.OuterRadius;
            TheMainWindow.OuterDisk.Height = 2 * Config.OuterRadius;

            TheMainWindow.ResizeWindow();
        }

        private void sliderOuterDiskNormalOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TheMainWindow == null)
                return;

            Config.OuterDiskNormalOpacity = sliderOuterDiskNormalOpacity.Value;

            labelOuterDiskNormalOpacityValue.Content = Config.OuterDiskNormalOpacity.ToString("0.##");

            TheMainWindow.OuterDisk.Opacity = Config.OuterDiskNormalOpacity;
        }

        private void sliderOuterDiskMouseDownOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TheMainWindow == null)
                return;

            Config.OuterDiskMouseDownOpacity = sliderOuterDiskMouseDownOpacity.Value;

            labelOuterDiskMouseDownOpacityValue.Content = Config.OuterDiskMouseDownOpacity.ToString("0.##");
        }

        private void buttonBrowseOuterDiskIcon_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Check for valid extension? Or nah?
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Bitmap outerDiskBitmap;

                try
                {
                    outerDiskBitmap = new Bitmap(openFileDialog.FileName);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Could not load icon.");
                    return;
                }

                // Valid image from here on.
                Config.OuterDiskImagePath = openFileDialog.FileName;

                TheMainWindow.OuterDisk.Source = Util.ToBitmapImage(outerDiskBitmap);

                textBoxOuterDiskIcon.Text = Config.OuterDiskImagePath;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            IsWindowActive = false;
            Config.SaveProgramSettings();
        }

        private void buttonItemFileNotFoundIcon_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Check for valid extension? Or nah?
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Bitmap bitmap;

                try
                {
                    bitmap = new Bitmap(openFileDialog.FileName);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Could not load icon.");
                    return;
                }

                string oldPath = Config.FileNotFoundIcon.ImagePath;

                Config.FileNotFoundIcon.ImagePath = openFileDialog.FileName;
                Config.FileNotFoundIcon.ImageBitmap = bitmap; // Again, already preloaded.
                Config.FileNotFoundIcon.CreateCache(); // Just in case it doesn't get preloaded before refreshing the icons.
                                                       // But, to my knowledge, it should.

                textBoxItemFileNotFoundIcon.Text = Config.FileNotFoundIcon.ImagePath;

                TheMainWindow.ClearCachedImagesFileNotFound(TheMainWindow.RootFolder, oldPath, Config.FileNotFoundIcon.ImagePath);
                TheMainWindow.PreloadIconsRecursive(TheMainWindow.RootFolder); // Icons outside the folder could be modified
                TheMainWindow.RefreshFolder();
            }
        }

        private void buttonItemFolderDefaultIcon_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Check for valid extension? Or nah?
            // TODO: There's also a lot of code duplication here. Fix.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Bitmap bitmap;

                try
                {
                    bitmap = new Bitmap(openFileDialog.FileName);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Could not load icon.");
                    return;
                }

                string oldPath = Config.FolderDefaultIcon.ImagePath;

                Config.FolderDefaultIcon.ImagePath = openFileDialog.FileName;
                Config.FolderDefaultIcon.ImageBitmap = bitmap; // Again, already preloaded.
                Config.FolderDefaultIcon.CreateCache(); // Just in case it doesn't get preloaded before refreshing the icons.
                                                        // But, to my knowledge, it should.

                textBoxItemFolderDefaultIcon.Text = Config.FolderDefaultIcon.ImagePath;

                TheMainWindow.ClearCachedImagesDefaultFolder(TheMainWindow.RootFolder, oldPath, Config.FolderDefaultIcon.ImagePath);
                TheMainWindow.PreloadIconsRecursive(TheMainWindow.RootFolder); // Icons outside the folder could be modified
                TheMainWindow.RefreshFolder();
            }
        }

        private void sliderItemSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TheMainWindow == null)
                return;

            Config.PieItemSize = sliderItemSize.Value;

            labelItemSizeValue.Content = Config.PieItemSize.ToString("0");

            TheMainWindow.UpdateItemAppearance();
        }

        private void sliderItemNormalOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TheMainWindow == null)
                return;

            Config.PieItemNormalOpacity = sliderItemNormalOpacity.Value;

            labelItemNormalOpacityValue.Content = Config.PieItemNormalOpacity.ToString("0.##");

            TheMainWindow.UpdateItemAppearance();
        }

        private void sliderItemMouseDownOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TheMainWindow == null)
                return;

            Config.PieItemMouseDownOpacity = sliderItemMouseDownOpacity.Value;

            labelItemMouseDownOpacityValue.Content = Config.PieItemMouseDownOpacity.ToString("0.##");
        }

        private void sliderItemRadiusFromCenter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TheMainWindow == null)
                return;

            Config.PieItemRadiusFromCenter = sliderItemRadiusFromCenter.Value;

            labelItemRadiusFromCenterValue.Content = Config.PieItemRadiusFromCenter.ToString("0");

            TheMainWindow.UpdateItemAppearance();
        }
    }
}
