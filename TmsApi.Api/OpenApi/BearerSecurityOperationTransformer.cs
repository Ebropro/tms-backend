using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TmsApi.Api.OpenApi;

public sealed class BearerSecurityOperationTransformer   
    : IOpenApiOperationTransformer
{ 
    public Task TransformAsync( 
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = 
            context.Description.ActionDescriptor.   EndpointMetadata;

        var requiresAuthorization =
            metadata.OfType<IAuthorizeData>().Any();

        if (requiresAuthorization)
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement
                { 
                    [
                        new OpenApiSecuritySchemeReference("Bearer")
                    ] = []
                }
            ];
        }

        return Task.CompletedTask;
    }
}