﻿Imports System
Imports System.IO
Imports System.Windows.Forms
Imports System.Text
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Excel

Public Class Form1


    ''' <summary>
    ''' ОК - предварительная загрузка окна с таблицей
    ''' </summary>
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ListView1.View = View.Details
        ListView1.GridLines = True
        ListView1.Columns.Add("Device", 200)
        ListView1.Columns.Add("Errors", 400)
        ListView1.FullRowSelect = True
    End Sub

    Private openfile As OpenFileDialog

    Private itm() As ListViewItem

    ''' <summary>
    ''' ОК - таблица с данными и файл в excel
    ''' 
    ''' Задачи:
    ''' Как выбрать папку, а не файлы?
    ''' Скрыть Таблицу     
    ''' Вывести названия папок 
    ''' Вывести системные сообщения
    ''' Открывать папку с файлом в конце
    ''' </summary>
    Private Sub FileOpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FileOpenToolStripMenuItem.Click

        Try
            openfile = New OpenFileDialog With {
                .Title = "Select files",
                .CheckFileExists = True,
                .Multiselect = True,
                .RestoreDirectory = True
            }

            If openfile.ShowDialog = System.Windows.Forms.DialogResult.OK Then

                For i As Integer = 0 To openfile.SafeFileNames.Count() - 1
                    ListBox1.Items.Add(openfile.SafeFileNames(i))
                    Dim vPath As String = openfile.FileNames(i)

                    TextBox3.Text = Directory.GetParent(vPath).Name

                    ListBox2.Items.Add(vPath)

                    Dim allTxt() As String = IO.File.ReadAllLines(vPath, Encoding.Default)

                    Dim addTxt = From vLine1 In allTxt Where (vLine1.StartsWith(vbTab & "OK") _
                                                           And vLine1 Like "*Закрытие*") _
                                                           OrElse vLine1.StartsWith(vbTab & "ERROR")

                    If addTxt.Count = 0 Then Continue For

                    Dim deviceName = openfile.SafeFileNames(i)
                    Dim deviceReport = String.Join(vbCrLf, addTxt.ToArray)

                    ListView1.Items.Add(New ListViewItem({deviceName, deviceReport}))
                Next

                ReDim itm(ListView1.Items.Count - 1)
                ListView1.Items.CopyTo(itm, 0)

                sortByError()
                findFolderName()
                exportToExcel()

            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End Try

    End Sub

    ''' <summary>
    ''' ОК - берет название папки выбранных файлов
    ''' </summary>
    Public Sub findFolderName()
        Try
            If ListBox1.Items.Count > 0 And ListBox2.Items.Count > 0 Then

                TextBox2.Text = System.Windows.Forms.Application.StartupPath
            Else
                Exit Sub
            End If
        Catch ex As Exception
            MsgBox("Folder name error " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' ОК - выгрузка из таблицы в файл
    ''' </summary>
    Public Sub exportToExcel()
        Try
            Dim objExcel As New Excel.Application
            Dim bkWorkBook As Excel.Workbook
            Dim shWorkSheet As Excel.Worksheet
            Dim i As Integer, j As Integer

            objExcel = New Excel.Application
            bkWorkBook = objExcel.Workbooks.Add
            shWorkSheet = CType(bkWorkBook.ActiveSheet, Excel.Worksheet)

            For i = 0 To Me.ListView1.Columns.Count - 1
                shWorkSheet.Cells(1, i + 1) = Me.ListView1.Columns(i).Text
            Next

            For i = 0 To Me.ListView1.Items.Count - 1
                For j = 0 To Me.ListView1.Items(i).SubItems.Count - 1
                    shWorkSheet.Cells(i + 2, j + 1) = Me.ListView1.Items(i).SubItems(j).Text
                Next
            Next

            objExcel.Columns.AutoFit()

            bkWorkBook.SaveAs(TextBox2.Text & "\Отчет " & Format(Now, "yyyyMMdd") & " " & TextBox3.Text)
            bkWorkBook.Close()
            objExcel.Quit()

        Catch ex As Exception
            MsgBox("Export excel error " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' ОК - очистка по кнопке для повторного использования
    ''' </summary>
    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        ListBox1.Items.Clear()
        ListBox2.Items.Clear()
        ListView1.Items.Clear()
    End Sub

    ''' <summary>
    ''' ERROR - сортировка
    ''' </summary>
    Public Sub sortByError()
        Try
            ListView1.Items.Clear()
            ListView1.Items.AddRange(itm)

            ListView1.Sorting = SortOrder.None
            Dim sorter As New lvItemComparer
            ListView1.ListViewItemSorter = sorter
            ListView1.Sort()

        Catch ex As Exception
            MsgBox("Dont sorting " & ex.Message)
        End Try
    End Sub

    Class lvItemComparer
        Implements IComparer
        Private col As Integer
        Public Sub New()
            col = 1
        End Sub
        Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare
            Dim xx() As String = (CType(x, ListViewItem).SubItems(col).Text).Split({"-"c})
            Dim yy() As String = (CType(y, ListViewItem).SubItems(col).Text).Split({"-"c})
            Dim sx As String = xx(0) & "-" & xx(xx.Length - 1)
            Dim sy As String = yy(0) & "-" & yy(yy.Length - 1)
            Return String.Compare(sx, sy)
        End Function
    End Class

End Class