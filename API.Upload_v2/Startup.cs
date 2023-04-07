using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FileUploadService.Models;
using FileUploadService.Process;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FileUploadService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        //public void ConfigureServices(IServiceCollection services)
        //{
        //    services.AddControllers();
        //}
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllHeaders",
                      builder =>
                      {
                          builder.AllowAnyOrigin()
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                      });
            });

            services.AddSingleton<MetaInfo>(GetMetaFileInfo().Result);

            services.AddHttpClient("StorageAccount", (serviceProvider, client) =>
            {
                MetaInfo metaInfo = serviceProvider.GetService<MetaInfo>();
                client.BaseAddress = new Uri(metaInfo.AzureStorage.BaseUrl);
                client.DefaultRequestHeaders.Add("X-Connection-String", metaInfo.BlobInfo.Connectstring);
                client.DefaultRequestHeaders.Add("X-Storage-Type", metaInfo.BlobInfo.StorageType);
            });

            services.AddSingleton(GetVendorInformations());



            services.AddSingleton<IFileUploadService, Process.FileUploadService>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "File upload",
                    Version = "v1",
                    Description = "Allows registered applications to send files to Ingka Centers",
                });

            });
        }

        private Vendors GetVendorInformations()
        {
            using (StreamReader r = new StreamReader("VendorInfo.json"))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<Vendors>(json);
            }

        }

        private async Task<MetaInfo> GetMetaFileInfo()
        {
            var response = Configuration.GetSection("MetaInfo").Get<MetaInfo>();

            if (response != null)
            {
                response.BlobInfo.Connectstring = await LoadAccountKey(response);
            }

            return response;

        }

        private async Task<string> LoadAccountKey(MetaInfo metaInfo)
        {


#if DEBUG
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            HttpClient client = new HttpClient(clientHandler);

#else
                HttpClient client = new HttpClient();
#endif




            client.BaseAddress = new Uri(metaInfo.KeyVault.Url);
            client.DefaultRequestHeaders.Add("X-client-id", metaInfo.KeyVault.ClientId);
            client.DefaultRequestHeaders.Add("X-client-secret", metaInfo.KeyVault.ClientSecret);
            var response = await client.GetAsync(metaInfo.KeyVault.ConnectionString);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                Console.WriteLine("Invalid KeyVault Info");
                throw new Exception();
            }

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCors("AllowAllHeaders");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //swagger
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v2/swagger.json", "PlaceInfo Services"));
        }

    }
}
