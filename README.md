# BeatSaber_Oblong
A custom Input System for Beat Saber which allows you to use Controllers of a
[Mezzaine Tracking / Conferencing System](https://www.oblong.com/mezzanine/tech-specs) to play Beat Saber :D

Commissioned by the [RIT/ESL Global Cybersecurity Institute/Cyber Range](https://www.rit.edu/cybersecurity/cyber-range)

As it stands, this is a very barebones plugin and is tested to work with their specific installation
(Of which as far as I know, are not many others). It is lacking some more advanced logic like
correcting the coordinate system to face in the right / playing direction, as it stands this was done
in a hardcoded fashion (In this case, by inverting the Z position in the OblongPlatformHelper)

### Requirements / dependencies (Can be found in Modassistant)

- Websocket-sharp
- BeatSabermarkupLanguage

### Install

Click on [releases](https://github.com/kinsi55/BeatSaber_Oblong/releases/latest), download the dll from the latest release and place it in your plugins folder.