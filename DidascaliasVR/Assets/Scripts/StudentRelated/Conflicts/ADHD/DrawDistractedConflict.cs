using System.Collections;
using System.Collections.Generic;

internal class DrawDistractedConflict : ADHDConflict
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
        _type = ConflictType.DrawDistracted;

        ConflictSetupResult result = default;

        result = base.IsConflictFeasible();
        if (result.Error != ConflictGenerationError.None) return result;

        // to avoid always getting the closes one to the front of the class :)
        _adhdSeatedStudents.Shuffle();

        bool feasible = false;
        foreach (Student st in _adhdSeatedStudents)
        {
            // we need a student that has their material placed
            feasible = st.Behaviour.HasMaterialPlaced;
            if (feasible)
            {
                _conflictiveStudent = st;
                break;
            }
        }

        if (!feasible)
        {
            // add context to why the conflict was not possible
            result.Error = ConflictGenerationError.NoValidStudent;
            result.errorWhy = "There is no ADHD Seated student with their material out to start drawing distracted";
            return result;
        }

        _wasSetUp = true;
        return result;
    }

    public override IEnumerator Run()
    {
        // for non-autistic students, getting distracted means to start drawing
        _behaviour.SetIsDistracted(true);

        yield return WaitForPlayerAction();

        bool isResolved = false;

        int progress = 0;

        while (!isResolved)
        {
            switch (_currentPlayerResolution)
            {
                case PlayerResolutionToConflict.Positive:
                    progress++;
                    yield return DrawDistractedPositiveResolution(progress);
                    isResolved = progress >= 0;
                    break;

                case PlayerResolutionToConflict.Neutral:
                    // nothing happens. we wait until the conflict evolves.
                    // it doesn't make sense for the conflict to stop here
                    yield return DrawDistractedNeutralResolution();
                    break;

                case PlayerResolutionToConflict.Negative:

                    progress--;
                    yield return DrawDistractedNegativeResolution(progress);
                    isResolved = progress <= -2;
                    break;
            }
        }

        ResolveConflict();
    }

    private IEnumerator DrawDistractedPositiveResolution(int progress)
    {
        _behaviour.SetIsCrying(false);
        _behaviour.SetIsJustifying(false);

        if (progress < 0) yield return WaitForPlayerAction();

        // no reaction from other students
        else _behaviour.SetIsDistracted(false); // todo: attend animations
    }

    private IEnumerator DrawDistractedNeutralResolution()
    {
        // nothing happens. we wait until the conflict evolves.
        // it doesn't make sense for the conflict to stop here
        yield return WaitForPlayerAction();
    }

    private IEnumerator DrawDistractedNegativeResolution(int progress)
    {
        int random = UnityEngine.Random.Range(0, 2);

        _behaviour.SetIsDistracted(false);

        if (random == 0)    _behaviour.SetIsJustifying(true);
        else                _behaviour.SetIsCrying(true);

        if (progress > -2)  yield return WaitForPlayerAction();
        else                StudentManager.Instance.MakeNearbyStudentsReactToBadlyResolvedConflict(_conflictiveStudent);

    }
}
