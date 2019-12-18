using System;
using System.Collections.Generic;
using System.Linq;
using Api.CrossCutting.DependencyInjection;
using Api.Domain.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Application {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            ConfigureService.ConfigureDependenciesService (services);
            ConfigureRepository.ConfigureDependenciesRepository (services);

            var signingConfigurations = new SigningConfigurations ();
            services.AddSingleton (signingConfigurations);

            var tokenConfigurations = new TokenConfigurations ();
            new ConfigureFromConfigurationOptions<TokenConfigurations> (Configuration.GetSection ("TokenConfigurations")).Configure (tokenConfigurations);
            services.AddSingleton (tokenConfigurations);

            services.AddAuthentication (authOptions => {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer (bearerOptions => {
                var paramnsValidation = bearerOptions.TokenValidationParameters;
                paramnsValidation.IssuerSigningKey = signingConfigurations.Key;
                paramnsValidation.ValidAudience = tokenConfigurations.Audience;
                paramnsValidation.ValidIssuer = tokenConfigurations.Issuer;
                paramnsValidation.ValidateIssuerSigningKey = true;
                paramnsValidation.ValidateLifetime = true;
                paramnsValidation.ClockSkew = TimeSpan.Zero;
            });

            services.AddAuthorization (auth => {
                auth.AddPolicy ("Bearer", new AuthorizationPolicyBuilder ()
                    .AddAuthenticationSchemes (JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser ().Build ());
            });

            services.AddSwaggerGen (c => {
                c.SwaggerDoc ("v1",
                    new OpenApiInfo {
                        Title = "My API in AspNetCore 3.0",
                            Version = "v1",
                            Description = "Learning dotnetcore 3.1",
                            Contact = new OpenApiContact {
                                Name = "Patrick Vianna",
                                    Email = "patrickviannapblv@gmail.com",
                                    Url = new Uri ("https://github.com/patrickvianna")
                            }
                    });
                c.AddSecurityDefinition ("Bearer", new OpenApiSecurityScheme {
                    In = ParameterLocation.Header,
                        Description = "Entre com o token JWT",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
                });

                c.AddSecurityRequirement (new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Id = "Bearer",
                                    Type = ReferenceType.SecurityScheme
                            }
                        }, new List<string> ()
                    }
                });
            });

            services.AddControllers ().AddNewtonsoftJson ();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }

            //Ativando middlewares para uso do Swagger
            app.UseSwagger ();
            app.UseSwaggerUI (c => {
                c.RoutePrefix = string.Empty;
                c.SwaggerEndpoint ("/swagger/v1/swagger.json", "Projeto em AspNetCore 3.1");
            });

            // Redireciona o Link para o Swagger, quando acessar a rota principal
            var option = new RewriteOptions ();
            option.AddRedirect ("^$", "swagger");
            app.UseRewriter (option);

            app.UseRouting ();
            app.UseAuthentication ();
            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });
        }
    }
}
