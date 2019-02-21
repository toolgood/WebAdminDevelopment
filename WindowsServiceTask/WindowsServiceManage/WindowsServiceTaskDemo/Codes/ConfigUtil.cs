using System;
using System.Collections.Concurrent;
using System.IO;
using System.Xml;

namespace ToolGood.Common
{

    public static class ConfigUtil
    {
        /// <summary>
        /// 固定路径
        /// </summary>
        private static readonly string[] _appConfigFiles = new string[] {
            "app.config",
            "App_Data/app.config",
            "App_Data/Config/app.config",
            "App_Data/Configs/app.config",
            "Config/app.config",
            "Configs/app.config",

            "appSettings.config",
            "App_Data/appSettings.config",
            "App_Data/Config/appSettings.config",
            "App_Data/Configs/appSettings.config",
            "Config/appSettings.config",
            "Configs/appSettings.config",

            "appSetting.config",
            "App_Data/appSetting.config",
            "App_Data/Config/appSetting.config",
            "App_Data/Configs/appSetting.config",
            "Config/appSetting.config",
            "Configs/appSetting.config",
        };
        /// <summary>
        /// 环境可变路径
        /// </summary>
        private static readonly string[] _appConfigFiles2 = new string[] {
            "app.{0}.config",
            "App_Data/app.{0}.config",
            "App_Data/Config/app.{0}.config",
            "App_Data/Configs/app.{0}.config",
            "Config/app.{0}.config",
            "Configs/app.{0}.config",

            "app_{0}.config",
            "App_Data/app_{0}.config",
            "App_Data/Config/app_{0}.config",
            "App_Data/Configs/app_{0}.config",
            "Config/app_{0}.config",
            "Configs/app_{0}.config",

            "appSettings.{0}.config",
            "App_Data/appSettings.{0}.config",
            "App_Data/Config/appSettings.{0}.config",
            "App_Data/Configs/appSettings.{0}.config",
            "Config/appSettings.{0}.config",
            "Configs/appSettings.{0}.config",

            "appSettings_{0}.config",
            "App_Data/appSettings_{0}.config",
            "App_Data/Config/appSettings_{0}.config",
            "App_Data/Configs/appSettings_{0}.config",
            "Config/appSettings_{0}.config",
            "Configs/appSettings_{0}.config",

            "appSetting.{0}.config",
            "App_Data/appSetting.{0}.config",
            "App_Data/Config/appSetting.{0}.config",
            "App_Data/Configs/appSetting.{0}.config",
            "Config/appSetting.{0}.config",
            "Configs/appSetting.{0}.config",

            "appSetting_{0}.config",
            "App_Data/appSetting_{0}.config",
            "App_Data/Config/appSetting_{0}.config",
            "App_Data/Configs/appSetting_{0}.config",
            "Config/appSetting_{0}.config",
            "Configs/appSetting_{0}.config",
        };

        private static string _appConfig = null;
        private static string _appEnvironment = null;

        /// <summary>
        /// 环境字符串：dev(开发)，test(测试)，prod(正式)
        /// </summary>
        public static string AppEnvironment {
            get { return _appEnvironment; }
            set {
                _appEnvironment = value;
                _appConfig = null;
            }
        }


        private static FileWatchUtil _configFileWacth = null;
        private static ConcurrentDictionary<string, string> _configCache = new ConcurrentDictionary<string, string>();//配置文件的缓存


        /// <summary>
        /// 获取配置文件的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            string value = null;
            if (_configFileWacth != null) {
                if (_configCache.TryGetValue(key, out value)) {
                    return value;
                }
            } else {
                XmlDocument xmlDoc = new XmlDocument();
                var file = GetAppConfigPath();
                if (string.IsNullOrWhiteSpace(file) == false || File.Exists(file)) {
                    xmlDoc.Load(file);
                }
                var xmlNode = xmlDoc.SelectSingleNode("//appSettings");
                if (xmlNode != null) {
                    XmlElement xmlElement = xmlNode.SelectSingleNode("//add[@key='" + key + "']") as XmlElement;
                    if (xmlElement != null) {
                        return xmlElement.GetAttribute("value");
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// 启动监听
        /// </summary>
        public static void StartWatch()
        {
            _configFileWacth = new FileWatchUtil(GetAppConfigPath(), InternalConfigure, isStart: true);
        }

        private static void InternalConfigure(FileStream configStream)
        {
            if (configStream == null) { LogUtil.Error("【configStream】为空"); }
            XmlDocument doc = new XmlDocument();
            try {
                doc.Load(configStream);
            } catch (Exception ex) {
                LogUtil.Error(ex.Message);
                return;
            }

            XmlNode xNode = doc.SelectSingleNode("//appSettings");
            if (xNode != null && xNode.HasChildNodes) {
                var configCache = new ConcurrentDictionary<string, string>();
                string key = string.Empty;
                string value = string.Empty;
                for (int i = 0; i < xNode.ChildNodes.Count; i++) {
                    XmlElement childrenElement = xNode.ChildNodes[i] as XmlElement;
                    if (childrenElement != null) {
                        key = childrenElement.GetAttribute("key");
                        value = childrenElement.GetAttribute("value");
                        if (string.IsNullOrWhiteSpace(key) == false) {
                            configCache[key] = value;
                        }
                    }
                }
                _configCache = configCache;
            }
        }

        private static string GetAppConfigPath()
        {
            if (string.IsNullOrEmpty(_appConfig) == false) { return _appConfig; }

            foreach (var file in _appConfigFiles) {
                var f = Path.GetFullPath(file);
                if (File.Exists(f)) { _appConfig = f; return _appConfig; }
            }

            if (string.IsNullOrEmpty(AppEnvironment)) {
                foreach (var file in _appConfigFiles2) {
                    var f = Path.GetFullPath(string.Format(file, AppEnvironment));
                    if (File.Exists(f)) { _appConfig = f; return _appConfig; }
                }
            }
            string[] appEnvironments;
            if (System.Diagnostics.Debugger.IsAttached) { //调试时 先采用 开发文件
                appEnvironments = new string[] { "dev", "test", "prod" };
            } else {
                appEnvironments = new string[] { "prod", "test", "dev" };
            }
            foreach (var item in appEnvironments) {
                foreach (var file in _appConfigFiles2) {
                    var f = Path.GetFullPath(string.Format(file, item));
                    if (File.Exists(f)) { _appConfig = f; return _appConfig; }
                }
            }
            _appConfig = "web.config";
            return _appConfig;
        }

    }


}
