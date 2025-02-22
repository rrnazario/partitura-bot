using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using TelegramPartHook.Application.Services;

namespace TelegramPartHook.Application.Commands.Contribution;

public record ContributeCommand : IRequest
{
    public IEnumerable<ContributionRequest> Contributions { get; }
    public string? User { get; }

    private static JsonSerializerOptions JsonSerializerOptions => new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ContributeCommand(IFormCollection requestForm)
    {
        var contributionKeys = requestForm.Keys.Where(k => k.StartsWith("contribution_")).ToArray();
        var valid = contributionKeys.Length > 0;

        if (!valid) throw new ArgumentException("Contributions array empty.");

        Contributions = contributionKeys.Select(s =>
            JsonSerializer.Deserialize<ContributionRequest>(requestForm[s]!, JsonSerializerOptions)!);
        
        User = requestForm["user"];
    }
}

public record ContributionRequest
{
    public string Name { get; set; }
    public string Content { get; set; }
}

public record Contribution : ContributionRequest
{
    public ObjectId Id { get; set; }

    public const string CollectionName = "contributions";
    
    public string GetNameWithExtension() => $"{Name}.{GetExtension()}";
    public string GetExtension() => Content.Split(":")[1].Split(";")[0].Split("/")[1];
}

public record ContributeCommandHandler(
    IMongoDatabase Database,
    ITelegramSender Sender)
    : IRequestHandler<ContributeCommand>
{

    public async Task Handle(ContributeCommand request, CancellationToken cancellationToken)
    {
        var contributions = Database.GetCollection<Contribution>(Contribution.CollectionName);
        await contributions.InsertManyAsync(request.Contributions.Select(s => new Contribution
        {
            Content = s.Content,
            Name = s.Name
        }), cancellationToken: cancellationToken);
        
        await Sender.SendToAdminAsync($"Contribuição enviada pelo usuário '{request.User ?? "desconhecido"}'. Envie /contributions para revisar", cancellationToken);
    }
}