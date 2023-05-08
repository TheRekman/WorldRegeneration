using System.ComponentModel;

namespace WorldRegeneration
{
    public static class Permissions
    {
        [Description("Save a terraria world data file.")]
        public static readonly string saveworld = "worldregen.main.save";

        [Description("Load a terraria world data file.")]
        public static readonly string loadworld = "worldregen.main.load";

        [Description("Allows various sub-commands.")]
        public static readonly string worldregen = "worldregen.main.info";
    }
}
