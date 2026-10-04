Imports ScottPlot

Public Class ucAnalytics

    Private Sub ucAnalytics_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadGradeStandingChart()
        LoadComponentAnalyticsChart()
        LoadAverageTrendChart()
    End Sub

    Private Sub LoadGradeStandingChart()
        Dim labels() As String = {"GE001", "GE002", "GE003", "GE004", "GE005", "GE006", "GE007", "GE008"}
        Dim values() As Double = {90, 87, 86, 92, 78, 91, 84, 86}

        FormsPlot1.Plot.Clear()
        Dim bar = FormsPlot1.Plot.Add.Bars(values)
        FormsPlot1.Plot.Axes.Bottom.SetTicks(Enumerable.Range(0, labels.Length).Select(Function(i) CDbl(i)).ToArray(), labels)
        FormsPlot1.Refresh()
    End Sub

    Private Sub LoadComponentAnalyticsChart()
        Dim labels() As String = {"PARTICIPATION", "QUIZZES", "PROJECT", "MAJOR EXAM"}
        Dim values() As Double = {100, 75, 88, 85}

        FormsPlot2.Plot.Clear()
        FormsPlot2.Plot.Add.Bars(values)
        FormsPlot2.Plot.Axes.Bottom.SetTicks(Enumerable.Range(0, labels.Length).Select(Function(i) CDbl(i)).ToArray(), labels)
        FormsPlot2.Refresh()
    End Sub

    Private Sub LoadAverageTrendChart()
        Dim xs() As Double = {1, 2, 3, 4}
        Dim ys() As Double = {95, 85, 90, 82}

        FormsPlot3.Plot.Clear()
        FormsPlot3.Plot.Add.Scatter(xs, ys)
        FormsPlot3.Refresh()
    End Sub

End Class