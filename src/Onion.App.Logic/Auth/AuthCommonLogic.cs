﻿using Onion.App.Data.Database.Entities;
using Onion.App.Data.Database.Repositories;
using Onion.App.Data.Security;
using Onion.Shared.Clock;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Onion.App.Logic.Auth;

public static class AuthCommonLogic
{
    public async static Task<(string accessToken, string refreshToken)> IssueAccessAsync(
        User user, 
        IClockProvider clockProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IWebTokenService tokenProvider,
        ICryptographyService cryptographyService,
        int accessTokenLifetime,
        int refreshTokenLifetime)
    {
        string accessToken = GenerateAccessToken(tokenProvider, user, accessTokenLifetime);
        var refreshToken = GenerateRefeshToken(tokenProvider, cryptographyService, clockProvider, user, refreshTokenLifetime);

        refreshToken = await refreshTokenRepository.CreateAsync(refreshToken);

        return (accessToken, refreshToken.Token);
    }

    private static string GenerateAccessToken(IWebTokenService webTokenService, User user, int tokenLifetime)
    {
        List<Claim> claims = new()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        return webTokenService.CreateWebToken(claims, tokenLifetime);
    }

    private static RefreshToken GenerateRefeshToken(
        IWebTokenService tokenProvider, 
        ICryptographyService cryptographyService,
        IClockProvider clockProvider, 
        User user, 
        int refreshTokenLifetime)
    {
        return new RefreshToken(
            cryptographyService.GetRandomString(32),
            user.Id,
            clockProvider.Now.AddMinutes(refreshTokenLifetime)
        );
    }
}
