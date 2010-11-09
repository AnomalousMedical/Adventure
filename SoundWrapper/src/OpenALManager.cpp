#include "StdAfx.h"
#include "OpenALManager.h"
#include "SourcePool.h"
#include "MemorySound.h"
#include "StreamingSound.h"

//Codecs
#include "OggCodec.h"

namespace SoundWrapper
{

OpenALManager::OpenALManager(void)
:ready(true)
{
	device = alcOpenDevice(NULL);
	if (device == NULL)
	{
		ready = false;
		return;
	}

	//Create context(s)
	context = alcCreateContext(device,NULL);

	//Set active context
	alcMakeContextCurrent(context);

	// Clear Error Code
	alGetError();

	sourcePool = new SourcePool();
}

OpenALManager::~OpenALManager(void)
{
	delete sourcePool;

	//Disable context
	alcMakeContextCurrent(NULL);
	//Release context(s)
	alcDestroyContext(context);
	//Close device
	alcCloseDevice(device);
}

Sound* OpenALManager::createMemorySound(Stream* stream)
{
	return new MemorySound(getCodecForStream(stream));
}

Sound* OpenALManager::createStreamingSound(Stream* stream)
{
	return new StreamingSound(getCodecForStream(stream));
}

Sound* OpenALManager::createStreamingSound(Stream* stream, int bufferSize, int numBuffers)
{
	return new StreamingSound(getCodecForStream(stream), bufferSize, numBuffers);
}

void OpenALManager::destroySound(Sound* sound)
{
	delete sound;
}

AudioCodec* OpenALManager::getCodecForStream(Stream* stream)
{
	return new OggCodec(stream);
}

}


//CWrapper

using namespace SoundWrapper;

extern "C" _AnomalousExport OpenALManager* OpenALManager_create()
{
	return new OpenALManager();
}

extern "C" _AnomalousExport void OpenALManager_destroy(OpenALManager* openALManager)
{
	delete openALManager;
}

extern "C" _AnomalousExport Sound* OpenALManager_createMemorySound(OpenALManager* openALManager, Stream* stream)
{
	return openALManager->createMemorySound(stream);
}

extern "C" _AnomalousExport Sound* OpenALManager_createStreamingSound(OpenALManager* openALManager, Stream* stream)
{
	return openALManager->createStreamingSound(stream);
}

extern "C" _AnomalousExport Sound* OpenALManager_createStreamingSound2(OpenALManager* openALManager, Stream* stream, int bufferSize, int numBuffers)
{
	return openALManager->createStreamingSound(stream, bufferSize, numBuffers);
}

extern "C" _AnomalousExport void OpenALManager_destroySound(OpenALManager* openALManager, Sound* sound)
{
	openALManager->destroySound(sound);
}