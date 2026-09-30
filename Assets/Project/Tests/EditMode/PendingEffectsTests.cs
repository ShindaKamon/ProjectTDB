using System.Collections;
using NUnit.Framework;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// PendingEffects : un effet différé compte tant que sa coroutine n'est pas terminée.
    /// </summary>
    public class PendingEffectsTests
    {
        private static IEnumerator TwoSteps()
        {
            yield return null;
            yield return null;
        }

        private static IEnumerator Failing()
        {
            yield return null;
            throw new System.InvalidOperationException("effet interrompu");
        }

        [Test]
        public void Any_IsTrueWhileEffectRuns_ThenFalseWhenDone()
        {
            IEnumerator tracked = PendingEffects.Track(TwoSteps());
            Assert.IsFalse(PendingEffects.Any, "rien tant que l'effet n'a pas démarré");

            Assert.IsTrue(tracked.MoveNext());
            Assert.IsTrue(PendingEffects.Any);
            Assert.IsTrue(tracked.MoveNext());
            Assert.IsTrue(PendingEffects.Any);

            Assert.IsFalse(tracked.MoveNext(), "fin de l'effet");
            Assert.IsFalse(PendingEffects.Any);
        }

        [Test]
        public void Any_CountsEveryRunningEffect()
        {
            IEnumerator first = PendingEffects.Track(TwoSteps());
            IEnumerator second = PendingEffects.Track(TwoSteps());
            first.MoveNext();
            second.MoveNext();

            while (first.MoveNext()) { }
            Assert.IsTrue(PendingEffects.Any, "le second tourne encore");

            while (second.MoveNext()) { }
            Assert.IsFalse(PendingEffects.Any);
        }

        [Test]
        public void Any_IsReleased_WhenEffectThrows()
        {
            IEnumerator tracked = PendingEffects.Track(Failing());
            tracked.MoveNext();
            Assert.IsTrue(PendingEffects.Any);

            Assert.Throws<System.InvalidOperationException>(() => tracked.MoveNext());
            Assert.IsFalse(PendingEffects.Any, "le bloc finally libère l'effet");
        }
    }
}
