﻿using System;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;

namespace WorldRegeneration
{
    public class Config
    {
        public bool EnableAutoRegen = false;
        public int MaxZRegion = 0;
        public int RegenerationInterval = 21600;
        public bool IgnoreChests = false;
        public bool ResetWorldGenStatus = false;
        public bool UseVanillaWorldFiles = true;
        public string TargetWorldNameFormat = "{0}-{1}-WR.wld";
        public bool UseSpecificFileName = false;
        public string specificName = "WordRegeneration.wld";

        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Config Read(string path)
        {
            return File.Exists(path) ? JsonConvert.DeserializeObject<Config>(File.ReadAllText(path)) : new Config();
        }
    }
}