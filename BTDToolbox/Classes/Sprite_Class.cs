﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using Sprite_Class;
//
//    var spriteReader = SpriteReader.FromJson(jsonString);

namespace Sprite_Class
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class SpriteReader
    {
        [JsonProperty("actors")]
        public Actor[] Actors { get; set; }

        [JsonProperty("customvariables")]
        public Customvariable[] Customvariables { get; set; }

        [JsonProperty("events")]
        public object Events { get; set; }

        [JsonProperty("stageOptions")]
        public StageOptions StageOptions { get; set; }

        [JsonProperty("timelines")]
        public Timeline[] Timelines { get; set; }
    }

    public partial class Actor
    {
        [JsonProperty("Alignment")]
        public long[] Alignment { get; set; }

        [JsonProperty("Alpha")]
        public long Alpha { get; set; }

        [JsonProperty("Angle")]
        public long Angle { get; set; }

        [JsonProperty("Flip")]
        public long Flip { get; set; }

        [JsonProperty("Position")]
        public long[] Position { get; set; }

        [JsonProperty("Scale")]
        public long[] Scale { get; set; }

        [JsonProperty("Shown")]
        public bool Shown { get; set; }

        [JsonProperty("sprite")]
        public string Sprite { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }

        [JsonProperty("uid")]
        public long Uid { get; set; }
    }

    public partial class Customvariable
    {
        [JsonProperty("VariableName")]
        public string VariableName { get; set; }

        [JsonProperty("ValueType")]
        public long ValueType { get; set; }

        [JsonProperty("Value")]
        public long[] Value { get; set; }
    }

    public partial class StageOptions
    {
        [JsonProperty("SpriteInfo")]
        public SpriteInfo[] SpriteInfo { get; set; }

        [JsonProperty("StageLength")]
        public double StageLength { get; set; }
    }

    public partial class SpriteInfo
    {
        [JsonProperty("SpriteInfo")]
        public string SpriteInfoSpriteInfo { get; set; }

        [JsonProperty("Texture")]
        public string Texture { get; set; }
    }

    public partial class Timeline
    {
        [JsonProperty("spriteuid")]
        public long Spriteuid { get; set; }

        [JsonProperty("stage")]
        public Stage[] Stage { get; set; }
    }

    public partial class Stage
    {
        [JsonProperty("Alignment")]
        public long[] Alignment { get; set; }

        [JsonProperty("Alpha")]
        public long Alpha { get; set; }

        [JsonProperty("Angle")]
        public long Angle { get; set; }

        [JsonProperty("Flip")]
        public long Flip { get; set; }

        [JsonProperty("Position")]
        public long[] Position { get; set; }

        [JsonProperty("Scale")]
        public long[] Scale { get; set; }

        [JsonProperty("Shown")]
        public bool Shown { get; set; }

        [JsonProperty("Time")]
        public double Time { get; set; }
    }

    public partial class SpriteReader
    {
        public static SpriteReader FromJson(string json) => JsonConvert.DeserializeObject<SpriteReader>(json, Sprite_Class.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this SpriteReader self) => JsonConvert.SerializeObject(self, Sprite_Class.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
