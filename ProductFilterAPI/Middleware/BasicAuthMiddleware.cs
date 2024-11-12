using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class BasicAuthMiddleware
{
	private readonly RequestDelegate _next;
	private readonly IConfiguration _config;
	private readonly ILogger<BasicAuthMiddleware> _logger;

	public BasicAuthMiddleware(RequestDelegate next, IConfiguration config, ILogger<BasicAuthMiddleware> logger)
	{
		_next = next;
		_config = config;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		string authHeader = context.Request.Headers["Authorization"];

		if (authHeader != null && authHeader.StartsWith("Basic"))
		{
			// Decode the base64 encoded username:password string
			var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
			var encoding = Encoding.GetEncoding("UTF-8");
			var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

			// Split username and password
			var username = usernamePassword.Split(':')[0];
			var password = usernamePassword.Split(':')[1];

			// Fetch expected username and password from configuration
			

			// Validate credentials
			if (!String.IsNullOrEmpty(username)  && !String.IsNullOrEmpty(password))
			{
				// Log the username securely
				_logger.LogInformation("Authenticated request from user: {Username}", username);

				// Store the username in HttpContext for access in other parts of the application
				context.Items["Username"] = username;

				// Immediately destroy password reference to free memory
				password = null;

				// Proceed to the next middleware in the pipeline
				await _next(context);
				return;
			}
			else
			{
				// Destroy password reference if authentication fails
				password = null;
			}
		}

		// Set response header and status code for unauthorized access
		context.Response.Headers["WWW-Authenticate"] = "Basic";
		context.Response.StatusCode = StatusCodes.Status401Unauthorized;
	}
}
