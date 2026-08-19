using System.Collections;
using System.Collections.Generic;

public class GetMaterialWrongConflict : ADHDConflict
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
        _type = ConflictType.MaterialOutWrong;

        ConflictSetupResult result = base.IsConflictFeasible();

        if (result.Error != ConflictGenerationError.None) return result;

        // to avoid always getting the closes one to the front of the class :)
        _adhdSeatedStudents.Shuffle();

        bool feasible = false;
        foreach (Student st in _adhdSeatedStudents)
        {
            feasible = !st.Behaviour.HasMaterialPlaced;
            if (feasible)
            {
                _conflictiveStudent = st;
                break;
            }
        }

        if (!feasible)
        {
            result.errorWhy = "There is no ADHD student without their material placed out";
            result.Error = ConflictGenerationError.NoValidStudent;
            return result;
        }
        _wasSetUp = true;

        // add context to why the conflict was not possible
        return result;
    }

    public override IEnumerator Run()
    {
        _behaviour.SetIsWrongMaterial(true);

        yield return WaitForPlayerAction();

        bool isResolved = false;

        while (!isResolved)
        {
            switch (_currentPlayerResolution)
            {
                case PlayerResolutionToConflict.Positive:
                    yield return WrongMaterialPositiveResolution();
                    isResolved = true;
                    break;

                case PlayerResolutionToConflict.Neutral:
                    WrongMaterialNeutralResolution();
                    isResolved = true;
                    break;

                case PlayerResolutionToConflict.Negative:
                    yield return WrongMaterialNegativeResolution();
                    isResolved = _currentPlayerResolution == PlayerResolutionToConflict.Negative;
                    break;
            }
        }

        ResolveConflict();
    }

    private IEnumerator WrongMaterialPositiveResolution()
    {
        _behaviour.TriggerStandUpWhileWrongMaterial();
        _behaviour.SetIsWrongMaterial(false);

        yield return _behaviour.TakeClassMaterial();

        // I don't think that students should react, since they will just turn their body and do nothing afterwards
        //StudentManager.Instance.MakeNearbyStudentsReactToPositivelyResolvedConflict(_st);
    }

    private void WrongMaterialNeutralResolution()
    {
        _behaviour.SetIsWrongMaterial(false);
        _behaviour.SetIsOff(true);
        StudentManager.Instance.MakeNearbyStudentsReactToNeutrallyResolvedConflict(_conflictiveStudent);
    }

    private IEnumerator WrongMaterialNegativeResolution()
    {
        _behaviour.SetIsWrongMaterial(false);

        int rand = UnityEngine.Random.Range(0, 2);

        if (rand == 0)  _behaviour.SetIsJustifying(true);
        else            _behaviour.SetIsCrying(true);

        yield return WaitForPlayerAction();

        if (IsNegative())
            // we only react when the conflict is totally resolved. 
            // maybe we would want reactions when crying or justifying (?)
            // and then stopping the reactions once the conflict is well handled (?)
            StudentManager.Instance.MakeNearbyStudentsReactToNeutrallyResolvedConflict(_conflictiveStudent);
    }
}