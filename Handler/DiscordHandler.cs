using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using GVRP.Module.Configurations;
using GVRP.Module.Logging;

namespace GVRP.Handler
{
    public class DiscordHandler
    {
        private string m_LiveWebhookURL = "https://discord.com/api/webhooks/922161435222626356/qMlIUswyyHOtfwrHpOkiBQtZsdD2N3TuuNSnDO1_3ocW3LiYs6YbVxB9Ny-hvzYibFoR";
        private string m_TestWebhookURL = "https://discord.com/api/webhooks/922161623437828126/pP6h8MQMp-umk9aoTP3o_wdb-_g8oHgKKTeRKfXWNZGOfY7VRqZtzEa24Eh2U1AGBaVF";

        public DiscordHandler()
        {
        }

        public enum Channels
        {
            KILL,
            SUPPORT,
            ANTICHEAT,
            DEFAULT
        }

        public void SendMessage(string p_Message, string p_Description = "")
        {

            try
            {
                DiscordMessage l_Message = new DiscordMessage($"{p_Message}", p_Description);

                     using (WebClient l_WC = new WebClient())
                 {
                    l_WC.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    l_WC.Encoding = Encoding.UTF8;

                     string l_Upload = JsonConvert.SerializeObject(l_Message);
                    if (Configuration.Instance.DevMode)
                     l_WC.UploadString(m_TestWebhookURL, l_Upload);
                       else
                    l_WC.UploadString(m_LiveWebhookURL, l_Upload);
                     }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                // muss funken lol
                // grüße von monkaMC
            }
        }
    }

    public class DiscordMessage
    {
        public string content { get; private set; }
        public bool tts { get; private set; }
        public EmbedObject[] embeds { get; private set; }

        public DiscordMessage(string p_Message, string p_EmbedContent)
        {
            content = p_Message;
            tts = false;

            EmbedObject l_Embed = new EmbedObject(p_EmbedContent);
            embeds = new EmbedObject[] { l_Embed };
        }
    }

    public class EmbedObject
    {
        public string title { get; private set; }
        public string description { get; private set; }

        public EmbedObject(string p_Desc)
        {
            title = DateTime.Now.ToString();
            description = p_Desc;
        }
    }
}
