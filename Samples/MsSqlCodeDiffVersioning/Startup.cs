﻿namespace WebApplication.ASPNetCore
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Swagger;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http.Features;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration
        {
            get;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion
                    (
                        CompatibilityVersion
                            .Version_2_1
                    );

            #region 异步批量入库案例专用
            var processor =
                new SingleThreadAsyncDequeueProcessorSlim<JToken>();
            var executor = new MsSqlStoreProceduresExecutor();
            processor
                .StartRunDequeueThreadProcess
                    (
                        (i, data) =>
                        {
                            //Debugger.Break();
                            var ja = new JArray(data);
                            var jo = new JObject();
                            jo["udt_vcidt"] = ja;
                            var sqlConnection = new SqlConnection("Initial Catalog=test;Data Source=localhost;User=sa;Password=!@#123QWE");
                            executor
                                .Execute
                                    (
                                        sqlConnection
                                        , "zsp_Test"
                                        , jo
                                    );
                        }
                        , null
                        , 1000
                        , 10 * 1000
                    );
            services
                .AddSingleton
                    //<SingleThreadAsyncDequeueProcessorSlim<JToken>>
                    (
                        processor
                    );
            #endregion

            services
                .AddSingleton
                    <
                        IStoreProceduresWebApiService
                        , StoreProceduresExecuteService
                    >
                    ();

            services
                .AddSingleton
                    //<
                    //     QueuedObjectsPool<Stopwatch>
                    //>
                    (
                        new QueuedObjectsPool<Stopwatch>(100, true)
                    );

            #region 跨域策略
            services
                    .Add
                        (
                            ServiceDescriptor
                                .Transient<ICorsService, WildcardCorsService>()
                        );
            services
                .AddCors
                    (
                        (options) =>
                        {
                            options
                                .AddPolicy
                                    (
                                        "SPE"
                                        , (builder) =>
                                        {
                                            builder
                                                .WithOrigins
                                                    (
                                                        "*.microshaoft.com"
                                                    );
                                        }
                                    );
                            // BEGIN02
                            options
                                .AddPolicy
                                    (
                                        "AllowAllOrigins"
                                        ,
                                        (builder) =>
                                        {
                                            builder.AllowAnyOrigin();
                                        }
                                    );

                        }
                  );
            #endregion


            services.AddResponseCaching();

            services
                .AddSwaggerGen
                    (
                        c =>
                        {
                            c
                                .SwaggerDoc
                                    (
                                        "v1"
                                        , new Info
                                        {
                                            Title = "My API"
                                                ,
                                            Version = "v1"
                                        }
                                    );
                        }
                    );

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            string timingKey = nameof(timingKey);
            app
                .UseRequestResponseGuard
                    <QueuedObjectsPool<Stopwatch>>
                        (
                            (middleware) =>
                            {
                                middleware
                                    .OnFilterProcessFunc
                                        = (injector, httpContext) =>
                                        {
                                            if (!httpContext.Items.ContainsKey(timingKey))
                                            {
                                                injector.TryGet(out var stopwatch);
                                                stopwatch.Start();
                                                httpContext.Items[timingKey] = stopwatch;
                                            }
                                            var httpRequestFeature = httpContext.Features.Get<IHttpRequestFeature>();
                                            var url = httpRequestFeature.RawTarget;
                                            var r = url.EndsWith("js");
                                            return r;
                                        };
                                middleware
                                    .OnInvokingProcessAsync
                                        = async (injector, httpContext) =>
                                        {
                                            if (!httpContext.Items.ContainsKey(timingKey))
                                            {
                                                injector.TryGet(out var stopwatch);
                                                stopwatch.Start();
                                                httpContext.Items[timingKey] = stopwatch;
                                            }
                                            var request = httpContext.Request;
                                            var httpRequestFeature = httpContext
                                                                        .Features
                                                                        .Get<IHttpRequestFeature>();
                                            var url = httpRequestFeature.RawTarget;
                                            var result = false;
                                            if
                                            (
                                                //request.ContentType == "image/jpeg"
                                                url.EndsWith("error.js")
                                            )
                                            {
                                                var response = httpContext.Response;
                                                response.StatusCode = 500;
                                                await
                                                    response
                                                        .WriteAsync
                                                                ("error");
                                                result = false;
                                            }
                                            else
                                            {
                                                result = true;
                                            }
                                            return
                                                await
                                                    Task.FromResult(result);
                                        };
                                middleware
                                    .OnResponseStartingProcess
                                        = (injector, httpContext, x) =>
                                        {
                                            var stopwatch = httpContext
                                                                .Items[timingKey] as Stopwatch;
                                            if (stopwatch != null)
                                            {
                                                stopwatch.Stop();
                                                var duration = stopwatch.ElapsedMilliseconds;
                                                httpContext
                                                        .Response
                                                        .Headers["X-Request-Response-Timing"]
                                                            = duration.ToString() + "ms";
                                                stopwatch.Reset();
                                                if (!injector.TryPut(stopwatch))
                                                {
                                                    stopwatch = null;
                                                }
                                            }
                                        };
                                middleware
                                    .OnAfterInvokedNextProcess
                                        = (injector, httpContext, x) =>
                                        {

                                        };
                                middleware
                                    .OnResponseCompletedProcess
                                        = (injector, httpContext, x) =>
                                        {
                                            if
                                                (
                                                    httpContext
                                                        .Items
                                                        .Remove(timingKey, out var removed)
                                                )
                                            {
                                                removed = null;
                                            }
                                        };
                            }
                        );
            app.UseCors();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseHsts();
            }
            //app.UseHttpsRedirection();
            app.UseMvc();
            Console.WriteLine(Directory.GetCurrentDirectory());
            app.UseDefaultFiles
                (
                    new DefaultFilesOptions()
                    {
                        DefaultFileNames =
                            {
                                "index.html"
                            }
                    }
                );
            //兼容 Linux/Windows wwwroot 路径配置
            var wwwroot = GetExistsPaths
                                (
                                    "wwwrootpaths.json"
                                    , "wwwroot"
                                )
                                .FirstOrDefault();
            if (wwwroot.IsNullOrEmptyOrWhiteSpace())
            {
                app.UseStaticFiles();
            }
            else
            {
                app
                    .UseStaticFiles
                        (
                            new StaticFileOptions()
                            {
                                FileProvider = new PhysicalFileProvider
                                                        (
                                                            wwwroot
                                                        )
                                ,
                                RequestPath = ""
                            }
                        );
            }


            app.UseSwagger();

            app
                .UseSwaggerUI
                (
                    c =>
                    {
                        c
                            .SwaggerEndpoint
                                (
                                    "/swagger/v1/swagger.json"
                                    , "My API V1"
                                );
                    }
                );



        }
        private static IEnumerable<string> GetExistsPaths(string configurationJsonFile, string sectionName)
        {
            var configurationBuilder =
                        new ConfigurationBuilder()
                                .AddJsonFile(configurationJsonFile);
            var configuration = configurationBuilder.Build();

            var executingDirectory =
                        Path
                            .GetDirectoryName
                                    (
                                        Assembly
                                            .GetExecutingAssembly()
                                            .Location
                                    );
            //executingDirectory = AppContext.BaseDirectory;
            var result =
                    configuration
                        .GetSection(sectionName)
                        .AsEnumerable()
                        .Select
                            (
                                (x) =>
                                {
                                    var r = x.Value;
                                    if (!r.IsNullOrEmptyOrWhiteSpace())
                                    {
                                        if
                                            (
                                                r.StartsWith(".")
                                            )
                                        {
                                            r = r.TrimStart('.', '\\', '/');
                                        }
                                        r = Path
                                                .Combine
                                                    (
                                                        executingDirectory
                                                        , r
                                                    );
                                    }
                                    return r;
                                }
                            )
                        .Where
                            (
                                (x) =>
                                {
                                    return
                                        (
                                            !x
                                                .IsNullOrEmptyOrWhiteSpace()
                                            &&
                                            Directory
                                                .Exists(x)
                                        );
                                }
                            );
            return result;
        }
    }
}
