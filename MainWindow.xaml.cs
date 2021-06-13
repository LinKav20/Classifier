using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace peer10
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Path to current selected item.
        string curPath = "";
        // Name of the file to save.
        string nameOfSavingFile = "all.json";
        // Selected item in treeView.
        TreeViewItem selectedTVI;
        // All items we have.
        List<Item> items;
        // Flag to show all items.
        public bool isAllItemsBool = false;
        // Flag to sort all items.
        public bool sortByCodeBool = false;
        // Flag to autosave all items.
        public bool autoSaveBool = false;
        /// <summary>
        /// Default construstor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            StartSettings();
            LoadAllData();
            FillTree();
        }
        /// <summary>
        /// Set started options for some textboxes and buttons.
        /// </summary>
        void StartSettings()
        {
            items = new List<Item>();
            addItem.IsEnabled = false;
            addFolder.IsEnabled = true;
            itemName.IsEnabled = false;
            articulItem.IsEnabled = false;
            countItems.IsEnabled = false;
            priceItem.IsEnabled = false;
            folderName.IsEnabled = true;
            wantToCreateFolder.IsChecked = true;
            changeFolder.IsEnabled = true;
            changeItem.IsEnabled = false;
            deleteFolder.IsEnabled = true;
            deleteItem.IsEnabled = false;
        }
        /// <summary>
        /// Check if it is the folder`s path.
        /// </summary>
        /// <param name="s">Path to check.</param>
        /// <returns>True if is folder`s path, else false.</returns>
        public bool IsFolder(string s)
        {
            return s.Last() == '/';
        }
        /// <summary>
        /// Changing selected item and reset option.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!(tree.SelectedItem is null))
            {
                selectedTVI = (TreeViewItem)tree.SelectedItem;
                curPath = ((TreeViewItem)tree.SelectedItem).Tag.ToString();
                path.Content = curPath;
                // Check if the folder selcted.
                if (IsFolder(curPath))
                {
                    // Reset option depended in selected item.
                    if ((bool)wantToCreateFolder.IsChecked)
                    {
                        if (curPath != "root/")
                        {
                            changeFolder.IsEnabled = true;
                            deleteFolder.IsEnabled = true;
                            sortCodeFolder.Text = FindItem(curPath).SortCode;
                        }
                        else
                        {
                            changeItem.IsEnabled = false;
                            changeFolder.IsEnabled = false;
                            deleteFolder.IsEnabled = false;
                            deleteItem.IsEnabled = false;
                            sortCodeFolder.Text = "";
                        }
                    }
                    if (!(bool)wantToCreateFolder.IsChecked)
                    {
                        changeFolder.IsEnabled = false;
                        changeItem.IsEnabled = false;
                        deleteFolder.IsEnabled = false;
                        sortCodeFolder.IsEnabled = false;
                        deleteItem.IsEnabled = false;
                    }
                }
                else
                {
                    if ((bool)wantToCreateFolder.IsChecked)
                    {
                        changeFolder.IsEnabled = false;
                        changeItem.IsEnabled = false;
                        deleteFolder.IsEnabled = false;
                        deleteItem.IsEnabled = false;
                    }
                    if (!(bool)wantToCreateFolder.IsChecked)
                    {
                        changeFolder.IsEnabled = false;
                        changeItem.IsEnabled = true;
                        deleteFolder.IsEnabled = false;
                        deleteItem.IsEnabled = true;
                    }
                }
                itemName.Text = "";
                articulItem.Text ="";
                priceItem.Text = "";
                countItems.Text = "";
                if (!IsFolder(curPath))
                {
                    var item = FindItem(curPath);
                    itemName.Text = item.Name;
                    articulItem.Text = item.Code;
                    priceItem.Text = item.Price.ToString();
                    countItems.Text = item.Count.ToString();
                }
                // Refill the listView items.
                FillTheListView();
            }
        }
        /// <summary>
        /// Fill  the listView.
        /// </summary>
        void FillTheListView()
        {
            // This works only if folder selected.
            if (IsFolder(curPath))
            {
                // Clear all.
                itemsView.Items.Clear();
                foreach (var i in items)
                {
                    // If need to show all tems in subfolders.
                    if (isAllItemsBool)
                    {
                        // Check if it is not a folder and located in selected folder.
                        if (!IsFolder(i.Path) && i.Path.Contains(curPath))
                            // Add the item.
                            itemsView.Items.Add(new Item
                            {
                                // How deep inside is this item.
                                Level = i.Path.Split('/').Length - curPath.Split('/').Length,
                                Name = i.Name,
                                Code = i.Code,
                                Count = i.Count,
                                Price = i.Price
                            });
                    }
                    else
                    {
                        // Check if it is not a folder and located ONLY in selected folder.
                        if (!IsFolder(i.Path) && ContainInSelectedFolder(i))
                            itemsView.Items.Add(new Item
                            {
                                Level = 0,
                                Name = i.Name,
                                Code = i.Code,
                                Count = i.Count,
                                Price = i.Price
                            });
                    }
                }
            }
        }
        /// <summary>
        /// Check if this item located inside the folder, not in subfolders.
        /// </summary>
        /// <param name="i">Irem to check.</param>
        /// <returns>True if located, else false.</returns>
        bool ContainInSelectedFolder(Item i)
        {
            return i.Path.Contains(curPath) && (i.Path.Split('/').Length - curPath.Split('/').Length) < 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addFolder_Click(object sender, RoutedEventArgs e)
        {
            // Check if all field is correctlly filled.
            if (CheckFolderInfo())
            {
                // Can create only in folder.
                if (FindItem(curPath).IsFolder || curPath=="root/")
                {
                    // Add folder to tree and in all items.
                    AddFolder();
                    items.Add(new Item()
                    {
                        IsFolder = true,
                        Name = folderName.Text,
                        Path = curPath + folderName.Text + "/",
                        SortCode = sortCodeFolder.Text
                    });
                    folderName.Text = "";
                    sortCodeFolder.Text = "";
                    MessageBox.Show("Folder added", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    folderName.Text = "";
                    sortCodeFolder.Text = "";
                    MessageBox.Show("Not in a folder",
                         "Dander", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                // Save all data.
                if(autoSaveBool)
                    SaveAllData();
            }
        }
        /// <summary>
        /// Check if all folder`s fields sre correcrtlly filled.
        /// </summary>
        /// <returns>True if is all is correct, else false.</returns>
        bool CheckFolderInfo()
        {
            if (folderName.Text.Contains('/'))
            {
                MessageBox.Show("Folder`s name cannot contains '/'.",
                    "Dander", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (folderName.Text.Length <= 0)
            {
                MessageBox.Show("Folder's name is empty.",
                    "Dander", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!FolderAlreadyExists())
            {
                MessageBox.Show("Folder with this name is already exists.",
                    "Dander", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Add item in treeView.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addItem_Click(object sender, RoutedEventArgs e)
        {
            // Works if only all field filled correctlly.
            if (CheckItemInfo())
            {
                if (FindItem(curPath).IsFolder || curPath == "root/")
                {
                    // Add item in treeView and in list.
                    AddItem();
                    items.Add(new Item()
                    {
                        IsFolder = false,
                        Name = itemName.Text,
                        Path = curPath + itemName.Text,
                        Count = Convert.ToInt32(countItems.Text),
                        Price = Convert.ToDouble(priceItem.Text),
                        Code = articulItem.Text
                    });
                    itemName.Text = "";
                    articulItem.Text = "";
                    countItems.Text = "";
                    priceItem.Text = "";
                    MessageBox.Show("Item added", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            if (autoSaveBool)
                SaveAllData();
        }
        /// <summary>
        /// Check how filled all item`s fileds.
        /// </summary>
        /// <returns>True if all is correct, else false.</returns>
        bool CheckItemInfo()
        {
            if (itemName.Text.Contains('/'))
            {
                MessageBox.Show("Item`s name cannot contains '/'.",
                     "Dander", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (itemName.Text.Length <= 0)
            {
                MessageBox.Show("Item`s name is empty.",
                     "Dander", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (articulItem.Text.Length <= 0)
            {
                MessageBox.Show("Item`s articul is empty.",
                     "Dander", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!ItemAlreadyExists())
            {
                MessageBox.Show("Item with this name is already exists.",
                     "Dander", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!int.TryParse(countItems.Text, out int c))
            {
                MessageBox.Show("Item`s count is not int.",
                     "Dander", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (c < 0)
            {
                MessageBox.Show("Item`s count is negative",
                     "Dander", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!double.TryParse(priceItem.Text, out double p))
            {
                MessageBox.Show("Item`s price is not double",
                     "Dander", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (p < 0)
            {
                MessageBox.Show("Item`s price is negative",
                     "Dander", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Check if exist the item with the same path.
        /// </summary>
        /// <returns></returns>
        bool ItemAlreadyExists()
        {
            foreach (var i in items)
            {
                if (i.Path == curPath + itemName.Text) return false;
            }
            return true;
        }
        /// <summary>
        /// Check if exist the folder with the same path.
        /// </summary>
        /// <returns></returns>
        bool FolderAlreadyExists()
        {
            foreach (var i in items)
            {
                if (i.Path == curPath + folderName.Text + "/") return false;
            }
            return true;
        }
        /// <summary>
        /// Adding a folder.
        /// </summary>
        void AddFolder()
        {
            // Can add onle if folder selected.
            if (IsFolder(curPath))
            {
                var folder = new TreeViewItem();
                folder.Header = folderName.Text;
                folder.Tag = curPath + folderName.Text + "/";
                selectedTVI.Items.Add(folder);
            }
        }
        /// <summary>
        /// Adding item.
        /// </summary>
        void AddItem()
        {
            // Can add only if folder selected.
            if (IsFolder(curPath))
            {
                var item = new TreeViewItem();
                item.Header = itemName.Text;
                item.Tag = curPath + itemName.Text;
                selectedTVI.Items.Add(item);
            }
            else
            {
                itemName.Text = "";
                articulItem.Text = "";
                countItems.Text = "";
                priceItem.Text = "";
                MessageBox.Show("Not in a folder",
                        "Dander", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Set the options if wantToCreateFolder rb checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wantToCreateFolder_Checked(object sender, RoutedEventArgs e)
        {
            addItem.IsEnabled = false;
            addFolder.IsEnabled = true;
            itemName.IsEnabled = false;
            articulItem.IsEnabled = false;
            countItems.IsEnabled = false;
            priceItem.IsEnabled = false;
            folderName.IsEnabled = true;
            if (curPath != "root/")
            {
                changeFolder.IsEnabled = true;
                deleteFolder.IsEnabled = true;
                sortCodeFolder.IsEnabled = true;
            }
            changeItem.IsEnabled = false;
            deleteItem.IsEnabled = false;
        }
        /// <summary>
        /// Set the options if wantToCreateItem rb checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wantToCreateItem_Checked(object sender, RoutedEventArgs e)
        {
            addFolder.IsEnabled = false;
            addItem.IsEnabled = true;
            itemName.IsEnabled = true;
            articulItem.IsEnabled = true;
            countItems.IsEnabled = true;
            priceItem.IsEnabled = true;
            folderName.IsEnabled = false;
            sortCodeFolder.IsEnabled = false;
            changeFolder.IsEnabled = false;
            changeItem.IsEnabled = true;
            deleteFolder.IsEnabled = false;
            deleteItem.IsEnabled = true;
        }
        /// <summary>
        /// Saving all data in json file.
        /// </summary>
        void SaveAllData()
        {
            using (var sw = new StreamWriter(nameOfSavingFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(sw, items);
            }
        }
        /// <summary>
        /// Load all data from json file.
        /// </summary>
        void LoadAllData()
        {
            using (StreamReader r = new StreamReader(nameOfSavingFile))
            {
                string data = r.ReadToEnd();
                if (data.Length > 0)
                    items = JsonConvert.DeserializeObject<List<Item>>(data);
            }
        }
        /// <summary>
        /// Save all data by click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void save_Click(object sender, RoutedEventArgs e)
        {
            SaveAllData();
        }
        /// <summary>
        /// Fill the treeView by List of items.
        /// </summary>
        void FillTree()
        {
            // Clear the tree.
            tree.Items.Clear();
            tree.Items.Refresh();
            // Add the root to the tree.
            var root = new TreeViewItem();
            root.Header = "root";
            root.Tag = "root/";
            root.Foreground = new SolidColorBrush(Colors.Red);
            tree.Items.Add(root);
            selectedTVI = root;
            curPath = "root/";
            // Start fiil the root.
            Fill(2, root);
        }
        /// <summary>
        /// Filling the filder with all items inside this folder (not sunfolders).
        /// </summary>
        /// <param name="n">Segments in path of located items in this folder.</param>
        /// <param name="folder">Folder to put items.</param>
        void Fill(int n, TreeViewItem folder)
        {
            // Get all suitable items.
            var list = HasNParts(n);
            // If we have not suitable itmes stop filling.
            if (list.Count == 0) return;
            foreach (var i in list)
            {
                // Create new treeViewItem.
                var tvi = new TreeViewItem() { Header = i.Name, Tag = i.Path };
                // Set it in tree if this item located in folder.
                if (i.Path.Contains(folder.Tag.ToString())) folder.Items.Add(tvi);
                // If it is a folder fill it again.
                if (i.IsFolder) Fill(n + 1, tvi);
            }
        }
        /// <summary>
        /// Choose all items which has n srgment in path.
        /// </summary>
        /// <param name="n">Amount of segments in path.</param>
        /// <returns>Lits of suitable items.</returns>
        List<Item> HasNParts(int n)
        {
            var res = new List<Item>();
            var sorting = items;
            if (sortByCodeBool)
                sorting = items.OrderBy(x => x.SortCode).ToList();
            foreach (var i in sorting)
            {
                // If it is a folder segmetns must be compared with n+1, because it`s path has '/' ending.
                if (i.IsFolder)
                {
                    if (i.Path.Split('/').Length == n + 1) res.Add(i);
                }
                else
                {
                    if (i.Path.Split('/').Length == n) res.Add(i);
                }
            }
            return res;
        }
        /// <summary>
        /// Change the selected folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeFolder_Click(object sender, RoutedEventArgs e)
        {
            var item = FindItem(curPath);
            int index = items.IndexOf(item);
            // Remove the item.
            items.Remove(item);
            if (CheckFolderInfo() && !CheckIfExists(ChangeFolderPath(item)))
            {
                string newPath = ChangeFolderPath(item);
                // Replace old path on new path in every connected items.
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Path.Contains(item.Path))
                    {
                        SetNewTagForAppropriateTreeViewItem(items[i].Path, items[i].Path.Replace(item.Path, newPath));
                        items[i].Path = items[i].Path.Replace(item.Path, newPath);
                    }
                }
                // Renew approprite treeViewItem.
                selectedTVI.Header = folderName.Text;
                selectedTVI.Tag = newPath;
                // Create new item and insert it in old place.
                item.Name = folderName.Text;
                item.Path = newPath;
                item.SortCode = sortCodeFolder.Text;
                curPath = newPath;
                folderName.Text = "";
                sortCodeFolder.Text = "";
                MessageBox.Show("Folder changed", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // Save all data.
                if (autoSaveBool)
                    SaveAllData();
            }
            else
            {
                folderName.Text = "";
                sortCodeFolder.Text = "";
                MessageBox.Show("The folder with the same path is already exists",
                    "Danger", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            items.Insert(index, item);
        }
        /// <summary>
        /// Set new tag for all appropriate items.
        /// </summary>
        /// <param name="path">Path to replace.</param>
        /// <param name="newPath">New path.</param>
        void SetNewTagForAppropriateTreeViewItem(string path, string newPath)
        {
            var list = FindTreeViewItems(tree);
            for (int i = 0; i < list.Count; i++)
            {
                // If it has a tag.
                if (!(list[i].Tag is null))
                {
                    if (list[i].Tag.ToString() == path)
                    {
                        list[i].Tag = newPath;
                    }
                }
            }

        }
        /// <summary>
        /// Get all tree structure in list.
        /// </summary>
        /// <param name="this"></param>
        /// <returns>List of all treeViewItems</returns>
        public static List<TreeViewItem> FindTreeViewItems(Visual @this)
        {
            if (@this == null)
                return null;

            var result = new List<TreeViewItem>();

            var frameworkElement = @this as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.ApplyTemplate();
            }

            Visual child = null;
            for (int i = 0, count = VisualTreeHelper.GetChildrenCount(@this); i < count; i++)
            {
                // Getting the child.
                child = VisualTreeHelper.GetChild(@this, i) as Visual;

                var treeViewItem = child as TreeViewItem;
                if (treeViewItem != null)
                {
                    result.Add(treeViewItem);
                    if (!treeViewItem.IsExpanded)
                    {
                        treeViewItem.IsExpanded = true;
                        treeViewItem.UpdateLayout();
                    }
                }
                // Add every child in child`s folder.
                foreach (var childTreeViewItem in FindTreeViewItems(child))
                {
                    result.Add(childTreeViewItem);
                }
            }
            return result;
        }
        /// <summary>
        /// Find item by path.
        /// </summary>
        /// <param name="path">Path of the otem.</param>
        /// <returns>Items with our path</returns>
        Item FindItem(string path)
        {
            var item = new Item();
            foreach (var i in items)
            {
                if (i.Path == path)
                    item = i;
            }
            return item;
        }
        /// <summary>
        /// Changing selected item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeItem_Click(object sender, RoutedEventArgs e)
        {
            var item = FindItem(curPath);
            items.Remove(item);
            int index = items.IndexOf(item);
            if (CheckItemInfo() && !CheckIfExists(ChangeItemPath(item)))
            {
                // Change item in tree.
                selectedTVI.Header = itemName.Text;
                selectedTVI.Tag = ChangeItemPath(item);
                // Create new item.
                item.Name = itemName.Text;
                item.Path = ChangeItemPath(item);
                item.Count = Convert.ToInt32(countItems.Text);
                item.Price = Convert.ToDouble(priceItem.Text);
                item.Code = articulItem.Text;
                // Save all data;
                itemName.Text = "";
                articulItem.Text = "";
                countItems.Text = "";
                priceItem.Text = "";
                MessageBox.Show("Item changed", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                if (autoSaveBool)
                    SaveAllData();
            }
            else
            {
                itemName.Text = "";
                articulItem.Text = "";
                countItems.Text = "";
                priceItem.Text = "";
                MessageBox.Show("The item with the same path is already exists",
                    "Danger", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // Add item in the old place.
            items.Add( item);
            curPath = item.Path;
            FillTheListView();
        }
        /// <summary>
        /// Check if item exist by path.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns>True if the folder exists, else false.</returns>
        bool CheckIfExists(string path)
        {
            foreach (var i in items)
            {
                if (i.Path == path)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Make new item path by old path.
        /// </summary>
        /// <param name="item">Item to change.</param>
        /// <returns>New path.</returns>
        string ChangeItemPath(Item item)
        {
            string[] name = item.Path.Split('/');
            name[name.Length - 1] = itemName.Text;
            return String.Join("/", name);
        }
        /// <summary>
        /// Make new folder path by old path.
        /// </summary>
        /// <param name="item">Folder to change.</param>
        /// <returns>New path.</returns>
        string ChangeFolderPath(Item item)
        {
            string[] name = item.Path.Split('/');
            name[name.Length - 2] = folderName.Text;
            return String.Join("/", name);
        }
        /// <summary>
        /// Deleting selected folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteFolder_Click(object sender, RoutedEventArgs e)
        {
            if (FindChildrends(curPath) < 2)
            {
                items.Remove(FindItem(curPath));
                FillTree();
                if (autoSaveBool)
                    SaveAllData();
                MessageBox.Show("Folder is deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Cannot delete folder, because it has items inside", "Danger",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        /// <summary>
        /// Count items in folder.
        /// </summary>
        /// <param name="path">Path of the folder.</param>
        /// <returns>Amount of items in folder.</returns>
        int FindChildrends(string path)
        {
            int sum = 0;
            foreach (var i in items)
            {
                if (i.Path.Contains(path))
                    sum++;
            }
            return sum;
        }
        /// <summary>
        /// Delete selected item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteItem_Click(object sender, RoutedEventArgs e)
        {
            items.Remove(FindItem(curPath));
            FillTree();
            if (autoSaveBool)
                SaveAllData();
            MessageBox.Show("Item is deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        /// <summary>
        /// Change the flag showAll on true.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showAll_Checked(object sender, RoutedEventArgs e)
        {
            isAllItemsBool = true;
            FillTheListView();
        }
        /// <summary>
        /// Change the flag showAll on false.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showAll_Unchecked(object sender, RoutedEventArgs e)
        {
            isAllItemsBool = false;
            FillTheListView();
        }
        /// <summary>
        /// Create csv report.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void csvBtn_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(minCount.Text, out int n) && n > 0)
            {
                var csv = new StringBuilder();
                csv.AppendLine("Path;Articul;Name;Count");
                foreach (var i in items)
                {
                    if (!i.IsFolder)
                    {
                        var first = i.Path;
                        var second = i.Code;
                        var third = i.Name;
                        var fo = i.Count;
                        // If the item is ending.
                        if (fo < n)
                        {
                            var newLine = string.Format("{0};{1};{2};{3}", first, second, third, fo);
                            csv.AppendLine(newLine);
                        }
                    }
                }
                // Write all lines in csv file.
                File.WriteAllText("report.csv", csv.ToString());
                MessageBox.Show("Report created", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Not int positive amount", "Danger", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Create random structure.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void randomC_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(amountFolders.Text, out int n) && n > 0 &&
                int.TryParse(amountItems.Text, out int m) && m > 0)
            {
                // Clear the items.
                items.Clear();
                tree.Items.Clear();
                tree.Items.Refresh();
                // Add the root.
                var root = new TreeViewItem();
                root.Header = "root";
                root.Tag = "root/";
                root.Foreground = new SolidColorBrush(Colors.Red);
                tree.Items.Add(root);
                selectedTVI = root;
                curPath = "root/";
                // Create items in loop.
                for (int i = 1; i <= n; i++)
                {
                    var tvi = new TreeViewItem()
                    {
                        Name = $"Folder{i}",
                        Header = $"Folder{i}",
                        Tag = $"root/Folder{i}/"
                    };
                    items.Add(new Item()
                    {
                        IsFolder = true,
                        Name = $"Folder{i}",
                        Path = $"root/Folder{i}/"
                    });
                    root.Items.Add(tvi);
                    for (int j = 1; j <= m; j++)
                    {
                        items.Add(new Item()
                        {
                            IsFolder = false,
                            Name = $"Item{j}",
                            Path = $"root/Folder{i}/Item{j}",
                            Count = 0,
                            Price = 0,
                            Code = "666"
                        });
                        tvi.Items.Add(new TreeViewItem()
                        {
                            Name = $"Item{j}",
                            Header = $"Item{j}",
                            Tag = $"root/Folder{i}/Item{j}"
                        });
                    }
                }
            }
            else
            {
                MessageBox.Show("Any of params is not positive int.");
            }
        }
        /// <summary>
        /// Change path of savng.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void savingChange_Click(object sender, RoutedEventArgs e)
        {
            if (whereToSave.Text.Length > 0)
            {
                nameOfSavingFile = whereToSave.Text + ".json";
                if (autoSaveBool)
                    SaveAllData();
                MessageBox.Show("Path changed", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Path is empty", "Danger", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Load data from file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadInFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                nameOfSavingFile = openFileDialog.FileName;
            }
            try
            {
                LoadAllData();
                FillTree();
            }
            catch
            {
                MessageBox.Show("Something wrong with the file", "Danger", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Change sorting flag on true.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sortByCode_Checked(object sender, RoutedEventArgs e)
        {
            sortByCodeBool = true;
            FillTree();
        }
        /// <summary>
        /// Change sorting flag on false.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sortByCode_Unchecked(object sender, RoutedEventArgs e)
        {
            sortByCodeBool = false;
            FillTree();
        }
        /// <summary>
        /// Change autosave flag on true.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autosave_Checked(object sender, RoutedEventArgs e)
        {
            autoSaveBool = true;
        }
        /// <summary>
        ///  Change autosave flag on false.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autosave_Unchecked(object sender, RoutedEventArgs e)
        {
            autoSaveBool = false;
        }
    }
}
