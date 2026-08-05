namespace Espada.Cli.Models
{
    internal sealed record CliHttpResult(int StatusCode, string Content)
    {
        public bool IsSuccess => StatusCode is >= 200 and < 300;
    }
}
