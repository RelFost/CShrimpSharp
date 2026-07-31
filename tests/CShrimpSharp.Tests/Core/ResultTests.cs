using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CShrimpSharp.Tests.Core;

[TestClass]
public sealed class ResultTests
{
    [TestMethod]
    public void Success_MapAndBind_TransformsValue()
    {
        Result<int, Failure> result = Result.Success(5)
            .Map(static value => value * 2)
            .Bind(static value => Result.Success(value + 1));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(11, result.Value);
    }

    [TestMethod]
    public void Failure_Map_DoesNotInvokeMapper()
    {
        bool invoked = false;
        var failure = new Failure("not_found", "The value was not found.");

        Result<int, Failure> result = Result.Failure<int>(failure)
            .Map(value =>
            {
                invoked = true;
                return value * 2;
            });

        Assert.IsFalse(invoked);
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(failure, result.Error);
    }

    [TestMethod]
    public void LinqQuery_ComposesSuccessfulResults()
    {
        Result<int, Failure> result =
            from left in Result.Success(2)
            from right in Result.Success(3)
            select left + right;

        Assert.AreEqual(5, result.Value);
    }

    [TestMethod]
    public void DefaultResult_ThrowsWhenAccessed()
    {
        Result<int, Failure> result = default;

        try
        {
            _ = result.Value;
            Assert.Fail("An uninitialized result must throw.");
        }
        catch (InvalidResultAccessException)
        {
        }
    }

    [TestMethod]
    public async Task MapAsync_TransformsSuccessfulValue()
    {
        Result<int, Failure> result = Result.Success(7);

        Result<int, Failure> mapped = await result.MapAsync(
            static (value, _) => ValueTask.FromResult(value * 3));

        Assert.AreEqual(21, mapped.Value);
    }
}
