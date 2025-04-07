using AuthorizeOnGrpc.Services;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Authentication;

public class ErrorHandlingInterceptor : Interceptor
{
    public ErrorHandlingInterceptor()
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
        catch (ValidationException validateException)
        {
            Console.WriteLine(validateException.Message);
            throw new RpcException(new Status(StatusCode.InvalidArgument, validateException.Message));
        }
        catch (AuthenticationFailureException authenticationFailureException)
        {
            Console.WriteLine(authenticationFailureException.Message);
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Unauthenticated."));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, "Internal.", ex));
        }
    }
}