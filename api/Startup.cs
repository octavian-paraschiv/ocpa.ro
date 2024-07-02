using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Helpers.Authentication;
using ocpa.ro.api.Helpers.Content;
using ocpa.ro.api.Helpers.Generic;
using ocpa.ro.api.Helpers.Medical;
using ocpa.ro.api.Helpers.Meteo;
using ocpa.ro.api.Helpers.Wiki;
using ocpa.ro.api.Models.Configuration;
using ocpa.ro.api.Policies;

namespace ocpa.ro.api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Configuration.ResolveConfiguration(services, JwtConfig.SectionName, out JwtConfig jwtConfig);
            Configuration.ResolveConfiguration(services, AuthConfig.SectionName, out AuthConfig _);

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy => policy
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(url => true)
                    .AllowCredentials());
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.SaveToken = true;

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(JwtConfig.KeyBytes),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };
            });

            services.AddAuthorization();

            services.AddSingleton<IAuthHelper, AuthHelper>();
            services.AddSingleton<IMeteoDataHelper, MeteoDataHelper>();
            services.AddSingleton<IMedicalDataHelper, MedicalDataHelper>();
            services.AddSingleton<IAuthorizationHandler, AuthorizePolicy>();
            services.AddSingleton<IContentHelper, ContentHelper>();

            services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();

            services.AddTransient<IMultipartRequestHelper, MultipartRequestHelper>();
            services.AddTransient<IWikiHelper, WikiHelper>();


            services
                .AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.IgnoreNullValues = true);

            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Backend API for ocpa.ro",
                        Version = "v1"
                    });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

#if DEBUG
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(url => true)
                .AllowCredentials());
#else
            app.UseCors();
#endif

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}
