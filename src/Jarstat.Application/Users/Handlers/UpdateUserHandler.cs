using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Jarstat.Application.Handlers;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result<User?>>
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserHandler(
        UserManager<User> userManager,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<User?>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user is null)
            return Result<User?>.Failure(DomainErrors.EntryNotFound
                .WithParameters(nameof(request.Id), typeof(Guid).ToString(), request.Id.ToString()));

        var lastUpdater = await _userManager.FindByIdAsync(request.LastUpdaterId.ToString());
        if (lastUpdater is null)
            return Result<User?>.Failure(DomainErrors.EntryNotFound
                .WithParameters(nameof(request.LastUpdaterId), typeof(Guid).ToString(), request.LastUpdaterId.ToString()));

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetPasswordResult = await _userManager.ResetPasswordAsync(user, resetToken, request.Password);
        if (!resetPasswordResult.Succeeded)
        {
            var errors = resetPasswordResult.Errors.Select(e => e.Description).ToArray();
            return Result<User?>.Failure(DomainErrors.Identity
                .WithParameters(errors));
        }

        user.Update(lastUpdater);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<User?>.Failure(DomainErrors.Exception
                .WithParameters(ex.InnerException?.Message!));
        }

        return user;
    }
}
