using System;
using VAkos;

namespace XMLConfig
{
    public class Config
    {
        private Xmlconfig config;

        public Config(string configFile, bool createIfNotExists)
        {
            config = new Xmlconfig(configFile, createIfNotExists);
        }

        public T GetValue<T>(string path, T defaultValue, bool writeDefaultIfEmpty = true)
        {
            string val = config.Settings[path].Value;
            if (val == "")
            {
                if (writeDefaultIfEmpty)
                {
                    config.Settings[path].Value = defaultValue.ToString();
                    config.Commit();
                }
                return defaultValue;
            }
            return (T)Convert.ChangeType(val, typeof(T));
        }

        public bool SetValue<T>(string path, T value)
        {
            try
            {
                config.Settings[path].Value = value.ToString();
                config.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
