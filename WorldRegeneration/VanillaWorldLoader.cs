using Rests;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using Terraria.Utilities;
using Terraria.IO;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using System.Net;
using Terraria.GameContent;
using Microsoft.Xna.Framework;

namespace WorldRegeneration
{
    public class VanillaWorldLoader
    {
        private WorldLoadSettings _settings;

        public VanillaWorldLoader(WorldLoadSettings settings = null)
        {
            _settings = settings == null ? new WorldLoadSettings() : settings;
        }

        public void LoadWorld(string path)
        {
            Task.Factory.StartNew(() =>
            {
                using (var reader = new BinaryReader(new MemoryStream(FileUtilities.ReadAllBytes(path, false))))
                {
                    reader.ReadInt32();
                    FileMetadata.Read(reader, FileType.World);
                    var positionCount = reader.ReadInt16();
                    var positions = new int[positionCount];
                    for(int i = 0; i < positionCount; i++)
                        positions[i] = reader.ReadInt32();
                    var importanceCount = reader.ReadInt16();
                    var importance = new bool[importanceCount];
                    byte b = 0; 
                    byte b2 = 128;
                    for (int i = 0; i < importanceCount; i++)
                    {
                        if (b2 == 128)
                        {
                            b = reader.ReadByte();
                            b2 = 1;
                        }
                        else
                        {
                            b2 = (byte)(b2 << 1);
                        }
                        if ((b & b2) == b2)
                        {
                            importance[i] = true;
                        }
                    }
                    var headerPosition = positions[0];
                    var tilePosition = positions[1];
                    var chestPosition = positions[2];
                    var signsPosition = positions[3];
                    var npcPosition = positions[4];
                    var tileEntityPosition = positions[5];
                    var weightetPlatesPosition = positions[6];
                    if(_settings.LoadTiles)
                    {
                        reader.BaseStream.Seek(tilePosition, SeekOrigin.Begin);
                        LoadTiles(reader, importance);
                    }
                    if (_settings.LoadChests)
                    {
                        reader.BaseStream.Seek(chestPosition, SeekOrigin.Begin);
                        LoadChests(reader);
                    }
                    if (_settings.LoadSigns)
                    {
                        reader.BaseStream.Seek(signsPosition, SeekOrigin.Begin);
                        LoadSigns(reader);
                    }
                    if (_settings.LoadEnities)
                    {
                        reader.BaseStream.Seek(tileEntityPosition, SeekOrigin.Begin);
                        LoadEntities(reader);
                    }
                    if (_settings.LoadWeightetPlates)
                    {
                        reader.BaseStream.Seek(weightetPlatesPosition, SeekOrigin.Begin);
                        LoadWeightetPlates(reader);
                    }
                    ResetSection(0, 0, Main.maxTilesX, Main.maxTilesY);
                    TSPlayer.All.SendInfoMessage("Successfully regenerated the world.");
                }
            });
        }

#warning All methods was copied from DnSpy. Must be rewrite in more readable way
        private void LoadWeightetPlates(BinaryReader reader)
        {
            PressurePlateHelper.Reset();
            PressurePlateHelper.NeedsFirstUpdate = true;
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                Point point = new Point(reader.ReadInt32(), reader.ReadInt32());
                if (TShock.Regions.InAreaRegion(point.X / 16, point.Y / 16).Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                    continue;
                PressurePlateHelper.PressurePlatesPressed.Add(point, new bool[255]);
            }
        }

        private void LoadEntities(BinaryReader reader)
        {
            TileEntity.ByID = TileEntity.ByID.Where(kv => TShock.Regions.InAreaRegion(kv.Value.Position.X, kv.Value.Position.Y)
                                                                        .Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                                             .ToDictionary(kv => kv.Key, kv => kv.Value);
            TileEntity.ByPosition = TileEntity.ByPosition.Where(kv => TShock.Regions.InAreaRegion(kv.Value.Position.X, kv.Value.Position.Y)
                                                                                    .Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                                                         .ToDictionary(kv => kv.Key, kv => kv.Value); ;
            int entityCount = reader.ReadInt32();
            int currentID = 0;
            for (int i = 0; i < entityCount; i++)
            {
                TileEntity tileEntity = TileEntity.Read(reader, false);
                if (TShock.Regions.InAreaRegion(tileEntity.Position.X, tileEntity.Position.Y)
                                  .Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                    continue;
                while (TileEntity.ByID.ContainsKey(currentID))
                    currentID++;
                tileEntity.ID = currentID++;
                TileEntity.ByID[tileEntity.ID] = tileEntity;
                TileEntity tileEntity2;
                if (TileEntity.ByPosition.TryGetValue(tileEntity.Position, out tileEntity2))
                {
                    TileEntity.ByID.Remove(tileEntity2.ID);
                }
                TileEntity.ByPosition[tileEntity.Position] = tileEntity;
            }
            TileEntity.TileEntitiesNextID = currentID;
            List<Point16> list = new List<Point16>();
            foreach (KeyValuePair<Point16, TileEntity> keyValuePair in TileEntity.ByPosition)
            {
                if (!WorldGen.InWorld((int)keyValuePair.Value.Position.X, (int)keyValuePair.Value.Position.Y, 1))
                {
                    list.Add(keyValuePair.Value.Position);
                }
                else if (!TileEntity.manager.CheckValidTile((int)keyValuePair.Value.type, (int)keyValuePair.Value.Position.X, (int)keyValuePair.Value.Position.Y))
                {
                    list.Add(keyValuePair.Value.Position);
                }
            }
            try
            {
                foreach (Point16 point in list)
                {
                    TileEntity tileEntity3 = TileEntity.ByPosition[point];
                    if (TileEntity.ByID.ContainsKey(tileEntity3.ID))
                    {
                        TileEntity.ByID.Remove(tileEntity3.ID);
                    }
                    if (TileEntity.ByPosition.ContainsKey(point))
                    {
                        TileEntity.ByPosition.Remove(point);
                    }
                }
            }
            catch
            {
            }
        }

        private void LoadSigns(BinaryReader reader)
        {
            short signCount = reader.ReadInt16();
            int i;
            int mainSignPos = 0;
            for (i = 0; i < (int)signCount; i++)
            {
                string text = reader.ReadString();
                int x = reader.ReadInt32();
                int y = reader.ReadInt32();
                ITile tile = Main.tile[x, y];
                Sign sign;
                if (tile.active() && Main.tileSign[(int)tile.type])
                {
                    sign = new Sign();
                    sign.text = text;
                    sign.x = x;
                    sign.y = y;
                }
                else
                {
                    sign = null;
                }
                if (sign != null && TShock.Regions.InAreaRegion(sign.x, sign.y).Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                    continue;
                while (Main.sign[mainSignPos] != null &&
                       TShock.Regions.InAreaRegion(Main.sign[mainSignPos].x, Main.sign[mainSignPos].y)
                                     .Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                    mainSignPos++;
                Main.sign[mainSignPos] = sign;
                mainSignPos++;
            }
            List<Point16> list = new List<Point16>();
            for (int j = 0; j < Sign.maxSigns; j++)
            {
                if (Main.sign[j] != null)
                {
                    Point16 point = new Point16(Main.sign[j].x, Main.sign[j].y);
                    if (list.Contains(point))
                    {
                        Main.sign[j] = null;
                    }
                    else
                    {
                        list.Add(point);
                    }
                }
            }
            while (mainSignPos < Sign.maxSigns)
            {
                while (Main.sign[mainSignPos] != null &&
                       TShock.Regions.InAreaRegion(Main.sign[mainSignPos].x, Main.sign[mainSignPos].y)
                                     .Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                    mainSignPos++;
                Main.sign[mainSignPos] = null;
                mainSignPos++;
            }
        }

        private void LoadTiles(BinaryReader reader, bool[] importance)
        {
            for(int x = 0; x < Main.maxTilesX; x++)
                for(int y = 0; y < Main.maxTilesY; y++)
                {
                    int num2 = -1;
                    byte b3;
                    byte b2;
                    byte b = (b2 = (b3 = 0));
                    ITile tile = new Tile();
                    byte b4 = reader.ReadByte();
                    bool flag = false;
                    if ((b4 & 1) == 1)
                    {
                        flag = true;
                        b2 = reader.ReadByte();
                    }
                    bool flag2 = false;
                    if (flag && (b2 & 1) == 1)
                    {
                        flag2 = true;
                        b = reader.ReadByte();
                    }
                    if (flag2 && (b & 1) == 1)
                    {
                        b3 = reader.ReadByte();
                    }
                    byte b5;
                    if ((b4 & 2) == 2)
                    {
                        tile.active(true);
                        if ((b4 & 32) == 32)
                        {
                            b5 = reader.ReadByte();
                            num2 = (int)reader.ReadByte();
                            num2 = (num2 << 8) | (int)b5;
                        }
                        else
                        {
                            num2 = (int)reader.ReadByte();
                        }
                        tile.type = (ushort)num2;
                        if (importance[num2])
                        {
                            tile.frameX = reader.ReadInt16();
                            tile.frameY = reader.ReadInt16();
                            if (tile.type == 144)
                            {
                                tile.frameY = 0;
                            }
                        }
                        else
                        {
                            tile.frameX = -1;
                            tile.frameY = -1;
                        }
                        if ((b & 8) == 8)
                        {
                            tile.color(reader.ReadByte());
                        }
                    }
                    if ((b4 & 4) == 4)
                    {
                        tile.wall = (ushort)reader.ReadByte();
                        if (tile.wall >= WallID.Count)
                        {
                            tile.wall = 0;
                        }
                        if ((b & 16) == 16)
                        {
                            tile.wallColor(reader.ReadByte());
                        }
                    }
                    b5 = (byte)((b4 & 24) >> 3);
                    if (b5 != 0)
                    {
                        tile.liquid = reader.ReadByte();
                        if ((b & 128) == 128)
                        {
                            tile.shimmer(true);
                        }
                        else if (b5 > 1)
                        {
                            if (b5 == 2)
                            {
                                tile.lava(true);
                            }
                            else
                            {
                                tile.honey(true);
                            }
                        }
                    }
                    if (b2 > 1)
                    {
                        if ((b2 & 2) == 2)
                        {
                            tile.wire(true);
                        }
                        if ((b2 & 4) == 4)
                        {
                            tile.wire2(true);
                        }
                        if ((b2 & 8) == 8)
                        {
                            tile.wire3(true);
                        }
                        b5 = (byte)((b2 & 112) >> 4);
                        if (b5 != 0 && (Main.tileSolid[(int)tile.type] || TileID.Sets.NonSolidSaveSlopes[(int)tile.type]))
                        {
                            if (b5 == 1)
                            {
                                tile.halfBrick(true);
                            }
                            else
                            {
                                tile.slope((byte)(b5 - 1));
                            }
                        }
                    }
                    if (b > 1)
                    {
                        if ((b & 2) == 2)
                        {
                            tile.actuator(true);
                        }
                        if ((b & 4) == 4)
                        {
                            tile.inActive(true);
                        }
                        if ((b & 32) == 32)
                        {
                            tile.wire4(true);
                        }
                        if ((b & 64) == 64)
                        {
                            b5 = reader.ReadByte();
                            tile.wall = (ushort)(((int)b5 << 8) | (int)tile.wall);
                            if (tile.wall >= WallID.Count)
                            {
                                tile.wall = 0;
                            }
                        }
                    }
                    if (b3 > 1)
                    {
                        if ((b3 & 2) == 2)
                        {
                            tile.invisibleBlock(true);
                        }
                        if ((b3 & 4) == 4)
                        {
                            tile.invisibleWall(true);
                        }
                        if ((b3 & 8) == 8)
                        {
                            tile.fullbrightBlock(true);
                        }
                        if ((b3 & 16) == 16)
                        {
                            tile.fullbrightWall(true);
                        }
                    }
                    b5 = (byte)((b4 & 192) >> 6);
                    int k;
                    if (b5 == 0)
                    {
                        k = 0;
                    }
                    else if (b5 == 1)
                    {
                        k = (int)reader.ReadByte();
                    }
                    else
                    {
                        k = (int)reader.ReadInt16();
                    }
                    if (num2 != -1)
                    {
                        if ((double)y <= Main.worldSurface)
                        {
                            if ((double)(y + k) <= Main.worldSurface)
                            {
                                WorldGen.tileCounts[num2] += (k + 1) * 5;
                            }
                            else
                            {
                                int num3 = (int)(Main.worldSurface - (double)y + 1.0);
                                int num4 = k + 1 - num3;
                                WorldGen.tileCounts[num2] += num3 * 5 + num4;
                            }
                        }
                        else
                        {
                            WorldGen.tileCounts[num2] += k + 1;
                        }
                    }
                    if (!TShock.Regions.InAreaRegion(x, y).Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                        Main.tile[x, y].CopyFrom(tile);
                    while (k > 0)
                    {
                        y++;
                        if (!TShock.Regions.InAreaRegion(x, y).Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                            Main.tile[x, y].CopyFrom(tile);
                        k--;
                    }
                }
        }

        private void LoadChests(BinaryReader reader)
        {
            if (WorldRegeneration.Config.IgnoreChests)
                return;
            int chestCount = (int)reader.ReadInt16();
            int worldChestCapacity = (int)reader.ReadInt16();
            int chestCapacity;
            int overCapacity;
            if (worldChestCapacity < Chest.maxItems)
            {
                chestCapacity = worldChestCapacity;
                overCapacity = 0;
            }
            else
            {
                chestCapacity = Chest.maxItems;
                overCapacity = worldChestCapacity - Chest.maxItems;
            }
            int i;
            int mainChestPos = 0;
            for (i = 0; i < chestCount; i++)
            {
                Chest chest = new Chest(false);
                chest.x = reader.ReadInt32();
                chest.y = reader.ReadInt32();
                chest.name = reader.ReadString();
                for (int j = 0; j < chestCapacity; j++)
                {
                    short stackSize = reader.ReadInt16();
                    Item item = new Item();
                    if (stackSize > 0)
                    {
                        item.netDefaults(reader.ReadInt32());
                        item.stack = (int)stackSize;
                        item.Prefix((int)reader.ReadByte());
                    }
                    else if (stackSize < 0)
                    {
                        item.netDefaults(reader.ReadInt32());
                        item.Prefix((int)reader.ReadByte());
                        item.stack = 1;
                    }
                    chest.item[j] = item;
                }
                for (int k = 0; k < overCapacity; k++)
                {
                    short stackSize = reader.ReadInt16();
                    if (stackSize > 0)
                    {
                        reader.ReadInt32();
                        reader.ReadByte();
                    }
                }
                if (TShock.Regions.InAreaRegion(chest.x, chest.y).Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                    continue;
                while (Main.chest[mainChestPos] != null &&
                       TShock.Regions.InAreaRegion(Main.chest[mainChestPos].x, Main.chest[mainChestPos].y)
                                     .Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                    mainChestPos++;
                Main.chest[mainChestPos] = chest;
                mainChestPos++;
            }
            List<Point16> list = new List<Point16>();
            for (int l = 0; l < mainChestPos; l++)
            {
                if (Main.chest[l] != null)
                {
                    Point16 point = new Point16(Main.chest[l].x, Main.chest[l].y);
                    if (list.Contains(point))
                    {
                        Main.chest[l] = null;
                    }
                    else
                    {
                        list.Add(point);
                    }
                }
            }
            while (mainChestPos < Main.chest.Length)
            {
                while (Main.chest[mainChestPos] != null &&
                      TShock.Regions.InAreaRegion(Main.chest[mainChestPos].x, Main.chest[mainChestPos].y)
                                    .Any(r => r != null && r.Z >= WorldRegeneration.Config.MaxZRegion))
                    mainChestPos++;
                    Main.chest[mainChestPos] = null;
                mainChestPos++; 
            }
            if (WorldFile._versionNumber < 115)
            {
                WorldFile.FixDresserChests();
            }
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
    }
}
