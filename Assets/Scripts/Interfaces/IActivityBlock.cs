using System.Collections;
/// <summary>
/// Interface switch if a object should exeture its behaviour or not
/// </summary>
public interface IActivityBlock
{
    public bool blockAllActivities { get; set; }
    public void StartConstructionTimer();
}
