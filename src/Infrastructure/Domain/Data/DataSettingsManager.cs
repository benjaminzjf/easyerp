﻿namespace Infrastructure.Domain.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class DataSettingsManager
    {
        protected const char Separator = ':';

        protected const string Filename = "Settings.txt";

        /// <summary>
        /// Maps a virtual path to a physical disk path.
        /// </summary>
        /// <param name="path">The path to map. E.g. "~/bin"</param>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        protected virtual string MapPath(string path)
        {
            //if (HostingEnvironment.IsHosted)
            //{
            //    //hosted
            //    return HostingEnvironment.MapPath(path);
            //}

            //not hosted. For example, run in unit tests
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
            return Path.Combine(baseDirectory, path);
        }

        protected virtual DataSettings ParseSettings(string text)
        {
            var shellSettings = new DataSettings();
            if (String.IsNullOrEmpty(text))
            {
                return shellSettings;
            }

            var settings = new List<string>();
            using (var reader = new StringReader(text))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    settings.Add(str);
                }
            }

            foreach (var setting in settings)
            {
                var separatorIndex = setting.IndexOf(Separator);
                if (separatorIndex == -1)
                {
                    continue;
                }
                var key = setting.Substring(0, separatorIndex).Trim();
                var value = setting.Substring(separatorIndex + 1).Trim();

                switch (key)
                {
                    case "DataProvider":
                        shellSettings.DataProvider = value;
                        break;

                    case "DataConnectionString":
                        shellSettings.DataConnectionString = value;
                        break;

                    default:
                        shellSettings.RawDataSettings.Add(key, value);
                        break;
                }
            }

            return shellSettings;
        }

        protected virtual string ComposeSettings(DataSettings settings)
        {
            if (settings == null)
            {
                return "";
            }

            return string.Format(
                "DataProvider: {0}{2}DataConnectionString: {1}{2}",
                settings.DataProvider,
                settings.DataConnectionString,
                Environment.NewLine
                );
        }

        /// <summary>
        /// Load settings
        /// </summary>
        /// <param name="filePath">File path; pass null to use default settings file path</param>
        /// <returns></returns>
        public virtual DataSettings LoadSettings(string filePath = null)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                //use webHelper.MapPath instead of HostingEnvironment.MapPath which is not available in unit tests
                filePath = Path.Combine(this.MapPath("~/App_Data/"), Filename);
            }
            if (!File.Exists(filePath))
            {
                return new DataSettings();
            }
            var text = File.ReadAllText(filePath);
            return this.ParseSettings(text);
        }

        public virtual void SaveSettings(DataSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            //use webHelper.MapPath instead of HostingEnvironment.MapPath which is not available in unit tests
            var filePath = Path.Combine(this.MapPath("~/App_Data/"), Filename);
            if (!File.Exists(filePath))
            {
                using (File.Create(filePath))
                {
                    //we use 'using' to close the file after it's created
                }
            }

            var text = this.ComposeSettings(settings);
            File.WriteAllText(filePath, text);
        }
    }
}