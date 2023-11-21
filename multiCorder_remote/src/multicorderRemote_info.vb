' ================== INIT CONFIG ==================
    dim pathTitleName       as string = "MultiMonitor"
    dim RECTextField  as string = "REC.Text"
    dim inputsTextField  as string = "Inputs.Text"
    dim foldersTextField      as string = "Folders.Text"
    dim path2TextField      as string = "Path2.Text"
    dim consoleTextField    as string = "Console.Text"
' ================== =========== ================== SetTextColour&Input=MultiMonitor&SelectedName=REC.Text&Value=%23FF0000

    try 
        dim vmixAssemblyTemp As Reflection.Assembly = Reflection.Assembly.GetEntryAssembly()            
        dim mySettingsTypeName as string = "vMix" & "." & "My" & "." & "MySettings"
        dim mySettingsType as type = vmixAssemblyTemp.gettype(mySettingsTypeName)
        Dim mySettingsObj As object = activator.CreateInstance(mySettingsType)   
        Dim mySettingsXML As XmlDocument = New XmlDocument()
        dim multiCorderSettingsNode as XMLNode = mySettingsXML.CreateElement("MultiCorderSettings")
        if mySettingsObj.MultiCorderSettings isnot nothing then 

            multiCorderSettingsNode.InnerXml = mySettingsObj.MultiCorderSettings

            console.wl(multiCorderSettingsNode.SelectSingleNode("AudioRecordingFormat").InnerText) 
            if multiCorderSettingsNode.SelectSingleNode("AudioRecordingFormat").InnerText = 0 then

                multiCorderSettingsNode.SelectSingleNode("AudioRecordingFormat").InnerText = 1
            else
                multiCorderSettingsNode.SelectSingleNode("AudioRecordingFormat").InnerText = 0
            end if 
            console.wl("=======================")
            Dim getSettMethod As Reflection.MethodInfo = mySettingsObj.GetType().GetMethod("get_a")
            
            dim sett as object = getSettMethod.invoke(mySettingsObj, Nothing)
            sett.MultiCorderSettings = multiCorderSettingsNode.InnerXml
            sett.Save()
        else 
            console.wl("ERROR (line " & me.CurrentLine.tostring() & "): mySettingsObj.MultiCorderSettings is NOTHING.")
        end if
           
    catch e as Exception
            console.wl("ERROR (line " & me.CurrentLine.tostring() & "): ERROR in GET Multicorder Settings part: " & e.toString())
    end try
