using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using dotenv.net;
using BtcAddress; // from the Bitcoin Address Utility

class Program
{
    static ITelegramBotClient botClient;

    static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Type == UpdateType.Message && update.Message!.Text != null)
        {
            var text = update.Message.Text;

            if (text.StartsWith("/start"))
            {
                await bot.SendTextMessageAsync(update.Message.Chat.Id,
                    "Welcome 🚀\nUse /gen to generate a Bitcoin address",
                    cancellationToken: ct);
            }
            else if (text.StartsWith("/gen"))
            {
                var key = new KeyPair();
                string address = key.AddressBase58;
                string privKey = key.PrivateKeyWif;

                await bot.SendTextMessageAsync(update.Message.Chat.Id,
                    $"💳 Address: {address}\n🔑 Private Key: {privKey}",
                    cancellationToken: ct);
            }
        }
    }

    static Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return Task.CompletedTask;
    }

    static async Task Main()
    {
        // Load local .env (optional for dev)
        DotEnv.Load();

        var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("❌ Bot token not found!");
            return;
        }

        botClient = new TelegramBotClient(token);

        using var cts = new CancellationTokenSource();

        botClient.StartReceiving(
            new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
            cts.Token);

        Console.WriteLine("🤖 Bot running...");
        await Task.Delay(-1, cts.Token); // keep app alive
    }
}
