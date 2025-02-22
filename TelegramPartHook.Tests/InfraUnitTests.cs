using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Element;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Infrastructure.Helpers;
using Xunit;
using static TelegramPartHook.Domain.Constants.Enums;
using Light.GuardClauses;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.SeedWork;
using FluentAssertions;
using TelegramPartHook.Domain.Helpers;
using TelegramPartHook.Tests.Core.Helpers;

namespace TelegramPartHook.UnitTests
{
    public class InfraUnitTests
    {
        private readonly string searcherName = "Rogim Nazario";
        private readonly string notFoundSearch = Guid.NewGuid().ToString();

        private Mock<ITelegramSender> _sender;
        private IAdminConfiguration _adminConfiguration;

        public InfraUnitTests()
        {
            //EnvironmentHelper.DefineEvironmentVariables();

            _sender = new Mock<ITelegramSender>();
            _sender.Setup(s => s.SendToAdminAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<ParseMode>(), It.IsAny<InlineKeyboardMarkup>()));

            var result = 0;
            _sender.Setup(s => s.SendFilesAsync(It.IsAny<IEnumerable<SheetSearchResult>>(), It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<SheetSearchResult>, User, bool, CancellationToken>((sheets, user, _, _) =>
                {
                    result = sheets.Count();
                })
                .Returns(() => Task.FromResult(result));

            var adminMock = new Mock<IAdminConfiguration>();
            adminMock.SetupGet(s => s.AdminChatId).Returns("11112222");

            _adminConfiguration = adminMock.Object;

        }

        [Fact]
        public void TestDependencyInjection()
        {
            Action createHostBuilderAction = () => Program.CreateHostBuilder(Array.Empty<string>()).Build();
            createHostBuilderAction.Should().NotThrow<Exception>();
        }

        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("{}", true)]
        [InlineData("{'a': 'b'}", true)]
        [InlineData("{: 'b'}", false)]
        [InlineData("'message': 'please wait'", false)]
        [Theory]
        public void VerifyJsonValid(string json, bool expected)
        {
            var result = json.IsValidJson();

            result.MustBe(expected);
        }

        [Fact]
        public async Task SendDifferentFilesViaTelegram()
        {
            var files = new string[]
            {
                "SampleFiles\\SamplePDFFile.pdf",
                @"http://acervo.casadochoro.com.br/files/uploads/scores/score_8142.pdf",
                "SampleFiles\\SampleImageFile.jpeg",
                @"https://www.superpartituras.com/Content/demonstracoes/estrela-de-madureira.jpg",
                @"https://ia800501.us.archive.org/2/items/Cantorion_sheet_music_collection_3/a21fcadeea878aad90784adc2cfeb7e4.pdf#track_/download/2908/a21fcadeea878aad90784adc2cfeb7e4/F%C3%BCr%20Elise%20Guitar%20Solo%20-%20Guitar%20-%20Tony%20Wilkinson.pdf"
            };

            var results = new Dictionary<string, SendFileResult>();

            var totalSent = await _sender.Object.SendFilesAsync(files.Select(s => new SheetSearchResult(s, FileSource.Crawler)), new(_adminConfiguration.AdminChatId, "Admin", "pt-br"));

            totalSent.Should().Be(files.Length);
        }

        [Fact]
        public async Task SendNotFoundMessageViaTelegram()
        {
            await _sender.Object.SendTextMessageAsync(_adminConfiguration.AdminChatId, MessageHelper.GetRandomNotFoundMessage(searcherName, ""), CancellationToken.None);
        }

        [Fact]
        public async Task ThrowingExceptionAndSendMessageToAdmin()
        {
            await _sender.Object.SendFilesAsync([new(notFoundSearch, FileSource.Crawler)], TestHelper.AdminUser);
        }

        [Fact]
        public async Task SendSuccesMessageWithKeyboard()
        {
            var message = MessageHelper.GetRandomSuccessMessage(string.Empty);

            await _sender.Object.SendToAdminAsync(message.text, CancellationToken.None, keyboard: TelegramHelper.GenerateKeyboard(new(new[] { (message.buttonCaption, message.buttonUrl) })));
        }

        [Fact]
        public async Task SendNotfoundMessageAndKeyboard()
        {
            await _sender.Object.SendToAdminAsync(MessageHelper.GetRandomNotFoundMessage(searcherName, ""), CancellationToken.None,
                  keyboard: TelegramHelper.GenerateKeyboard(new(new[] { ("Ficar de olho pra mim", $"/monitorar {notFoundSearch}") })));
        }

        [Fact]
        public void GenerateSamplePDF()
        {
            using var writer = new PdfWriter($"C:\\temp\\{DateTime.Now:HHmmssfffff}.pdf");
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4);

            //foreach (var item in Directory.GetFiles(@"C:\Users\rrnaz\Pictures\Partituras\Pagode do Pericao 2022"))
            foreach (var item in new[] {
"https://1.bp.blogspot.com/-tQgi4DR1Q6c/YCu45HsNFII/AAAAAAAANzg/LGYB_WWC_fc2kCv-fR8hyZltYazXYCzNgCLcBGAsYHQ/w320-h313/Imaginasamba-Com-Voce-to-completo-capa.png",
"http://2.bp.blogspot.com/-nuW2E-OTuGI/Uovs3yrfUWI/AAAAAAAABdw/uI92dp0rNLg/s16000/Com+você+estou+completo.jpg"
            })
            {
                var imdata = Uri.TryCreate(item, new UriCreationOptions(), out var url)
                ? ImageDataFactory.Create(url: url)
                : ImageDataFactory.Create(filename: item);

                var ima = new PdfImageXObject(imdata);
                document.Add(new Image(ima));
            }

            document.Close();
        }
    }
}
