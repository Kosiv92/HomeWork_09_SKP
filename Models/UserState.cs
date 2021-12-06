namespace HomeWork_09_SKP
{
    /// <summary>
    /// Класс хранящий состояние взаимодействия с пользователем в чает
    /// </summary>
    internal class UserState
    {
        
        public UserState()
        {
            WeatherSearchState = WeatherSearchState.isOff;

            FileSendState = FileSendState.isOff;
        }

        /// <summary>
        /// Состояние поиска погоды
        /// </summary>
        public WeatherSearchState WeatherSearchState { get; set; }

        /// <summary>
        /// Состояние загрузки файлов
        /// </summary>
        public FileSendState FileSendState { get; set; }
    }
}