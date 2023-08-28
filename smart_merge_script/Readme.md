# Smart Merge script

The script makes a smooth (no overlaping and correct layer ordering) transition between supersources (ME lines) when using _MERGE_ transition.

## Basic principle

Script dynamically generates a _STINGER_ transition for each Preview/Program combination, emulating the effect of a _MERGE_ transition.
The source for _STINGER_ is a **COLOR** input with a transparent background, in which all (a predetermined number) sources for supersource are collected. Each source is represented by a separate **MIX** type input for which a mask is applied, which, in turn, is also a **MIX** type input. And in fact, there is a _MERGE_ for each source separately, with the subsequent merging of these images into one _STINGER_. For each Preview / Program combination, its own Preview / Program is formed inside each _MIX_ and at the time of the transition, _MERGE_ occurs in each **MIX**, and the transition in the main program is performed as a _STINGER_ in the image of which all _MERGE_ sources are displayed.
The main advantages of this script:

- when it is implemented in the project, no constructive changes are made to the initial structure and logic of the project
- all tally are saved and there are INPUTs in the program, consisting of real sources,
  all additional shots and INPUTs used by the script participate ONLY at the moment of the transition and do not add any additional significant load on the CPU.
- all "heavy" manipulations with the API occur only if all conditions for script execution are met. Those. first, a complete analysis of the current situation takes place, and only after all the manipulations with the data, the resulting decision on reassignments and other interactions with the API is formed.

## Work principle

1. Making Program and Preview for each source in separate MIX inputs for video and it's mask
2. Simultaneous launch of _MERGE_ within all **MIXes**
3. In case of a successful _MERGE_, _STINGER_ is used in the main program; in case of an error or failure to execute _MEGRE_, _FADE_ is used.

The script has the ability to fully customize:

- configurable transition duration from the vMIX GUI
- configurable number of sources (limited only by the number of already used MIX inputs in the project (up to 16 in total in the latest version))
- customizable number of supersources

Full instuctions you can find in ./docs folder.
