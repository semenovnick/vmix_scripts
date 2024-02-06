dim config as new system.xml.xmldocument
config.loadxml(API.XML)
dim inputsNodeList as XMLNodeList = config.SelectSingleNode("/vmix/inputs").SelectNodes("input")
API.Function("PreviewInput", Input:=inputsNodeList.count)