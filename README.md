Spelldagger is a top-down stealth game I've been developing as a personal project using Godot and C#. This is a snapshot of the project's development repository.

Descriptions of the most interesting technical aspects of the project can be found below, along with links to the most relevant scripts for each one.

### Editor Tools
Spelldagger features a suite of editor tools designed for rapid in-engine development and iteration of levels. These tools allow walls, floors, props, enemies, and interactable objects to be easily placed and configured in an intuitive 2D editor view. This 2D map is then procedurally converted into a 3D level mesh.\
The most interesting scripts involved in this system are:
- [WallEditor.cs](Prefabs/Wall/WallEditor.cs) which handles defining and generating wall meshes. There are equivalent scripts for all other level elements.
- [LevelEditor.cs](Prefabs/Level%20Template/LevelEditor.cs) which handles global level editing operations.
https://github.com/user-attachments/assets/60cdc816-d9fd-4c9e-916e-f652907f3fc3

### Modular Enemy Behavior System
As a stealth game, one of the main obstacles players must overcome in Spelldagger is NPC guards. I wanted an easy way to create guards with a wide variety of different behaviors, senses, and weaknesses. To achieve this, I created a modular enemy bahavior system. All guards share the same core controller script and the same 4 states (idle, invetigating, alerted, and stunned) but their behavior while in each of those states is entirely defined by separate scripts that can be swapped out on a per-guard basis. Additionally, guards can have any number of different senses (sight, sound, mage-sense) and weakpoints that all feed into the central guard controller script and are passed to the individual state behaviors.\
Some example scripts involved in this system are:
- [GuardController.cs](Prefabs/Guard/GuardController.cs) which is the central guard behavior script that interfaces with the modular state behavior scripts.
- [GuardBehaviorPatrol.cs](Prefabs/Guard/State%20Behaviors/GuardBehaviorPatrol.cs) which is an example modular guard state behavior script, in this case for patrolling along a path defined by another editor tool.

### Rewind System
Spelldagger features a rewind (and fast-forward) system to allow players to correct minor mistakes by rewinding the game-state a limited number of times. To achieve this, I developed a centralized temporal controller system that all other aspects of the game can hook into to manage rewinding their state.\
The key scripts involved in this system are:
- [TemporalController.cs](Prefabs/Player/Pocketwatch/TemporalController.cs) which manages the rewind feature.
- [ITemporalControl.cs](Prefabs/Player/Pocketwatch/ITemporalControl.cs) which is an interface implemented by scripts that control object affected by the rewind feature.

### Technical Art
Spelldagger's unique art style took extensive technical art development using a variety of techniques. In particular, many of the most important effects were created by using shaders reacting to otherwise invisible lights and making extensive use of shadows.\
The most important shaders are:
- [PostProcessOutlines.gdshader](Prefabs/Camera/PostProcessOutlines.gdshader) which handles edge outlines, including those around lights.
- [SDStandardShader.gdshader](Resources/Shaders/SDStandardShader.gdshader) which is a modified version of Godot's standard shader that handles objects' appearences in darkness and the floor transition animation.
