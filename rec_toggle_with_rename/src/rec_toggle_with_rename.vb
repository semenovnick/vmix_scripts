' ================== INIT CONFIG ==================
    dim pathTitleName       as string = "FileNames"
    dim filename1TextField  as string = "Filename1.Text"
    dim filename2TextField  as string = "Filename2.Text"
    dim path1TextField      as string = "Path1.Text"
    dim path2TextField      as string = "Path2.Text"
    dim consoleTextField    as string = "Console.Text"
' ================== =========== ==================
' ==================== START TIMER ======================
    dim startTime       as Datetime  = System.DateTime.Now
    dim scriptTime      as TimeSpan
' ==================== ============ ======================
' ================== INTERNAL VARIABLES =========================
    dim config              as new system.xml.xmldocument
    config.loadxml(API.XML)
    dim pathTitleInput      as object = Input.Find(pathTitleName)
    dim fileNames()         as string = {   "", ""}
    dim newFileNames()      as string = {   pathTitleInput.Text(filename1TextField) , pathTitleInput.Text(filename2TextField)   }
    dim newPaths()          as string = {   pathTitleInput.Text(path1TextField)     , pathTitleInput.Text(path2TextField)       }
    dim humanRedable()      as string = {   "First", "Second"}

    dim consoleMessage as string = ""

    dim isRecording         as boolean
    dim isSecondRecorder    as boolean = false

    dim isError             as boolean = true
    dim isRename            as boolean = false
 
' ===============================================================
' ================== PART ONE. Checking recorder==================
    pathTitleInput.Text(consoleTextField) = ""
    dim recordingNode as XMLNode = config.SelectSingleNode("/vmix/recording")
    if recordingNode isnot nothing then       
        isRecording = Convert.ToBoolean(recordingNode.InnerText)
        if isRecording then 
            ' console.wl("DEBUG (line " & me.CurrentLine.tostring() & "): recording in progress.")
            dim fileIndex as integer = 0
            for Each fileName as string in fileNames              
                if recordingNode.Attributes.GetNamedItem("filename" & (fileIndex + 1).toString()) isnot nothing then
                if fileIndex = 1 then 
                    isSecondRecorder = true
                end if
                fileNames(fileIndex) = recordingNode.Attributes.GetNamedItem("filename" + (fileIndex + 1).toString()).Value
                ' console.wl("DEBUG (line " & me.CurrentLine.tostring() & "): " & humanRedable(fileIndex) & " recorder path: " &  fileNames(fileIndex))
            else 
                if fileIndex = 0 then
                    console.wl("ERROR (line " & me.CurrentLine.tostring() & "): There is no `filename " & (fileIndex + 1).toString() & "`in API`")
                else 
                    ' console.wl("DEBUG (line " & me.CurrentLine.tostring() & "): There is no second recorder.")
                end if 
            end if
            fileIndex += 1
            next
        else 
            ' console.wl("DEBUG (line " & me.CurrentLine.tostring() & "): recording stopped.")      
        end if 
    else 
        console.wl("ERROR (line " & me.CurrentLine.tostring() & "): There is no `recording` Node in API")
    end if
' ================== =========== ===============
' ================== Doing path works ========================================
    if isRecording then
        if not isSecondRecorder then
            ReDim Preserve newFileNames(0)
            ReDim Preserve fileNames(0)
            ReDim Preserve newPaths(0)
        end if
        
        isRename = true
        dim newFileIndex as integer = 0
        for Each newFileName as string in newFileNames 
            dim recDirectory        as string = Path.GetDirectoryName(fileNames(newFileIndex))
            dim extention           as string = Path.GetExtension(fileNames(newFileIndex))
            dim newDir              as string = ""
            
            if newPaths(newFileIndex) <> "" then
                newDir = Path.GetDirectoryName(newPaths(newFileIndex))
            end if 

            dim newOnlyFileName     as string = Path.GetFileNameWithoutExtension(newFileName)
            dim newFileExtention    as string  = Path.GetExtension(newFileName)

            if newFileExtention <> ""  AND newFileExtention <> extention then 
                console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): " & humanRedable(newFileIndex) & " recorder new file name has wrong exention (" & newFileExtention & "<>" & extention & ")!")
                console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): " & humanRedable(newFileIndex) & " recorder: Script can handle it!")
            end if 

            newFileExtention = extention

            if newOnlyFileName = "" then 
                console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): " & humanRedable(newFileIndex) & " recorder new filename is empty. Recorded file will not be renamed/moved!")
                consoleMessage += humanRedable(newFileIndex) & " recorder: new filename is empty" & Environment.NewLine
                isRename = false

            else 
                dim isInvalid       as boolean = false
                for each character  as string in Path.GetInvalidFileNameChars()
                    if newOnlyFileName.Contains(character) then
                        isInvalid = true
                    end if
                next
                if isInvalid then 
                    console.wl("ERROR (line " & me.CurrentLine.tostring() & "): " & humanRedable(newFileIndex) & " recorder > new filename has invalid characters! Recorded file will not be renamed/moved!")
                    consoleMessage += humanRedable(newFileIndex) & " recorder: new filename has  ivalid characters!" & Environment.NewLine
                    isRename = false
                end if 
            end if

            if newDir = "" then 
                console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): " & humanRedable(newFileIndex) & " recorder > new path is empty file remains in the same directory as default! ")
                newDir = recDirectory
            else 
                dim isInvalid       as boolean = false
                for each character  as string in Path.GetInvalidPathChars()
                    if newOnlyFileName.Contains(character) then
                        isInvalid = true
                    end if
                next
                if isInvalid then 
                    console.wl("ERROR (line " & me.CurrentLine.tostring() & "): " & humanRedable(newFileIndex) & " recorder > new path has ivalid characters!")
                    consoleMessage += humanRedable(newFileIndex) & " recorder: new path has has ivalid characters!" & Environment.NewLine
                    isRename = false
                else if not Directory.Exists(newDir) Then
                    console.wl("ERROR (line " & me.CurrentLine.tostring() & "): " & humanRedable(newFileIndex) & " recorder > new path not exists!")
                    consoleMessage += humanRedable(newFileIndex) & " recorder: new path not exists!" & Environment.NewLine
                    isRename = false
                end if

            end if
            if isRename then
                newFileNames(newFileIndex) = Path.Combine(newDir, newOnlyFileName & newFileExtention)
                dim counter         as integer = 1
                if File.Exists(newFileNames(newFileIndex)) then 
                    dim tempName    as string = Path.Combine(newDir, newOnlyFileName & newFileExtention)
                    while File.Exists(tempName)
                        tempName = Path.Combine(newDir, newOnlyFileName & "_" & counter.toString() & newFileExtention)
                        counter += 1
                    end while
                    console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): " & humanRedable(newFileIndex) & " recorder > file already exists making new name!")
                    newFileNames(newFileIndex) = tempName
                end if 
            else
                newFileNames(newFileIndex) = fileNames(newFileIndex)
            end if 

            ' console.wl(newFileNames(newFileIndex))
            newFileIndex += 1
        next

    end if     

' ================== PART TWO. DOING STUFF ==================

    if isRecording then 
        if isRename then
            ' console.wl("DEBUG (line " & me.CurrentLine.tostring() & "): Stopping record...")
            dim movingErr as boolean = false
            API.Function("StopRecording")
            consoleMessage = "Recording stopped!"
            dim newFileNameIndex as integer = 0 
            for each newFileName as string in newFileNames 
                consoleMessage += Environment.NewLine & "File moving to: " & newFileName & " ..."
                pathTitleInput.Text(consoleTextField) = consoleMessage
                try 
                    File.Move(fileNames(newFileNameIndex), newFileName)
                    ' sleep(5000)
                    ' throw New System.Exception("Something went wrong!")
                    consoleMessage += " OK!"
                Catch e As Exception
                    console.wl("ERROR (line " & me.CurrentLine.tostring() & "): File moving error: " & e.toString())
                    consoleMessage += "The process failed: " & e.ToString()
                    movingErr = true
                End Try
                ' console.wl("DEBUG (line " & me.CurrentLine.tostring() & "): File will be  renamed/moved: " & newFileName )              
            newFileNameIndex += 1
            next
            if not movingErr then
                consoleMessage += Environment.NewLine & "All files moved successfully!"
            else 
                consoleMessage += Environment.NewLine & "ERROR(s) occured!"
            end if 
            pathTitleInput.Text(consoleTextField) = consoleMessage
        else 
            console.wl("ERROR (line " & me.CurrentLine.tostring() & "): There ERROR! No Actions!")
        end if
    else
        ' console.wl("DEBUG (line " & me.CurrentLine.tostring() & "): Strarting record...") 
        API.Function("StartRecording")
        consoleMessage = "Recording started!"
    end if

' ================== =========== ===============

pathTitleInput.Text(consoleTextField) = consoleMessage

scriptTime = System.DateTime.Now - startTime
Console.WriteLine("Script done in " + scriptTime.TotalMilliseconds.ToString() + " milliseconds")