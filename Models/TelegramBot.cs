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
using System.IO;

namespace HomeWork_09_SKP
{
    public class TelegramBot
    {
        readonly string token;

        readonly TelegramBotClient _botClient;

        readonly string pathToRepository = Environment.CurrentDirectory + "\\repo\\";

        int numberOfFile = 0;

        public TelegramBotClient BotClient { get => _botClient; }

        const string DownloadText = "Download";
        const string VideoText = "Video";
        const string MusicText = "Music";
        const string SchoolText = "School";
        const string AddressText = "Address";
        const string WeatherText = "Weather";
        const string UploadText = "Upload";
        const string PrevFileText = "<";
        const string NextFileText = ">";
        const string CancelText = "Cancel";



        public TelegramBot(string token)
        {
            this.token = token;

            _botClient = new TelegramBotClient(token);

        }

        CancellationToken cts = new CancellationToken();

        private Dictionary<long, UserState> _userState = new Dictionary<long, UserState>();

        /// <summary>
        /// Метод запуска приема обновлений от клиентов
        /// </summary>
        public void StartReceiveUpdates()
        {
            using var cts = new CancellationTokenSource();
            BotClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), cts.Token);
            Console.ReadLine();
            cts.Cancel();
        }

        /// <summary>
        /// Метод обнаружения ошибок и их вывода на экран консоли
        /// </summary>
        /// <param name="botClient">Бот получающий ошибку</param>
        /// <param name="exception">Ошибка(исключение)</param>
        /// <param name="cancellationToken">Токен прерывания</param>
        /// <returns></returns>        
        static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            ConsoleMethods.GetErrorMesage(ErrorMessage);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Метод обработки обновлений от пользователя
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Thread.Sleep(500);

            if (update.Type != UpdateType.Message) return;

            var chatId = update.Message.Chat.Id;

            ConsoleMethods.GetUpdateMessage(update, chatId);

            if (update.Message.Type != MessageType.Text) MessageHandler(update);
            else TextHandler(update);

        }

        async private void MessageHandler(Update update)
        {

            switch (update.Message.Type)
            {
                case MessageType.Sticker:
                    SendSticker(update);
                    break;
                case MessageType.Photo:
                    Repository.Download(_botClient, update);
                    break;
                case MessageType.Document:
                    Repository.Download(_botClient, update);
                    break;
                case MessageType.Audio:
                    Repository.Download(_botClient, update);
                    break;
            }


        }

        private void TextHandler(Update update)
        {

            if (_userState.ContainsKey(update.Message.Chat.Id) && (_userState[update.Message.Chat.Id].WeatherSearchState == WeatherSearchState.IsOn || _userState[update.Message.Chat.Id].FileSendState == FileSendState.IsOn))
            {
                if (_userState[update.Message.Chat.Id].WeatherSearchState == WeatherSearchState.IsOn)
                {
                    WeatherHandler(update);
                }
                else if (_userState[update.Message.Chat.Id].FileSendState == FileSendState.IsOn)
                {
                    UploadHandler(update);
                }
            }
            else
            {
                _botClient.SendTextMessageAsync(update.Message.Chat.Id, text: "Choose action", replyMarkup: GetButtons());
                switch (update.Message.Text)
                {
                    case VideoText:
                        SendVideo(update);
                        break;
                    case SchoolText:
                        SendReference(update);
                        break;
                    case MusicText:
                        SendMusic(update);
                        break;
                    case AddressText:
                        SendAddress(update);
                        break;
                    case WeatherText:
                        TurnOnWeatherSearch(update);
                        break;
                    case UploadText:
                        TurnOnFileUpload(update);
                        break;
                }
            }



        }

        async private void TurnOnWeatherSearch(Update update)
        {
            if (_userState.ContainsKey(update.Message.Chat.Id)) _userState[update.Message.Chat.Id].WeatherSearchState = WeatherSearchState.IsOn;
            else _userState[update.Message.Chat.Id] = new UserState { WeatherSearchState = WeatherSearchState.IsOn };

            _botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Write name of city which weather you need to know!", replyMarkup: new ReplyKeyboardMarkup("Cancel"));
        }

        private void WeatherHandler(Update update)
        {
            if (update.Message.Text == CancelText)
            {
                _userState[update.Message.Chat.Id].WeatherSearchState = WeatherSearchState.IsOff;
                TextHandler(update);
            }
            else
            {
                SendWeatherForecast(update);
            }
        }

        async private void SendWeatherForecast(Update update)
        {
            string temperature = WeatherHerald.WeatherRequest(update.Message.Text);
            await _botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: temperature);
        }

        async private void TurnOnFileUpload(Update update)
        {
            if (_userState.ContainsKey(update.Message.Chat.Id)) _userState[update.Message.Chat.Id].FileSendState = FileSendState.IsOn;
            else _userState[update.Message.Chat.Id] = new UserState { FileSendState = FileSendState.IsOn };
        }

        async private void UploadHandler(Update update)
        {
            FileInfo[] files = Repository.GetFilesName();

            await _botClient.SendTextMessageAsync(update.Message.Chat.Id, text: "Choose file to upload", replyMarkup: GetUploadButtons(files, numberOfFile));

            switch (update.Message.Text)
            {
                case CancelText:
                    _userState[update.Message.Chat.Id].FileSendState = FileSendState.IsOff;
                    numberOfFile = 0;
                    TextHandler(update);
                    break;
                case PrevFileText:
                    if (numberOfFile > 0) numberOfFile--;
                    else numberOfFile = files.Length - 1;
                    break;
                case NextFileText:
                    if (numberOfFile < files.Length - 1) numberOfFile++;
                    else numberOfFile = 0;
                    break;
                case UploadText:
                    break;
                default:
                    //Upload(update.Message.Text, update.Message.Chat.Id);
                    Repository.Upload(_botClient, update.Message.Text, update.Message.Chat.Id);
                    break;
            }

        }

        //public async void Upload(string fileName, ChatId chatId)
        //{
        //    fileName = pathToRepository + "\\" + fileName;

        //    try
        //    {
        //        using (FileStream stream = System.IO.File.OpenRead(fileName))
        //        {
        //            InputOnlineFile inputOnlineFile = new InputOnlineFile(stream, fileName);
        //            await BotClient.SendDocumentAsync(chatId, inputOnlineFile);
        //        }
        //    }
        //    catch (System.IO.FileNotFoundException)
        //    {
        //        await _botClient.SendTextMessageAsync(chatId: chatId, text: $"File \"{fileName}\" does not exists");
        //    }
        //}

        //async void DownLoad(Update update)
        //{
        //    string path = pathToRepository + update.Message.Document.FileName;
        //    var file = await BotClient.GetFileAsync(update.Message.Document.FileId);
        //    FileStream fs = new FileStream(path, FileMode.Create);
        //    await BotClient.DownloadFileAsync(file.FilePath, fs);
        //    fs.Close();

        //    fs.Dispose();
        //}

        async private void SendReference(Update update)
        {

            Message message = await _botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                    text: "You can *learn* many `professions` on this site", parseMode: ParseMode.Markdown,
                    disableNotification: true, replyToMessageId: update.Message.MessageId,
                    replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Online school", "https://skillbox.ru")));

        }

        async private void SendMusic(Update update)
        {
            Message message = await _botClient.SendAudioAsync(
                    chatId: update.Message.Chat.Id,
                    audio: "https://github.com/TelegramBots/book/raw/master/src/docs/audio-guitar.mp3"
                    /* ,
                    performer: "Joel Thomas Hunger",
                    title: "Fun Guitar and Ukulele",
                    duration: 91 // in seconds
                    */);
        }

        async private void SendVideo(Update update)
        {
            await _botClient.SendVideoAsync(chatId: update.Message.Chat.Id, video: "https://github.com/TelegramBots/book/raw/master/src/docs/video-bulb.mp4");

        }

        async private void SendAddress(Update update)
        {
            Message message = await _botClient.SendVenueAsync(
                    chatId: update.Message.Chat.Id,
                    latitude: 56.287686f,
                    longitude: 101.783908f,
                    title: "Home of Siberia energy",
                    address: "Bratsk, Russia");
        }

        async private void SendSticker(Update update)
        {
            await _botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Cool! Now check out mine!");
            await _botClient.SendStickerAsync(chatId: update.Message.Chat.Id, sticker: "https://tlgrm.ru/_/stickers/5a7/cb3/5a7cb3d0-bca6-3459-a3f0-5745d95d54b7/1.webp");
        }

        async private void SendPhoto(Update update)
        {
            await _botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Not bad! I have one photo for you too!");
            Message message = await _botClient.SendPhotoAsync(
            chatId: update.Message.Chat.Id,
            photo: "https://img1.goodfon.ru/wallpaper/nbig/a/e7/dzheyson-steytem-jason-statham-1317.jpg",
            caption: "<b>Jason Statham</b>. <i>Source</i>: <a href=\"https://www.kinopoisk.ru/name/1514/\">Kinopoisk</a>",
            parseMode: ParseMode.Html);
        }


        //async private void TurnOnFileSender(Update update)
        //{
        //    await _botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Choose file to upload!", replyMarkup: new ReplyKeyboardMarkup("Cancel"));
        //    _userState[update.Message.Chat.Id].WeatherSearchState = WeatherSearchState.isOn;
        //}


        //private FileInfo[] GetFilesName()
        //{
        //    DirectoryInfo d = new DirectoryInfo(pathToRepository);
        //    FileInfo[] Files = d.GetFiles("*.pdf");
        //    string str = "";
        //    foreach (FileInfo file in Files)
        //    {
        //        str = str + ", " + file.Name;
        //    }
        //    return Files;
        //}

        private IReplyMarkup GetButtons()
        {

            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{ new KeyboardButton { Text = VideoText }, new KeyboardButton { Text = MusicText } },
                    new List<KeyboardButton>{ new KeyboardButton { Text = SchoolText }, new KeyboardButton { Text = AddressText } },
                    new List<KeyboardButton>{ new KeyboardButton { Text = WeatherText }, new KeyboardButton { Text = UploadText } }
                    },
                ResizeKeyboard = true
            };
        }



        private IReplyMarkup GetUploadButtons(FileInfo[] files, int position)
        {
            string filename = files[position].Name;

            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{ new KeyboardButton { Text = PrevFileText } },
                    new List<KeyboardButton>{ new KeyboardButton { Text = filename } },
                    new List<KeyboardButton>{ new KeyboardButton { Text = NextFileText } },
                    new List<KeyboardButton>{ new KeyboardButton { Text = "Cancel" } }
                    },
                ResizeKeyboard = true
            };
        }
    }
}
