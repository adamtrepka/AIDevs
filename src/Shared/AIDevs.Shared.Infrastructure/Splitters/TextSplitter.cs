using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs.Shared.Infrastructure.Splitters
{
    public class TextSplitter
    {
        private readonly string _separator;
        private int _chunkSize;
        private int _chunkOverlap;
        private Func<string, int> _lengthFunction;

        public TextSplitter(string separator = "\n\n", int chunkSize = 4000, int chunkOverlap = 200, Func<string, int>? lengthFunction = null)
        {
            if (chunkOverlap > chunkSize)
            {
                throw new ArgumentException($"Chunk overlap ({chunkOverlap}) is greater than chunk size ({chunkSize}).");
            }
            _separator = separator;
            _chunkSize = chunkSize;
            _chunkOverlap = chunkOverlap;
            _lengthFunction = lengthFunction ?? new Func<string, int>((str) => str.Length);
        }

        public List<string> SplitText(string text)
        {
            List<string> splits;
            if (_separator != null)
            {
                splits = text.Split(new[] { _separator }, StringSplitOptions.None).ToList();
            }
            else
            {
                splits = new List<string> { text };
            }
            return MergeSplits(splits, _separator!);
        }

        private List<string> MergeSplits(IEnumerable<string> splits, string separator)
        {
            var separatorLen = _lengthFunction(separator);
            var docs = new List<string>();
            var currentDoc = new List<string>();
            int total = 0;

            foreach (var split in splits)
            {
                int len = _lengthFunction(split);

                
                if (total + len + (currentDoc.Count > 0 ? separatorLen : 0) >= _chunkSize)
                {
                    if (total > _chunkSize)
                    {
                        // todo: Implement a logger
                        // todo: Log a warning about a split that is larger than the chunk size
                    }


                    if (currentDoc.Count > 0)
                    {
                        var doc = JoinDocs(currentDoc, separator);
                        if (doc != null)
                        {
                            docs.Add(doc);
                        }

                        while (total > _chunkOverlap || (total + len + (currentDoc.Count > 1 ? separatorLen : 0) > _chunkSize && total > 0))
                        {
                            total -= _lengthFunction(currentDoc[0]) + (currentDoc.Count > 1 ? separatorLen : 0);
                            currentDoc.RemoveAt(0);
                        }
                    }
                }

                currentDoc.Add(split);
                total += len + (currentDoc.Count > 1 ? separatorLen : 0);
            }

            var lastDoc = JoinDocs(currentDoc, separator);
            if (lastDoc != null)
            {
                docs.Add(lastDoc);
            }

            return docs;
        }

        private static string? JoinDocs(List<string> docs, string separator)
        {
            var text = string.Join(separator, docs).Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }
    }
}
