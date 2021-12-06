using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace HomeWork_09_SKP
{
    /// <summary>
    /// Статический класс с методами взаимодействия с репозиторием 
    /// </summary>
    static class Repository
    {
        //строка хранящая путь к репозиторию
        static readonly string pathToRepository = Environment.CurrentDirectory + "\\repo\\";
                
        /// <summary>
        /// Метод создания директории для загрузки/скачивания и хранения файлов, в случае если такой директории не существует
        /// </summary>
        static public void CreateRepoDirectory()
        {
            if (!CheckRepoDirectory())
            {
                Directory.CreateDirectory(pathToRepository);
            }
        }

        /// <summary>
        /// Метод проверки существования директории для загрузки/скачивания и хранения файлов
        /// </summary>
        /// <returns>Результат проверки</returns>
        static public bool CheckRepoDirectory()
        {
            DirectoryInfo path = new DirectoryInfo(pathToRepository);
            if (path.Exists) return true;
            else return false;
        }

        /// <summary>
        /// Метод отправки выбранного пользователем файла в его чат
        /// </summary>
        /// <param name="botClient">Чат-бот от которого поступает запрос</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="chatId">ID чата</param>
        static public async void Upload(TelegramBotClient botClient, string fileName, ChatId chatId)
        {
            fileName = pathToRepository + "\\" + fileName;

            try
            {
                using (FileStream stream = System.IO.File.OpenRead(fileName))
                {
                    InputOnlineFile inputOnlineFile = new InputOnlineFile(stream, fileName);
                    await botClient.SendDocumentAsync(chatId, inputOnlineFile);
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                await botClient.SendTextMessageAsync(chatId: chatId, text: $"File \"{fileName}\" does not exists");
            }
        }

        /// <summary>
        /// Метод сохранения передаваемого пользователем документа в репозиторий
        /// </summary>
        /// <param name="botClient">Чат-бот от которого поступает запрос</param>
        /// <param name="update">Передаваемый пользователем update содержащий файл</param>
        static public async void Download(TelegramBotClient botClient, Update update)
        {
            string path = pathToRepository + update.Message.Document.FileName;
            var file = await botClient.GetFileAsync(update.Message.Document.FileId);
            FileStream fs = new FileStream(path, FileMode.Create);
            await botClient.DownloadFileAsync(file.FilePath, fs);
            fs.Close();

            fs.Dispose();
        }

    }
}
