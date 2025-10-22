namespace Nexum.Server.Infrastructures.Surreal
{
    public sealed class SurrealOptions
    {
        public string Endpoint { get; set; } = default!;
        public string Namespace { get; set; } = default!;
        public string Database { get; set; } = default!;
        public string User { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
