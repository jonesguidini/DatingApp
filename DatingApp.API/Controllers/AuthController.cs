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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DatingApp.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(
        IConfiguration config, 
        IMapper mapper, 
        UserManager<User> userManager,
        SignInManager<User> signInManager)
        {
            _config = config;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userRegisterDto)
        {

            // não precisa disso mais... aspnet core identity já faz os métodos abaixos
            // userRegisterDto.Username = userRegisterDto.Username.ToLower();
            // if (await _repo.UserExists(userRegisterDto.Username))
            //     return BadRequest("Username already exists");

            var userToCreate = _mapper.Map<User>(userRegisterDto);
            
            var result = await _userManager.CreateAsync(userToCreate, userRegisterDto.Password);

            //var createdUser = await _repo.Register(userToCreate, userRegisterDto.Password);

            var userToReturn = _mapper.Map<UserForDetailedDto>(userToCreate);

            if(result.Succeeded)
            {
                return CreatedAtRoute("GetUser", new { Controller = "Users", id = userToCreate.Id }, userToReturn);
            }    

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {

            // var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            // if (userFromRepo == null)
            //     return Unauthorized();

            var user = await _userManager.FindByNameAsync(userForLoginDto.Username);

            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.Password, false);

            if(result.Succeeded){
                var appUser = await _userManager.Users.Include(p => p.Photos)
                    .FirstOrDefaultAsync(u => u.NormalizedUserName == userForLoginDto.Username.ToUpper());

                var userToReturn = _mapper.Map<UserForListDto>(appUser);

                // retorna o tolen gerado
                return Ok(new
                {
                    token = GenerateJwtToken(appUser).Result,
                    user = userToReturn
                });
            }

                return Unauthorized();
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            // cria dois claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            // pega todos roles do usuário no BD (caso ele tenha mais de um )
            var roles = await _userManager.GetRolesAsync(user);

            // adiciona clains de roles encontrado no BD para o usuario especificado
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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

            return tokenHandler.WriteToken(token);
        }
    }
}