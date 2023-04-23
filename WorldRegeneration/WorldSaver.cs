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

namespace WorldRegeneration
{
    public static class WorldSaver
    {
        const int BUFFER_SIZE = 1048576;

        public static void SaveWorldSection(int x, int y, int x2, int y2, string path)
        {
            // GZipStream is already buffered, but it's much faster to have a 1 MB buffer.
            using (var writer =
                new BinaryWriter(
                    new BufferedStream(
                        new GZipStream(File.Open(path, FileMode.Create), CompressionMode.Compress), BUFFER_SIZE)))
            {
                writer.Write(Main.worldSurface);
                writer.Write(Main.rockLayer);
                writer.Write(Main.dungeonX);
                writer.Write(Main.dungeonY);
                writer.Write(WorldGen.crimson);

                writer.Write(WorldGen.SavedOreTiers.Copper);
                writer.Write(WorldGen.SavedOreTiers.Silver);
                writer.Write(WorldGen.SavedOreTiers.Iron);
                writer.Write(WorldGen.SavedOreTiers.Gold);

                writer.Write(WorldGen.SavedOreTiers.Cobalt);
                writer.Write(WorldGen.SavedOreTiers.Mythril);
                writer.Write(WorldGen.SavedOreTiers.Adamantite);

                writer.Write(x);
                writer.Write(y);
                writer.Write(x2);
                writer.Write(y2);

                for (int i = x; i <= x2; i++)
                {
                    for (int j = y; j <= y2; j++)
                    {
                        writer.Write(Main.tile[i, j] ?? new Tile());
                    }
                }
                TSPlayer.All.SendInfoMessage("Tile Data Saved...");

                #region Chest Data
                int totalChests = 0;
                for (int i = 0; i < Main.chest.Length; i++)
                {
                    Chest chest = Main.chest[i];
                    if (chest != null)
                        totalChests++;
                }

                writer.Write(totalChests);
                for (int i = 0; i < Main.chest.Length; i++)
                {
                    Chest chest = Main.chest[i];
                    if (chest != null)
                        writer.WriteChest(chest);
                }
                TSPlayer.All.SendInfoMessage("{0} Chest Data Saved...", totalChests);
                #endregion

                #region Sign Data
                int totalSigns = 0;
                for (int i = 0; i < Main.sign.Length; i++)
                {
                    Sign sign = Main.sign[i];
                    if (sign != null)
                    {
                        totalSigns++;
                    }
                }

                writer.Write(totalSigns);
                for (int i = 0; i < Main.sign.Length; i++)
                {
                    Sign sign = Main.sign[i];
                    if (sign != null && sign.text != null)
                        writer.WriteSign(sign);
                }
                TSPlayer.All.SendInfoMessage("{0} Sign Data Saved...", totalSigns);
                #endregion

                #region Tile Entitity Data
                writer.Write(TileEntity.ByID.Count);
                foreach (KeyValuePair<int, TileEntity> byID in TileEntity.ByID)
                {
                    TileEntity.Write(writer, byID.Value);
                }
                TSPlayer.All.SendInfoMessage("{0} Tile Entitity Data Saved...", Terraria.DataStructures.TileEntity.ByID.Count);
                #endregion
            }
        }

        public static void Write(this BinaryWriter writer, ITile tile)
        {
            writer.Write(tile.sTileHeader);
            writer.Write(tile.bTileHeader);
            writer.Write(tile.bTileHeader2);

            if (tile.active())
            {
                writer.Write(tile.type);
                if (Main.tileFrameImportant[tile.type])
                {
                    writer.Write(tile.frameX);
                    writer.Write(tile.frameY);
                }
            }
            writer.Write(tile.wall);
            writer.Write(tile.liquid);
        }

        public static void WriteChest(this BinaryWriter writer, Chest chest)
        {
            writer.Write(chest.x);
            writer.Write(chest.y);
            //writer.Write(chest.name);
            for (int l = 0; l < 40; l++)
            {
                Item item = chest.item[l];
                if (item != null && item.stack > 0)
                {
                    writer.Write((short)item.stack);
                    writer.Write(item.netID);
                    writer.Write(item.prefix);
                }
                else
                {
                    writer.Write((short)0);
                }
            }
        }

        public static void WriteSign(this BinaryWriter writer, Sign sign)
        {
            writer.Write(sign.text);
            writer.Write(sign.x);
            writer.Write(sign.y);
        }
    }
}
