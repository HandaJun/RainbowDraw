using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace RainbowDraw.LOGIC
{
    [Serializable]
    public class ApplicationSetting
    {
        public static readonly string Location = "RainbowDrawSetting.yaml";

        public string UpdateUrl { get; set; } = "https://samek.tistory.com/8";
        public string HelpUrl { get; set; } = "https://samek.tistory.com/12";
        public int Size { get; set; } = 5;
        public int ColorChangeTime { get; set; } = 250;
        public double Magify { get; set; } = 1.5;
        public int RemainMs { get; set; } = 3000;
        public bool RemoveFlg { get; set; } = false;
        public List<string> ColorList { get; set; } = new List<string>();
        public string LastColor { get; set; } = "Rainbow";

        public static ApplicationSetting Load()
        {
            //Directory.CreateDirectory(Path.GetDirectoryName(Location));
            if (!File.Exists(Location))
                using (var fs = File.Create(Location)) { }

            ApplicationSetting setting = Deserialize(Location);
            if (setting == null)
            {
                setting = new ApplicationSetting();
                setting.Save();
            }

            return setting;
        }

        public void Save()
        {
            Serialize(this, Location);
        }

        public bool Ensure(bool isChanged = false)
        {
            //if (Level == 0)
            //{
            //    Level = 1;
            //    isChanged |= true;
            //}

            return isChanged;
        }

        private static void Serialize(ApplicationSetting setting, string path)
        {
            var serializer = new SerializerBuilder().Build();
            var yml = serializer.Serialize(setting);
            using (var sr = new StreamWriter(path))
            {
                sr.Write(yml);
            }
        }

        private static ApplicationSetting Deserialize(string path)
        {
            using (var sr = new StreamReader(path))
            {
                using (var input = new StringReader(sr.ReadToEnd()))
                {
                    var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                    return deserializer.Deserialize<ApplicationSetting>(input);
                }
            }
        }
    }
}
