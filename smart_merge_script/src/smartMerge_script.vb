' ===================== USER DEFINED SETTINGS =======================================================

    ' ============== define MERGE EFFECT DURATION ====================

        dim stingerNumber   as integer = 2   

    ' ============== define new ME name here  ========================

        dim ME1Input        as string = "ME1"
        dim ME2Input        as string = "ME2"
        dim ME3Input        as string = "ME3"
        dim ME4Input        as string = "ME4"
        dim ME5Input        as string = "ME5"

    ' ============== add new ME name to ARRAY  =======================

        dim MEInputNames()  as string = {ME1Input , ME2Input, ME3Input, ME4Input, ME5Input} 

    ' ============== string VARIABLES OF NAMING METHOD ===============

        dim POSpref         as string = "POS"
        dim POSpostFix      as string = "_POS"        ' postfix of virtual with virtual PTZ
        dim fakePTZpostfix  as string = "_FAKE_PTZ"   ' input for emulation of PTZ
        dim MEStingerInput  as string  = "ME_STINGER_SOURCE"   ' stinger input

    ' ============= Define Layers of Position in ME ==================

        dim position1Layer  as integer = 6
        dim position2Layer  as integer = 5
        dim position3Layer  as integer = 4
        dim position4Layer  as integer = 3

    ' ============= ADD new Layer to ARRAY =========================

        dim positionLayers() as integer = { position1Layer , position2Layer , position3Layer , position4Layer }  

    ' ========= additional layers ==========================

        dim fullFrameLayer as integer = 1                   ' Layer of FullFrame like CAM Multiview ( Camera with overlays as PGM)
        dim FullFramePOSMaskName as string = "FullFrame_M"
        dim fullFramePOSlayer as integer = 10              ' Layer for changing when ME0 (Fullscreen layer is going to merge)

    '=======================================================
    ' ============= POS Input setups =======================

            dim POSmainLayer        as integer = 1  '   Main layer 
            dim POSchangingLayer    as integer = 2  '   Layer used for simple change source
            dim POSmainNewLayer     as integer = 3  '   Layer equal of main layer if no change - empty 

        ' =======================================================

            dim POSX1Layer          as integer = 5  '   Layer to change for Px=P1
            dim POSX2Layer          as integer = 6  '   Layer to change for Px=P2
            dim POSX3Layer          as integer = 7  '   Layer to change for Px=P3
            dim POSX4Layer          as integer = 8  '   Layer to change for Px=P4

        ' =======================================================

            dim POSXLaysers() as integer = { POSX1Layer, POSX2Layer, POSX3Layer, POSX4Layer}

    '=============== POS MIXES SETTINGS==========================

            dim POS1_Input as string = "POS1_mix"
            dim POS2_Input as string = "POS2_mix"
            dim POS3_Input as string = "POS3_mix"
            dim POS4_Input as string = "POS4_mix"

            
            dim POSMixesNames() as string = { POS1_Input , POS2_Input , POS3_Input, POS4_Input}
        ' =========================================================

            dim POS1_MASK_Input as string = "POS1_MASK"
            dim POS2_MASK_Input as string = "POS2_MASK"
            dim POS3_MASK_Input as string = "POS3_MASK"
            dim POS4_MASK_Input as string = "POS4_MASK"

            dim POSMasksNames() as string = { POS1_MASK_Input , POS2_MASK_Input , POS3_MASK_Input, POS4_MASK_Input}
        '=========================================================
' ===================== =============================== =============================================
' ===================== DO NOT TOUCH SETTINGS =======================================================
    ' ==================== START TIMER ======================
        dim startTime   as Datetime  = System.DateTime.Now
        dim scriptTime  as TimeSpan
    ' ==================== ============ ======================

    dim isError as boolean = false                                          ' GLOBAL ERROR FLAG
    dim stringerCommand as string = "Stinger" & stingerNumber.ToString()

    dim config as new system.xml.xmldocument
    config.loadxml(API.XML)

    dim numberOfPOSITIONS   as integer = positionLayers.Length
    dim numberOfMEs         as integer = MEInputNames.Length

    dim allMixesNodeList as XMLNodeList =  config.SelectNodes("/vmix/inputs/input[@type=""Mix""]")
    dim inputsNodeList as XMLNodeList = config.SelectSingleNode("/vmix/inputs").SelectNodes("input") 
    dim inputsNames( inputsNodeList.count - 1 ) as string

    dim transitionList as XMLNodeList = config.SelectSingleNode("/vmix/transitions").SelectNodes("transition") 

    dim POSMixesIndexes(POSMixesNames.length - 1 ) as integer
    dim MASKMixesIndexes(POSMasksNames.length - 1) as integer
    dim POSes(      (numberOfPOSITIONS - 1), (numberOfPOSITIONS - 1), (numberOfMEs - 1)) as string
    dim MASKsON(    (numberOfPOSITIONS - 1), (numberOfPOSITIONS - 1), (numberOfMEs - 1)) as string
    dim MASKsOFF(   (numberOfPOSITIONS - 1), (numberOfPOSITIONS - 1), (numberOfMEs - 1)) as string


    '======SETUPS
    dim prwSetup(( numberOfPOSITIONS - 1 )) as string       ' array of POS src of preview
    dim pgmSetup(( numberOfPOSITIONS - 1 )) as string       ' array of POS src of program
    dim prwFullFrameLayerName               as string = ""
    dim pgmFullFrameLayerName               as string = ""
    dim prwStateBitMask                     as integer = 0  ' bit mask of relationship between prw and pgm

    dim MESetups(( numberOfMEs - 1 ), ( positionLayers.Length - 1 )) as string       ' array of POS src of preview
    
    '========================
    dim prwMIXSetup(    ( numberOfPOSITIONS - 1 )) as string    ' array of prwMIX config 
    dim prwMASKSetup(   ( numberOfPOSITIONS - 1 )) as string    ' array of prwMASK config
    dim pgmMIXSetup(    ( numberOfPOSITIONS - 1 )) as string    ' array of pgmMIX config
    dim pgmMASKSetup(   ( numberOfPOSITIONS - 1 )) as string    ' array of pgmMASK config

    ' =========================


    'The KEYS of PROGRAM and PREVIEW
    dim PROGInputName   as string = ""              ' The ShortTitle (AS String) of the INPUT NOW in PROGRAM
    dim PREVInputName   as string = ""              ' The ShortTitle (AS String) of the INPUT NOW in PREVIEW
    dim PROGInputKey    as string = ""              ' The KEY (AS String) of the INPUT NOW in PROGRAM
    dim PREVInputKey    as string = ""              ' The KEY (AS String) of the INPUT NOW in PROGRAM
    dim PROGInputType   as string = ""
    dim PREVInputType   as string = ""


    'INPUT NUMBERS of PROGRAM and PREVIEW
    dim PROGInputNumber as string = ""      ' TPROGInputhe NUMBER (AS String) of the INPUT NOW in PROGRAM
    dim PREVInputNumber as string = ""      ' The NUMBER (AS String) of the INPUT NOW in PREVIEW

    'XML Components
    dim PROGInputNodeList   as XMLNodeList          ' NodeList of PROGRAM layers
    dim PROGInputNode       as XMLNode              ' The XMLNode of PROGRAM INPUT
    dim PREVInputNodeList   as XMLNodeList          ' NodeList of PREVIEW layers
    dim PREVInputNode       as XMLNode              ' The XMLNode of PREVIEW INPUT

    'Get the XMLNode for the Input in PROGRAM:
    PROGInputNumber = config.SelectSingleNode("/vmix/active").InnerText
    PROGInputNode   = config.SelectSingleNode("/vmix/inputs/input[@number=" & PROGInputNumber & "]")
    PROGInputType   = PROGInputNode.Attributes.GetNamedItem("type").Value
    PROGInputName   = PROGInputNode.Attributes.GetNamedItem("shortTitle").Value
    PROGInputKey    = PROGInputNode.Attributes.GetNamedItem("key").Value

    'Get the XMLNode for the Input in PREVIEW:
    PREVInputNumber = config.SelectSingleNode("/vmix/preview").InnerText
    PREVInputNode   = config.SelectSingleNode("/vmix/inputs/input[@number=" & PREVInputNumber & "]")
    PREVInputType   = PREVInputNode.Attributes.GetNamedItem("type").Value
    PREVInputName   = PREVInputNode.Attributes.GetNamedItem("shortTitle").Value
    PREVInputKey    = PREVInputNode.Attributes.GetNamedItem("key").Value

    'The XMLNodeList of LAYERS in PROGRAM:
    PROGInputNodeList = config.SelectSingleNode("/vmix/inputs/input[@key=""" & PROGInputKey & """]").SelectNodes("overlay") 

    'The XMLNodeList of LAYERS in PREVIEW:
    PREVInputNodeList = config.SelectSingleNode("/vmix/inputs/input[@key=""" & PREVInputKey & """]").SelectNodes("overlay") 

    dim isME0inPGM as boolean = false ' trigger for ME0 situation in Program
    dim isME0inPRV as boolean = false ' trigger for ME0 situation in Preview

' =========== FINDING MIXes Indexes and TEST for  existance of USER Defined variables

    if POSMixesNames.length <> POSMasksNames.length then
        isError = true
        console.wl("ERROR (line " & me.CurrentLine.tostring() & "): Length of POSMixesNames array is not equal to length of POSMasksNames array. Check script settings.")
    else       
        for mixIndex as integer = 0 to (POSMixesNames.length - 1)
            dim isHereMIX as boolean = false
            dim isHereMASK as boolean = false
            dim isLocalError as boolean = true
            for mixNodeIndex as integer = 0 to allMixesNodeList.count - 1
                dim mixNodeName as string =  allMixesNodeList.Item(mixNodeIndex).Attributes.GetNamedItem("shortTitle").Value
                if POSMixesNames(mixIndex) = mixNodeName then
                    isHereMIX = true
                    POSMixesIndexes(mixIndex) = CInt(mixNodeIndex) + 1
                else
                    if POSMasksNames(mixIndex) = mixNodeName then
                        isHereMASK = true
                        MASKMixesIndexes(mixIndex) = CInt(mixNodeIndex) + 1
                    end if
                end if 
            next 
            isLocalError = not ( isHereMIX and isHereMASK )
            if isLocalError then
                isError = true
                if not isHereMIX then
                    console.wl("ERROR (line " & me.CurrentLine.tostring() & "): Cannot find "& POSMixesNames(mixIndex) & " as POS " & mixIndex + 1  & " MIX in your project. Check script settings.")
                end if
                if not isHereMASK then
                    console.wl("ERROR (line " & me.CurrentLine.tostring() & "): Cannot find "& POSMasksNames(mixIndex) & " as MASK " & mixIndex + 1  & " MIX in your project. Check script settings.")
                end if
            end if
        next
    end if
' ============= TEST FOR ALL USER DEFINED STRING VARIABLES ==========
    ' making an array of all input Names
    for inputIndex as integer = 0 to inputsNodeList.count - 1 
        inputsNames(inputIndex) = inputsNodeList.Item(inputIndex).Attributes.GetNamedItem("shortTitle").Value
    next

    for each MEInputName as string in MEInputNames
        if Array.IndexOf(inputsNames, MEInputName) = -1 then
            isError = true
            console.wl("ERROR (line " & me.CurrentLine.tostring() & "): Cannot find "& MEInputName & " as ME " & Array.IndexOf(MEInputNames, MEInputName) + 1  & ". Check script settings.")
        end if 
    next

    if Array.IndexOf(inputsNames, MEStingerInput) = -1 then
        isError = true
        console.wl("ERROR (line " & me.CurrentLine.tostring() & "): Cannot find "& MEStingerInput & " as MEStingerInput. Check script settings.")
    end if 

    if Array.IndexOf(inputsNames, FullFramePOSMaskName) = -1 then
        isError = true
        console.wl("ERROR (line " & me.CurrentLine.tostring() & "): Cannot find "& MEStingerInput & " as MEStingerInput. Check script settings.")
    end if 
' ================= SETTING UP the new STINGER DURATION ============
    dim mergeDuration  as integer = 1000       ' this will be overwritten if you add your stinger to fast transition buttons list in vmix GUI
    dim newMergeDuration as integer = mergeDuration
    dim transitionNumber as integer = -1
    for each transitionNode as XMLNode in transitionList 
        if transitionNode.Attributes.GetNamedItem("effect").Value = stringerCommand
            newMergeDuration = CInt(transitionNode.Attributes.GetNamedItem("duration").Value) 
            transitionNumber = CInt(transitionNode.Attributes.GetNamedItem("number").Value)
        end if 
    next
    
    try 
            dim vmixAssemblyTemp As Reflection.Assembly = Reflection.Assembly.GetEntryAssembly()            
            dim mySettingsTypeName as string = "vMix" & "." & "My" & "." & "MySettings"
            dim mySettingsType as type = vmixAssemblyTemp.gettype(mySettingsTypeName)
            Dim mySettingsObj As object = activator.CreateInstance(mySettingsType)   
            Dim mySettingsXML As XmlDocument = New XmlDocument()
            dim overlaySettingsNode as XMLNode = mySettingsXML.CreateElement("OverlaySettings")
            if mySettingsObj.OverlaySettings isnot nothing then 
                overlaySettingsNode.InnerXml = mySettingsObj.OverlaySettings
                dim overlayStingerNumber as integer = 3 + stingerNumber
                dim stingerDisplayDuration as integer = CInt(overlaySettingsNode.SelectSingleNode("/DisplayDuration" & overlayStingerNumber).InnerText)
                if newMergeDuration > stingerDisplayDuration then 
                    console.wl("Attention (line " & me.CurrentLine.tostring() & "): Duration you set in Stinger"& stingerNumber & " in BUTTON is bigger than duration set in Overlay Settings. Setting it to OverlaySetting value: " & stingerDisplayDuration)
                    newMergeDuration = stingerDisplayDuration
                    if transitionNumber <> -1 then 
                        API.Function("SetTransitionDuration" & transitionNumber.toString(), Value: = stingerDisplayDuration.toString() )
                    end if
                end if 
            else 
                console.wl("ERROR (line " & me.CurrentLine.tostring() & "): Cannot find "& MEStingerInput & " as MEStingerInput. Check script settings.")
            end if
           
    catch e as Exception
            console.wl("ERROR (line " & me.CurrentLine.tostring() & "): ERROR in STINGER DURATION part: " & e.toString())
            console.wl("ERROR (line " & me.CurrentLine.tostring() & "): MergeDuration is set to script difined value : " & mergeDuration)
            newMergeDuration = mergeDuration
    end try

    mergeDuration = newMergeDuration

' ===================================================================================

' ========MAIN SCRIPT ONLY IF NO ERROR ==================

    if not isError then
        ' =========== MAKING MATIX OF AVAILABLE POS_a_b_ON/OFF_M INPUTS ===

            for each input as XMLNode in inputsNodeList
                dim inputName as string = input.Attributes.GetNamedItem("shortTitle").Value 
                dim inpuNameSplitted() as string = inputName.Split("_")
                if inpuNameSplitted(0) = POSpref  then
                    dim startAlterPOSIndex as integer = 0
                    dim isV as boolean = false
                    dim isMask as boolean = false
                    dim isOn as boolean = false
                    dim splitX() as string = inpuNameSplitted(1).split("x")
                    dim POSNumber as integer = CInt(splitX(0))
                    if POSNumber <> 0 then
                        dim POSXNumber as integer = POSNumber
                        dim splitV() as string = inpuNameSplitted(2).split("v")
                        ' ========== computing flags ===========
                        
                            
                            if splitX.length > 1 then
                                startAlterPOSIndex = 1
                                
                            else
                                startAlterPOSIndex = 0
                            end if 

                            ' if splitV.length > 1 then
                            '     isV = true
                                
                            ' else
                            '     isV = false
                            ' end if 
                            if inpuNameSplitted(inpuNameSplitted.Length - 1) = "M" then 
                                isMask = true
                                select inpuNameSplitted(inpuNameSplitted.Length - 2)
                                    case "ON"
                                        isOn = true
                                    case "OFF"
                                        isOn = false
                                end select
                            else
                                isMask = false
                            end if
                        ' ========== put input in array ===========
                            for each meNumber as string in splitV  
                                for alterPOS as integer = startAlterPOSIndex to splitX.length - 1
                                    POSXNumber = CInt(splitX(alterPOS))
                                    ' console.wl(inputName & "SPLIT X Length " & splitX.length & "splitX" & splitX(alterPOS)) 
                                        if isMask then
                                            ' mask
                                            if isOn then
                                                MASKsON(POSNumber - 1 , POSXNumber - 1 , CInt(meNumber) - 1) = inputName
                                            else
                                                MASKsOFF(POSNumber - 1 , POSXNumber - 1 , CInt(meNumber) - 1) = inputName ' POS = POSX Case
                                            end if                         
                                        else
                                        ' not mask
                                            POSes(POSNumber -1 , POSXNumber -1 , CInt(meNumber)-1) = inputName
                                        end If
                                next
                            next  
                    end if 
                end if 
            next 

            ' ========================= TEST ====================================

                ' for MENum as integer = 0 to (numberOfMEs - 1)
                ' console.WL("ME " & (MENum+1).tostring() & "==========================")
                '     for fromPOS as integer = 0 to (numberOfPOSITIONS - 1)
                '     dim line as string= ""
                '         for ToPOS as integer = 0 to (numberOfPOSITIONS - 1)
                '             dim testInstance as string = MASKsON(fromPOS, ToPOS , MENum)
                '             if testInstance isnot nothing
                '             line += " " & testInstance & " "
                '             else 
                '             line += " Nothing " & fromPOS.tostring() & ToPOS.toString() & MENum.toString()
                '             end if 
                '         next
                '     console.WL((fromPOS+1).tostring() & ": " & line)
                '     next
                ' Next

            ' ========================= TEST ====================================


        ' ============== INITIALIZATION of MEStingerInput ===============

            for POSNumber as integer = 0 to ( numberOfPOSITIONS - 1 )
                Input.Find(MEStingerInput).Function("SetMultiViewOverlay", (positionLayers(POSNumber)).ToString() & "," & POSMixesNames(POSNumber))
            next
            
            Input.Find(MEStingerInput).Function("SetMultiViewOverlay", (fullFramePOSlayer).ToString() & "," & "")

        ' ============== READING PRW PRG AND ALL ME SETUPS ==============

            for each positionIndex as integer in positionLayers
                dim POSNumber as integer = Array.IndexOf(positionLayers, positionIndex)
                
                prwSetup(POSNumber) = ""
                pgmSetup(POSNumber) = ""

                dim setupTypes() as string = { "PRW", "PGM", "ME" }

                for each setupType as string in setupTypes 
                    dim InputNodeLists() as XMLNodeList 
                    select case setupType
                        case "PRW"
                            ReDim InputNodeLists(0)
                            InputNodeLists(0) =  PREVInputNodeList 
                        case "PGM"
                            ReDim InputNodeLists(0)
                            InputNodeLists(0) =  PROGInputNodeList
                        case "ME"
                            ReDim InputNodeLists(( numberOfMEs - 1 ))
                            for MEindex as integer = 0 to ( numberOfMEs - 1 ) 
                                InputNodeLists(MEindex) = config.SelectSingleNode("/vmix/inputs/input[@shortTitle=""" & MEInputNames(MEindex) & """]").SelectNodes("overlay")
                                MESetups(MEindex, POSNumber) = ""
                            next
                    end select
                    for each NodeList as XMLNodeList in InputNodeLists
                        if NodeList.count > 0 then
                            dim MEindex as integer = Array.IndexOf(InputNodeLists, NodeList)
                            for each layer as XMLNode in NodeList
                                dim layerIndex as string = layer.Attributes.GetNamedItem("index").Value
                                dim layerKey as string = layer.Attributes.GetNamedItem("key").Value
                                dim layerNode  as XMLNode = config.SelectSingleNode("/vmix/inputs/input[@key=""" & layerKey & """]")
                                dim layerShortTitle as string = layerNode.Attributes.GetNamedItem("shortTitle").Value
                                
                                if (layerIndex = (positionIndex - 1)) then
                                    select case setupType
                                        case "PRW"
                                            prwSetup(POSNumber) = layerShortTitle
                                        case "PGM"
                                            pgmSetup(POSNumber) = layerShortTitle
                                        case "ME"
                                            MESetups(MEindex, POSNumber) = layerShortTitle
                                    end select
                                end if
                                if ((layerIndex = fullFrameLayer - 1)) then
                                    select case setupType
                                        case "PRW"
                                            prwFullFrameLayerName = layerShortTitle
                                        case "PGM"
                                            pgmFullFrameLayerName = layerShortTitle
                                    end select
                                end if

                            next                
                        end if
                    next
                next
            next
            ' ========================= TEST ================================

                ' console.WL("====== PRV / PGM ======")
                ' for POSNumber as integer = 0 to ( prwSetup.length - 1)
                '     console.WL("POS " & (POSNumber + 1) & ": " & "PRW: " & prwSetup(POSNumber) & " / PGM: "& pgmSetup(POSNumber) )
                ' next
                ' for MEindex as integer = 0 to ( MEInputNames.Length - 1 )
                '     console.WL("====== ME " & (MEindex + 1 ) & " ======")
                '     for POSNumber as integer = 0 to ( positionLayers.Length - 1 )
                '         console.WL("POS " & (POSNumber + 1) & ": " &  MESetups(MEindex, POSNumber) )
                '     next
                ' next

            ' ========================= ==== ================================
        ' ============== ================================= ==============
        ' ============== CHECKING FOR ME IN PRW / PRM ==============
            dim MEInPGMIndex as integer = Array.indexOf(MEInputNames, PROGInputName)
            dim MEInPRWIndex as integer = Array.indexOf(MEInputNames, PREVInputName)
        ' ============== ============================ ==============
        ' ============== MAKING PGM FOR MIXES ==============

            if MEInPGMIndex <> -1 then
            ' ME IN PGM
                Console.WriteLine("Making PGM for ME " & MEInPGMIndex + 1)
                
                for POSNumber as integer = 0 to numberOfPOSITIONS - 1 
                    ' ============ MAKING pgmMIX and prmMASK setups =========

                        pgmMIXSetup(POSNumber) = POSes(POSNumber, POSNumber, MEInPGMIndex )

                        if pgmMIXSetup(POSNumber) is nothing or pgmMIXSetup(POSNumber) = "" then 
                            console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): cannot find """ & POSpref & """ INPUT for: " & "POS " & POSNumber + 1 & " and ME " & MEInPGMIndex + 1)
                        end if 
                        
                        dim POSmainLayerName as string = ""

                        if (pgmSetup(POSNumber) isnot nothing) and pgmSetup(POSNumber) <> "" then
                            pgmMASKSetup(POSNumber) = MASKsON(POSNumber, POSNumber, MEInPGMIndex)
                            POSmainLayerName = MESetups(MEInPGMIndex, POSNumber)

                            if pgmMASKSetup(POSNumber) is nothing or pgmMIXSetup(POSNumber) = "" then 
                                console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): cannot find """ & POSpref & """ MASK ON for: " & "POS " & POSNumber + 1 & " and ME " & MEInPGMIndex + 1)
                            end if 

                        else
                            pgmMASKSetup(POSNumber) = MASKsOFF(POSNumber, POSNumber, MEInPGMIndex)
                            if MEInPRWIndex <> -1 then
                                POSmainLayerName = MESetups(MEInPRWIndex, POSNumber)
                            end if 

                            if pgmMASKSetup(POSNumber) is nothing or pgmMIXSetup(POSNumber) = "" then 
                                console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): cannot find """ & POSpref & """ MASK OFF for: " & "POS " & POSNumber + 1 & " and ME " & MEInPGMIndex + 1)
                            end if 

                        end if


                    ' ============ MAKING overlays for POS Inputs =========

                        dim POSinPGMInput as object = Input.Find(pgmMIXSetup(POSNumber)) 

                        POSinPGMInput.Function("SetMultiViewOverlay", (POSmainLayer).ToString() & "," & POSmainLayerName)

                        POSinPGMInput.Function("SetMultiViewOverlay", (POSchangingLayer).ToString() & "," & "")
                        POSinPGMInput.Function("SetMultiViewOverlay", (POSmainNewLayer).ToString() & "," & "")
                        
                        for each POSXLayer as integer in POSXLaysers
                            POSinPGMInput.Function("SetMultiViewOverlay", (POSXLayer).ToString() & "," & "")
                        next 
                        ' console.WL("MIX of POS " & (POSNumber + 1 ) & " : " & pgmMIXSetup(POSNumber))
                        ' console.WL("MASK of POS " & (POSNumber + 1 ) & " : " & pgmMASKSetup(POSNumber))
                next 
            else
            ' M0 Stuff
                Console.WriteLine("Making PGM for FULL FRAME / OTHER : " & PROGInputName)

                if PROGInputType <> "Colour" then
                    pgmFullFrameLayerName = PROGInputName
                end if

                isME0inPGM = true
            end if

        ' ============== ==================== ==============
        ' ============== MAKING PRW FOR MIXES ==============    
            if MEInPRWIndex <> -1 then
            ' ME IN PRW
                Console.WriteLine("Making PRW for ME " & MEInPRWIndex + 1 )
                ' ============ MAKING prwMIX and prwMASK setups Initial =========
                    ' ============ CALCULATING SCENARIO =========
                        ' changingPOS is status of POS and equals pos to change 
                        ' ======= JUST INIT ==========
                        ' ALL EMPTY             0000 xxxx = 0
                        ' ALL EQUALS            0001 xxxx = 1
                        ' FROM EMPTY FOREVER    0010 xxxx = 2
                        ' TO EMPTY FOREVER      0011 xxxx = 3
                        ' ======= CHANGE SCENARIO ====
                        ' SIMPLE CHANGE         0100 xxxx = 4
                        ' ======= TRANSFORM SCENARIO =
                        ' TRANSFORM             1000 xxxx = 8
                        ' FROM EMPTY BUT FROM POS    1001 xxxx = 9
                        ' TO EMPTY BUT FROM POS     1010 xxxx = 10
                            ' dim POS as integer =   changingPOS AND  15 
                            ' dim scenario as integer = changingPOS AND 255 >> 4 
                            ' dim isChange as boolean =  Convert.ToBoolean(((scenario AND  7   ) >> 2 ))
                            ' dim isTransform as boolean =  Convert.ToBoolean(((scenario AND  15  ) >> 3 ))
                        ' ============ LOCAL CONSTANTS =====================

                            const I_ALL_EMPTY           as integer = 0  << 4
                            const I_ALL_EQUALS          as integer = 1  << 4 
                            const I_FROM_EMPTY          as integer = 2  << 4
                            const I_TO_EMPTY            as integer = 3  << 4
                            const C_CHANGE              as integer = 4  << 4
                            const T_TRANSFORM_FROM      as integer = 8  << 4
                            const T_TRANSFORM_TO        as integer = 9  << 4
                            const T_TO_EMPTY_POS        as integer = 10 << 4
                            const T_FROM_EMPTY_POS      as integer = 11 << 4

                            const LOCK_TO               as integer = 1
                            const LOCK_FROM             as integer = -1

                        ' =================================================== 

                        dim changingPOSes(numberOfPOSITIONS - 1) as integer
                        dim changingLOCK(numberOfPOSITIONS - 1) as integer ' priority to changeTo operation from POSX => POSY where X<Y
                        for POSNumber as integer = 0 to numberOfPOSITIONS - 1 
                            dim isPGMSetupNotEmpty as boolean = (pgmSetup(POSNumber) isnot nothing) and pgmSetup(POSNumber) <> ""
                            dim isPRWSetupNotEmpty as boolean = (prwSetup(POSNumber) isnot nothing) and prwSetup(POSNumber) <> ""                   
                            if pgmSetup(POSNumber) = prwSetup(POSNumber) then
                                if isPGMSetupNotEmpty and isPRWSetupNotEmpty then
                                    changingPOSes(POSNumber) = POSNumber + I_ALL_EQUALS                            
                                else   
                                    changingPOSes(POSNumber) = POSNumber + I_ALL_EMPTY 
                                end if
                            else                   
                                changingPOSes(POSNumber) = POSNumber + C_CHANGE
                                if isPGMSetupNotEmpty or isPRWSetupNotEmpty then
                                    dim POSXtoChangeNumber as integer = -1
                                    for POSXNumber as integer = 0 to numberOfPOSITIONS - 1
                                        dim isPRWXNotEmpty as boolean = (prwSetup(POSXNumber) isnot nothing) and prwSetup(POSXNumber) <> ""
                                        dim isPGMXNotEmpty as boolean = (pgmSetup(POSXNumber) isnot nothing) and pgmSetup(POSXNumber) <> ""
                                        dim foundChange as boolean = false
                                            if pgmSetup(POSNumber) = prwSetup(POSXNumber) then
                                                if isPRWXNotEmpty
                                                    if changingLOCK(POSNumber) <> LOCK_FROM then
                                                        changingPOSes(POSNumber) = POSXNumber + T_TRANSFORM_TO
                                                        POSXtoChangeNumber = POSXNumber
                                                        changingLOCK(POSNumber) = LOCK_TO
                                                        changingLOCK(POSXNumber)= LOCK_FROM
                                                    end if
                                                end if 
                                            end if
                                        if prwSetup(POSNumber) = pgmSetup(POSXNumber) then
                                                if isPGMXNotEmpty
                                                    if changingLOCK(POSNumber) <> LOCK_TO then
                                                        changingPOSes(POSNumber) = POSXNumber + T_TRANSFORM_FROM
                                                        POSXtoChangeNumber = POSXNumber
                                                    end if 
                                                end if 
                                        end if                                        
                                    next
                                    if POSXtoChangeNumber <> -1 then 
                                        if changingLOCK(POSNumber) <> LOCK_TO then 
                                            if not isPRWSetupNotEmpty then
                                                if POSXtoChangeNumber <> POSNumber and ( changingPOSes(POSNumber) AND 255 >> 4 ) <> I_ALL_EMPTY then
                                                    changingPOSes(POSNumber) = POSXtoChangeNumber + T_TO_EMPTY_POS
                                                end if 
                                            end if
                                            if not isPGMSetupNotEmpty then
                                                if POSXtoChangeNumber <> POSNumber and ( changingPOSes(POSNumber) AND 255 >> 4 ) <> I_ALL_EMPTY  then
                                                    changingPOSes(POSNumber) = POSXtoChangeNumber + T_FROM_EMPTY_POS
                                                end if 
                                            end if
                                        end if
                                    else        
                                        if not isPRWSetupNotEmpty then
                                        changingPOSes(POSNumber) = POSNumber + I_TO_EMPTY
                                        end if
                                        if not isPGMSetupNotEmpty then
                                            changingPOSes(POSNumber) = POSNumber + I_FROM_EMPTY
                                        end if                      
                                        
                                    end if 
                                end if                                 
                            end if                              
                        next
                    ' ============ MAKING DECISIONS =========
                        for POSNumber as integer = 0 to numberOfPOSITIONS - 1 
                            ' ============ main variables =========
                                dim POSmainLayerSourceName as string = ""
                                dim POSchangingLayerSourceName as string = ""
                                dim POSmainNewLayerSourceName as string = ""
                                
                                dim POSmainLayerName as string = ""
                                dim POSXLayerSourceName as string = ""
                                dim POSXLayerIndex as integer = -1 ' ==> false NOT CHANGING
                            ' ============ INIT =========

                                prwMIXSetup(POSNumber) = POSes(POSNumber, POSNumber, MEInPRWIndex)

                                if prwMIXSetup(POSNumber) is nothing or prwMIXSetup(POSNumber) = "" then 
                                    console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): cannot find """ & POSpref & """ INPUT for: " & "POS " & POSNumber + 1 & " and ME " & MEInPRWIndex + 1)
                                end if 

                                dim isPRWSetupNotEmpty as boolean = (prwSetup(POSNumber) isnot nothing) and prwSetup(POSNumber) <> ""

                                if isPRWSetupNotEmpty then
                                    prwMASKSetup(POSNumber) = MASKsON(POSNumber, POSNumber, MEInPRWIndex)
                                    POSmainLayerSourceName = MESetups(MEInPRWIndex, POSNumber)

                                    if prwMASKSetup(POSNumber) is nothing or prwMASKSetup(POSNumber) = "" then 
                                        console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): cannot find """ & POSpref & """ MASK for: " & "POS " & POSNumber + 1 & " and ME " & MEInPRWIndex + 1)
                                    end if 

                                else
                                    prwMASKSetup(POSNumber) = MASKsOFF(POSNumber, POSNumber, MEInPRWIndex)
                                    if MEInPGMIndex <> -1 then
                                        POSmainLayerSourceName = MESetups(MEInPGMIndex, POSNumber)
                                    end if 

                                    if prwMASKSetup(POSNumber) is nothing or prwMASKSetup(POSNumber) = "" then 
                                        console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): cannot find """ & POSpref & """ MASK for: " & "POS " & POSNumber + 1 & " and ME " & MEInPRWIndex + 1)
                                    end if 

                                end if

                            ' ============== SCENARIOS =========
                                ' console.wl("============================================")
                                dim scenario as integer = changingPOSes(POSNumber) AND 240  ' AND 1111 0000

                                dim changingPOS as integer = changingPOSes(POSNumber) AND 15
                                
                                ' trying to transorfm... if no than "change" scenario

                                ' Console.wl("Debug " & me.CurrentLine.tostring() & " : " & "Scenario: " & (scenario >> 4).tostring() & " POSNumber: " & POSNumber + 1 & "; changingPOS: " & changingPOS + 1)
                                
                                select case scenario 
                                    case T_TRANSFORM_TO
                                        ' console.wl("PGM POS" & POSNumber + 1 & " (" & pgmSetup(POSNumber) & ")" & " => " & "PRV POS" & changingPOS + 1 & " (" &  prwSetup(changingPOS)& ")")
                                        
                                        dim newMIXSetup as string = POSes(POSNumber, changingPOS, MEInPRWIndex )
                                        dim newMASKOffSetup as string = MASKsOFF(changingPOS , POSNumber,  MEInPRWIndex)                              
                                        dim newMASKOnSetup as string = MASKsON(POSNumber, changingPOS, MEInPRWIndex)
                                        
                                        if newMIXSetup isnot nothing and newMASKOnSetup isnot nothing and newMASKOffSetup isnot nothing then 
                                            ' console.wl( (POSNumber + 1) &" > " & (changingPOS +1))
                                            ' console.wl( newMIXSetup & " > " & newMASKOnSetup)
                                            prwMIXSetup(POSNumber) = newMIXSetup
                                            prwMASKSetup(POSNumber) = newMASKOnSetup
                                            ' console.wl("using MIX: " & newMIXSetup )
                                            ' console.wl("using MASK: " & newMASKOnSetup )
                                            POSmainLayerSourceName      = MESetups(MEInPRWIndex, POSNumber)
                                            POSchangingLayerSourceName  = ""
                                            POSmainNewLayerSourceName   = ""    

                                            POSXLayerIndex = changingPOS
                                            POSXLayerSourceName = MESetups(MEInPRWIndex, changingPOS)
                                        else 
                                            if newMIXSetup is nothing then
                                                console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): cannot find """ & POSpref & """ INPUT for: " & "POS " & POSNumber + 1 & "x"& changingPOS + 1 &" and ME " & MEInPRWIndex + 1)
                                            end if 
                                            if newMASKOnSetup is nothing then
                                                console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): cannot find """ & POSpref & """ MASK ON for: " & "POS " & POSNumber + 1 & "x"& changingPOS + 1 &" and ME " & MEInPRWIndex + 1)
                                            end if 
                                            if newMASKOffSetup is nothing then
                                                console.wl("ATTENTION (line " & me.CurrentLine.tostring() & "): cannot find """ & POSpref & """ MASK OFF for: " & "POS " & changingPOS + 1 & "x"& POSNumber + 1 &" and ME " & MEInPRWIndex + 1)
                                            end if 

                                            ' changingPOSes(POSNumber) = changingPOS + C_CHANGE

                                            if MESetups(MEInPRWIndex, POSNumber) <> "" then 
                                                changingPOSes(POSNumber) = changingPOS + C_CHANGE
                                            else 
                                                changingPOSes(POSNumber) = changingPOS + I_TO_EMPTY
                                            end if 
                                        end if  


                                    case T_TRANSFORM_FROM
                                        ' console.wl("PGM POS" & changingPOS + 1 & " (" & pgmSetup(changingPOS) & ")" & " <= " & "PRV POS" & POSNumber + 1 & " (" & prwSetup(POSNumber)& ")")
                                        
                                        dim newMIXSetup as string = POSes(changingPOS, POSNumber, MEInPRWIndex )
                                        dim newMASKOnSetup as string = MASKsON(changingPOS ,POSNumber,  MEInPRWIndex)
                                        dim newMASKOffSetup as string = MASKsOFF(POSNumber, changingPOS , MEInPRWIndex)
                                        

                                        if newMASKOffSetup isnot nothing and newMIXSetup isnot nothing and newMASKOnSetup isnot nothing then
                                            prwMASKSetup(POSNumber) = newMASKOffSetup
                                            ' console.wl("using MASK: " & newMASKOffSetup )
                                            POSmainLayerSourceName      = MESetups(MEInPGMIndex, POSNumber)

                                            POSchangingLayerSourceName  = ""
                                            POSmainNewLayerSourceName   = ""
                                        else 
                                            if MESetups(MEInPGMIndex, POSNumber) <> "" then 
                                                changingPOSes(POSNumber) = changingPOS + C_CHANGE
                                            else 
                                                changingPOSes(POSNumber) = changingPOS + I_FROM_EMPTY
                                            end if 
                                        end if 
                                        


                                    case T_TO_EMPTY_POS
                                        ' console.wl("PGM POS" & POSNumber + 1 & " ~> " & " TRANSFORM TO PRV POS" & changingPOS + 1)
                                    case T_FROM_EMPTY_POS
                                        ' console.wl("PGM POS" & POSNumber + 1 & " <~ " & " TRANSFORM FROM PRV POS" & changingPOS + 1)                                                           
                                end select  

                                scenario = changingPOSes(POSNumber) AND 240

                                ' Console.wl("Debug " & me.CurrentLine.tostring() & " : " & "Scenario: " & (scenario >> 4).tostring() & " POSNumber: " & POSNumber + 1 & "; changingPOS: " & changingPOS + 1)

                                select case scenario    
                                    case C_CHANGE
                                        ' console.wl("PGM POS" & POSNumber + 1 & " (" & pgmSetup(POSNumber) & ")" &  " <> " & "PRV POS" & POSNumber + 1 & " (" & prwSetup(POSNumber) & ")")
                                        try
                                            POSmainLayerSourceName      = MESetups(MEInPGMIndex, POSNumber)
                                            POSchangingLayerSourceName  = MESetups(MEInPRWIndex, POSNumber)
                                            POSmainNewLayerSourceName   = MESetups(MEInPRWIndex, POSNumber)
                                        Catch ex As Exception
                                            isError = true
                                            console.wl("ERROR on line:" & me.CurrentLine.tostring() & " - " & ex.tostring()  )
                                        end try
                                    case I_ALL_EQUALS
                                        ' console.wl("PGM POS" & POSNumber + 1 & " (" & pgmSetup(POSNumber) & ")" & " = " & "PRV POS" & POSNumber + 1 & " (" & prwSetup(POSNumber)& ")")
                                    case I_FROM_EMPTY
                                        ' console.wl("PGM POS" & POSNumber + 1 & " From empty forever")
                                    case I_TO_EMPTY
                                        ' console.wl("PGM POS" & POSNumber + 1 & " To empty forever")
                                    case I_ALL_EMPTY
                                        ' console.wl("PGM POS" & POSNumber + 1 & " EMPTY " & " = " & "PRV POS" & POSNumber + 1 &  " EMPTY ")
                                end select  

                                ' console.wl("============================================")
                            
                                
                            ' ============ MAKING overlays for POS Inputs =========
                            
                                dim POSinPGMInput as object = Input.Find(pgmMIXSetup(POSNumber)) 
                                dim POSinPRWInput as object = Input.Find(prwMIXSetup(POSNumber))

                                POSinPRWInput.Function("SetMultiViewOverlay", (POSmainLayer).ToString() & "," & POSmainLayerSourceName)

                                
                                POSinPRWInput.Function("SetMultiViewOverlay", (POSchangingLayer).ToString() & "," & "")

                                POSinPRWInput.Function("SetMultiViewOverlay", (POSmainNewLayer).ToString() & "," & POSmainNewLayerSourceName)
                                
                                for each POSXLayer as integer in POSXLaysers
                                    POSinPRWInput.Function("SetMultiViewOverlay", (POSXLayer).ToString() & "," & "")
                                    if POSXLayerIndex <> -1 then
                                        if POSXLayer = POSXLaysers(POSXLayerIndex) then                  
                                            POSinPRWInput.Function("SetMultiViewOverlay", (POSXLayer).ToString() & "," & POSXLayerSourceName)
                                        end if
                                    end if 
                                next 
                                ' program overlays

                                    POSinPGMInput.Function("SetMultiViewOverlay", (POSchangingLayer).ToString() & "," & POSchangingLayerSourceName)
                                for each POSXLayer as integer in POSXLaysers
                                    POSinPGMInput.Function("SetMultiViewOverlay", (POSXLayer).ToString() & "," & "")
                                    if POSXLayerIndex <> -1 then
                                        if POSXLayer = POSXLaysers(POSXLayerIndex) then                  
                                            POSinPGMInput.Function("SetMultiViewOverlay", (POSXLayer).ToString() & "," & POSmainLayerSourceName)
                                        end if
                                    end if
                                next 
                                ' console.WL("MIX of POS " & (POSNumber + 1 ) & " : " & pgmMIXSetup(POSNumber))
                                ' console.WL("MASK of POS " & (POSNumber + 1 ) & " : " & pgmMASKSetup(POSNumber))
                    next 
            else
            ' ME0 in PRW 
                Console.WriteLine("Making PRW for FULL FRAME / OTHER : " & PREVInputName)
                if PREVInputType <> "Colour" then
                    prwFullFrameLayerName = PREVInputName
                end if       

                isME0inPRV = true  
            end if

        ' ============== ==================== ==============
        ' ============== FAKE PTZ ==============================
            ' ========================= FAKE PTZ SETTINGS ==================
            dim noPTZInputName  as string = ""         ' input vith no virtual PTZ

                dim SearchInputNodeList as XMLNodeList
                if isME0inPGM then
                    noPTZInputName = pgmFullFrameLayerName
                    ' console.WL("ME0PGM is " & noPTZInputName )
                end if
                if isME0inPRV then
                    noPTZInputName = prwFullFrameLayerName
                    ' console.WL("ME0PRV is " & noPTZInputName )
                end if

            ' ========================= FAKE PTZ PART =====================
            dim FakePTZCondition as boolean = isME0inPRV or isME0inPGM ' condition when use FAKE PTZ use it if you add this script to other script
            ' =============================================================


            ' ========================= FAKE PTZ INITIALIZE=================
            
                dim PTZInputName            as string = noPTZInputName & POSpostFix
                dim PTZInputKey             as string
                dim PTZInputNumber          as integer
                dim PTZInputDisplayName     as string
                dim fakePTZInputName        as string = noPTZInputName & fakePTZpostfix
                dim isPTZ                   as boolean = false ' use in other scripts 
                dim isNeedToSave            as boolean = false
                dim PTZInputPositionsXML    as string = ""

                dim PTZInputNodeFromAPI as XMLNode = config.SelectSingleNode("/vmix/inputs/input[@shortTitle=""" & PTZInputName & """]")
                if PTZInputNodeFromAPI isNot Nothing then 
                    PTZInputKey         = PTZInputNodeFromAPI.Attributes.GetNamedItem("key").Value
                    PTZInputNumber      = PTZInputNodeFromAPI.Attributes.GetNamedItem("number").Value
                    PTZInputDisplayName = PTZInputNodeFromAPI.Attributes.GetNamedItem("title").Value
                    FakePTZCondition    = true and (FakePTZCondition)
                else
                    FakePTZCondition    = false and (FakePTZCondition)
                    isPTZ               = false 
                    ' console.WL("No FAKE_PTZ input found")
                end if

            if FakePTZCondition then
            ' ================== GETTING PTZInputPositionsXML ==============
                    ' ===== GETTING PTZInputPositionsXML FROM INTERNAL API =======
                        try 
                        ' ================= INITIALIZING INTERNAL API ============
                                ' console.WL("====== INERNAL METHOD ==========")
                                Dim vMixAssembly As Reflection.Assembly = Reflection.Assembly.GetEntryAssembly()

                                dim ShortcutInputTypeName as string = "vMix" & "." & "ShortcutInput"
                                dim matrixPositionTypeName as string = "vMix" & "." & "MatrixPosition"

                                dim ShortcutInputType as type = vMixAssembly.gettype(ShortcutInputTypeName)
                                Dim ShortcutInputObj As object = activator.CreateInstance(ShortcutInputType, PTZInputNumber)   ' make it by make shortcutInput instane - API works similar
                                dim getMatrixPositionFromInputMethodName as string = "get_za"                               ' default name from dnspy (it can be different, because there is no good named method that returns us matrixPosition)
                                Dim getInputMethod As Reflection.MethodInfo = ShortcutInputObj.GetType().GetMethod("a", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance) 
                                ' a - is method that returns InputClass instance of Input
                                '=======================================
                                ' a():
                                ' If inputNumber > 0 AndAlso ArrayOfInputs.Length >= inputNumber Then
                                ' 		' The following expression was wrapped in a checked-expression
                                ' 		Return InputInstance
                                ' End If
                                ' ' /else
                                ' return nothing
                                '=================================
                                if getInputMethod isnot nothing then
                                    dim inputObj as object = getInputMethod.Invoke(ShortcutInputObj, Nothing)
                                    Dim isIntputInctance As boolean = inputObj.GetType().GetMethod("get_DisplayTitleAndNumber") isnot nothing 
                                    if isIntputInctance then
                                        if inputObj.get_DisplayTitleAndNumber() = (PTZInputNumber.tostring() & " " & PTZInputDisplayName ) then 
                                            ' console.WL("Input with PTZ: " & PTZInputDisplayName)
                                        end if 
                                        dim methods as Reflection.MethodInfo() = inputObj.GetType().GetMethods()                   ' this methods should be public
                                        ' in InputInstane there are two methods which return positionMatrix one returns position matrix for overlays and have parameter (integer = number of overlay) and other which 
                                        ' returns main matrixPosition
                                        For Each method  as Reflection.MethodInfo In methods
                                            if method.ReturnType.tostring() = matrixPositionTypeName then
                                                Dim myParameters As Reflection.ParameterInfo() = method.GetParameters()
                                                if method.GetParameters().Length = 0 then   
                                                    ' Console.WriteLine("Founded method: " & method.Name.tostring() & ":" & method.ReturnType.tostring()) 
                                                    dim isThereProperMethod as boolean = false
                                                    for each meth as Reflection.MethodInfo in method.invoke(inputObj, nothing).gettype().GetMethods()
                                                        if meth.name.toLower() = "toxml" then
                                                            isThereProperMethod = true
                                                            exit for
                                                        end if
                                                    next
                                                    if isThereProperMethod then
                                                            PTZInputPositionsXML = method.invoke(inputObj, nothing).toxml()
                                                        exit for
                                                    end if
                                                end if 
                                            end if
                                        Next
                                        isNeedToSave = false    
                                    else 
                                        console.WL("ATTENTION ( line: " & me.CurrentLine.tostring() & "): FAKE PTZ/INTERNAL. Wrong Instance, trying another method")
                                        isNeedToSave = true
                                    end if
                                else 
                                    console.WL("ATTENTION ( line: " & me.CurrentLine.tostring() &  "): FAKE PTZ/INTERNAL. No ""a"" method in ShortcutInput Class, trying another method")
                                    isNeedToSave = true
                                end if

                        catch e as Exception
                            
                        end try
                
                    ' ===== SAVE and OPEN vmix preset and PTZInputPositionsXML ====

                        if isNeedToSave then
                            ' console.WL("====== SAVEOPEN METHOD ==========")
                            dim presetPath as string = ""
                            dim tempFolder as string = ".vmix_temp"
                            dim tempName as string = "temp.vmix"
                            dim myDocuments as string = My.Computer.FileSystem.SpecialDirectories.MyDocuments
                            dim presetNode as XMLNode

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
                                    presetPath = myDocuments & "\" & tempFolder & "\" & tempName
                                    ' console.WL("Seems that you are not save anything lets save it to MyDocuments\" & tempFolder)
                                end if
                                try
                                        API.Function("SavePreset", Value:= presetPath)
                                catch e as Exception
                                        console.WL("ATTENTION ( line: " & me.CurrentLine.tostring()& "): SAVEOPEN METHOD. Some error when running API SavePreset... saving im MyDocuments")
                                        presetPath = myDocuments & "\" & tempName                
                                        API.Function("SavePreset", Value:= presetPath)
                                end try
                                ' console.WL("Saved as: " & presetPath)  

                            ' =================== OPENING PRESET ==========================

                                dim vmixFile as new system.xml.xmldocument
                                try
                                    vmixFile.load(presetPath)
                                    dim version as string = vmixFile.SelectSingleNode("/XML/Version").InnerText
                                    ' console.WL("File loaded as version: " & version)
                                catch e as System.IO.FileNotFoundException
                                    console.WL("ERROR ( line: " & me.CurrentLine.tostring()& "): SAVEOPEN METHOD. File " & presetPath & " not found")
                                    isError = true
                                end try

                            ' ======== GETTING PTZInputPositionsXML from file =============

                                if not isError then
                                        dim PTZInputNode as XMLNode = vmixFile.SelectSingleNode("/XML/Input[@Key=""" & PTZInputKey & """]")
                                        if PTZInputNode isnot Nothing then
                                            dim isPTZConnected as string = (PTZInputNode.Attributes.GetNamedItem("PTZAutoConnect").Value)
                                            dim PTZRange as string = PTZInputNode.Attributes.GetNamedItem("PTZConnection").Value
                                            PTZInputPositionsXML = PTZInputNode.Attributes.GetNamedItem("VirtualPTZPosition").Value
                                        end if 
                                end if 
                    
                        end if

                    ' =================== END OF SAVE OPEN ========================
            ' ==============PROCESSING PTZInputPositionsXML=================
                    ' console.WL("====== PROCESSING ==========")
                    dim PTZInputPositions as new system.xml.xmldocument
                    PTZInputPositions.loadxml(PTZInputPositionsXML)
                    ' console.WL("Getting FAKE PTZ Data from XML")
                    dim PostZoomX as string = PTZInputPositions.SelectSingleNode("/MatrixPosition/PostZoomX").InnerText
                    dim PostZoomY as string = PTZInputPositions.SelectSingleNode("/MatrixPosition/PostZoomY").InnerText
                    dim PanX as string = PTZInputPositions.SelectSingleNode("/MatrixPosition/PanX").InnerText
                    dim PanY as string = PTZInputPositions.SelectSingleNode("/MatrixPosition/PanY").InnerText
                    ' console.WL("PostZoomX: "+ PostZoomX)
                    ' console.WL("PostZoomY: "+ PostZoomY)
                    ' console.WL("PanX: "+ PanX)
                    ' console.WL("PanY: "+ PanY)
                    ' in XML saved "." or "," in float position variables then hard to parse we need to culturealize it
                    dim CultureInfo as System.Globalization.CultureInfo = new System.Globalization.CultureInfo("en-US")
                            
                    dim realZoomX as double = double.parse(PostZoomX, CultureInfo)
                    dim realZoomY as double = double.parse(PostZoomY, CultureInfo)
                    dim realPanX as double = double.parse(PanX, CultureInfo) * realZoomX
                    dim realPanY as double= double.parse(PanY, CultureInfo) * realZoomY

            ' =============== APPLYING TO FAKE PTZ INPUT  ==================  
                    try
                                API.Function("SetPanX", Input:= fakePTZInputName, Value:=realPanX.tostring(CultureInfo))
                                API.Function("SetPanY", Input:= fakePTZInputName, Value:=realPanY.tostring(CultureInfo))
                                API.Function("SetZoom", Input:= fakePTZInputName, Value:=realZoomX.tostring(CultureInfo))
                                isPTZ = true
                                ' console.WL("====== FAKE PTZ DONE ==========")
                    catch
                                console.WL("ERROR ( line: " & me.CurrentLine.tostring()& "): FAKE PTZ API. Error")
                                isError = true
                    end try 
            ' ==============================================================  
            end if

        ' ==================================================================
        ' ============== MAKING ME0 ===================================
            ' ME 0 stuff
            if isME0inPRV and isME0inPGM then 
                isError = true
                ' console.WL("ATTENTION ( line: " & me.CurrentLine.tostring()& "): Full Frame Inputs are trying to merge. Fade looks better.")
            else 
                if isME0inPGM then 
                    isError = true

                    for POSNumber as integer = 0 to numberOfPOSITIONS - 1 
                    pgmMIXSetup(POSNumber) = prwMIXSetup(POSNumber)
                    pgmMASKSetup(POSNumber) = prwMASKSetup(POSNumber)
                    if (pgmFullFrameLayerName = prwSetup(POSNumber)) or (pgmFullFrameLayerName + POSpostFix = prwSetup(POSNumber))
                        ' Console.WL("PRG is " & prwSetup(POSNumber) & " POS" & (POSNumber+1).ToString())
                        if isPTZ then 
                            Input.Find( prwMIXSetup(POSNumber)).Function("SetMultiViewOverlay", (POSmainLayer).ToString() & "," & fakePTZInputName)

                        end if     
                        pgmMIXSetup(POSNumber) = pgmFullFrameLayerName
                        pgmMASKSetup(POSNumber) = FullFramePOSMaskName
                        Input.Find(MEStingerInput).Function("SetMultiViewOverlay", (positionLayers(POSNumber)).ToString() & "," & "")
                        Input.Find(MEStingerInput).Function("SetMultiViewOverlay", (fullFramePOSlayer).ToString() & "," & POSMixesNames(POSNumber))
                        isError = false
                    end if 
                    next

                end if 

                if isME0inPRV then 
                    isError = true
                    for POSNumber as integer = 0 to numberOfPOSITIONS - 1 
                    prwMIXSetup(POSNumber) = pgmMIXSetup(POSNumber)
                    prwMASKSetup(POSNumber) = pgmMASKSetup(POSNumber)
                    if prwFullFrameLayerName = pgmSetup(POSNumber) or (prwFullFrameLayerName + POSpostFix = pgmSetup(POSNumber))
                    '    Console.WL("PRV is " & pgmSetup(POSNumber) & " POS" & (POSNumber+1).ToString())
                        if isPTZ then 
                            Input.Find(pgmMIXSetup(POSNumber)).Function("SetMultiViewOverlay", (1).ToString() & "," & fakePTZInputName)
                        end if
                        prwMIXSetup(POSNumber) = prwFullFrameLayerName
                        prwMASKSetup(POSNumber) = FullFramePOSMaskName
                        Input.Find(MEStingerInput).Function("SetMultiViewOverlay", (positionLayers(POSNumber)).ToString() & "," & "")
                        Input.Find(MEStingerInput).Function("SetMultiViewOverlay", (fullFramePOSlayer).ToString() & "," & POSMixesNames(POSNumber))
                        isError = false
                    end if 
                    next
            end if
            end if 
           


        ' ============================================================
        ' ================== MAIN API COMMANDS =======================

            ' scriptTime = System.DateTime.Now - startTime
            ' Console.WriteLine("Before PRE API " + scriptTime.TotalMilliseconds.ToString() + " milliseconds")

            ' ================== PRE API ==============================
                if not isError then
                    for POSNumber as integer = 0 to numberOfPOSITIONS - 1 
                        try
                            ' console.wl("PRVMIX: " & prwMIXSetup(POSNumber).ToString() & " | " & "PGMMIX: " & pgmMIXSetup(POSNumber).ToString())
                            ' console.wl("PRVMSK: " & prwMASKSetup(POSNumber).ToString() & " | " & "PGMMSK: " & pgmMASKSetup(POSNumber).ToString())
                
                            API.Function("ActiveInput",Input:= pgmMIXSetup(POSNumber).ToString(),Mix:=(POSMixesIndexes(POSNumber)).ToString())
                            API.Function("ActiveInput",Input:= pgmMASKSetup(POSNumber).ToString(),Mix:=(MASKMixesIndexes(POSNumber)).ToString())
                            API.Function("PreviewInput",Input:= prwMIXSetup(POSNumber).ToString(),Mix:=(POSMixesIndexes(POSNumber)).ToString())
                            API.Function("PreviewInput",Input:= prwMASKSetup(POSNumber).ToString(),Mix:=(MASKMixesIndexes(POSNumber)).ToString())

                        catch e as Exception
                            isError = true
                            console.WL("ERROR ( line: " & me.CurrentLine.tostring()& "): PRE API. POS " & POSNumber &". error:" & e.toString())
                        end try
                    next 
                end if
        ' ============================================================           
    end if
' ================== MASTER API ==============================
            ' MERGE COMMAND
    if isError then
        API.Function("Fade")
    else 
            API.Function(stringerCommand)
        for POSNumber as integer = 0 to numberOfPOSITIONS - 1 
            API.Function("Merge", Mix:=(POSMixesIndexes(POSNumber)).ToString(), Duration:=mergeDuration.ToString())
            API.Function("Merge", Mix:=(MASKMixesIndexes(POSNumber)).ToString(), Duration:=mergeDuration.ToString())
        next 
    end if

' ============================================================


scriptTime = System.DateTime.Now - startTime
Console.WriteLine("Script done in " + scriptTime.TotalMilliseconds.ToString() + " milliseconds")