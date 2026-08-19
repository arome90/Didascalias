using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StudentBehaviour;

class HyperstimulationConflict : TEAConflict
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
        _type = ConflictType.Hyperstimulation;

        ConflictSetupResult result = base.IsConflictFeasible();

        if (result.Error != ConflictGenerationError.None) return result;

        _conflictiveStudent = _autisticSeatedStudents.Next();

        _wasSetUp = true;

        return result; ;
    }

    public override IEnumerator Run()
    {
        _behaviour.SetIsHyperstimulated(true);

        yield return WaitForPlayerAction();

        bool isResolved = false;

        while (!isResolved)
        {
            switch (_currentPlayerResolution)
            {
                case PlayerResolutionToConflict.Positive:
                    yield return HyperstimulationPositiveResolution();
                    isResolved = (_currentPlayerResolution == PlayerResolutionToConflict.Positive);
                    break;

                case PlayerResolutionToConflict.Neutral:
                    HyperstimulationNeutralResolution();
                    isResolved = true;
                    break;

                case PlayerResolutionToConflict.Negative:
                    yield return NegativeResolution();
                    isResolved = true;
                    break;
            }
        }

        ResolveConflict();
    }

    private IEnumerator HyperstimulationPositiveResolution()
    {
        _behaviour.StartTalking();
        _behaviour.SetAnxiety(false);

        int progress = 0;

        // when progress reaches -2, we change to the Neutral or Negative conflict resolution
        // when progress reaches +2, we continue the Positive conflict resolution
        while (Mathf.Abs(progress) < 2)
        {
            yield return WaitForPlayerAction();

            // if neutral or negative -> We set anxiety and deduct progress from player.
            if (!IsPositive())
            {
                progress--;
                // we set anxiety
                _behaviour.SetAnxiety(true);
            }
            else
            {
                progress++;
                // we remove axniety
                _behaviour.SetAnxiety(false);
            }
        }

        if (progress >= 2)
        {
            StudentManager.Instance.MakeNearbyStudentsReactToPositivelyResolvedConflict(_conflictiveStudent);
            _behaviour.SetIsHyperstimulated(false);
        }

        _behaviour.StopTalking();
    }

    private void HyperstimulationNeutralResolution()
    {
        _behaviour.SetIsHyperstimulated(false);
        _behaviour.SetIsOff(true);
        StudentManager.Instance.MakeNearbyStudentsReactToNeutrallyResolvedConflict(_conflictiveStudent);
    }

    // Lógica de resolución negativa separada para mantener el corrutina principal limpia
    private IEnumerator NegativeResolution()
    {
        int rand = UnityEngine.Random.Range(0, 3);

        if (rand == 0) _behaviour.SetHighAnxiety(true);
        else if (rand == 1)
        {
            yield return _behaviour.GoToFloor_();
            _behaviour.SetAnxiety(true);
        }
        else
        {
            yield return _behaviour.MoveToRandomPoint(MovementAction.RunAnxiety);
            yield return _behaviour.GoToFloor_();
            _behaviour.SetAnxiety(true);
        }

        // make all students react to the badly resolved conflict that just ended
        StudentManager.Instance.MakeNearbyStudentsReactToBadlyResolvedConflict(_conflictiveStudent);
    }
}
