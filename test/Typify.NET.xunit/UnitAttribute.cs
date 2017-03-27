namespace Typify.NET.xunit
{
    using System;
    using Xunit.Sdk;

    [TraitDiscoverer("Typify.NET.xunit.UnitDiscoverer", "Typify.NET.xunit")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class UnitAttribute : Attribute, ITraitAttribute
    {
    }
}
