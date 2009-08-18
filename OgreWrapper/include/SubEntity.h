#pragma once

namespace Ogre
{
	class SubEntity;
}

namespace OgreWrapper{

ref class MaterialPtr;

[Engine::Attributes::NativeSubsystemTypeAttribute]
public ref class SubEntity
{
private:
	Ogre::SubEntity* subEntity;

internal:
	SubEntity(Ogre::SubEntity* subEntity);

public:
	virtual ~SubEntity(void);

	System::String^ getMaterialName();

	void setMaterialName(System::String^ name);

	void setVisible(bool visible);

	bool isVisible();
	
	MaterialPtr^ getMaterial();

	void setCustomParameter(size_t index, Engine::Quaternion value);

	Engine::Quaternion getCustomParameter(size_t index);
};

}