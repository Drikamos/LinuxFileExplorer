using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace LinuxExplorer
{
    public partial class MainForm : Form
    {
        private string currentPath = "";
        private string clipboardPath = "";
        private bool isCopy = true;
        private Stack<string> historyBack = new Stack<string>();
        private Stack<string> historyForward = new Stack<string>();
        private bool showHiddenFiles = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeListView();
            LoadDrives();
            SetupShortcuts();
        }

        private void LoadDrives()
        {
            listViewFiles.Items.Clear();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    var item = new ListViewItem(drive.Name, 0) { Tag = drive.Name };
                    listViewFiles.Items.Add(item);
                }
            }
            currentPath = string.Empty;
        }

        private void InitializeListView()
        {
            listViewFiles.View = View.Details;
            listViewFiles.Columns.Add("Name", 400);
            listViewFiles.FullRowSelect = true;
            listViewFiles.LabelEdit = true;
            listViewFiles.AllowDrop = true;

            listViewFiles.MouseClick += listViewFiles_MouseClick;
            listViewFiles.MouseDoubleClick += listViewFiles_MouseDoubleClick;
            listViewFiles.AfterLabelEdit += listViewFiles_AfterLabelEdit;
            listViewFiles.MouseUp += listViewFiles_MouseUp;

            listViewFiles.DragEnter += (s, e) => e.Effect = DragDropEffects.Copy;
            listViewFiles.DragDrop += listViewFiles_DragDrop;
        }

        private void listViewFiles_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var clickedItem = listViewFiles.GetItemAt(e.X, e.Y);
            var contextMenu = new ContextMenuStrip();

            if (clickedItem != null)
            {
                listViewFiles.SelectedItems.Clear();
                clickedItem.Selected = true;

                contextMenu.Items.Add("Copy", null, (s, a) => CopySelected());
                contextMenu.Items.Add("Cut", null, (s, a) => CutSelected());
                contextMenu.Items.Add("Delete", null, (s, a) => DeleteSelected());
            }
            else
            {
                contextMenu.Items.Add("Paste", null, (s, a) => PasteItem());
                contextMenu.Items.Add("New Folder", null, (s, a) => CreateNewFolder());
                contextMenu.Items.Add("New Text File", null, (s, a) => CreateNewTextFile());
            }

            contextMenu.Show(listViewFiles, e.Location);
        }

        private void listViewFiles_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var clickedItem = listViewFiles.GetItemAt(e.X, e.Y);
                if (clickedItem == null)
                {
                    var contextMenu = new ContextMenuStrip();
                    contextMenu.Items.Add("Paste", null, (s, a) => PasteItem());
                    contextMenu.Items.Add("New Folder", null, (s, a) => CreateNewFolder());
                    contextMenu.Items.Add("New Text File", null, (s, a) => CreateNewTextFile());

                    contextMenu.Show(listViewFiles, e.Location);
                }
            }
        }

        private void listViewFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewFiles.SelectedItems.Count == 0) return;

            var path = listViewFiles.SelectedItems[0].Tag.ToString();
            if (Directory.Exists(path))
            {
                historyBack.Push(currentPath);
                historyForward.Clear();
                LoadFiles(path);
            }
            else if (File.Exists(path))
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
        }

        private void SetupShortcuts()
        {
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.C) CopySelected();
                if (e.Control && e.KeyCode == Keys.X) CutSelected();
                if (e.Control && e.KeyCode == Keys.V) PasteItem();
                if (e.KeyCode == Keys.Delete) DeleteSelected();
                if (e.KeyCode == Keys.F2) RenameSelected();
            };
        }

        private void LoadFiles(string path)
        {
            try
            {
                listViewFiles.Items.Clear();

                foreach (var dir in Directory.GetDirectories(path))
                {
                    if (!showHiddenFiles && Path.GetFileName(dir).StartsWith(".")) continue;
                    var name = Path.GetFileName(dir);
                    listViewFiles.Items.Add(new ListViewItem(name, 0) { Tag = dir });
                }

                foreach (var file in Directory.GetFiles(path))
                {
                    if (!showHiddenFiles && Path.GetFileName(file).StartsWith(".")) continue;
                    var name = Path.GetFileName(file);
                    listViewFiles.Items.Add(new ListViewItem(name, 1) { Tag = file });
                }

                currentPath = path;
                labelPath.Text = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void CreateNewFolder()
        {
            string baseName = "New Folder";
            string newFolder = Path.Combine(currentPath, baseName);
            int count = 1;
            while (Directory.Exists(newFolder))
                newFolder = Path.Combine(currentPath, $"{baseName} ({count++})");

            Directory.CreateDirectory(newFolder);
            LoadFiles(currentPath);

            foreach (ListViewItem item in listViewFiles.Items)
            {
                if (item.Tag.ToString() == newFolder)
                {
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    item.BeginEdit();
                    break;
                }
            }
        }

        private void CreateNewTextFile()
        {
            string baseName = "New File.txt";
            string newFile = Path.Combine(currentPath, baseName);
            int count = 1;
            while (File.Exists(newFile))
                newFile = Path.Combine(currentPath, $"New File ({count++}).txt");

            File.WriteAllText(newFile, "");
            LoadFiles(currentPath);

            foreach (ListViewItem item in listViewFiles.Items)
            {
                if (item.Tag.ToString() == newFile)
                {
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    item.BeginEdit();
                    break;
                }
            }
        }

        private void CopySelected()
        {
            if (listViewFiles.SelectedItems.Count == 0) return;
            clipboardPath = listViewFiles.SelectedItems[0].Tag.ToString();
            isCopy = true;
        }

        private void CutSelected()
        {
            if (listViewFiles.SelectedItems.Count == 0) return;
            clipboardPath = listViewFiles.SelectedItems[0].Tag.ToString();
            isCopy = false;
        }

        private void PasteItem()
        {
            if (string.IsNullOrEmpty(clipboardPath)) return;

            string dest = Path.Combine(currentPath, Path.GetFileName(clipboardPath));

            if (Directory.Exists(clipboardPath))
            {
                if (isCopy)
                    CopyDirectory(clipboardPath, dest);
                else
                    Directory.Move(clipboardPath, dest);
            }
            else if (File.Exists(clipboardPath))
            {
                if (isCopy)
                    File.Copy(clipboardPath, dest, true);
                else
                    File.Move(clipboardPath, dest);
            }

            clipboardPath = "";
            LoadFiles(currentPath);
        }

        private void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string dest = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }
            foreach (var folder in Directory.GetDirectories(sourceDir))
            {
                string dest = Path.Combine(targetDir, Path.GetFileName(folder));
                CopyDirectory(folder, dest);
            }
        }

        private void DeleteSelected()
        {
            if (listViewFiles.SelectedItems.Count == 0) return;

            var path = listViewFiles.SelectedItems[0].Tag.ToString();
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            else if (File.Exists(path))
                File.Delete(path);

            LoadFiles(currentPath);
        }

        private void RenameSelected()
        {
            if (listViewFiles.SelectedItems.Count == 0) return;
            listViewFiles.LabelEdit = true;
            listViewFiles.SelectedItems[0].BeginEdit();
        }

        private void listViewFiles_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (e.Label == null) return;

            string oldPath = listViewFiles.Items[e.Item].Tag.ToString();
            string newPath = Path.Combine(currentPath, e.Label);

            if (Directory.Exists(oldPath))
                Directory.Move(oldPath, newPath);
            else if (File.Exists(oldPath))
                File.Move(oldPath, newPath);

            LoadFiles(currentPath);
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            if (historyBack.Count == 0) return;
            historyForward.Push(currentPath);
            LoadFiles(historyBack.Pop());
        }

        private void buttonForward_Click(object sender, EventArgs e)
        {
            if (historyForward.Count == 0) return;
            historyBack.Push(currentPath);
            LoadFiles(historyForward.Pop());
        }

        private void listViewFiles_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    string dest = Path.Combine(currentPath, Path.GetFileName(file));
                    if (Directory.Exists(file))
                        CopyDirectory(file, dest);
                    else
                        File.Copy(file, dest, true);
                }
                LoadFiles(currentPath);
            }
        }
    }
}
