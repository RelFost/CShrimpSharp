using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CShrimpSharp.Tests.Core;

[TestClass]
public sealed class OptionTests
{
    [TestMethod]
    public void Some_MapAndFilter_KeepsMatchingValue()
    {
        Option<int> option = Option.Some(10)
            .Map(static value => value + 5)
            .Filter(static value => value > 10);

        Assert.IsTrue(option.HasValue);
        Assert.AreEqual(15, option.Value);
    }

    [TestMethod]
    public void None_ToResult_CreatesFailure()
    {
        Result<int, Failure> result = Option.None<int>().ToResult(
            static () => new Failure("missing", "No value was supplied."));

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual("missing", result.Error.Code);
    }

    [TestMethod]
    public void FromNull_CreatesNone()
    {
        Option<string> option = Option.From<string>(null);
        Assert.IsTrue(option.IsNone);
    }
}
