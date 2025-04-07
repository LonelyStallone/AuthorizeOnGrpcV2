using AuthorizeOnGrpc.Services;
using AuthorizeOnGrpc.Services.Grpc.Requests;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Authentication;

public class PkbAuditIntercaptor : Interceptor
{
    public PkbAuditIntercaptor()
    {
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            var response = await continuation(request, context);

            return response;
        }
        catch (Exception exception)
        {
            if (request is INameHolder holder)
            {
                // Audit failure
                Console.WriteLine(holder.Name);
            }

            throw;
        }
    }
}