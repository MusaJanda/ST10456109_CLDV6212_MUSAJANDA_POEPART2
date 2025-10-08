using ABCRetailsFunctions.Services;

namespace ABCRetails
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure the named HttpClient "Functions"
            builder.Services.AddHttpClient("Functions", (serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                var baseUrl = config["FunctionSettings:BaseUrl"];

                // Provide a default for development if not configured
                if (string.IsNullOrEmpty(baseUrl))
                {
                    baseUrl = "http://localhost:7071/api/";
                    Console.WriteLine($"Warning: FunctionSettings:BaseUrl not configured. Using default: {baseUrl}");
                }

                // Ensure trailing slash for correct URI combination
                if (!baseUrl.EndsWith("/"))
                    baseUrl += "/";

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // Register the FunctionsApiClient
            builder.Services.AddScoped<IFunctionsApi, FunctionsApiClient>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}