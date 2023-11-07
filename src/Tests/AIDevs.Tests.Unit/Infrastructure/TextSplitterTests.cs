using AIDevs.Shared.Infrastructure.Splitters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs.Tests.Unit.Infrastructure
{
    public class TextSplitterTests
    {
        [Fact]
        public void Should_Split_Text()
        {
            // Arrange
            var input = File.ReadAllText("Assets/LongText.txt");

            // Act
            var splitter = new TextSplitter(separator: ".", chunkSize: 4000);
            var chunks = splitter.SplitText(input);

            // Assert
            chunks.Should().NotBeNullOrEmpty();
            chunks.Should().AllSatisfy(chunk => chunk.Length.Should().BeLessThanOrEqualTo(4000));
            
        }
    }
}
