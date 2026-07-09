using Synkan.Application.Dto.Response;
using Synkan.Application.Interfaces;
using Synkan.Application.Mappers;
using Synkan.Domain.Entities;
using Synkan.Domain.Exceptions;
using Synkan.Domain.Repositories;

namespace Synkan.Application.Services;

public class IdentityService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService
    ) : IIdentityService
{
    public async Task<string> Register(string username, string password)
    {
        var existingUser = await userRepository.GetByUsernameAsync(username);
        if (existingUser is not null)
            throw new ConflictException($"User with username {username} already exists.");
        
        var passwordHash = passwordHasher.Hash(password);
        var user = new User(username, passwordHash);
        
        await userRepository.AddAsync(user);

        var token = jwtService.GenerateToken(user.Id);
        return token;
    }

    public async Task<string> Login(string username, string password)
    {
        var user = await userRepository.GetByUsernameAsync(username);
        if (user is null)
            throw new NotFoundException("User not found.");

        if (!passwordHasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedException("Incorrect username or password.");
        
        var token = jwtService.GenerateToken(user.Id);
        return token;
    }

    public async Task<UserDto> GetMe(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("User not found.");
        return user.ToDto();
    }
}