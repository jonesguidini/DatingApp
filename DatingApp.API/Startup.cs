using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // public void ConfigureServices(IServiceCollection services)
        // {
        //     services.AddDbContext<DataContext>(x => x.UseMySql(Configuration.GetConnectionString("DefaultConnection"))
        //         // adiciona config p ignorar warnings quando app estiver rodando no terminal
        //         .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.IncludeIgnoredWarning))); 


        //     services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1) // mudado de 2.2 p 2.1
        //        .AddJsonOptions( opt => {
        //            opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; // ignora self reference **MUITO UTIL
        //        }) ; 

        //     // adiciona a classe Seed que contem alguns registros de usuários pré preenchidos para registro em desenvolvimento 
        //     services.AddTransient<Seed>();

        //     services.AddAutoMapper();

        //     // adiciona permissão para acesso via API (browser não retorna os dados sem essa permissão)
        //     services.AddCors();

        //     // adiciona mapeamento de valores da config definida em 'appsettings.json' para a classe C#
        //     services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));

        //     // adiciona mapeamento de injeção das dependencias entre interface e classes concretas
        //     services.AddScoped<IAuthRepository, AuthRepository>();
        //     services.AddScoped<IDatingRepository, DatingRepository>();

        //     // adiciona autenticação middleware (Jwt)
        //     services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //     .AddJwtBearer(options => {
        //         options.TokenValidationParameters = new TokenValidationParameters 
        //         {
        //             ValidateIssuerSigningKey = true,
        //             IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
        //                 .GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
        //             ValidateIssuer = false,
        //             ValidateAudience = false
        //         };
        //     });

        //     // adiciona registrador de log (ultima atividade)
        //     services.AddScoped<LogUserActivity>();
        // }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            // cria configuração para Identity Core
            IdentityBuilder builder = services.AddIdentityCore<User>(opt => {
                opt.Password.RequireDigit = false; // não é recomentado para versão em produção... requerer digitos na senha aumenta a segurança
                opt.Password.RequiredLength = 4;  // não é recomentado para versão em produção
                opt.Password.RequireNonAlphanumeric = false; // não é recomentado para versão em produção
                opt.Password.RequireUppercase = false; // não é recomentado para versão em produção
            });

            // adiciona aos usuários as Roles do msmo
            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
            builder.AddEntityFrameworkStores<DataContext>();
            builder.AddRoleValidator<RoleValidator<Role>>();
            builder.AddRoleManager<RoleManager<Role>>();
            builder.AddSignInManager<SignInManager<User>>();

            // adiciona autenticação middleware (Jwt)
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters 
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                        .GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // criado Policies
            services.AddAuthorization(options => {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
                options.AddPolicy("VipOnle", policy => policy.RequireRole("VIP"));
            });

            services.AddMvc(opt => 
                {
                    // a definição de OPT aqui substitui o uso de 'authorize' em todos controllers... a autorização vai ser globalmente p todos controllers
                    // ou seja, toda requisição deverá ser autenticada automaticamente
                    // no caso do AuthController foi adicionado o 'AllowAnonymous' p liberar essa autenticação

                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

                    opt.Filters.Add(new AuthorizeFilter(policy));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1) // mudado de 2.2 p 2.1
                .AddJsonOptions( opt => {
                   opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; // ignora self reference **MUITO UTIL
               }) ; 

            // adiciona a classe Seed que contem alguns registros de usuários pré preenchidos para registro em desenvolvimento 
            services.AddTransient<Seed>();

            // fix p resolver a questão do comando 'dotnet ef database drop'
            Mapper.Reset();

            services.AddAutoMapper();

            // adiciona permissão para acesso via API (browser não retorna os dados sem essa permissão)
            services.AddCors();

            // adiciona mapeamento de valores da config definida em 'appsettings.json' para a classe C#
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));

            // adiciona mapeamento de injeção das dependencias entre interface e classes concretas
            // services.AddScoped<IAuthRepository, AuthRepository>(); // não precisa mais uma vez q o Entity user core já tem os métodos necessários
            services.AddScoped<IDatingRepository, DatingRepository>();

            // adiciona registrador de log (ultima atividade)
            services.AddScoped<LogUserActivity>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                
                // configuração para facilitar captura de detalhes da exceção pela aplicação ANGULAR
                // obs, ver arquivo 'Helpers/Extentions.cs' criado com o propósito de complementar essa config
                app.UseExceptionHandler( buider => {
                    buider.Run(async context => {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var error = context.Features.Get<IExceptionHandlerFeature>();

                        if(error != null){
                            context.Response.AddApplicationError(error.Error.Message); // arquivo 'Helpers/Extentions.cs' contem esse método extensor
                            await context.Response.WriteAsync(error.Error.Message);
                        }
                    });
                });

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            seeder.SeedUsers(); // seed registros de usuários p teste em desenvolvimento

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseAuthentication();
            
            app.UseDefaultFiles(); //carrega os arquivos default como 'index', 'default' entre outros dentro da pasta 'wwwroot'
            app.UseStaticFiles(); // essa função habilita que o IIS rode a aplicação de dentro da pasta 'wwwroot'

            app.UseMvc(routes => {
                // a configuração usada aqui é para configurar a aplição disponibilizada no diretório 'wwwroot'
                // verificar arquivo 'controllers/Fallback.cs' criado com esse propósito
                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Fallback", action = "Index"}
                );
            });
        }
    }
}
