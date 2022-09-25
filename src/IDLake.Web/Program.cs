using IDLake.Web.Data;
using MudBlazor.Services;
using Blazored.LocalStorage;
using Blazored.Toast;
using IDLake.Tools;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text;
using IDLake.Web.Helpers;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using IDLake.Web.Data;
using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//end swagger

// ******
// BLAZOR COOKIE Auth Code (begin)
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
// BLAZOR COOKIE Auth Code (end)
// ******
// ******
// BLAZOR COOKIE Auth Code (begin)
// From: https://github.com/aspnet/Blazor/issues/1554
// HttpContextAccessor
//Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


// Add services to the container.

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpContextAccessor>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddTransient<AzureBlobHelper>();
builder.Services.AddSingleton<UserProfileService>();
builder.Services.AddTransient<InfoDatasetService>();
builder.Services.AddTransient<UserProfileService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin().AllowAnyHeader().WithMethods("GET, PATCH, DELETE, PUT, POST, OPTIONS"));
});
var configBuilder = new ConfigurationBuilder()
  .SetBasePath(Directory.GetCurrentDirectory())
  .AddJsonFile("appsettings.json", optional: false);
IConfiguration Configuration = configBuilder.Build();

AppConstants.SQLConn = Configuration["ConnectionStrings:SqlConn"];
AppConstants.RedisCon = Configuration["RedisCon"];
AppConstants.BlobConn = Configuration["ConnectionStrings:BlobConn"];
AppConstants.GMapApiKey = Configuration["GmapKey"];

AppConstants.LaporanStatistikUrl = Configuration["Reports:LaporanStatistikUrl"];

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredToast();

MailService.MailUser = Configuration["MailSettings:MailUser"];
MailService.MailPassword = Configuration["MailSettings:MailPassword"];
MailService.MailServer = Configuration["MailSettings:MailServer"];
MailService.MailPort = int.Parse(Configuration["MailSettings:MailPort"]);
MailService.SetTemplate(Configuration["MailSettings:TemplatePath"]);
MailService.SendGridKey = Configuration["MailSettings:SendGridKey"];
MailService.UseSendGrid = true;


SmsService.UserKey = Configuration["SmsSettings:ZenzivaUserKey"];
SmsService.PassKey = Configuration["SmsSettings:ZenzivaPassKey"];
SmsService.TokenKey = Configuration["SmsSettings:TokenKey"];
AppConstants.UploadUrlPrefix = Configuration["UploadUrlPrefix"];


builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.KnownProxies.Add(IPAddress.Parse("103.189.234.66"));
});

builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.MaximumReceiveMessageSize = 128 * 1024; // 1MB
});


var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    StaticWebAssetsLoader.UseStaticWebAssets(
              app.Environment,
              app.Configuration);
   
}
//swagger
app.UseSwagger();
app.UseSwaggerUI();
//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// ******
// BLAZOR COOKIE Auth Code (begin)
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
// BLAZOR COOKIE Auth Code (end)
// ******

app.UseCors(x => x
.AllowAnyMethod()
.AllowAnyHeader()
.SetIsOriginAllowed(origin => true) // allow any origin  
.AllowCredentials());               // allow credentials 

// BLAZOR COOKIE Auth Code (begin)
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
// BLAZOR COOKIE Auth Code (end)

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

var db = new IDLakeDB();
db.Database.EnsureCreated();
//inference API
app.MapGet("/viewdataset", async (string uid) =>
{
    try
    {
        InfoDatasetService svc = new();
        var item = svc.GetDataByUid(uid);
        if (item !=null && !string.IsNullOrEmpty(item.DatasetPath))
        {
            var TableDataSet = DataConverter.ConvertCSVtoDataTable(item.DatasetPath);
            var ds = TableDataSet.ToDynamic();
            return Results.Ok(ds);
        }
        return Results.NoContent();
        
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.ToString());
    }
}).WithName("ViewDataset");



app.Run();



public static class DataTableExtension
{
    public static List<dynamic> ToDynamic(this DataTable dt)
    {
        var dynamicDt = new List<dynamic>();
        foreach (DataRow row in dt.Rows)
        {
            dynamic dyn = new ExpandoObject();
            dynamicDt.Add(dyn);
            //--------- change from here
            foreach (DataColumn column in dt.Columns)
            {
                var dic = (IDictionary<string, object>)dyn;
                dic[column.ColumnName] = row[column];
            }
            //--------- change up to here
        }
        return dynamicDt;
    }
    public static IEnumerable<dynamic> AsDynamicEnumerable(this DataTable table)
    {
        // Validate argument here..

        return table.AsEnumerable().Select(row => new DynamicRow(row));
    }

    private sealed class DynamicRow : DynamicObject
    {
        private readonly DataRow _row;

        internal DynamicRow(DataRow row) { _row = row; }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var retVal = _row.Table.Columns.Contains(binder.Name);
            result = retVal ? _row[binder.Name] : null;
            return retVal;
        }
    }
}
