namespace Typify
{
    internal interface ITypeScriptDefinition
    {
        string Name { get; }

        string Namespace { get; set; }

        string ToTypescriptString(int startTabIndex = 0);
    }
}
