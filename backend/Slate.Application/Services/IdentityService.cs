using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;
using Slate.Application.Mappers;
using Slate.Domain.Entities;
using Slate.Domain.Repositories;

namespace Slate.Application.Services;

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
            throw new Exception($"User with username {username} already exists."); // TODO: make a custom exception
        
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
            throw new Exception("User does not exist."); // TODO: make a custom exception

        if (!passwordHasher.Verify(password, user.PasswordHash))
            throw new Exception("Incorrect username or password."); // TODO: make a custom exception
        
        var token = jwtService.GenerateToken(user.Id);
        return token;
    }

    public async Task<UserDto> GetMe(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new Exception("User does not exist.");
        return user.ToDto();
    }
}