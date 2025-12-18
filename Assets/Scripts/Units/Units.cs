// classe qui stocke les données d'une unité
using NUnit.Framework;
using UnityEngine;

public class Unit
{
    public readonly int id; // id
    public Transform unitTransform; // le transform de l'unité
    public Vector3 unitOffset = Vector3.zero;
    public readonly UnitType unitType; // le type d'unité (classe générale)
    public Player master; // le maître de l'unité (le joueur ou l'IA)
    public readonly UnitPin unitPin; // le pin de l'unité (pour le repérer sur la carte)

    public string unitName; // le nom de l'unité (personnalisable)

    private MilitaryUnitType militaryUnitType; // type de l'unité spécial militaire
    private CivilianUnitType civilianUnitType; // type de l'unité spécial civil
    public float currentHealth = 0; // vie actuelle de l'unité

    public float movesDone = 0; // le nombre de déplacements effectués ce tour
    public int lastDamagingTurn = -1; // le dernier où l'unité a subi des dégats
    public int chargesLeft = 0;
    public bool hasAttacked = false;

    // constructeur de la classe
    public Unit(Transform unitTransform, UnitType unitType, UnitPin unitPin, Player master)
    {
        this.id = UnitManager.instance.nextAvailableId;
        UnitManager.instance.nextAvailableId++;

        this.unitTransform = unitTransform;
        this.unitType = unitType;
        this.unitPin = unitPin;
        this.master = master;

        this.movesDone = 0;
        this.lastDamagingTurn = -1;
        this.unitName = UnitManager.instance.NAMES_LIST[UnityEngine.Random.Range(0, UnitManager.instance.NAMES_LIST.Length - 1)];

        unitPin.InitializePin(this.unitType.unitIcon, this.master.livery, UnitManager.instance.lastDistance);
        unitPin.worldTarget = this.unitTransform;

        if (unitType.unitCategory == UnitType.UnitCategory.military)
        {
            this.militaryUnitType = unitType as MilitaryUnitType;
            this.currentHealth = this.militaryUnitType.MaxHealth;
        }
        else
        {
            this.civilianUnitType = unitType as CivilianUnitType;
            this.chargesLeft = this.civilianUnitType.actionCharges;
        }
    }

    // prendre des dégats
    public void TakeDamage(float damage)
    {
        this.currentHealth -= damage;
        this.unitPin.UpdateHealth(this.currentHealth, this.militaryUnitType.MaxHealth);
        lastDamagingTurn = TurnManager.instance.currentTurn;
    }

    // se soigner
    public void Heal(float healAmount)
    {
        this.currentHealth += healAmount;

        if (this.currentHealth >= this.militaryUnitType.MaxHealth)
        {
            this.currentHealth = this.militaryUnitType.MaxHealth;
        }

        this.unitPin.UpdateHealth(this.currentHealth, this.militaryUnitType.MaxHealth);
    }

    // l'unité est-elle en vie
    public bool IsAlive()
    {
        return this.currentHealth >= 0;
    }

    // obtenir les données militaires de l'unité
    public MilitaryUnitType GetUnitMilitaryData()
    {
        return this.militaryUnitType;
    }

    public CivilianUnitType GetUnitCivilianData()
    {
        return this.civilianUnitType;
    }

    public bool ConsumeCharge()
    {
        this.chargesLeft -= 1;
        this.movesDone = this.unitType.MoveReach;
        return this.chargesLeft <= 0;
    }

    public void ApplyNewOffset(Vector3 newOffset)
    {
        this.unitTransform.position += newOffset - this.unitOffset;
        this.unitOffset = newOffset;
    }
}