using NextGen.src.Services.Security;
using NextGen.src.Services;
using Npgsql;
using System.Threading.Tasks;

public class UserSessionService
{
    private static UserSessionService? _instance;
    private readonly DatabaseService _databaseService;

    public static UserSessionService Instance => _instance ??= new UserSessionService(new DatabaseService());

    private UserSessionService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public UserAuthData? CurrentUser { get; private set; }

    public void SetCurrentUser(UserAuthData user)
    {
        CurrentUser = user;
    }

    public void ClearCurrentUser()
    {
        CurrentUser = null;
    }

    public async Task<bool> LoadAdditionalUserDataAsync()
    {
        if (CurrentUser != null)
        {
            using (var conn = await _databaseService.GetConnectionAsync())
            {
                using (var cmd = new NpgsqlCommand(@"
            SELECT u.user_id, u.username, u.last_login, e.employee_id, e.first_name, e.middle_name, e.last_name, e.role_id, e.email, e.phone, e.address, e.photo_url, r.role_name, r.permissions 
            FROM employees e 
            JOIN users u ON e.employee_id = u.employee_id 
            JOIN roles r ON e.role_id = r.role_id 
            WHERE u.employee_id = @employeeId", conn))
                {
                    cmd.Parameters.AddWithValue("@employeeId", CurrentUser.EmployeeId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            CurrentUser.UserId = reader.GetInt32(0);        // user_id
                            CurrentUser.Username = reader.GetString(1);      // username
                            CurrentUser.FirstName = reader.GetString(4);     // first_name
                            CurrentUser.MiddleName = reader.GetString(5);    // middle_name
                            CurrentUser.LastName = reader.GetString(6);      // last_name
                            CurrentUser.RoleName = reader.GetString(12);     // role_name
                            CurrentUser.Permissions = reader.GetString(13);  // permissions JSON
                            CurrentUser.PhotoUrl = reader.IsDBNull(11) ? null : reader.GetString(11); // photo_url
                            return true; // Данные успешно загружены
                        }
                    }
                }
            }
        }
        return false; // Данные не загружены
    }
}
