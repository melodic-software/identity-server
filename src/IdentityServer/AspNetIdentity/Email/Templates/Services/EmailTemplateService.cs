using System.Reflection;

namespace IdentityServer.AspNetIdentity.Email.Templates.Services;

// TODO: Double check compatibility when using smtp4dev locally.
// https://www.caniemail.com/

// TODO: Replace these HTML files with Razor templates.

public class EmailTemplateService
{
    private readonly ILogger _logger;

    public const string FallbackEmailConfirmationTemplate = "Please confirm your account by <a href=\"{confirmationLink}\">clicking here</a>.";
    public const string FallbackPasswordResetTemplate = "Please reset your password by <a href=\"{confirmationLink}\">clicking here</a>.";

    public EmailTemplateService(ILogger<EmailTemplateService> logger)
    {
        _logger = logger;
    }

    public string LoadEmailConfirmationTemplate()
    {
        return LoadTemplate("EmailConfirmation", FallbackEmailConfirmationTemplate);
    }

    public string LoadPasswordResetTemplate()
    {
        return LoadTemplate("PasswordReset", FallbackPasswordResetTemplate);
    }

    private string LoadTemplate(string templateFileName, string fallbackTemplate)
    {
        try
        {
            Assembly templateAssembly = AssemblyReference.Assembly;
            string assemblyName = templateAssembly.GetName().Name ?? throw new InvalidOperationException("Assembly name is null");
            string resourceName = $"{assemblyName}.AspNetIdentity.Email.Templates.{templateFileName}.html";

            string[] resourceNames = templateAssembly.GetManifestResourceNames();

            var assembly = Assembly.GetExecutingAssembly();
            using Stream? stream = templateAssembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load email template. Using fallback template.");
            return fallbackTemplate;
        }
    }
}
