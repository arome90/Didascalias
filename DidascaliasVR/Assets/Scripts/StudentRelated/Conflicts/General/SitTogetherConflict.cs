using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SitTogetherConflict : Conflict
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
        _type = ConflictType.SitTogether;

        ConflictSetupResult result = default;

        _manager = StudentManager.Instance;
        _nonConflictiveStudents = _manager.GetStudents();

        if (_nonConflictiveStudents.Count < 3)
        {
            result.errorWhy = $"There are not enough students for a {_type.ToString()} conflict to take place. Minimum is 3";
            result.Error = ConflictGenerationError.NoValidStudent;
            return result;
        }

        // Getting a random conflictive students
        if (_conflictiveStudent == null) _conflictiveStudent = _nonConflictiveStudents.Next();

        if (_nonConflictiveStudents.Count == 3) HandleOnlyThreeStudents();

        _affectedStudents = new List<Student>();
        _affectedStudents.Add(StudentManager.Instance.GetSittingStudentFarFromOtherStudent(_conflictiveStudent));

        if (_conflictiveStudent != null && _affectedStudents[0] != null)
        {
            result.Error = ConflictGenerationError.None;
            
            _wasSetUp = true;

            return result;
        }
        else
        {
            result.Error = ConflictGenerationError.NoValidStudent;
            result.errorWhy = $"There are no students to change sits with. Other students are Standing Up or not seated at their Desk";
            return result;
        }
    }

    /// <summary>
    /// Changes the conflictive student, since it's possible it is the middle one,
    /// which is already sitting in between both of the students
    /// </summary>
    private void HandleOnlyThreeStudents()
    {
        if (_conflictiveStudent == _nonConflictiveStudents[1])
        {
            Didascalia.Utils.Log.Warning(
                "Selected student is the middle one, which is already sitting with the other two. Selecting another student for the conflict.",
                this
            );
            _conflictiveStudent = _nonConflictiveStudents[UnityEngine.Random.Range(0, 2) == 0 ? 0 : 2];
        }
    }

    private IEnumerator PositiveResolution()
    {
        yield return _behaviour.SitDown_();
        ResolveConflict();
    }

    private void NegativeResolution()
    {
        _behaviour.SetIsJustifying(true);
        StudentManager.Instance.MakeNearbyStudentsReactToBadlyResolvedConflict(_conflictiveStudent);
        ResolveConflict();
    }

    public override IEnumerator Run()
    {
        Student farStudent = _affectedStudents[0];
        _behaviour.StartCoroutine(_behaviour.MovingTowardsPoint_(farStudent.Desk.OutOfDeskTransform.position));
        yield return WaitForPlayerAction();

        if (IsPositive()) yield return _behaviour.SitDown_();
        else
        {
            ResetPlayerResolution();

            yield return new WaitUntil(() => _behaviour.IsStandingOutOfDesk());
            yield return _behaviour.TalkToSomeoneForTime_(farStudent, 1.0f);

            StudentBehaviour targetBehaviour = farStudent.GetComponent<StudentBehaviour>();

            while (HasNotActed())
                yield return WaitForPlayerAction();

            // we wait until the student has talked with someone else and is changing their position for the player's actions
            // to take place
            if (IsPositive()) PositiveResolution();
            else
            {
                ResetPlayerResolution();

                // we wait until the other student has left their desk and sat down
                yield return targetBehaviour.LeaveDesk_();

                if (IsPositive())
                {
                    targetBehaviour.SitDown();
                    yield return PositiveResolution();
                }
                else
                {
                    _behaviour.ChangeDeskWithStudent(targetBehaviour, false);

                    targetBehaviour.SitDown();

                    yield return _behaviour.SitDown_();

                    if (HasNotActed())
                        yield return WaitForPlayerAction();

                    if (IsPositive())
                    {
                        yield return new WaitUntil(() => targetBehaviour.IsSittingOnChair());

                        targetBehaviour.LeaveDesk();
                        yield return _behaviour.LeaveDesk_();

                        _behaviour.ChangeDeskWithStudent(targetBehaviour, false);

                        targetBehaviour.SitDown();
                        yield return PositiveResolution();
                    }
                    // the neutral resolution would be to ignore it, so we do nothing in that case
                    // Negative resolution
                    else if (IsNegative()) NegativeResolution();
                }
            }
        }
    }
}
