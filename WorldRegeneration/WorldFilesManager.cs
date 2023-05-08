using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace WorldRegeneration
{
    public class WorldFilesManager
    {
        private string _worldsPath;
        private string _worldFormat;
        private string _specificWorldName;
        private bool _useSpecificWorld;

        private Regex WorldFileParser => new Regex(_worldFormat.SFormat(@"(?<name>\w+)", @"(?<id>\w+)") + @"\.(wld|twd)");
        public static string GetMainSaveFolder => Path.Combine(Main.SavePath, "Worlds");
        public static string GetLocalSaveFolder => Path.Combine(TShock.SavePath, "WorldRegen");

        public WorldFilesManager(string worldsPath, string worldFormat, string specificWorldName, bool useSpecificWorld)
        {
            _worldsPath = worldsPath;
            _worldFormat = worldFormat;
            _specificWorldName = specificWorldName;
            _useSpecificWorld = useSpecificWorld;
        }

        public IEnumerable<string> GetFiles()
        {
            var regex = new Regex(@"{\d+}");
            var worldFormat = regex.Replace(_worldFormat, "*");
            return Directory.EnumerateFiles(_worldsPath, worldFormat);
        }

        public string GetWorldFilePathByID(string id)
        {
            var files = GetFiles();
            var regex = WorldFileParser;
            var v = GetFiles().Select(s =>
            {
                var match = regex.Match(Path.GetFileName(s));
                return (Path: s, Name: match.Groups["name"].Value, Id: match.Groups["id"].Value);
            }).Where(f => f.Id == id);
            if (v.Count() > 1)
                v = v.Where(m => Main.worldName == m.Name);
            if (!(v.Count() > 0))
                return null;
            return v.First().Path;
        }

        public (string FullPath, string Name, string Id) GetWorldFileInfo(string path)
        {
            var match = WorldFileParser.Match(Path.GetFileName(path));
            return (path, match.Groups["name"].Value, match.Groups["id"].Value);
        }

        public string GetCurrentWorldPath()
        {
            var files = GetFiles();
            if(_useSpecificWorld)
                return files.FirstOrDefault(s => Path.GetFileName(s).Split('.')[0] == _specificWorldName);
            var worldId = Main.worldID;
            return GetWorldFilePathByID(worldId.ToString());
        }

        public string GenerateWorldPath() => Path.Combine(_worldsPath, _worldFormat.SFormat(Main.worldName, Main.worldID));
        public string GenerateWorldPath(string name, string id) => Path.Combine(_worldsPath, _worldFormat.SFormat(name, id));
    }
}
