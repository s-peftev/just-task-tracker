using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Hubs.BoardExportStatus;

public record SubscribeBoardExportStatusCommand(IReadOnlyList<Guid> BoardIds)
    : IRequest<Result<IReadOnlyList<Guid>>>;

public class SubscribeBoardExportStatusCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository)
    : IRequestHandler<SubscribeBoardExportStatusCommand, Result<IReadOnlyList<Guid>>>
{
    public async Task<Result<IReadOnlyList<Guid>>> Handle(
        SubscribeBoardExportStatusCommand request,
        CancellationToken ct)
    {
        var distinctBoardIds = SubscribeBoardExportStatusBoardIds.Normalize(request.BoardIds);

        var rolesByBoardId = await boardRepository.GetUserRolesForArchivedBoardsAsync(
            distinctBoardIds,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (rolesByBoardId.Count != distinctBoardIds.Count)
            return Result<IReadOnlyList<Guid>>.Failure(BoardsErrors.ExportStatusSubscribeNotAllowed);

        if (rolesByBoardId.Values.Any(role => !BoardRolePermissions.CanExportBoard(role)))
            return Result<IReadOnlyList<Guid>>.Failure(GeneralErrors.Forbidden);

        return Result<IReadOnlyList<Guid>>.Success(distinctBoardIds);
    }
}

public class SubscribeBoardExportStatusCommandValidator : AbstractValidator<SubscribeBoardExportStatusCommand>
{
    public SubscribeBoardExportStatusCommandValidator(ValidationSettings validationSettings)
    {
        var maxBoardCount = validationSettings.Boards!.MaxExportStatusSubscribeBoardCount;

        RuleFor(x => x.BoardIds)
            .NotNull();

        RuleFor(x => x.BoardIds!)
            .Must(ids => SubscribeBoardExportStatusBoardIds.Normalize(ids).Count > 0)
            .WithMessage("At least one valid board id is required.");

        RuleFor(x => x.BoardIds!)
            .Must(ids => SubscribeBoardExportStatusBoardIds.Normalize(ids).Count <= maxBoardCount)
            .WithMessage($"Cannot subscribe to more than {maxBoardCount} boards.");
    }
}

internal static class SubscribeBoardExportStatusBoardIds
{
    internal static List<Guid> Normalize(IReadOnlyList<Guid> boardIds) =>
        boardIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();
}
