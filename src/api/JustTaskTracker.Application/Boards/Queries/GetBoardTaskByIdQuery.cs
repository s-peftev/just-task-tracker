using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries;

public record GetBoardTaskByIdQuery(Guid BoardTaskId) : IRequest<Result<BoardTaskDetailsDto>>;

public class GetBoardTaskByIdQueryHandler(ICurrentUserAccessor currentUserAccessor, IBoardTaskRepository boardTaskRepository)
    : IRequestHandler<GetBoardTaskByIdQuery, Result<BoardTaskDetailsDto>>
{
    public async Task<Result<BoardTaskDetailsDto>> Handle(GetBoardTaskByIdQuery request, CancellationToken ct)
    {
        var userRole = await boardTaskRepository.GetUserRoleAsync(request.BoardTaskId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanViewBoard(authorizedRole))
            return Result<BoardTaskDetailsDto>.Failure(GeneralErrors.Forbidden);

        var task = await boardTaskRepository.GetBoardTaskDetailsAsync(request.BoardTaskId, ct);

        if (task is null)
            return Result<BoardTaskDetailsDto>.Failure(GeneralErrors.NotFound);

        return Result<BoardTaskDetailsDto>.Success(task with { UserRole = userRole!.Value });
    }
}
