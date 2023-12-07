using static Types;

/// <summary>
/// Interface to make objects legitimate targets for towers
/// </summary>
public interface IDamageableByTowers
{
    /// <summary>
    /// Calculates the incoming damage under consideration of the objects defence values
    /// </summary>
    /// <param name="damageType">Type of incoming damage</param>
    /// <param name="incomingRawDamage">Incoming damage with no reductions</param>
    /// <returns>Damage value considering the defense of the hitten object</returns>
    public float CalculateComittedDamage(DamageType damageType, float incomingRawDamage);
    
    /// <summary>
    /// Reduce the current health of the object by the given value
    /// </summary>
    /// <param name="reductionValue">Value to reduce current ovjects health</param>
    public void DegreaseHealth(float reductionValue);
    
    /// <summary>
    /// Checks if the objects healt has reached 0
    /// </summary>
    /// <returns>true if objects health value is 0 or less
    ///          <br>false otherwise</br></returns>
    public bool IsDead();

    /// <summary>
    /// Performes actions in the frame the tower will be deleted
    /// </summary>
    public void OnDestroy();

    /// <summary>
    /// Make sure the tower is not in the list of activ towers in ObjectCollections class
    /// </summary>
    public void RemoveSelfFromAgentCollection();

    /// <summary>
    /// make sure the tower is added to the list of activ towers in  ObjectsCollections class
    /// </summary>
    public void AddSelfToAgentCollection();

    /// <summary>
    /// Adds the given status effect to the object and/or its group
    /// </summary>
    /// <param name="statusToAdd">The status effect that should be added</param>
    /// <param name="effectDuration">Time the effect should be activ in seconds</param>
    /// <param name="toEntireGroup">Effect should affect each object in the group (yes/no)</param>
    public void AddStatus(Status statusToAdd, float effectDuration, bool toEntireGroup);
}
