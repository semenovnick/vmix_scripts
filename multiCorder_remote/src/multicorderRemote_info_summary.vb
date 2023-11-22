' ================== INIT CONFIG ==================
    dim monitorInputName       as string = "MultiMonitor"
    dim RECTextField        as string = "REC.Text"
    dim inputsTextField     as string = "Inputs.Text"
    dim foldersTextField    as string = "Folders.Text"

    dim codecTextField         as string = "Codec.Text"
    dim audioSourceTextField      as string = "Audio_Source.Text"
    dim intervalTextField     as string = "Interval.Text"
    dim fileFormatTextField     as string = "File_format.Text"
    dim wavTextField     as string = "IsWAV.Text"
' ================== =========== ==================

' ==================== START TIMER ======================
    dim startTime       as Datetime  = System.DateTime.Now
    dim scriptTime      as TimeSpan
' ==================== ============ ======================
' ================== =========== ================== SetTextColour&Input=MultiMonitor&SelectedName=REC.Text&Value=%23FF0000

    dim currentSettings as object               '  vMIX MySettings OBJECT
    dim multiCorderSettingsNode as XMLNode      ' multicorder settings NODE

    dim isError as boolean = false

    dim RECText         as string = ""
    dim inputsText      as string = ""
    dim foldersText     as string = ""

    dim isRecording     as boolean = false
    dim config              as new system.xml.xmldocument
    config.loadxml(API.XML)

    dim monitorInput as object = Input.Find(monitorInputName)

    dim recordingNode       as XMLNode = config.SelectSingleNode("/vmix/multiCorder")
    
    if recordingNode isnot nothing then       
        isRecording = Convert.ToBoolean(recordingNode.InnerText)
    else 
        console.wl("ERROR (line " & me.CurrentLine.tostring() & "): There is no `recording` Node in API")
    end if

    dim codecText         as string = ""
    dim audioSourceText      as string = ""
    dim intervalText     as string = ""
    dim fileFormatText     as string = ""
    dim wavText     as string = ""



' ========================= GET CURRENT SETTINGS ============================
    if not isError then
        try 
            dim vmixAssemblyTemp As Reflection.Assembly = Reflection.Assembly.GetEntryAssembly()            
            dim mySettingsTypeName as string = "vMix" & "." & "My" & "." & "MySettings"
            dim mySettingsType as type = vmixAssemblyTemp.gettype(mySettingsTypeName)
            Dim getSettingsMethod As Reflection.MethodInfo = mySettingsType.GetMethod("get_a")
            Dim mySettingsInstance As object = activator.CreateInstance(mySettingsType)
            currentSettings = getSettingsMethod.invoke(mySettingsInstance, Nothing)

            Dim mySettingsXML As XmlDocument = New XmlDocument()
            multiCorderSettingsNode = mySettingsXML.CreateElement("MultiCorderSettings")
            if currentSettings.MultiCorderSettings isnot nothing then 
                multiCorderSettingsNode.InnerXml = currentSettings.MultiCorderSettings
                console.wl("DEBUG (line " & me.CurrentLine.tostring() & "): Multicorder settings loaded successfully")
                isError = false
            else 
                isError = true
                console.wl("ERROR (line " & me.CurrentLine.tostring() & "): currentSettings.MultiCorderSettings is NOTHING.")
            end if   
        catch e as Exception
                console.wl("ERROR (line " & me.CurrentLine.tostring() & "): ERROR in GET Multicorder Settings part: " & e.toString())
        end try

    else
        console.wl("ERROR (line " & me.CurrentLine.tostring() & "): Error with target name. Stopping script...")
    end if

' ====================================

' ====================== UPDATING FIELDS ============================

    dim multicorderSettingsNodes    as XMLNodeList  = multiCorderSettingsNode.SelectNodes("*")
    ' ============= TYPE CONSTANTS ===========
        dim type_INPUT   as integer = 1
        dim type_OUTPUT as integer = 0
        dim type_DELETED    as integer = -1
    ' ===================================

    dim inputType as integer = type_DELETED
    
    for each node as XMLNode in multicorderSettingsNodes
        ' ========================== 
        if node.Name.toString().split(".").length > 1 then
        ' ==================== on/off + FOLDERS ============================
            dim source as string = node.Name.toString().split(".")(1)
            dim nodeType as string = node.Name.toString().split(".")(0)

            if source.ToLower().StartsWith("output") then 
                inputType = type_OUTPUT
            else
                dim sourceNode as XMLNode = config.SelectSingleNode("/vmix/inputs/input[@key=""" & source & """]")
                if sourceNode isnot nothing then 
                    inputType = type_INPUT
                    source = sourceNode.Attributes("shortTitle").Value
                else
                    inputType = type_DELETED
                end if
            end if

            if inputType <> type_DELETED then
                select nodeType
                        case "MultiCorderEnabled"
                            if node.InnerText = 1 then                      
                                if isRecording then 
                                    RECText += "REC" & Environment.NewLine 
                                else 
                                    RECText += "ON" & Environment.NewLine 
                                end if
                            else 
                                RECText += "" & Environment.NewLine
                            end if
                        case "MultiCorderFolder"
                                foldersText += node.InnerText & Environment.NewLine
                                inputsText += source & Environment.NewLine
                    end select
                end if
        else
        ' ===================== OTHER SETTINGS ========================
            select node.Name
                case "CodecDevice"
                case "MPEGBitRate"
                case "AudioBitRate"
                case "MultiCorderAudioSource"
                    select node.InnerText
                        case "0"
                            audioSourceText = "INPUT"
                        case "1"
                            audioSourceText = "MASTER"
                        case "2"
                            audioSourceText = "NONE"
                    end select  
                case "Interval"
                    intervalText = node.InnerText
                case "MultiCorderVersion"
                case "SelectedTab"
                    select node.InnerText
                        case "0"
                            codecText = "AVI"
                        case "1"
                            codecText = "MPEG2"
                        case "2"
                            codecText = "WMV"
                        case "3"
                            codecText = "WMVStreaming"
                        case "4"
                            codecText = "H.264"
                        case "5"
                            codecText = "FFMPEG"
                        case "6"
                            codecText = "VMIX"
                    end select                                                                                                                                                            
                case "MP4Format"
                case "VideoFileFormat"
                    select node.InnerText
                        case "0"
                            fileFormatText = "AVI"
                        case "10"
                            fileFormatText = "MKV"
                        case "20"
                            fileFormatText = "TS"
                        case "30"
                            fileFormatText = "MPG"
                        case "80"
                            fileFormatText = "MP4"
                    end select                    
                case "ShowAllInputs"
                case "AudioRecordingFormat"
                    select node.InnerText
                        case "0"
                            wavText = "NO"
                        case "1"
                            wavText = "YES"
                    end select                    
                case "MultiCorderPreserveFormat"
            end select

        end if
    next

' ===================================================================
' ===================================================================
    console.wl("================")
    monitorInput.text(RECTextField) = RECText
    monitorInput.text(inputsTextField) = inputsText
    monitorInput.text(foldersTextField) = foldersText

    monitorInput.text(codecTextField) = codecText
    monitorInput.text(audioSourceTextField) = audioSourceText
    monitorInput.text(intervalTextField) = intervalText
    monitorInput.text(fileFormatTextField) = fileFormatText
    monitorInput.text(wavTextField) = wavText


    if isRecording then
        API.Function("SetTextColour", Input:=monitorInputName, SelectedName:=RECTextField, Value:="#FF0000")
    else 
        API.Function("SetTextColour", Input:=monitorInputName, SelectedName:=RECTextField, Value:="#FFFFFF")
    end if
' ===================================================================