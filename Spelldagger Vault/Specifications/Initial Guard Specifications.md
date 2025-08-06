### Behavior
##### Always:
- If spot player, begin raising awareness
	- If player collides with guard, instantly set awareness to full
	- When awareness reaches full, shout and enter chasing state
- If hearing noise, enter investigating state
	+ Investigation urgency depends on noise? (guard death = high urgency, dagger noise = low urgency)
- If guard spots incapacitated guard, enter high alert and searching state

##### Patrol:
- Follow path, rotate smoothly at turns
- If high alert, look around at path points

##### Investigating:
- Move towards source of investigation, speed depends on urgency
+ If high urgency, enter high alert

##### Chasing:
- Move towards target
- Enter high alert
- If lose sight of target, move to last seen location
	+ Enter searching state

##### Searching:
+ Search area somehow?

### Potential Guard Types
Camera: Just makes a loud noise when seeing the player
Ranged attacker: When chasing, get good sightline on player and charge up attack
Magic sensing: Sense use of runes
Pinging: Attach something to the player that pings out noise for a bit

### Potential Additions:
- Alarms guards can trigger
- Lock down areas when player is spotted
- Guards can spot players in darkness but awareness just builds up slower
- Where do guards keep shields?

### Potential Puzzles:
Single guard patroling corridor:
- Incapacitate from behind
- Duck into an adjacent room
- Distract or teleport

Locked room with guard patroling in and out:
- Stick dagger to shield and teleport in

Large room with several guards patroling:
- Distract them with dagger
