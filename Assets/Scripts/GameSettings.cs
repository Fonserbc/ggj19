using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Game Settings Values", order = 0)]
public class GameSettings : ScriptableObject {

    public int longitudeDivisions = 12;

    [Tooltip("in seconds")]
    public int[] speedPeriods;


}
