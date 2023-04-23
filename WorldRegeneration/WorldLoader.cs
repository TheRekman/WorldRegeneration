using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria;
using TShockAPI;
using Microsoft.Xna.Framework;

namespace WorldRegeneration
{
    public static class WorldLoader
    {
        public static void LoadWorldSection(string path, Rectangle rect, bool useRect = false, bool informPlayers = true)
        {
            Task.Factory.StartNew(() =>
            {
                using (var reader = new BinaryReader(new GZipStream(new FileStream(path, FileMode.Open), CompressionMode.Decompress)))
                {
                    LoadWorldInfo(reader);
                    LoadTilesInArea(rect, useRect, reader);
                    if (useRect)
                        return;
                    LoadChests(reader);
                    LoadSigns(reader);
                    LoadTileEntity(reader);
                    if (WorldRegeneration.Config.ResetWorldGenStatus)
                        ResetWorldData();
                    if (informPlayers)
                        TSPlayer.All.SendInfoMessage("Successfully regenerated the world.");
                }
            });
        }

        public static void RegenerateWorld(string path) =>
            LoadWorldSection(path, Rectangle.Empty, false, true);

        private static void LoadWorldInfo(BinaryReader reader)
        {
            Main.worldSurface = reader.ReadDouble();
            Main.rockLayer = reader.ReadDouble();
            Main.dungeonX = reader.ReadInt32();
            Main.dungeonY = reader.ReadInt32();
            WorldGen.crimson = reader.ReadBoolean();

            WorldGen.SavedOreTiers.Copper = reader.ReadInt32();
            WorldGen.SavedOreTiers.Silver = reader.ReadInt32();
            WorldGen.SavedOreTiers.Iron = reader.ReadInt32();
            WorldGen.SavedOreTiers.Gold = reader.ReadInt32();

            WorldGen.SavedOreTiers.Cobalt = reader.ReadInt32();
            WorldGen.SavedOreTiers.Mythril = reader.ReadInt32();
            WorldGen.SavedOreTiers.Adamantite = reader.ReadInt32();
        }

        private static void LoadTilesInArea(Rectangle rect, bool useRect, BinaryReader reader, bool informPlayers = true)
        {
            reader.ReadInt32();
            reader.ReadInt32();

            int x = 0;
            int y = 0;

            int x2 = reader.ReadInt32();
            int y2 = reader.ReadInt32();

            for (int i = x; i <= x2; i++)
            {
                for (int j = y; j <= y2; j++)
                {
                    Tile tile = reader.ReadTile();
                    if (i >= 0 && j >= 0 && i < Main.maxTilesX && j < Main.maxTilesY)
                    {

                        if (TShock.Regions.InAreaRegion(i, j).Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                        {
                            continue;
                        }
                        else if (useRect)
                            if (rect.Contains(i, j))
                                Main.tile[i, j] = tile;
                            else
                                continue;
                        else
                        {
                            Main.tile[i, j] = tile; // Paste Tiles
                        }
                    }
                }
            }
            if (informPlayers)
                TSPlayer.All.SendInfoMessage("Tile Data Loaded...");
            ResetSection(x, y, x2, y2);
        }

        private static void LoadTileEntity(BinaryReader reader, bool informPlayers = true)
        {
            int totalTileEntities = reader.ReadInt32();
            int num1 = 0;
            for (int i = 0; i < totalTileEntities; i++)
            {
                TileEntity tileEntity = TileEntity.Read(reader);
                for (int j = 0; j < 1000; j++)
                {
                    TileEntity entityUsed;
                    if (TileEntity.ByID.TryGetValue(j, out entityUsed))
                    {
                        if (entityUsed.Position == tileEntity.Position)
                        {
                            break;
                        }
                        continue;
                    }
                    else
                    {
                        tileEntity.ID = j;
                        TileEntity.ByID[tileEntity.ID] = tileEntity;
                        TileEntity.ByPosition[tileEntity.Position] = tileEntity;
                        TileEntity.TileEntitiesNextID = j++;
                        num1++;
                        break;
                    }
                }
            }
            if (informPlayers)
                TSPlayer.All.SendInfoMessage("{0} of {1} Tile Entity Data Loaded...", num1, totalTileEntities);
        }

        private static void LoadSigns(BinaryReader reader, bool informPlayers = true)
        {
            int totalSigns = reader.ReadInt32();
            int signs = 0;
            int index = 0;
            for (int a = 0; a < totalSigns; a++)
            {
                Sign sign = reader.ReadSign();
                for (int s = index; s < Main.sign.Length; s++)
                {
                    if (TShock.Regions.InAreaRegion(sign.x, sign.y).Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                    {
                        break;
                    }
                    else if (Main.sign[s] != null && TShock.Regions.InAreaRegion(Main.sign[s].x, Main.sign[s].y).Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                    {
                        index++;
                        continue;
                    }
                    else
                    {
                        Main.sign[s] = sign;
                        index++;
                        signs++;
                        break;
                    }
                }
            }
            if (informPlayers)
                TSPlayer.All.SendInfoMessage("{0} of {1} Signs Data Loaded...", signs, totalSigns);
        }

        private static void LoadChests(BinaryReader reader, bool informPlayers = true)
        {
            int totalChests = reader.ReadInt32();
            int chests = 0;
            int index = 0;
            if (!WorldRegeneration.Config.IgnoreChests)
            {
                for (int a = 0; a < totalChests; a++)
                {
                    Chest chest = reader.ReadChest();
                    for (int c = index; c < Main.chest.Length; c++)
                    {
                        if (TShock.Regions.InAreaRegion(chest.x, chest.y).Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                        {
                            break;
                        }
                        else if (Main.chest[c] != null
                                 && TShock.Regions.InAreaRegion(Main.chest[c].x, Main.chest[c].y)
                                                  .Any(r => r != null
                                                         && r.Z >= WorldRegeneration.Config.MaxZRegion))
                        {
                            index++;
                            continue;
                        }
                        else
                        {
                            Main.chest[c] = chest;
                            index++;
                            chests++;
                            break;
                        }
                    }
                }
                if (informPlayers)
                    TSPlayer.All.SendInfoMessage("{0} of {1} Chest Data Loaded...", chests, totalChests);
            }
            else
            {
                for (int a = 0; a < totalChests; a++)
                {
                    reader.ReadChest();
                }
                if (informPlayers)
                    TSPlayer.All.SendInfoMessage("{0} Chest Data Ignored...", totalChests);
            }
        }

        public static Tile ReadTile(this BinaryReader reader)
        {
            Tile tile = new Tile();
            tile.sTileHeader = reader.ReadUInt16();
            tile.bTileHeader = reader.ReadByte();
            tile.bTileHeader2 = reader.ReadByte();

            // Tile type
            if (tile.active())
            {
                tile.type = reader.ReadUInt16();
                if (Main.tileFrameImportant[tile.type])
                {
                    tile.frameX = reader.ReadInt16();
                    tile.frameY = reader.ReadInt16();
                }
            }
            tile.wall = reader.ReadUInt16();
            tile.liquid = reader.ReadByte();
            return tile;
        }

        public static Chest ReadChest(this BinaryReader reader)
        {
            Chest chest = new Chest(false);
            chest.x = reader.ReadInt32();
            chest.y = reader.ReadInt32();
            chest.name = "World Chest";
            for (int l = 0; l < 40; l++)
            {
                Item item = new Item();
                int stack = reader.ReadInt16();
                if (stack > 0)
                {
                    int netID = reader.ReadInt32();
                    byte prefix = reader.ReadByte();
                    item.netDefaults(netID);
                    item.stack = stack;
                    item.Prefix(prefix);
                }
                chest.item[l] = item;
            }
            return chest;
        }

        public static Sign ReadSign(this BinaryReader reader)
        {
            Sign sign = new Sign();
            sign.text = reader.ReadString();
            sign.x = reader.ReadInt32();
            sign.y = reader.ReadInt32();
            return sign;
        }

        public static void ResetSection(int x, int y, int x2, int y2)
        {
            int lowX = Netplay.GetSectionX(x);
            int highX = Netplay.GetSectionX(x2);
            int lowY = Netplay.GetSectionY(y);
            int highY = Netplay.GetSectionY(y2);
            foreach (RemoteClient sock in Netplay.Clients.Where(s => s.IsActive))
            {
                for (int i = lowX; i <= highX; i++)
                {
                    for (int j = lowY; j <= highY; j++)
                        sock.TileSections[i, j] = false;
                }
            }
        }

        public static void ResetWorldData()
        {
            Main.hardMode = false;
            NPC.downedBoss1 = false;
            NPC.downedBoss2 = false;
            NPC.downedBoss3 = false;
            NPC.downedQueenBee = false;
            NPC.downedSlimeKing = false;
            NPC.downedMechBossAny = false;
            NPC.downedMechBoss1 = false;
            NPC.downedMechBoss2 = false;
            NPC.downedMechBoss3 = false;
            NPC.downedFishron = false;
            NPC.downedMartians = false;
            NPC.downedAncientCultist = false;
            NPC.downedMoonlord = false;
            NPC.downedHalloweenKing = false;
            NPC.downedHalloweenTree = false;
            NPC.downedChristmasIceQueen = false;
            NPC.downedChristmasSantank = false;
            NPC.downedChristmasTree = false;
            NPC.downedPlantBoss = false;
            NPC.savedStylist = false;
            NPC.savedGoblin = false;
            NPC.savedWizard = false;
            NPC.savedMech = false;
            NPC.downedGoblins = false;
            NPC.downedClown = false;
            NPC.downedFrost = false;
            NPC.downedPirates = false;
            NPC.savedAngler = false;
            NPC.downedMartians = false;
            NPC.downedGolemBoss = false;
            NPC.savedTaxCollector = false;
            WorldGen.shadowOrbSmashed = false;
            WorldGen.altarCount = 0;
            WorldGen.shadowOrbCount = 0;
        }
    }
}
