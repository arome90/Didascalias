using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

/// <summary>
/// Specific type of conflict that handles getting an autistic 
/// and sitting student as the _conflictiveStudent, so we don't implement it thrice
/// </summary>
public abstract class TEAConflict : Conflict
{
    protected List<Student> _autisticSeatedStudents = null;

    public override ConflictSetupResult IsConflictFeasible()
    {
        ConflictSetupResult result = default;

        _autisticSeatedStudents = new List<Student>();

        List<Student> allAutisticStudents = _manager.GetAutisticStudents();

        foreach (Student st in allAutisticStudents)
        {
            if (st.Behaviour.IsSittingOnChair()) _autisticSeatedStudents.Add(st);
        }

        if (_autisticSeatedStudents.Count == 0)
        {
            result.Error = ConflictGenerationError.NoValidStudent;
            if (allAutisticStudents.Count > 0)  result.errorWhy = "No autistic student is seated, so conflict can't start";
            else                                result.errorWhy = "There are no autistic students";
        }

        return result;
    }
}
