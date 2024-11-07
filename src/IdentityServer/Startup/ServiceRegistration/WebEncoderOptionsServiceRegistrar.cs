using Enterprise.DI.Core.Registration.Abstract;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace IdentityServer.Startup.ServiceRegistration;

internal sealed class WebEncoderOptionsServiceRegistrar : IRegisterServices
{
    // TODO: Do we want to move this to an enterprise library, and add configuration around it?

    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.Configure<WebEncoderOptions>(options =>
        {
            // https://www.unicode.org/charts

            UnicodeRange[] unicodeRanges = [
                UnicodeRanges.BasicLatin,
                UnicodeRanges.Latin1Supplement
            ];

            // This will override the default settings, which is why we need to include the BasicLatin range (if all isn't specified).
            options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
        });
    }
}