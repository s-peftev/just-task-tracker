using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Kanban.Repositories;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using JustTaskTracker.Domain.Kanban.DTOs;
using JustTaskTracker.Domain.Kanban.Enums;
using MediatR;

namespace JustTaskTracker.Application.Kanban.Queries;

public record GetBoardByIdQuery(Guid BoardId) : IRequest<Result<BoardDetailsDto>>;

public class GetBoardByIdQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository)
    : IRequestHandler<GetBoardByIdQuery, Result<BoardDetailsDto>>
{
    public async Task<Result<BoardDetailsDto>> Handle(GetBoardByIdQuery request, CancellationToken ct)
    {
        var access = await boardRepository.GetBoardAccessAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        switch (access)
        {
            case BoardAccessStatus.NotFound:
                return Result<BoardDetailsDto>.Failure(GeneralErrors.NotFound);
            case BoardAccessStatus.Forbidden:
                return Result<BoardDetailsDto>.Failure(GeneralErrors.Forbidden);
        }

        var board = await boardRepository.GetBoardDetailsByIdAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (board is null)
            return Result<BoardDetailsDto>.Failure(GeneralErrors.NotFound);

        return Result<BoardDetailsDto>.Success(board);
    }
}
