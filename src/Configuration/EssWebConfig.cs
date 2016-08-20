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

using System;
using System.IO;
using Essentials.Api;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Essentials.Configuration {

    internal class EssWebConfig : EssConfig {

        public override void Load(string filePath) {
            var logger = UEssentials.Logger;
            var curWebConf = UEssentials.Config.WebConfig;

            try {
                logger.LogInfo($"Loading web configuration from '{curWebConf.Url}'");

                using (var wc = new WebClient()) {
                    var resp = JObject.Parse(wc.DownloadString(curWebConf.Url));
                    resp["WebConfig"]["Url"] = curWebConf.Url;
                    resp["WebConfig"]["Enabled"] = curWebConf.Enabled;
                    File.WriteAllText(filePath, resp.ToString());
                }
            } catch (Exception ex) {
                logger.LogError("Could not load webconfig.");
                logger.LogError(ex.ToString());
            }

            base.Load(filePath);
        }

    }

}