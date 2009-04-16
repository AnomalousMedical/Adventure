//Header
#pragma once

namespace Rendering{

ref class Technique;
ref class RenderMaterial;
ref class TechniqueCollection : public WrapperCollection<Technique^>
{
protected:
	virtual Technique^ createWrapper(void* nativeObject, ...array<System::Object^>^ args) override;

public:
	virtual ~TechniqueCollection() {}

	Technique^ getObject(Ogre::Technique* nativeObject, RenderMaterial^ parent);

	void destroyObject(Ogre::Technique* nativeObject);
};

}