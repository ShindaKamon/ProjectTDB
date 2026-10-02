using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phases 1 et 2 du Monstre sous le lit (Enemies.md, révision du 02/10/2026) :
/// - phase 1, cache-cache : le boss est caché sous un lit (invisible et ignoré par la grille). Seul ce lit subit
///   des dégâts, qui passent au boss ; le premier coup le révèle (son ombre dépasse du lit). Il change de lit, et
///   redevient caché, quand il joue une carte qui le fait changer de cachette (Marée d'ombre) et quand son lit casse ;
/// - phase 2 (Enemy passe en phase 2 à 0 PV) : il fusionne avec son lit, le Lit, qu'il porte sur son dos (à moitié visible, immobile) ; les
///   autres lits s'effondrent, le Lit reçoit les PV de la phase et ses dégâts passent toujours au boss ;
/// - phase 3 : le Lit cède et le boss en sort.
/// Ajouté au boss par GridManager quand la rencontre a des lits.
/// </summary>
public class BedHiding : MonoBehaviour
{

    private Enemy _boss;
    private readonly List<BedUnit> _beds = new List<BedUnit>();
    private BedUnit _current;
    private bool _fused;

    /// <summary>True tant que le boss est dans un lit (caché en phase 1, fusionné en phase 2).</summary>
    public bool IsHiding => _current != null;

    /// <summary>Lit où se trouve le boss (null une fois sorti).</summary>
    public BedUnit CurrentBed => _current;

    public void Begin(Enemy boss, List<BedUnit> beds)
    {
        _boss = boss;
        _beds.AddRange(beds);
        int bedHealth = BedHideout.BedHealth(boss.GetMaxHealth(), beds.Count);
        foreach (BedUnit bed in beds) bed.SetFullHealth(bedHealth);

        SetVisible(false);
        MoveTo(_beds[boss.Rng.Next(_beds.Count)]);
        EventBus.Subscribe<UnitDamagedEvent>(OnUnitDamaged);
        EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
        EventBus.Subscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
        EventBus.Subscribe<UnitHealedEvent>(OnUnitHealed);
        GameLog.Log($"{boss.name} se cache sous l'un des {beds.Count} lits ({bedHealth} PV chacun)");
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<UnitDamagedEvent>(OnUnitDamaged);
        EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
        EventBus.Unsubscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
        EventBus.Unsubscribe<UnitHealedEvent>(OnUnitHealed);
    }

    // Son lit récupère ce que le boss récupère (ex. Tapi dans le noir) : en phase 1 la barre reste à la mesure des
    // lits, en phase 2 le Lit porte les PV du boss
    private void OnUnitHealed(UnitHealedEvent e)
    {
        if (e.Target == _boss && _current != null) _current.Heal(e.HealAmount);
    }

    /// <summary>Phase 1 : il passe sous un autre lit, caché à nouveau (ex. Marée d'ombre).</summary>
    public void MoveToAnotherBed()
    {
        if (!IsHiding || _fused || _beds.Count <= 1) return;
        Transform shadow = _current.ShadowTemplate;
        MoveTo(_beds[BedHideout.NextBed(_boss.Rng, _beds.Count, _beds.IndexOf(_current))]);
        if (shadow != null) StartCoroutine(ShadowVanishes(shadow)); // il se perd dans le noir : on sait qu'il a bougé, pas où
    }

    /// <summary>
    /// Cases d'où il peut frapper : n'importe quel lit encore debout (une main sort de sous n'importe quel lit),
    /// ce qui ne trahit pas celui où il se cache.
    /// </summary>
    public List<Vector2Int> AttackOrigins()
    {
        var cells = new List<Vector2Int>();
        foreach (BedUnit bed in _beds) if (bed != null && bed.GetHealth() > 0) cells.Add(bed.GetCurrentGridPos());
        return cells;
    }

    /// <summary>
    /// Case d'où sortent les monstres qu'il invoque : un lit au hasard (son lit en phase 2), pour ne pas
    /// trahir sa cachette en phase 1.
    /// </summary>
    public Vector2Int SpawnOrigin() => _beds[_boss.Rng.Next(_beds.Count)].GetCurrentGridPos();

    private void MoveTo(BedUnit bed)
    {
        if (_current != null) _current.Occupied = false;
        _current = bed;
        bed.Occupied = true;
        _boss.TeleportTo(bed.GetCurrentGridPos());
        GameLog.Log($"{_boss.name} se cache sous le lit {bed.GetCurrentGridPos()}");
    }

    // Coup sur son lit : il est révélé, et le boss perd ce que le lit a perdu (peut le faire changer de phase)
    private void OnUnitDamaged(UnitDamagedEvent e)
    {
        if (e.Target != _current || e.EffectiveDamage <= 0) return;
        _current.Revealed = true;
        _boss.LoseHealth(e.EffectiveDamage);
    }

    // Phase 1 : son lit casse, il fuit sous un autre (caché à nouveau). Quand il ne reste qu'un lit (ou aucun), la
    // phase 1 se termine quoi qu'il reste dans sa barre : il fusionne avec le dernier lit (sinon, sans cachette, le
    // combat serait bloqué)
    private void OnUnitDied(UnitDiedEvent e)
    {
        if (!(e.DeadUnit is BedUnit bed) || !_beds.Remove(bed) || _fused) return;
        Transform fledShadow = bed == _current ? bed.ShadowTemplate : null;
        if (bed == _current) _current = null;

        if (_beds.Count <= 1)
        {
            if (_current == null && _beds.Count == 1) FleeTo(_beds[0], fledShadow);
            _boss.LoseHealth(_boss.GetHealth()); // fin de la phase 1 → Fuse (ou Emerge sans lit)
            return;
        }
        if (_current == null) FleeTo(_beds[_boss.Rng.Next(_beds.Count)], fledShadow);
    }

    // Son lit vient de casser : il fuit sous un autre, et on voit son ombre y filer (décision du 02/10/2026)
    private void FleeTo(BedUnit bed, Transform fromShadow)
    {
        MoveTo(bed);
        if (fromShadow != null) StartCoroutine(ShadowRunsTo(fromShadow, bed));
    }

    private const float ShadowRunSeconds = 0.8f;

    // Ombre noire qui file d'un lit à l'autre ; à l'arrivée, le lit est marqué comme occupé (révélé)
    private System.Collections.IEnumerator ShadowRunsTo(Transform fromShadow, BedUnit bed)
    {
        Transform blob = MakeShadowBlob(fromShadow);
        Transform target = bed.ShadowTemplate;
        Vector3 from = blob.position, to = target != null ? target.position : bed.transform.position;
        to.y = from.y;
        if ((to - from).sqrMagnitude > 0.0001f) blob.rotation = Quaternion.LookRotation(to - from);
        for (float t = 0f; t < 1f && bed != null; t += Time.deltaTime / ShadowRunSeconds)
        {
            blob.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        Destroy(blob.gameObject);
        if (bed != null && bed == _current && bed.Occupied) bed.Revealed = true;
    }

    // Marée d'ombre : l'ombre sort du lit vers la salle et se dissout dans le noir
    private System.Collections.IEnumerator ShadowVanishes(Transform fromShadow)
    {
        Transform blob = MakeShadowBlob(fromShadow);
        Vector3 outward = -fromShadow.parent.forward; // vers le pied du lit, côté salle
        Vector3 from = blob.position + outward * 0.9f, to = from + outward * 2f; // sort par le pied du lit, bien visible
        Vector3 scale = blob.localScale;
        for (float t = 0f; t < 1f; t += Time.deltaTime / (ShadowRunSeconds * 1.5f))
        {
            blob.position = Vector3.Lerp(from, to, t);
            blob.localScale = scale * Mathf.Clamp01(2f - 2f * t); // entière, puis se dissout dans la seconde moitié
            yield return null;
        }
        Destroy(blob.gameObject);
    }

    // Copie de l'ombre d'un lit, en tache ronde au sol, détachée du lit (qui peut disparaître)
    private static Transform MakeShadowBlob(Transform template)
    {
        Transform blob = Instantiate(template.gameObject).transform;
        blob.name = "ShadowOnTheMove";
        blob.gameObject.SetActive(true);
        blob.position = template.position;
        blob.rotation = template.rotation;
        blob.localScale = new Vector3(0.9f, template.lossyScale.y, 0.9f);
        return blob;
    }

    private void OnBossPhaseChanged(BossPhaseChangedEvent e)
    {
        if (e.Boss != _boss) return;
        if (e.Phase == 1) Fuse();
        else if (e.Phase == 2) Emerge();
    }

    // Phase 2 : il fusionne avec son lit (ou un autre encore debout si le sien vient de casser) ; les autres s'effondrent
    private void Fuse()
    {
        BedUnit lit = _current != null && _current.GetHealth() > 0 ? _current : _beds.Find(b => b != null && b.GetHealth() > 0);
        if (lit == null) { Emerge(); return; }

        if (lit != _current) MoveTo(lit);
        _fused = true;
        foreach (BedUnit bed in new List<BedUnit>(_beds))
            if (bed != lit && bed != null && bed.GetHealth() > 0) bed.Collapse();
        _beds.Clear();
        _beds.Add(lit);

        lit.SetFullHealth(_boss.GetMaxHealth());
        lit.Revealed = true;
        SetRenderersVisible(true); // à moitié visible : il porte le Lit sur son dos
        Vector3 center = lit.LiftOntoBoss();
        _boss.transform.position = new Vector3(center.x, _boss.transform.position.y, center.z);
        ShowFusedModel(true, lit);
        GameLog.Log($"{_boss.name} fusionne avec le lit {lit.GetCurrentGridPos()} ({lit.GetHealth()} PV)");
    }

    // Phase 3 : le Lit cède (s'il n'est pas déjà tombé avec la barre) ; le boss sort à côté
    private void Emerge()
    {
        BedUnit lit = _current;
        _current = null;
        _fused = false;
        _beds.Clear();
        if (lit == null)
        {
            // Plus aucun lit : il sort là où il était caché, sur la case libre la plus proche
            Vector2Int from = _boss.GetCurrentGridPos();
            Vector2Int? free = EnemyAI.NearestFreeCell(from, p => Services.Grid.GetTileAtPosition(p) != null && Services.Grid.GetUnitAtGridPos(p) == null);
            ShowFusedModel(false, null);
            SetVisible(true);
            _boss.TeleportTo(free ?? from);
            return;
        }

        Vector2Int cell = lit.GetCurrentGridPos();
        var litCells = new List<Vector2Int>(lit.OccupiedCells);
        Vector2Int? exit = EnemyAI.NearestFreeCell(cell, p => !litCells.Contains(p)
            && Services.Grid.GetTileAtPosition(p) != null && Services.Grid.GetUnitAtGridPos(p) == null);
        if (lit.GetHealth() > 0) lit.Collapse();
        else lit.Occupied = false;

        ShowFusedModel(false, null);
        SetVisible(true);
        _boss.TeleportTo(exit ?? cell);
        GameLog.Log($"{_boss.name} sort du Lit");
    }

    // Phase 2 : le modèle « fusionné » (enfant FusedModel du prefab : des pattes d'araignée sous le Lit soulevé, des yeux
    // rouges sous le matelas, côté pied) remplace le modèle habituel ; phase 3 : retour à l'araignée
    private void ShowFusedModel(bool fused, BedUnit lit)
    {
        Transform fusedModel = _boss.transform.Find("FusedModel");
        Transform model = _boss.transform.Find("Model");
        if (fusedModel == null) return;
        fusedModel.gameObject.SetActive(fused);
        if (model != null) model.gameObject.SetActive(!fused);
        if (!fused) return;

        fusedModel.rotation = Quaternion.LookRotation(-lit.transform.forward); // les yeux au pied du lit, vers la salle
    }

    private void SetVisible(bool visible)
    {
        _boss.IsHidden = !visible;
        SetRenderersVisible(visible);
        foreach (Collider collider in _boss.GetComponentsInChildren<Collider>(true)) collider.enabled = visible; // les clics vont au lit
    }

    private void SetRenderersVisible(bool visible)
    {
        foreach (Renderer renderer in _boss.GetComponentsInChildren<Renderer>(true)) renderer.enabled = visible;
    }
}
