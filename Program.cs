using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeWork_09_SKP
{
    class Program
    {        
        static void Main(string[] args)
        {

            string token = System.IO.File.ReadAllText("token.txt"); //получаем токен из текстового файла                       

            TelegramBot telegramBot = new TelegramBot(token);

            ConsoleMethods.GetUserInformation(telegramBot);

            Repository.CreateRepoDirectory();

            telegramBot.StartReceiveUpdates();

        }              
               
    }
}
