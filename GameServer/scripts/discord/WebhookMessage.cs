using System;
using DOL.GS.PacketHandler;
using DOL.GS.PacketHandler.Client.v168;
using DOL.GS.ServerProperties;
using JNogueira.Discord.Webhook.Client;

namespace DOL.GS.Scripts.discord
{
    public static class WebhookMessage
    {
        public static void SendMessage(String webhookId, String message, String userName = "Titan Bot", string avatar = "https://cdn.discordapp.com/avatars/865566424537104386/282901fdaa488f57a95faae665a7c245.webp")
        {
            var client = new DiscordWebhookClient(webhookId);

            var content = message;
            var msg = new DiscordMessage(
                content,
                username: userName,
                avatarUrl: avatar,
                tts: false
            );
            client.SendToDiscord(msg);
        }
        
        public static void SendMessageWithFile(String webhookId, String message, DiscordFile file, String userName = "Titan Bot", string avatar = "https://cdn.discordapp.com/avatars/865566424537104386/282901fdaa488f57a95faae665a7c245.webp")
        {
            var client = new DiscordWebhookClient(webhookId);

            var content = message;
            var msg = new DiscordMessage(
                content,
                username: userName,
                avatarUrl: avatar,
                tts: false
            );
            client.SendToDiscord(msg, new[] { file });
        }
        public static void SendEmbeddedMessage(String webhookId, String message)
        {
            //TODO: Possibly implement this to have all other discord messages delegate through something like this
        }
        
        public static void LogChatMessage(GamePlayer player, eChatType chatType, String message)
        {
            // Format message
            String formattedMessage = "";
            switch (chatType)
            {
                case eChatType.CT_Broadcast:
                    formattedMessage = "**[" + player.CurrentZone.Description + "] ";
                    break;
                case eChatType.CT_Help:
                    formattedMessage = "**[HELP] ";
                    break;
                case eChatType.CT_Advise:
                    formattedMessage = "**[ADVICE] ";
                    break;
                case eChatType.CT_LFG:
                    formattedMessage = "**[LFG] (" + player.CharacterClass.Name + " " + player.Level + ") ";
                    break;
                case eChatType.CT_Trade:
                    formattedMessage = "**[TRADE] ";
                    break;
                default:
                    formattedMessage = "**[UNKNOWN] ";
                    break;
            }
            formattedMessage += player.Name + ":** " + message;

            string avatar;

            switch (chatType)
            {
                case eChatType.CT_Broadcast:
                    if (string.IsNullOrEmpty(Properties.DISCORD_REGION_WEBHOOK_ID))
                        return;
                    
                    switch (player.CurrentZone.Realm)
                    {
                        case eRealm._First:
                            avatar = "https://cdn.discordapp.com/attachments/861979059550421023/929455017902104627/alb.png";
                            SendMessage(Properties.DISCORD_REGION_WEBHOOK_ID, formattedMessage, avatar: avatar);
                            break;
                        case eRealm.Hibernia:
                            avatar = "https://cdn.discordapp.com/attachments/861979059550421023/929455017457496214/hib.png";
                            SendMessage(Properties.DISCORD_REGION_WEBHOOK_ID, formattedMessage, avatar: avatar);
                            break;
                        case eRealm.Midgard:
                            avatar = "https://cdn.discordapp.com/attachments/861979059550421023/929455017675616288/mid.png";
                            SendMessage(Properties.DISCORD_REGION_WEBHOOK_ID, formattedMessage, avatar: avatar);
                            break;
                    }
                    
                    break;
                
                default:
                    if (string.IsNullOrEmpty(Properties.DISCORD_ADVICE_WEBHOOK_ID))
                        return;
                    SendMessage(Properties.DISCORD_ADVICE_WEBHOOK_ID, formattedMessage);
                    break;
            }
        }
    }
}