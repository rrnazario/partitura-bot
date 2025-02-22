using MediatR;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Queries;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Domain.Helpers;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Commands
{
    public record GeneratePDFCommand
        : BaseBotStartsWithRequestCommand
    {
        public override string Prefix => "/pdf";
    }

    public class GeneratePDFCommandHandler : IRequestHandler<GeneratePDFCommand>
    {
        private readonly ITelegramSender _sender;
        private readonly IMediator _mediator;
        private readonly IPdfService _pdfService;
        private readonly Search _search;

        public GeneratePDFCommandHandler(
            ITelegramSender sender,
            IMediator mediator,
            IPdfService pdfService,
            ISearchAccessor searchAccessor)
        {
            _search = searchAccessor.CurrentSearch();

            if (!_search.User.IsVipValid())
            {
                throw new NotVipUserException(_search.User);
            }

            _sender = sender;
            _mediator = mediator;
            _pdfService = pdfService;
        }

        public async Task Handle(GeneratePDFCommand request, CancellationToken cancellationToken)
        {
            ClearTerm(request);

            var parts = (await _mediator.Send(new GetSheetLinksQuery(_search.Term), cancellationToken))
                .ToArray();

            if (parts.Any())
            {
                var pdfPath = await _pdfService.GenerateAsync(parts, _search.Term);

                var file = SheetSearchResult.CreateLocalFile(pdfPath);

                await _sender.SendFilesAsync([file], _search.User, false, cancellationToken: cancellationToken);
            }
            else
                await SendNotFoundMessageAsync(cancellationToken);
        }

        private void ClearTerm(GeneratePDFCommand request)
            => _search.UpdateTerm(_search.Term.Replace($"{request.Prefix}", "").Trim());

        private async Task SendNotFoundMessageAsync(CancellationToken cancellationToken)
        {
            var msg = MessageHelper.GetRandomNotFoundMessage(_search.User.fullname, _search.User.culture);

            await _sender.SendTextMessageAsync(_search.User.telegramid, msg, cancellationToken);
        }
    }
}