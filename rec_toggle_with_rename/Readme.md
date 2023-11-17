# Recording toggle with rename script

The script toggling rec button and renaming last recorded file based on contents of text field in specified GT title

## About script

Script gets info about new file names and new path from ttl/info_ttl.gtzip or any your personal GT title, which contains this fields

- new name for recorder 1 (you can use only name without extention, script adds proper extention)
- new path for recorder 1 (if path contains file in the end, script gets only path ignoring filename, name should be in name field) Path should end with "\\"
- new name for recorder 2
- new path for recorder 2
- script stdout interactive field

When you start the script if recorder stopped - it will start recorder with preinstalled parameters (default). No other actions.
If recorder is running - script check name and path. On error message apears in script stdout interactive field. Record will not stop. You can make corrections. If everything ok. Record stops and script will try to move file with settings you specify. If file exists - script will add postfix.
