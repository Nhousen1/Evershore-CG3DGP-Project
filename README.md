# **Evershore**

## **Team Members**

Liam Housenbold, Marcus King, Samuel Huang 

## **Game Summary**

Our project is a disguised survival horror game with a top-down isometric perspective. You find yourself on an island which seems like paradise, only to encounter horrors beyond your comprehension.  

## **Genres**

Horror, Low Poly, Isometric Top Down, Action, Narrative Heavy, 3D world 

## **Inspiration**

### **Hades**

<img src="GDD_Images/Hades_example.png" alt="Hades example" width="400">

Hades is an isometric top down game that’s heavily based on combat. While we won’t be taking combat from it directly, we will take its good environment and atmosphere for the horror section.

### **Bastion**

<img src="GDD_Images/Bastion_example.png" alt="Bastion example" width="400">

Similar to Hades, a simple but fun combat system from the top-down isometric perspective. Offers good inspiration for weapon mechanics. Heavy on story and multiple endings.

### **Eden** 

<img src="GDD_Images/Eden_example.png" alt="Eden example" width="400">

A top down mobile game involving building camps. We will take inspiration from its low poly aesthetic and “cozy” like vibes. 

## **Gameplay**

* Top down isometric player controller along with a camera system. Movement includes walk, sprint, roll, jump system.  
* Basic stealth system to sneak up on enemies when hunting during the day and to hide/avoid enemies during the night  
  * Sightline  
    * Can hide in bushes or behind rocks/trees  
    * Walking through water leaves a fading footprint trail  
  * Scent (nice to have feature)  
    * Longer in one place stronger radius of sphere  the smell  
    * Walking through river resets smell  
    * Wind direction indicated on Hud? Feature to be implemented if given time.  
  * Sound  
    * Stepping on branches makes noise  
    * Running/rolling is louder than walking  
    * Walking in water is louder  
    * entering/exiting a bush makes noise  
* Minimal verticality no more than height of the camera from the ground.  
* Hp systems “3 hearts” maybe item to gain another one   
  * Checkpoint rest to regain hp system no consumables/passive regen  
* Weapons system including up to 3 weapons (2 projectiles, 1 melee).   
* 3 basic enemies with navigational, detection and attack systems.   
  * Enemies are weak (taking 1-3 hits).  
  * Each enemy has their own attack sequence: a specific series of gameplay events in which you need to time your attacks so you can defeat/dodge the enemy. This sequence boils down to a decision tree.   
  * Enemy navigation is based on live environment information and enemy objective, possibly using Goal Oriented Action Programming.  
* A simple decision tree which influences ending sequence (3 endings, bad, default)  
* Linear level system with 3 combat encounters (stealth and fighting) end result of encounter influences ending tree.

## **Development Plan**  
   
### **Project Checkpoint 1-2: Basic Mechanics and Scripting (Ch 5-9)**  
\-~~Build isometric perspective~~, ~~player controller~~, ~~combat system~~, stealth, ~~movement~~, ~~health~~, ~~attacks~~  
*“We were not able to complete the stealth system due to  time constraints. The combat system took longer to implement than expected.”*  
~~\-Implement enemy navigation objective system and attack sequences. Set up sight and sound detection.~~  
*“The enemy works on a range detection, not a sound detection system”*  
~~\-Finish design sketch of all areas, characters, and items of the game.~~  
\-Implement individual scene decisions and lore of the endings.  
*“We did not get to the ending portion. We invested our time in gameplay resources. We needed to make a gameplay loop first so the ending could be reached we now realize after getting started that adding the ending text scene will be the last thing we do after all the other gameplay and impacting decisions are implemented”*

*Additions:*  
*~~We were able to make a scene transition script to move between scenes~~*  
*~~We were also able to make a GameManager Script to transition to the loss screen on player death and the win screen on puzzle completion~~*

### **Project Checkpoint 2:**  
#### **Player Prefab Updates (Marcus)**  
~~\-Clean up and document player code~~   
~~\-Add updates from last submission to README~~   
~~\-Add rigidbody to player controller~~   
~~\-Document how to use the player controller and attack system in README~~   
~~\-Configure to work with scene transition using player tag~~   
~~\-Win loss conditions~~   
~~\-Find or make a static player model and cite~~ 

Nice to have Additions:  
\-update oar Collison and add flare gun(requires charge up mechanic to be added)  
\-Add basic animator to player  
“Both of these are partially implemented and in progress for the next checkpoint”

#### **Enemy Updates (Liam)**  
~~\-Clean up and document enemy code~~  
~~\-add updates from last submission to README~~  
~~\-add 3rd attack to enemy (diagonal swing)~~  
~~\-add model/texture to enemy prefab~~  
~~\-add model animations to enemy prefab?~~  
~~\-Document how the enemies work in README~~  
~~\-add collider bounds gizmo to enemy/weapon hitboxes~~  
~~Add enemy island with island terrain texture~~  
~~Create water shader for water effect for scenes~~

Nice to have Additions:  
\-Ranged enemy version  
\-Enemy visual HP bar  
“Didnt get to these extra steps but am planning on adding the HP bars this weekend and I made a lot of bonus progress on the enemy animations”

#### **Map/town updates: (Samuel)**  
~~\-Comment and add documentation to previous code~~  
~~\-Populate town with assets~~  
~~\-add boat assets and fix scene transition~~  
~~\-add puzzle island and town island~~   
~~\-Update puzzle with windmill assets~~  
~~\-Add villager models~~  
~~\-Add textbox dialogue system with villagers on interact~~

### **Preliminary Plan for Project Part 3 (Chapters 11, 12, and 13):**  
\-Add scene Illumination (lighting and shadows)  
\-Finalize level design   
\-Add ranged enemies  
\-Finish Player weapon Animations  
\-Update Player movement Animations  
\-Add basic ending scene  
\-Object fader script  
\-Add blood particle effect on weapon hit  
\-Add particle effect to ranged projectile  
\-Add second Puzzle  
\-Standardize scene hierarchy organization  
\-Add player  
\-enemy HP bars

Possible nice to have:  
\-Add dust trail effect on sprint  
\-Add respawn/replay button to win and lose screens

## **Instructions for Testing the Project:**  
There are 3 main scenes in a ProjectSubmission2 folder in the Scenes folder:  
IslandHub scene, EnemyIsland scene, and Forrest scene (the puzzle).

Player:

- WASD controls. Point player in the direction of your cursor. Q previous weapon, E next weapon. Left click to attack. Space to jump, shift to sprint. (weapon system is currently broken when adding animations to it)  
- 

\-The Hub map has a marketplace, houses, and three NPCs. I have implemented a simple dialogue system where you can talk to the NPC by pressing 'E', and the dialogue UI will show up. 

\-Enemies have a few states(can be found in detail in the EnemyFSM script)   
\-it has an out of combat patrol state that enters combat upon the player entering its view angle unobstructed (shown by the red gizmo)   
\-It has a charge attack that speeds up the enemy in the player direction when the enemy is in combat and far away (range shown by the yellow gizmo)  
	\-It has chase player state that follows the player  
	\-It has swing attack that swipes the skeleton sword at the player (swipe attack range shown by the green gizmo) (weapon hitbox is also outlined by a gizmo)  
	\-It has a defensive stance state where the enemy will glow red and get increased armor(take reduced damage for a few seconds) \[future TODO: add reflective thorns damage if the player hits the enemy while they are in this stance\]

**Assets  used:** https://assetstore.unity.com/packages/3d/characters/humanoids/low-poly-medieval-characters-lite-316247  
https://assetstore.unity.com/packages/3d/environments/wooden-house-free-low-poly-270889  
https://assetstore.unity.com/packages/vfx/particles/fire-explosions/low-poly-fire-244190  
https://assetstore.unity.com/packages/3d/environments/low-poly-tropical-island-lite-242437  
https://assetstore.unity.com/packages/3d/environments/landscapes/low-poly-simple-nature-pack-162153  
https://assetstore.unity.com/packages/3d/environments/simplepoly-city-low-poly-assets-58899  
[https://assetstore.unity.com/packages/3d/environments/landscapes/low-poly-atmospheric-locations-pack-278928](https://assetstore.unity.com/packages/3d/environments/landscapes/low-poly-atmospheric-locations-pack-278928)  
[https://assetstore.unity.com/packages/3d/animations/skeleton-animations-free-217504\#description](https://assetstore.unity.com/packages/3d/animations/skeleton-animations-free-217504#description)  
[https://assetstore.unity.com/packages/3d/characters/humanoids/humans/human-character-dummy-178395](https://assetstore.unity.com/packages/3d/characters/humanoids/humans/human-character-dummy-178395), [https://assetstore.unity.com/packages/3d/animations/human-melee-animations-free-165785](https://assetstore.unity.com/packages/3d/animations/human-melee-animations-free-165785), [https://gamedevbeginner.com/singletons-in-unity-the-right-way/](https://gamedevbeginner.com/singletons-in-unity-the-right-way/), [https://github.com/roboryantron/Unite2017](https://github.com/roboryantron/Unite2017)  
All material/texture/noise pngs from are from https://www.freepik.com  

