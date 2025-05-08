using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Telegram.Bot.TelegramBotClient;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var botClient = new TelegramBotClient("7933914671:AAEtOU4Op6kCNPskqk_q_sbCAGfveludvTg");

        using var cts = new CancellationTokenSource();

        var me = await botClient.GetMe(cts.Token);
        Console.WriteLine($"Бот запущен: @{me.Username}");

        var handler = new BotUpdateHandler();

        botClient.StartReceiving(
            updateHandler: handler,
            receiverOptions: new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            },
            cancellationToken: cts.Token
        );

        Console.ReadLine();
        cts.Cancel();
    }
}



