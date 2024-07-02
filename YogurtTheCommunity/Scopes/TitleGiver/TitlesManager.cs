using Telegram.Bot;
using Telegram.Bot.Exceptions;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.TitleGiver;

public class TitlesManager(
    ITelegramBotClient telegramBotClient,
    ILogger<TitlesManager> logger,
    MembersStorage membersStorage,
    ChatsRegistry chatsRegistry)
{
    public async Task<bool> UpdateTitle(Guid memberId, string title)
    {
        var tgId = await membersStorage.GetTelegramId(memberId);

        if (!tgId.HasValue)
        {
            logger.LogWarning("Error setting title for {memberId} - no telegram id", memberId);

            return false;
        }

        var mangedChats = await chatsRegistry.GetManagedTelegramChats();

        foreach (var chat in mangedChats)
        {
            await UpdateTitleInChat(memberId, title, chat, tgId.Value);
        }

        return true;
    }

    public async Task UpdateTitleInChat(Guid memberId, string title, long chat, long tgId)
    {
        var admins = await telegramBotClient.GetChatAdministratorsAsync(chat);

        if (admins.All(x => x.User.Id != tgId))
        {
            try
            {
                await telegramBotClient.PromoteChatMemberAsync(
                    chat,
                    tgId,
                    canManageChat: true
                );
            }
            catch (ApiRequestException ex)
            {
                logger.LogError(ex, "Error promoting {memberId} in chat {chat}", memberId, chat);
                return;
            }
        }

        try
        {
            await telegramBotClient.SetChatAdministratorCustomTitleAsync(chat, tgId, title);
        }
        catch (ApiRequestException ex)
        {
            logger.LogError(ex, "Error setting title in chat {chat} for {memberId}", chat, memberId);
        }
    }
}