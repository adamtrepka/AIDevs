using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AIDevs.Shared.Infrastructure.Whisper
{
    public record TimeStamp(TimeSpan From, TimeSpan To)
    {
        [JsonIgnore]
        public TimeSpan Duration => To - From;
    }

    public record TranscriptionSegment(string Content, TimeStamp TimeStamp);

    public record TranscriptionWindow(IEnumerable<TranscriptionSegment> Segments)
    {
        public TimeStamp TimeStamp => new(Segments.Min(x => x.TimeStamp.From), Segments.Max(x => x.TimeStamp.To));
        public string Content => string.Join(" ", Segments.Select(x => x.Content.Trim()));
    }

    public static class TranscriptionSegmentExtensions
    {
        public static List<TranscriptionWindow> ToTranscriptionWindows(this ICollection<TranscriptionSegment> segments, TimeSpan windowWidth) 
        {
            var windows = new List<TranscriptionWindow>();

            var startTimestamp = segments.Min(x => x.TimeStamp.From);
            var endTimestamp = segments.Max(x => x.TimeStamp.To);

            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments.ElementAt(i);
                var timeToLeft = windowWidth - segment.TimeStamp.Duration;
                var timeOffset = timeToLeft / 2;

                var offsetRight = segments.Skip(i)
                                               .TakeWhile(x => x.TimeStamp.To - segment.TimeStamp.To < timeOffset);
                var offsetLeft = segments.Take(i)
                    .OrderByDescending(x => x.TimeStamp.From)
                    .TakeWhile(x => segment.TimeStamp.From - x.TimeStamp.From <= timeOffset)
                    .OrderBy(x => x.TimeStamp.From);

                var window = new TranscriptionWindow(offsetLeft.Concat(offsetRight));

                windows.Add(window);
            }

            windows = windows.SkipWhile(x => x.TimeStamp.From == startTimestamp).TakeWhile(x => x.TimeStamp.To !=  endTimestamp).ToList();

            return windows;
        }
    }
}
