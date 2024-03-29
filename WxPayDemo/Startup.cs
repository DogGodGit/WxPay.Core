﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.IO;
using System.Reflection;
using WxPayAPI;

namespace WxPayDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            WxPayConfig.config = new DemoConfig(configuration);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // 注册Swagger生成器，定义一个和多个Swagger 文档
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "WxPayDemo Core API",
                    Version = "v1"
                });

                // 为 Swagger JSON and UI设置xml文档注释路径
                var basePath = Directory.GetCurrentDirectory() + @"\XML";
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(basePath, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // 过滤方法
                options.IgnoreObsoleteActions();

                // 过滤属性
                options.IgnoreObsoleteProperties();
            });

            // 请求上下文
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //允许一个或多个具体来源:
            services.AddCors(options =>
            {
                options.AddPolicy("AllowedCors", policy =>
                {
                    // 設定允許跨域的來源，有多個的話可以用 `,` 隔開
                    policy.WithOrigins("*")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // 异常记录
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex != null)
                    {
                        //记录日志
                        //Logger.Error(ex.Error.Message, ex.Error);
                    }

                    await context.Response.WriteAsync(ex?.Error?.Message ?? "an error occure");
                });
            });

            #region Swagger

            //启用中间件服务生成Swagger作为JSON终结点
            app.UseSwagger();

            //启用中间件服务对swagger-ui，指定Swagger JSON终结点s
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "docs";
                c.DefaultModelsExpandDepth(-1);
                c.DocExpansion(DocExpansion.None);
                c.EnableDeepLinking();
                c.ShowExtensions();
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WxPayDemo API V1");
                c.RoutePrefix = string.Empty;
            });

            #endregion Swagger

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}