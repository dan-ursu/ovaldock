using System;
using System.Collections.Generic;
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
        private MainWindow TheMainWindow { get; }

        private ProgramSettingsWindow()
        {
            InitializeComponent();
        }

        // This is what we're always gonna be calling,
        // so that we can have changes show up in real time
        public ProgramSettingsWindow(MainWindow mainWindow) : this()
        {
            TheMainWindow = mainWindow;

            sliderInnerDiskRadius.Value           = Config.InnerRadius;
            sliderInnerDiskNormalOpacity.Value    = Config.InnerDiskNormalOpacity;
            sliderInnerDiskMouseDownOpacity.Value = Config.InnerDiskMouseDownOpacity;

            labelInnerDiskRadiusValue.Content           = Config.InnerRadius.ToString("0");
            labelInnerDiskNormalOpacityValue.Content    = Config.InnerDiskNormalOpacity.ToString("0.##");
            labelInnerDiskMouseDownOpacityValue.Content = Config.InnerDiskMouseDownOpacity.ToString("0.##");
        }

        private void sliderInnerDiskRadius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // This is VERY NECESSARY because apparently this method CAN and WILL get called before
            // the value of TheMainWindow is assigned in the constructor.
            // Even if we do the obvious and only call InitializeComponent() and everything else after
            // it has been assigned. Spooky.
            if (TheMainWindow == null)
                return;

            // TODO: Just change Config.InnerRadius into a double. Keeps things easier.
            //       Make sure that doesn't break anything also.
            Config.InnerRadius = sliderInnerDiskRadius.Value;

            labelInnerDiskRadiusValue.Content = Config.InnerRadius.ToString("0");

            // TODO: Use a property for InnerDisk, etc...
            TheMainWindow.InnerDisk.Width = 2 * Config.InnerRadius;
            TheMainWindow.InnerDisk.Height = 2 * Config.InnerRadius;
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
    }
}
