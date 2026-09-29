using NUnit.Framework;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Gains de PA : plafonnés au maximum, sauf gain explicite au-delà (ex: Sang pour sang).
    /// </summary>
    public class ActionPointsComponentTests
    {
        [Test]
        public void AddPA_IsCappedAtMaxByDefault()
        {
            var pa = new ActionPointsComponent(5, "Test");

            pa.AddPA(2);

            Assert.AreEqual(5, pa.GetCurrentPA());
        }

        [Test]
        public void AddPA_CanExceedMax()
        {
            var pa = new ActionPointsComponent(5, "Test");

            pa.AddPA(2, canExceedMax: true);

            Assert.AreEqual(7, pa.GetCurrentPA());
        }
    }
}
