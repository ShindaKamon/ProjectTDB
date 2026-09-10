using NUnit.Framework;

namespace ProjectTDB.Tests
{
    public class ValidationResultTests
    {
        [Test]
        public void Success_IsValid_WithEmptyMessage()
        {
            var result = ValidationResult.Success();

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(string.Empty, result.ErrorMessage);
        }

        [Test]
        public void Fail_IsNotValid_AndKeepsMessage()
        {
            var result = ValidationResult.Fail("boom");

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("boom", result.ErrorMessage);
        }

        [Test]
        public void Generic_Success_CarriesValue()
        {
            var result = ValidationResult<int>.Success(42);

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(42, result.Value);
        }

        [Test]
        public void Generic_Fail_HasDefaultValue_AndMessage()
        {
            var result = ValidationResult<string>.Fail("nope");

            Assert.IsFalse(result.IsValid);
            Assert.IsNull(result.Value);
            Assert.AreEqual("nope", result.ErrorMessage);
        }
    }
}
