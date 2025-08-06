Design:
- Distinct types of guard with unique visuals and behaviors
- Approximately 5-10 types throughout the game
- Need to be easy to prototype

Things that can vary between guards (but may be shared by some):
- Vision cones
- Senses
- Idle behavior
- Investigation behavior
- Player spotted behavior
- Attack method
- Mobility (immobile, walking, flying)
- Can/can't be distracted
- Alertness
- Can/can't be stunned (and method)
- Visuals (model, animation, VFX)

Things that are consistent between all guards:
- React to player and/or daggers
- Idle/investigating/aware/stunned states (may not all be used)

Base guard controller/brain script:
- Receives perception information from vision cones and other senses
	- Has exposed references
	- Tracks perception but doesn't act on it, just exposes it to the state logic scripts?
- Has a state machine reference for handling main states
	- Initializes states
	- Each state handles transitioning to the others?
- Has references to resources (or maybe nodes) for handling state behaviors
	- Resource can reference other nodes in scene

Each guard:
- Inherited scene
	- Contains path editor?, navigation agent?, guard controller, guard body?
	- Some of these aren't needed for some types of guard