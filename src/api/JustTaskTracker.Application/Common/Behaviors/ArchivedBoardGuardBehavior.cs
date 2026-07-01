using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using MediatR;

namespace JustTaskTracker.Application.Common.Behaviors;

/// <summary>
/// Marks a MediatR request that targets a board which must not be archived.
/// </summary>
public interface IRequireActiveBoard
{
    Guid BoardId { get; }
}

/// <summary>
/// MediatR pipeline behavior that blocks mutations against archived boards before the handler runs.
/// Applies only to requests implementing <see cref="IRequireActiveBoard"/>.
/// </summary>
public class ArchivedBoardGuardBehavior<TRequest, TResponse>(IBoardRepository boardRepository)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not IRequireActiveBoard activeBoardRequest)
            return await next(ct);

        if (!await boardRepository.IsArchivedAsync(activeBoardRequest.BoardId, ct))
            return await next(ct);

        return ResultResponseFactory.CreateFailure<TResponse>(BoardsErrors.Archived);
    }
}
