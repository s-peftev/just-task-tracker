using JustTaskTracker.Application.Boards.Authorization;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries;

public record GetBoardTaskByIdQuery(Guid BoardId, Guid ColumnId, Guid BoardTaskId) : IRequest<Result<BoardTaskDetailsDto>>;

public class GetBoardTaskByIdQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IBoardTaskRepository boardTaskRepository)
    : IRequestHandler<GetBoardTaskByIdQuery, Result<BoardTaskDetailsDto>>
{
    public async Task<Result<BoardTaskDetailsDto>> Handle(GetBoardTaskByIdQuery request, CancellationToken ct)
    {
        var (boardExists, userRole) = await boardRepository.GetUserBoardRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (BoardRoleAuthorization.EnsureBoardAccess(boardExists, userRole, BoardRolePermissions.CanViewBoard) is { } failure)
            return Result<BoardTaskDetailsDto>.Failure(failure.Error);

        var task = await boardTaskRepository.GetDetailsByBoardIdAndColumnIdAndIdAsync(
            request.BoardId,
            request.ColumnId,
            request.BoardTaskId,
            ct);

        if (task is null)
            return Result<BoardTaskDetailsDto>.Failure(GeneralErrors.NotFound);

        return Result<BoardTaskDetailsDto>.Success(task);
    }
}
