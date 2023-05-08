using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using TShockAPI;
using TShockAPI.DB;
using Microsoft.Xna.Framework;

namespace WorldRegeneration
{
    public static class Utilities
    {
        public static void SaveWorldSection(int x, int y, int x2, int y2, string path)
        {
            if(WorldRegeneration.Config.UseVanillaWorldFiles)
            {
                var oldPath = Main.ActiveWorldFileData.Path;
                var oldName = Main.worldName;
                Main.ActiveWorldFileData._path = path;
                Main.worldName = Path.GetFileName(path).Split('.')[0];
                Terraria.IO.WorldFile.SaveWorld(false);
                Main.ActiveWorldFileData._path = oldPath; 
                Main.worldName = oldName;
                return;
            }    
            WorldSaver.SaveWorldSection(x, y, x2, y2, path);
        }

        public static void LoadWorldSection(string path, Rectangle rect, bool useRect = false, bool informPlayers = true)
        {
            if (WorldRegeneration.Config.UseVanillaWorldFiles)
            {
                var settings = new WorldLoadSettings() { LoadChests = !WorldRegeneration.Config.IgnoreChests };
                var loader = new VanillaWorldLoader(settings);
                loader.LoadWorld(path);
                return;
            }
            WorldLoader.LoadWorldSection(path, rect, useRect, informPlayers);
        }

        public static void RegenerateWorld(string path) => Utilities.LoadWorldSection(path, Rectangle.Empty, false);
    }
}
