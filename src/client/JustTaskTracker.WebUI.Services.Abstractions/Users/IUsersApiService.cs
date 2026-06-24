using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Auth.Requests;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Abstractions.Users;
public interface IUsersApiService
{
    Task<PagedList<UserForBoardLookupDto>> GetUsersForBoardLookupAsync(
        Guid boardId,
        GetUsersForBoardLookupRequest request,
        CancellationToken ct = default);

    Task<ProfilePhotoDto> UploadProfilePhotoAsync(
        byte[] content,
        string fileName,
        string contentType,
        CancellationToken ct = default);

    Task DeleteProfilePhotoAsync(DeleteProfilePhotoRequest request, CancellationToken ct = default);
}
