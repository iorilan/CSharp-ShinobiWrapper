using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Sample.DataAccess.Tenant.DbModels;

namespace Sample.ShinobiVideo.Lib.Models
{
    public class RequestAddMonitor
    {
        public static string Create(CameraVm camera)
        {
            var baseUrl = ShinobiConfig.ShinobiBaseUrl;
            var apiKey = ShinobiConfig.ShinobiApiKey;
            var groupKey = ShinobiConfig.ShinobiGroupKey;
            var pathTemplate = ShinobiConfig.ShinobiTemplateRtsp;

            var url = $"{baseUrl}/{apiKey}/configureMonitor/{groupKey}";


			var cameraStreamUrl = camera.StreamUrl;

			var parts = cameraStreamUrl.Split('/');
            var protocol = parts[0];
            if (protocol.ToLower() == "http")
            {
                pathTemplate = ShinobiConfig.ShinobiTemplateHttp;
            }

            // part[1] is empty
            var credentialAndIpPort = parts[2];

            var parts2 = credentialAndIpPort.Split('@');
            var credentail = parts2[0].Split(':');
            var userName = credentail[0];
            var pwd = credentail[1];
            var ipPort = parts2[1].Split(':');
            var ip = ipPort[0];
            var mid = camera.IpAddress.Replace(".", "_");//shinobi mid not allow '.'
            var port = ipPort[1];

            var subStreamPath = "";
            if (parts.Length > 3)
            {
                for (var i = 3; i < parts.Length; i++)
                {
                    subStreamPath += "/" + parts[i];
                }
            }

            var json = File.ReadAllText(pathTemplate);
            json = json.Replace("\"mid\":\"\"", $"\"mid\":\"{mid}\"");
            json = json.Replace("\"name\":\"\"", $"\"name\":\"{mid}\"");
            json = json.Replace("\"host\":\"\"", $"\"host\":\"{ip}\"");
            json = json.Replace("\"port\":\"\"", $"\"port\":\"{port}\"");
            json = json.Replace("\"path\":\"\"", $"\"path\":\"{subStreamPath}\"");
            json = json.Replace("\\\"auto_host\\\":\\\"\\\"", $"\\\"auto_host\\\":\\\"{cameraStreamUrl}\\\"");
            json = json.Replace("\\\"muser\\\":\\\"\\\"", $"\\\"muser\\\":\\\"{userName}\\\"");
            json = json.Replace("\\\"mpass\\\":\\\"\\\"", $"\\\"mpass\\\":\\\"{pwd}\\\"");

            json = HttpUtility.UrlEncode(json);

            return url + $"/{mid}/?data={json}";
        }

    }

}
