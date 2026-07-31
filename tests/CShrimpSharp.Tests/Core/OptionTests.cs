namespace CShrimpSharp.Tests.Core;

[TestClass]
public sealed class OptionTests
{
    [TestMethod] public void Default_IsNone() => Assert.IsTrue(default(Option<string>).IsNone);
    [TestMethod] public void Some_IsEqualToSameValue() => Assert.AreEqual(Option.Some(3), Option.Some(3));
    [TestMethod] public void Bind_MapsToAnotherOption() => Assert.AreEqual("3", Option.Some(3).Bind(value => Option.Some(value.ToString(System.Globalization.CultureInfo.InvariantCulture))).Value);
    [TestMethod] public void OrElse_UsesFallback() => Assert.AreEqual(5, Option.None<int>().OrElse(() => Option.Some(5)).Value);
    [TestMethod] public void ToResult_MapsNoneToFailure() => Assert.IsTrue(Option.None<int>().ToResult(() => "missing").IsFailure);
    [TestMethod] public async Task MapAsync_TransformsSome() { Option<int> result = await Option.Some(2).MapAsync(static (value, _) => ValueTask.FromResult(value * 4)); Assert.AreEqual(8, result.Value); }
}
