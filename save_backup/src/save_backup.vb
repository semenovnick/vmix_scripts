' ===================== USER DEFINED SETTINGS =======================================================

    dim backupFolderName as string = "BACKUP"
    dim backupTemplate as string = "$NAME-backup_$DAY_$MONTH_$mSEC"

    ' ==================================================================
    '   $NAME - project name
    '   $YEAR - four digits year
    '   $YY - two digits year
    '   $MONTH    
    '   $DAY - number of date
    '   $HOUR
    '   $MIN 
    '   $SEC
    '   $mSEC
    ' ==================================================================

' ===================== =============================== =============================================
    ' ==================== START TIMER ======================
        dim startTime   as Datetime  = System.DateTime.Now
        dim scriptTime  as TimeSpan
    ' ==================== ============ ======================
' ===================== =============================== =============================================
    dim config as new system.xml.xmldocument

    config.loadxml(API.XML)

    
    dim presetFullPath as string = ""
    dim backupFullPath as string = ""
    dim tempFolder as string = ".vmix_temp"
    dim tempName as string = "temp.vmix"
    dim myDocuments as string = My.Computer.FileSystem.SpecialDirectories.MyDocuments
    dim presetNode as XMLNode

    presetNode = config.SelectSingleNode("/vmix/preset")

    
    ' =================== SAVING PRESET ===========================

        if presetNode isnot Nothing then
        ' get path from api.xml
            ' console.WL("Preset is saved before")
            presetFullPath = presetNode.InnerText
        else 
        ' default path work
            If Not System.IO.Directory.Exists(Path.combine(myDocuments,tempFolder)) Then
                System.IO.Directory.CreateDirectory(Path.combine(myDocuments,tempFolder))
            End If
            presetFullPath = Path.Combine(Path.Combine(myDocuments, tempFolder), tempName)
            ' console.WL("Seems that you are not save anything lets save it to MyDocuments\" & tempFolder)
        end if

        dim presetDirectory as string = Path.GetDirectoryName(presetFullPath)
        dim presetExtention as string = Path.GetExtension(presetFullPath)
        dim presetName as string = Path.GetFileNameWithoutExtension(presetFullPath) 
        
        If Not System.IO.Directory.Exists(Path.combine(presetDirectory,backupFolderName)) Then
                System.IO.Directory.CreateDirectory(Path.combine(presetDirectory,backupFolderName))
                
        End If

        dim backupDirectory as string = Path.combine(presetDirectory,backupFolderName)
        dim backupName As String = backupTemplate.Replace("$NAME", presetName).Replace("$YEAR", startTime.toString("yyyy")).Replace("$YY", startTime.toString("yy")).Replace("$MONTH", startTime.toString("MM")).Replace("$DAY", startTime.toString("dd")).Replace("$HOUR", startTime.toString("HH")).Replace("$MIN", startTime.toString("mm")).Replace("$SEC", startTime.toString("ss")).Replace("$mSEC", startTime.toString("fff")) & presetExtention                
        ' console.wl(backupName)
        backupFullPath = Path.Combine(backupDirectory, backupName )
        try
                API.Function("SavePreset", Value:= backupFullPath)
                API.Function("SavePreset", Value:= presetFullPath)
        catch e as Exception
                console.WL("ATTENTION ( line: " & me.CurrentLine.tostring()& "): Some error when running API SavePreset... saving im MyDocuments")
                presetFullPath = Path.combine(myDocuments,tempFolder)              
                API.Function("SavePreset", Value:= presetFullPath)
        end try
' ===================== =============================== =============================================


scriptTime = System.DateTime.Now - startTime
Console.WriteLine("Script done in " + scriptTime.TotalMilliseconds.ToString() + " milliseconds")