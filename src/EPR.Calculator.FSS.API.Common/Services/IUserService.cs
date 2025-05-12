using EPR.Calculator.FSS.API.Common.Models;
using EPR.Calculator.FSS.API.Common.Result;

namespace BackendAccountService.Core.Services;

public interface IUserService
{
    Task<User?> GetUserByInviteAsync(string email, string inviteToken);

    Task<User?> GetUserByUserId(Guid userId);

    Task<Result<UserOrganisationsListModel>> GetUserOrganisationAsync(Guid userId);

    Task<bool> InvitationTokenExists(string inviteToken);

    Task<Result<UserOrganisation>> GetSystemUserAndOrganisationAsync(string appUser);
}