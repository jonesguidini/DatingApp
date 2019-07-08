using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public class Seed
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        public Seed(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void SeedUsers()
        {

            if (!_userManager.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                // criando roles
                var roles = new List<Role> {
                    new Role { Name = "Member"},
                    new Role { Name = "Admin"},
                    new Role { Name = "Moderator"},
                    new Role { Name = "VIP"},
                };

                foreach (var role in roles)
                {
                    _roleManager.CreateAsync(role).Wait();
                }

                foreach (var user in users)
                {
                    // *** com o uso de 'User Identities and Roles' isso não é mais necessário
                    // byte[] passwordHash, passwordSalt;
                    // CreatePasswordHash("password", out passwordHash, out passwordSalt);

                    // user.PasswordHash = passwordHash;
                    // user.PasswordSalt = passwordSalt;
                    //user.UserName = user.UserName.ToLower();

                    _userManager.CreateAsync(user, "password").Wait();
                    _userManager.AddToRoleAsync(user, "Member").Wait();

                    // _userManager.Users.Add(user);
                }


                // cria usuário Admin
                var adminUser = new User {
                    UserName = "Admin"
                };

                IdentityResult result = _userManager.CreateAsync(adminUser, "admin").Result;

                if(result.Succeeded)
                {
                     var admin = _userManager.FindByNameAsync("Admin").Result;
                     _userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"}).Wait();
                }


                // _userManager.SaveChanges();
            }


        }

        // *** com o uso de 'User Identities and Roles' isso não é mais necessário
        // private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        // {
        //     using (var hmac = new HMACSHA512())
        //     {
        //         passwordSalt = hmac.Key;
        //         passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        //     }
        // }
    }
}