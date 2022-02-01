using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SGBD_Project.Dtos;
using SGBD_Project.Models;
using SGBD_Project.Services;

namespace SGBD_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AccountController : ControllerBase
    {

        private UserService _userService { get; set; }
        public AccountController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<Response<bool>> Register(RegisterDto user)
        {
            var response = await _userService.RegisterAsync(user);
            if (response.Status != 200)
            {
                Response.StatusCode = response.Status;
               
            }

            return response;

        }

        [HttpPost("login")]
        public async Task<Response<UserDto>> Login(LoginDto userLogin)
        {
            var response = await _userService.LoginAsync(userLogin);
            if (response.Status != 200)
            {
                Response.StatusCode = response.Status;

            }

            return response;
        }
        [HttpGet("test")]
        public async Task<IActionResult> ForgotPassword()
        {
            throw new NotImplementedException();
        }



    }
}
