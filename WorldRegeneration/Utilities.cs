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
        public static void SaveWorldSection(int x, int y, int x2, int y2, string path) =>
            WorldSaver.SaveWorldSection(x, y, x2, y2, path);

        public static void LoadWorldSection(string path, Rectangle rect, bool useRect = false, bool informPlayers = true) =>
            WorldLoader.LoadWorldSection(path, rect, useRect, informPlayers);

        public static void RegenerateWorld(string path) => WorldLoader.RegenerateWorld(path);
    }
}
