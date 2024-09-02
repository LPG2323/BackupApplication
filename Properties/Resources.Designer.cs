namespace BackupApp.Properties
{
    using System;
    using System.Reflection;
    using System.Resources;
    using System.Globalization;

    public class Resources
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        internal Resources() { }

        public static ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    resourceMan = new ResourceManager("BackupApp.Properties.Resources", typeof(Resources).Assembly);
                }
                return resourceMan;
            }
        }

        public static CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        public static string SourcePath
        {
            get { return ResourceManager.GetString("SourcePath", resourceCulture); }
        }

        public static string DestinationPath
        {
            get { return ResourceManager.GetString("DestinationPath", resourceCulture); }
        }

        public static string StartBackup
        {
            get { return ResourceManager.GetString("StartBackup", resourceCulture); }
        }

        public static string BackupTask
        {
            get { return ResourceManager.GetString("BackupTask", resourceCulture); }
        }
    }
}