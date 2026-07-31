namespace CShrimpSharp.Tests.Core;

[TestClass]
public sealed class ResultTests
{
    [TestMethod] public void Success_ExposesValue() => Assert.AreEqual(42, Result.Success(42).Value);
    [TestMethod] public void Failure_ExposesError() => Assert.AreEqual("missing", Result.Failure<int>(new Failure("missing", "Missing")).Error.Code);
    [TestMethod] public void Default_ThrowsWhenObserved() => Assert.ThrowsExactly<InvalidOperationException>(() => default(Result<int, Failure>).ToString());
    [TestMethod] public void Equality_ComparesActiveBranch() => Assert.AreEqual(Result.Success(4), Result.Success(4));
    [TestMethod] public void MapError_TransformsFailure() => Assert.AreEqual("E", Result<int, string>.Failure("e").MapError(static value => value.ToUpperInvariant()).Error);
    [TestMethod] public void Recover_ProducesSuccess() => Assert.AreEqual(7, Result<int, string>.Failure("x").Recover(static _ => 7).Value);
    [TestMethod] public void Tap_OnlyRunsForSuccess() { int observed = 0; Result.Success(3).Tap(value => observed = value); Assert.AreEqual(3, observed); }
    [TestMethod] public async Task MapAsync_TransformsSuccess() { Result<int, Failure> result = await Result.Success(2).MapAsync(static (value, _) => ValueTask.FromResult(value * 3)); Assert.AreEqual(6, result.Value); }
}
