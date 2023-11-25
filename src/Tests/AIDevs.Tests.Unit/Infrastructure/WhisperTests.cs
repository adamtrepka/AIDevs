using AIDevs.Shared.Infrastructure.Whisper;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Whisper.net;

namespace AIDevs.Tests.Unit.Infrastructure
{
    public class WhisperTests
    {
        //const string ModelPath = @"D:\Repos\Whisper\ggml-large.bin";
        const string ModelPath = @"D:\Repos\Whisper\ggml-small-q5_1.bin";
        const string FilePath = @"C:\Users\adamt\Downloads\ffmpeg-master-latest-win64-gpl\ffmpeg-master-latest-win64-gpl\bin\input.wav";

        [Fact]
        public async Task Should_Generate_Transcription()
        {
            var transcription = new ObservableCollection<TranscriptionSegment>();
            transcription.CollectionChanged += (src, args) =>
            {
                if (args.NewItems is not null)
                {
                    foreach (var item in args.NewItems)
                    {
                        if(item is TranscriptionSegment segment)
                        {
                            Debug.WriteLine($"[{segment.TimeStamp.From} --> {segment.TimeStamp.To}] : {segment.Content}");
                        }
                    }
                };
            };
            using var whisperFactory = WhisperFactory.FromPath(ModelPath);
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("pl")
                .WithTranslate()
                .WithNoContext()
                .WithThreads(20)
                .Build();

            using var fileStream = File.OpenRead(FilePath);

            await foreach (var result in processor.ProcessAsync(fileStream))
            {
                transcription.Add(new(result.Text, new(result.Start, result.End)));
            }

            var json = JsonSerializer.Serialize(transcription);
            File.WriteAllText("transcription.json", json);
        }
    }
}
