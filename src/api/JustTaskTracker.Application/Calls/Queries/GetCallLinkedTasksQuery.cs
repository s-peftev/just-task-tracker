using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Mappings;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Calls.Queries;

// AD-13: linked tasks are topic references -- loaded once, when the call page opens, as full
// BoardTaskDetailsDto (same shape the board's own task-details view uses).
public record GetCallLinkedTasksQuery(Guid CallSessionId) : IRequest<Result<IReadOnlyList<BoardTaskDetailsDto>>>;

public class GetCallLinkedTasksQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    ICallRepository callRepository,
    ICallSessionLinkedTaskRepository linkedTaskRepository,
    IBoardTaskRepository boardTaskRepository,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<GetCallLinkedTasksQuery, Result<IReadOnlyList<BoardTaskDetailsDto>>>
{
    public async Task<Result<IReadOnlyList<BoardTaskDetailsDto>>> Handle(GetCallLinkedTasksQuery request, CancellationToken ct)
    {
        var callSession = await callRepository.GetByIdAsync(request.CallSessionId, ct);

        if (callSession is null)
            return Result<IReadOnlyList<BoardTaskDetailsDto>>.Failure(GeneralErrors.NotFound);

        var userRole = await boardRepository.GetUserRoleAsync(callSession.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } role || !BoardRolePermissions.CanJoinCall(role))
            return Result<IReadOnlyList<BoardTaskDetailsDto>>.Failure(GeneralErrors.Forbidden);

        var linkedTaskIds = await linkedTaskRepository.GetLinkedTaskIdsAsync(callSession.Id, ct);

        Func<UserReadModel, string?> profilePhotoUrlResolver = user =>
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(user.Id, user.ProfilePhotoVersion);

        var tasks = new List<BoardTaskDetailsDto>(linkedTaskIds.Count);

        foreach (var taskId in linkedTaskIds)
        {
            var taskDetails = await boardTaskRepository.GetBoardTaskDetailsAsync(taskId, ct);

            // The task may have been soft-deleted since linking; skip it defensively rather
            // than fail the whole list.
            if (taskDetails is null)
                continue;

            tasks.Add(taskDetails.ToDto(profilePhotoUrlResolver, role));
        }

        return Result<IReadOnlyList<BoardTaskDetailsDto>>.Success(tasks);
    }
}

public class GetCallLinkedTasksQueryValidator : AbstractValidator<GetCallLinkedTasksQuery>
{
    public GetCallLinkedTasksQueryValidator()
    {
        RuleFor(x => x.CallSessionId)
            .NotEmpty();
    }
}
