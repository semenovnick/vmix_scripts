' ================== INIT CONFIG ==================
    dim dynamicInputNumber as integer = 4
' ================== =========== ==================

' ==================== START TIMER ======================
    dim startTime       as Datetime  = System.DateTime.Now
    dim scriptTime      as TimeSpan
' ==================== ============ ======================

' ===================DYNAMIC INPUT PROCESSING =======================
    dim config              as new system.xml.xmldocument
    config.loadxml(API.XML)

    dim inputsNodeList as XMLNodeList = config.SelectSingleNode("/vmix/inputs").SelectNodes("input")
    dim targetName as string = config.SelectSingleNode("/vmix/dynamic").SelectSingleNode("input" & dynamicInputNumber.toString()).InnerText
    dim inputGUID as string = ""
    dim isTargetOutput as boolean = false
    dim outputNumber as string = ""
    dim targetNode as XMLNode  
    dim isError as boolean = true

    if targetName.toLower().StartsWith("output") then 
        isTargetOutput = true
        dim isCorrectOutput as boolean = false
        for i as integer = 1 to 4 
            if not isCorrectOutput then 
                if targetName.toLower() = "output" & i.toString() then 
                    isCorrectOutput = true
                    console.wl("DEBUG (line " & me.CurrentLine.tostring() & "): Output "& i.toString() &" is correct")
                    outputNumber = i.toString()
                end if
            end if
        next
        if not isCorrectOutput then 
            console.wl("ERROR (line " & me.CurrentLine.tostring() & "): No such output => " & targetName)
        end if
        isError = not isCorrectOutput
    else 
        isTargetOutput = false
        targetNode = config.SelectSingleNode("/vmix/inputs/input[@shortTitle=""" & targetName & """]")
        if targetNode isnot Nothing then 
            inputGUID = targetNode.Attributes("key").Value
            isError = false
            console.wl("DEBUG (line " & me.CurrentLine.tostring() & "): " & targetName & " GUID: "& inputGUID)
        else 
            isError = true
            console.wl("ERROR (line " & me.CurrentLine.tostring() & "): No such input => " & targetName)
        end if 
    end if


' ===========================================================

' ========================= PROCESSING UPDATES =====================
    dim currentSettings as object               '  vMIX MySettings OBJECT
    dim multiCorderSettingsNode as XMLNode      ' multicorder settings NODE
    ' ======================== Getting settings ===================
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
    ' ==============================================================

    ' ======================== Checking settings ===================
        if not isError then
            dim targetNodeName as string= ""
            if isTargetOutput then 
                targetNodeName = "MultiCorderEnabled.Output" & outputNumber
            else
                targetNodeName = "MultiCorderEnabled." & inputGUID
            end if     
            dim enablingNode as XMLNode = multiCorderSettingsNode.SelectSingleNode(targetNodeName)
                if enablingNode isnot nothing then
                    ' console.wl("WAS: " & enablingNode.InnerText)
                    multiCorderSettingsNode.SelectSingleNode(targetNodeName).InnerText = Convert.toInt32(not Convert.ToBoolean(Convert.toInt32(enablingNode.InnerText))).toString()
                    ' console.wl("AFTER: " & multiCorderSettingsNode.SelectSingleNode(targetNodeName).InnerText)
                else
                    console.wl("ERROR (line " & me.CurrentLine.tostring() & "): No such input(" & targetName & ") in Multicorder...")
                    isError = true
                end if
        else
            console.wl("ERROR (line " & me.CurrentLine.tostring() & "): Error with getting settings. Stopping script...")
            isError = true
        end if
    
    ' ==============================================================    
    ' ======================== Updating settings ===================
    if not isError then
        try 
                currentSettings.MultiCorderSettings = multiCorderSettingsNode.InnerXml
                currentSettings.Save()

        catch e as Exception
                console.wl("ERROR (line " & me.CurrentLine.tostring() & "): ERROR in SAVE Multicorder Settings part: " & e.toString())
        end try
    else
        console.wl("ERROR (line " & me.CurrentLine.tostring() & "): Error with updating settings. Stopping script...")
    end if 
    ' ==============================================================
' =========================================================


' ==================== STOP TIMER ======================
scriptTime = System.DateTime.Now - startTime
Console.WriteLine("Script done in " + scriptTime.TotalMilliseconds.ToString() + " milliseconds")
' ==================== ============ ======================