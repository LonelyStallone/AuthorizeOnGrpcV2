using AuthorizeOnGrpc.Services;
using AuthorizeOnGrpc.Services.Grpc.Requests;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Authentication;
using System.Security.Principal;

public class KerberosInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var result = await context.GetHttpContext().AuthenticateAsync(KerberosConstans.SchemeName);

        Authenticate(result);

        var identity = context.GetHttpContext().User.Identity;

        Authorize(request, identity);

        var response = await continuation(request, context);

        return response;
    }

    private void Authenticate(AuthenticateResult? result)
    {
        if (result is null)
        {
            throw new AuthenticationFailureException("AuthenticateResult is null");
        }

        if (result.Failure is not null)
        {
            throw result.Failure;
        }

        if (result.Succeeded is false)
        {
            throw new AuthenticationFailureException("Authenticate is false");
        }
    }

    private void Authorize<TRequest>(TRequest request, IIdentity? identity)
    {
        if (identity is null)
        {
            throw new AuthenticationFailureException("Identity is null.");
        }

        if (!identity.IsAuthenticated)
        {
            throw new AuthenticationFailureException("Identity is not authenticated.");
        }

        if (request is INameHolder holder)
        {
            if (!string.Equals(holder.Name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                throw new AuthenticationFailureException($"UPN: admin, cant issue for {holder.Name}");
            }
        }
    }
}