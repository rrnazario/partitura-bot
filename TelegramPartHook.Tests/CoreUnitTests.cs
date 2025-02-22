using System;
using TelegramPartHook.Domain.Helpers;
using Xunit;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.UnitTests
{
    public class CoreUnitTests
    {
        [InlineData("es", "es")]
        [InlineData("en", "en")]
        [InlineData("pt-pt", "pt")]
        [InlineData("pt-br", "pt")]
        [InlineData("", "en")]
        [InlineData(null, "en")]
        [Theory(DisplayName = "Get different message varying by culture")]
        public void GetDifferentMessagesByCulture(string language, string expectedCulture)
        {
            var msg = MessageHelper.GetMessage(language, MessageName.NotFoundMessage1);

            Assert.Equal(msg, MessageHelper.GetMessage(expectedCulture, MessageName.NotFoundMessage1));
        }

        [Fact]
        public void CheckAllMessagesFromResourcesAreDifferentBetweenCultures()
        {
            var cultures = new[] { "pt", "es", "en" };
            var messages = Enum.GetValues<MessageName>();

            foreach (var message in messages)
            {
                for (int j = 0; j < cultures.Length - 2; j++)
                {
                    var outerCulture = cultures[j];
                    var outerMessage = MessageHelper.GetMessage(outerCulture, message);

                    for (int i = 1; i < cultures.Length - 1; i++)
                    {
                        var innerCulture = cultures[i];
                        var innerMessage = MessageHelper.GetMessage(innerCulture, message);

                        Assert.NotEqual(outerCulture, innerCulture);
                        Assert.NotEqual(outerMessage, innerMessage);
                    }
                }
            }
        }
    }
}
