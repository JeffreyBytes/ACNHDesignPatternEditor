using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

public class ImportOperation : IOperation, ISelectPatternOperation, IBackToPatternExchangeOperation
{
    private DesignServer.Pattern Pattern;
    private bool _IsFinished = false;
    private bool _IsPro = false;
    public ImportOperation(DesignServer.Pattern pattern, bool isPro = false)
    {
        this._IsPro = isPro;
        this.Pattern = pattern;
    }

    public void Abort()
    {
        _IsFinished = true;
    }

    public bool IsFinished()
    {
        return _IsFinished;
    }

    public bool IsPro()
    {
        return this._IsPro;
    }

    public void SelectPattern(DesignPattern pattern)
    {
        var copy = (System.Action) (() =>
        {
            DesignPattern designPattern = null;
            var acnhFileFormat = new ACNHFileFormat(this.Pattern.Bytes);
            if (acnhFileFormat.IsPro)
            {
                designPattern = new ProDesignPattern();
                designPattern.CopyFrom(acnhFileFormat);
            }
            else
            {
                designPattern = new SimpleDesignPattern();
                designPattern.CopyFrom(acnhFileFormat);
            }
            pattern.CopyFrom(designPattern);
            pattern.Name = Pattern.Name;
            pattern.ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
        });
        if (pattern.IsSet)
        {
            Controller.Instance.ConfirmationPopup.Show("<align=\"center\"><#827157>,,<#12c7ce>" + pattern.Name + "<#827157>‘‘\r\nwill be overwritten. Continue?", () => {
                copy();
                _IsFinished = true;
            }, () => {
                _IsFinished = true;
            });
        }
        else
        {
            copy();
            _IsFinished = true;
        }
    }

    public void Start()
    {
    }
}