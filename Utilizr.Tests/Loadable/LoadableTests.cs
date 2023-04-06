using Utilizr.Util;

namespace Utilizr.Tests.Loadable;

public class LoadableTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void LoadBasic()
    {
        //create a dummy loadable and save it

        var name = "DumDum";
        var num = 123;
        var dt = DateTime.UtcNow;

        JsonConfig<DummyLoadable>.Instance.SomeName = name;
        JsonConfig<DummyLoadable>.Instance.SomeNumber = num;
        JsonConfig<DummyLoadable>.Instance.SomeDateTime = dt;

        JsonConfig<DummyLoadable>.Instance.Save();

        JsonConfig<DummyLoadable>.Instance.SomeName = "";
        JsonConfig<DummyLoadable>.Instance.SomeNumber = 0;
        JsonConfig<DummyLoadable>.Instance.SomeDateTime = DateTime.UtcNow.AddDays(100);
        
        JsonConfig<DummyLoadable>.Instance.Reload();

        Console.WriteLine(JsonConfig<DummyLoadable>.Instance.LoadPath);
        
        Assert.That(JsonConfig<DummyLoadable>.Instance.SomeName, Is.EqualTo(name));
        Assert.That(JsonConfig<DummyLoadable>.Instance.SomeNumber, Is.EqualTo(num));
        Assert.That(JsonConfig<DummyLoadable>.Instance.SomeDateTime, Is.EqualTo(dt));
    }
}