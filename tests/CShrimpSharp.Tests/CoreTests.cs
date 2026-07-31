using CShrimpSharp.Collections;

namespace CShrimpSharp.Tests;

[TestClass]
public sealed class CoreTests
{
    [TestMethod] public void OptionDefaultIsNone() => Assert.IsTrue(default(Option<string>).IsNone);
    [TestMethod] public void ResultMapTransformsSuccess() => Assert.AreEqual(4, Result.Success(2).Map(x=>x*2).Value);
    [TestMethod] public void ValidationAccumulatesErrors() => Assert.AreEqual(2, Validation<int,string>.Invalid("a","b").Errors.Count);
    [TestMethod] public void SafeIndexReturnsNone() => Assert.IsTrue(new[] { 1 }.AtOrNone(2).IsNone);
}
