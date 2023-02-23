namespace GeneratorBased.Tests;

[UsesVerify]
public class InterfaceSnapshotTests : SnapshotTestsBase<ServiceGenerator>
{
  [Fact]
  public Task PublicMethod_Void_GeneratesInterfaceMember()
  {
    var source = GetSource("public void TestMethod() {}");

    return VerifyServiceAsync(source);
  }
