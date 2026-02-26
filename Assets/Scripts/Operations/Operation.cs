using System.Collections;
using System.Collections.Generic;

public interface IOperation
{
	void Start();
	bool IsFinished();
	void Abort();
}

public interface IChangeNameOperation
{
	void SetName(string name);
}

public interface ISelectPatternOperation
{
    bool IsPro();
    void SelectPattern(DesignPattern pattern);
}
public interface ISelectSecondPatternOperation
{
    DesignPattern GetPattern();
    void SelectPattern(DesignPattern pattern);
}

public interface IMultiplePatternSelectorOperation
{
    void Execute();
    string GetConfirmName();
    List<DesignPattern> GetPatterns();
    void SelectPattern(DesignPattern pattern);
    void UnselectPattern(DesignPattern pattern);
}

public interface IBackToPatternExchangeOperation
{
    bool IsPro();
}