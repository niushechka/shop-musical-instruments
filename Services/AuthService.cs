using course_project_shop_musical_instruments.Data;
using course_project_shop_musical_instruments.Models;

namespace course_project_shop_musical_instruments.Services;

public class AuthService
{
    private readonly UserRepository _userRepository;
    private User? _currentUser;

    public AuthService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public User? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;
    public bool IsAdmin => _currentUser?.Role == UserRole.Admin;

    public async Task<bool> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user != null && user.PasswordHash == password) // Simple password check (in production, use hashing)
        {
            _currentUser = user;
            return true;
        }
        return false;
    }

    public async Task<bool> RegisterAsync(string email, string password, string firstName, string lastName, string phone, string address)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
        {
            return false; // User already exists
        }

        var user = new User
        {
            Email = email,
            PasswordHash = password, // In production, hash the password
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            Address = address,
            Role = UserRole.Client
        };

        await _userRepository.AddAsync(user);
        _currentUser = user;
        return true;
    }

    public void Logout()
    {
        _currentUser = null;
    }

    public async Task<bool> UpdateCurrentUserAsync()
    {
        if (_currentUser == null)
        {
            return false;
        }

        var updatedUser = await _userRepository.GetByIdAsync(_currentUser.Id);
        if (updatedUser != null)
        {
            _currentUser = updatedUser;
            return true;
        }
        return false;
    }
}

