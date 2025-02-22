using FluentAssertions;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.ML;
using Xunit;

namespace TelegramPartHook.UnitTests
{
    public class MLTests
    {
        //[InlineData("C:\\Users\\rrnaz\\Downloads\\Partituras @tieeoficial Samba pro meu PovoCréditos_ @ramon.__________ Arranjos_ @ramon.__________ Ficha técnica Baixo_ Ramon Torres – Instagram @ramon.__________ Cavaco_ Tuim Vasconcelos – Instagram @tuimoficial Te.jpg", ImageClassification.NoSheet)]        
        [InlineData("C:\\Users\\rrnaz\\Pictures\\Partituras\\Pixote Drivein\\nuance Pixote Drive In (10).jpg", ImageClassification.Sheet)]
        [Theory(Skip = "Don't needed")]
        public void PredictionTest(string path, ImageClassification classification)
        {
            using var analyzer = new SheetAnalyzer();

            var sheet = new SheetSearchResult(path, TelegramPartHook.Domain.Constants.Enums.FileSource.Crawler);

            var result = analyzer.Predict(sheet);

            result.score.Should().BeGreaterThan(0.7F);
            result.classification.Should().Be(classification);
        }
    }
}
