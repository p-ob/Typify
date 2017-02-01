namespace Typify.NET
{
    internal interface ITypeScriptDefinition
    {
        string Name { get; }

        string Namespace { get; }

        string ToTypescriptString(int startTabIndex = 0);
    }
}
