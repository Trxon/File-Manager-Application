using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System;

namespace TotalCommanderProject
{
    public partial class MainWindow : Window
    {

        private string buttonTextCopy;
        private string buttonTextMove;

        #region Constructor

        // Prazan konstruktor(default)
        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region On Loaded

        // Kada se aplikacije pokrene . . .
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Dobijamo svaki logički disk na racunaru
            loading_all_data();

            buttonTextCopy = btnCopy.Content.ToString();
            buttonTextMove = btnMove.Content.ToString();
        }
        #endregion

        #region Getting all data (logical drives, folders, files...)
        private void loading_all_data()
        {
            FolderView.Items.Clear();
            FolderView2.Items.Clear();

            foreach (var drive in Directory.GetLogicalDrives())
            {
                // Kreiramo objekat tipa TreeViewItem
                var item = new TreeViewItem()
                {
                    // Setujemo header naseg objekta
                    Header = drive,
                    // Putanja
                    Tag = drive
                };

                // Kreiramo novi objekat tipa TreeViewItem, da bismo imali dve razlicita TreeView-a
                var item2 = new TreeViewItem()
                {
                    // Setujemo header naseg objekta
                    Header = drive,
                    // Putanja
                    Tag = drive
                };

                // Dodajemo tzv. dummy item, kako bismo imali strelicu
                item.Items.Add(null);
                item2.Items.Add(null);

                // za svaki objekat prosirimo njegove foldere pomocu metode Folder_Expanded()
                item.Expanded += Folder_Expanded;
                item2.Expanded += Folder_Expanded;

                // dodajemo TreeViewItem u TreeView, svaki item u svoj TreeView, kako bismo imali podelu
                FolderView.Items.Add(item);
                FolderView2.Items.Add(item2);
            }
        }
        #endregion

        #region Folder Expanded

        // kada se folder prosiri, trazimo njegove fajlove/foldere
        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            #region Initial Checks
            // početne provere

            // kastujemo nas objekat da bude tipa TreeViewItem
            var item = (TreeViewItem)sender;

            // Ako je nas objekat dummy item ili ako postoji samo jedan item, nema potrebe za daljim radom
            if (item.Items.Count != 1 || item.Items[0] != null)
                return;

            // da bismo se resili naseg dummy itema u folderu
            item.Items.Clear();

            // dobijamo celu putanju, koja je tipa String
            var fullPath = (string)item.Tag;

            #endregion

            #region Get Folders

            // kreiramo praznu listu u kojoj cuvamo nase direktorijume
            var directories = new List<string>();

            // pokusaj da dobijemo direktorijume iz foldera
            // smatramo da nece biti nikakvih gresaka usput, zato je catch block prazan
            try
            {
                var dirs = Directory.GetDirectories(fullPath);

                if (dirs.Length > 0)
                    directories.AddRange(dirs);
            }
            catch { }

            // za svaki direktorijum . . .
            directories.ForEach(directoryPath =>
            {
                // kreiramo objekat tipa TreeViewItem, koji predstavlja direktorijum
                var subItem = new TreeViewItem()
                {
                    // setujemo header koristeci metodu GetFileFolderName(String path)
                    Header = GetFileFolderName(directoryPath),
                    // putanja
                    Tag = directoryPath
                };

                // ponovno dodavanje dummy item-a, kako bismo prosirili item
                subItem.Items.Add(null);

                // ponovno pozivamo metodu, dokle god moze da se prosiri direktorijum
                subItem.Expanded += Folder_Expanded;

                // dodajemo direktorijum nasem poslatom objektu tipa TreeViewItem
                item.Items.Add(subItem);
            });

            #endregion

            #region Get Files

            // kreiramo praznu listu za nase fajlove
            var files = new List<string>();

            // pokusamo da dobijemo fajlove iz foldera
            // pretpostavimo da nece biti nikakvih izuzetaka, zato je catch block prazan
            try
            {
                var fs = Directory.GetFiles(fullPath);

                if (fs.Length > 0)
                    files.AddRange(fs);
            }
            catch { }

            // za svaki fajl . . .
            files.ForEach(filePath =>
            {
                // kreiramo objekat tipa TreeViewItem, koji predstavlja nas fajl
                var subItem = new TreeViewItem()
                {
                    // setujemo header koristeci metodu GetFileFolderName(String path)
                    Header = GetFileFolderName(filePath),
                    // putanja
                    Tag = filePath
                };

                // dodajemo fajl nasem poslatom objektu tipa TreeViewItem
                item.Items.Add(subItem);
            });

            #endregion
        }

        #endregion

        #region Helpers

        public static string GetFileFolderName(string path)
        {
            // ako nemamo putanju, vrati EMPTY
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            // sve kose crte (/) da budu crte unazad (\)
            var normalizedPath = path.Replace('/', '\\');

            // trazenje zadnje crte unazad u putanji
            var lastIndex = normalizedPath.LastIndexOf('\\');

            // ako ne nadejmo crtu unazad, vratimo nasu putanju (path)
            if (lastIndex <= 0)
                return path;

            // vratimo putanju(path) posle nase zadnje crte unazad
            return path.Substring(lastIndex + 1);
        }


        #endregion

        #region Button Copy
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            var tvi = (TreeViewItem)FolderView.SelectedItem;

            var tvi2 = (TreeViewItem)FolderView2.SelectedItem;

            if(tvi != null && tvi2 != null)
            {
                if (buttonTextCopy.Equals(btnCopy.Content))
                {
                    try
                    {
                        string fileName = (string)tvi.Header;
                        string sourcePath = (string)tvi.Tag;
                        sourcePath = sourcePath.Substring(0, sourcePath.Length - (fileName.Length + 1));
                        string sourceFile = Path.Combine(sourcePath, fileName);

                        string targetPath = (string)tvi2.Tag;
                        string targetFile = Path.Combine(targetPath, fileName);
                        File.Copy(sourceFile, targetFile);
                        MessageBox.Show("You successfully copied the file: "
                            + fileName + " to the desired destination: "
                            + targetPath,
                            "Successful operation", MessageBoxButton.OK, MessageBoxImage.Information);
                        tvi.IsSelected = false;
                        loading_all_data();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Copying was not done. Access to the file is not allowed",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show("Copying was not done. File was not found",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show("Copying was not done. No file selected!",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Copying was not done. Error found",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    try
                    {
                        string fileName = (string)tvi2.Header;
                        string sourcePath = (string)tvi2.Tag;
                        sourcePath = sourcePath.Substring(0, sourcePath.Length - (fileName.Length + 1));
                        string sourceFile = Path.Combine(sourcePath, fileName);

                        string targetPath = (string)tvi.Tag;
                        string targetFile = Path.Combine(targetPath, fileName);
                        File.Copy(sourceFile, targetFile);
                        MessageBox.Show("You successfully copied the file: "
                            + fileName + " to the desired destination: "
                            + targetPath,
                            "Successful operation", MessageBoxButton.OK, MessageBoxImage.Information);
                        tvi2.IsSelected = false;
                        loading_all_data();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Copying was not done. Access to the file is not allowed",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show("Copying was not done. File was not found",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show("Copying was not done. No file selected!",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Copying was not done. Error found",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

        }
        #endregion

        #region Button Delete
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var tvi = (TreeViewItem)FolderView.SelectedItem;
            string path;

            if (tvi != null)
            {
                path = (string)tvi.Tag;

                if (File.Exists(path))
                {
                    if (MessageBox.Show("Do you want to delete the file " + tvi.Header + "?",
                    "Deleting file",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            FolderView.Items.Remove(tvi);
                            File.Delete(path);
                            tvi.IsSelected = false;
                            MessageBox.Show("Successfully deleted " 
                                + (string)tvi.Header, "Successful operation", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            loading_all_data();
                        }
                        catch (IOException)
                        {
                            MessageBox.Show("You cannot remove the file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else if (Directory.Exists(path))
                {
                    if (MessageBox.Show("Do you want to delete directory " + tvi.Header + "?",
                    "Deleting directory",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Directory.Delete(path);
                            FolderView.Items.Remove(FolderView.SelectedItem);
                            tvi.IsSelected = false;
                            MessageBox.Show("Successfully deleted " + 
                                (string)tvi.Header, "Successful operation", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            loading_all_data();
                        }
                        catch (IOException)
                        {
                            MessageBox.Show("You do not have an access to the directory", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                tvi = (TreeViewItem)FolderView2.SelectedItem;

                if (tvi != null)
                {

                    path = (string)tvi.Tag;
                    if (File.Exists(path))
                    {
                        if (MessageBox.Show("Do you want to delete the file " + tvi.Header + "?",
                        "Deleting file",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            try
                            {
                                FolderView2.Items.Remove(tvi);
                                File.Delete(path);
                                tvi.IsSelected = false;
                                MessageBox.Show("Successfully deleted " + (string)tvi.Header, "Successful operation", MessageBoxButton.OK, MessageBoxImage.Information);
                                loading_all_data();
                            }
                            catch (IOException)
                            {
                                MessageBox.Show("You cannot remove the file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else if (Directory.Exists(path))
                    {
                        if (MessageBox.Show("Do you want to delete directory?",
                        "Deleting directory",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            try
                            {
                                Directory.Delete(path);
                                MessageBox.Show("Successfully deleted " + (string)tvi.Header, "Successful operation", MessageBoxButton.OK, MessageBoxImage.Information);
                                loading_all_data();
                            }
                            catch (IOException)
                            {
                                MessageBox.Show("You do not have access to the directory", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Button Rename
        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            var tvi = (TreeViewItem)FolderView.SelectedItem;

            if (tvi != null)
            {
                string path = (string)tvi.Tag;

                if (File.Exists(path))
                {
                    string newName = txtBoxRename.Text;
                    if (newName != null)
                    {
                        try
                        {
                            string fileName = (string)tvi.Header;
                            string tag = fileName.Substring(fileName.LastIndexOf('.'));
                            newName += tag;
                            string sourcePath = (string)tvi.Tag;
                            sourcePath = sourcePath.Substring(0, sourcePath.Length - (fileName.Length + 1));
                            string sourceFile = Path.Combine(sourcePath, fileName);

                            string targetFile = Path.Combine(sourcePath, newName);

                            File.Move(sourceFile, targetFile, true);
                            File.Delete(sourceFile);
                            tvi.IsSelected = false;
                            MessageBox.Show("File " + fileName + " was successfully renamed to: "
                                + newName, "Successful operation",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            txtBoxRename.Text = "";
                            loading_all_data();
                        }
                        catch (UnauthorizedAccessException)
                        {
                            MessageBox.Show("Renaming was not done. Access to the file is not allowed",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (FileNotFoundException)
                        {
                            MessageBox.Show("Renaming was not done. File was not found",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show("Renaming was not done. Error found",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }

            tvi = (TreeViewItem)FolderView2.SelectedItem;

            if(tvi != null)
            {
                string path = (string)tvi.Tag;

                if (File.Exists(path))
                {
                    string newName = txtBoxRename.Text;
                    if (newName != null)
                    {
                        try
                        {
                            string fileName = (string)tvi.Header;
                            string tag = fileName.Substring(fileName.LastIndexOf('.'));
                            newName += tag;
                            string sourcePath = (string)tvi.Tag;
                            sourcePath = sourcePath.Substring(0, sourcePath.Length - (fileName.Length + 1));
                            string sourceFile = Path.Combine(sourcePath, fileName);

                            string targetFile = Path.Combine(sourcePath, newName);

                            File.Move(sourceFile, targetFile, true);
                            File.Delete(sourceFile);
                            tvi.IsSelected = false;
                            MessageBox.Show("File " + fileName + " was successfully renamed to: " + newName, "Successful operation", MessageBoxButton.OK, MessageBoxImage.Information);
                            txtBoxRename.Text = "";
                            loading_all_data();
                        }
                        catch (UnauthorizedAccessException)
                        {
                            MessageBox.Show("Renaming was not done. Access to the file is not allowed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (FileNotFoundException)
                        {
                            MessageBox.Show("Renaming was not done. File was not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show("Renaming was not done. Error found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
        #endregion

        #region Button Move
        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            var tvi = (TreeViewItem)FolderView.SelectedItem;

            var tvi2 = (TreeViewItem)FolderView2.SelectedItem;

            if (tvi != null && tvi2 != null)
            {
                if(buttonTextMove.Equals(btnMove.Content))
                {
                    try
                    {
                        string fileName = (string)tvi.Header;
                        string sourcePath = (string)tvi.Tag;
                        sourcePath = sourcePath.Substring(0, sourcePath.Length - (fileName.Length + 1));
                        string sourceFile = Path.Combine(sourcePath, fileName);

                        string targetPath = (string)tvi2.Tag;
                        string targetFile = Path.Combine(targetPath, fileName);
                        File.Move(sourceFile, targetFile, true);
                        MessageBox.Show("You successfully moved the file: "
                            + fileName + " to the desired destination: "
                            + targetPath, "Successful operation",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        tvi.IsSelected = false;
                        loading_all_data();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Moving was not done. Access to the file is not allowed",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show("Moving was not done. File was not found",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show("Copying was not done. No file selected!",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Moving was not done. Error found",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    try
                    {
                        string fileName = (string)tvi2.Header;
                        string sourcePath = (string)tvi2.Tag;
                        sourcePath = sourcePath.Substring(0, sourcePath.Length - (fileName.Length + 1));
                        string sourceFile = Path.Combine(sourcePath, fileName);

                        string targetPath = (string)tvi.Tag;
                        string targetFile = Path.Combine(targetPath, fileName);
                        File.Move(sourceFile, targetFile, true);
                        MessageBox.Show("You successfully moved the file: "
                            + fileName + " to the desired destination: "
                            + targetPath, "Successful operation",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        tvi2.IsSelected = false;
                        loading_all_data();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Moving was not done. Access to the file is not allowed",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show("Moving was not done. File was not found",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show("Copying was not done. No file selected!",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Moving was not done. Error found",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        #endregion

        #region TreeViewFocus
        private void FolderView_GotFocus(object sender, RoutedEventArgs e)
        {
            btnCopy.Content = buttonTextCopy;
            btnMove.Content = buttonTextMove;
        }
        #endregion

        #region TreeView2Focus
        private void FolderView2_GotFocus(object sender, RoutedEventArgs e)
        {
            btnCopy.Content = "<--" + buttonTextCopy.Substring(0, 4);
            btnMove.Content = "<--" + buttonTextMove.Substring(0, 4);
        }
        #endregion
    }
}
