using AutoMapper;
using eTMS.API.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using FileManagementAPI.API.Infrastructure;
using eTMS.API.Infrastructure.Middlewares;
using System.Reflection;
using System.IO;
using eTMS.API.Service.Infrastructure;
using eTMS.IdentityServer.Service.Models;
using Microsoft.Extensions.FileProviders;
using FileManagement.DL.IService;
using FileManagement.DL.Services;
using Amazon.S3;

namespace FileManagementAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfigureSetting(Configuration)
                .AddCustomMvc()
                .AddCustomDbContext(Configuration)
                .AddCustomSwagger(Configuration)
                .AddCustomIntegrations(Configuration)
                //.AddCustomAuthentication(Configuration)
                //.AddCustomCulture(Configuration)
                .AddInfrastructure<Startup>(Configuration);

            //16/08/2021
            services.AddSession();
            //services.AddSingleton<IAWSS3Service, AWSS3Service>();
            //services.AddSingleton<ICsDocumentService, CsDocumentService>();
            services.AddAWSService<IAmazonS3>();
        }
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory,
            IHostingEnvironment env, IApiVersionDescriptionProvider provider)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/plain";
                        var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (errorFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exception logger");
                            logger.LogError(500, errorFeature.Error, errorFeature.Error.Message);
                        }

                        await context.Response.WriteAsync("There was an error");
                    });
                });
            }


            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                string swaggerJsonBasePath = string.IsNullOrWhiteSpace(options.RoutePrefix) ? "." : "..";
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"{swaggerJsonBasePath}/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
            });

            //app.UseCors("AllowAllOrigins");
            app.UseCors("CorsPolicy");
            //ConfigureAuth(app);

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            app.UseAuthentication();
            app.UseSession();
            app.UseMvc();

            app.UseStaticFiles(); // For the wwwroot folder
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserGuide")),
                RequestPath = "/UserGuide"
            });
        }
        //protected virtual void ConfigureAuth(IApplicationBuilder app)
        //{
        //    if (Configuration.GetValue<bool>("UseLoadTest"))
        //    {
        //        app.UseMiddleware<ByPassAuthMiddleware>();
        //    }

        //    app.UseAuthentication();
        //}
    }
    static class CustomExtensionsMethods
    {
        public static IServiceCollection AddCustomMvc(this IServiceCollection services)
        {
            // Add framework services.
            //services.AddMvc(options =>
            //{
            //    options.Filters.Add(typeof(HttpGlobalExceptionFilter));
            //}).AddControllersAsServices();  //Injecting Controllers themselves thru DI
            //                                //For further info see: http://docs.autofac.org/en/latest/integration/aspnetcore.html#controllers-as-services

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowAllOrigins",
            //        builder =>
            //        {
            //            builder
            //                .WithHeaders("accept", "content-type", "origin", "x-custom-header")
            //                .AllowAnyOrigin()
            //                .AllowAnyHeader()
            //                .AllowAnyMethod();
            //        });
            //});


            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(implementationFactory =>
            {
                var actionContext = implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });
            // Lỗi không hiển thị hết data vì có quan hệ các bảng khác
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            services.AddVersionedApiExplorer(o => o.GroupNameFormat = "'v'VVV");
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                        options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
                    });
            services.AddApiVersioning(config =>
            {
                config.ReportApiVersions = true;
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });
            services.AddMvc().AddDataAnnotationsLocalization();
            return services;
        }

        public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDbContext<DNTDataContext>(options => options.UseSqlServer(configuration["ConnectStrings:Default"]));
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<eTMSDataContextDefault>(options =>
                {
                    options.UseSqlServer(configuration["ConnectionStrings:eTMSConnection"],
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            //sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                            sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                        });
                },
                ServiceLifetime.Scoped  //Showing explicitly that the DbContext is shared across the HTTP request scope (graph of objects started in the HTTP request)
                );
            //services.AddDistributedRedisCache(options =>
            //{
            //    options.InstanceName = "eTMS";
            //    options.Configuration = configuration.GetConnectionString("RedisConnection");
            //});
            return services;
        }

        public static IServiceCollection AddCustomSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(options =>
                {
                    options.DescribeAllEnumsAsStrings();

                    var provider = services.BuildServiceProvider()
                                           .GetRequiredService<IApiVersionDescriptionProvider>();

                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                    options.IncludeXmlComments(xmlPath);

                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(
                            description.GroupName,
                            new Info()
                            {
                                Title = $"System Management API {description.ApiVersion}",
                                Version = description.ApiVersion.ToString(),
                                Description = "This API provides functions processing for system service",
                                Contact = new Contact()
                                {
                                    Name = "LogTecHub",
                                    Email = @"it-app@itlvn.com",
                                    Url = @"https://logtechub.com/vn/gioi-thieu.html"
                                },
                                License = new License()
                                {
                                    Name = "LogTecHub 1.0",
                                    Url = @"https://logtechub.com"
                                },
                                TermsOfService = @"https://logtechub.com/vn/gioi-thieu.html"
                            });
                    }

                    var security = new Dictionary<string, IEnumerable<string>>
                                        {
                                            {"Bearer", new string[] { }},
                                        };

                    options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = "header",
                        Type = "apiKey"
                    });

                    options.AddSecurityRequirement(security);

                    //options.OperationFilter<AuthorizeCheckOperationFilter>();
                });
            return services;
        }

        public static IServiceCollection AddCustomIntegrations(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper();
            ServiceRegister.Register(services, configuration);
            services.AddOptions();
            return services;
        }
        //public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        //{
        //    services.AddUserManager();

        //    services.AddMvc(config =>
        //    {
        //        var policy = new AuthorizationPolicyBuilder()
        //        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        //        .RequireAuthenticatedUser()
        //        .Build();
        //        config.Filters.Add(new AuthorizeFilter(policy));
        //    });

        //    services.AddMvcCore().AddAuthorization();

        //    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        //    services
        //        .AddAuthentication(options =>
        //        {
        //            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //            //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        //        })
        //        .AddJwtBearer(options =>
        //        {
        //            options.Authority = configuration["Authentication:Authority"];
        //            options.RequireHttpsMetadata = bool.Parse(configuration["Authentication:RequireHttpsMetadata"]);
        //            options.Audience = configuration["Authentication:ApiName"];
        //            options.SaveToken = true;
        //            options.Events = new JwtBearerEvents()
        //            {
        //                OnTokenValidated = async context =>
        //                {
        //                    String userID = context.Principal.FindFirstValue("user_id");
        //                    Guid workplaceID = Guid.Parse(context.Principal.FindFirstValue("current_workplace_id"));
        //                    var vPermissionEmail = context.HttpContext.RequestServices.GetService<IViewPermissionEmailService>();

        //                    var lstPermission = await vPermissionEmail.GetPermissionAsync(userID, workplaceID);

        //                    if (lstPermission.Any())
        //                    {
        //                        List<Claim> lstClaim = new List<Claim>();
        //                        lstPermission.ForEach(p => lstClaim.Add(new Claim(JwtClaimTypes.Role, p)));
        //                        context.Principal.AddIdentity(new ClaimsIdentity(lstClaim, JwtBearerDefaults.AuthenticationScheme, "name", "role"));
        //                    }
        //                }
        //            };
        //            options.BackchannelHttpHandler = new HttpClientHandler()
        //            {
        //                // CAUTION USING THIS !!!
        //                ServerCertificateCustomValidationCallback =
        //                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        //            };
        //        });
        //    return services;
        //}

        //public static IServiceCollection AddCustomCulture(this IServiceCollection services, IConfiguration configuration)
        //{
        //    services.AddJsonLocalization(opts => opts.ResourcesPath = configuration["LANGUAGE_PATH"]);

        //    //Multiple language setting
        //    var supportedCultures = new[]
        //    {
        //        new CultureInfo("en-US"),
        //        new CultureInfo("vi-VN")
        //    };

        //    var localizationOptions = new RequestLocalizationOptions()
        //    {
        //        DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US"),
        //        SupportedCultures = supportedCultures,
        //        SupportedUICultures = supportedCultures
        //    };

        //    localizationOptions.RequestCultureProviders = new[]
        //    {
        //        new RouteDataRequestCultureProvider()
        //        {
        //            RouteDataStringKey = "lang",
        //            Options = localizationOptions
        //        }
        //    };
        //    services.AddSingleton(localizationOptions);
        //    return services;
        //}
    }
}
