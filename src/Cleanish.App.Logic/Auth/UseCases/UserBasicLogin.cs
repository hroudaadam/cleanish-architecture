﻿using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using Cleanish.App.Data.Database.Repositories;
using Cleanish.App.Data.Security;
using Cleanish.App.Logic.Auth.Exceptions;
using Cleanish.App.Logic.Auth.Models;
using Cleanish.App.Logic.Common;
using Cleanish.Shared.Clock;
using System.Threading;
using System.Security.Claims;
using Cleanish.App.Data.Database.Entities;

namespace Cleanish.App.Logic.Auth.UseCases;

public class UserBasicLoginRequest : IRequest<AuthRes>
{
    public string Email { get; set; }
    public string Password { get; set; }
}

internal class UserBasicLoginRequestValidator : AbstractValidator<UserBasicLoginRequest>
{
    public UserBasicLoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.Password)
            .MinimumLength(5)
            .NotEmpty();
    }
}

internal class UserBasicLoginHandler : IRequestHandler<UserBasicLoginRequest, AuthRes>
{
    private readonly IUserRepository _userRepository;
    private readonly ICryptographyService _cryptographyService;
    private readonly IWebTokenService _webTokenService;
    private readonly ApplicationSettings _applicationSettings;

    public UserBasicLoginHandler(
        IUserRepository userRepository,
        ICryptographyService cryptographyService,
        IWebTokenService webTokenService,
        IOptions<ApplicationSettings> applicationSettings)
    {
        _userRepository = userRepository;
        _cryptographyService = cryptographyService;
        _webTokenService = webTokenService;
        _applicationSettings = applicationSettings.Value;
    }

    public async Task<AuthRes> Handle(UserBasicLoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !_cryptographyService.VerifyStringHash(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new InvalidEmailPasswordException();
        }

        string accessToken = GenerateAccessToken(user);

        return user.Adapt<AuthRes>(a =>
        {
            a.AccessToken = accessToken;
        });
    }

    private string GenerateAccessToken(User user)
    {
        List<Claim> claims = new()
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("role", user.Role.ToString())
        };
        return _webTokenService.CreateWebToken(claims, _applicationSettings.AccessTokenLifetime);
    }
}