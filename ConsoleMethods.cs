using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeWork_09_SKP
{
    /// <summary>
    /// Статический класс хранящий методы передающие логи в консоль
    /// </summary>
    public static class ConsoleMethods
    {
        /// <summary>
        /// Метод вывода в окно консоли информации о запущенном боте
        /// </summary>
        /// <param name="botClient">Запущенный Telegram бот</param>
        public static async void GetUserInformation(TelegramBot botClient)
        {
            User me = await botClient.BotClient.GetMeAsync();
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
        }

        /// <summary>
        /// Метод вывода в консоль сообщения об ошибке
        /// </summary>
        /// <param name="errorMessage"></param>
        static public void GetErrorMesage(string errorMessage)
        {
            Console.WriteLine(errorMessage);
        }

        /// <summary>
        /// Метод передачи в консоль информации о принимаемых ботом текстовых сообщениях с указанием чата
        /// </summary>
        /// <param name="update">Текстовое сообщение</param>
        /// <param name="chatId">ID чата</param>
        public static void GetUpdateMessage(Update update)
        {
            
            Console.WriteLine($"Received a '{update.Message.Text}' message in chat {update.Message.Chat.Id}.");
        }

    }
}
