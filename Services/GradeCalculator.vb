''' <summary>
''' THE grading formula (school policy: base 50). Used by the teacher gradebook, the student
''' gradebook, and mirrored exactly by the SQL procedure sp_compute_class_grades.
'''   GrA       = raw / max * (100 - Base) + Base         (rounded to 2 decimals)
'''   category  = average of GrA of the scored items       -> x weight / 100 (rounded to 2)
'''   final     = sum of categories (max 100); equivalent = grade_scale row with the
'''               highest min_percent that is &lt;= final (no gaps like 76.49 - 76.50).
''' Delete the private ToGrA / GrABase copies in ucGradebook and ucStudentGradebook and
''' call these instead.
''' </summary>
Public Class GradeCalculator
  Public Const Base As Decimal = 50D

  Public Shared Function ToGrA(raw As Decimal, maxScore As Decimal) As Decimal
    If maxScore <= 0 Then Return 0D
    Return Math.Round(raw / maxScore * (100D - Base) + Base, 2, MidpointRounding.AwayFromZero)
  End Function

  ''' <summary>Weighted contribution of one category given the sum and count of its GrA values.</summary>
  Public Shared Function CategoryTotal(sumGrA As Decimal, count As Integer, weightPercent As Decimal) As Decimal
    If count <= 0 Then Return 0D
    Return Math.Round(sumGrA / count * weightPercent / 100D, 2, MidpointRounding.AwayFromZero)
  End Function

  Public Shared Function Final(categoryTotals As IEnumerable(Of Decimal)) As Decimal
    Return Math.Min(100D, categoryTotals.Sum())
  End Function

  ''' <summary>Grade equivalent (1.00 ... 5.00) for a final percent, or Nothing.</summary>
  Public Shared Function Equivalent(finalPercent As Decimal) As Decimal?
    Dim r As Object = DatabaseHelper.ExecuteScalar(
      "SELECT grade_equivalent FROM grade_scale WHERE min_percent <= @p ORDER BY min_percent DESC LIMIT 1",
      {DatabaseHelper.P("@p", finalPercent)})
    If r Is Nothing Then Return Nothing
    Return Convert.ToDecimal(r)
  End Function
End Class
