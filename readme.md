Just a simple service for notifying about discord channel joins to Telegram

# Quick Start

## Discord Bot
1. Create Discord app via [official guide](https://discord.com/developers/docs/quick-start/getting-started)
2. Copypaste token from "Bot" page to `appsettings.json` (`DiscordNotifier.Token` section)
3. Turn off "Public bot" setting

## Telegram Section
1. Create Telegram bot via [@BotFather](https://t.me/BotFather)
2. Copypaste bot's token to `appsettings.json` (`Telegram.Token` section)
3. Goto `https://api.telegram.org/bot<token>/getUpdates` via your browser
4. Add bot to your channel/chat
5. If you are want to specify topic, create it and send any message to it
6. Refresh page at browser
7. You will see some events about chat, you need next values:
```json
{
  "ok": true,
  "result": [
    {
      "update_id": 112498495,
      "message": {
        "message_id": 69978,
        ...
        "chat": {
          "id": -10101782938225, <--- This is TelegramTargetId
          "title": "...",
        },
        "new_chat_participant": {
          "id": <yours_bot_id>,
          "is_bot": true,
          "first_name": "<Your's bot name>",
          "username": "<yours_bot_username>"
        },
        ...
      }
    },
    {
      "message": {
        "message_id": ...,
        ...
        "message_thread_id": 69979, <--- This is TelegramThreadId
        "forum_topic_created": {
          "name": "<Topic name>",
          ...
        },
        "is_topic_message": true
      }
    }
  ]
}
```
8. Copypaste them to `appsettings.json`

## Deploy and run

### Docker Compose
Just start it via `docker compose up -d`

### Any other way
Change `ConnectionStrings.Redis` field at `appsettings.json` to correct value and start app as you want.