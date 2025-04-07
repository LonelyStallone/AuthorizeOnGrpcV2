using AuthorizeOnGrpc.Services;
using AuthorizeOnGrpc.Services.Grpc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddGrpc().AddServiceOptions<IssueServiceGrpc>(options =>
{
    options.Interceptors.Add<ErrorHandlingInterceptor>();
    options.Interceptors.Add<PkbAuditIntercaptor>();
    options.Interceptors.Add<KerberosInterceptor>();
});

builder.Services
    .AddAuthentication()
    .AddScheme<KerberosAuthenticationOptions, KerberosAuthenticationHandler>(KerberosConstans.SchemeName, null);

var app = builder.Build();


app.MapGrpcService<IssueServiceGrpc>();

app.Run();