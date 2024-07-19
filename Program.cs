var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

IronOcr.License.LicenseKey = "IRONSUITE.HUNGHV.BIZSYS.VN.5400-5392C5AA3F-EAJJOIFOGN35PU-3L5DWP5YUJTC-OJ4JTECUO3XQ-IZC657H6LISI-2DI3PJ6FWSV3-B57AYDUWI2XR-XCFZTP-TIVM3P7VJNCNEA-DEPLOYMENT.TRIAL-BDXZGH.TRIAL.EXPIRES.17.AUG.2024";

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
