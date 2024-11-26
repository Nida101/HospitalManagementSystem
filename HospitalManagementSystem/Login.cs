using System;
using System.Text.Json;
using System.Linq;
using BCrypt.Net;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // Secure hashed password
    public string UserType { get; set; } = "Standard"; // Admin, Doctor, Nurse, etc.
    public bool IsEnabled { get; set; } = true; // Account active or disabled
}

public class UserService
{
    private const string FilePath = "users.json";

    private List<User> LoadUsers() // Load all users from the JSON file
    {
        if (!File.Exists(FilePath))
        {
            return new List<User>();
        }

        var json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
    }

    private void SaveUsers(List<User> users) // Save all users to the JSON file
    {
        var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

    public bool AuthenticateUser(string email, string password, out string? userType) // Authenticate a user
    {
        var users = LoadUsers();

        var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (user != null && user.IsEnabled && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            userType = user.UserType;
            return true;
        }

        userType = null;
        return false;
    }

    public void AddUser(string adminEmail, string email, string password, string userType) // Add a new user (Admin only)
    {
        var users = LoadUsers();

        if (users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase))) // Ensure unique email
        {
            throw new Exception("A user with this email already exists.");
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password); // Hash the password

        var newUser = new User // Create a new user
        {
            Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1,
            Email = email,
            PasswordHash = hashedPassword,
            UserType = userType,
            IsEnabled = true
        };

        users.Add(newUser);
        SaveUsers(users);
    }

    public void UpdateUser(string adminEmail, string targetEmail, string? newPassword = null, string? newUserType = null, bool? enableAccount = null) // Update user (Admin only)
    {
        var users = LoadUsers();

        var admin = users.FirstOrDefault(u => u.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase) && u.UserType == "Admin");
        if (admin == null)
        {
            throw new Exception("You do not have permission to update users.");
        }

        var user = users.FirstOrDefault(u => u.Email.Equals(targetEmail, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        if (enableAccount.HasValue && !enableAccount.Value && adminEmail.Equals(targetEmail, StringComparison.OrdinalIgnoreCase)) // Prevent admin from disabling their own account
        {
            throw new Exception("You cannot disable your own account.");
        }

        if (!string.IsNullOrEmpty(newPassword)) // Update password
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        }

        if (!string.IsNullOrEmpty(newUserType)) // Update user type
        {
            user.UserType = newUserType;
        }

        if (enableAccount.HasValue) // Update enable/disable status
        {
            user.IsEnabled = enableAccount.Value;
        }

        SaveUsers(users);
    }

    public List<User> ListAllUsers(string adminEmail) // List all users (Admin only)
    {
        var users = LoadUsers();

        var admin = users.FirstOrDefault(u => u.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase) && u.UserType == "Admin"); // Verify admin permissions
        if (admin == null)
        {
            throw new Exception("You do not have permission to view users.");
        }

        return users;
    }
}

