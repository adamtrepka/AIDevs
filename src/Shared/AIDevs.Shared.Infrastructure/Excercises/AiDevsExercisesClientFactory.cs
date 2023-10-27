namespace AIDevs.Shared.Infrastructure.Excercises
{
    public static class AiDevsExercisesClientFactory
    {
        private static readonly HttpClient _httpClient;

        static AiDevsExercisesClientFactory()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://zadania.aidevs.pl")
            };
        }

        public static AiDevsExercisesClient Create()
            => new AiDevsExercisesClient(_httpClient);
    }
}