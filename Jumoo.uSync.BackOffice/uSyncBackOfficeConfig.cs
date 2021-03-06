﻿
namespace Jumoo.uSync.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;

    using Umbraco.Core.IO;
    using Umbraco.Core.Logging;

    public class uSyncBackOfficeConfig
    {
        private uSyncBackOfficeSettings _settings;
        public uSyncBackOfficeSettings Settings
        {
            get { return _settings; }
        }

        public uSyncBackOfficeConfig()
        {
            Init();
        }

        private void Init()
        {
            if (_settings != null) return;

            try
            {
                var configFile = IOHelper.MapPath(
                    Path.Combine(SystemDirectories.Config, "uSyncBackOffice.Config"));

                if (System.IO.File.Exists(configFile))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(uSyncBackOfficeSettings));
                    using (FileStream fs = new FileStream(configFile, FileMode.Open))
                    {
                        _settings = (uSyncBackOfficeSettings)serializer.Deserialize(fs);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn<uSyncBackOfficeConfig>("Unable to load the settings: {0}", () => ex);
            }

            if (_settings == null)
            {
                // default settings...
                _settings = new uSyncBackOfficeSettings
                {
                    Import = true,
                    ExportAtStartup = false,
                    ExportOnSave = true,
                    WatchForFileChanges = false,
                    ArchiveVersions = false,

                    Folder = "~/uSync/data/",
                    ArchiveFolder = "~/uSync/Archive/",
                    BackupFolder = "~/uSync/Backup/",

                    Handlers = new List<HandlerConfig>()
                };

                foreach(var handler in uSyncBackOfficeContext.Instance.Handlers)
                {
                    _settings.Handlers.Add(new HandlerConfig
                    {
                        Name = handler.Name,
                        Enabled = true
                    });
                }

                // save the defaults to disk...
                SaveSettings(_settings);
            }
        }

        public void SaveSettings(uSyncBackOfficeSettings settings)
        {
            var configFile = IOHelper.MapPath(
                Path.Combine(SystemDirectories.Config, "uSyncBackOffice.Config"));

            if (File.Exists(configFile))
                File.Delete(configFile);

            XmlSerializer serializer = new XmlSerializer(typeof(uSyncBackOfficeSettings));

            using (StreamWriter w = new StreamWriter(configFile))
            {
                serializer.Serialize(w, settings);
            }
        }
    }

    public class HandlerConfig
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "Enabled")]
        public bool Enabled { get; set; }
    }


    public class uSyncBackOfficeSettings
    {
        public string MappedFolder()
        {
            return IOHelper.MapPath(Folder);
        }

        public bool Import { get; set; }
        public bool ExportAtStartup { get; set; }
        public bool ExportOnSave { get; set; }
        public bool WatchForFileChanges { get; set; }

        public bool ArchiveVersions { get; set; }

        public string Folder { get; set; }
        public string ArchiveFolder { get; set; }
        public string BackupFolder { get; set; }
        public int MaxArchiveVersionCount { get; set; }

        public List<HandlerConfig> Handlers {get;set;}
    }
}