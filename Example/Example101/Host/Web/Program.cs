using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Example001.Host.Web.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Example101.Access.Entity.Service;
using SoEx.Context;
using Example101.Common.Policy;
using Web.iFx;


namespace Example001.Host.Web
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = Example101.iFx.Hosting.Host.InProc(args);            
            //
            //  Register Context flow policy for this project
            //
            builder.Services.AddSingleton<IContextFlowPolicy, ContextFlowPolicy>();
            //
            //  Register Manager/Engine/Access dependencies
            //
            builder.Services.AddEntityAccessDependencies();                   
            //
            //  Register Filters to automatically handle lifetime and contexts 
            //
            builder.Services.AddRazorPages().AddMvcOptions( options =>{
                options.Filters.Add(new LifetimeScopePageFilter());
                options.Filters.Add(new AuthContextPageFilter());
            } );


            var connectionString = builder.Configuration.GetConnectionString("IdentityDataContextConnection") ?? throw new InvalidOperationException("Connection string 'IdentityDataContextConnection' not found.");

            builder.Services.AddDbContext<IdentityDataContext>(options => options.UseSqlite(connectionString));

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<IdentityDataContext>();

            builder.Services.AddAuthorization(options => {                        
                        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();}); 


            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            
            app.MapRazorPages();

            app.Run();
        }
    }
}