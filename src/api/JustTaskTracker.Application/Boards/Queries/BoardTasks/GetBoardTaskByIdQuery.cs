using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Mappings;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries.BoardTasks;

public record GetBoardTaskByIdQuery(Guid BoardTaskId) : IRequest<Result<BoardTaskDetailsDto>>;

public class GetBoardTaskByIdQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardTaskRepository boardTaskRepository,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<GetBoardTaskByIdQuery, Result<BoardTaskDetailsDto>>
{
    public async Task<Result<BoardTaskDetailsDto>> Handle(GetBoardTaskByIdQuery request, CancellationToken ct)
    {
        var userRole = await boardTaskRepository.GetUserRoleAsync(request.BoardTaskId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanViewBoard(authorizedRole))
            return Result<BoardTaskDetailsDto>.Failure(GeneralErrors.Forbidden);

        var taskInfo = await boardTaskRepository.GetBoardTaskDetailsAsync(request.BoardTaskId, ct);

        if (taskInfo is null)
            return Result<BoardTaskDetailsDto>.Failure(GeneralErrors.NotFound);

        return Result<BoardTaskDetailsDto>.Success(taskInfo.ToDto(profilePhotoService, userRole!.Value));
    }
}
