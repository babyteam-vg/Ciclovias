using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int mapState;
    public int smokeState;
    public int materialAmount;
    public GraphData graph;
    public List<TaskData> tasks;
    public List<TutorialData> tutorials;
    public List<TipData> tips;
    public SplineData splines;
    // ...
}