##### Guard Vision:
- Hard edge at end of vision cone
- Display in 3D space or on floor
- Interact with lighting? (fainter, hashed, or outlined in darkness)
Ideas:
- Just a 3D light
- 3D light and next-pass or overlay material that reacts to specific lights

##### Player Vision:
- Obstruct view behind objects
- Black plane over world or just turn everything black
Ideas:
- Vertex shader on walls that extrudes away from the player (can include y-testing)
- Viewport captures bright omni-light on player, anything in shadow is hidden using a plane or post-processing

##### Light Post-Process:
- Outlines outside of light, no outlines inside
- Grayscale outside of light, color inside?
- Light edges controlled with shader
Ideas:
- Next-pass or overlay material that captures specific lights, viewport texture used in post-process

##### Methods:
- Guard vision: 3D light and overlay material
- Player vision: Either viewport or vertex shader
- Light post-process: Viewport, overlay material and post-process shader

##### Universal Overlay Material:
- Capture post-process light marking lights
	- Need fadeout of intensity at the edge (float)
- Capture guard vision
	- Need color (either vec3 or float)

##### Distinguishable Light Characteristics:
- Render layer to choose which lights get captured by which viewports
- Color * energy
- Specular amount?
- Is directional

Use specular to pass data between light functions?
Use directional light to indicate whether this is the guard vision pass or not?
Just make a duplicate of every mesh