' ================== INIT CONFIG ==================
    dim pathTitleName       as string = "FileNamesMulti"

    dim consoleTextField    as string = "Console.Text"

    dim recOutTextFileds    as string() = {     "REC_Out1.Text" , "REC_Out2.Text"   , "REC_Out3.Text",  "REC_Out4.Text"     } 
    dim fileNamesFields     as string() = {     "Name_Out1.Text", "Name_Out2.Text"  , "Name_Out3.Text", "Name_Out4.Text"    }
    dim filePathFields      as string() = {     "Path_Out1.Text", "Path_Out2.Text"  , "Path_Out3.Text", "Path_Out4.Text"    }
' ================== =========== ==================
' ==================== START TIMER ======================
    dim startTime       as Datetime  = System.DateTime.Now
    dim scriptTime      as TimeSpan
' ==================== ============ ======================
' ================== INTERNAL VARIABLES =========================
    dim config              as new system.xml.xmldocument
    
    config.loadxml(API.XML)

    dim pathTitleInput      as object = Input.Find(pathTitleName)
    dim consoleMessage      as string = ""

    dim isRecording         as boolean

    dim isError             as boolean = true
    dim isRename            as boolean = false

    dim outputPaths         as string()        = { ""   , ""    , ""    , ""    }
    dim outputFullpaths     as string()        = { ""  , ""    , ""    , ""    }
    dim newFullPaths        as string()        = { ""   , ""    , ""    , ""    }
    dim outputRecords       as boolean()       = { false, false , false , false }

    dim recordingNode       as XMLNode = config.SelectSingleNode("/vmix/multiCorder")
    
    if recordingNode isnot nothing then       
        isRecording = Convert.ToBoolean(recordingNode.InnerText)
    else 
        console.wl("ERROR (line " & me.CurrentLine.tostring() & "): There is no `recording` Node in API")
    end if
    dim newFileNames(fileNamesFields.length -1 ) as string
    dim newPaths(filePathFields.length -1 ) as string

     dim humanRedable()      as string = {   "First", "Second", "Third", "Fourth"}

    for i as integer = 0 to fileNamesFields.length - 1
        newFileNames(i) = pathTitleInput.Text(fileNamesFields(i))
        newPaths(i) = pathTitleInput.Text(filePathFields(i))
    next
' 
' =========================  Getting Multirecorder Settings XML   ========================



    ' console.WL("====== SAVEOPEN METHOD ==========")
    dim presetPath as string = ""
    dim tempFolder as string = ".vmix_temp"
    dim tempPresetName as string = "temp.vmix"
    dim myDocuments as string = My.Computer.FileSystem.SpecialDirectories.MyDocuments
    dim presetNode as XMLNode
    dim vmixFile as new system.xml.xmldocument
    presetNode = config.SelectSingleNode("/vmix/preset")

    ' =================== SAVING PRESET ===========================

        if presetNode isnot Nothing then
        ' get path from api.xml
            ' console.WL("Preset is saved before")
            presetPath = presetNode.InnerText
        else 
        ' default path work
            If Not System.IO.Directory.Exists(myDocuments & "\" & tempFolder) Then
                System.IO.Directory.CreateDirectory(myDocuments & "\" & tempFolder)
            End If
            presetPath = myDocuments & "\" & tempFolder & "\" & tempPresetName
            ' console.WL("Seems that you are not save anything lets save it to MyDocuments\" & tempFolder)
        end if
        try
                API.Function("SavePreset", Value:= presetPath)
        catch e as Exception
                console.WL("ATTENTION ( line: " & me.CurrentLine.tostring()& "): SAVEOPEN METHOD. Some error when running API SavePreset... saving im MyDocuments")
                presetPath = myDocuments & "\" & tempPresetName                
                API.Function("SavePreset", Value:= presetPath)
        end try
        ' console.WL("Saved as: " & presetPath)  

    ' =================== OPENING PRESET ==========================

        
        try
            vmixFile.load(presetPath)
            dim version as string = vmixFile.SelectSingleNode("/XML/Version").InnerText
            console.WL("File loaded as version: " & version)
        catch e as System.IO.FileNotFoundException
            console.WL("ERROR ( line: " & me.CurrentLine.tostring()& "): SAVEOPEN METHOD. File " & presetPath & " not found")
            isError = true
        end try


' =========================   =========================================================



' ======================== ===============================================================
    dim multicorderSettingsFromFile as XMLNode      = vmixFile.SelectSingleNode("/XML/MultiCorderSettings")

    dim resultMulticorderSettings   as XMLNode      = multicorderSettingsFromFile
    dim multicorderSettingsNodes    as XMLNodeList  = resultMulticorderSettings.SelectNodes("*")

    for each node as XMLNode in multicorderSettingsNodes
        if node.Name.toString().split(".").length > 1 then
            dim source as string = node.Name.toString().split(".")(1)
            dim nodeType as string = node.Name.toString().split(".")(0)
            if source.ToLower().StartsWith("output") then 
                dim ouputIndex as integer = Convert.toInt32(source.ToLower().replace("output", ""))
                select nodeType
                    case "MultiCorderEnabled"
                        outputRecords( ouputIndex - 1 ) = Convert.ToBoolean(Convert.toInt32(node.InnerText))
                        if outputRecords( ouputIndex - 1 ) and not isRecording then 
                            pathTitleInput.Text(recOutTextFileds(ouputIndex - 1)) = "REC"
                        else 
                            pathTitleInput.Text(recOutTextFileds(ouputIndex - 1)) = ""
                        end if
                    case "MultiCorderFolder"
                        outputPaths( ouputIndex - 1 ) = node.InnerText
                end select
            else
                ' ================== INPUT PART ================
                ' in this variant there no input part. If you want you can write your own
                ' ==============================================
            end if
    end if
    next

' ============================= Making stuff with paths and names =============
    for outputIndex as integer = 0 to outputPaths.length - 1
        dim outputPath as string = outputPaths(outputIndex)
        if outputRecords(outputIndex) then
            ' ==================== finding filenames of recording files =============
            dim fileNameBase as string = "Output " & (outputIndex + 1).toString()
            dim dir as New DirectoryInfo(outputPath)
            dim files As FileInfo() = dir.GetFiles("MultiCorder* - "& fileNameBase &"*.*")
            if files.length > 0 then
                ' =====searhing for file with letest writeTime (which is recording now)
                    dim lastFile as FileInfo = files(0)
                    for each file as FileInfo in files
                            if DateTime.Compare(file.LastWriteTime, lastFile.LastWriteTime) >= 0 then
                                lastFile = file
                            end if      
                    next
                    outputFullpaths(outputIndex) = lastFile.FullName
                    ' console.wl("Latest is => " & lastFile.FullName)
                '============ ==============================================
                '============= NEW PATH WORK ===============================                    
                        isRename = true
                        dim newFileName         as string = newFileNames(outputIndex)
                        dim recDirectory        as string = Path.GetDirectoryName(outputFullpaths(outputIndex))
                        dim extention           as string = Path.GetExtension(outputFullpaths(outputIndex))
                        dim newDir              as string = ""
                        
                        if newPaths(outputIndex) <> "" then
                            newDir = Path.GetDirectoryName(newPaths(outputIndex))
                        end if 

                        dim newOnlyFileName     as string = Path.GetFileNameWithoutExtension(newFileName)
                        dim newFileExtention    as string  = Path.GetExtension(newFileName)

                        if newFileExtention <> ""  AND newFileExtention <> extention then 
                            console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): " & humanRedable(outputIndex) & " recorder new file name has wrong exention (" & newFileExtention & "<>" & extention & ")!")
                            console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): " & humanRedable(outputIndex) & " recorder: Script can handle it!")
                        end if 

                        newFileExtention = extention

                        if newOnlyFileName = "" then 
                            console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): " & humanRedable(outputIndex) & " recorder new filename is empty. Recorded file will not be renamed/moved!")
                            consoleMessage += humanRedable(outputIndex) & " recorder: new filename is empty" & Environment.NewLine
                            isRename = false

                        else 
                            dim isInvalid       as boolean = false
                            for each character  as string in Path.GetInvalidFileNameChars()
                                if newOnlyFileName.Contains(character) then
                                    isInvalid = true
                                end if
                            next
                            if isInvalid then 
                                console.wl("ERROR (line " & me.CurrentLine.tostring() & "): " & humanRedable(outputIndex) & " Output > new filename has invalid characters! Recorded file will not be renamed/moved!")
                                consoleMessage += humanRedable(outputIndex) & " Output: new filename has  ivalid characters!" & Environment.NewLine
                                isRename = false
                            end if 
                        end if

                        if newDir = "" then 
                            console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): " & humanRedable(outputIndex) & " Output > new path is empty file remains in the same directory as default! ")
                            newDir = recDirectory
                        else 
                            dim isInvalid       as boolean = false
                            for each character  as string in Path.GetInvalidPathChars()
                                if newOnlyFileName.Contains(character) then
                                    isInvalid = true
                                end if
                            next
                            if isInvalid then 
                                console.wl("ERROR (line " & me.CurrentLine.tostring() & "): " & humanRedable(outputIndex) & " Output > new path has ivalid characters!")
                                consoleMessage += humanRedable(outputIndex) & " Output: new path has has ivalid characters!" & Environment.NewLine
                                isRename = false
                            else if not Directory.Exists(newDir) Then
                                console.wl("ERROR (line " & me.CurrentLine.tostring() & "): " & humanRedable(outputIndex) & " Output > new path not exists!")
                                consoleMessage += humanRedable(outputIndex) & " Output: new path not exists!" & Environment.NewLine
                                isRename = false
                            end if

                        end if

                        if isRename then
                            newFullPaths(outputIndex) = Path.Combine(newDir, newOnlyFileName & newFileExtention)
                            dim counter         as integer = 1
                            if File.Exists(newFullPaths(outputIndex)) then 
                                dim tempFileName    as string = Path.Combine(newDir, newOnlyFileName & newFileExtention)
                                while File.Exists(tempFileName)
                                    tempFileName = Path.Combine(newDir, newOnlyFileName & "_" & counter.toString() & newFileExtention)
                                    counter += 1
                                end while
                                console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): " & humanRedable(outputIndex) & " Output > file already exists making new name!")
                                newFullPaths(outputIndex) = tempFileName
                            end if 
                        else
                            newFullPaths(outputIndex) = outputFullpaths(outputIndex)
                        end if 
                        ' console.wl(newFullPaths(outputIndex))                                       
                '===========================================================
            else 
                console.wl("ERROR (line " & me.CurrentLine.tostring() & "): " & "Output " & (outputIndex + 1).toString() & " no target file in recFolder ")
            end if
        end if
    next

' =============================================================================

' ============================ START / STOP Multirecorder =====================
if isRecording then 
        if isRename then
            ' console.wl("DEBUG (line " & me.CurrentLine.tostring() & "): Stopping record...")
            dim movingErr as boolean = false
            API.Function("StopMultiCorder")
            consoleMessage = "Recording stopped!"
            for newFilePathtIndex as integer = 0 to newFullPaths.length - 1
            dim newFullPathName as string = newFullPaths(newFilePathtIndex)
                if outputRecords(newFilePathtIndex) then
                    consoleMessage += Environment.NewLine & "File moving to: " & newFullPathName & " ..."
                    pathTitleInput.Text(consoleTextField) = consoleMessage
                    try 
                        File.Move(outputFullpaths(newFilePathtIndex), newFullPathName)
                        ' sleep(5000)
                        ' throw New System.Exception("Something went wrong!")
                        consoleMessage += " OK!"
                    Catch e As Exception
                        console.wl("ERROR (line " & me.CurrentLine.tostring() & "): File moving error: " & e.toString())
                        consoleMessage += "The process failed: " & e.ToString()
                        movingErr = true
                    End Try
                    ' console.wl("DEBUG (line " & me.CurrentLine.tostring() & "): File will be  renamed/moved: " & newFullPathName )              
                end if
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
        API.Function("StartMultiCorder")
        consoleMessage = "Recording started!"
    end if
' =============================================================================

pathTitleInput.Text(consoleTextField) = consoleMessage

scriptTime = System.DateTime.Now - startTime
Console.WriteLine("Script done in " + scriptTime.TotalMilliseconds.ToString() + " milliseconds")