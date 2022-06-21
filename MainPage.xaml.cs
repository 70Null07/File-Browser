using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace File_Browser
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        StorageFile file;

        public MainPage()
        {
            this.InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(720, 1020);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Files.Items.Clear();
            Folders.Items.Clear();

            if (OpenFilePath.Text != String.Empty)
            {
                file = await StorageFile.GetFileFromPathAsync(OpenFilePath.Text);
            }
            else
            {
                var openPicker = new FileOpenPicker();
                var openPicker1 = new FolderPicker();

                openPicker.ViewMode = PickerViewMode.Thumbnail;

                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                openPicker.CommitButtonText = "Открыть";

                openPicker.FileTypeFilter.Add("*");

                file = await openPicker.PickSingleFileAsync();
            }

            var basicProperties = await file.GetBasicPropertiesAsync();

            FilePath.Text = file.Path.Remove(file.Path.Length - file.Name.Length);

            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(file.Path.Remove(file.Path.Length - file.Name.Length));

            IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();

            foreach (StorageFile file1 in fileList)
            {
                Button button = new Button()
                {
                    Content = file1.Name,
                };
                Files.Items.Add(button);
            }

            IReadOnlyList<StorageFolder> folderList = await folder.GetFoldersAsync();

            foreach (StorageFolder folder1 in folderList)
            {
                Button button = new Button()
                {
                    Content = folder1.Name,
                };
                Folders.Items.Add(button);
            }

            FileName.Text = file.Name;
            FileSize.Text = basicProperties.Size.ToString();
            FileTimeCreated.Text = file.DateCreated.ToString();
            FileLastModify.Text = basicProperties.DateModified.ToString();

            var propertynames = new List<string>();
            propertynames.Add("System.DateAccessed");

            IDictionary<string, object> extraProperties = await file.Properties.RetrievePropertiesAsync(propertynames);

            FileLastAccess.Text = extraProperties["System.DateAccessed"].ToString();
        }

        private async void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (file != default)
                {
                    ContentDialog contentDialog = new ContentDialog()
                    {
                        Title = "Удалить файл?",
                        Content = "Вы уверены что хотите удалить этот файл? (действие невозможно отменить)",
                        PrimaryButtonText = "Удалить",
                        SecondaryButtonText = "Отмена",
                    };

                    var result = await contentDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        await file.DeleteAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderPicker folderPicker = new FolderPicker();

                folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                folderPicker.FileTypeFilter.Add("*");

                var folder = await folderPicker.PickSingleFolderAsync();

                await file.CopyAsync(folder);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderPicker folderPicker = new FolderPicker();

                folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                folderPicker.FileTypeFilter.Add("*");


                var folder = await folderPicker.PickSingleFolderAsync();

                await file.MoveAsync(folder);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void NavigateUpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (file != default)
                {
                    string[] splitFolder = FilePath.Text.Split("\\");
                    string newFolder = splitFolder[0];

                    for (int i = 1; i < splitFolder.Length - 2; i++)
                        newFolder += "\\" + splitFolder[i];

                    var folder = await StorageFolder.GetFolderFromPathAsync(newFolder);

                    Files.Items.Clear();
                    Folders.Items.Clear();

                    IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();

                    foreach (StorageFile file1 in fileList)
                    {
                        Button button = new Button()
                        {
                            Content = file1.Name,
                        };
                        Files.Items.Add(button);
                    }

                    IReadOnlyList<StorageFolder> folderList = await folder.GetFoldersAsync();

                    foreach (StorageFolder folder1 in folderList)
                    {
                        Button button = new Button()
                        {
                            Content = folder1.Name,
                        };
                        Folders.Items.Add(button);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void Files_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Button selectedItem = Files.SelectedItem as Button;

            if (selectedItem != null)
            {

                Files.Items.Clear();
                Folders.Items.Clear();

                file = await StorageFile.GetFileFromPathAsync(FilePath.Text + selectedItem.Content);

                var basicProperties = await file.GetBasicPropertiesAsync();

                FilePath.Text = file.Path.Remove(file.Path.Length - file.Name.Length);

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(file.Path.Remove(file.Path.Length - file.Name.Length));

                IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();

                foreach (StorageFile file1 in fileList)
                {
                    Button button = new Button()
                    {
                        Content = file1.Name,
                    };
                    Files.Items.Add(button);
                }

                IReadOnlyList<StorageFolder> folderList = await folder.GetFoldersAsync();

                foreach (StorageFolder folder1 in folderList)
                {
                    Button button = new Button()
                    {
                        Content = folder1.Name,
                    };
                    Folders.Items.Add(button);
                }

                FileName.Text = file.Name;
                FileSize.Text = basicProperties.Size.ToString();
                FileTimeCreated.Text = file.DateCreated.ToString();
                FileLastModify.Text = basicProperties.DateModified.ToString();

                var propertynames = new List<string>();
                propertynames.Add("System.DateAccessed");

                IDictionary<string, object> extraProperties = await file.Properties.RetrievePropertiesAsync(propertynames);

                FileLastAccess.Text = extraProperties["System.DateAccessed"].ToString();
            }
        }
    }
}
