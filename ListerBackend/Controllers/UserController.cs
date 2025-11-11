using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMongoCollection<User> _users;
    private readonly JWTService _jwtService;

    public UserController(IMongoDatabase database, JWTService jwtService)
    {
        _users = database.GetCollection<User>("Users");
        _jwtService = jwtService;
    }

    // /api/user/loggedin
    [Authorize]
    [HttpGet("loggedin")]
    public async Task<IActionResult> GetLoggedIn()
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var loggedInUser = await _users.Find(filter).FirstOrDefaultAsync();

            if (loggedInUser == null)
            {
                return NotFound(new { errorMessage = "User Not Found" });
            }

            return Ok(new
            {
                loggedIn = true,
                user = new
                {
                    firstName = loggedInUser.FirstName,
                    lastName = loggedInUser.LastName,
                    email = loggedInUser.Email
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500);
        }
    }

    // /api/user/register
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
    {
        try
        {
            // Validate all fields are provided
            if (string.IsNullOrEmpty(request.FirstName) ||
                string.IsNullOrEmpty(request.LastName) ||
                string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.Password) ||
                string.IsNullOrEmpty(request.PasswordVerify))
            {
                return BadRequest(new { errorMessage = "Please enter all required fields." });
            }

            // Check password length
            if (request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    errorMessage = "Please enter a password of at least 8 characters."
                });
            }

            // Check passwords match
            if (request.Password != request.PasswordVerify)
            {
                return BadRequest(new
                {
                    errorMessage = "Please enter the same password twice."
                });
            }

            // Check if user already exists
            var filter = Builders<User>.Filter.Eq(u => u.Email, request.Email);
            var existingUser = await _users.Find(filter).FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage = "An account with this email address already exists."
                });
            }

            // Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new user
            var newUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = passwordHash,
            };

            await _users.InsertOneAsync(newUser);

            // Generate token and set cookie
            var token = _jwtService.GenerateToken(newUser.Id);

            Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                success = true,
                user = new
                {
                    firstName = newUser.FirstName,
                    lastName = newUser.LastName,
                    email = newUser.Email
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500);
        }
    }
    
    // /api/user/login
    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginRequest request)
    {
        try
        {
            // Validate email
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { errorMessage = "Please enter email." });
            }

            // Validate password
            if (string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { errorMessage = "Please enter password." });
            }

            // Find user by email
            var filter = Builders<User>.Filter.Eq(u => u.Email, request.Email);
            var existingUser = await _users.Find(filter).FirstOrDefaultAsync();

            if (existingUser == null)
            {
                return BadRequest(new 
                { 
                    errorMessage = $"Email address {request.Email} does not exists." 
                });
            }

            // Verify password
            var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, existingUser.PasswordHash);
            
            if (!isPasswordCorrect)
            {
                return BadRequest(new { errorMessage = "Incorrect password." });
            }

            // Generate token and set cookie
            var token = _jwtService.GenerateToken(existingUser.Id);
            
            Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                success = true,
                user = new
                {
                    firstName = existingUser.FirstName,
                    lastName = existingUser.LastName,
                    email = existingUser.Email
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500);
        }
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    public IActionResult LogoutUser()
    {
        Response.Cookies.Append("token", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(-1) 
        });

        return Ok(new { success = true });
    }
}