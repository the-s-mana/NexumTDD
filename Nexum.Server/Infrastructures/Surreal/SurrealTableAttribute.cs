namespace Nexum.Server.Infrastructures.Surreal
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class SurrealTableAttribute : Attribute
    {
        public string Name { get; }
        public SurrealTableAttribute(string name) => Name = name;
    }
}
