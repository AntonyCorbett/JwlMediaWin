namespace JwlMediaWin.Services
{
    using System;
    using System.IO;
    using JwlMediaWin.Models;
    using Newtonsoft.Json;
    using Serilog;

    internal class OptionsService : IOptionsService
    {
        private readonly int _optionsVersion = 1;
        private Options _options;
        private string _optionsFilePath;

        public Options Options
        {
            get
            {
                Init();
                return _options;
            }
        }

        public void Save()
        {
            try
            {
                WriteOptions();
                Log.Logger.Information("Settings saved");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not save settings");
            }
        }

        private void Init()
        {
            if (_options == null)
            {
                try
                {
                    _optionsFilePath = FileUtils.GetUserOptionsFilePath(_optionsVersion);
                    ReadOptions();
                    
                    if (_options == null)
                    {
                        _options = new Options();
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Could not read options file");
                    _options = new Options();
                }
            }
        }

        private void ReadOptions()
        {
            if (!File.Exists(_optionsFilePath))
            {
                WriteDefaultOptions();
            }
            else
            {
                using (StreamReader file = File.OpenText(_optionsFilePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    _options = (Options)serializer.Deserialize(file, typeof(Options));
                }
            }
        }

        private void WriteDefaultOptions()
        {
            _options = new Options();
            WriteOptions();
        }

        private void WriteOptions()
        {
            if (_options != null)
            {
                using (StreamWriter file = File.CreateText(_optionsFilePath))
                {
                    var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                    serializer.Serialize(file, _options);
                }
            }
        }
    }
}
