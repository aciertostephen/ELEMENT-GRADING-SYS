' Helpers/GradePredictor.vb
' Pure logic: no controls, no database. The UI builds the item list and calls these functions.

''' <summary>One gradable item (Q1, Project, Final Exam...) for one student.</summary>
Public Class PredictItem
    ''' <summary>This item's share of the FINAL grade, in percentage points.
    ''' Example: Quizzes are 20% and Q1 is 10 of the 30 total quiz points, so Weight = 20 * 10/30 = 6.67.
    ''' Use the same formula as GradeCalculator so predicted and real grades agree.</summary>
    Public Property Weight As Double
    Public Property MaxScore As Double
    ''' <summary>Nothing = not scored yet (this is what we predict).
    ''' A value = the real score, or a guess the student typed.</summary>
    Public Property Score As Double?
End Class

Public Class PredictResult
    ''' <summary>"AlreadyMet", "Achievable", "Impossible" or "NothingLeft"</summary>
    Public Property Status As String
    ''' <summary>Percent of the max score needed on EVERY remaining item (0-100).</summary>
    Public Property NeededPercent As Double
    ''' <summary>Final percent if every remaining item gets a perfect score.</summary>
    Public Property MaxPossible As Double
    ''' <summary>Final percent already locked in (remaining items = 0).</summary>
    Public Property EarnedSoFar As Double
End Class

Public Module GradePredictor

    ''' <summary>Solves for the percent needed on each remaining item to reach targetPercent.</summary>
    Public Function SolveForTarget(items As List(Of PredictItem), targetPercent As Double) As PredictResult

        Dim earned As Double = 0
        Dim remainingWeight As Double = 0

        For Each it As PredictItem In items
            If it.Score.HasValue Then
                earned += it.Weight * (it.Score.Value / it.MaxScore)
            Else
                remainingWeight += it.Weight
            End If
        Next

        Dim result As New PredictResult With {
            .EarnedSoFar = earned,
            .MaxPossible = earned + remainingWeight
        }

        If remainingWeight <= 0 Then
            result.Status = "NothingLeft"
            Return result
        End If

        Dim needed As Double = (targetPercent - earned) / remainingWeight * 100

        If needed <= 0 Then
            result.Status = "AlreadyMet"
            result.NeededPercent = 0
        ElseIf needed > 100 Then
            result.Status = "Impossible"
            result.NeededPercent = needed
        Else
            result.Status = "Achievable"
            result.NeededPercent = needed
        End If

        Return result

    End Function

    ''' <summary>Suggested score for one remaining item. Rounded UP so the target is really met.</summary>
    Public Function SuggestedScore(item As PredictItem, neededPercent As Double) As Double
        Dim raw As Double = neededPercent / 100 * item.MaxScore
        Return Math.Min(item.MaxScore, Math.Ceiling(raw * 100) / 100)
    End Function

End Module