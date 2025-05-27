


using Auction.ApiGateway.Contracts;
using Auction.ApiGateway.Contracts.ApiGatewayContracts;

namespace Auction.ApiGateway.Extensions

{
    public class ExceptionHandlingMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<ExceptionHandlingMiddleware> _logger;

            public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task Invoke(HttpContext context)
            {
                try
                {
                    await _next(context);
                }

           
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation error occurred");

                context.Response.StatusCode = StatusCodes.Status409Conflict;
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new ApiGatewayResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "InvalidOperationException",
                        Details = ex.Message,
                        Status = 409
                    }
                });
            }
            catch (ArgumentException ex)
                {
                    _logger.LogError(ex, "Argument error occurred");

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(new ApiGatewayResponse<object>
                    {
                        Error = new ErrorDto
                        {
                            Error = "ArgumentException",
                            Details = ex.Message,
                            Status = 400
                        }
                    });
            }
            
            catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception occurred");

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new ApiGatewayResponse<object> {Error = new ErrorDto
                {
                    Error = "Internal Server Error. Please try again later.",
                    Details = ex.Message,
                    Status = 500
                    
                } });
                }
            }
        }
    }
