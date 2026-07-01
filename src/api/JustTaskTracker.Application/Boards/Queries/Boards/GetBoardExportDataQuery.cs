using FluentValidation;
using JustTaskTracker.Application.Boards.Mappings;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Domain.Boards.DTOs.Archiving;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries.Boards;

public record GetBoardExportDataQuery(Guid BoardId, BoardExportOptions ExportOptions) : IRequest<Result<BoardExportDataDto>>;

public class GetBoardExportDataQueryHandler(
    IBoardRepository boardRepository,
    IBlobStorageService blobStorageService,
    BlobStorageSettings blobStorageSettings)
    : IRequestHandler<GetBoardExportDataQuery, Result<BoardExportDataDto>>
{
    // SAS links are valid for the duration of one archival run;
    // 30 min covers slow attachment downloads in the Function.
    private static readonly TimeSpan AttachmentSasValidity = TimeSpan.FromMinutes(30);

    public async Task<Result<BoardExportDataDto>> Handle(
        GetBoardExportDataQuery request,
        CancellationToken ct)
    {
        var raw = await boardRepository.GetBoardExportRawDataAsync(
            request.BoardId, request.ExportOptions, ct);

        if (raw is null)
            return Result<BoardExportDataDto>.Failure(ArchivingErrors.BoardNotEligibleForExport);

        var dto = raw.ToDto(
            request.ExportOptions,
            blobStorageService,
            blobStorageSettings.TaskAttachments!.ContainerName,
            AttachmentSasValidity);

        return Result<BoardExportDataDto>.Success(dto);
    }
}

public class GetBoardExportDataQueryValidator : AbstractValidator<GetBoardExportDataQuery>
{
    public GetBoardExportDataQueryValidator()
    {
        RuleFor(x => x.BoardId).NotEmpty();
        RuleFor(x => x.ExportOptions).NotNull();
    }
}
