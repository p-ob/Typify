namespace Typify.NET
{
    internal interface ITypeScriptDefinition
    {
        string Name { get; }

        string Namespace { get; }

        string ToTypeScriptString(int startTabIndex = 0);
    }
}
