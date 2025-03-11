using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int MapState;
    public int SmokeState;
    public int MaterialAmount;
    public GraphData graph;
    public List<TaskData> tasks;
    public List<TutorialData> tutorials;
    // ...
}