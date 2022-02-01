using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using SGBD_Project.Dtos;
using SGBD_Project.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SGBD_Project.Services
{
    public class UserService
    {

        private SGBDContext Context { get; }
        private IConfiguration _configuration { get; }
        public UserService(SGBDContext context, IConfiguration configuration)
        {
            Context = context;
            _configuration = configuration;
        }


        public async Task<Response<bool>> RegisterAsync(RegisterDto dto)
        {
            var isRegistered = 0;
            using (var conn = new SqlConnection((string)_configuration.GetValue(typeof(string), "ConnectionStrings")))
            {
                isRegistered = conn.Query<int>("SELECT 1 FROM USERS U WHERE U.Email = @email AND U.DeletedAt IS NOT NULL",
                    new { email = dto.Email }).FirstOrDefault();
            };

            if(isRegistered == 1)
            {
                return new Response<bool>("User already registered.", 400);
            }

            var user = new User
            {
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
            };
            Context.Users.Add(user);
            Context.SaveChanges();

            return new Response<bool>(true);
        }


        public async Task<Response<UserDto>> LoginAsync(LoginDto dto)
        {
            User existingUser;


            using (var conn = new SqlConnection((string)_configuration.GetValue(typeof(string), "ConnectionStrings")))
            {
               existingUser = conn.Query<User>(
                    "SELECT U.Id, U.Email, U.Password FROM USERS U WHERE U.Email = @email",
                    new { email = dto.Email })
                    .FirstOrDefault();
                
            };

            if (existingUser is null)
                return new Response<UserDto>("User does not exist.", 400);
            
            if(!BCrypt.Net.BCrypt.Verify(dto.Password, existingUser.Password))
                return new Response<UserDto>("Password incorrect.", 400);

            var workspaces = new List<WorkspaceDto>();
            using (var conn = new SqlConnection((string)_configuration.GetValue(typeof(string), "ConnectionStrings")))
            {
                workspaces = conn.Query<WorkspaceDto>("SELECT W.Id, W.Name FROM Workspaces W WHERE W.ID = @userId;",
                    new { userId = existingUser.Id }).ToList();

            };

            var response = new UserDto
            {
                Id = existingUser.Id,
                Email = existingUser.Email,
                Workspaces = workspaces,
                Token = GenerateJwtToken(existingUser.Id.ToString()),

            };

            return new Response<UserDto>(response);

        }

        private string GenerateJwtToken(string userName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", userName) }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }



    }
}
