using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries.Boards;

public record GetBoardArchiveQuery(Guid BoardId) : IRequest<Result<BoardArchiveDownloadDto>>;

public class GetBoardArchiveQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IBoardExportService boardExportService,
    IBlobStorageService blobStorageService,
    BlobStorageSettings blobStorageSettings)
    : IRequestHandler<GetBoardArchiveQuery, Result<BoardArchiveDownloadDto>>
{
    private static readonly TimeSpan DownloadSasValidity = TimeSpan.FromMinutes(30);

    public async Task<Result<BoardArchiveDownloadDto>> Handle(GetBoardArchiveQuery request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanExportBoard(authorizedRole))
            return Result<BoardArchiveDownloadDto>.Failure(GeneralErrors.Forbidden);

        if (board is null)
            return Result<BoardArchiveDownloadDto>.Failure(GeneralErrors.NotFound);

        var exportInfo = await boardExportService.GetBoardExportInfoAsync(request.BoardId, ct);

        if (exportInfo is null)
            return Result<BoardArchiveDownloadDto>.Failure(GeneralErrors.NotFound);

        if (exportInfo.ExportStatus != BoardExportStatus.Completed)
            return Result<BoardArchiveDownloadDto>.Failure(BoardsErrors.ExportNotCompleted);

        var archives = blobStorageSettings.BoardArchives!;
        var blobName = archives.BuildArchiveBlobName(request.BoardId, board.Name);
        var fileName = Path.GetFileName(blobName);

        if (!await blobStorageService.ExistsAsync(archives.ContainerName, blobName, ct))
            return Result<BoardArchiveDownloadDto>.Failure(BoardsErrors.ArchiveFileNotFound);

        var expiresAtUtc = DateTime.UtcNow.Add(DownloadSasValidity);
        var downloadUrl = blobStorageService.GenerateReadSasUri(
            archives.ContainerName,
            blobName,
            DownloadSasValidity);

        return Result<BoardArchiveDownloadDto>.Success(new BoardArchiveDownloadDto(downloadUrl, expiresAtUtc, fileName));
    }
}

public class GetBoardArchiveQueryValidator : AbstractValidator<GetBoardArchiveQuery>
{
    public GetBoardArchiveQueryValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();
    }
}
