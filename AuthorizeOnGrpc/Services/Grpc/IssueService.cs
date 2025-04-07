using Grpc.Core;

namespace AuthorizeOnGrpc.Services.Grpc;

public class IssueServiceGrpc : IssueService.IssueServiceBase
{
    public IssueServiceGrpc()
    {
    }

    public override Task<IssueReply> Issue(IssueRequest request, ServerCallContext context)
    {
        if (request.Name != "admin")
        {
            throw new ValidationException();
        }

        return Task.FromResult(new IssueReply
        {
            Message = "Hello " + request.Name
        });
    }
}
