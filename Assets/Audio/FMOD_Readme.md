# <p align="center"><img src="https://github.com/swin-sep/2019GP-Under-Ctrl/blob/master/images/Star%20Jump%20Logo.png"></p>
**Developed by Under Ctrl**
**FMOD**

## Readme
This goes over the basic implementation and notes for how FMOD is to be integrated for UNITY. This will go over any basic errors that can occur and how to fix them.
Currently, no implementation is set, as the FMOD project needs to be ready to run first before we begin integrating fully, and the additional steps in Unity when done will be listed below.

## Download Requirements
- Unity 2018.3.2
- FMOD Studio (See Sources for download)
- FMOD Studio Unity Integration from (See Sources for download)
You will need to FMOD account to download, free to sign-up and download.

## Steps Taken
- In Unity, select [Assets -> Import Packge -> Custom Package...]
- Select the Package you've downloaded
- Ensure that all assets of the package are selected by clicking "All", and click "Import".
- If an error appears in the console saying: "FMOD Studio: FMOD Studio Project path not set", the import was successful.
- Open FMOD Studio, save a new project in the desired directory within the Unity project.
- Once done, start by building the FMOD Project in [File -> Build], or press F7.
- Head over to Unity, a new tab called FMOD should appear, select [FMOD -> Edit Settings]
- FMODStudioSettings should appear in the inspector, select Studio Project Path, and select the FMOD Studio Project in your Unity Project.

## FMOD Studio Steps for new Audio Events
- Import Audio needed for your event.
- Do the work needed.
- When done, assign the audio event to a bank, this allows Unity to find it.
- Build the project (F7) and save.

## Steps For when FMOD is ready to integrate
- When ready to use FMOD Studio Project for Unity, select [Edit -> Project Settings...]
- Select Audio, and click the tickbox for "Disable Unity Audio"
- Replace all Unity Audio components with FMOD equivalents.
- In principal, you have a "listener" and "emitters" of sounds.

## Sources
- Download for FMOD Studio and Unity Integration - https://www.fmod.com/download
- How To Integrate FMOD Studio With Unity: Scott Game Sounds - https://www.youtube.com/watch?v=IJRIowt_6PE
- Adding Game Sounds | Part 1 - OneShots and Animation: https://www.youtube.com/watch?v=E7KSGSHrjTI
- More to come as they're found.

## Final Note
Hope this is helpful and allows you to learn the skills needed to make contributions here. Feel free to ask additional enquiries.
Matthew Germon