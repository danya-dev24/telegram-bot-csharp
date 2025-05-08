using System;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class BotUpdateHandler : IUpdateHandler
{
    static int total = 0;
    static int good = 0;    
    static int bad = 0;

    static string statsFile = "stats.txt";

    static BotUpdateHandler()
    {
        if (System.IO.File.Exists(statsFile))
        {
            string[] parts = System.IO.File.ReadAllText(statsFile).Split(';');
            foreach (var part in parts)
            {
                var kv = part.Split('=');
                if (kv.Length == 2)
                {
                    if (kv[0] == "total") total = int.Parse(kv[1]);
                    if (kv[0] == "good") good = int.Parse(kv[1]);
                    if (kv[0] == "bad") bad = int.Parse(kv[1]);
                }
            }
        }
    }
    static void SaveStats()
    {
        string data = $"total={total};good={good};bad={bad}";
        System.IO.File.WriteAllText(statsFile, data);


    }



    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message?.Text == null)
            return;

        var message = update.Message;
        var chatId = message.Chat.Id;
        var text = message.Text;
        total++;
        SaveStats();
        double sum = 0;
        double result = 0;
        string[] parts = text.Split();
        string command = parts[0];
        string res = command switch


        {
            "/start" => null,
            "/hello" => null,
            "/help" => "доступные команды: /start, /hello, /help, /info, /repeat, /stats, /time",
            "/info" => "я бот написанный на C#",
            "/repeat" => null,
            "/stats" => null,
            "/calc" => null,
            "/time" => null,
            _ => "неизвестная команда",
        };
        if (parts[0] == "/time")
        {
            DateTime dateTime = DateTime.Now;
            string response = $"сейчас: {dateTime.ToString("dd.MM.yyyy HH:mm")}";
            await botClient.SendTextMessageAsync(chatId, response, cancellationToken: cancellationToken);
        }




        if (parts[0] == "/start")
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] {new KeyboardButton("/start"),new KeyboardButton("/help")},
                new[] {new KeyboardButton("/hello"),new KeyboardButton("/info")},
                new[] {new KeyboardButton("/stats") ,new KeyboardButton("/time")}
            })
            {
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "привет, я бот, вот мои команды, команды которых нет в кнопках,вы можете найти - написав команду /help",
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
                );
        }






        if (!string.IsNullOrEmpty(res))
        {

            await botClient.SendTextMessageAsync(chatId, res, cancellationToken: cancellationToken);
        }

        if (res == "неизвестная команда")
        {
            bad++;
            SaveStats();
        }
        else { good++; SaveStats(); }


        if (parts[0] == "/hello")
        {
            if (parts.Length > 1)
            {
                await botClient.SendTextMessageAsync(chatId, $"привет, {parts[1]}", cancellationToken: cancellationToken);
            }

            else
            {
                await botClient.SendTextMessageAsync(chatId, "вам нужно ввести /hello и имя ", cancellationToken: cancellationToken);
            }

        }
        

        
        if (parts[0] == "/repeat")
        {
            if (parts.Length < 3)
            {
                await botClient.SendTextMessageAsync(chatId, "вам нужно ввести /repeat количество повторов и слово    ", cancellationToken: cancellationToken);


            }
            else
            {
                int kol;
                bool ok = int.TryParse(parts[1], out kol);
                if (ok)
                {
                    if (kol > 20)
                    {
                        await botClient.SendTextMessageAsync(chatId, "количество повторов должно быть меньше 20", cancellationToken: cancellationToken);
                        return;
                    }
                    for (int i = 0; i < kol; i++)
                    {
                        await botClient.SendTextMessageAsync(chatId, parts[2], cancellationToken: cancellationToken);

                    }


                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, $"вы должны заменить {parts[1]} на число ", cancellationToken: cancellationToken);
                }
            }


        }

        if (parts[0] == "/stats")
        {
            await botClient.SendTextMessageAsync(chatId, $"всего команд {total} ", cancellationToken: cancellationToken);
            await botClient.SendTextMessageAsync(chatId, $"успешных команд {good}", cancellationToken: cancellationToken);
            await botClient.SendTextMessageAsync(chatId, $"неизвестных {bad}", cancellationToken: cancellationToken);

        }

        if (parts[0] == "/calc")
        {
            if (parts.Length == 4)
            {
                string resultText = null;
                bool ok1 = double.TryParse(parts[1], out double num1);
                bool ok2 = double.TryParse(parts[3], out double num2);

                if (ok1 && ok2)
                {



                    if (parts[2] == "+" || parts[2] == "-" || parts[2] == "*" || parts[2] == "/")
                    {
                        switch (parts[2])
                        {
                            case "+":
                                result = num1 + num2;
                                resultText = $"результат: {result}";
                                break;
                            case "-":
                                result = num1 - num2;
                                resultText = $"результат: {result}";
                                break;
                            case "/":
                                if (num2 != 0)
                                {
                                    result = num1 / num2;
                                    resultText = $"результат: {result}";
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(chatId, "делить на ноль нельзя", cancellationToken: cancellationToken);
                                }
                                break;
                            case "*":
                                result = num1 * num2;
                                resultText = $"результат: {result}";
                                break;
                            default:
                                await botClient.SendTextMessageAsync(chatId, "ваш знак должен быть +, -, /, *", cancellationToken: cancellationToken);
                                break;
                        }
                        if (!string.IsNullOrEmpty(resultText))
                        {
                            await botClient.SendTextMessageAsync(chatId, $" {resultText}", cancellationToken: cancellationToken);

                        }

                    }

                  
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "вы дожны ввести число знак и число ", cancellationToken: cancellationToken);

                }


            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "вы должны ввести /calc число знак(+, -, *, /) и число", cancellationToken: cancellationToken);
            }









        }

       


    }
    

    public Task HandleErrorAsync(
     ITelegramBotClient botClient,
     Exception exception,
     HandleErrorSource errorSource,
     CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        return Task.CompletedTask;
    }
}