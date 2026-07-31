using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CShrimpSharp.Tests.Core;

[TestClass]
public sealed class FailureTests
{
    [TestMethod]
    public void Constructor_RejectsBlankCode()
    {
        bool thrown = false;

        try
        {
            _ = new Failure(" ", "A message.");
        }
        catch (ArgumentException)
        {
            thrown = true;
        }

        Assert.IsTrue(thrown);
    }

    [TestMethod]
    public void FromException_PreservesException()
    {
        var exception = new InvalidOperationException("broken");

        Failure failure = Failure.FromException(exception, "operation_failed");

        Assert.AreEqual("operation_failed", failure.Code);
        Assert.AreEqual("broken", failure.Message);
        Assert.AreSame(exception, failure.Exception);
    }

    [TestMethod]
    public void FromException_UsesTypeNameWhenMessageIsBlank()
    {
        var exception = new BlankMessageException();

        Failure failure = Failure.FromException(exception);

        Assert.AreEqual(nameof(BlankMessageException), failure.Message);
        Assert.AreSame(exception, failure.Exception);
    }

    private sealed class BlankMessageException : Exception
    {
        public override string Message => string.Empty;
    }
}
