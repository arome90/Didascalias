using System.Collections;
using System.Collections.Generic;

public class StandUpConflict : Conflict
{
    public override void RegisterActions()
    {
        RegisterPositiveActions(new List<PlayerAction> {
            PlayerAction.Acalmar
        });
        RegisterNeutralActions(new List<PlayerAction> {
            PlayerAction.FalarBaixo
        });
        RegisterNegativeActions(new List<PlayerAction> {
            PlayerAction.Advertencia
        });
    }

    public override ConflictSetupResult IsConflictFeasible()
    {
        _type = ConflictType.StandUp;
        ConflictSetupResult result = default;

        _nonConflictiveStudents.Shuffle();

        bool anybodySitting = false; 
        foreach (Student st in _nonConflictiveStudents)
        {
            anybodySitting = st.Behaviour.IsSittingOnChair();
            if (anybodySitting) { _conflictiveStudent = st; }
        }

        if (!anybodySitting)
        {
            result.Error = ConflictGenerationError.NoValidStudent;
            result.errorWhy = "There is no seated student to stand up";
        }

        _wasSetUp = true;

        return result;
    }

    private IEnumerator PositiveResolution()
    {
        _behaviour.StopBotheringTarget();
        yield return _behaviour.SitDown_();
        ResolveConflict();
    }

    private IEnumerator NegativeResolution()
    {
        _behaviour.StopBotheringTarget();
        yield return _behaviour.Expel_();
        ResolveConflict();
    }

    public override IEnumerator Run()
    {
        ResetPlayerResolution();
        yield return _behaviour.StandUp_();

        while (HasNotActed())
            yield return WaitForPlayerAction();

        if (IsPositive()) yield return PositiveResolution();
        else if (IsNeutral())
        {
            ResetPlayerResolution();
            yield return _behaviour.BotherSomeone_(null);

            while (HasNotActed() || IsNeutral())
                yield return WaitForPlayerAction();

            if (IsPositive()) yield return PositiveResolution();
            else if (IsNegative()) yield return NegativeResolution();
        }
        else if (IsNegative())
        {
            _behaviour.SetIsJustifying(true);
            _behaviour.StartTalking();

            yield return WaitForPlayerAction();

            if (IsPositive()) yield return PositiveResolution();
            else
            {
                _behaviour.SetIsJustifying(false);
                yield return NegativeResolution();
            }
        }
    }
}