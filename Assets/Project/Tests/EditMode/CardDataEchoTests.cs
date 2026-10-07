using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ProjectTDB.Tests
{
    /// <summary>
    /// Miroir fraternel (écho de Lyse) sur une carte à cibles multiples, ex. Frappe rapide.
    /// </summary>
    public class CardDataEchoTests
    {
        private readonly List<Object> _created = new List<Object>();

        private static void SetField(object target, string name, object value)
        {
            System.Type type = target.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) { field.SetValue(target, value); return; }
                type = type.BaseType;
            }
            Assert.Fail($"Champ {name} introuvable");
        }

        private T NewUnit<T>(Vector2Int pos) where T : Unit
        {
            var go = new GameObject(typeof(T).Name);
            _created.Add(go);
            var unit = go.AddComponent<T>();
            SetField(unit, "_maxHealth", 100);
            SetField(unit, "_health", 100);
            SetField(unit, "_currentGridPos", pos);
            return unit;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }
            _created.Clear();
        }

        [Test]
        public void AllyOrEnemyCard_DamagesEnemy_ButNeverAlly()
        {
            // Corde de rappel : tire un allié ou un ennemi, seul l'ennemi prend les dégâts
            var crux = NewUnit<CruxUnit>(new Vector2Int(0, 0));
            var ally = NewUnit<EvanUnit>(new Vector2Int(0, 3));
            var enemy = NewUnit<Enemy>(new Vector2Int(3, 0));
            var card = ScriptableObject.CreateInstance<CardData>();
            _created.Add(card);
            card.targetType = CardTargetType.OtherUnit;
            card.targetRange = 5;
            card.damageAmount = 15;

            card.ExecuteEffect(crux, ally);
            card.ExecuteEffect(crux, enemy);

            Assert.AreEqual(100, ally.GetHealth());
            Assert.AreEqual(85, enemy.GetHealth());
        }

        [Test]
        public void FrappeRapide_TwoTargets_LyseEchoesEachTarget()
        {
            var evan = NewUnit<EvanUnit>(new Vector2Int(0, 0));
            var lyse = NewUnit<SummonUnit>(new Vector2Int(2, 0));
            lyse.SetOwner(evan);
            SetField(lyse, "_echoDelay", 0f); // écho immédiat (pas de coroutine en EditMode)
            SetField(evan, "_activeSummon", lyse);
            var first = NewUnit<Enemy>(new Vector2Int(1, 0));
            var second = NewUnit<Enemy>(new Vector2Int(0, 1));

            var card = ScriptableObject.CreateInstance<CardData>();
            _created.Add(card);
            card.cardName = "Frappe rapide";
            card.targetType = CardTargetType.Enemy;
            card.targetRange = 3;
            card.targetCount = 2;
            card.damageAmount = 16;

            // Comme HandUIController.ExecutePendingMultiTargetCard : une résolution par cible
            card.ExecuteEffect(evan, first, default, false);
            card.ExecuteEffect(evan, second, default, true);

            Assert.AreEqual(100 - 16 - 6, first.GetHealth(), "1re cible : 16 d'Evan + écho de Lyse (40 % de 16 = 6)");
            Assert.AreEqual(100 - 16 - 6, second.GetHealth(), "2e cible : Lyse renvoie aussi l'attaque");
        }

        [Test]
        public void Echo_40PercentOfOriginalAttack_ThenEchoTargetDefense()
        {
            // Evan attaque de 30 un boss à 10 d'armure (il subit 20) ; Lyse : 40 % de 30 = 12, moins 10 = 2
            var evan = NewUnit<EvanUnit>(new Vector2Int(0, 0));
            var lyse = NewUnit<SummonUnit>(new Vector2Int(2, 0));
            lyse.SetOwner(evan);
            SetField(lyse, "_echoDelay", 0f);
            SetField(evan, "_activeSummon", lyse);
            var boss = NewUnit<Enemy>(new Vector2Int(1, 0));
            boss.ModifyStats(0, 10, 0, 0);

            var card = ScriptableObject.CreateInstance<CardData>();
            _created.Add(card);
            card.targetType = CardTargetType.Enemy;
            card.targetRange = 3;
            card.damageAmount = 30;

            card.ExecuteEffect(evan, boss, default, false);

            Assert.AreEqual(100 - 20 - 2, boss.GetHealth());
        }
    }
}
