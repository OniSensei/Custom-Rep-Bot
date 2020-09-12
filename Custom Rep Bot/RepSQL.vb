Imports System.Data.SQLite
Imports System.Text

Module RepSQL
    Public Sub AddUser(ByVal userID As String)
        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\RepBotDB.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "INSERT INTO users (userID) VALUES (@userID);"
        sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID
        sqlite_cmd.ExecuteNonQuery()

        Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)
    End Sub

    Public Function CheckUser(ByVal userID As String) As Boolean
        Dim userexists As Boolean = False

        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\RepBotDB.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "SELECT count(*) FROM users WHERE userID = @userID;"
        sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID

        Dim hasrows = Convert.ToInt32(sqlite_cmd.ExecuteScalar())

        If hasrows >= 1 Then
            userexists = True
        End If

        Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)

        Return userexists
    End Function

    Public Function CheckPerm(ByVal userID As String) As Integer
        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\RepBotDB.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "SELECT userAccess FROM users WHERE userID = @userID;"
        sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID

        Dim accessLvl As Integer = sqlite_cmd.ExecuteScalar()

        Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)

        Return accessLvl
    End Function

    Public Sub UpdatePermission(ByVal userid As String, ByVal rank As String)
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\RepBotDB.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            sqlite_cmd.CommandText = "UPDATE users SET userAccess = @value WHERE userID = @user;"
            sqlite_cmd.Parameters.Add("@value", SqlDbType.Int).Value = rank
            sqlite_cmd.Parameters.Add("@user", SqlDbType.VarChar, 50).Value = userid
            sqlite_cmd.ExecuteNonQuery()

            Colorize("[SQL]       [" & userid & "] | " & sqlite_cmd.CommandText)
        Catch ex As Exception
            Colorize("[ERROR]     [" & userid & "] | " & ex.ToString)
        End Try
    End Sub

    Public Function UserQuery(ByVal userID As String, ByVal clm As String) As String
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\RepBotDB.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            sqlite_cmd.CommandText = "SELECT " & clm & " FROM users WHERE userID = @userID;"
            sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID

            Dim profile As String = sqlite_cmd.ExecuteScalar()

            Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)

            Return profile
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
    End Function

    Public Function LoadRolesList(ByVal limit As Integer) As StringBuilder
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\RepBotDB.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            limit = 10 * (limit - 1)

            sqlite_cmd.CommandText = "SELECT repName FROM reputations LIMIT " & limit & ",10;"

            Dim dr = sqlite_cmd.ExecuteReader()
            Dim sbuilder As New StringBuilder
            While dr.Read
                sbuilder.Append(dr("repName").ToString).AppendLine()
            End While

            Return sbuilder
            Colorize("[SQL]       " & sqlite_cmd.CommandText)
        Catch ex As Exception
            Colorize("[ERROR]     " & ex.ToString)
        End Try
    End Function

    Public Sub AddRole(ByVal rolename As String, ByVal rolelimit As Integer)
        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\RepBotDB.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "INSERT INTO reputations (repName, repLimit) VALUES (@name, @limit);"
        sqlite_cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = rolename
        sqlite_cmd.Parameters.Add("@limit", SqlDbType.Int).Value = rolelimit
        sqlite_cmd.ExecuteNonQuery()

        Colorize("[SQL]       [" & rolename & "] | " & sqlite_cmd.CommandText)
    End Sub

    Public Sub RemoveRole(ByVal rolename As String)
        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\RepBotDB.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "DELETE FROM reputations WHERE repName = @name;"
        sqlite_cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = rolename
        sqlite_cmd.ExecuteNonQuery()

        Colorize("[SQL]       [" & rolename & "] | " & sqlite_cmd.CommandText)
    End Sub

    Public Sub UpdateUser(ByVal userID As String, ByVal clm As String, ByVal cval As String)
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\RepBotDB.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            sqlite_cmd.CommandText = "UPDATE users SET " & clm & " = @cval WHERE userID = @userID;"
            sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID
            sqlite_cmd.Parameters.Add("@cval", SqlDbType.VarChar, 50).Value = cval
            sqlite_cmd.ExecuteNonQuery()

            Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)
        Catch ex As Exception
            Colorize("[ERROR]     [" & userID & "] | " & ex.ToString)
        End Try
    End Sub

    Public Function RoleCount() As Integer
        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\RepBotDB.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "SELECT count(*) FROM reputations;"

        Dim rc As Integer = Convert.ToInt32(sqlite_cmd.ExecuteScalar())

        Colorize("[SQL]       " & sqlite_cmd.CommandText)

        Return RoleCount
    End Function

    Public Function RoleQuery(ByVal roleName As String, ByVal clm As String) As String
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\RepBotDB.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            sqlite_cmd.CommandText = "SELECT " & clm & " FROM reputations WHERE repName = @name;"
            sqlite_cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = roleName

            Dim profile As String = sqlite_cmd.ExecuteScalar()

            Colorize("[SQL]       [" & roleName & "] | " & sqlite_cmd.CommandText)

            Return profile
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
    End Function

    Public Function GetRole(ByVal userRep As Integer) As String
        Dim rolename As String = ""

        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\RepBotDB.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "SELECT repName FROM reputations ORDER BY ABS(@userRep - repLimit) LIMIT 1;"
        sqlite_cmd.Parameters.Add("@userRep", SqlDbType.Int).Value = userRep

        rolename = sqlite_cmd.ExecuteScalar()

        Colorize("[SQL]       " & sqlite_cmd.CommandText)

        Return rolename
    End Function

    Public Function LowestRole(ByVal repLimit As Integer) As String
        Dim rolename As String = ""

        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\RepBotDB.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "SELECT repName FROM reputations WHERE repLimit >= 0 AND repLimit <= @userRep ORDER BY repLimit DESC LIMIT 1;"
        sqlite_cmd.Parameters.Add("@userRep", SqlDbType.Int).Value = repLimit

        rolename = sqlite_cmd.ExecuteScalar()

        Colorize("[SQL]       " & sqlite_cmd.CommandText)

        Return rolename
    End Function
End Module
