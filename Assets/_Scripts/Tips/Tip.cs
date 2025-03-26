using UnityEngine;

[System.Serializable]
public class Tip
{
    [Header("Basic")]
    public bool seen;
    public TipInfo info;

    // :::::::::: STORAGE ::::::::::
    // ::::: Tip -> TipData
    public TipData SaveTip()
    {
        return new TipData
        {
            seen = this.seen,
        };
    }
}

[System.Serializable]
public class TipData
{
    public bool seen;
}
