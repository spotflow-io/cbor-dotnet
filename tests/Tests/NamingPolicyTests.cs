using Spotflow.Cbor;

namespace Tests;

[TestClass]
public class NamingPolicyTests
{
    [TestMethod]
    [DataRow("TestProperty", "testProperty")]
    [DataRow("testProperty", "testProperty")]
    [DataRow("Testproperty", "testproperty")]
    [DataRow("", "")]
    [DataRow("AAAA", "aaaa")]
    public void CamelCase_Policy_Should_Convert_As_Expected(string name, string expectedConvertedName)
    {
        var actualConvertedName = CborNamingPolicy.CamelCase.ConvertName(name);

        actualConvertedName.Should().Be(expectedConvertedName);
    }
}
