using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class GetDistractedTEAConflict : TEAConflict
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
        _type = ConflictType.DistractionTEA;

        ConflictSetupResult result = base.IsConflictFeasible();

        if (result.Error != ConflictGenerationError.None) return result;

        _conflictiveStudent = _autisticSeatedStudents.Next();

        _wasSetUp = true;

        return result;
    }

    public override IEnumerator Run()
    {
        _behaviour.SetIsDistracted(true);

        yield return WaitForPlayerAction();

        bool isResolved = false;

        while (!isResolved)
        {
            switch (_currentPlayerResolution)
            {
                case PlayerResolutionToConflict.Positive:
                    yield return GetDistractedPositiveResolution();
                    isResolved = (_currentPlayerResolution == PlayerResolutionToConflict.Positive);
                    break;

                case PlayerResolutionToConflict.Neutral:
                    GetDistractedNeutralResolution();
                    isResolved = true;
                    break;

                case PlayerResolutionToConflict.Negative:
                    GetDistractedNegativeResolution();
                    isResolved = true;
                    break;
            }
        }

        ResolveConflict();
    }

    private IEnumerator GetDistractedPositiveResolution()
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
                // we remove anxiety
                _behaviour.SetAnxiety(false);
            }
        }

        if (progress >= 2)
        {
            _behaviour.SetIsDistracted(false);
            StudentManager.Instance.MakeNearbyStudentsReactToPositivelyResolvedConflict(_conflictiveStudent);
        }

        _behaviour.StopTalking();
    }

    private void GetDistractedNeutralResolution()
    {
        _behaviour.SetAnxiety(false);
        _behaviour.SetIsDistracted(false);
        _behaviour.SetIsOff(true);

        StudentManager.Instance.MakeNearbyStudentsReactToNeutrallyResolvedConflict(_conflictiveStudent);
    }

    private void GetDistractedNegativeResolution()
    {
        int rand = UnityEngine.Random.Range(0, 2);

        if (rand == 0)  _behaviour.SetHighAnxiety(true);
        else            _behaviour.SetAnxiety(true);

        StudentManager.Instance.MakeNearbyStudentsReactToBadlyResolvedConflict(_conflictiveStudent);
    }
}
