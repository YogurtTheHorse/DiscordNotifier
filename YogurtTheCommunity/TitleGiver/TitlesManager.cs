using System.Diagnostics.CodeAnalysis;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.TitleGiver;

public class TitlesManager
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ILogger<TitlesManager> _logger;
    private readonly MembersStorage _membersStorage;
    private readonly ChatsRegistry _chatsRegistry;

    public TitlesManager(ITelegramBotClient telegramBotClient, ILogger<TitlesManager> logger, MembersStorage membersStorage,
        ChatsRegistry chatsRegistry)
    {
        _telegramBotClient = telegramBotClient;
        _logger = logger;
        _membersStorage = membersStorage;
        _chatsRegistry = chatsRegistry;
    }

    public async Task<bool> UpdateTitle(Guid memberId, string title)
    {
        var tgId = await _membersStorage.GetTelegramId(memberId);

        if (!tgId.HasValue)
        {
            _logger.LogWarning("Error setting title for {memberId} - no telegram id", memberId);

            return false;
        }

        var mangedChats = await _chatsRegistry.GetManagedTelegramChats();

        foreach (var chat in mangedChats)
        {
            await UpdateTitleInChat(memberId, title, chat, tgId.Value);
        }

        return true;
    }

    public async Task UpdateTitleInChat(Guid memberId, string title, long chat, long tgId)
    {
        var admins = await _telegramBotClient.GetChatAdministratorsAsync(chat);

        if (admins.All(x => x.User.Id != tgId))
        {
            try
            {
                await _telegramBotClient.PromoteChatMemberAsync(
                    chat,
                    tgId,
                    canManageChat: true
                );
            }
            catch (ApiRequestException ex)
            {
                _logger.LogError(ex, "Error promoting {memberId} in chat {chat}", memberId, chat);
                return;
            }
        }

        try
        {
            await _telegramBotClient.SetChatAdministratorCustomTitleAsync(chat, tgId, title);
        }
        catch (ApiRequestException ex)
        {
            _logger.LogError(ex, "Error setting title in chat {chat} for {memberId}", chat, memberId);
        }
    }
}