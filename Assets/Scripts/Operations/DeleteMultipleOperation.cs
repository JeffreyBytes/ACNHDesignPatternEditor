using System.Collections.Generic;

public class DeleteMultipleOperation : IOperation, IMultiplePatternSelectorOperation
{
    private bool _IsFinished = false;
    private List<DesignPattern> Patterns = new List<DesignPattern>();

    public DeleteMultipleOperation()
    {
    }

    public void Abort()
    {
        _IsFinished = true;
    }

    public bool IsFinished()
    {
        return this._IsFinished;
    }

    public void SelectPattern(DesignPattern pattern)
    {
        Patterns.Add(pattern);
    }

    public void Start()
    {

    }

    public void Execute()
    {
        Controller.Instance.ConfirmationPopup.Show("<align=\"center\"><#827157><#12c7ce>" + Patterns.Count + "<#827157> patterns\r\nwill be removed. Continue?", () => {
            for (var i = 0; i < Patterns.Count; i++)
                Patterns[i].Empty();
            _IsFinished = true;
        }, () => {
            _IsFinished = true;
        });
    }

    public List<DesignPattern> GetPatterns()
    {
        return Patterns;
    }

    public void UnselectPattern(DesignPattern pattern)
    {
        Patterns.Remove(pattern);
    }

    public string GetConfirmName()
    {
        return "Delete selected";
    }
}