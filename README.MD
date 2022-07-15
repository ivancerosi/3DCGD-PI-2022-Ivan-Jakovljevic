# Task 1:
## Specific mechanic
Wall run (used on Level 3). Jump near a wall, look parallel to the wall and hold space bar.
## User interface
Main menu - start or exit
Ingame menu - resune or return to main menu		

# Task 2:
## Sound effects
Player has sound effects for running, shooting, juming and reloading
NPCs have sound effects for moving, attacking and dying
## Lights, GI, materials, animated lights
### Lights, animated lights
FlashyLight and AlarmLight prefabs have been created which use spotlight and emissive material.
FlashyLight has a script which periodically turns the light off while AlarmLight has a script which
rotates the light.
### GI
Last level contains emissive materials (molten material) which are set to static and global illumination has been
baked to produce reflected light effect
### Materials
There are some custom textured materials such as concrete, metal box and ventilation. There are also three emissive materials.
### Post-processing
ViewModel gameobject contains a child Post-Processing Volume. The ViewModel is intended to hold users settings and between-scene information and is made
to persist between scene changes.

# Task 3:
## 3 different models of NPCs
Zombie, wolf and crab monster
## Each should have at least 5 animation states
Zombie: idle, patrol, run, attack, die
Wolf: idle, run, attack, die, leap
Crab monser: sleep, run, attack, take damage, die
## Implement different behaviours and animations of NPCs by using state machines
Each NPC has sleep, attack and die states. Zombie has additional patrol state, wolf has leap state. Crab doesn't
have unique AI state but its special mechanic is that it can sleep on wall and jump down when player gets close or
damages it.
