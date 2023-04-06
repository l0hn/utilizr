using Utilizr.Util;

namespace Utilizr.Tests;

public class DummyLoadable: Loadable<DummyLoadable>
{
    public string SomeName { get; set; }
    public int SomeNumber { get; set; }
    public DateTime SomeDateTime { get; set; }
    
    protected override string CustomDeserializeStep(string source)
    {
        return source;
    }

    protected override string CustomSerializeStep(string source)
    {
        return source;
    }

    protected override string GetLoadPath()
    {
        return Path.GetFullPath(Path.Combine("UnitTestData", "Dummy.Loadable"));
    }
}