using CShrimpSharp.Collections;

namespace CShrimpSharp.Tests;

[TestClass]
public sealed class CoreTests
{
    [TestMethod]
    public void OptionDefaultIsNone()
    {
        Assert.IsTrue(default(Option<string>).IsNone);
    }

    [TestMethod]
    public void ResultMapTransformsSuccess()
    {
        Result<int, Failure> result = Result.Success(2).Map(value => value * 2);
        Assert.AreEqual(4, result.Value);
    }

    [TestMethod]
    public void ValidationAccumulatesErrors()
    {
        Validation<int, string> validation =
            Validation<int, string>.Invalid("a", "b");

        Assert.AreEqual(2, validation.Errors.Count);
    }

    [TestMethod]
    public void SafeIndexReturnsNone()
    {
        Assert.IsTrue(new[] { 1 }.AtOrNone(2).IsNone);
    }
}
