Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Discord
Imports Discord.WebSocket
Module RepBot
    Sub Main(args As String())
        handler = New ConsoleEventDelegate(AddressOf ConsoleEventCallback)
        SetConsoleCtrlHandler(handler, True)
        ' Start main as an async sub
        MainAsync.GetAwaiter.GetResult()
    End Sub

    Public clientconfig As DiscordSocketConfig = New DiscordSocketConfig With {
        .TotalShards = 1
    }
    Public _client As DiscordShardedClient = New DiscordShardedClient(clientconfig)

    Sub New()
        ' Set console encoding for names with symbols like ♂️ and ♀️
        Console.OutputEncoding = Text.Encoding.UTF8
        ' Set our log, ready, timer, and message received functions
        AddHandler _client.Log, AddressOf LogAsync
        AddHandler _client.ShardReady, AddressOf ReadAsync
        AddHandler _client.MessageReceived, AddressOf MessageReceivedAsync
        AddHandler _client.ShardConnected, AddressOf ShardConnectedAsync
        AddHandler _client.ShardReady, AddressOf ShardReadyAsync
        AddHandler _client.UserJoined, AddressOf UserJoinedAsync

    End Sub

    <STAThread()>
    Public Async Function MainAsync() As Task
        ' Set thread
        Threading.Thread.CurrentThread.SetApartmentState(Threading.ApartmentState.STA)

        Await _client.LoginAsync(TokenType.Bot, "NzI1OTAxNDU5NzgyMjM4MzAw.XvVeag.RdTFjHkfQqpEhMKP3XZHIp9SUAM")

        ' Wait for the client to start
        Await _client.StartAsync
        Await Task.Delay(-1)
    End Function

    Private Async Function LogAsync(ByVal log As LogMessage) As Task(Of Task)
        ' Once loginasync and startasync finish we get the log message of "Ready" once we get that, we load everything else
        If log.ToString.Contains("Ready") Then
            Colorize("[GATEWAY]   " & log.ToString)

            Await _client.SetGameAsync(" god.")
        ElseIf log.ToString.Contains("gateway") Or log.ToString.Contains("unhandled") Then
        Else
            Colorize("[GATEWAY]   " & log.ToString) ' update console
        End If
        Return Task.CompletedTask
    End Function

    ' Async reader
    Private Async Function ReadAsync() As Task(Of Task)
        Return Task.CompletedTask
    End Function

    Private Async Function ShardConnectedAsync(ByVal shard As DiscordSocketClient) As Task(Of Task)
        Colorize("[SHARD]     #" & shard.ShardId + 1 & " connected! Guilds: " & shard.Guilds.Count & " Users: " & shard.Guilds.Sum(Function(x) x.MemberCount))
        Return Task.CompletedTask
    End Function

    Private Async Function ShardReadyAsync(ByVal shard As DiscordSocketClient) As Task(Of Task)
        Colorize("[SHARD]     #" & shard.ShardId + 1 & " ready! Guilds: " & shard.Guilds.Count & " Users: " & shard.Guilds.Sum(Function(x) x.MemberCount))
        Return Task.CompletedTask
    End Function

    Private Async Function UserJoinedAsync(ByVal member As SocketGuildUser) As Task
        Try
            Dim guild As SocketGuild = member.Guild
            Dim d As Date = member.CreatedAt.DateTime
            Dim t As Date = Date.Now
            Dim avatarurl As String = ""
            If member.GetAvatarUrl IsNot Nothing Then
                avatarurl = member.GetAvatarUrl
            Else
                avatarurl = member.GetDefaultAvatarUrl
            End If

            AddUser(member.Id)

            Colorize("[INFO]      UserID: " & member.Id & " Joined the server.")
        Catch ex As Exception
            Colorize("[ERROR]     " & ex.ToString)
        End Try
    End Function

    Private Async Function MessageReceivedAsync(ByVal message As SocketMessage) As Task
        Dim content As String = message.Content
        Dim author As IUser = message.Author
        Dim channel As IChannel = message.Channel
        Dim chn As SocketGuildChannel = message.Channel
        Dim guild As IGuild = chn.Guild
        Dim member As SocketGuildUser = author

        Dim prefix As String = My.Settings.prefix

        If content.ToLower.StartsWith(prefix) Then
            If CheckUser(author.Id) = False Then
                AddUser(member.Id)
            End If
            If content.ToLower.StartsWith(prefix & "ping") Then
                Dim ping1 As String = _client.Shards(0).Latency
                ' Dim ping2 As String = _client.Shards(1).Latency
                Dim builder As EmbedBuilder = New EmbedBuilder
                builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                builder.WithThumbnailUrl(My.Settings.botIcon)
                builder.WithColor(219, 172, 69)

                builder.WithDescription("Shard #1: " & ping1 & "ms | Guilds: " & _client.Shards(0).Guilds.Count & " Users: " & _client.Shards(0).Guilds.Sum(Function(x) x.MemberCount))

                Await message.Channel.SendMessageAsync("", False, builder.Build)

                Colorize("[INFO]      Shard #1: " & ping1 & "ms | Guilds: " & _client.Shards(0).Guilds.Count & " Users: " & _client.Shards(0).Guilds.Sum(Function(x) x.MemberCount))
            ElseIf content.ToLower.StartsWith(prefix & "help") Then
                Dim split As String() = content.Split(" ")
                If split.Count > 1 Then
                    If split(1).ToLower = "settings" Then
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Please use `" & prefix & "help` to see the main help menu any time." & Environment.NewLine & Environment.NewLine &
                                                    "**Current Prefix:** `" & prefix & "`")

                        builder.AddField("**Change bot prefix:**", "`" & prefix & "prefix [new prefix]`", True)
                        builder.AddField("**Change bot icon:**", "`" & prefix & "icon [image url]`", True)
                        builder.AddField("**Change bot name:**", "`" & prefix & "name [new name]`", True)
                        builder.AddField("**Change server ID:**", "`" & prefix & "server [new id]`", True)
                        builder.AddField("**Check Latency:**", "`" & prefix & "ping`", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    ElseIf split(1).ToLower = "user" Then
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Please use `" & prefix & "help` to see the main help menu any time." & Environment.NewLine & Environment.NewLine &
                                                    "**Current Prefix:** `" & prefix & "`")

                        builder.AddField("**View reputation:**", "`" & prefix & "profile`", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)

                    ElseIf split(1).ToLower = "admin" Then
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Please use `" & prefix & "help` to see the main help menu any time." & Environment.NewLine & Environment.NewLine &
                                                    "**Current Prefix:** `" & prefix & "`")

                        builder.AddField("**Give reputation:**", "`" & prefix & "addrep [@user] [amt]`", True)
                        builder.AddField("**Take reputation:**", "`" & prefix & "subrep [@user] [amt]`", True)
                        builder.AddField("**Give user bot admin:**", "`" & prefix & "giveadmin [@user]`", True)
                        builder.AddField("**Remove user bot admin:**", "`" & prefix & "takeadmin [@user]`", True)
                        builder.AddField("**Add role to list:**", "`" & prefix & "addrole [role name] [amt req]`", True)
                        builder.AddField("**Remove role from list:**", "`" & prefix & "subrole [role name]`", True)
                        builder.AddField("**Role list:**", "`" & prefix & "roles [page]`", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                Else
                    Dim builder As EmbedBuilder = New EmbedBuilder
                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                    builder.WithThumbnailUrl(My.Settings.botIcon)
                    builder.WithColor(219, 172, 69)

                    builder.WithDescription("Please use `" & prefix & "help` to see this menu any time." & Environment.NewLine & Environment.NewLine &
                                            "**Current Prefix:** `" & prefix & "`")

                    builder.AddField("**Settings Help:**", "`" & prefix & "help settings`", True)
                    builder.AddField("**User Help:**", "`" & prefix & "help user`", True)
                    builder.AddField("**Admin Help:**", "`" & prefix & "help admin`", True)

                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                End If
            ElseIf content.ToLower.StartsWith(prefix & "prefix") Then
                If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                    Dim newprefix As String() = content.Split(" ")
                    If newprefix.Count > 0 Then
                        My.Settings.prefix = newprefix(1)
                        My.Settings.Save()

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("The bot prefix has been updated!")

                        builder.AddField("**Current prefix: **", "`" & My.Settings.prefix & "`", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf CheckPerm(author.Id) = 100 Then
                    Dim newprefix As String() = content.Split(" ")
                    If newprefix.Count > 0 Then
                        My.Settings.prefix = newprefix(1)
                        My.Settings.Save()

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("The bot prefix has been updated!")

                        builder.AddField("**Current prefix: **", "`" & My.Settings.prefix & "`", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                Else
                    Dim builder As EmbedBuilder = New EmbedBuilder
                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                    builder.WithThumbnailUrl(My.Settings.botIcon)
                    builder.WithColor(111, 33, 39)

                    builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                End If
            ElseIf content.ToLower.StartsWith(prefix & "icon") Then
                If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                    Dim split As String() = content.Split(" ")
                    Dim newicon As String = Split(1)
                    If newicon.Length > 0 Then
                        If newicon.ToLower.EndsWith(".png") Or newicon.ToLower.Contains(".jpg") Or newicon.ToLower.Contains(".jpeg") Or newicon.ToLower.Contains(".gif") Or newicon.ToLower.Contains(".bmp") Then
                            My.Settings.botIcon = newicon
                            My.Settings.Save()

                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                            builder.WithThumbnailUrl(My.Settings.botIcon)
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("The bot icon has been updated!")

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    End If
                ElseIf CheckPerm(author.Id) = 100 Then
                    Dim split As String() = content.Split(" ")
                    Dim newicon As String = split(1)
                    If newicon.Length > 0 Then
                        If newicon.ToLower.EndsWith(".png") Or newicon.ToLower.Contains(".jpg") Or newicon.ToLower.Contains(".jpeg") Or newicon.ToLower.Contains(".gif") Or newicon.ToLower.Contains(".bmp") Then
                            My.Settings.botIcon = newicon
                            My.Settings.Save()

                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                            builder.WithThumbnailUrl(My.Settings.botIcon)
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("The bot icon has been updated!")

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    End If
                Else
                    Dim builder As EmbedBuilder = New EmbedBuilder
                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                    builder.WithThumbnailUrl(My.Settings.botIcon)
                    builder.WithColor(111, 33, 39)

                    builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                End If
            ElseIf content.ToLower.StartsWith(prefix & "server") Then
                If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                    Dim split As String() = content.Split(" ")
                    Dim newserver As String = split(1)
                    If newserver.Length > 0 Then
                        My.Settings.serverID = newserver
                        My.Settings.Save()

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("The bot icon has been updated!")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf CheckPerm(author.Id) = 100 Then
                    Dim split As String() = content.Split(" ")
                    Dim newserver As String = split(1)
                    If newserver.Length > 0 Then
                        My.Settings.serverID = newserver
                        My.Settings.Save()

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("The bot icon has been updated!")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                Else
                    Dim builder As EmbedBuilder = New EmbedBuilder
                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                    builder.WithThumbnailUrl(My.Settings.botIcon)
                    builder.WithColor(111, 33, 39)

                    builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                End If
            ElseIf content.ToLower.StartsWith(prefix & "name") Then
                If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                    Dim split As String() = content.Split(" ")
                    Dim newbotName As String = content.Remove(0, prefix.Length + 5)
                    If newbotName.Length > 0 Then
                        My.Settings.botName = newbotName
                        My.Settings.Save()

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("The bot name has been updated!")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf CheckPerm(author.Id) = 100 Then
                    Dim split As String() = content.Split(" ")
                    Dim newbotName As String = split(1)
                    If newbotName.Length > 0 Then
                        My.Settings.botName = newbotName
                        My.Settings.Save()

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("The bot name has been updated!")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                Else
                    Dim builder As EmbedBuilder = New EmbedBuilder
                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                    builder.WithThumbnailUrl(My.Settings.botIcon)
                    builder.WithColor(111, 33, 39)

                    builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                End If
            ElseIf content.ToLower.StartsWith(prefix & "giveadmin") Then
                If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                    If message.MentionedUsers.Count > 0 Then
                        UpdatePermission(message.MentionedUsers.FirstOrDefault.Id, "100")
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Permissions updated!")

                        builder.AddField("**Admin Granted:**", "`" & message.MentionedUsers.FirstOrDefault.Username & "`", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf CheckPerm(author.Id) = 100 Then
                    If message.MentionedUsers.Count > 0 Then
                        UpdatePermission(message.MentionedUsers.FirstOrDefault.Id, "100")
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Permissions updated!")

                        builder.AddField("**Admin Granted:**", "`" & message.MentionedUsers.FirstOrDefault.Username & "`", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                Else
                    Dim builder As EmbedBuilder = New EmbedBuilder
                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                    builder.WithThumbnailUrl(My.Settings.botIcon)
                    builder.WithColor(111, 33, 39)

                    builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                End If
            ElseIf content.ToLower.StartsWith(prefix & "takeadmin") Then
                If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                    If message.MentionedUsers.Count > 0 Then
                        UpdatePermission(message.MentionedUsers.FirstOrDefault.Id, "0")
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Permissions updated!")

                        builder.AddField("**Admin Stripped:**", "`" & message.MentionedUsers.FirstOrDefault.Username & "`", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf CheckPerm(author.Id) = 100 Then
                    If message.MentionedUsers.Count > 0 Then
                        UpdatePermission(message.MentionedUsers.FirstOrDefault.Id, "0")
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Permissions updated!")

                        builder.AddField("**Admin Stripped:**", "`" & message.MentionedUsers.FirstOrDefault.Username & "`", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                Else
                    Dim builder As EmbedBuilder = New EmbedBuilder
                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                    builder.WithThumbnailUrl(My.Settings.botIcon)
                    builder.WithColor(111, 33, 39)

                    builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                End If
            ElseIf content.ToLower.StartsWith(prefix & "profile") Then
                Dim rep As Integer = UserQuery(author.Id, "userRep")
                Dim avatar As String = ""

                Dim builder As EmbedBuilder = New EmbedBuilder
                builder.WithAuthor(author.Username & "'s Profile", My.Settings.botIcon)

                If Not author.GetAvatarUrl Is Nothing Then
                    avatar = author.GetAvatarUrl
                Else
                    avatar = author.GetDefaultAvatarUrl
                End If

                builder.WithThumbnailUrl(avatar)
                builder.WithColor(219, 172, 69)

                builder.WithDescription(author.Mention & "**Rep: " & rep & "**")

                Await message.Channel.SendMessageAsync("", False, builder.Build)
            ElseIf content.ToLower.StartsWith(prefix & "roles") Then
                Dim page As Integer = Num(content)
                Dim sbuilder As New StringBuilder
                If page > 0 Then
                    If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                        sbuilder = LoadRolesList(page)

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription(sbuilder.ToString)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    ElseIf CheckPerm(author.Id) = 100 Then
                        sbuilder = LoadRolesList(page)

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription(sbuilder.ToString)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(111, 33, 39)

                        builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                End If
            ElseIf content.ToLower.StartsWith(prefix & "addrole") Then
                Dim split As String() = content.Split(" ")
                If split.Count > 0 Then
                    If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                        Dim newrole As String = split(1)
                        Dim rolereq As Integer = split(2)

                        AddRole(newrole, rolereq)

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Role Added!")

                        builder.AddField("**Role:**", "`" & newrole & "` will be granted with rep of `" & rolereq & "`.", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    ElseIf CheckPerm(author.Id) = 100 Then
                        Dim newrole As String = split(1)
                        Dim rolereq As Integer = split(2)

                        AddRole(newrole, rolereq)

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Role Added!")

                        builder.AddField("**Role:**", "`" & newrole & "` will be granted with rep of `" & rolereq & "`.", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(111, 33, 39)

                        builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                End If
            ElseIf content.ToLower.StartsWith(prefix & "subrole") Then
                Dim split As String() = content.Split(" ")
                If split.Count > 0 Then
                    If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                        Dim newrole As String = split(1)

                        RemoveRole(newrole)

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Role Removed!")

                        builder.AddField("**Role:**", "`" & newrole & "` has been removed from the database.", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    ElseIf CheckPerm(author.Id) = 100 Then
                        Dim newrole As String = split(1)

                        RemoveRole(newrole)

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Role Removed!")

                        builder.AddField("**Role:**", "`" & newrole & "` has been removed from the database.", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                        builder.WithThumbnailUrl(My.Settings.botIcon)
                        builder.WithColor(111, 33, 39)

                        builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                End If
            ElseIf content.ToLower.StartsWith(prefix & "addrep") Then
                Try
                    Dim split As String() = content.Split(" ")
                    If split.Count > 0 Then
                        Dim addrep As Integer = split(2)
                        If message.MentionedUsers.Count > 0 Then
                            If addrep > 0 Then
                                If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                                    Dim userrep As Integer = UserQuery(author.Id, "userRep")
                                    Dim newrep As Integer = userrep + addrep
                                    UpdateUser(author.Id, "userRep", newrep)

                                    Dim roleup As Boolean = False
                                    Dim newrole As String = GetRole(newrep)
                                    Dim oldrole As String = UserQuery(author.Id, "userRole")
                                    If RoleQuery(newrole, "repLimit") <= newrep Then
                                        If newrole <> oldrole Then
                                            roleup = True
                                            UpdateUser(author.Id, "userRole", newrole)
                                            Dim role = _client.GetGuild(My.Settings.serverID).Roles.FirstOrDefault(Function(x) x.Name = newrole)
                                            Await member.AddRoleAsync(role)

                                            If oldrole <> "None" Then
                                                Dim subrole = _client.GetGuild(My.Settings.serverID).Roles.FirstOrDefault(Function(x) x.Name = oldrole)
                                                Await member.RemoveRoleAsync(subrole)
                                            End If
                                        End If
                                    End If

                                    Dim builder As EmbedBuilder = New EmbedBuilder
                                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                                    builder.WithThumbnailUrl(My.Settings.botIcon)
                                    builder.WithColor(219, 172, 69)

                                    builder.WithDescription("You have added " & addrep & " reputation to " & message.MentionedUsers.FirstOrDefault.Username & "!")

                                    If roleup = True Then
                                        builder.AddField("Role has been granted!", "`" & newrole & "`", True)
                                    End If

                                    builder.AddField("**User's New Reputation:**", "`" & newrep & "`.", True)

                                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                                ElseIf CheckPerm(author.Id) = 100 Then
                                    Dim userrep As Integer = UserQuery(author.Id, "userRep")
                                    Dim newrep As Integer = userrep + addrep
                                    UpdateUser(author.Id, "userRep", newrep)

                                    Dim roleup As Boolean = False
                                    Dim newrole As String = GetRole(newrep)
                                    Dim oldrole As String = UserQuery(author.Id, "userRole")
                                    If RoleQuery(newrole, "repLimit") <= newrep Then
                                        If newrole <> oldrole Then
                                            roleup = True
                                            UpdateUser(author.Id, "userRole", newrole)
                                            Dim role = _client.GetGuild(My.Settings.serverID).Roles.FirstOrDefault(Function(x) x.Name = newrole)
                                            Await member.AddRoleAsync(role)

                                            If oldrole <> "None" Then
                                                Dim subrole = _client.GetGuild(My.Settings.serverID).Roles.FirstOrDefault(Function(x) x.Name = oldrole)
                                                Await member.RemoveRoleAsync(subrole)
                                            End If
                                        End If
                                    End If

                                    Dim builder As EmbedBuilder = New EmbedBuilder
                                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                                    builder.WithThumbnailUrl(My.Settings.botIcon)
                                    builder.WithColor(219, 172, 69)

                                    builder.WithDescription("You have added " & addrep & " reputation to " & message.MentionedUsers.FirstOrDefault.Username & "!")

                                    If roleup = True Then
                                        builder.AddField("Role has been granted!", "`" & newrole & "`", True)
                                    End If

                                    builder.AddField("**User's New Reputation:**", "`" & newrep & "`.", True)

                                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                                Else
                                    Dim builder As EmbedBuilder = New EmbedBuilder
                                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                                    builder.WithThumbnailUrl(My.Settings.botIcon)
                                    builder.WithColor(111, 33, 39)

                                    builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                                End If
                            End If
                        End If
                    End If
                Catch ex As Exception
                    Console.WriteLine(ex.ToString)
                End Try
            ElseIf content.ToLower.StartsWith(prefix & "subrep") Then
                Try
                    Dim split As String() = content.Split(" ")
                    If split.Count > 0 Then
                        Dim addrep As Integer = split(2)
                        If message.MentionedUsers.Count > 0 Then
                            If addrep > 0 Then
                                If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                                    Dim userrep As Integer = UserQuery(author.Id, "userRep")
                                    Dim newrep As Integer = userrep - addrep
                                    If newrep >= 100 Then
                                        UpdateUser(author.Id, "userRep", newrep)

                                        Dim roleup As Boolean = False
                                        Dim newrole As String = LowestRole(newrep)
                                        Dim oldrole As String = UserQuery(author.Id, "userRole")
                                        Dim oldrolerep As Integer = RoleQuery(oldrole, "repLimit")

                                        If oldrolerep > newrep Then
                                            roleup = True
                                            UpdateUser(author.Id, "userRole", newrole)
                                            Dim role = _client.GetGuild(My.Settings.serverID).Roles.FirstOrDefault(Function(x) x.Name = newrole)
                                            Await member.AddRoleAsync(role)

                                            If oldrole <> "None" Then
                                                Dim subrole = _client.GetGuild(My.Settings.serverID).Roles.FirstOrDefault(Function(x) x.Name = oldrole)
                                                Await member.RemoveRoleAsync(subrole)
                                            End If
                                        End If

                                        Dim builder As EmbedBuilder = New EmbedBuilder
                                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                                        builder.WithThumbnailUrl(My.Settings.botIcon)
                                        builder.WithColor(219, 172, 69)

                                        builder.WithDescription("You have removed " & addrep & " reputation from " & message.MentionedUsers.FirstOrDefault.Username & "!")

                                        If roleup = True Then
                                            builder.AddField("Role has been removed!", "`" & oldrole & "`", True)
                                        End If

                                        builder.AddField("**User's New Reputation:**", "`" & newrep & "`.", True)

                                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                                    Else
                                        If newrep <= 0 Then newrep = 0
                                        UpdateUser(author.Id, "userRep", newrep)

                                        Dim roleup As Boolean = False
                                        Dim newrole As String = LowestRole(newrep)
                                        Dim oldrole As String = UserQuery(author.Id, "userRole")
                                        Dim oldrolerep As Integer = RoleQuery(oldrole, "repLimit")

                                        If oldrole <> "None" Then
                                            If oldrolerep > newrep Then
                                                roleup = True
                                                UpdateUser(author.Id, "userRole", newrole)
                                                Dim role = _client.GetGuild(My.Settings.serverID).Roles.FirstOrDefault(Function(x) x.Name = newrole)
                                                Await member.AddRoleAsync(role)

                                                Dim subrole = _client.GetGuild(My.Settings.serverID).Roles.FirstOrDefault(Function(x) x.Name = oldrole)
                                                Await member.RemoveRoleAsync(subrole)
                                            End If
                                        End If

                                        Dim builder As EmbedBuilder = New EmbedBuilder
                                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                                        builder.WithThumbnailUrl(My.Settings.botIcon)
                                        builder.WithColor(219, 172, 69)

                                        builder.WithDescription("You have removed " & addrep & " reputation from " & message.MentionedUsers.FirstOrDefault.Username & "!")

                                        If roleup = True Then
                                            builder.AddField("Role has been removed!", "`" & oldrole & "`", True)
                                        End If

                                        builder.AddField("**User's New Reputation:**", "`" & newrep & "`.", True)

                                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                                    End If
                                ElseIf CheckPerm(author.Id) = 100 Then
                                    Dim userrep As Integer = UserQuery(author.Id, "userRep")
                                    Dim newrep As Integer = userrep - addrep
                                    If newrep >= 100 Then
                                        UpdateUser(author.Id, "userRep", newrep)

                                        Dim roleup As Boolean = False
                                        Dim newrole As String = GetRole(newrep)
                                        Dim oldrole As String = UserQuery(author.Id, "userRole")
                                        Dim oldrolerep As Integer = RoleQuery(oldrole, "repLimit")

                                        If oldrolerep < newrep Then
                                            If oldrole <> "None" Then
                                                roleup = True
                                                UpdateUser(author.Id, "userRole", newrole)
                                                Dim role = _client.GetGuild(My.Settings.serverID).Roles.FirstOrDefault(Function(x) x.Name = newrole)
                                                Await member.AddRoleAsync(role)
                                            End If
                                        End If

                                        Dim builder As EmbedBuilder = New EmbedBuilder
                                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                                        builder.WithThumbnailUrl(My.Settings.botIcon)
                                        builder.WithColor(219, 172, 69)

                                        builder.WithDescription("You have removed " & addrep & " reputation from " & message.MentionedUsers.FirstOrDefault.Username & "!")

                                        If roleup = True Then
                                            builder.AddField("Role has been granted!", "`" & newrole & "`", True)
                                        End If

                                        builder.AddField("**User's New Reputation:**", "`" & newrep & "`.", True)

                                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                                    Else
                                        If newrep >= 0 Then newrep = 0
                                        UpdateUser(author.Id, "userRep", newrep)
                                        Dim builder As EmbedBuilder = New EmbedBuilder
                                        builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                                        builder.WithThumbnailUrl(My.Settings.botIcon)
                                        builder.WithColor(219, 172, 69)

                                        builder.WithDescription("You have removed " & addrep & " reputation from " & message.MentionedUsers.FirstOrDefault.Username & "!")

                                        builder.AddField("**User's New Reputation:**", "`0`.", True)

                                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                                    End If
                                Else
                                    Dim builder As EmbedBuilder = New EmbedBuilder
                                    builder.WithAuthor(My.Settings.botName, My.Settings.botIcon)
                                    builder.WithThumbnailUrl(My.Settings.botIcon)
                                    builder.WithColor(111, 33, 39)

                                    builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                                End If
                            End If
                        End If
                    End If
                Catch ex As Exception
                    Console.WriteLine(ex.ToString)
                End Try
            End If
        Else
            If CheckUser(author.Id) = False Then
                AddUser(member.Id)
            End If
        End If
    End Function

    Private Function ConsoleEventCallback(ByVal eventType As Integer) As Boolean
        Select Case eventType
            Case 0
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
            Case 1
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
            Case 2
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
            Case 5
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
            Case 6
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
        End Select
        Return False
    End Function

    Dim handler As ConsoleEventDelegate
    Private Delegate Function ConsoleEventDelegate(ByVal eventType As Integer) As Boolean
    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function SetConsoleCtrlHandler(ByVal callback As ConsoleEventDelegate, ByVal add As Boolean) As Boolean
    End Function
End Module