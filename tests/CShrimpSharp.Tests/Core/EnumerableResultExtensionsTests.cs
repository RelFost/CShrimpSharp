using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CShrimpSharp.Tests.Core;

[TestClass]
public sealed class EnumerableResultExtensionsTests
{
    [TestMethod]
    public void Sequence_CollectsValuesInOrder()
    {
        Result<int, Failure>[] source =
        [
            Result.Success(1),
            Result.Success(2),
            Result.Success(3),
        ];

        Result<IReadOnlyList<int>, Failure> result = source.Sequence();

        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.Value.ToArray());
    }

    [TestMethod]
    public void Traverse_StopsAtFirstFailure()
    {
        int visited = 0;

        Result<IReadOnlyList<int>, Failure> result = new[] { "1", "broken", "3" }
            .Traverse(value =>
            {
                visited++;
                return int.TryParse(value, out int parsed)
                    ? Result.Success(parsed)
                    : Result.Failure<int>(new Failure("parse", $"Cannot parse '{value}'."));
            });

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(2, visited);
        Assert.AreEqual("parse", result.Error.Code);
    }
}
