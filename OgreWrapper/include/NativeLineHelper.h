#pragma once

#pragma unmanaged

#include <string>

#include "OgreResource.h"

namespace Ogre
{
	class ManualObject;
	class SceneManager;
	class SceneNode;
	class Resource;
}

namespace OgreWrapper{

public class __declspec(dllexport) NativeLineHelper
{
private:
	Ogre::ManualObject* lineBuffer;
	std::string material;
	float r;
	float g;
	float b;
	float a;

public:
	NativeLineHelper();

	~NativeLineHelper(void);

	virtual void drawLine(float* p1, float* p2);

	void setColor(float r, float g, float b, float a);

	void clear();

	void begin();

	void end();

	void attachToNode(Ogre::SceneNode* node);

	void detachFromNode(Ogre::SceneNode* node);

	void setBuffer(Ogre::ManualObject* manualObject);

	void clearBuffer();

	void setVisible(bool visible);

	void setMaterial(std::string material);

	/// <summary>
	/// Sets the render queue group this entity will be rendered through. 
	/// </summary>
	/// <param name="queueID">The queue id to add this object to.</param>
	void setRenderQueueGroup(unsigned char queueID);

	/// <summary>
	/// Gets the queue group for this entity.
	/// </summary>
	/// <returns>The render queue group of this object.</returns>
	unsigned char getRenderQueueGroup();
};

}

#pragma managed