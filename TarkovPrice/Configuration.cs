using System.Collections;
using System.Collections.Generic;
using TarkovPrice.XmlModels;

namespace TarkovPrice
{
    public static class Configuration
    {
        public static IDictionary<string, string> ConfigurationDictionary = new Dictionary<string, string>();
        public static IDictionary<string, string> DefaultConfigurationDictionary = new Dictionary<string, string>
        {
            { "PickedMonitor", "" },
            { "ApiKey", "" },
            { "CutPointX", "650" },
            { "CutPointY", "70" },
            { "CutWidth", "1000" },
            { "CutHeight", "850" }
        };

        public static string PickedMonitor = "PickedMonitor";
        public static string ApiKey = "ApiKey";

        public static string CutPointX = "CutPointX";
        public static string CutPointY = "CutPointY";
        public static string CutWidth = "CutWidth";
        public static string CutHeight = "CutHeight";

        public static bool GetBoolValue(string key)
        {
            return ConfigurationDictionary.ContainsKey(key) && bool.Parse(ConfigurationDictionary[key]);
        }

        public static string GetStringValue(string key)
        {
            //if (ConfigurationDictionary.ContainsKey(key))
                return ConfigurationDictionary[key];
        }
        public static int GetIntValue(string key)
        {
            return int.Parse(ConfigurationDictionary[key]);
        }

        public static void SetCutPointX(int value)
        {
            //CutPointX = value;
            ConfigurationDictionary.AddOrUpdate("CutPointX", value.ToString());
        }

        public static void SetCutPointY(int value)
        {
            //CutPointY = value;
            ConfigurationDictionary.AddOrUpdate("CutPointY", value.ToString());
        }

        public static void SetCutWidth(int value)
        {
            //CutWidth = value;
            ConfigurationDictionary.AddOrUpdate("CutWidth", value.ToString());
        }

        public static void SetCutHeight(int value)
        {
            //CutHeight = value;
            ConfigurationDictionary.AddOrUpdate("CutHeight", value.ToString());
        }

        public static void SetApiKey(string value)
        {
            //ApiKey = value;
            ConfigurationDictionary.AddOrUpdate("ApiKey", value);
        }

        public static void SetPickedMonitor(string value)
        {
            //PickedMonitor = value;
            ConfigurationDictionary.AddOrUpdate("PickedMonitor", value);
        }

        public static void LoadConfiguration(Configurations configurations)
        {
            foreach (var configuration in configurations.ConfigurationCollection)
            {
                ConfigurationDictionary.Add(configuration.Key, configuration.Value);
            }
        }
    }
}