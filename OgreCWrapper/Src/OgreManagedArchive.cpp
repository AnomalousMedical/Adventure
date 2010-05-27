#include "StdAfx.h"
#include "..\Include\OgreManagedArchive.h"

OgreManagedArchive::OgreManagedArchive(String name, String archType, LoadDelegate loadCallback, UnloadDelegate unloadCallback, OpenDelegate openCallback, ListDelegate listCallback, ListFileInfoDelegate listFileInfoCallback, FindDelegate findCallback, FindFileInfoDelegate findFileInfoCallback, ExistsDelegate existsCallback, DeleteDelegate deletedCallback)
:Ogre::Archive(name, archType),
loadCallback(loadCallback),
unloadCallback(unloadCallback),
openCallback(openCallback),
listCallback(listCallback),
listFileInfoCallback(listFileInfoCallback),
findCallback(findCallback),
findFileInfoCallback(findFileInfoCallback),
existsCallback(existsCallback),
deletedCallback(deletedCallback)
{
}

OgreManagedArchive::~OgreManagedArchive(void)
{
	deletedCallback();
}

void OgreManagedArchive::load()
{
	loadCallback();
}

void OgreManagedArchive::unload()
{
	unloadCallback();
}

Ogre::DataStreamPtr OgreManagedArchive::open(const Ogre::String& filename, bool readOnly) const
{
	return Ogre::DataStreamPtr(openCallback(filename.c_str(), readOnly));
}

Ogre::StringVectorPtr OgreManagedArchive::list(bool recursive, bool dirs)
{
	return Ogre::StringVectorPtr(listCallback(recursive, dirs), Ogre::SPFM_DELETE_T);
}

Ogre::FileInfoListPtr OgreManagedArchive::listFileInfo(bool recursive, bool dirs)
{
	return Ogre::FileInfoListPtr(listFileInfoCallback(recursive, dirs), Ogre::SPFM_DELETE_T);
}

Ogre::StringVectorPtr OgreManagedArchive::find(const Ogre::String& pattern, bool recursive, bool dirs)
{
	return Ogre::StringVectorPtr(findCallback(pattern.c_str(), recursive, dirs), Ogre::SPFM_DELETE_T);
}

Ogre::FileInfoListPtr OgreManagedArchive::findFileInfo(const Ogre::String& pattern, bool recursive, bool dirs)
{
	return Ogre::FileInfoListPtr(findFileInfoCallback(pattern.c_str(), recursive, dirs), Ogre::SPFM_DELETE_T);
}

bool OgreManagedArchive::exists(const Ogre::String& filename)
{
	return existsCallback(filename.c_str());
}

//PInvoke functions
extern "C" __declspec(dllexport) OgreManagedArchive* OgreManagedArchive_Create(String name, String archType, LoadDelegate loadCallback, UnloadDelegate unloadCallback, OpenDelegate openCallback, ListDelegate listCallback, ListFileInfoDelegate listFileInfoCallback, FindDelegate findCallback, FindFileInfoDelegate findFileInfoCallback, ExistsDelegate existsCallback, DeleteDelegate deletedCallback)
{
	return OGRE_NEW OgreManagedArchive(name, archType, loadCallback, unloadCallback, openCallback, listCallback, listFileInfoCallback, findCallback, findFileInfoCallback, existsCallback, deletedCallback);
}

extern "C" __declspec(dllexport) void OgreManagedArchive_Delete(OgreManagedArchive* archive)
{
	OGRE_DELETE archive;
}

extern "C" __declspec(dllexport) Ogre::StringVector* OgreManagedArchive_createOgreStringVector()
{
	return OGRE_NEW_T(Ogre::StringVector, Ogre::MEMCATEGORY_GENERAL)();
}

extern "C" __declspec(dllexport) void OgreStringVector_push_back(Ogre::StringVector* stringVector, String value)
{
	stringVector->push_back(value);
}

extern "C" __declspec(dllexport) Ogre::FileInfoList* OgreManagedArchive_createOgreFileInfoList()
{
	return OGRE_NEW_T(Ogre::FileInfoList, Ogre::MEMCATEGORY_GENERAL)();
}

extern "C" __declspec(dllexport) void OgreFileInfoList_push_back(Ogre::FileInfoList* fileList, OgreManagedArchive* archive, size_t compressedSize, size_t uncompressedSize, String baseName, String filename, String path)
{
	Ogre::FileInfo info;
	info.archive = archive;
	info.compressedSize = compressedSize;
	info.uncompressedSize = uncompressedSize;
	info.basename = baseName;
	info.filename = filename;
	info.path = path;
	fileList->push_back(info);
}