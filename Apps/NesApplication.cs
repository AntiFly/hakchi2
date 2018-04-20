﻿using com.clusterrr.hakchi_gui.Properties;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace com.clusterrr.hakchi_gui
{
    public class NesApplication : NesMenuElementBase
    {
        public const uint MaxCompress = 16 * 1024 * 1024; // 16 megabytes

        public static Form ParentForm;
        public static bool? NeedPatch;
        public static bool? Need3rdPartyEmulator;
        public static bool? NeedAutoDownloadCover;
        public static string[] CachedCoverFiles;

        public static Dictionary<hakchi.ConsoleType, string[]> DefaultGames = new Dictionary<hakchi.ConsoleType, string[]>()
        {
            {
                hakchi.ConsoleType.Famicom,
                new string[]
                {
                    "CLV-P-HAAAJ",
                    "CLV-P-HAACJ",
                    "CLV-P-HAADJ",
                    "CLV-P-HAAEJ",
                    "CLV-P-HAAHJ",
                    "CLV-P-HAAMJ",
                    "CLV-P-HAANJ",
                    "CLV-P-HAAPJ",
                    "CLV-P-HAAQJ",
                    "CLV-P-HAARJ",
                    "CLV-P-HAASJ",
                    "CLV-P-HAAUJ",
                    "CLV-P-HAAWJ",
                    "CLV-P-HAAXJ",
                    "CLV-P-HABBJ",
                    "CLV-P-HABCJ",
                    "CLV-P-HABLJ",
                    "CLV-P-HABMJ",
                    "CLV-P-HABNJ",
                    "CLV-P-HABQJ",
                    "CLV-P-HABRJ",
                    "CLV-P-HABVJ",
                    "CLV-P-HACAJ",
                    "CLV-P-HACBJ",
                    "CLV-P-HACCJ",
                    "CLV-P-HACEJ",
                    "CLV-P-HACHJ",
                    "CLV-P-HACJJ",
                    "CLV-P-HACLJ",
                    "CLV-P-HACPJ",
                }
            },
            {
                hakchi.ConsoleType.NES,
                new string[]
                {
                    "CLV-P-NAAAE",
                    "CLV-P-NAACE",
                    "CLV-P-NAADE",
                    "CLV-P-NAAEE",
                    "CLV-P-NAAFE",
                    "CLV-P-NAAHE",
                    "CLV-P-NAANE",
                    "CLV-P-NAAPE",
                    "CLV-P-NAAQE",
                    "CLV-P-NAARE",
                    "CLV-P-NAASE",
                    "CLV-P-NAATE",
                    "CLV-P-NAAUE",
                    "CLV-P-NAAVE",
                    "CLV-P-NAAWE",
                    "CLV-P-NAAXE",
                    "CLV-P-NAAZE",
                    "CLV-P-NABBE",
                    "CLV-P-NABCE",
                    "CLV-P-NABJE",
                    "CLV-P-NABKE",
                    "CLV-P-NABME",
                    "CLV-P-NABNE",
                    "CLV-P-NABQE",
                    "CLV-P-NABRE",
                    "CLV-P-NABVE",
                    "CLV-P-NABXE",
                    "CLV-P-NACBE",
                    "CLV-P-NACDE",
                    "CLV-P-NACHE",
                }
            },
            {
                hakchi.ConsoleType.SNES_EUR,
                new string[]
                {
                    "CLV-P-SAAAE",
                    "CLV-P-SAABE",
                    "CLV-P-SAAEE",
                    "CLV-P-SAAFE",
                    "CLV-P-SAAHE",
                    "CLV-P-SAAJE",
                    "CLV-P-SAAKE",
                    "CLV-P-SAALE",
                    "CLV-P-SAAQE",
                    "CLV-P-SAAXE",
                    "CLV-P-SABCE",
                    "CLV-P-SABDE",
                    "CLV-P-SABHE",
                    "CLV-P-SABQE",
                    "CLV-P-SABRE",
                    "CLV-P-SABTE",
                    "CLV-P-SACBE",
                    "CLV-P-SACCE",
                    "CLV-P-SADGE",
                    "CLV-P-SADJE",
                    "CLV-P-SADKE",
                }
            },
            {
                hakchi.ConsoleType.SNES_USA,
                new string[]
                {
                    "CLV-P-SAAAE",
                    "CLV-P-SAABE",
                    "CLV-P-SAAEE",
                    "CLV-P-SAAFE",
                    "CLV-P-SAAHE",
                    "CLV-P-SAAJE",
                    "CLV-P-SAAKE",
                    "CLV-P-SAALE",
                    "CLV-P-SAAQE",
                    "CLV-P-SAAXE",
                    "CLV-P-SABCE",
                    "CLV-P-SABDE",
                    "CLV-P-SABHE",
                    "CLV-P-SABQE",
                    "CLV-P-SABRE",
                    "CLV-P-SABTE",
                    "CLV-P-SACBE",
                    "CLV-P-SACCE",
                    "CLV-P-SADGE",
                    "CLV-P-SADJE",
                    "CLV-P-SADKE",
                }
            },
            {
                hakchi.ConsoleType.SuperFamicom,
                new string[]
                {
                    "CLV-P-VAAAJ",
                    "CLV-P-VAABJ",
                    "CLV-P-VAAEJ",
                    "CLV-P-VAAFJ",
                    "CLV-P-VAAGJ",
                    "CLV-P-VAAHJ",
                    "CLV-P-VAALJ",
                    "CLV-P-VAAQJ",
                    "CLV-P-VABBJ",
                    "CLV-P-VABCJ",
                    "CLV-P-VABDJ",
                    "CLV-P-VABQJ",
                    "CLV-P-VABRJ",
                    "CLV-P-VABTJ",
                    "CLV-P-VACCJ",
                    "CLV-P-VACDJ",
                    "CLV-P-VADFJ",
                    "CLV-P-VADGJ",
                    "CLV-P-VADJJ",
                    "CLV-P-VADKJ",
                    "CLV-P-VADZJ",
                }
            },
        };

        public static string[] CurrentDefaultGames
        {
            get
            {
                if (ConfigIni.Instance.ConsoleType != hakchi.ConsoleType.Unknown)
                {
                    return DefaultGames[ConfigIni.Instance.ConsoleType];
                }
                return new string[0];
            }
        }
        private static string[] allDefaultGames = null;
        public static string[] AllDefaultGames
        {
            get
            {
                if (allDefaultGames == null)
                {
                    allDefaultGames = Shared.ConcatArrays(
                        DefaultGames[hakchi.ConsoleType.Famicom],
                        DefaultGames[hakchi.ConsoleType.NES],
                        DefaultGames[hakchi.ConsoleType.SNES_EUR],
                        DefaultGames[hakchi.ConsoleType.SuperFamicom]
                    );
                }
                return allDefaultGames;
            }
        }

        public static readonly string OriginalGamesDirectory = Path.Combine(Program.BaseDirectoryExternal, "games_originals");
        public static readonly string OriginalGamesCacheDirectory = Path.Combine(Program.BaseDirectoryExternal, "games_cache");
        public static string GamesDirectory
        {
            get
            {
                switch (ConfigIni.Instance.ConsoleType)
                {
                    default:
                    case hakchi.ConsoleType.NES:
                    case hakchi.ConsoleType.Famicom:
                        return Path.Combine(Program.BaseDirectoryExternal, "games");
                    case hakchi.ConsoleType.SNES_EUR:
                    case hakchi.ConsoleType.SNES_USA:
                    case hakchi.ConsoleType.SuperFamicom:
                        return Path.Combine(Program.BaseDirectoryExternal, "games_snes");
                }
            }
        }

        public class AppMetadata
        {
            public string System = string.Empty;
            public string Core = string.Empty;
            public string OriginalFilename = string.Empty;
            public uint OriginalCrc32 = 0;
            public bool CustomCoverArt = false;
            [JsonIgnore]
            private AppTypeCollection.AppInfo appInfo = null;
            [JsonIgnore]
            public AppTypeCollection.AppInfo AppInfo
            {
                get
                {
                    if (appInfo != null)
                        return appInfo;
                    if (string.IsNullOrEmpty(System))
                        return AppTypeCollection.UnknownApp;
                    appInfo = AppTypeCollection.GetAppBySystem(System);
                    if (appInfo.Unknown)
                    {
                        appInfo = new AppTypeCollection.AppInfo()
                        {
                            Name = System,
                            Class = typeof(UnknownGame),
                            DefaultCore = Core,
                            LegacyApps = new string[] { },
                            Extensions = new string[] { },
                            Prefix = AppTypeCollection.GetAvailablePrefix(System),
                            DefaultCover = Resources.blank_app,
                            GoogleSuffix = "game"
                        };
                    }
                    return appInfo;
                }
            }
            [JsonIgnore]
            public CoreCollection.CoreInfo CoreInfo
            {
                get
                {
                    if (!string.IsNullOrEmpty(Core))
                    {
                        return CoreCollection.GetCore(Core);
                    }
                    return null;
                }
            }
            public static AppMetadata Load(string filename)
            {
                return JsonConvert.DeserializeObject<AppMetadata>(File.ReadAllText(filename));
            }
            public void Save(string filename)
            {
                File.WriteAllText(filename, JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\r", "") + "\n");
            }
        }
        public AppMetadata Metadata;

        public const string GameGenieFileName = "gamegenie.txt";
        public string GameGeniePath { private set; get; }
        public string GameGenie { get; set; }

        public string GameFilePath
        {
            get
            {
                foreach (var arg in desktop.Args)
                {
                    Match m = Regex.Match(arg, @"(^\/.*)\/(?:" + desktop.Code + @"\/)([^.]*)(.*$)"); // actual regex: /(^\/.*)\/(?:...-.-.....\/)([^.]*)(.*$)/
                    if (m.Success)
                    {
                        string gameFile = Path.Combine(basePath, m.Groups[2].ToString() + m.Groups[3].ToString());
                        if (File.Exists(gameFile))
                            return gameFile;
                    }
                }
                return null;
            }
        }

        public Stream GameFileStream
        {
            get
            {
                string[] compressedFiles = new string[] { ".7z", ".zip" };

                string gameFilePath = GameFilePath;
                if (gameFilePath != null)
                {
                    int extPos = gameFilePath.LastIndexOf('.');
                    if (extPos > -1 && compressedFiles.Contains(gameFilePath.Substring(extPos)))
                    {
                        using (var extractor = new SevenZipExtractor(gameFilePath))
                        {
                            if (extractor.FilesCount == 1)
                            {
                                MemoryStream stream = new MemoryStream();
                                extractor.ExtractFile(0, stream);
                                return stream;
                            }
                        }
                    }
                    else
                    {
                        return new FileStream(gameFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                }
                return null;
            }
        }

        public byte[] GameFileData
        {
            get
            {
                Stream gameFileStream = GameFileStream;
                if (gameFileStream != null)
                {
                    if (gameFileStream is MemoryStream)
                    {
                        byte[] buffer = ((MemoryStream)gameFileStream).ToArray();
                        gameFileStream.Dispose();
                        return buffer;
                    }
                    else
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            gameFileStream.CopyTo(ms);
                            byte[] buffer = ms.ToArray();
                            gameFileStream.Dispose();
                            return buffer;
                        }
                    }
                }
                return null;
            }
        }

        private bool isOriginalGame;
        public bool IsOriginalGame
        {
            get { return isOriginalGame; }
        }

        private bool isDeleting;
        public bool IsDeleting
        {
            get { return isDeleting; }
            set { isDeleting = value; }
        }

        public IEnumerable<string> CoverArtMatches
        {
            get; private set;
        }
        public bool CoverArtMatchSuccess
        {
            get; private set;
        }

        public static NesApplication FromDirectory(string path, bool ignoreEmptyConfig = false)
        {
            (new DirectoryInfo(path)).Refresh();
            string[] files = Directory.GetFiles(path, "*.desktop", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                if (!ignoreEmptyConfig)
                    throw new FileNotFoundException($"Invalid application folder: \"{path}\".");
                return new NesApplication(path, null, true);
            }

            // let's find the AppInfo entry for current application
            AppMetadata metadata = null;

            // look for the new metadata file
            if (File.Exists(Path.Combine(path, "metadata.json")))
            {
                try
                {
                    metadata = AppMetadata.Load(Path.Combine(path, "metadata.json"));
                }
                catch
                {
                    metadata = null;
                }
            }
            if (metadata == null)
            {
                metadata = new AppMetadata();
            }

            // fallback to reading .desktop file and guess app type if no metadata match
            AppTypeCollection.AppInfo appInfo = null;
            if (string.IsNullOrEmpty(metadata.System) || metadata.System.Length == 0 || !CoreCollection.Systems.Contains(metadata.System))
            {
                string[] config = File.ReadAllLines(files[0]);
                foreach (var line in config)
                {
                    if (line.ToLower().StartsWith("exec="))
                    {
                        string exec = line.Substring(5);
                        appInfo = AppTypeCollection.GetAppByExec(exec); // guaranteed to at least return UnknownApp
                        if (!appInfo.Unknown)
                        {
                            metadata.System = appInfo.Name;
                            break;
                        }
                    }
                }
            }
            if (appInfo == null)
            { 
                appInfo = metadata.AppInfo; // guaranteed to at least return UnknownApp
            }

            try
            {
                var constructor = appInfo.Class.GetConstructor(new Type[] { typeof(string), typeof(AppMetadata), typeof(bool) });
                return (NesApplication)constructor.Invoke(new object[] { path, metadata, ignoreEmptyConfig });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating app from directory {path} : " + ex.Message + ex.StackTrace);
                return new UnknownGame(path);
            }
        }

        public static NesApplication Import(string inputFileName, string originalFileName = null, byte[] rawRomData = null)
        {
            var ext = Path.GetExtension(inputFileName).ToLower();
            if (ext == ".desktop") // already hakchi2-ed game
                return ImportApp(inputFileName);

            if (rawRomData == null && (new FileInfo(inputFileName)).Length <= MaxCompress) // read file if not already and not too big
                rawRomData = File.ReadAllBytes(inputFileName);
            if (originalFileName == null) // Original file name from archive
                originalFileName = Path.GetFileName(inputFileName);

            // try to autodetect app type
            AppTypeCollection.AppInfo appInfo = AppTypeCollection.GetAppByExtension(ext);
            if (appInfo.Unknown)
            {
                IEnumerable<string> systems = CoreCollection.GetSystemsFromExtension(ext).Except(SystemCollection.RedundancyList);
                if(systems.Count() == 1)
                {
                    var system = systems.First();
                    appInfo = AppTypeCollection.GetAppBySystem(system);
                    if (appInfo.Unknown)
                    {
                        appInfo = new AppTypeCollection.AppInfo()
                        {
                            Name = system,
                            Class = typeof(LibretroGame),
                            DefaultCore = CoreCollection.GetCoresFromSystem(system).First().Bin,
                            LegacyApps = new string[] { },
                            Extensions = new string[] { },
                            Prefix = AppTypeCollection.GetAvailablePrefix(system),
                            DefaultCover = Resources.blank_app,
                            GoogleSuffix = "game",
                            Unknown = false
                        };
                    }
                }
            }

            CoreCollection.CoreInfo coreInfo = string.IsNullOrEmpty(appInfo.DefaultCore) ? null : CoreCollection.GetCore(appInfo.DefaultCore);
            string application;
            string args;
            char prefix;
            if (coreInfo != null)
            {
                Debug.WriteLine("Import Detected Core: " + coreInfo.DisplayName);
                application = coreInfo.QualifiedBin;
                args = coreInfo.DefaultArgs;
            }
            else
            {
                application = ext.Length > 2 ? ("/bin/" + ext.Substring(1)) : "";
                args = string.Empty;
            }
            if (appInfo.Unknown && appInfo.Prefix == 'Z' && !string.IsNullOrEmpty(ext))
            {
                prefix = AppTypeCollection.GetAvailablePrefix(ext);
            }
            else
            {
                prefix = appInfo.Prefix;
            }
            byte saveCount = 0;
            Image cover = appInfo.DefaultCover;
            uint crc32 = rawRomData != null ? Shared.CRC32(rawRomData) : Shared.CRC32(new FileStream(inputFileName, FileMode.Open, FileAccess.Read));
            string outputFileName = GenerateSafeFileName(Path.GetFileName(inputFileName));

            // only attempt patching if file is reasonable and fits in memory
            if (rawRomData != null)
            {
                bool patched = false;
                if (!appInfo.Unknown)
                {
                    var patch = appInfo.Class.GetMethod("Patch");
                    if (patch != null)
                    {
                        object[] values = new object[] { inputFileName, rawRomData, prefix, application, outputFileName, args, cover, saveCount, crc32 };
                        var result = (bool)patch.Invoke(null, values);
                        if (!result)
                            return null;

                        rawRomData = (byte[])values[1];
                        prefix = (char)values[2];
                        application = (string)values[3];
                        outputFileName = (string)values[4];
                        args = (string)values[5];
                        cover = (Image)values[6];
                        saveCount = (byte)values[7];
                        crc32 = (uint)values[8];
                        patched = true;

                        CoreCollection.CoreInfo newCoreInfo = CoreCollection.GetCoreFromExec(application);
                        if (newCoreInfo != coreInfo)
                        {
                            Debug.WriteLine("Patching override core: " + newCoreInfo.Name);
                            coreInfo = newCoreInfo;
                        }
                    }
                }

                // find ips patches if applicable
                if (!patched)
                    FindPatch(ref rawRomData, inputFileName, crc32);
            }

            // create directory and rom file
            var code = GenerateCode(crc32, prefix);
            var gamePath = Path.Combine(GamesDirectory, code);
            var romPath = Path.Combine(gamePath, outputFileName);
            if (Directory.Exists(gamePath))
            {
                Shared.DirectoryDeleteInside(gamePath);
            }
            Directory.CreateDirectory(gamePath);
            if (rawRomData != null)
            {
                File.WriteAllBytes(romPath, rawRomData);
            }
            else
            {
                File.Copy(inputFileName, romPath);
            }

            // save desktop file
            var game = new NesApplication(gamePath, null, true);
            var name = Path.GetFileNameWithoutExtension(inputFileName);
            name = Regex.Replace(name, @" ?\(.*?\)| ?\[.*?\]", string.Empty);
            name = name.Replace("_", " ").Replace("  ", " ").Trim();
            game.desktop.Name = name;
            game.desktop.Exec = $"{application} {hakchi.GamesPath}/{code}/{outputFileName} {args}";
            game.desktop.ProfilePath = hakchi.GamesProfilePath;
            game.desktop.IconPath = hakchi.GamesPath;
            game.desktop.Code = code;
            game.desktop.SaveCount = saveCount;
            game.Save();

            // save metadata file
            var metadata = new AppMetadata();
            metadata.System = appInfo.Unknown ? string.Empty : appInfo.Name;
            metadata.Core = coreInfo == null ? string.Empty : coreInfo.Bin;
            metadata.OriginalFilename = Path.GetFileName(inputFileName);
            metadata.OriginalCrc32 = crc32;
            metadata.Save(Path.Combine(gamePath, "metadata.json"));

            // recreate game object and finalize
            game = NesApplication.FromDirectory(gamePath);
            if (game is ICloverAutofill)
                (game as ICloverAutofill).TryAutofill(crc32);
            if (!game.FindCover(inputFileName, crc32, name))
                game.SetDefaultImage(cover);

            if (ConfigIni.Instance.Compress)
            {
                game.Compress();
                game.Save();
            }
            return game;
        }

        private static NesApplication ImportApp(string filename)
        {
            if (!File.Exists(filename) || Path.GetExtension(filename).ToLower() != ".desktop")
                throw new FileNotFoundException($"Invalid application file \"{filename}\"");
            var code = Path.GetFileNameWithoutExtension(filename).ToUpper();
            var targetDir = Path.Combine(GamesDirectory, code);
            Shared.DirectoryCopy(Path.GetDirectoryName(filename), targetDir, true, false, true, false);
            return FromDirectory(targetDir);
        }

        public static NesApplication CreateEmptyApp(string path, string name)
        {
            // create directory and rom file
            var code = Path.GetFileName(path);
            if (Directory.Exists(path))
            {
                Shared.DirectoryDeleteInside(path);
            }
            Directory.CreateDirectory(path);

            // save desktop file
            var game = new NesApplication(path, null, true);
            name = Regex.Replace(name, @" ?\(.*?\)| ?\[.*?\]", string.Empty).Trim();
            game.desktop.Name = name.Replace("_", " ").Replace("  ", " ").Trim();
            game.desktop.Exec = "/bin/enter-custom-command-here";
            game.desktop.ProfilePath = hakchi.GamesProfilePath;
            game.desktop.IconPath = hakchi.GamesPath;
            game.desktop.Code = code;
            game.desktop.SaveCount = 0;
            game.Save();
            game.SetDefaultImage(AppTypeCollection.UnknownApp.DefaultCover);

            return NesApplication.FromDirectory(path);
        }

        protected NesApplication() : base()
        {
            Metadata = new AppMetadata();
            isOriginalGame = false;
            isDeleting = false;
            CoverArtMatches = new string[0];
            CoverArtMatchSuccess = false;
        }

        protected NesApplication(string path, AppMetadata metadata = null, bool ignoreEmptyConfig = false)
            : base(path, ignoreEmptyConfig)
        {
            Metadata = metadata ?? new AppMetadata();

            isOriginalGame = false;
            isDeleting = false;
            isOriginalGame = AllDefaultGames.Contains(desktop.Code);

            CoverArtMatches = new string[0];
            CoverArtMatchSuccess = false;

            GameGeniePath = Path.Combine(path, GameGenieFileName);
            if (File.Exists(GameGeniePath))
                GameGenie = File.ReadAllText(GameGeniePath);
        }

        public override bool Save()
        {
            // safety when deleting
            if (IsDeleting)
                return false;

            // fix potentially problematic name and sort name
            desktop.Name = Regex.Replace(desktop.Name, @"'(\d)", @"`$1"); // Apostrophe + any number in game name crashes whole system. What. The. Fuck?
            desktop.SortName = Regex.Replace(desktop.SortName, @"'(\d)", @"`$1");
            if (string.IsNullOrEmpty(desktop.SortName))
            {
                string s = desktop.Name.ToLower();
                if (s.StartsWith("the "))
                    s = s.Substring(4); // Sorting without "THE"
                desktop.SortName = s;
            }

            // save .desktop file
            bool snesExtraFields = IsOriginalGame && (
                ConfigIni.Instance.ConsoleType == hakchi.ConsoleType.SNES_EUR ||
                ConfigIni.Instance.ConsoleType == hakchi.ConsoleType.SNES_USA ||
                ConfigIni.Instance.ConsoleType == hakchi.ConsoleType.SuperFamicom);
            desktop.Save($"{basePath}/{desktop.Code}.desktop", snesExtraFields);

            // game genie stuff
            if (!string.IsNullOrEmpty(GameGenie))
                File.WriteAllText(GameGeniePath, GameGenie);
            else if (File.Exists(GameGeniePath))
                File.Delete(GameGeniePath);

            return true;
        }

        public void SaveMetadata()
        {
            Metadata.Save($"{basePath}/metadata.json");
        }

        public bool Repair()
        {
            if (IsOriginalGame)
                return false;

            string[] excludedFiles = new string[] {
                    Desktop.Code + ".desktop",
                    Desktop.Code + ".png",
                    Desktop.Code + "_small.png",
                    "metadata.json" };
            string[] compressedFiles = new string[] { ".7z", ".zip" };

            string bin = Desktop.Bin;
            string core = string.IsNullOrEmpty(bin) ? string.Empty : bin.Substring(bin.LastIndexOf('/') + 1);
            string fullgamepath = "";
            string fullfilename = "";
            string path = "";
            string filename = "";
            string extension = "";
            string gameFile = "";

            // attempt to find game file according to exec
            Match m = Regex.Match(Desktop.Exec, @"([^\s]*\/...\-.\-.....\/)(.+?)(?:\s\-+|\s\/|$)");
            if (m.Success)
            {
                path = m.Groups[1].Value;
                fullfilename = m.Groups[2].Value.Trim();
                fullgamepath = path + fullfilename;
                filename = Path.GetFileNameWithoutExtension(fullfilename);
                extension = Path.GetExtension(fullfilename);
                gameFile = Shared.PathCombine(basePath, fullfilename);
            }

            // if we didn't find a match, attempt to detect a game file
            string[] foundFiles;
            if (!string.IsNullOrEmpty(gameFile) && File.Exists(gameFile))
            {
                foundFiles = new string[] { gameFile };
            }
            else
            {
                foundFiles = Directory.GetFiles(basePath, "*.*", SearchOption.TopDirectoryOnly).Where(
                    file => !excludedFiles.Contains(Path.GetFileName(file))).ToArray();
            }

            Debug.WriteLine("Found files: " + string.Join(", ", foundFiles.Select(file => Path.GetFileName(file))));

            List<string> acceptedFiles = new List<string>();
            foreach (var file in foundFiles)
            {
                int extPos = file.LastIndexOf('.');
                if (extPos > -1 && compressedFiles.Contains(file.Substring(extPos)))
                {
                    using (var extractor = new SevenZipExtractor(file))
                    {
                        if (extractor.FilesCount == 1)
                        {
                            var extractedFileName = extractor.ArchiveFileNames[0];
                            if (!File.Exists(Path.Combine(this.basePath, extractedFileName)))
                            {
                                extractor.ExtractArchive(this.basePath);
                                acceptedFiles.Add(Path.Combine(basePath, extractedFileName));
                                File.Delete(file);
                            }
                        }
                        else
                        {
                            acceptedFiles.Add(file);
                        }
                    }
                }
                else
                {
                    acceptedFiles.Add(file);
                }
            }

            Debug.WriteLine("Accepted files: " + string.Join(", ", acceptedFiles.Select(file => Path.GetFileName(file))));

            string selectedFile = string.Empty;
            if (acceptedFiles.Count == 0)
            {
                if (fullgamepath != "")
                    Desktop.Exec = Desktop.Exec.Replace(fullgamepath, "").Replace("  ", " ");
                Debug.WriteLine("No validated file found in game folder");
                return true;
            }
            else if (acceptedFiles.Count == 1)
            {
                selectedFile = Path.GetFileName(acceptedFiles[0]);
            }
            else
            {
                ParentForm.Invoke(new Action(()=>{
                    var form = new SelectFileForm(
                        acceptedFiles.Select(file => Path.GetFileName(file)).ToArray(),
                        string.Format(Resources.SelectFileFor, Desktop.Name),
                        Resources.Abort);
                    var result = form.ShowDialog();
                    if (form.listBoxFiles.SelectedItem != null)
                        selectedFile = form.listBoxFiles.SelectedItem.ToString();
                }));
                if (string.IsNullOrEmpty(selectedFile))
                    return false;
            }

            string newFileName = GenerateSafeFileName(Path.GetFileNameWithoutExtension(selectedFile)) + Path.GetExtension(selectedFile);
            if (newFileName != selectedFile)
            {
                if (File.Exists(Path.Combine(basePath, newFileName)))
                    throw new IOException($"A file of the generated filename \"{newFileName}\" already exists");
                File.Move(Path.Combine(basePath, selectedFile), Path.Combine(basePath, newFileName));
            }

            CoreCollection.CoreInfo coreInfo = CoreCollection.GetCoreFromExec(Desktop.Exec);
            if (coreInfo == null)
            {
                Desktop.Exec = bin + $" {hakchi.GamesPath}/{Desktop.Code}/{newFileName}";
            }
            else
            {
                Desktop.Exec = (coreInfo.QualifiedBin + $" {hakchi.GamesPath}/{Desktop.Code}/{newFileName} " + coreInfo.DefaultArgs).Trim();
            }

            if (ConfigIni.Instance.Compress)
                Compress();
            Save();
            return true;
        }

        public override Image Image
        {
            set
            {
                if (value == null)
                {
                    if (IsOriginalGame)
                    {
                        base.Image = null;
                        Save();
                    }
                    else
                    {
                        SetImage(AppTypeCollection.GetAppBySystem(Metadata.System).DefaultCover, false);
                    }
                    Metadata.CustomCoverArt = false; SaveMetadata();
                }
                else
                {
                    base.Image = value;
                    if (IsOriginalGame) Save();
                    Metadata.CustomCoverArt = true; SaveMetadata();
                }
            }
            get
            {
                try
                {
                    Image i = base.Image;
                    if (IsOriginalGame && i == null)
                    {
                        string cachedIconPath = Shared.PathCombine(OriginalGamesCacheDirectory, Code, Code + ".png");
                        return File.Exists(cachedIconPath) ? Shared.LoadBitmapCopy(cachedIconPath) : AppTypeCollection.GetAppBySystem(Metadata.System).DefaultCover;
                    }
                    return i;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Image loading error: " + ex.Message + ex.StackTrace);
                }
                return null;
            }
        }

        public override Image Thumbnail
        {
            get
            {
                try
                {
                    Image i = base.Thumbnail;
                    if (IsOriginalGame && i == null)
                    {
                        string cachedIconPath = Shared.PathCombine(OriginalGamesCacheDirectory, Code, Code + "_small.png");
                        return File.Exists(cachedIconPath) ? Image.FromFile(cachedIconPath) : Shared.ResizeImage(AppTypeCollection.GetAppBySystem(Metadata.System).DefaultCover, null, null, 40, 40, false, true, false, false);
                    }
                    return i;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Image loading error: " + ex.Message + ex.StackTrace);
                }
                return null;
            }
        }

        protected override void SetImage(Image img, bool EightBitCompression = false)
        {
            base.SetImage(img, EightBitCompression);
            Metadata.CustomCoverArt = true; SaveMetadata();
        }

        public override void SetImageFile(string path, bool EightBitCompression = false)
        {
            base.SetImageFile(path, EightBitCompression);
            Metadata.CustomCoverArt = true; SaveMetadata();
        }

        public override void SetThumbnailFile(string path, bool EightBitCompression = false)
        {
            base.SetThumbnailFile(path, EightBitCompression);
            Metadata.CustomCoverArt = true; SaveMetadata();
        }

        public void SetDefaultImage(Image img)
        {
            base.SetImage(img);
            Metadata.CustomCoverArt = false; SaveMetadata();
        }

        public bool FindCover(string inputFileName, uint crc32 = 0, string alternateTitle = null)
        {
            var artDirectory = Path.Combine(Program.BaseDirectoryExternal, "art");
            var imageExtensions = new string[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff" };
            var filename = Path.GetFileNameWithoutExtension(inputFileName);

            Directory.CreateDirectory(artDirectory);
            try
            {
                // init accessors
                CoverArtMatches = new string[0];
                CoverArtMatchSuccess = false;

                // first test for crc32 match (most precise)
                if (crc32 != 0)
                {
                    string[] covers = Directory.GetFiles(artDirectory, string.Format("{0:X8}*.*", crc32), SearchOption.AllDirectories);
                    if (covers.Length > 0 && imageExtensions.Contains(Path.GetExtension(covers[0])))
                    {
                        SetImageFile(covers[0], ConfigIni.Instance.CompressCover);
                        CoverArtMatches = new string[] { covers[0] };
                        return CoverArtMatchSuccess = true;
                    }
                }

                // test presence of image file alongside the source file, if inputFileName is fully qualified
                if (!string.IsNullOrEmpty(filename) && !string.IsNullOrEmpty(Path.GetDirectoryName(inputFileName)))
                {
                    foreach (var ext in imageExtensions)
                    {
                        var imagePath = Path.Combine(Path.GetDirectoryName(inputFileName), filename + ext);
                        if (File.Exists(imagePath))
                        {
                            SetImageFile(imagePath, ConfigIni.Instance.CompressCover);
                            CoverArtMatches = new string[] { imagePath };
                            return CoverArtMatchSuccess = true;
                        }
                    }
                }

                // if a system named subdirectory exists, use it
                if (!string.IsNullOrEmpty(Metadata.System) && Directory.Exists(Path.Combine(artDirectory, Metadata.System)))
                    artDirectory = Path.Combine(artDirectory, Metadata.System);

                // compiled list for the two fuzzy search passes
                var matches = new List<string>();

                // get cover files list
                if (CachedCoverFiles == null)
                {
                    CachedCoverFiles = Directory.GetFiles(artDirectory, "*.*", SearchOption.AllDirectories);
                }

                // first fuzzy search on inputFileName
                if (!string.IsNullOrEmpty(filename))
                {
                    var findResults = FindCoverMatch(filename, CachedCoverFiles);

                    string preciseMatch = string.Empty;
                    foreach (var cover in findResults)
                        if (cover.Value == false)
                        {
                            if (string.IsNullOrEmpty(preciseMatch))
                                preciseMatch = cover.Key;
                            else
                            {
                                preciseMatch = string.Empty;
                                break;
                            }
                        }
                    matches.AddRange(findResults.Select(pair => pair.Key));
                    if (!string.IsNullOrEmpty(preciseMatch))
                    {
                        SetImageFile(preciseMatch, ConfigIni.Instance.CompressCover);
                        CoverArtMatches = matches.ToArray();
                        return CoverArtMatchSuccess = true;
                    }
                }

                // second fuzzy search on alternateTitle
                if (!string.IsNullOrEmpty(alternateTitle))
                {
                    var findResults = FindCoverMatch(alternateTitle, CachedCoverFiles, true);

                    string preciseMatch = string.Empty;
                    foreach (var cover in findResults)
                        if (cover.Value == false)
                        {
                            if (string.IsNullOrEmpty(preciseMatch))
                                preciseMatch = cover.Key;
                            else
                            {
                                preciseMatch = string.Empty;
                                break;
                            }
                        }
                    matches.AddRange(findResults.Select(pair => pair.Key));
                    if (!string.IsNullOrEmpty(preciseMatch))
                    {
                        SetImageFile(preciseMatch, ConfigIni.Instance.CompressCover);
                        CoverArtMatches = matches.Distinct();
                        return CoverArtMatchSuccess = true;
                    }
                }

                // set accessor and remove duplicates
                CoverArtMatches = matches.Distinct();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error trying to find cover art: " + ex.Message + ex.StackTrace);
            }

            return CoverArtMatchSuccess = false;
        }

        private static IEnumerable<KeyValuePair<string, bool>> FindCoverMatch(string name, string[] covers, bool stripCodes = false)
        {
            var imageExtensions = new string[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff" };
            var regexStripCodes = new Regex(@"\s?\(.*?\)|\s?\[.*?\]", RegexOptions.Compiled);
            //var regexSanitize = new Regex(@"[^a-zA-Z0-9\&]", RegexOptions.Compiled);
            var regexSanitize = new Regex(@"[^\p{L}\p{Nd}0-9\&]", RegexOptions.Compiled);
            var regexTrim = new Regex(@"\s+", RegexOptions.Compiled);

            name = regexTrim.Replace(" " + regexSanitize.Replace(name, " ").ToLower() + " ", " ");
            //Debug.WriteLine("FindCoverMatch: " + name);

            if (!string.IsNullOrEmpty(name.Trim()))
            {
                // build word list, with exceptions
                List<string[]> words = new List<string[]>();
                foreach(string word in name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    //Debug.WriteLine($"\"{word}\"");
                    if (word == "&" || word == "and") // and, & can be used interchangeably, or omitted
                    {
                        words.Add(new string[] { "&", "and", string.Empty });
                    }
                    else if (word == "the") // the can be omitted
                    {
                        words.Add(new string[] { "the", string.Empty });
                    }
                    else if (Regex.IsMatch(word, @"^[0-9]{1,2}$")) // alternate short numbers to roman letters
                    {
                        words.Add(new string[] { word, Shared.IntegerToRoman(int.Parse(word)).ToLower() });
                    }
                    else if (Regex.IsMatch(word, @"^[ivx]+$")) // alternate short roman numbers to integer
                    {
                        words.Add(new string[] { word, Shared.RomanToInteger(word).ToString() });
                    }
                    else if (word.Length == 1) // add an alternative to one letter words
                    {
                        words.Add(new string[] { word, string.Empty });
                    }
                    else // otherwise, just add the word
                    {
                        words.Add(new string[] { word });
                    }
                }

                // scan files and compile results (precise and partial)
                var results = new List<KeyValuePair<string, bool>>();
                foreach (var file in covers)
                {
                    if (imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                    {
                        string sanitized = Path.GetFileNameWithoutExtension(file);
                        if (stripCodes)
                        {
                            sanitized = regexStripCodes.Replace(sanitized, "");
                        }
                        sanitized = regexTrim.Replace(" " + regexSanitize.Replace(sanitized, " ").ToLower() + " ", " ");
                        bool match, partial;
                        PuzzleMatch(words, sanitized, out match, out partial);
                        if (match)
                        {
                            results.Add(new KeyValuePair<string, bool>(file, partial));
                        }
                    }
                }

                return results;
            }

            return new Dictionary<string, bool>();
        }

        private static void PuzzleMatch(List<string[]> pieces, string puzzle, out bool match, out bool partial)
        {
            match = false;
            partial = false;

            var regexIteration = new Regex(@"\s[ivx]+\s|[0-9]{1,2}", RegexOptions.Compiled);
            if (pieces.Count == 0)
            {
                if(puzzle.Trim().Length == 0)
                {
                    match = true;
                    partial = false;
                }
                else
                {
                    if (!regexIteration.IsMatch(puzzle))
                    {
                        match = true;
                        partial = true;
                    }
                }
                return;
            }
            else if (puzzle.Trim().Length == 0)
            {
                string words = string.Empty;
                pieces.ForEach(delegate (string[] p) { words += " " + string.Join(" ", p); });
                words += " ";
                if (!regexIteration.IsMatch(words))
                {
                    match = true;
                    partial = true;
                }
                return;
            }

            var piece = pieces.First();
            foreach(var alt in piece)
            {
                if (string.IsNullOrEmpty(alt))
                {
                    PuzzleMatch(pieces.Skip(1).ToList(), string.Copy(puzzle), out match, out partial);
                }
                else
                {
                    int position = 0;
                    while (position < puzzle.Length && position != -1 && !match)
                    {
                        position = puzzle.IndexOf(alt, position);
                        if (position != -1)
                        {
                            var remainingPieces = pieces.Skip(1).ToList();
                            var remainingPuzzle = puzzle.Remove(position++, alt.Length);
                            PuzzleMatch(remainingPieces, remainingPuzzle, out match, out partial);
                        }
                    }
                }
                if (match) break;
            }
        }

        protected static bool FindPatch(ref byte[] rawRomData, string inputFileName, uint crc32 = 0)
        {
            string patch = null;
            var patchesDirectory = Path.Combine(Program.BaseDirectoryExternal, "patches");
            Directory.CreateDirectory(patchesDirectory);
            if (!string.IsNullOrEmpty(inputFileName))
            {
                if (crc32 != 0)
                {
                    var patches = Directory.GetFiles(patchesDirectory, string.Format("{0:X8}*.*", crc32), SearchOption.AllDirectories);
                    if (patches.Length > 0)
                        patch = patches[0];
                }
                var patchesPath = Path.Combine(patchesDirectory, Path.GetFileNameWithoutExtension(inputFileName) + ".ips");
                if (File.Exists(patchesPath))
                    patch = patchesPath;
                patchesPath = Path.Combine(Path.GetDirectoryName(inputFileName), Path.GetFileNameWithoutExtension(inputFileName) + ".ips");
                if (File.Exists(patchesPath))
                    patch = patchesPath;
            }

            if (!string.IsNullOrEmpty(patch))
            {
                if (NeedPatch != true)
                {
                    if (NeedPatch != false)
                    {
                        var result = Tasks.MessageForm.Show(ParentForm, Resources.PatchAvailable,
                            string.Format(Resources.PatchQ, Path.GetFileName(inputFileName)),
                            Resources.sign_question,
                            new Tasks.MessageForm.Button[] { Tasks.MessageForm.Button.YesToAll, Tasks.MessageForm.Button.Yes, Tasks.MessageForm.Button.No },
                            Tasks.MessageForm.DefaultButton.Button2);
                        if (result == Tasks.MessageForm.Button.YesToAll)
                            NeedPatch = true;
                        if (result == Tasks.MessageForm.Button.No)
                            return false;
                    }
                    else return false;
                }
                IpsPatcher.Patch(patch, ref rawRomData); // TODO
                return true;
            }
            return false;
        }

        public static string GenerateSafeFileName(string input)
        {
            string f = Path.GetFileNameWithoutExtension(input);
            string e = Path.GetExtension(input);
            f = Regex.Replace(f, @" ?\(.*?\)| ?\[.*?\]", string.Empty);
            f = Regex.Replace(f, @"[^A-Za-z0-9\.\!]+", "_");
            f = Regex.Replace(f, @"_+", "_");
            int maxLength = 64 - ".sfrom.lzma".Length;
            if(f.Length > maxLength) // 64 characters, leave room for longest extension: '.sfrom.lzma'
            {
                Debug.WriteLine($"'{f}' l:{f.Length}");
                f = f.Substring(0, maxLength);
                Debug.WriteLine($"'{f}' l:{f.Length}");
            }
            string output = f.Trim(new char[] { '_', '.' }) + e;
            return output;
        }

        public static string GenerateCode(uint crc32, char prefixCode)
        {
            return string.Format("CLV-{5}-{0}{1}{2}{3}{4}",
                (char)('A' + (crc32 % 26)),
                (char)('A' + (crc32 >> 5) % 26),
                (char)('A' + ((crc32 >> 10) % 26)),
                (char)('A' + ((crc32 >> 15) % 26)),
                (char)('A' + ((crc32 >> 20) % 26)),
                prefixCode);
        }

        // --- adjusted desktop file for export/upload
        private DesktopFile GetAdjustedDesktopFile(string mediaGamePath, string iconPath, string profilePath)
        {
            DesktopFile newdesktop = (DesktopFile)desktop.Clone();
            foreach (var arg in newdesktop.Args)
            {
                Match m = Regex.Match(arg, @"(^\/.*)\/(?:" + newdesktop.Code + @"\/)([^.]*)(.*$)");
                if (m.Success)
                {
                    newdesktop.Exec = newdesktop.Exec.Replace(m.Groups[1].ToString(), mediaGamePath);
                    break;
                }
            }
            newdesktop.IconPath = iconPath;
            newdesktop.ProfilePath = profilePath;
            return newdesktop;
        }

        // --- patch desktop file
        private ApplicationFileInfo GetDesktopApplicationFileInfo(string relativeTargetPath, string mediaGamePath, string iconPath, string profilePath)
        {
            var newdesktop = GetAdjustedDesktopFile(mediaGamePath, iconPath, profilePath);
            var f = new FileInfo(Path.Combine(basePath, newdesktop.Code + ".desktop"));
            var canonicalPath = $"./{relativeTargetPath}/{desktop.Code}/{desktop.Code}.desktop";

            return new ApplicationFileInfo(
                canonicalPath, f.LastWriteTimeUtc, newdesktop.SaveTo(new MemoryStream()));
        }

        // --- normal sync
        private HashSet<ApplicationFileInfo> CopyToSync(string relativeTargetPath)
        {
            string targetDir = $"{relativeTargetPath}/{desktop.Code}";

            string mediaGamePath = hakchi.GamesPath;
            string iconPath = hakchi.GamesPath;
            if (IsOriginalGame)
            {
                mediaGamePath = hakchi.SquashFsPath + hakchi.GamesSquashFsPath;
                if (!File.Exists(this.iconPath))
                    iconPath = hakchi.SquashFsPath + hakchi.GamesSquashFsPath;
            }
            HashSet<ApplicationFileInfo> gameSet = ApplicationFileInfo.GetApplicationFileInfoForDirectory(
                basePath, targetDir, true, new string[] { desktop.Code + ".desktop" });
            gameSet.Add(GetDesktopApplicationFileInfo(
                relativeTargetPath, mediaGamePath, iconPath, hakchi.GamesProfilePath));
            return gameSet;
        }

        // --- linked sync
        private HashSet<ApplicationFileInfo> CopyToSyncLinked(string relativeTargetPath)
        {
            string targetDir = $"{relativeTargetPath}/{desktop.Code}";
            string targetStorageDir = $".storage/{desktop.Code}";

            string mediaGamePath = hakchi.GetRemoteGameSyncPath(ConfigIni.Instance.ConsoleType) + "/.storage";
            string iconPath = mediaGamePath;
            if (IsOriginalGame)
            {
                mediaGamePath = hakchi.SquashFsPath + hakchi.GamesSquashFsPath;
                if (!File.Exists(this.iconPath))
                    iconPath = hakchi.SquashFsPath + hakchi.GamesSquashFsPath;
            }
            HashSet<ApplicationFileInfo> gameSet = ApplicationFileInfo.GetApplicationFileInfoForDirectory(
                basePath, targetStorageDir, true, new string[] { desktop.Code + ".desktop" });
            gameSet.Add(GetDesktopApplicationFileInfo(
                relativeTargetPath, mediaGamePath, iconPath, hakchi.GamesProfilePath));
            return gameSet;
        }

        // --- normal export
        private HashSet<ApplicationFileInfo> CopyToExport(string relativeTargetPath)
        {
            string targetDir = $"{relativeTargetPath}/{desktop.Code}";

            string mediaGamePath = hakchi.GamesPath;
            string iconPath = hakchi.GamesPath;

            HashSet<ApplicationFileInfo> gameSet = ApplicationFileInfo.GetApplicationFileInfoForDirectory(
                basePath, targetDir, true, new string[] { desktop.Code + ".desktop" });

            if (IsOriginalGame)
            {
                string originalBasePath = Path.Combine(OriginalGamesCacheDirectory, desktop.Code);
                if (Directory.Exists(originalBasePath))
                {
                    HashSet<ApplicationFileInfo> supplementalGameSet = ApplicationFileInfo.GetApplicationFileInfoForDirectory(
                        originalBasePath, targetDir, true, new string[] { desktop.Code + ".desktop" });
                    gameSet = gameSet.CopyFilesTo(supplementalGameSet, false);
                }

                bool romExists = false;
                bool iconExists = false;
                foreach (var afi in gameSet)
                {
                    if (Regex.IsMatch(afi.FilePath, @"[.](?:sfrom|qd|nes)$")) romExists = true;
                    if (afi.FilePath.EndsWith(desktop.Code + ".png")) iconExists = true;
                    if (romExists && iconExists) break;
                }

                if (!romExists)
                    mediaGamePath = hakchi.SquashFsPath + hakchi.GamesSquashFsPath;
                if (!iconExists)
                    iconPath = hakchi.SquashFsPath + hakchi.GamesSquashFsPath;
            }

            gameSet.Add(GetDesktopApplicationFileInfo(
                relativeTargetPath, mediaGamePath, iconPath, hakchi.GamesProfilePath));
            return gameSet;
        }

        // --- linked export
        private HashSet<ApplicationFileInfo> CopyToExportLinked(string relativeTargetPath)
        {
            string targetDir = $"{relativeTargetPath}/{desktop.Code}";
            string mediaGamePath = hakchi.MediaPath + GamesDirectory.Substring(2).Replace("\\", "/");
            string iconPath = mediaGamePath;

            var gameSet = new HashSet<ApplicationFileInfo>();
            if (IsOriginalGame)
            {
                mediaGamePath = hakchi.MediaPath + OriginalGamesDirectory.Substring(2).Replace("\\", "/");
                iconPath = mediaGamePath;

                var originalExtensions = new string[] { ".sfrom", ".qd", ".nes" };
                bool foundIcon = File.Exists(this.iconPath);
                bool foundRom = false;
                foreach (var file in Directory.GetFiles(basePath, "*.*"))
                    if (originalExtensions.Contains(Path.GetExtension(file)))
                        foundRom = true;

                string originalCacheMediaGamePath = hakchi.MediaPath + OriginalGamesCacheDirectory.Substring(2).Replace("\\", "/");
                string originalCacheBasePath = Path.Combine(OriginalGamesCacheDirectory, desktop.Code);
                if (Directory.Exists(originalCacheBasePath))
                {
                    // handle copying or linking original games rom and icons
                    if (!foundRom || !foundIcon)
                        foreach (var file in Directory.GetFiles(originalCacheBasePath, "*.*"))
                        {
                            if (!foundRom && originalExtensions.Contains(Path.GetExtension(file)))
                            {
                                mediaGamePath = originalCacheMediaGamePath;
                                foundRom = true;
                            }
                            if (!foundIcon && Path.GetFileName(file).Equals(desktop.Code + ".png"))
                            {
                                iconPath = originalCacheMediaGamePath;
                                foundIcon = true;
                            }
                            if (foundRom && foundIcon) break;
                        }

                    // copy autoplay and pixelart folders when they are in cache
                    if (Directory.Exists(Path.Combine(originalCacheBasePath, "autoplay")))
                    {
                        gameSet.UnionWith(ApplicationFileInfo.GetApplicationFileInfoForDirectory(
                            Path.Combine(originalCacheBasePath, "autoplay"), $"{targetDir}/autoplay"));
                    }
                    if (Directory.Exists(Path.Combine(originalCacheBasePath, "pixelart")))
                    {
                        gameSet.UnionWith(ApplicationFileInfo.GetApplicationFileInfoForDirectory(
                            Path.Combine(originalCacheBasePath, "pixelart"), $"{targetDir}/pixelart"));
                    }
                }

                if (!foundRom)
                    mediaGamePath = hakchi.SquashFsPath + hakchi.GamesSquashFsPath;
                if (!foundIcon)
                    iconPath = hakchi.SquashFsPath + hakchi.GamesSquashFsPath;
            }
            else
            {
                if (File.Exists(Path.Combine(basePath, GameGenieFileName)))
                {
                    gameSet.Add(ApplicationFileInfo.GetApplicationFileInfo(GameFilePath, targetDir));
                    mediaGamePath = hakchi.GamesPath;
                }
            }

            // add adjusted desktop file
            gameSet.Add(
                GetDesktopApplicationFileInfo(relativeTargetPath, mediaGamePath, iconPath, hakchi.GamesProfilePath));

            return gameSet;
        }

        // --- legacy
        public NesApplication CopyTo(string path)
        {
            string targetDir = Path.Combine(path, desktop.Code);
            Shared.DirectoryCopy(basePath, targetDir, true, false, true, false);
            return FromDirectory(targetDir);
        }

        // --- used to calculate file set when syncing/exporting
        public enum CopyMode { Sync, LinkedSync, Export, LinkedExport }
        public long CopyTo(string relativeTargetPath, HashSet<ApplicationFileInfo> localGameSet, CopyMode copyMode)
        {
            HashSet<ApplicationFileInfo> gameSet = null;
            switch (copyMode)
            {
                case CopyMode.Sync:
                    gameSet = CopyToSync(relativeTargetPath);
                    break;
                case CopyMode.LinkedSync:
                    gameSet = CopyToSyncLinked(relativeTargetPath);
                    break;
                case CopyMode.Export:
                    gameSet = CopyToExport(relativeTargetPath);
                    break;
                case CopyMode.LinkedExport:
                    gameSet = CopyToExportLinked(relativeTargetPath);
                    break;
            }
            localGameSet.UnionWith(gameSet);
            return gameSet.GetSize(hakchi.BLOCK_SIZE);
        }

        public static readonly string[] nonCompressibleExtensions = { ".7z", ".zip", "*.rar", ".hsqs", ".sh", ".pbp", ".chd" };
        public static readonly string[] compressedExtensions = { ".7z", ".zip" };
        public string[] CompressPossible()
        {
            if (!Directory.Exists(basePath)) return new string[0];
            var result = new List<string>();
            var exec = Regex.Replace(desktop.Exec, "[/\\\"]", " ") + " ";
            var files = Directory.GetFiles(basePath, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                if (nonCompressibleExtensions.Contains(Path.GetExtension(file).ToLower()))
                    continue;
                if (exec.Contains(" " + Path.GetFileName(file) + " ")) { 
                    if ((new FileInfo(file)).Length <= MaxCompress)
                        result.Add(file);
                }
            }
            return result.ToArray();
        }

        public string[] DecompressPossible()
        {
            if (!Directory.Exists(basePath)) return new string[0];
            var result = new List<string>();
            var exec = Regex.Replace(desktop.Exec, "[/\\\"]", " ") + " ";
            var files = Directory.GetFiles(basePath, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                foreach (var e in compressedExtensions)
                    if (file.ToLower().EndsWith(e) && exec.Contains(" " + Path.GetFileName(file) + " ") &&
                        new SevenZipExtractor(file).FilesCount == 1)
                    {
                        result.Add(file);
                        break;
                    }
            }
            return result.ToArray();
        }

        public void Compress()
        {
            foreach (var filename in CompressPossible())
            {
                var archName = filename + ".7z";
                var compressor = new SevenZipCompressor();
                compressor.CompressionLevel = CompressionLevel.High;
                Debug.WriteLine("Compressing " + filename);
                compressor.CompressFiles(archName, filename);
                Thread.Sleep(1);
                File.Delete(filename);
                desktop.Exec = desktop.Exec.Replace(Path.GetFileName(filename), Path.GetFileName(archName));
            }
        }

        public void Decompress()
        {
            foreach (var filename in DecompressPossible())
            {
                using (var szExtractor = new SevenZipExtractor(filename))
                {
                    Debug.WriteLine("Decompressing " + filename);
                    szExtractor.ExtractArchive(basePath);
                    foreach (var f in szExtractor.ArchiveFileNames)
                        desktop.Exec = desktop.Exec.Replace(Path.GetFileName(filename), f);
                }
                Thread.Sleep(1);
                File.Delete(filename);
            }
        }

        public class NesAppEqualityComparer : IEqualityComparer<NesApplication>
        {
            public bool Equals(NesApplication x, NesApplication y)
            {
                return x.Code == y.Code;
            }

            public int GetHashCode(NesApplication obj)
            {
                return obj.Code.GetHashCode();
            }
        }

    }
}
