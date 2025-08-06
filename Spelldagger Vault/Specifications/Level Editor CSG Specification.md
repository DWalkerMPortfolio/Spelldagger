- Separate CSG tree for each object
	- Would need to register all intersections
- One CSG tree per floor
	- No, can't split into sections
- One CSG tree for entire level
	- No, can't split into sections
- One CSG tree per section of each floor of level
	- Maybe? Could be more optimized if I can figure out how to do it

- One CSG tree for all subtractive geometry in the level
	- Subtractive objects get added and removed as their objects are created and destroyed
	- Additive objects work the same
		- Saved back to mesh instance when scene is saved
	- CSG tree gets destroyed when game starts before it can run any calculations

- Problem: get_meshes() only works on root node of CSG tree

- Either:
	- One CSG tree of all subtractive geometry, add one additive mesh at a time only when edited
		- Problem: Would need to edit walls after moving doors for them to update (unless update triggered manually)
	- Each wall and floor has a CSG tree that all intersecting subtractive geometry is added to

- Solution:
	- Each wall has a CSG tree
	- Doors snapped to the wall add subtractive CSG geometry to the tree
	- Wall is in the CSG tree at all times in editor
		- Mesh is copied over when scene saves
		- Mesh is hidden in editor
		- CSG tree is destroyed in game
	- Floors are handled separately either as CSG or using clip_polygon_2D()

- Walls need CSG per segment