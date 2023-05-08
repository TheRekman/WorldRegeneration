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
                var oldPath = Main.WorldPath;
                Main.WorldPath = path;
                Terraria.IO.WorldFile.SaveWorld(false);
                Main.WorldPath = oldPath;
                return;
            }    
            WorldSaver.SaveWorldSection(x, y, x2, y2, path);
        }

        public static void LoadWorldSection(string path, Rectangle rect, bool useRect = false, bool informPlayers = true)
        {
            if (WorldRegeneration.Config.UseVanillaWorldFiles)
            {
                var loader = new VanillaWorldLoader();
                loader.LoadWorld(path);
                return;
            }
            WorldLoader.LoadWorldSection(path, rect, useRect, informPlayers);
        }

        public static void RegenerateWorld(string path) => SaveWorldSection(0, 0, Main.maxTilesX, Main.maxTilesY, path);
    }
}
