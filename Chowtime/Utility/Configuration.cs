using System.Configuration;

namespace SGApp.Utility
{
    public static class Configuration
    {
        /// <summary>
        /// Gets the app settings value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetAppSettingsValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Base url of Web API
        /// </summary>
        public static string ServiceUrl
        {
            get
            {
                string url = GetAppSettingsValue("ServiceUrl");
                if (string.IsNullOrEmpty(url))
                {
                    url = "http://localhost:53280/";
                }
                return url;
            }
            
            
            
        
            
        }
    }
}
