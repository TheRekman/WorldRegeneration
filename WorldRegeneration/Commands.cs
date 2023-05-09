using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using TShockAPI;
using Microsoft.Xna.Framework;

namespace WorldRegeneration
{
    public static class Commands
    {
        public static string GetWorldPath(CommandArgs args, string properSyntax)
        {
            if(args.Parameters.Count > 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}".SFormat(properSyntax));
                return null;
            }
            var worldName = string.Empty;
            if (args.Parameters.Count == 1)
            {
                worldName = args.Parameters[0];
                return WorldRegeneration.FilesManager.GenerateWorldPath(worldName, Main.worldID.ToString());
            }
            if (WorldRegeneration.Config.UseSpecificFileName)
                return WorldRegeneration.FilesManager.GenerateSpecificWorldPath();
            worldName = Main.worldName;
            args.Player.SendInfoMessage("World name was not defined. Current world name was taken.");
            return WorldRegeneration.FilesManager.GenerateWorldPath(worldName, Main.worldID.ToString());
        }

        public static void SaveWorld(CommandArgs args)
        {
            string schematicPath = GetWorldPath(args, "/saveworld [name]");
            Utilities.SaveWorldSection(0, 0, Main.maxTilesX, Main.maxTilesY, schematicPath);
        }

        public static void LoadWorld(CommandArgs args)
        {
            string schematicPath = GetWorldPath(args, "/loadworld [name]");
            if (!File.Exists(schematicPath))
            {
                args.Player.SendErrorMessage("Invalid world file '{0}'!", Path.GetFileName(schematicPath));
                return;
            }
            Utilities.LoadWorldSection(schematicPath, Rectangle.Empty, false);
            WorldRegeneration.lastWorldID = WorldRegeneration.FilesManager.GetWorldFileInfo(schematicPath).Id;
        }

        public static void WorldRegen(CommandArgs args)
        {
            string cmd = "help";
            if (args.Parameters.Count > 0)
            {
                cmd = args.Parameters[0].ToLower();
            }
            switch (cmd)
            {
                case "time":
                    TimeSpan NextRegen = WorldRegeneration.WorldRegenCheck - DateTime.UtcNow.AddSeconds(-WorldRegeneration.Config.RegenerationInterval);
                    args.Player.SendInfoMessage("World Regeneration will be in{0}{1}{2}.",
                        NextRegen.Hours > 0 ? NextRegen.Hours == 1 ? " " + NextRegen.Hours + " Hour" 
                                                                   : " " + NextRegen.Hours + " Hours" 
                                            : "",
                        NextRegen.Minutes > 0 ? NextRegen.Minutes == 1 ? " " + NextRegen.Minutes + " Minute" 
                                                                       : " " + NextRegen.Minutes + " Minutes" 
                                              : "",
                        NextRegen.Seconds > 0 ? NextRegen.Seconds == 1 ? " " + NextRegen.Seconds + " Second" 
                                                                       : " " + NextRegen.Seconds + " Seconds"
                                              : "");
                    break;
                case "force":
                    int time = 0;
                    if (!(args.Parameters.Count == 2) || !int.TryParse(args.Parameters[1], out time))
                    {
                        time = 300;
                        args.Player.SendInfoMessage("Time automatically setted to 5 minutes.");
                    }
                    args.Player.SendInfoMessage("You forced World Regeneration.");
                    WorldRegeneration.WorldRegenCheck = DateTime.UtcNow.AddSeconds(-WorldRegeneration.Config.RegenerationInterval + time + 1);
                    break;
                case "list":
                    if (args.Parameters.Count > 2)
                    {
                        args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /worldregen list [page]");
                        return;
                    }

                    int page;
                    if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out page))
                        return;

                    var schematics = WorldRegeneration.FilesManager.GetFiles()
                                                                   .Select(p => WorldRegeneration.FilesManager.GetWorldFileInfo(p))
                                                                   .Where(w => w.Id == Main.worldID.ToString())
                                                                   .Select(w => w.Name);
                    PaginationTools.SendPage(args.Player, page, PaginationTools.BuildLinesFromTerms(schematics),
                        new PaginationTools.Settings
                        {
                            HeaderFormat = "Worlds ({0}/{1}):",
                            FooterFormat = "Type /worldregen list {0} for more."
                        });
                    args.Player.SendInfoMessage("Last World Regenerated: {0}.", WorldRegeneration.lastWorldID);
                    break;
                default:
                    {
                        int pageNumber;
                        int pageParamIndex = 0;
                        if (args.Parameters.Count > 1)
                            pageParamIndex = 1;

                        if (cmd != "help")
                        {
                            if (!PaginationTools.TryParsePageNumber(args.Parameters, pageParamIndex, args.Player, out pageNumber))
                            {
                                args.Player.SendErrorMessage("Proper syntax: /search <option> <name>");
                                return;
                            }
                        }
                        else pageNumber = 1;

                        List<string> lines = new List<string> {
                        "time - Information on next world regeneration.",
                        "force [seconds] - Force the world regeneration to 5 minutes, or setted time.",
                        "list - List available world IDs.",
                        };
                        PaginationTools.SendPage(
                            args.Player, pageNumber, lines,
                            new PaginationTools.Settings
                            {
                                HeaderFormat = "Available [c/ffffff:World Regen] Sub-Commands ({0}/{1}):",
                                FooterFormat = "Type {0}worldregen {{0}} for more sub-commands.".SFormat(TShockAPI.Commands.Specifier)
                            }
                        );
                        return;
                    }
            }
        }
    }
}
