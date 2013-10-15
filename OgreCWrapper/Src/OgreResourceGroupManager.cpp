#include "Stdafx.h"

extern "C" _AnomalousExport void ResourceGroupManager_createResourceGroup(String name)
{
	Ogre::ResourceGroupManager::getSingleton().createResourceGroup(name);
}

extern "C" _AnomalousExport bool ResourceGroupManager_initializeAllResourceGroups()
{
	try
	{
		Ogre::ResourceGroupManager::getSingleton().initialiseAllResourceGroups();
	}
	catch(Ogre::Exception e)
	{
		return false;
	}
	return true;
}

extern "C" _AnomalousExport bool ResourceGroupManager_initializeResourceGroup(String name)
{
	try
	{
		Ogre::ResourceGroupManager::getSingleton().initialiseResourceGroup(name);
	}
	catch(Ogre::Exception e)
	{
		return false;
	}
	return true;
}

extern "C" _AnomalousExport void ResourceGroupManager_destroyResourceGroup(String name)
{
	Ogre::ResourceGroupManager::getSingleton().destroyResourceGroup(name);
}

extern "C" _AnomalousExport void ResourceGroupManager_addResourceLocation(String name, String locType, String group, bool recursive)
{
	try
	{
		Ogre::ResourceGroupManager::getSingleton().addResourceLocation(name, locType, group, recursive);
	}
	catch(Ogre::Exception e)
	{
	}
}

extern "C" _AnomalousExport void ResourceGroupManager_removeResourceLocation(String name, String resGroup)
{
	Ogre::ResourceGroupManager::getSingleton().removeResourceLocation(name, resGroup);
}

extern "C" _AnomalousExport String ResourceGroupManager_findGroupContainingResource(String resourceName)
{
	try
	{
		return Ogre::ResourceGroupManager::getSingleton().findGroupContainingResource(resourceName).c_str();
	}
	catch (Ogre::Exception& e)
	{
		return 0;	
	}
}

extern "C" _AnomalousExport bool ResourceGroupManager_resourceGroupExists(String name)
{
	return Ogre::ResourceGroupManager::getSingleton().resourceGroupExists(name);
}