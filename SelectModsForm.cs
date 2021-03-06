﻿using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CommonMark;
using System.Drawing;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;
using com.clusterrr.hakchi_gui.Tasks;
using com.clusterrr.hakchi_gui.Properties;

namespace com.clusterrr.hakchi_gui
{
    public struct ReadmeCache
    {
        public string Checksum;
        public DateTime LastModified;
        public string[][] ReadmeData;
        public Dictionary<string, string> getReadmeDictionary()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
                foreach (string[] item in ReadmeData)
                {
                    data.Add(item[0], item[1]);
                }
                return data;
        }

        public ReadmeCache(Dictionary<string, string> ReadmeData, string Checksum, DateTime LastModified)
        {
            List<string[]> list = new List<string[]>();
            foreach (string key in ReadmeData.Keys)
            {
                list.Add(new string[] { key, ReadmeData[key] });
            }
            this.ReadmeData = list.ToArray();
            this.Checksum = Checksum;
            this.LastModified = LastModified;
        }
    }
    public struct HmodReadme
    {
        public readonly Dictionary<string, string> frontMatter;
        public readonly string readme;
        public readonly string rawReadme;
        public readonly bool isMarkdown;
        public HmodReadme(string readme, bool markdown = false)
        {
            this.rawReadme = readme;
            Dictionary<string, string> output = new Dictionary<string, string>();
            Match match = Regex.Match(readme, "^(?:-{3,}[\\r\\n]+(.*?)[\\r\\n]*-{3,})?[\\r\\n\\t\\s]*(.*)[\\r\\n\\t\\s]*$", RegexOptions.Singleline);
            this.readme = match.Groups[2].Value.Trim();
            MatchCollection matches = Regex.Matches(match.Groups[1].Value, "^[\\s\\t]*([^:]+)[\\s\\t]*:[\\s\\t]*(.*?)[\\s\\t]*$", RegexOptions.Multiline);
            foreach (Match fmMatch in matches)
            {
                if (!output.ContainsKey(fmMatch.Groups[1].Value))
                {
                    output.Add(fmMatch.Groups[1].Value, fmMatch.Groups[2].Value);
                }
            }
            this.frontMatter = output;
            this.isMarkdown = markdown;
        }
    }
    public struct Hmod
    {
        public readonly string Name;
        public readonly string HmodPath;
        public readonly bool isFile;
        public readonly HmodReadme Readme;
        public readonly string RawName;
        public readonly string Category;
        public readonly string Creator;
        public readonly bool isInstalled;

        public Hmod(string mod, string[] installedHmods = null)
        {
            isInstalled = false;
            if (installedHmods != null)
            {
                isInstalled = installedHmods.Contains(mod);
            }
            RawName = mod;
            this.HmodPath = null;
            this.isFile = false;

            string[] readmeFiles = new string[] { "readme.txt", "readme.md", "readme" };
            string usermodsDirectory = Path.Combine(Program.BaseDirectoryExternal, "user_mods");
            string cacheDir = Shared.PathCombine(Program.BaseDirectoryExternal, "user_mods", "readme_cache");
            string cacheFile = Path.Combine(cacheDir, $"{mod}.xml");


            Dictionary<string, string> readmeData = new Dictionary<string, string>();

            try
            {
                var dir = Path.Combine(usermodsDirectory, mod + ".hmod");
                if (Directory.Exists(dir))
                {
                    isFile = false;
                    HmodPath = dir;
                    foreach (var f in readmeFiles)
                    {
                        var fn = Path.Combine(dir, f);
                        if (File.Exists(fn))
                        {
                            readmeData.Add(f.ToLower(), File.ReadAllText(fn));
                        }
                    }
                }
                else if (File.Exists(dir))
                {
                    isFile = true;
                    HmodPath = dir;

                    ReadmeCache cache;
                    FileInfo info = new FileInfo(dir);
                    
                    bool skipExtraction = false;
                    if (File.Exists(cacheFile))
                    {
                        try
                        {
                            cache = XMLSerialization.DeserializeXMLFileToObject<ReadmeCache>(cacheFile);
                            if (cache.LastModified == info.LastWriteTimeUtc)
                            {
                                skipExtraction = true;
                                readmeData = cache.getReadmeDictionary();
                            }
                        } catch { }
                    }


                    if (!skipExtraction)
                    {
                        using (var szExtractor = new SevenZipExtractor(dir))
                        {
                            var tar = new MemoryStream();
                            szExtractor.ExtractFile(0, tar);
                            tar.Seek(0, SeekOrigin.Begin);
                            using (var szExtractorTar = new SevenZipExtractor(tar))
                            {
                                foreach (var f in szExtractorTar.ArchiveFileNames)
                                {
                                    foreach (var readmeFilename in readmeFiles)
                                    {
                                        if (f.ToLower() != readmeFilename && f.ToLower() != $".\\{readmeFilename}")
                                            continue;

                                        var o = new MemoryStream();
                                        szExtractorTar.ExtractFile(f, o);
                                        readmeData.Add(readmeFilename, Encoding.UTF8.GetString(o.ToArray()));
                                    }
                                }
                            }
                        }
                        cache = new ReadmeCache(readmeData, "", info.LastWriteTimeUtc);

                        if (!Directory.Exists(cacheDir))
                            Directory.CreateDirectory(cacheDir);

                        File.WriteAllText(cacheFile, cache.Serialize());
                    }
                }
                else
                {
                    if (File.Exists(cacheFile))
                    {
                        try
                        {
                            ReadmeCache cache;
                            cache = XMLSerialization.DeserializeXMLFileToObject<ReadmeCache>(cacheFile);
                            readmeData = cache.getReadmeDictionary();
                        }
                        catch { }
                    }
                }
            }
            catch
            {
            }

            string readme;
            bool markdown = false;
            if (readmeData.TryGetValue("readme.md", out readme))
            {
                markdown = true;
            }
            else if (readmeData.TryGetValue("readme.txt", out readme)) { }
            else if (readmeData.TryGetValue("readme", out readme)) { }
            else
            {
                readme = "";
            }

            this.Readme = new HmodReadme(readme, markdown);

            if (!this.Readme.frontMatter.TryGetValue("Name", out this.Name))
            {
                this.Name = mod;
            }
            if (!this.Readme.frontMatter.TryGetValue("Category", out this.Category))
            {
                this.Category = Properties.Resources.Unknown;
            }

            if (!this.Readme.frontMatter.TryGetValue("Creator", out this.Creator))
            {
                this.Creator = Properties.Resources.Unknown;
            }
        }
    }
    public enum HmodListSort
    {
        Category,
        Creator
    }
    public partial class SelectModsForm : Form
    {
        private string hmodDisplayed = null;
        private readonly string usermodsDirectory;
        private List<Hmod> hmods = new List<Hmod>();
        private string[] installedMods = new string[] { };
        private bool loadInstalledMods;

        public SelectModsForm(bool loadInstalledMods, bool allowDropMods, string[] filesToAdd = null)
        {

            InitializeComponent();
            this.loadInstalledMods = loadInstalledMods;

            switch (ConfigIni.Instance.hmodListSort)
            {
                case HmodListSort.Category:
                    categoryToolStripMenuItem.Checked = true;
                    break;

                case HmodListSort.Creator:
                    creatorToolStripMenuItem.Checked = true;
                    break;
            }

            wbReadme.Document.BackColor = this.BackColor;
            usermodsDirectory = Path.Combine(Program.BaseDirectoryExternal, "user_mods");
            var modsList = new List<string>();

            if (hakchi.Shell.IsOnline && (hakchi.MinimalMemboot || hakchi.CanInteract))
            {
                bool wasMounted = true;
                if (hakchi.MinimalMemboot)
                {
                    if (hakchi.Shell.Execute("hakchi eval 'mountpoint -q \"$mountpoint/var/lib\"'") != 0)
                    {
                        wasMounted = false;
                        hakchi.Shell.ExecuteSimple("hakchi mount_base");
                    }
                }
                installedMods = hakchi.Shell.ExecuteSimple("hakchi pack_list", 0, true).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (!wasMounted)
                    hakchi.Shell.ExecuteSimple("hakchi umount_base");
            }

            if (loadInstalledMods && hakchi.Shell.IsOnline)
            {
                foreach (var mod in installedMods)
                {
                    modsList.Add(mod);
                }
            }
            else
            {
                if (Directory.Exists(usermodsDirectory))
                {
                    modsList.AddRange(from m
                                      in Directory.GetDirectories(usermodsDirectory, "*.hmod", SearchOption.TopDirectoryOnly)
                                      select Path.GetFileNameWithoutExtension(m));
                    modsList.AddRange(from m
                                      in Directory.GetFiles(usermodsDirectory, "*.hmod", SearchOption.TopDirectoryOnly)
                                      select Path.GetFileNameWithoutExtension(m));
                }
            }

            using(Tasker tasker = new Tasker(this))
            {
                tasker.AttachView(new Tasks.TaskerForm());
                var modObject = new ModTasks.ModObject();
                modObject.HmodsToLoad = modsList;
                modObject.InstalledHmods = installedMods;
                tasker.SetTitle(Resources.LoadingHmods);
                tasker.SetStatusImage(Resources.sign_brick);
                tasker.SyncObject = modObject;
                tasker.AddTask(ModTasks.GetHmods);
                tasker.Start();
                hmods = modObject.LoadedHmods;
            }

            populateList();

            if (filesToAdd != null) AddMods(filesToAdd);
            this.AllowDrop = allowDropMods;
        }

        private void populateList()
        {
            SortedDictionary<string, ListViewGroup> listGroups = new SortedDictionary<string, ListViewGroup>();
            listViewHmods.BeginUpdate();
            listViewHmods.Groups.Clear();
            listViewHmods.Items.Clear();

            foreach (Hmod hmod in hmods)
            {
                string groupName = Properties.Resources.Unknown;

                switch (ConfigIni.Instance.hmodListSort)
                {
                    case HmodListSort.Category:
                        groupName = hmod.Category;
                        break;

                    case HmodListSort.Creator:
                        groupName = hmod.Creator;
                        break;
                }

                ListViewGroup group;
                if (!listGroups.TryGetValue(groupName.ToLower(), out group))
                {
                    group = new ListViewGroup(groupName, HorizontalAlignment.Center);
                    listGroups.Add(groupName.ToLower(), group);
                }
                ListViewItem item = new ListViewItem(new String[] { hmod.Name, hmod.Creator });
                item.SubItems.Add(hmod.Creator);
                item.Tag = hmod;
                item.Group = group;
                if (!loadInstalledMods && installedMods.Contains(hmod.RawName))
                {
                    item.Checked = true;
                    item.ForeColor = SystemColors.GrayText;
                }

                listViewHmods.Items.Add(item);
            }

            foreach (ListViewGroup group in listGroups.Values)
            {
                listViewHmods.Groups.Add(group);
            }

            listViewHmods.EndUpdate();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (listViewHmods.CheckedItems.Count > 0)
                DialogResult = DialogResult.OK;
            else
                DialogResult = DialogResult.Cancel;
            Close();
        }

        static string getHmodPath(string hmod)
        {
            try
            {
                Dictionary<string, string> readmeData = new Dictionary<string, string>();
                var dir = Shared.PathCombine(Program.BaseDirectoryExternal, "user_mods", hmod + ".hmod");
                if (Directory.Exists(dir))
                {
                    return dir + Path.DirectorySeparatorChar;
                }
                else if (File.Exists(dir))
                {
                    return dir;
                }
            }
            catch { }

            return null;
        }

        string formatReadme(ref Hmod hmod)
        {
            string[] headingFields = { "Creator", "Version" };
            List<string> headingLines = new List<string>();

            foreach (string heading in headingFields)
            {
                string keyValue;
                if (hmod.Readme.frontMatter.TryGetValue(heading, out keyValue))
                {
                    headingLines.Add($"**{heading}:** {keyValue}");
                }
            }

            foreach (string keyName in hmod.Readme.frontMatter.Keys)
            {
                if (!headingFields.Contains(keyName) && keyName != "Name")
                {
                    headingLines.Add($"**{keyName}:** {hmod.Readme.frontMatter[keyName]}");
                }
            }
            
            return CommonMarkConverter.Convert(String.Join("  \n", headingLines.ToArray()) + "\n\n" + (hmod.Readme.isMarkdown || hmod.Readme.readme.Length == 0 ? hmod.Readme.readme : $"```\n{hmod.Readme.readme}\n```"));
        }

        void showReadMe(ref Hmod hmod)
        {
            if (hmodDisplayed == hmod.RawName)
                return;

            hmodDisplayed = hmod.RawName;
            Color color = this.BackColor;
            string html = String.Format(Properties.Resources.readmeTemplateHTML, Properties.Resources.readmeTemplateCSS, hmod.Name, formatReadme(ref hmod), $"rgb({color.R},{color.G},{color.B})");
            wbReadme.DocumentText = html;
        }

        private void SelectModsForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void SelectModsForm_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            AddMods(files);
        }

        private void AddMods(string[] files)
        {
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLower();
                if (ext == ".hmod")
                {
                    var target = Path.Combine(usermodsDirectory, Path.GetFileName(file));
                    if (file != target)
                        if (Directory.Exists(file))
                            Shared.DirectoryCopy(file, target, true, false, true, false);
                        else
                            File.Copy(file, target, true);
                    hmods.Add(new Hmod(Path.GetFileNameWithoutExtension(file)));
                }
                else if (ext == ".7z" || ext == ".zip" || ext == ".rar")
                {
                    using (var szExtractor = new SevenZipExtractor(file))
                    {
                        foreach(var f in szExtractor.ArchiveFileData)
                        {
                            if (Path.GetExtension(f.FileName).ToLower() == ".hmod")
                            {
                                if (f.IsDirectory)
                                {
                                    List<int> indices = new List<int>();
                                    for (int i = 0; i < szExtractor.ArchiveFileData.Count; ++i)
                                        if (szExtractor.ArchiveFileData[i].FileName.StartsWith(f.FileName))
                                            indices.Add(i);
                                    szExtractor.ExtractFiles(usermodsDirectory, indices.ToArray());

                                    if (!Directory.Exists(Path.Combine(usermodsDirectory, Path.GetFileName(f.FileName))))
                                    {
                                        Directory.Move(Path.Combine(usermodsDirectory, f.FileName), Path.Combine(usermodsDirectory, Path.GetFileName(f.FileName)));

                                        new DirectoryInfo(usermodsDirectory).Refresh();
                                        int pos = f.FileName.IndexOfAny(new char[] { '/', '\\' });
                                        Directory.Delete(Path.Combine(usermodsDirectory, pos > 0 ? f.FileName.Substring(0, pos) : f.FileName), true);
                                    }

                                    hmods.Add(new Hmod(Path.GetFileNameWithoutExtension(f.FileName)));
                                }
                                else
                                {
                                    using (var outFile = new FileStream(Path.Combine(usermodsDirectory, Path.GetFileName(f.FileName)), FileMode.Create))
                                    {
                                        szExtractor.ExtractFile(f.FileName, outFile);
                                        hmods.Add(new Hmod(Path.GetFileNameWithoutExtension(f.FileName)));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            populateList();
        }

        private void wbReadme_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.ToString() == "about:blank") return;

            //cancel the current event
            e.Cancel = true;

            //this opens the URL in the user's default browser
            Process.Start(e.Url.ToString());
        }

        private void listViewHmods_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void categoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            categoryToolStripMenuItem.Checked = true;
            creatorToolStripMenuItem.Checked = false;
            ConfigIni.Instance.hmodListSort = HmodListSort.Category;
            populateList();
        }

        private void creatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            creatorToolStripMenuItem.Checked = true;
            categoryToolStripMenuItem.Checked = false;
            ConfigIni.Instance.hmodListSort = HmodListSort.Creator;
            populateList();
        }

        private void listViewHmods_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if(!loadInstalledMods && e.Item.Checked == false && ((Hmod)e.Item.Tag).isInstalled)
            {
                e.Item.Checked = true;
            }
        }

        private void listViewHmods_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.Item != null && listViewHmods.SelectedItems.Count > 0)
            {
                Hmod hmod = (Hmod)(listViewHmods.SelectedItems[0].Tag);
                showReadMe(ref hmod);
            }
            else
            {
                Color color = this.BackColor;
                string html = String.Format(Properties.Resources.readmeTemplateHTML, Properties.Resources.readmeTemplateCSS, "", "", $"rgb({color.R},{color.G},{color.B})");
                wbReadme.DocumentText = html;
                hmodDisplayed = null;
            }

            if (!loadInstalledMods && e.Item != null && e.Item.Selected == true && ((Hmod)e.Item.Tag).isInstalled)
            {
                e.Item.Selected = false;
            }
        }

        private void SelectModsForm_Shown(object sender, EventArgs e)
        {
            splitContainer1_SplitterMoved(null, null);
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            listViewHmods.BeginUpdate();
            hmodName.Width = -1;
            hmodName.Width = listViewHmods.Width - 4 - SystemInformation.VerticalScrollBarWidth;
            listViewHmods.EndUpdate();
        }
    }
}
