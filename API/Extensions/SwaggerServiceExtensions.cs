namespace API.Extensions;

public static class SwaggerServiceExtensions
{
    public static WebApplicationBuilder AddSwaggerDocumentation(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen();
        return builder;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }
}