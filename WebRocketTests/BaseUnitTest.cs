using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace WebRocketTests {
  public abstract class BaseUnitTest {
    [SetUp]
    public void Setup() {
      mMocks.Clear();
    }

    [TearDown]
    public void Teardown() {
      Array.ForEach(mMocks.ToArray(), m => m.VerifyAll());
    }

    protected Mock<T> Mock<T>() where T : class {
      var mock = new Mock<T>(MockBehavior.Strict);
      mMocks.Add(mock);
      return mock;
    }

    private readonly IList<Mock> mMocks = new List<Mock>();
  }
}