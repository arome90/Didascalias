using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using static StudentBehaviour;

public class BotherSomeoneConflict : ADHDConflict
{
    private Student PopAdhdStudent()
    {
        Student st = _adhdSeatedStudents.Next();
        _adhdSeatedStudents.Remove(st);
        return st;
    }

    private bool HasNearStudentSeated()
    {
        return (_conflictiveStudent.NextStudent != null && _conflictiveStudent.NextStudent.Behaviour.IsSittingOnChair())
            || (_conflictiveStudent.PreviousStudent != null && _conflictiveStudent.PreviousStudent.Behaviour.IsSittingOnChair());
    }

    public override ConflictSetupResult IsConflictFeasible()
    {
        _type = ConflictType.BotherStudents;

        ConflictSetupResult result = base.IsConflictFeasible();

        if (result.Error != ConflictGenerationError.None) return result;

        if (_nonConflictiveStudents.Count == 1)
        {
            result.Error = ConflictGenerationError.NoValidStudent;
            result.errorWhy = "There is only one student";
            return result;
        }

        if (_conflictiveStudent == null) _conflictiveStudent = PopAdhdStudent();

        // we check if any ADHD has anybody near them that is seated, so the conflictive student can bother them.
        // IF (DOESNT HAVE NEAR STUDENT OR IF THAT STUDENT IS NOT SEATED)
        while (!HasNearStudentSeated()
            && _adhdSeatedStudents.Count > 0) ;
        _conflictiveStudent = PopAdhdStudent();

        if (!HasNearStudentSeated())
        {
            result.Error = ConflictGenerationError.NoValidStudent;
            result.errorWhy = "ADHD students don't have anyone near them to bother";
            return result;
        }
        else
        {
            // we get one of those students as our target
            _affectedStudents = new List<Student>();
            int rand = UnityEngine.Random.Range(0, 2);
            if (rand == 0 && _conflictiveStudent.NextStudent != null)
                _affectedStudents.Add(_conflictiveStudent.NextStudent);
            else
                _affectedStudents.Add(_conflictiveStudent.PreviousStudent);
        }

        _wasSetUp = true;

        return result;
    }

    public override IEnumerator Run()
    {
        Student st = _affectedStudents[0];
        yield return _behaviour.BotherSomeone_(st);

        ListenToPlayerResolution();

        LookDirection savedLookDirection = _behaviour.CurrentLookDirection;

        float time = 0.0f;
        bool isBothering = false;
        while (HasNotActed())
        {
            yield return null;
            time += Time.deltaTime;

            if (time > 5.0f)
            {
                int random = UnityEngine.Random.Range(0, 3);
                // in this loop we try to bother a student the maximum time possible and then stop bothering them for at least 5 seconds.
                // then we come back to bother him
                if (random < 2 && !isBothering)
                {
                    isBothering = true;
                    _behaviour.StopTalking();
                    _behaviour.SetIsBothering(true);

                    st.Behaviour.SetAnnoyed(true, _conflictiveStudent.transform);
                    _behaviour.SetLookDirection(savedLookDirection);
                    _behaviour.StartTalking(true);
                }
                else if (random == 2)
                {
                    isBothering = false;
                    st.Behaviour.SetAnnoyed(false, null);
                    _behaviour.SetIsBothering(false);
                    _behaviour.StopTalking();
                    _behaviour.SetLookDirection(LookDirection.Front);
                }

                time = 0.0f;
            }
        }

        _behaviour.StopTalking();
        _behaviour.SetIsBothering(false);

        bool isResolved = false;

        while (!isResolved)
        {
            switch (_currentPlayerResolution)
            {
                case PlayerResolutionToConflict.Positive:
                    yield return BotherStudentPositiveResolution(st);
                    isResolved = true;
                    break;

                case PlayerResolutionToConflict.Neutral:
                    // this never has a neutral resolution, because the student keeps bothering people, so we never resolve the conflict.
                    // instead we wait until the user does something, either positive or negative
                    yield return BotherStudentNeutralResolution(st);
                    break;

                case PlayerResolutionToConflict.Negative:
                    yield return BotherStudentNegativeResolution(st);
                    isResolved = _currentPlayerResolution != PlayerResolutionToConflict.Positive;
                    break;

            }
        }

        ResolveConflict();
    }

    private IEnumerator BotherStudentPositiveResolution(Student st)
    {
        //_animator.SetIsBothering(false);
        //_animator.SetIsJustifying(false);
        //st.Behaviour.SetAnnoyed(false, transform);

        _behaviour.StopBotheringTarget();

        if (_behaviour.IsStanding()) yield return _behaviour.SitDown_();

        _behaviour.SetIsWriting(true);
    }

    private IEnumerator BotherStudentNeutralResolution(Student st)
    {
        yield return _behaviour.LeaveDesk_();
        ListenToPlayerResolution();

        yield return _behaviour.MoveAndLookToStudent_(st);

        _behaviour.SetIsBothering(true);
        _behaviour.SetLookDirection(LookDirection.Front);

        _behaviour.StartTalking(true);

        float time = 0.0f;
        while (HasNotActed())
        {
            yield return null;
            time += Time.deltaTime;

            if (time > 5.0f)
            {
                int rand = UnityEngine.Random.Range(0, 3);
                if (rand >= 2)
                {
                    _behaviour.SetIsBothering(false);

                    st.Behaviour.SetAnnoyed(false, null);
                    st = StudentManager.Instance.GetStudentFarFromOtherStudent(_conflictiveStudent);

                    yield return _behaviour.MoveAndLookToStudent_(st);

                    st.Behaviour.SetAnnoyed(true, _conflictiveStudent.transform);
                    _behaviour.SetIsBothering(true);
                }
                time = 0.0f;
            }
        }

        st.Behaviour.SetAnnoyed(false, null);
    }

    private IEnumerator BotherStudentNegativeResolution(Student st)
    {
        st.Behaviour.SetAnnoyed(false, null);

        int random = -1;

        // we add the "IsStanding" check because the third option needs the student to be sitting
        if (_behaviour.IsStanding())    random = UnityEngine.Random.Range(0, 2);
        else                            random = UnityEngine.Random.Range(0, 3);

        ListenToPlayerResolution();
        if (random == 0)
        {
            _behaviour.SetIsJustifying(true);

            yield return _behaviour.SmoothLookAt_(FindFirstObjectByType<XROrigin>().transform, 1.0f);

            while (HasNotActed())
                yield return WaitForPlayerAction();

            if (IsPositive()) _behaviour.SetIsJustifying(false);

        }
        // we add the "IsStanding" check because the third option needs the student to be sitting
        else if (random == 1)
        {
            yield return _behaviour.StandUp_();

            yield return _behaviour.SmoothLookAt_(FindFirstObjectByType<XROrigin>().transform, 1.0f);
            _behaviour.StartTalking(true);
            _behaviour.SetIsJustifying(true);

            while (HasNotActed())
                yield return WaitForPlayerAction();

            _behaviour.StopTalking();

            if (!IsPositive())
            {
                yield return _behaviour.MoveToFrontDoor_();
                yield return _behaviour.OpenDoorInside_();
                yield return _behaviour.MovementAnimationAndRotate_(ClassManager.Instance.FrontDoor.OutsideStandingPoint, 0.95f);
                yield return _behaviour.CloseDoorOutside_();
            }
        }
        else
        {
            _behaviour.SetIsCrying(true);

            while (HasNotActed())
                yield return WaitForPlayerAction();

            if (IsPositive()) _behaviour.SetIsCrying(false);
        }
    }
}