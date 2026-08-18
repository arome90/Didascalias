using System.Collections.Generic;

/// <summary>
/// Specific type of conflict that handles getting an ADHD 
/// and sitting student as the _conflictiveStudent, so we don't implement it thrice
/// </summary>
public abstract class ADHDConflict : Conflict
{
    protected List<Student> _adhdSeatedStudents = null;

    public override ConflictSetupResult IsConflictFeasible()
    {
        ConflictSetupResult result = default;

        _adhdSeatedStudents = new List<Student>();

        List<Student> allAutisticStudents = _manager.GetADHDStudents();

        foreach (Student st in allAutisticStudents)
        {
            if (st.Behaviour.IsSittingOnChair()) _adhdSeatedStudents.Add(st);
        }

        if (_adhdSeatedStudents.Count == 0)
        {
            result.Error = ConflictGenerationError.NoValidStudent;

            if (allAutisticStudents.Count > 0)
                result.errorWhy = "There are no ADHD students who are seated";
            else
                result.errorWhy = "There are no ADHD students";

            return result;
        }

        if (_conflictiveStudent == null) _conflictiveStudent = _adhdSeatedStudents.Next();

        return result;
    }
}
