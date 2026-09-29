using NUnit.Framework;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Comparaison des empreintes d'état hôte / clients, quel que soit l'ordre d'arrivée.
    /// </summary>
    public class DesyncDetectorTests
    {
        [Test]
        public void SameState_NoMismatch_WhateverTheOrder()
        {
            var detector = new DesyncDetector();

            Assert.IsEmpty(detector.AddHost(1, "A"));
            Assert.IsEmpty(detector.AddClient(7, 1, "A"));   // client en retard
            Assert.IsEmpty(detector.AddClient(7, 2, "B"));   // client en avance
            Assert.IsEmpty(detector.AddHost(2, "B"));
        }

        [Test]
        public void DifferentState_ReportedWithBothStates()
        {
            var detector = new DesyncDetector();
            detector.AddClient(7, 3, "Evan PV90");

            var mismatches = detector.AddHost(3, "Evan PV74");

            Assert.AreEqual(1, mismatches.Count);
            Assert.AreEqual(7ul, mismatches[0].ClientId);
            Assert.AreEqual(3, mismatches[0].Turn);
            Assert.AreEqual("Evan PV74", mismatches[0].HostState);
            Assert.AreEqual("Evan PV90", mismatches[0].ClientState);
        }
    }
}
