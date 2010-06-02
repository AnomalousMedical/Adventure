#include "Stdafx.h"

extern "C" __declspec(dllexport) Ogre::KeyFrame* NodeAnimationTrack_getKeyFrame(Ogre::NodeAnimationTrack* animTrack, ushort index)
{
	return animTrack->getKeyFrame(index);
}

extern "C" __declspec(dllexport) float NodeAnimationTrack_getKeyFramesAtTime1(Ogre::NodeAnimationTrack* animTrack, float timePos, uint keyIndex, Ogre::KeyFrame* keyFrame1, Ogre::KeyFrame* keyFrame2)
{
	Ogre::TimeIndex timeIndex(timePos, keyIndex);
	return animTrack->getKeyFramesAtTime(timeIndex, &keyFrame1, &keyFrame2);
}

extern "C" __declspec(dllexport) float NodeAnimationTrack_getKeyFramesAtTime2(Ogre::NodeAnimationTrack* animTrack, float timePos, uint keyIndex, Ogre::KeyFrame* keyFrame1, Ogre::KeyFrame* keyFrame2, ushort* firstKeyIndex)
{
	Ogre::TimeIndex timeIndex(timePos, keyIndex);
	return animTrack->getKeyFramesAtTime(timeIndex, &keyFrame1, &keyFrame2, firstKeyIndex);
}

extern "C" __declspec(dllexport) Ogre::KeyFrame* NodeAnimationTrack_createKeyFrame(Ogre::NodeAnimationTrack* animTrack, float timePos)
{
	return animTrack->createKeyFrame(timePos);
}

extern "C" __declspec(dllexport) void NodeAnimationTrack_removeKeyFrame(Ogre::NodeAnimationTrack* animTrack, ushort index)
{
	animTrack->removeKeyFrame(index);
}

extern "C" __declspec(dllexport) void NodeAnimationTrack_removeAllKeyFrames(Ogre::NodeAnimationTrack* animTrack)
{
	animTrack->removeAllKeyFrames();
}