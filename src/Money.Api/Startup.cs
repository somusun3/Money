﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Money.Data;
using Money.Models;
using Money.Users.Data;
using Money.Users.Models;

namespace Money
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConnectionStrings connectionStrings = Configuration
                .GetSection("ConnectionStrings")
                .Get<ConnectionStrings>();

            string ApplyBasePath(string value) => value.Replace("{BasePath}", Environment.ContentRootPath);

            connectionStrings.Application = ApplyBasePath(connectionStrings.Application);
            connectionStrings.EventSourcing = ApplyBasePath(connectionStrings.EventSourcing);
            connectionStrings.ReadModel = ApplyBasePath(connectionStrings.ReadModel);

            services
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionStrings.Application));

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    JwtOptions configuration = Configuration.GetSection("Jwt").Get<JwtOptions>();

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration.Issuer,
                        ValidAudience = configuration.Issuer,
                        IssuerSigningKey = configuration.GetSecurityKey()
                    };

                    options.SaveToken = true;
                });

            services
                .AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .Build();
                });
            
            services
                .AddIdentityCore<ApplicationUser>(options => Configuration.GetSection("Identity").GetSection("Password").Bind(options.Password))
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services
                .AddRouting(options => options.LowercaseUrls = true)
                .AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services
                .AddTransient<JwtSecurityTokenHandler>()
                .Configure<JwtOptions>(Configuration.GetSection("Jwt"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
