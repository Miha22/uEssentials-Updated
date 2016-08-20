/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  leonardosnt
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Essentials.Api;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.Common.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Essentials.I18n {

    public static class EssLang {

        private const string KEY_NOT_FOUND_MESSAGE = "Lang: Key not found '{0}', report to an adminstrator.";
        private static readonly string[] LANGS = { "en", "pt-br", "es", "ru" };
        private static readonly Dictionary<string, object> _translations = new Dictionary<string, object>();

        public static void LoadDefault(string locale) {
            LoadDefault(locale, Path.Combine(UEssentials.TranslationFolder, $"lang_{locale}.json"));
        }

        public static void LoadDefault(string locale, string destPath) {
            if (File.Exists(destPath))
                File.WriteAllText(destPath, string.Empty);
            else
                File.Create(destPath).Close();

            var sw = new StreamWriter(destPath);
            var defaultLangStream = GetDefaultStream(locale);

            using (var sr = new StreamReader(defaultLangStream, Encoding.UTF8, true)) {
                for (string line; (line = sr.ReadLine()) != null;) {
                    sw.WriteLine(line);
                }
            }

            sw.Close();
        }

        public static void Load() {
            // Load defaults
            LANGS.ForEach(l => {
                var lpath = $"{UEssentials.TranslationFolder}lang_{l}.json";
                if (!File.Exists(lpath)) LoadDefault(l);
            });

            var locale = UEssentials.Config.Locale.ToLowerInvariant();
            var translationPath = $"{UEssentials.TranslationFolder}lang_{locale}.json";

            if (!File.Exists(translationPath)) {
                if (LANGS.Contains(locale)) {
                    LoadDefault(locale);
                } else {
                    UEssentials.Logger.LogError($"Invalid locale '{locale}', " +
                                                $"File not found '{translationPath}'");
                    UEssentials.Logger.LogError("Switching to default locale (en)...");
                    locale = "en";
                    translationPath = $"{UEssentials.TranslationFolder}lang_{locale}.json";
                }
            }

            JObject json;

            try {
                json = JObject.Parse(File.ReadAllText(translationPath));

                /*
                    Update translation
                */
                var defaultJson = JObject.Load(new JsonTextReader(new StreamReader(
                    GetDefaultStream(locale), Encoding.UTF8, true)));

                if (defaultJson.Count != json.Count) {
                    foreach (var key in  defaultJson) {
                        JToken outVal;
                        if (json.TryGetValue(key.Key, out outVal)) {
                            defaultJson[key.Key] = outVal;
                        }
                    }

                    File.WriteAllText(translationPath, string.Empty);
                    JsonUtil.Serialize(translationPath, defaultJson);
                    json = defaultJson;
                }
            } catch (JsonReaderException ex) {
                UEssentials.Logger.LogError($"Invalid translation ({translationPath})");
                UEssentials.Logger.LogError(ex.Message);

                // Load default
                json = JObject.Load(new JsonTextReader(new StreamReader(
                    GetDefaultStream(locale), Encoding.UTF8, true)));
            }

            _translations.Clear();
            foreach (var entry in json) {
                _translations.Add(entry.Key, entry.Value.Value<string>());
            }
        }

        public static string Translate(string key, params object[] args) {
            var raw = GetEntry(key) as string;
            return raw == null ? null : string.Format(raw, args);
        }

        public static bool HasEntry(string key) {
            return _translations.ContainsKey(key);
        }

        public static object GetEntry(string key) {
            object val;
            return _translations.TryGetValue(key, out val) ? val : null;
        }

        public static void Send(ICommandSource target, string key, params object[] args) {
            var message = Translate(key, args);
            Color color;

            if (message == null) {
                color = Color.red;
                message = string.Format(KEY_NOT_FOUND_MESSAGE, key);
            } else {
                color = ColorUtil.GetColorFromString(ref message);
            }

            target.SendMessage(message, color);
        }

        public static void Broadcast(string key, params object[] args) {
            var message = Translate(key, args);
            Color color;

            if (message == null) {
                color = Color.red;
                message = string.Format(KEY_NOT_FOUND_MESSAGE, key);
            } else {
                color = ColorUtil.GetColorFromString(ref message);
            }

            UServer.Broadcast(message, color);
        }

        private static Stream GetDefaultStream(string locale) {
            var path = $"Essentials.default.lang_{locale}.json";
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        }

    }

}