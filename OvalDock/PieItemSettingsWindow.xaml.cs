using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for PieItemSettingsWindow.xaml
    /// </summary>
    public partial class PieItemSettingsWindow : Window
    {
        // TODO: FIX THE BUG WHERE TEXTBOXES CAN EXPAND IF THEY CONTAIN TOO MUCH TEXT!

        // The window will "return" a new pie item to replace the old one.
        private PieItem OldPieItem { get; }
        public PieItem NewPieItem { get; private set; }
        public PieFolderItem ContainingFolder { get; private set; }
        public bool Saved { get; private set; }

        // TODO: There isn't anything stupid like 0xEEEEEE (colour used for background)
        //       being copyrighted by Microsoft, right?
        private PieItemSettingsWindow()
        {
            InitializeComponent();
        }

        /*
         * containingFolder is the folder that contains this PieItem.
         */
        public PieItemSettingsWindow(PieItem item, PieFolderItem containingFolder) : this()
        {
            OldPieItem = item;
            NewPieItem = item; // TODO: Feels mildly clunky. Rewrite?
            ContainingFolder = containingFolder;
            Saved = false;

            if (item.IsCustomName)
            {
                textBoxName.Text = (item.Name == null) ? "" : item.Name;
                checkBoxName.IsChecked = true;
                checkBoxName_Checked(null, null);
            }
            else
            {
                checkBoxName.IsChecked = false;
                checkBoxName_Unchecked(null, null);
            }

            if (item.IsCustomIcon)
            {
                textBoxIcon.Text = (item.Icon.ImagePath == null) ? "" : item.Icon.ImagePath;
                checkBoxIcon.IsChecked = true;
                checkBoxIcon_Checked(null, null);
            }
            else
            {
                checkBoxIcon.IsChecked = false;
                checkBoxIcon_Unchecked(null, null);
            }

            // Type-specific configuration.
            if (item is PieFolderItem)
            {
                radioButtonPieFolder.IsChecked = true;
                radioButtonPieFolder_Checked(null, null);
            }
            else if(item is FileItem)
            {
                switch (((FileItem)item).Type)
                {
                    case FileItemType.File:
                        radioButtonFile.IsChecked = true;
                        radioButtonFile_Checked(null, null);
                        break;

                    case FileItemType.Folder:
                        radioButtonFolder.IsChecked = true;
                        radioButtonFolder_Checked(null, null);
                        break;

                    case FileItemType.Other:
                        radioButtonOther.IsChecked = true;
                        radioButtonOther_Checked(null, null);
                        break;
                }

                textBoxTarget.Text = (((FileItem)item).FilePath == null) ? "" : ((FileItem)item).FilePath;
                textBoxArguments.Text = (((FileItem)item).Arguments == null) ? "" : ((FileItem)item).Arguments;
            }
        }

        private void checkBoxName_Checked(object sender, RoutedEventArgs e)
        {
            textBoxName.IsEnabled = true;
        }

        private void checkBoxName_Unchecked(object sender, RoutedEventArgs e)
        {
            textBoxName.IsEnabled = false;
        }

        private void checkBoxIcon_Checked(object sender, RoutedEventArgs e)
        {
            textBoxIcon.IsEnabled = true;
            buttonBrowseIcon.IsEnabled = true;
        }

        private void checkBoxIcon_Unchecked(object sender, RoutedEventArgs e)
        {
            textBoxIcon.IsEnabled = false;
            buttonBrowseIcon.IsEnabled = false;
        }

        private void radioButtonFile_Checked(object sender, RoutedEventArgs e)
        {
            labelTarget.Visibility = Visibility.Visible;
            textBoxTarget.Visibility = Visibility.Visible;
            buttonBrowseTarget.Visibility = Visibility.Visible;

            labelArguments.Visibility = Visibility.Visible;
            textBoxArguments.Visibility = Visibility.Visible;
        }

        private void radioButtonFolder_Checked(object sender, RoutedEventArgs e)
        {
            labelTarget.Visibility = Visibility.Visible;
            textBoxTarget.Visibility = Visibility.Visible;
            buttonBrowseTarget.Visibility = Visibility.Visible;

            labelArguments.Visibility = Visibility.Hidden;
            textBoxArguments.Visibility = Visibility.Hidden;
        }

        private void radioButtonOther_Checked(object sender, RoutedEventArgs e)
        {
            labelTarget.Visibility = Visibility.Visible;
            textBoxTarget.Visibility = Visibility.Visible;
            buttonBrowseTarget.Visibility = Visibility.Hidden;

            labelArguments.Visibility = Visibility.Hidden;
            textBoxArguments.Visibility = Visibility.Hidden;
        }

        private void radioButtonPieFolder_Checked(object sender, RoutedEventArgs e)
        {
            labelTarget.Visibility = Visibility.Hidden;
            textBoxTarget.Visibility = Visibility.Hidden;
            buttonBrowseTarget.Visibility = Visibility.Hidden;

            labelArguments.Visibility = Visibility.Hidden;
            textBoxArguments.Visibility = Visibility.Hidden;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Use nicer message boxes everywhere (not only in this class)?
            if(checkBoxIcon.IsChecked.HasValue
                && checkBoxIcon.IsChecked.Value == true
                && !File.Exists(textBoxIcon.Text))
            {
                MessageBox.Show("Icon does not exist");
                return;
            }

            //TODO: Might be worth being consistent with HasValue and != null

            // TODO: Creating a new object every time even if the type doesn't change is a bit clunky. Rewrite?

            // TODO: Warn about switching from a PieFolderItem to a non-PieFolderItem.
            //       Everything in that folder will be lost!
            //
            // TODO: Check to make sure nothing gets saved as null. Might cause some problems? Ex: null FileItem.Arguments?
                        
            // PieFolderItem is handled separately
            if (radioButtonPieFolder.IsChecked != null && radioButtonPieFolder.IsChecked.Value == true)
            {
                NewPieItem = new PieFolderItem(
                    checkBoxName.IsChecked == null ? false : checkBoxName.IsChecked.Value,
                    textBoxName.Text,
                    checkBoxIcon.IsChecked == null ? false : checkBoxIcon.IsChecked.Value,
                    textBoxIcon.Text,
                    ContainingFolder
                );

                // Do NOT lose the items in a PieFolderItem if we're starting/ending with a PieFolderItem
                if (OldPieItem is PieFolderItem)
                {
                    foreach (PieItem item in ((PieFolderItem)OldPieItem).Items)
                    {
                        ((PieFolderItem)NewPieItem).Items.Add(item);
                    }
                }
            }
            else // Everything else uses the FileItem class
            {
                // Placeholder value for now.
                FileItemType type = FileItemType.File;

                if(radioButtonFile.IsChecked.HasValue
                    && radioButtonFile.IsChecked == true)
                {
                    if (!File.Exists(textBoxTarget.Text))
                    {
                        MessageBox.Show("Target file does not exist.");
                        return;
                    }

                    type = FileItemType.File;
                }

                if(radioButtonFolder.IsChecked.HasValue
                    && radioButtonFolder.IsChecked == true)
                {
                    if (!Directory.Exists(textBoxTarget.Text))
                    {
                        MessageBox.Show("Target folder does not exist.");
                        return;
                    }

                    type = FileItemType.Folder;
                }

                if (radioButtonOther.IsChecked.HasValue
                    && radioButtonOther.IsChecked == true)
                {
                    type = FileItemType.Other;
                }

                NewPieItem = new FileItem(
                    checkBoxName.IsChecked == null ? false : checkBoxName.IsChecked.Value,
                    textBoxName.Text,
                    checkBoxIcon.IsChecked == null ? false : checkBoxIcon.IsChecked.Value,
                    textBoxIcon.Text,
                    textBoxTarget.Text,
                    textBoxArguments.Text,
                    type
                );
            }            

            Saved = true;

            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonBrowseIcon_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Check for valid extension.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                textBoxIcon.Text = openFileDialog.FileName;
            }
        }

        private void buttonBrowseTarget_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Browse for file or folder depending on which radio button is selected.
            if (radioButtonFolder.IsChecked.HasValue && radioButtonFolder.IsChecked == true)
            {
                // Unfortunately gotta use windows forms here. WPF has no folder browse dialog.
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        textBoxTarget.Text = dialog.SelectedPath;
                    }
                }
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();

                openFileDialog.DereferenceLinks = false;

                if (openFileDialog.ShowDialog() == true)
                {
                    textBoxTarget.Text = openFileDialog.FileName;
                }
            }
        }
    }
}
