using ListerBackend.Models;

namespace ListerBackend.Services;

public class UserService : IUserService
{
    // For now, create an in-memory list 
    private static List<User> _users = new()
    {
        new User { Id = 1, Name = "John Doe", Email = "johndoe@gmail.com", UserCreatedAT = DateTime.UtcNow },
        new User { Id = 2, Name = "Jane Doe", Email = "janedoe@gmail.com", UserCreatedAT = DateTime.UtcNow}
    };
    private static int _nextId = 3;

    // Get All Users Async
    public Task<List<User>> GetAllUsersAsync()
    {
        return Task.FromResult(_users);
    }

    public Task<User?>
 }