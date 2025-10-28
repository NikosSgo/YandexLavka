using WareHouse.API.Middleware;

namespace WareHouse.API.Configuration;

public static class ApiPipelineConfiguration
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WareHouse API v1");
                c.RoutePrefix = "api-docs";
            });

            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");

        // Global Exception Handling
        app.UseGlobalExceptionHandler();

        // Custom Middleware
        app.UseRequestLogging();
        app.UseCorrelationId();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        // Health Checks
        app.MapHealthChecks("/health");

        // API Controllers
        app.MapControllers();

        return app;
    }
}