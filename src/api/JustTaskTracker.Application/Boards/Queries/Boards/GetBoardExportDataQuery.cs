using FluentValidation;
using JustTaskTracker.Application.Boards.Mappings;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Boards.DTOs.Archiving;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries.Boards;

public record GetBoardExportDataQuery(Guid BoardId, BoardExportOptions ExportOptions) : IRequest<Result<BoardExportDataDto>>;

public class GetBoardExportDataQueryHandler(IBoardRepository boardRepository)
    : IRequestHandler<GetBoardExportDataQuery, Result<BoardExportDataDto>>
{
    public async Task<Result<BoardExportDataDto>> Handle(
        GetBoardExportDataQuery request,
        CancellationToken ct)
    {
        var raw = await boardRepository.GetBoardExportRawDataAsync(
            request.BoardId, request.ExportOptions, ct);

        if (raw is null)
            return Result<BoardExportDataDto>.Failure(ArchivingErrors.BoardNotEligibleForExport);

        return Result<BoardExportDataDto>.Success(raw.ToDto(request.ExportOptions));
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
