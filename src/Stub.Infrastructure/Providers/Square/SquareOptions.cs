namespace Stub.Infrastructure.Providers.Square;

public class SquareOptions
{
    public const string SectionName = "Square";

    public string BaseUrl { get; set; } = "https://connect.squareupsandbox.com";
    public string AccessToken { get; set; } = string.Empty;
}
