# TODO

## Get rid of renamed vertex and index buffers in shaders
now there there is only 1 primary shader instance, we don't need to rename the g_vertices and g_indices buffers.

## Can you do better than a map and array lookup for each sprite each frame
Even sprites that aren't animated will keep updating blas and tlas data with the SpriteBlasLinker. Optimize this so things that are one frame don't update.

Also the sprites are linked to the sprite instances by looking up the frames from a map then an array. This doesn't seem too bad usage wise, but it can probably be better

## Don't allow enemies in connecting corridors
Don't allow enemies to appear in the connecting corridors or make them larger so you can't contact one not in the current zone

## Add occlusion map
Add the occlusion maps to at least see what they look like.

## Add north - south transitions
Make it so levels can go north and south too

## Add variable width and height to levels
Levels are hardcoded to 150, increase this size and allow it to change.

## Smooth out player transition
The player will stop a bit between levels. See if the velocity needs to be preserved or something.

## Level -65271249 puts start location in a bad spot
This level will make the connection in a corridor, since the easternmost room could have an
eastern corridor this will have to be delt with somehow. Same for other directions too.

## Level hole
Random seen 40 or 19 has a hole in the geometry. The collision seems ok though.

## Don't forget mipmaps
Have mipmaps, but need to renormalize normals when creating normal mipmaps.

## Add wrapping support for material textures
Right now the material textures only work if the dest size is smaller than the source size
make it so the source can be wrapped to get pixels that would lay outside it, the groundwork is there

## GetDeviceCaps_GetNDCAttribs is pretty slow
This will do a copy on the native side into a pass struct then into a class on the managed side. Not so great. See if you can do other stuff.
Seems like you could pass a pass struct as ref then fill it out and have it "returned" that way. Not sure about putting that into a class. That still
seems smart to avoid passing around huge c# structs, even with some overhead. Depends on if its per frame or not (and you could pass the struct in as ref
if you had a big one per frame).

## Figure out SRGB
Figure out how to deal with srgb. The colors for the UI have been shifted with a ToSrgb function on Color. This can be found easily enough.
No changes to any shaders were made to deal with it. Need to figure out if we want to keep srgb or change to linear rgb. It looks like the gltf
shaders can do either mode with some defines.

## Make backspace better
Make backspace work better, it needs to work on key down not just repeat and key up

## Filter out characters
currently filtering out anything below 32 in the ascii table, could do this in the control, currently at the higher input level

## Tabs in fonts still need to be figured out
Haven't tried tabs yet in the fonts. They are their own setting so need to mess with it.

## Use new in keyword
Switch ref to in where possible and pass as many structs as possible with in. See if things like Vector3.Forward can be made readonly then.

## Level Generation - boundaryCubeCenterPoints will add extra cubes
If you keep this method of boundary cubes know that L shaped corners will get an extra cube. Should try to prevent this for less per frame physics work.

----------------------------------------------------------------------------------------------------------------------------------------------------------------

# Low Priority
Stuff that is partially solved or maybe doesn't matter.

## Physics character input seems backward
The input for the charcter seems to need a reversed x axis to work. Left / right are +1 and -1 vs -1 and + as would be expected.
Things collide correctly, so it must be right, but its strange.

This is currently fixed in the CharacterMover, since this is part of the physics this is should be enough for now.

## Figure out why camera position is backward
In order to move the camera through the world in the same coords as the rest the position must be inverted before setting the shader. This is
handled in pbr camera and lights.

This is fixed in the pbr renderer when setting position via vector3, quaternion. Probably not a big deal for now. Everything else seems right.

## Figure out why orientation has to be inverted from physics to rendering
This is all handled by the pbr renderer, but it is strange that rotations have to be inverted to render correctly.

Seems to work, but not sure what the deal is