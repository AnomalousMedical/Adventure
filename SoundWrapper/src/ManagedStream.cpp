#include "StdAfx.h"
#include "Stream.h"

namespace SoundWrapper
{

typedef size_t (*ReadDelegate)(void* buffer, int size, int count);
typedef int (*SeekDelegate)(long offset, SeekMode origin);
typedef void (*CloseDelegate)();
typedef size_t (*TellDelegate)();
typedef bool (*EofDelegate)();
typedef void (*DeleteDelegate)();

class ManagedStream : public Stream
{
private:
	ReadDelegate readCB;
	SeekDelegate seekCB;
	CloseDelegate closeCB;
	TellDelegate tellCB;
	EofDelegate eofCB;
	DeleteDelegate deleteCB;

public:
	ManagedStream(ReadDelegate readCB, SeekDelegate seekCB, CloseDelegate closeCB, TellDelegate tellCB, EofDelegate eofCB, DeleteDelegate deleteCB)
		:readCB(readCB), seekCB(seekCB), closeCB(closeCB), tellCB(tellCB), eofCB(eofCB), deleteCB(deleteCB)
	{
	}


	virtual ~ManagedStream(void)
	{
		deleteCB();
	}

	virtual size_t read(void* buffer, int size, int count)
	{
		return readCB(buffer, size, count);
	}

	virtual int seek(long offset, SeekMode origin)
	{
		return seekCB(offset, origin);
	}

	virtual void close()
	{
		closeCB();
	}

	virtual size_t tell()
	{
		return tellCB();
	}

	virtual bool eof()
	{
		return eofCB();
	}
};

}

//CWrapper

using namespace SoundWrapper;

extern "C" _AnomalousExport ManagedStream* ManagedStream_create(ReadDelegate readCB, SeekDelegate seekCB, CloseDelegate closeCB, TellDelegate tellCB, EofDelegate eofCB, DeleteDelegate deleteCB)
{
	return new ManagedStream(readCB, seekCB, closeCB, tellCB, eofCB, deleteCB);
}

extern "C" _AnomalousExport void ManagedStream_destroy(ManagedStream* managedStream)
{
	delete managedStream;
}