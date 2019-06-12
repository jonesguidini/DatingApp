using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;

        public IConfiguration _config { get; }
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userRegisterDto)
        {

            userRegisterDto.Username = userRegisterDto.Username.ToLower();

            if (await _repo.UserExists(userRegisterDto.Username))
            {
                return BadRequest("Username already exists");
            }

            var userToCreate = new User()
            {
                Username = userRegisterDto.Username
            };

            var createdUser = await _repo.Register(userToCreate, userRegisterDto.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {

            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized();

            // cria dois claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            // cria a key a ser usada na credencial em base do token informado no arquivo appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

            // cria a credencial
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Cria estrutura do token
            var tokenDecriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), // com base nas claims já criada anteriormente
                Expires = DateTime.Now.AddDays(1), // expira em 1 dia   
                SigningCredentials = credentials // com base na credencial criada anteiormente
            };

            // cria um handler de token a ser usando para gerar o token
            var tokenHandler = new JwtSecurityTokenHandler();

            // gera o token
            var token = tokenHandler.CreateToken(tokenDecriptor);

            var user = _mapper.Map<UserForListDto>(userFromRepo);

            // retorna o tolen gerado
            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                user
            });

        }
    }
}